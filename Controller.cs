using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DubinsPathsTutorial;
using UtilsWithoutDirection;
using ImprovedAPF;

using System;

public class Controller : MonoBehaviour
{
    public bool work = false;
    public SettingControl setting;

    public float speed = 238.0f;

    [Range(-45.0f, 45.0f)]
    public float right = 0.0f;

    [Range(-45.0f, 45.0f)]
    public float up = 0.0f;

    public float upChangeSpped = 0.1f;
    public float rightChangeSpped = 0.1f;

    public float stand_Speed = 238.0f;
    public float stand_MaxRotate = 38.6f;
    public float stand_MaxCenterG = 0.8f;
    public float stand_HalfLength = 7225.0f;

    public float stand_OneRoundTime = 0.0f;
    public float stand_PerSecondRotate = 0.0f;
    public float stand_RotateCoefficient = 0.0f;

    public float right_coefficient = (float)((360 / ((2 * Mathf.PI * 7225) / 0.8)) / 38.6);

    public Animator animator;

    public Text text_x;
    public Text text_x_stand;
    public Text text_z;
    public Text text_z_stand;
    public Text text_h;
    public Text text_h_stand;
    public Text text_v;

    private float x_value;
    private float z_value;
    private float h_value;

    public Text text_r;
    public Image imagePoint;

    public bool limitFPS = false;

    public bool startSimulator = false;

    public bool navigate = false;
    public Vector3[] navigateRoute;
    public int nav_pointer = 1;
    public int frameCount = 0;
    public ShipWork enmyTarget;

    public GameObject model_RF;
    public bool work_RF = true;

    public LineRenderer path_Gone;

    public GameObject pathGiver;
    public GameObject pathTrace;

    public List<FindShip> findShips = new List<FindShip>();

    public float moveLength = 0.0f;
    public Text text_length;

    public bool execute_avoid_tatic = false;

    public bool turnStayRF = true;

    public Toggle set_turnStayRF;

    public bool complicated_static = true;

    public Toggle set_hardAvoid;

    public bool Hit_CV_direct_path = false;

    public Toggle set_directHit;

    public bool avoid_static = true;

    public Toggle set_AvoidStatic;

    public bool guess_formation = true;

    public Toggle set_GuessFormation;

    public FormationPredictor predictor = new FormationPredictor();

    public bool predicted_CV = false;

    public System.Numerics.Vector2 predicted_CV_pos = new System.Numerics.Vector2(0, -100);

    public List<System.Tuple<System.Numerics.Vector2, float>> predicted_Frigate_pos = new List<System.Tuple<System.Numerics.Vector2, float>>();

    public List<ShipPermutation> sp_candidates, sp_predictions = new List<ShipPermutation>();

    public bool IAPF_attack_fail = false;

    public System.Numerics.Vector2 IAPF_fail_pos = new System.Numerics.Vector2(0, -100);

    public int top_n = 0;

    public bool early_stop = false;
    public int early_stop_level = 0;

    public int find_Ships_count = 0;

    public bool avoid_again = false;
    public Vector3 ship_wait_to_solve = new Vector3(0, 0, 0);

    private Transform m_transform;
    private Rigidbody m_rigidbody;

    private int timerCount = 0;
    private int threatCount = 0;

    public int threat_total_time = 0;

    public float threat_total_value = 0.0f;
    public float threat_max_value = 0.0f;

    private Timer timer;

    private bool RF_state = false;

    private float final_x = 0.0f;

    private float final_y = 0.0f;


    private void Awake()
    {
        if (limitFPS)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }
        if (set_turnStayRF != null)
        {
            set_turnStayRF.onValueChanged.AddListener(TurnStayRF);
        }
        if (set_hardAvoid != null)
        {
            set_hardAvoid.onValueChanged.AddListener(AvoidSet);
        }
        if (set_directHit != null)
        {
            set_directHit.onValueChanged.AddListener(DirectHit);
        }
        if (set_AvoidStatic != null)
        {
            set_AvoidStatic.onValueChanged.AddListener(AvoidStatic);
        }
        if (set_GuessFormation != null)
        {
            set_GuessFormation.onValueChanged.AddListener(GuessFormation);
        }

        m_transform = this.transform;
        m_rigidbody = this.GetComponent<Rigidbody>();

        timer = FindObjectOfType<Timer>();
        timer.TimerWorked += RFWorking;
        timer.TimerWorked += LostTimeCall;
        timer.TimerWorked += Threat_Count;
        timer.TimerEndWork += TimeEnd;
    }

    public void TurnStayRF(bool arg0)
    {
        turnStayRF = arg0;
    }

    public void AvoidSet(bool arg0)
    {
        complicated_static = arg0;
    }

    public void DirectHit(bool arg0)
    {
        Hit_CV_direct_path = arg0;
    }

    public void AvoidStatic(bool arg0)
    {
        avoid_static = arg0;
    }

    public void GuessFormation(bool arg0)
    {
        guess_formation = arg0;
    }

    // Start is called before the first frame update
    void Start()
    {
        stand_OneRoundTime = (2 * Mathf.PI * stand_HalfLength) / stand_Speed;
        stand_PerSecondRotate = 360 / stand_OneRoundTime;
        stand_RotateCoefficient = stand_PerSecondRotate / stand_MaxRotate;



    }

    public void StartSimulator()
    {
        if (startSimulator)
            return;
        startSimulator = true;
        path_Gone.SetPosition(0, this.transform.position);
        path_Gone.SetPosition(1, this.transform.position);

        work_RF = true;
        timerCount = 190;
        model_RF.SetActive(false);
        timer.TimerWork();

        StartCoroutine(PathRecord());

        var pathGroups = GameObject.FindObjectOfType<PathGroupMaker>().pathGroups;
        final_x = pathGroups[0].Circles[pathGroups[0].Circles.Count - 1].transform.position.x;
        final_y = pathGroups[0].Circles[pathGroups[0].Circles.Count - 1].transform.position.z;

        //RF_WORK();
        // StartCoroutine(UpdateShip());
        if (navigate)
            StartCoroutine(SimulatorNavigate());

    }


    public void Work()
    {
        work = true;



    }

    public void End()
    {
        startSimulator = false;

        if (setting != null)
        {
            setting.Reset();
            enmyTarget = null;
            path_Gone.positionCount = 2;
            var defaultPoints = new Vector3[2];
            defaultPoints[0] = this.transform.position;
            defaultPoints[1] = this.transform.position;
            path_Gone.SetPositions(defaultPoints);
            findShips.Clear();
            work_RF = false;
            predicted_CV = false;
            predicted_CV_pos = new System.Numerics.Vector2(0, -100);
            predicted_Frigate_pos = new List<System.Tuple<System.Numerics.Vector2, float>>();
            sp_candidates = new List<ShipPermutation>();
            sp_predictions = new List<ShipPermutation>();
            top_n = 0;
            IAPF_attack_fail = false;
            IAPF_fail_pos = new System.Numerics.Vector2(0, -100);
            early_stop = false;
            early_stop_level = 0;
            find_Ships_count = 0;
            avoid_again = false;
            execute_avoid_tatic = false;
            ship_wait_to_solve = new Vector3(0, 0, 0);
            RF_state = false;
            threat_total_time = 0;
            threat_total_value = 0.0f;
            threat_max_value = 0.0f;
            moveLength = 0;

            if (pathTrace.transform.childCount == 0)
                return;

            for (int i = pathTrace.transform.childCount - 1; i > 0; i--)
                GameObject.Destroy(pathTrace.transform.GetChild(i).gameObject);

            timer.TimerEnd();

            StopCoroutine(PathRecord());
            //StopCoroutine(RFWork());
            //StopCoroutine(RFRest());
            // StopCoroutine(UpdateShip());
            if (navigate)
                StopCoroutine(SimulatorNavigate());

        }
    }

    public void Broken()
    {
        if (startSimulator)
        {
            startSimulator = false;
            work_RF = false;
            speed = 0;
        }
    }

    IEnumerator SimulatorNavigate()
    {
        while (startSimulator)
        {
            yield return null;

            if (enmyTarget == null)
            {
                if (nav_pointer < navigateRoute.Length)
                {
                    this.transform.LookAt(navigateRoute[nav_pointer]);
                    this.transform.Translate(0.0f, 0.0f, speed / 60.0f * Time.timeScale);

                    frameCount += (int)(1 * Time.timeScale);

                    if (frameCount >= 3)
                    {
                        nav_pointer = nav_pointer + (frameCount / 3);
                        frameCount %= 3;
                    }
                }
                else
                    startSimulator = false;
            }
            else
            {
                this.transform.LookAt(new Vector3(enmyTarget.transform.position.x, 10, enmyTarget.transform.position.z));
                this.transform.Translate(0.0f, 0.0f, speed / 60.0f * Time.timeScale);
            }
        }
    }

    IEnumerator PathRecord()
    {
        while (startSimulator)
        {
            if (path_Gone.GetPosition(0) == path_Gone.GetPosition(1))
                path_Gone.SetPosition(1, this.transform.position);
            else
            {
                path_Gone.positionCount++;
                path_Gone.SetPosition(path_Gone.positionCount - 1, this.transform.position);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    public virtual void TimeEnd()
    {
        threatCount = 0;
        timerCount = 190;
        model_RF.SetActive(false);
    }

    public virtual void Threat_Count()
    {
        threatCount++;

        if (threatCount >= 50)
        {
            threatCount -= 50;

            // 從猜陣型或看到航母開始計算威脅，把下方的try catch 放到 if 內
            // if (predicted_CV == true || enmyTarget != null)
            try
            {
                var real_ships = GameObject.FindObjectOfType<ShipSettingControl>().ships;

                float survive_rate = 1.0f;
                for (int i = 1; i < real_ships.Count; i++)
                {
                    if (real_ships[i].ship.name == "052C(Clone)" || real_ships[i].ship.name == "052D(Clone)")
                    {
                        float threat = Improved_APF.CD_threat_value(real_ships[i].ship.distance / 1000.0f);
                        survive_rate *= (1 - threat);

                    }
                    else if (real_ships[i].ship.name == "054A(Clone)")
                    {
                        float threat = Improved_APF.A_threat_value(real_ships[i].ship.distance / 1000.0f);
                        survive_rate *= (1 - threat);
                    }

                }
                // Debug.Log("Threat value = " + (1 - survive_rate));
                threat_total_value += (1 - survive_rate);

                if ((1 - survive_rate) > threat_max_value)
                    threat_max_value = 1 - survive_rate;
                if ((1 - survive_rate) > 0)
                    threat_total_time++;
            }
            catch (System.NullReferenceException ex)
            {
                Debug.Log("Ships are not exist!");
            }

        }
    }

    public virtual void LostTimeCall()
    {
        var count = findShips.Count;

        for (int i = 0; i < count; i++)
        {
            findShips[i].lostTime += Time.fixedDeltaTime;
        }
    }

    public virtual void RFWorking()
    {
        if ((!work_RF) || (!startSimulator))
            return;

        timerCount++;

        if (RF_state)
        {
            if (timerCount >= 10)
            {
                RF_Off();
                RF_state = false;
                timerCount -= 10;

            }

        }
        else
        {
            if (timerCount >= 200)
            {
                RF_On();
                RF_state = true;
                timerCount -= 200;
            }
        }
    }

    public void RF_On()
    {
        model_RF.SetActive(true);
        // if ((startSimulator) && (work_RF))
        // {
        //     model_RF.SetActive(true);
        // }
    }

    public void RF_Off()
    {
        model_RF.SetActive(false);
    }


    // IEnumerator RFWork()
    // {
    //     if ((startSimulator) && (work_RF))
    //     {
    //         model_RF.SetActive(true);

    //         yield return new WaitForSeconds(0.05f);
    //         StartCoroutine(RFRest());
    //     }
    // }
    // IEnumerator RFRest()
    // {
    //     if (startSimulator)
    //     {
    //         model_RF.SetActive(false);
    //         yield return new WaitForSeconds(4.0f);
    //         // StartCoroutine(RFWork());
    //     }
    // }

    // IEnumerator UpdateShip()
    // {
    //     yield return new WaitForSeconds(0.1f);
    //     if (findShips.Count > 0)
    //     {
    //         for (int i = 0; i < findShips.Count; i++)
    //         {
    //             findShips[i].lostTime += 0.1f;
    //         }
    //     }
    //     if (startSimulator)
    //         StartCoroutine(UpdateShip());
    // }

    // public void RF_WORK()
    // {
    //     work_RF = true;
    //     // StartCoroutine(RFWork());
    // }

    public void SettingAvoidPath(Vector3 ship_pos)
    {
        List<System.Numerics.Vector3> DetectedShips = new List<System.Numerics.Vector3>();
        for (int i = 0; i < findShips.Count; i++)
        {

            System.Numerics.Vector3 ship_position = new System.Numerics.Vector3(x: findShips[i].pos.x,
                                                                                y: 0.0f,
                                                                                z: findShips[i].pos.y) / 1000.0f;

            DetectedShips.Add(ship_position);
        }

        if (this.right != 0)
        {
            ship_wait_to_solve = ship_pos;
            execute_avoid_tatic = true;

        }
        else if (this.right == 0 && find_Ships_count != findShips.Count)
        // else if (this.right == 0)
        {
            string dubin_type = "";
            // 三角動態
            dubin_type = ModifyPath_surround_ship(ship_pos);

            // 第二階段動態路徑修正，與三角動態則一
            // ModifyPath(ship_pos);

            find_Ships_count = findShips.Count;
            ship_wait_to_solve = new Vector3(0, 0, 0);
            execute_avoid_tatic = false;

            // 飛彈當前位置
            // Vector2 self_2D_pos = new Vector2(this.transform.position.x, this.transform.position.z);
            System.Numerics.Vector3 startPos = new System.Numerics.Vector3(x: this.transform.position.x, y: this.transform.position.y, z: this.transform.position.z) / 1000.0f;

            // 飛彈當前航向
            // Vector2 self_f = new Vector2(this.transform.forward.x, this.transform.forward.z);
            System.Numerics.Vector2 heading_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: this.transform.forward.x, y: this.transform.forward.z));
            float startHeading = -(180 * Mathf.Atan2(this.transform.forward.z, this.transform.forward.x) / Mathf.PI - 90) * (Mathf.PI / 180);

            // Vector2 tar_2D_pos = new Vector2(ship_pos.x, ship_pos.z);

            float self_r = stand_HalfLength;
            float avoid_R = 23000 - self_r;

            #region #New tradegy
            List<PathGroup> pathGroups = GameObject.FindObjectOfType<PathGroupMaker>().pathGroups;
            List<System.Tuple<System.Numerics.Vector3, char>> InitialDiamondCircle = new List<System.Tuple<System.Numerics.Vector3, char>>();

            // 若pathGroups.Count > 1代表除了預設地毯式搜索路徑外，還有上一次避障規劃的路徑
            if (pathGroups.Count > 1)
            {
                // 從上一條未完成的避障路徑開始
                for (int i = 0; i < pathGroups[1].Circles.Count; i++)
                {
                    // 若未完成的避障路徑的最後一個圓(上一次避障的目標圓)沒有完成，則要保留繼續推估
                    if (i == pathGroups[1].Circles.Count - 1 && pathGroups[1].Circles[i].end != true)
                    {
                        // 上一個避障路徑的目標圓的迴轉方向
                        char turn_side = pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].turnMode.ToString()[0];
                        // 將上一個路徑未完成的目標圓加入InitialDiamondCircle，做為下一個避障路徑的第一個避障圓
                        System.Tuple<System.Numerics.Vector3, char> inital_circle = new System.Tuple<System.Numerics.Vector3, char>(
                                                                                    new System.Numerics.Vector3(x: pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].transform.position.x,
                                                                                                                y: pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].transform.position.y,
                                                                                                                z: pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].transform.position.z) / 1000.0f,
                                                                                    turn_side);
                        InitialDiamondCircle.Add(inital_circle);

                    }
                    // 若不是最後一個目標圓，避障圓都設定為完成
                    else
                    {
                        pathGroups[1].Circles[i].end = true;
                        pathGroups[1].Circles[i].gameObject.active = false;
                    }
                }
            }

            // 把地毯式搜索未完成的迴轉圓也依序插入InitialDiamondCircle
            List<int> not_end_idx_list = new List<int>();
            for (int i = 0; i < pathGroups[0].Circles.Count; i++)
            {
                if (!pathGroups[0].Circles[i].end)
                {
                    char turn_side = pathGroups[0].Circles[i].turnMode.ToString()[0];
                    System.Tuple<System.Numerics.Vector3, char> inital_circle = new System.Tuple<System.Numerics.Vector3, char>(
                                                                                new System.Numerics.Vector3(x: pathGroups[0].Circles[i].transform.position.x,
                                                                                                            y: pathGroups[0].Circles[i].transform.position.y,
                                                                                                            z: pathGroups[0].Circles[i].transform.position.z) / 1000.0f,
                                                                                turn_side);
                    InitialDiamondCircle.Add(inital_circle);
                    not_end_idx_list.Add(i);
                }
            }

            // avoid_path是從新的迴轉圓開始，所以要將當前迴轉圓insert到第0的位置
            (List<System.Tuple<MathFunction.Circle, char>> avoid_path, int push_circle_Index) = GeneratePath.GeneratePathFunc(startPos, startHeading, DetectedShips, InitialDiamondCircle, dubin_type);

            System.Numerics.Vector2 normal_vec;
            if (avoid_path[0].Item2 == 'R')
            {
                normal_vec = new System.Numerics.Vector2(heading_vec.Y, -heading_vec.X);
            }
            else
            {
                normal_vec = new System.Numerics.Vector2(-heading_vec.Y, heading_vec.X);
            }

            System.Drawing.PointF center = new System.Drawing.PointF(x: startPos.X + stand_HalfLength * normal_vec.X / 1000.0f,
                                                                    y: startPos.Z + stand_HalfLength * normal_vec.Y / 1000.0f);
            MathFunction.Circle first_avoid_circle = new MathFunction.Circle(center, stand_HalfLength / 1000.0f);

            // 將當前迴轉圓insert到第0的位置
            avoid_path.Insert(0, new System.Tuple<MathFunction.Circle, char>(first_avoid_circle, avoid_path[0].Item2));

            // 將原本推估新的迴轉圓移除(從當前迴轉圓到新的迴轉圓的路程太長，以下要進行路徑修正)
            avoid_path.RemoveAt(1);

            // 修正迴轉圓減少飛行路徑
            avoid_path = Reduce_Path(avoid_path);

            //根據push_circle_Index修正目標迴轉圓的end參數為true
            for (int i = 0; i < InitialDiamondCircle.Count; i++)
            {
                if (push_circle_Index >= 0)
                {
                    if (i == 0)
                    {
                        if (pathGroups.Count > 1 && pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].end != true)
                        {
                            pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].end = true;
                            pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].gameObject.active = false;
                        }
                        else
                        {
                            pathGroups[0].Circles[not_end_idx_list[0]].end = true;
                            pathGroups[0].Circles[not_end_idx_list[0]].gameObject.active = false;
                            not_end_idx_list.RemoveAt(0);
                        }

                    }
                    else
                    {
                        pathGroups[0].Circles[not_end_idx_list[0]].end = true;
                        pathGroups[0].Circles[not_end_idx_list[0]].gameObject.active = false;
                        not_end_idx_list.RemoveAt(0);

                    }
                    push_circle_Index -= 1;
                }
                else
                {
                    break;
                }
            }
            if (pathGroups.Count == 2)
            {
                pathGroups.RemoveAt(1);
            }

            var path = GameObject.FindObjectOfType<PathGroupMaker>();

            // 新增避障路徑物件，以及命名該避障路徑名稱
            var avoidPath = new PathSetting();
            avoidPath.name = "avoidPath1";
            for (int i = 0; i < avoid_path.Count; i++)
            {
                var C = new CircleData();
                C.position = new Vector2(x: avoid_path[i].Item1.center.X, y: avoid_path[i].Item1.center.Y) * 1000;
                if (avoid_path[i].Item2 == 'R')
                {
                    C.turnMode = TurnMode.Right;
                }
                else
                {
                    C.turnMode = TurnMode.Left;
                }

                // 將生成的避障圓資訊依序新增到避障路徑物件內
                avoidPath.circleDatas.Add(C);
            }

            // 將避障路徑物件丟到PathGroupMaker中的SettingPathGroup
            GameObject.FindObjectOfType<PathGroupMaker>().SettingPathGroup(avoidPath);

            #endregion
        }

    }
    public void SimpleAvoidPath(Vector3 ship_pos)
    {
        if (this.right != 0)
        {
            ship_wait_to_solve = ship_pos;
            execute_avoid_tatic = true;

        }
        else if (this.right == 0 && (find_Ships_count != findShips.Count || avoid_again == true))
        {
            avoid_again = false;
            ModifyPath_connect_final(ship_pos);

            find_Ships_count = findShips.Count;
            ship_wait_to_solve = new Vector3(0, 0, 0);
            execute_avoid_tatic = false;
            float threaten_radius = 13.55f;
            float turning_radius = 7.225f;

            var p_missile = new System.Numerics.Vector2(this.transform.position.x, this.transform.position.z) / 1000.0f;
            var p_ship = new System.Numerics.Vector2(ship_pos.x, ship_pos.z) / 1000.0f;

            List<PathGroup> pathGroups = GameObject.FindObjectOfType<PathGroupMaker>().pathGroups;
            List<System.Tuple<System.Numerics.Vector3, char>> InitialDiamondCircle = new List<System.Tuple<System.Numerics.Vector3, char>>();

            // 若pathGroups.Count > 1代表除了預設地毯式搜索路徑外，還有上一次避障規劃的路徑
            if (pathGroups.Count > 1)
            {
                // 把未完成的當前迴轉圓與避障圓刪除
                for (int i = 0; i < pathGroups[1].Circles.Count; i++)
                {

                    pathGroups[1].Circles[i].end = true;
                    pathGroups[1].Circles[i].gameObject.active = false;

                }
                pathGroups.RemoveAt(1);
            }

            // 把地毯式搜索未完成的迴轉圓也依序插入InitialDiamondCircle
            List<int> not_end_idx_list = new List<int>();
            for (int i = 0; i < pathGroups[0].Circles.Count; i++)
            {
                if (!pathGroups[0].Circles[i].end)
                {
                    char turn_side = pathGroups[0].Circles[i].turnMode.ToString()[0];
                    System.Tuple<System.Numerics.Vector3, char> inital_circle = new System.Tuple<System.Numerics.Vector3, char>(
                                                                                new System.Numerics.Vector3(x: pathGroups[0].Circles[i].transform.position.x,
                                                                                                            y: pathGroups[0].Circles[i].transform.position.y,
                                                                                                            z: pathGroups[0].Circles[i].transform.position.z) / 1000.0f,
                                                                                turn_side);
                    InitialDiamondCircle.Add(inital_circle);
                    not_end_idx_list.Add(i);
                }
            }

            if (InitialDiamondCircle.Count == 1)
            {
                SettingHitPath_direct(new System.Numerics.Vector2(p_ship.X, p_ship.Y));
                return;
            }

            System.Numerics.Vector2 target_center = new System.Numerics.Vector2(InitialDiamondCircle[0].Item1.X, InitialDiamondCircle[0].Item1.Z);
            for (int i = 0; i < InitialDiamondCircle.Count; i++)
            {
                target_center = new System.Numerics.Vector2(InitialDiamondCircle[0].Item1.X, InitialDiamondCircle[0].Item1.Z);
                float tar2obs_dist = System.Numerics.Vector2.Distance(target_center, p_ship);

                if (tar2obs_dist < 25.0f && InitialDiamondCircle.Count > 2)
                {
                    pathGroups[0].Circles[not_end_idx_list[0]].end = true;
                    pathGroups[0].Circles[not_end_idx_list[0]].gameObject.active = false;
                    not_end_idx_list.RemoveAt(0);
                    InitialDiamondCircle.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            var v_missile = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(this.transform.forward.x, this.transform.forward.z));
            var v_missile2tar = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(pathGroups[0].Circles[not_end_idx_list[0]].pointOut.transform.position.x - this.transform.position.x,
                                                                                        pathGroups[0].Circles[not_end_idx_list[0]].pointOut.transform.position.z - this.transform.position.z));

            var missile2tar_line = new MathFunction.Line(new System.Drawing.PointF(this.transform.position.x / 1000.0f, this.transform.position.z / 1000.0f),
                                                        new System.Drawing.PointF(pathGroups[0].Circles[not_end_idx_list[0]].pointOut.transform.position.x / 1000.0f,
                                                                                pathGroups[0].Circles[not_end_idx_list[0]].pointOut.transform.position.z / 1000.0f));

            var ship_pointf = new System.Drawing.PointF(p_ship.X, p_ship.Y);

            var v_relative = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(p_ship.X - p_missile.X, p_ship.Y - p_missile.Y));
            float det = v_missile2tar.X * v_relative.Y - v_relative.X * v_missile2tar.Y;
            float dot = v_missile2tar.X * v_relative.X + v_missile2tar.Y + v_relative.Y;
            float radian = Mathf.Atan2(det, dot);

            TurnMode td_route = radian > 0 ? TurnMode.Left : TurnMode.Right;
            TurnMode td_now;

            float radian_half_pi;

            if (td_route == TurnMode.Left)
            {
                td_now = TurnMode.Right;
                radian_half_pi = -Mathf.PI / 2;
            }
            else
            {
                td_now = TurnMode.Left;
                radian_half_pi = Mathf.PI / 2;
            }
            // 當前迴轉圓的計算方式是參考當前航向的法向量進行推算
            float rotated_x = v_missile.X * Mathf.Cos(radian_half_pi) - v_missile.Y * Mathf.Sin(radian_half_pi);
            float rotated_y = v_missile.X * Mathf.Sin(radian_half_pi) + v_missile.Y * Mathf.Cos(radian_half_pi);
            var v_normal = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(rotated_x, rotated_y));

            PathSetting avoidPath = new PathSetting();
            avoidPath.name = "avoidPath1";

            // 當前迴轉圓
            CircleData c_now = new CircleData();
            c_now.position = new Vector2(p_missile.X + v_normal.X * turning_radius, p_missile.Y + v_normal.Y * turning_radius) * 1000;
            c_now.turnMode = td_now;

            // 避障圓的計算方式是參考當前位置往目標位置的向量的法向量
            float rotated_missile2tar_x = v_missile2tar.X * Mathf.Cos(radian_half_pi) - v_missile2tar.Y * Mathf.Sin(radian_half_pi);
            float rotated_missile2tar_y = v_missile2tar.X * Mathf.Sin(radian_half_pi) + v_missile2tar.Y * Mathf.Cos(radian_half_pi);
            var v_missile2tar_normal = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(rotated_missile2tar_x, rotated_missile2tar_y));

            // 避障圓
            CircleData c_route = new CircleData();

            var ship2line_dist = (float)MathFunction.pointToLine(missile2tar_line, ship_pointf);
            if (ship2line_dist < threaten_radius + turning_radius)
            {
                c_route.position = new Vector2(p_ship.X + v_missile2tar_normal.X * threaten_radius, p_ship.Y + v_missile2tar_normal.Y * threaten_radius) * 1000;
            }
            else
            {
                c_route.position = new Vector2(p_ship.X + v_missile2tar_normal.X * (ship2line_dist - turning_radius), p_ship.Y + v_missile2tar_normal.Y * (ship2line_dist - turning_radius)) * 1000;
            }

            c_route.turnMode = td_route;

            for (int i = 0; i < InitialDiamondCircle.Count; i++)
            {
                target_center = new System.Numerics.Vector2(InitialDiamondCircle[0].Item1.X, InitialDiamondCircle[0].Item1.Z);
                float tar2returncircle_dist = System.Numerics.Vector2.Distance(target_center, new System.Numerics.Vector2(c_route.position.x / 1000.0f, c_route.position.y / 1000.0f));

                if (tar2returncircle_dist < 14.5f && InitialDiamondCircle.Count > 2)
                {
                    pathGroups[0].Circles[not_end_idx_list[0]].end = true;
                    pathGroups[0].Circles[not_end_idx_list[0]].gameObject.active = false;
                    not_end_idx_list.RemoveAt(0);
                    InitialDiamondCircle.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            float now2route = System.Numerics.Vector2.Distance(new System.Numerics.Vector2(c_now.position.x / 1000.0f, c_now.position.y / 1000.0f),
                                                                new System.Numerics.Vector2(c_route.position.x / 1000.0f, c_route.position.y / 1000.0f));


            if (now2route < 14.5f)
            {
                // c_route.position.x = ((c_route.position.x / 1000.0f) + target_center.X) / 2 * 1000.0f;
                // c_route.position.y = ((c_route.position.y / 1000.0f) + target_center.Y) / 2 * 1000.0f;
                avoidPath.circleDatas.Add(c_now);
            }
            else
            {
                avoidPath.circleDatas.Add(c_now);
                avoidPath.circleDatas.Add(c_route);
            }

            // 將避障路徑物件丟到PathGroupMaker中的SettingPathGroup
            GameObject.FindObjectOfType<PathGroupMaker>().SettingPathGroup(avoidPath);
        }
    }

    public List<System.Tuple<MathFunction.Circle, char>> Reduce_Path(List<System.Tuple<MathFunction.Circle, char>> avoid_path)
    {
        // 用角度依序判斷迴轉圓是否與目標反方向
        for (int i = 1; i < avoid_path.Count - 1; i++)
        {
            // 第一迴轉圓心至當前迴轉圓心至目標圓心的夾角
            float angle_first = (float)MathFunction.Angle(avoid_path[0].Item1.center, avoid_path[1].Item1.center, avoid_path[avoid_path.Count - 1].Item1.center);
            // 第二迴轉圓心至當前迴轉圓心至目標圓心的夾角
            float angle_second = (float)MathFunction.Angle(avoid_path[0].Item1.center, avoid_path[2].Item1.center, avoid_path[avoid_path.Count - 1].Item1.center);
            // 當前迴轉圓心至第一迴轉圓心的直線距離
            float ori_first_dist = (float)MathFunction.Distance(avoid_path[0].Item1.center, avoid_path[1].Item1.center);

            // 若angle_first與angle_second都大於90度，代表該兩圓都在當前飛行方向的後方(與目標方向相反)，刪除第一個迴轉圓，保留第二個
            if (angle_first > 90 && angle_second > 90)
            {
                avoid_path.RemoveAt(1);
            }
            // 若angle_second小於90度，且不論angle_first是大於90度或第一個迴轉圓與當前迴轉圓兩個相割，都要對第一個轉折圓進行推算
            else if (angle_second < 90 && (angle_first > 90 || ori_first_dist < 14.45f))
            {
                // 當前迴轉圓心至第二迴轉圓心的直線距離
                float ori_second_dist = (float)MathFunction.Distance(avoid_path[0].Item1.center, avoid_path[2].Item1.center);

                if (ori_second_dist > 14.55f)
                {
                    System.Drawing.PointF new_return_center;
                    System.Drawing.PointF intersection1;
                    System.Drawing.PointF intersection2;
                    int intersections = MathFunction.FindLineCircleIntersections(avoid_path[0].Item1.center.X, avoid_path[0].Item1.center.Y, 14.55f,
                                                                                avoid_path[1].Item1.center, avoid_path[2].Item1.center, out intersection1, out intersection2);

                    if (intersections == 2)
                    {
                        System.Numerics.Vector2 ori_center_intersect = new System.Numerics.Vector2(intersection1.X - avoid_path[0].Item1.center.X,
                                                                                                intersection1.Y - avoid_path[0].Item1.center.Y);

                        System.Numerics.Vector2 ori_center_second_center = new System.Numerics.Vector2(avoid_path[2].Item1.center.X - avoid_path[0].Item1.center.X,
                                                                                                        avoid_path[2].Item1.center.Y - avoid_path[0].Item1.center.Y);

                        if (System.Numerics.Vector2.Dot(ori_center_second_center, ori_center_intersect) > 0)
                        {
                            new_return_center = intersection1;
                        }
                        else
                        {
                            new_return_center = intersection2;
                        }

                        // 經推算的第一個迴轉圓，此迴轉圓一定不會與當前迴轉圓相割，但要確保也不會與第二個迴轉圓相割
                        MathFunction.Circle new_avoid_center = new MathFunction.Circle(new_return_center, stand_HalfLength / 1000.0f);

                        // 計算當前迴轉圓與第二個迴轉圓的圓心距離
                        float ori_center_second_center_dist = (float)MathFunction.Distance(avoid_path[0].Item1.center, avoid_path[2].Item1.center);
                        // 計算當前迴轉圓與新推算迴轉圓的圓心距離
                        float ori_center_new_center_dist = (float)MathFunction.Distance(avoid_path[0].Item1.center, new_avoid_center.center);
                        // 計算新推算迴轉圓與第二個迴轉圓的圓心距離
                        float new_center_second_center_dist = (float)MathFunction.Distance(avoid_path[2].Item1.center, new_avoid_center.center);

                        //若原本第一迴轉圓(新迴轉圓與第一迴轉圓迴轉方向相同)與第二迴轉圓的旋轉方向不同(代表要取內公切線)，
                        //若新的迴轉圓與第二個迴轉圓的圓心距離小於14.45代表兩圓相割，沒有內公切線
                        if (avoid_path[1].Item2 != avoid_path[2].Item2 && new_center_second_center_dist < 14.45f)
                        {
                            avoid_path.RemoveAt(1);
                            return avoid_path;
                        }

                        if (ori_center_second_center_dist > ori_center_new_center_dist)
                        {
                            avoid_path.Insert(1, new System.Tuple<MathFunction.Circle, char>(new_avoid_center, avoid_path[1].Item2));
                            avoid_path.RemoveAt(2);
                            return avoid_path;
                        }
                        else
                        {
                            return avoid_path;

                        }
                    }
                    else
                    {
                        return avoid_path;
                    }
                }
                else if (ori_second_dist < 14.55f && avoid_path[0].Item2 != avoid_path[2].Item2)
                {
                    System.Drawing.PointF new_return_center;
                    System.Drawing.PointF intersection1;
                    System.Drawing.PointF intersection2;
                    int intersections = MathFunction.FindLineCircleIntersections(avoid_path[0].Item1.center.X, avoid_path[0].Item1.center.Y, 14.55f,
                                                                                avoid_path[2].Item1.center, avoid_path[3].Item1.center, out intersection1, out intersection2);

                    if (intersections == 2)
                    {
                        System.Numerics.Vector2 ori_center_intersect = new System.Numerics.Vector2(intersection1.X - avoid_path[0].Item1.center.X,
                                                                                                intersection1.Y - avoid_path[0].Item1.center.Y);

                        System.Numerics.Vector2 ori_center_third_center = new System.Numerics.Vector2(avoid_path[3].Item1.center.X - avoid_path[0].Item1.center.X,
                                                                                                        avoid_path[3].Item1.center.Y - avoid_path[0].Item1.center.Y);

                        if (System.Numerics.Vector2.Dot(ori_center_third_center, ori_center_intersect) > 0)
                        {
                            new_return_center = intersection1;
                        }
                        else
                        {
                            new_return_center = intersection2;
                        }

                        // 經推算的第一個迴轉圓，此迴轉圓一定不會與當前迴轉圓相割，但要確保也不會與第二個迴轉圓相割
                        MathFunction.Circle new_avoid_center = new MathFunction.Circle(new_return_center, stand_HalfLength / 1000.0f);

                        avoid_path.Insert(3, new System.Tuple<MathFunction.Circle, char>(new_avoid_center, avoid_path[2].Item2));
                        avoid_path.RemoveAt(2);
                        avoid_path.RemoveAt(1);
                        return avoid_path;
                    }
                }
            }
            else
            {
                return avoid_path;
            }
        }
        return avoid_path;
    }

    public (System.Numerics.Vector2, List<System.Tuple<System.Numerics.Vector2, float>>, List<ShipPermutation>) Predict_CV()
    {
        find_Ships_count = findShips.Count;
        predicted_CV = true;
        System.Numerics.Vector2 CV_pos = new System.Numerics.Vector2();
        var Frigate_pos = new List<System.Tuple<System.Numerics.Vector2, float>>();

        var sp_candidates_remain = new List<ShipPermutation>();

        for (int i = top_n; i < sp_predictions.Count; i++)
        {
            sp_candidates_remain.Add(sp_predictions[i]);
        }

        List<Ship> ships = new List<Ship>();
        for (int i = 0; i < findShips.Count; i++)
        {

            System.Numerics.Vector3 ship_position = new System.Numerics.Vector3(x: findShips[i].pos.x,
                                                                                y: 0.0f,
                                                                                z: findShips[i].pos.y) / 1000.0f;
            if (findShips[i].guessName == "CVLL")
            {
                ships.Add(new Ship(x: ship_position.X, y: ship_position.Z, position_type: "xy", name: "CVLL"));
            }
            else
            {
                ships.Add(new Ship(x: ship_position.X, y: ship_position.Z, position_type: "xy"));
            }

        }

        // double course = -(180 * Mathf.Atan2(Ship_move_vec.y, Ship_move_vec.x) / Mathf.PI - 90);
        // course = -course - 90;
        // List<Ship> ships = new List<Ship>{
        //     new Ship(x:DetectedShips[0].X, y:DetectedShips[0].Z, position_type:"xy"),
        //     new Ship(x:DetectedShips[1].X, y:DetectedShips[1].Z, position_type:"xy"),
        //     new Ship(x:DetectedShips[2].X, y:DetectedShips[2].Z, position_type:"xy"),
        // };

        (sp_candidates, sp_predictions) = this.predictor.predict(new ShipPermutation(mode: "inference", ships: ships));

        top_n = 0;

        if (sp_candidates_remain.Count > 0)
        {
            sp_predictions.Insert(0, sp_candidates_remain[0]);
        }

        for (int i = 1; i < sp_candidates_remain.Count; i++)
        {
            bool remove_i = false;
            var CV_pos_i = new Vector2((float)sp_candidates_remain[i].ship_position_predict["CVLL"].x, (float)sp_candidates_remain[i].ship_position_predict["CVLL"].y);
            for (int j = 0; j < sp_predictions.Count; j++)
            {
                var CV_pos_j = new Vector2((float)sp_predictions[j].ship_position_predict["CVLL"].x, (float)sp_predictions[j].ship_position_predict["CVLL"].y);
                var dist = Vector2.Distance(CV_pos_i, CV_pos_j);
                if (dist < 7.225f)
                {
                    remove_i = true;
                    break;
                }
            }
            if (remove_i == false)
            {
                sp_predictions.Add(sp_candidates_remain[i]);
            }

        }

        (CV_pos, Frigate_pos) = potential_CV();

        return (CV_pos, Frigate_pos, sp_predictions);

    }

    public (System.Numerics.Vector2, List<System.Tuple<System.Numerics.Vector2, float>>) potential_CV()
    {
        var CV_pos = new System.Numerics.Vector2();
        var Frigate_pos = new List<System.Tuple<System.Numerics.Vector2, float>>();

        // 飛彈當前位置
        var missile_pos = new System.Numerics.Vector2(this.transform.position.x, this.transform.position.z) / 1000.0f;
        var startPos_point = new System.Drawing.PointF(x: missile_pos.X, y: missile_pos.Y);
        // 飛彈當前航向
        var heading_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: this.transform.forward.x, y: this.transform.forward.z));
        var second_point = new System.Drawing.PointF(x: startPos_point.X + this.transform.forward.x, y: startPos_point.Y + this.transform.forward.z);

        for (int idx = top_n; idx < sp_predictions.Count; idx++)
        {
            CV_pos = new System.Numerics.Vector2(x: (float)sp_predictions[idx].ship_position_predict["CVLL"].x, y: (float)sp_predictions[idx].ship_position_predict["CVLL"].y);
            var CV_point = new System.Drawing.PointF(x: CV_pos.X, y: CV_pos.Y);

            int CV_side = MathFunction.SideOfVector(startPos_point, second_point, CV_point);

            System.Numerics.Vector2 normal_vec;
            // right
            if (CV_side == -1)
            {
                normal_vec = new System.Numerics.Vector2(x: heading_vec.Y, y: -heading_vec.X);
            }
            // left
            else
            {
                normal_vec = new System.Numerics.Vector2(x: -heading_vec.Y, y: heading_vec.X);
            }

            var return_center = new System.Drawing.PointF(x: startPos_point.X + 7.225f * normal_vec.X, y: startPos_point.Y + 7.225f * normal_vec.Y);
            float return_center2CV_dist = (float)MathFunction.Distance(return_center, CV_point);

            if (return_center2CV_dist > 7.225f)
            {
                string[] frigates_type = new string[] { "C1", "C2", "D", "A1", "A2" };
                for (int i = 0; i < frigates_type.Length; i++)
                {
                    float threaten_radius;
                    if (frigates_type[i] == "C1" || frigates_type[i] == "C2" || frigates_type[i] == "D") threaten_radius = 28.0f;
                    else threaten_radius = 15.0f;

                    Frigate_pos.Add(new System.Tuple<System.Numerics.Vector2, float>(new System.Numerics.Vector2(x: (float)sp_predictions[idx].ship_position_predict[frigates_type[i]].x,
                                                                                                                y: (float)sp_predictions[idx].ship_position_predict[frigates_type[i]].y),
                                                                                                                threaten_radius));
                }
                top_n = idx;
                Debug.Log($"Top{top_n}, Type={sp_predictions[top_n].formation}, CV=({CV_pos.X}, {CV_pos.Y})");
                return (CV_pos, Frigate_pos);
            }

        }

        Debug.Log($"Can't reach target!");
        return (new System.Numerics.Vector2(), new List<System.Tuple<System.Numerics.Vector2, float>>());
    }


    public void SettingHitPath_direct(System.Numerics.Vector2 CV_pos)
    {

        // System.Numerics.Vector2 CV_pos = new System.Numerics.Vector2(x: (float)ships_pos["CVLL"].x, y: (float)ships_pos["CVLL"].y);

        // 飛彈當前位置
        System.Numerics.Vector2 startPos = new System.Numerics.Vector2(x: this.transform.position.x, y: this.transform.position.z) / 1000.0f;
        System.Drawing.PointF startPos_point = new System.Drawing.PointF(x: startPos.X, y: startPos.Y);

        // 飛彈當前航向
        System.Numerics.Vector2 heading_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: this.transform.forward.x, y: this.transform.forward.z));
        float startHeading = -(180 * Mathf.Atan2(this.transform.forward.z, this.transform.forward.x) / Mathf.PI - 90) * (Mathf.PI / 180);

        System.Drawing.PointF second_point = new System.Drawing.PointF(x: startPos_point.X + this.transform.forward.x, y: startPos_point.Y + this.transform.forward.z);

        System.Drawing.PointF CV_point = new System.Drawing.PointF(x: CV_pos.X, y: CV_pos.Y);

        int CV_side = MathFunction.SideOfVector(startPos_point, second_point, CV_point);

        System.Numerics.Vector2 normal_vec;
        // right
        if (CV_side == -1)
        {
            normal_vec = new System.Numerics.Vector2(x: heading_vec.Y, y: -heading_vec.X);
        }
        // left
        else
        {
            normal_vec = new System.Numerics.Vector2(x: -heading_vec.Y, y: heading_vec.X);
        }

        System.Drawing.PointF return_center = new System.Drawing.PointF(x: startPos_point.X + 7.225f * normal_vec.X, y: startPos_point.Y + 7.225f * normal_vec.Y);
        float return_center2CV_dist = (float)MathFunction.Distance(return_center, CV_point);
        float radian = Mathf.Acos(7.225f / return_center2CV_dist);
        var center2CV_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(CV_point.X - return_center.X, CV_point.Y - return_center.Y));

        Vector3 waypoint_1;
        Vector3 waypoint_2 = new Vector3(x: CV_point.X, y: 0.0f, z: CV_point.Y); ;
        // right
        if (CV_side == -1)
        {
            var center2wp1_vec = new System.Numerics.Vector2(center2CV_vec.X * Mathf.Cos(radian) - center2CV_vec.Y * Mathf.Sin(radian),
                                                            center2CV_vec.X * Mathf.Sin(radian) + center2CV_vec.Y * Mathf.Cos(radian));
            waypoint_1 = new Vector3(x: return_center.X + 7.225f * center2wp1_vec.X, y: 0.0f, z: return_center.Y + 7.225f * center2wp1_vec.Y);
        }
        else
        {
            var center2wp1_vec = new System.Numerics.Vector2(center2CV_vec.X * Mathf.Cos(-radian) - center2CV_vec.Y * Mathf.Sin(-radian),
                                                            center2CV_vec.X * Mathf.Sin(-radian) + center2CV_vec.Y * Mathf.Cos(-radian));

            waypoint_1 = new Vector3(x: return_center.X + 7.225f * center2wp1_vec.X, y: 0.0f, z: return_center.Y + 7.225f * center2wp1_vec.Y);
        }

        System.Numerics.Vector2 final_dir_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: waypoint_2.x - waypoint_1.x, y: waypoint_2.z - waypoint_1.z));
        System.Numerics.Vector2 final_dir_normal_vec;
        if (CV_side == -1)
        {
            final_dir_normal_vec = new System.Numerics.Vector2(x: final_dir_vec.Y, y: -final_dir_vec.X);
        }
        else
        {
            final_dir_normal_vec = new System.Numerics.Vector2(x: -final_dir_vec.Y, y: final_dir_vec.X);
        }
        System.Drawing.PointF final_return_center = new System.Drawing.PointF(x: CV_pos.X + 7.225f * final_dir_normal_vec.X, y: CV_pos.Y + 7.225f * final_dir_normal_vec.Y);

        List<PathGroup> pathGroups = GameObject.FindObjectOfType<PathGroupMaker>().pathGroups;

        if (pathGroups.Count == 2)
        {
            for (int i = 0; i < pathGroups[1].Circles.Count; i++)
            {
                pathGroups[1].Circles[i].end = true;
                pathGroups[1].Circles[i].gameObject.active = false;
            }
            pathGroups.RemoveAt(1);
        }

        for (int i = 0; i < pathGroups[0].Circles.Count; i++)
        {
            pathGroups[0].Circles[i].end = true;
            pathGroups[0].Circles[i].gameObject.active = false;
        }

        var path = GameObject.FindObjectOfType<PathGroupMaker>();
        // 新增避障路徑物件，以及命名該避障路徑名稱
        var avoidPath = new PathSetting();
        avoidPath.name = "avoidPath1";

        var C1 = new CircleData();
        C1.position = new Vector2(x: return_center.X, y: return_center.Y) * 1000;

        var C2 = new CircleData();
        C2.position = new Vector2(x: final_return_center.X, y: final_return_center.Y) * 1000;

        if (CV_side == -1)
        {
            C1.turnMode = TurnMode.Right;
            C2.turnMode = TurnMode.Right;
        }
        else
        {
            C1.turnMode = TurnMode.Left;
            C2.turnMode = TurnMode.Left;
        }
        avoidPath.circleDatas.Add(C1);
        avoidPath.circleDatas.Add(C2);

        var group = new PathGroup();
        // PathGroup物件的名稱為避障路徑名稱
        group.groupName = avoidPath.name;
        var PathGroupMaker = GameObject.FindObjectOfType<PathGroupMaker>();
        for (int i = 0; i < avoidPath.circleDatas.Count; i++)
        {
            var circle = GameObject.Instantiate(PathGroupMaker.turncircle_prefab, new Vector3(avoidPath.circleDatas[i].position.x, 10, avoidPath.circleDatas[i].position.y), new Quaternion().normalized, PathGroupMaker.transform);
            circle.name = avoidPath.name + "_circle" + (i + 1);
            circle.turnMode = avoidPath.circleDatas[i].turnMode;
            circle.pathGroupMaker = PathGroupMaker;
            group.Circles.Add(circle);

        }
        pathGroups.Add(group);
        PathGroupMaker.LinkPathCircles(group.groupName);

        // 將避障路徑物件丟到PathGroupMaker中的SettingPathGroup
        // GameObject.FindObjectOfType<PathGroupMaker>().SettingPathGroup(avoidPath);

    }

    public void SettingHitPath_APF(System.Numerics.Vector2 CV_pos, List<System.Tuple<System.Numerics.Vector2, float>> Frigate_pos)
    {

        // 飛彈當前位置
        System.Numerics.Vector2 startPos = new System.Numerics.Vector2(x: this.transform.position.x, y: this.transform.position.z) / 1000.0f;
        System.Drawing.PointF startPos_point = new System.Drawing.PointF(x: startPos.X, y: startPos.Y);

        // 飛彈當前航向
        System.Numerics.Vector2 heading_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: this.transform.forward.x, y: this.transform.forward.z));
        System.Drawing.PointF second_point = new System.Drawing.PointF(x: startPos_point.X + this.transform.forward.x, y: startPos_point.Y + this.transform.forward.z);

        (var APF_path, var APF_plan_success) = Improved_APF.IAPF_returnCircle(Frigate_pos, startPos, heading_vec, CV_pos);

        try
        {
            if (APF_path.Count > 0)
            {
                // 飛彈終點(航母位置)之航向，為最後一個避障圓之切點到航母位置之向量
                System.Numerics.Vector2 final_direction_vec = new System.Numerics.Vector2(x: CV_pos.X - APF_path[APF_path.Count - 1].Item3[1].X, y: CV_pos.Y - APF_path[APF_path.Count - 1].Item3[1].Y);
                final_direction_vec = System.Numerics.Vector2.Normalize(final_direction_vec);

                System.Numerics.Vector2 final_direction_normal_vec;
                if (APF_path[APF_path.Count - 1].Item2 == "R")
                {
                    final_direction_normal_vec = new System.Numerics.Vector2(x: final_direction_vec.Y, y: -final_direction_vec.X);
                }
                else
                {
                    final_direction_normal_vec = new System.Numerics.Vector2(x: -final_direction_vec.Y, y: final_direction_vec.X);
                }

                System.Drawing.PointF final_return_center = new System.Drawing.PointF(x: CV_pos.X + 7.225f * final_direction_normal_vec.X, y: CV_pos.Y + 7.225f * final_direction_normal_vec.Y);

                List<PathGroup> pathGroups = GameObject.FindObjectOfType<PathGroupMaker>().pathGroups;

                if (pathGroups.Count == 2)
                {
                    for (int i = 0; i < pathGroups[1].Circles.Count; i++)
                    {
                        pathGroups[1].Circles[i].end = true;
                        pathGroups[1].Circles[i].gameObject.active = false;
                    }
                    pathGroups.RemoveAt(1);
                }

                for (int i = 0; i < pathGroups[0].Circles.Count; i++)
                {
                    pathGroups[0].Circles[i].end = true;
                    pathGroups[0].Circles[i].gameObject.active = false;
                }

                var path = GameObject.FindObjectOfType<PathGroupMaker>();
                // 新增避障路徑物件，以及命名該避障路徑名稱
                var avoidPath = new PathSetting();
                avoidPath.name = "avoidPath1";

                var C_first = new CircleData();
                C_first.position = new Vector2(x: APF_path[0].Item1.center.X, y: APF_path[0].Item1.center.Y) * 1000.0f;
                if (APF_path[0].Item2 == "R") C_first.turnMode = TurnMode.Right;
                else C_first.turnMode = TurnMode.Left;
                avoidPath.circleDatas.Add(C_first);

                var C_final = new CircleData();
                C_final.position = new Vector2(x: final_return_center.X, y: final_return_center.Y) * 1000.0f;
                if (APF_path[APF_path.Count - 1].Item2 == "R") C_final.turnMode = TurnMode.Right;
                else C_final.turnMode = TurnMode.Left;
                avoidPath.circleDatas.Add(C_final);

                for (int i = 1; i < APF_path.Count; i++)
                {
                    var C = new CircleData();
                    C.position = new Vector2(x: APF_path[i].Item1.center.X, y: APF_path[i].Item1.center.Y) * 1000.0f;
                    if (APF_path[i].Item2 == "L") C.turnMode = TurnMode.Left;
                    else C.turnMode = TurnMode.Right;
                    avoidPath.circleDatas.Insert(avoidPath.circleDatas.Count - 1, C);

                }

                var group = new PathGroup();
                // PathGroup物件的名稱為避障路徑名稱
                group.groupName = avoidPath.name;
                var PathGroupMaker = GameObject.FindObjectOfType<PathGroupMaker>();
                for (int i = 0; i < avoidPath.circleDatas.Count; i++)
                {
                    var circle = GameObject.Instantiate(PathGroupMaker.turncircle_prefab, new Vector3(avoidPath.circleDatas[i].position.x, 10, avoidPath.circleDatas[i].position.y), new Quaternion().normalized, PathGroupMaker.transform);
                    circle.name = avoidPath.name + "_circle" + (i + 1);
                    circle.turnMode = avoidPath.circleDatas[i].turnMode;
                    circle.pathGroupMaker = PathGroupMaker;
                    group.Circles.Add(circle);

                }
                pathGroups.Add(group);
                PathGroupMaker.LinkPathCircles(group.groupName);
                IAPF_attack_fail = false;
                IAPF_fail_pos = new System.Numerics.Vector2(0, -100);
            }
            else if (APF_plan_success == true && APF_path.Count == 0)
            {
                SettingHitPath_direct(CV_pos);
            }
            else
            {
                IAPF_attack_fail = true;
                IAPF_fail_pos = new System.Numerics.Vector2(x: this.transform.position.x, y: this.transform.position.z) / 1000.0f;
                predicted_Frigate_pos = new List<System.Tuple<System.Numerics.Vector2, float>>(Frigate_pos);
                Debug.Log("兩個引力參數都無法規劃APF路徑, 5公里後再試!");
            }

        }
        catch (System.ArgumentOutOfRangeException ex)
        {
            Debug.Log("未知原因無法規劃APF路徑");
            // throw;
        }
    }

    // 回傳航母的位置與護衛艦的位置
    public (System.Numerics.Vector2, List<System.Tuple<System.Numerics.Vector2, float>>) Reorganize_ships()
    {
        find_Ships_count = findShips.Count;
        System.Numerics.Vector2 CV_pos = new System.Numerics.Vector2();
        List<System.Tuple<System.Numerics.Vector2, float>> Frigate_pos = new List<System.Tuple<System.Numerics.Vector2, float>>();

        for (int i = 0; i < findShips.Count; i++)
        {
            if (findShips[i].guessName == "CVLL")
            {
                // CV_pos = new System.Numerics.Vector2(findShips[i].pos.x + findShips[i].lostTime * findShips[i].moveVec.x,
                //                                     findShips[i].pos.y + findShips[i].lostTime * findShips[i].moveVec.y) / 1000.0f;
                CV_pos = new System.Numerics.Vector2(findShips[i].pos.x,
                                                    findShips[i].pos.y) / 1000.0f;
            }
            else
            {
                // Frigate_pos.Add(new System.Tuple<System.Numerics.Vector2, float>(new System.Numerics.Vector2(x: findShips[i].pos.x + findShips[i].lostTime * findShips[i].moveVec.x,
                //                                                                                             y: findShips[i].pos.y + findShips[i].lostTime * findShips[i].moveVec.y) / 1000.0f,
                //                                                                                             28.0f));
                Frigate_pos.Add(new System.Tuple<System.Numerics.Vector2, float>(new System.Numerics.Vector2(x: findShips[i].pos.x,
                                                                                                            y: findShips[i].pos.y) / 1000.0f,
                                                                                                            28.0f));
            }

        }
        return (CV_pos, Frigate_pos);
    }
    public void Hit_CV(System.Numerics.Vector2 CV_pos, List<System.Tuple<System.Numerics.Vector2, float>> Frigate_pos)
    {
        if (Frigate_pos.Count == 0 && (CV_pos.X == 0 && CV_pos.Y == 0))
            return;
        System.Drawing.PointF CV_point = new System.Drawing.PointF(CV_pos.X, CV_pos.Y);
        System.Drawing.PointF now_pos = new System.Drawing.PointF(this.transform.position.x / 1000.0f, this.transform.position.z / 1000.0f);

        float dist2CV = (float)MathFunction.Distance(now_pos, CV_point);

        if (dist2CV < 15.0f || Frigate_pos.Count == 0 || Hit_CV_direct_path == true)
        {

            SettingHitPath_direct(CV_pos);
        }
        else
        {
            SettingHitPath_APF(CV_pos, Frigate_pos);
        }
    }
    public void ModifyPath(Vector3 ship_pos)
    {
        List<PathGroup> pathGroups = GameObject.FindObjectOfType<PathGroupMaker>().pathGroups;
        var PathGroupMaker = GameObject.FindObjectOfType<PathGroupMaker>();
        int connect_back_idx = 6;

        if (pathGroups[0].Circles[1].pointIn.isActiveAndEnabled == true && early_stop == false)
        {
            for (int i = pathGroups[0].Circles.Count - 1; i > 1; i--)
            {
                GameObject.Destroy(pathGroups[0].Circles[i - 1].pointOut.gameObject);
                pathGroups[0].Circles[i].end = true;
                pathGroups[0].Circles[i].gameObject.active = false;
                pathGroups[0].Circles.RemoveAt(i);
            }

            List<Vector3> early_stop_circle_list = new List<Vector3>();
            early_stop_circle_list.Add(new Vector3(-52775.0f, 10.0f, 0.0f));
            early_stop_circle_list.Add(new Vector3(-26387.5f, 10.0f, -26387.5f));
            early_stop_circle_list.Add(new Vector3(0.0f, 10.0f, 0.0f));

            string group_name = pathGroups[0].groupName;
            for (int i = 0; i < early_stop_circle_list.Count; i++)
            {
                var circle = GameObject.Instantiate(PathGroupMaker.turncircle_prefab, new Vector3(early_stop_circle_list[i].x, 10, early_stop_circle_list[i].z), new Quaternion().normalized, PathGroupMaker.transform);
                circle.name = group_name + "_circle" + (pathGroups[0].Circles.Count + 1);
                circle.turnMode = pathGroups[0].Circles[pathGroups[0].Circles.Count - 1].turnMode;
                circle.pathGroupMaker = PathGroupMaker;
                pathGroups[0].Circles.Add(circle);
                pathGroups[0].Circles[pathGroups[0].Circles.Count - 2].LinkNext(pathGroups[0].Circles[pathGroups[0].Circles.Count - 1]);

            }
            early_stop = true;
            early_stop_level = 1;
        }
        else if (pathGroups[0].Circles[2].pointIn.isActiveAndEnabled == true && early_stop == false)
        {
            for (int i = pathGroups[0].Circles.Count - 1; i > 2; i--)
            {
                GameObject.Destroy(pathGroups[0].Circles[i - 1].pointOut.gameObject);
                pathGroups[0].Circles[i].end = true;
                pathGroups[0].Circles[i].gameObject.active = false;
                pathGroups[0].Circles.RemoveAt(i);
            }

            List<Vector3> early_stop_circle_list = new List<Vector3>();
            early_stop_circle_list.Add(new Vector3(-52775.0f, 10.0f, 0.0f));
            early_stop_circle_list.Add(new Vector3(0.0f, 10.0f, 0.0f));

            string group_name = pathGroups[0].groupName;
            for (int i = 0; i < early_stop_circle_list.Count; i++)
            {
                var circle = GameObject.Instantiate(PathGroupMaker.turncircle_prefab, new Vector3(early_stop_circle_list[i].x, 10, early_stop_circle_list[i].z), new Quaternion().normalized, PathGroupMaker.transform);
                circle.name = group_name + "_circle" + (pathGroups[0].Circles.Count + 1);
                circle.turnMode = pathGroups[0].Circles[pathGroups[0].Circles.Count - 1].turnMode;
                circle.pathGroupMaker = PathGroupMaker;
                pathGroups[0].Circles.Add(circle);
                pathGroups[0].Circles[pathGroups[0].Circles.Count - 2].LinkNext(pathGroups[0].Circles[pathGroups[0].Circles.Count - 1]);

            }
            early_stop = true;
            early_stop_level = 2;

        }

        connect_back_idx = (early_stop == true) ? 5 : 6;
        if (pathGroups[0].Circles.Count == 7)
        {
            // if (pathGroups[0].Circles[connect_back_idx - 1].end == false || (pathGroups[0].Circles[connect_back_idx - 1].end == true && pathGroups.Count == 2))
            if (pathGroups[0].Circles[connect_back_idx - 1].end == false)
            {
                // GameObject.Destroy(pathGroups[0].Circles[connect_back_idx - 1].pointOut.gameObject);
                // pathGroups[0].Circles[connect_back_idx].end = true;
                // pathGroups[0].Circles[connect_back_idx].gameObject.active = false;
                // pathGroups[0].Circles.RemoveAt(connect_back_idx);

                int remove_time = 1;
                if (early_stop == true)
                    remove_time = 2;

                for (int i = 0; i < remove_time; i++)
                {
                    GameObject.Destroy(pathGroups[0].Circles[pathGroups[0].Circles.Count - 2].pointOut.gameObject);
                    pathGroups[0].Circles[pathGroups[0].Circles.Count - 1].end = true;
                    pathGroups[0].Circles[pathGroups[0].Circles.Count - 1].gameObject.active = false;
                    pathGroups[0].Circles.RemoveAt(pathGroups[0].Circles.Count - 1);
                }

                Vector2 middle_point = new Vector2(0.0f, 0.0f);
                for (int i = 0; i < findShips.Count; i++)
                {
                    // Vector2 ship_pos_update = findShips[i].pos + (findShips[i].lostTime * findShips[i].moveVec);
                    Vector2 ship_pos_update = findShips[i].pos;
                    middle_point += ship_pos_update;
                }
                middle_point /= findShips.Count;

                string group_name = pathGroups[0].groupName;
                var circle = GameObject.Instantiate(PathGroupMaker.turncircle_prefab, new Vector3(middle_point.x, 10, middle_point.y), new Quaternion().normalized, PathGroupMaker.transform);
                circle.name = group_name + "_circle" + (pathGroups[0].Circles.Count + 1);
                circle.pathGroupMaker = PathGroupMaker;

                if (pathGroups[0].Circles[pathGroups[0].Circles.Count - 1].end == false)
                {
                    circle.turnMode = pathGroups[0].Circles[pathGroups[0].Circles.Count - 2].turnMode;
                    pathGroups[0].Circles.Add(circle);
                    pathGroups[0].Circles[pathGroups[0].Circles.Count - 2].LinkNext(pathGroups[0].Circles[pathGroups[0].Circles.Count - 1]);
                }
                // else if (pathGroups[0].Circles[pathGroups[0].Circles.Count - 1].end == true && pathGroups.Count == 2)
                // {
                //     circle.turnMode = pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].turnMode;
                //     pathGroups[0].Circles.Add(circle);
                //     pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].LinkNext(pathGroups[0].Circles[pathGroups[0].Circles.Count - 1]);
                // }
            }

        }
        else
        {
            string group_name = pathGroups[0].groupName;
            var circle = GameObject.Instantiate(PathGroupMaker.turncircle_prefab, new Vector3(ship_pos.x, 10, ship_pos.z), new Quaternion().normalized, PathGroupMaker.transform);
            circle.name = group_name + "_circle" + (pathGroups[0].Circles.Count + 1);
            circle.turnMode = TurnMode.Left;
            circle.pathGroupMaker = PathGroupMaker;
            pathGroups[0].Circles.Add(circle);
            pathGroups[0].Circles[pathGroups[0].Circles.Count - 2].LinkNext(pathGroups[0].Circles[pathGroups[0].Circles.Count - 1]);
        }

        if (early_stop == true && pathGroups[0].Circles.Count == 6)
        {
            Vector2 shipped_circle;
            if (early_stop_level == 1)
            {
                shipped_circle = new Vector2(0.0f, 52775.0f);
            }
            else
            {
                shipped_circle = new Vector2(-26387.5f, -26387.5f);
            }
            string group_name = pathGroups[0].groupName;
            var circle = GameObject.Instantiate(PathGroupMaker.turncircle_prefab, new Vector3(shipped_circle.x, 10, shipped_circle.y), new Quaternion().normalized, PathGroupMaker.transform);
            circle.name = group_name + "_circle" + (pathGroups[0].Circles.Count + 1);
            circle.turnMode = TurnMode.Left;
            circle.pathGroupMaker = PathGroupMaker;
            pathGroups[0].Circles.Add(circle);
            pathGroups[0].Circles[pathGroups[0].Circles.Count - 2].LinkNext(pathGroups[0].Circles[pathGroups[0].Circles.Count - 1]);
        }
    }

    public void ModifyPath_connect_final(Vector3 ship_pos)
    {
        List<PathGroup> pathGroups = GameObject.FindObjectOfType<PathGroupMaker>().pathGroups;
        var PathGroupMaker = GameObject.FindObjectOfType<PathGroupMaker>();
        if (pathGroups[0].Circles[pathGroups[0].Circles.Count - 1].transform.position.x != final_x &&
            pathGroups[0].Circles[pathGroups[0].Circles.Count - 1].transform.position.z != final_y)
        {
            // 移除連線到的護衛艦
            GameObject.Destroy(pathGroups[0].Circles[pathGroups[0].Circles.Count - 2].pointOut.gameObject);
            pathGroups[0].Circles[pathGroups[0].Circles.Count - 1].end = true;
            pathGroups[0].Circles[pathGroups[0].Circles.Count - 1].gameObject.active = false;
            pathGroups[0].Circles.RemoveAt(pathGroups[0].Circles.Count - 1);
        }

        string group_name = pathGroups[0].groupName;
        var circle = GameObject.Instantiate(PathGroupMaker.turncircle_prefab, new Vector3(ship_pos.x, 10, ship_pos.z), new Quaternion().normalized, PathGroupMaker.transform);
        circle.name = group_name + "_circle" + (pathGroups[0].Circles.Count + 1);
        circle.turnMode = TurnMode.Left;
        circle.pathGroupMaker = PathGroupMaker;
        pathGroups[0].Circles.Add(circle);
        pathGroups[0].Circles[pathGroups[0].Circles.Count - 2].LinkNext(pathGroups[0].Circles[pathGroups[0].Circles.Count - 1]);

    }

    public string ModifyPath_surround_ship(Vector3 ship_pos)
    {
        if (find_Ships_count != findShips.Count)
        {
            find_Ships_count = findShips.Count;

            // 飛彈當前位置
            System.Numerics.Vector3 startPos = new System.Numerics.Vector3(x: this.transform.position.x, y: this.transform.position.y, z: this.transform.position.z) / 1000.0f;
            System.Drawing.PointF startPos_point = new System.Drawing.PointF(x: startPos.X, y: startPos.Z);

            // 飛彈當前航向
            System.Numerics.Vector2 heading_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: this.transform.forward.x, y: this.transform.forward.z));

            System.Drawing.PointF second_point = new System.Drawing.PointF(x: startPos_point.X + this.transform.forward.x, y: startPos_point.Y + this.transform.forward.z);

            System.Drawing.PointF Corvette_point = new System.Drawing.PointF(x: findShips[findShips.Count - 1].pos.x / 1000.0f, y: findShips[findShips.Count - 1].pos.y / 1000.0f);

            System.Drawing.PointF Corvettes_center_gravity = new System.Drawing.PointF(0, 0);
            for (int i = 0; i < findShips.Count; i++)
            {
                Corvettes_center_gravity = new System.Drawing.PointF(Corvettes_center_gravity.X + findShips[i].pos.x / 1000.0f, Corvettes_center_gravity.Y + findShips[i].pos.y / 1000.0f);
            }
            Corvettes_center_gravity = new System.Drawing.PointF(Corvettes_center_gravity.X / findShips.Count, Corvettes_center_gravity.Y / findShips.Count);

            int Corvette_side = MathFunction.SideOfVector(startPos_point, second_point, Corvettes_center_gravity);

            System.Numerics.Vector2 missile2Corvette = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: startPos_point.X - Corvette_point.X, y: startPos_point.Y - Corvette_point.Y));
            List<System.Numerics.Vector2> normal_vec = new List<System.Numerics.Vector2>();
            int circle_numbers = 3;
            float single_angle = Mathf.PI * (360.0f / circle_numbers) / 180.0f;
            char turn_side;
            // Corvette is on the right of the missile, then turn left first
            if (Corvette_side == -1)
            {
                for (int i = circle_numbers - 1; i >= 0; i--)
                {
                    float x = missile2Corvette.X * Mathf.Cos(single_angle * i) - missile2Corvette.Y * Mathf.Sin(single_angle * i);
                    float y = missile2Corvette.X * Mathf.Sin(single_angle * i) + missile2Corvette.Y * Mathf.Cos(single_angle * i);
                    System.Numerics.Vector2 next_vec = new System.Numerics.Vector2(x, y);
                    normal_vec.Add(next_vec);
                }
                turn_side = 'R';
            }
            // Corvette is on the left of the missile, then turn right first
            else
            {
                for (int i = 0; i < circle_numbers; i++)
                {
                    float x = missile2Corvette.X * Mathf.Cos(single_angle * (i + 1)) - missile2Corvette.Y * Mathf.Sin(single_angle * (i + 1));
                    float y = missile2Corvette.X * Mathf.Sin(single_angle * (i + 1)) + missile2Corvette.Y * Mathf.Cos(single_angle * (i + 1));
                    System.Numerics.Vector2 next_vec = new System.Numerics.Vector2(x, y);
                    normal_vec.Add(next_vec);
                }
                turn_side = 'L';
            }

            List<PathGroup> pathGroups = GameObject.FindObjectOfType<PathGroupMaker>().pathGroups;
            var PathGroupMaker = GameObject.FindObjectOfType<PathGroupMaker>();

            if (pathGroups.Count == 2)
            {
                for (int i = 0; i < pathGroups[1].Circles.Count; i++)
                {
                    pathGroups[1].Circles[i].end = true;
                    pathGroups[1].Circles[i].gameObject.active = false;
                }
                pathGroups.RemoveAt(1);
            }

            for (int i = 0; i < pathGroups[0].Circles.Count; i++)
            {
                pathGroups[0].Circles[i].end = true;
                pathGroups[0].Circles[i].gameObject.active = false;
            }


            var group = new PathGroup();
            // PathGroup物件的名稱為避障路徑名稱
            group.groupName = pathGroups[0].groupName;
            pathGroups.RemoveAt(0);
            for (int i = 0; i < normal_vec.Count + 1; i++)
            {
                System.Numerics.Vector2 return_center;
                if (i != normal_vec.Count)
                {
                    return_center = new System.Numerics.Vector2(x: Corvette_point.X + 35.325f * normal_vec[i].X, y: Corvette_point.Y + 35.325f * normal_vec[i].Y) * 1000.0f;

                    if (normal_vec.Count > 1)
                    {
                        System.Numerics.Vector2 old_Corvette = new System.Numerics.Vector2(findShips[0].pos.x, findShips[0].pos.y);
                        float old_Corvette_dist = System.Numerics.Vector2.Distance(return_center, old_Corvette);
                        if (old_Corvette_dist < 35225.0f)
                        {
                            System.Drawing.PointF lineStart = new System.Drawing.PointF(x: Corvette_point.X * 1000.0f, y: Corvette_point.Y * 1000.0f);
                            System.Drawing.PointF lineEnd = new System.Drawing.PointF(x: return_center.X, y: return_center.Y);
                            System.Drawing.PointF intersection1;
                            System.Drawing.PointF intersection2;
                            int intersections = MathFunction.FindLineCircleIntersections(old_Corvette.X, old_Corvette.Y, 35325.0f, lineStart, lineEnd, out intersection1, out intersection2);
                            float new_Corvette_dist = (float)MathFunction.Distance(lineStart, intersection1);

                            return_center = (new_Corvette_dist > 35225.0f) ? new System.Numerics.Vector2(intersection1.X, intersection1.Y) : new System.Numerics.Vector2(intersection2.X, intersection2.Y);
                        }
                    }
                }
                else
                {
                    return_center = new System.Numerics.Vector2(x: Corvette_point.X, y: Corvette_point.Y) * 1000.0f;
                }

                var circle = GameObject.Instantiate(PathGroupMaker.turncircle_prefab, new Vector3(return_center.X, 10, return_center.Y), new Quaternion().normalized, PathGroupMaker.transform);
                circle.name = group.groupName + "_circle" + (i + 1);
                if (turn_side == 'R')
                    circle.turnMode = TurnMode.Right;
                else
                    circle.turnMode = TurnMode.Left;
                circle.pathGroupMaker = PathGroupMaker;
                group.Circles.Add(circle);
            }
            pathGroups.Add(group);
            PathGroupMaker.LinkPathCircles(group.groupName);

            if (turn_side == 'R')
                return "LSR";
            else
                return "RSL";
        }
        return "";
    }

    public bool CheckAvoidNeed(Vector2 ship_pos, float range)
    {
        //原直線方程式 ax+by+c = 0
        Vector2 self_2D_pos = new Vector2(this.transform.position.x, this.transform.position.z);
        Vector2 self_f = new Vector2(this.transform.forward.x, this.transform.forward.z);

        var a = self_f.y;
        var b = -1 * self_f.x;
        var c = -1 * a * self_2D_pos.x - b * self_2D_pos.y;

        var dis = Mathf.Abs(a * ship_pos.x + b * ship_pos.y + c) / Mathf.Sqrt((a * a + b * b));

        if (dis >= range)
            return false;
        else
            return true;
    }

    public void RF_Finded(ShipWork ship)
    {

        var set = false;
        var count = findShips.Count;
        Vector2 newPos = new Vector2(ship.transform.position.x, ship.transform.position.z);

        for (int i = 0; i < count; i++)
        {
            if (findShips[i].target == ship)
            {
                var data = findShips[i];

                // 如果同艘護衛艦經過200秒以上的lost time又重新匹配到，則也要避障
                if (data.lostTime > 200.0f)
                {
                    avoid_again = true;
                }

                if (!data.setdone)
                {
                    Vector2 moveVec = newPos - data.pos;

                    data.pos = newPos;
                    data.lostTime = 0;
                    data.setdone = true;
                }
                else
                {
                    float dis = Vector2.Distance(newPos, data.pos);

                    if (dis <= data.lostTime * 20.0f)
                    {

                        data.pos = newPos;
                        data.lostTime = 0;
                    }
                    else
                    {
                        Debug.Log("Not Same! dis:" + dis);
                    }
                }
                set = true;
            }
        }

        if (!set)
        {
            var newdata = new FindShip();
            newdata.guessName = (ship.shipName != "CVLL") ? "Other" : ship.shipName;
            newdata.pos = newPos;
            newdata.target = ship;
            findShips.Add(newdata);
        }

    }

    private void OnGUI()
    {
        if (text_x != null)
        {
            if (Mathf.Abs(x_value) >= 1000)
            {
                text_x.text = (this.transform.transform.position.x / 1000).ToString("0.0");
                text_x_stand.text = "km";
            }
            else
            {
                text_x.text = (this.transform.transform.position.x).ToString("0.0");
                text_x_stand.text = "m";
            }
        }
        if (text_z != null)
        {
            if (Mathf.Abs(z_value) >= 1000)
            {
                text_z.text = (this.transform.transform.position.z / 1000).ToString("0.0");
                text_z_stand.text = "km";
            }
            else
            {
                text_z.text = (this.transform.transform.position.z).ToString("0.0");
                text_z_stand.text = "m";
            }
        }
        if (text_h != null)
        {
            if (Mathf.Abs(h_value) >= 1000)
            {
                text_h.text = (this.transform.transform.position.y / 1000).ToString("0.0");
                text_h_stand.text = "km";
            }
            else
            {
                text_h.text = (this.transform.transform.position.y).ToString("0.0");
                text_h_stand.text = "m";
            }
        }
        if (text_v != null)
            text_v.text = speed.ToString("0.0");
        if (text_r != null)
            text_r.text = this.transform.eulerAngles.y.ToString("0.0");
        if (imagePoint != null)
            imagePoint.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -1 * this.transform.localEulerAngles.y);

        if (text_length != null)
            text_length.text = "飛行總路程: " + (moveLength / 1000.0f).ToString("0.0") + "Km";
    }

    // Update is called once per frame
    void Update()
    {
        if (limitFPS)
        {
            if (Application.targetFrameRate != 60)
                Application.targetFrameRate = 60;
        }
        animator.SetFloat("Up", up);
        animator.SetFloat("Right", right);

        if ((right == 0.0f) && (execute_avoid_tatic) && (!ship_wait_to_solve.Equals(new Vector3(0, 0, 0))) && predicted_CV == false && enmyTarget == null && startSimulator && avoid_static == true)
        {
            if (complicated_static == true)
            {
                SettingAvoidPath(ship_wait_to_solve);
            }
            else
            {
                SimpleAvoidPath(ship_wait_to_solve);
            }
        }
        else if (predicted_CV == true && enmyTarget == null)
        {
            var missile_pos = new System.Numerics.Vector2(this.transform.position.x, this.transform.position.z) / 1000.0f;
            float missile2CV_dist = System.Numerics.Vector2.Distance(missile_pos, predicted_CV_pos);
            float missile2FailPos_dist = System.Numerics.Vector2.Distance(missile_pos, IAPF_fail_pos);

            var CV_point = new System.Drawing.PointF(predicted_CV_pos.X, predicted_CV_pos.Y);
            var missile_point = new System.Drawing.PointF(missile_pos.X, missile_pos.Y);
            var forward_point = new System.Drawing.PointF(x: missile_point.X + this.transform.forward.x, y: missile_point.Y + this.transform.forward.z);
            var angle = MathFunction.Angle(missile_point, CV_point, forward_point);

            if (missile2CV_dist < 20.0f && angle < 25 && top_n + 1 < sp_predictions.Count && IAPF_attack_fail == false)
            {
                top_n += 1;
                (var CV_pos, var Frigate_pos) = potential_CV();
                predicted_CV_pos = new System.Numerics.Vector2(x: CV_pos.X, y: CV_pos.Y);
                predicted_Frigate_pos = new List<System.Tuple<System.Numerics.Vector2, float>>(Frigate_pos);
                this.Hit_CV(CV_pos, Frigate_pos);
            }
            else if (IAPF_attack_fail == true && missile2FailPos_dist >= 10)
            {
                this.Hit_CV(predicted_CV_pos, predicted_Frigate_pos);
            }
        }


        if (work)
        {
            work = false;
            StartSimulator();
        }
        x_value = this.transform.transform.position.x;
        z_value = this.transform.transform.position.z;
        h_value = this.transform.transform.position.y;


    }
    private void FixedUpdate()
    {
        if (!startSimulator)
            return;
        if (enmyTarget != null)
        {
            float dist = Vector3.Distance(enmyTarget.transform.position, this.transform.position);
            if (dist < 15000.0f)
            {
                right = 0.0f;
                this.transform.LookAt(new Vector3(enmyTarget.transform.position.x, 10, enmyTarget.transform.position.z));
                this.transform.Translate(0.0f, 0.0f, speed / 60.0f);
            }
            else
            {
                var nextPos = m_transform.position + m_transform.forward * speed * Time.fixedDeltaTime;
                m_rigidbody.MovePosition(nextPos);
                var nextRot = m_transform.rotation * Quaternion.Euler(-up * stand_RotateCoefficient * Time.fixedDeltaTime, right * stand_RotateCoefficient * Time.fixedDeltaTime, 0.0f);
                m_rigidbody.MoveRotation(nextRot);
            }
        }
        else
        {
            var nextPos = m_transform.position + m_transform.forward * speed * Time.fixedDeltaTime;
            m_rigidbody.MovePosition(nextPos);
            var nextRot = m_transform.rotation * Quaternion.Euler(-up * stand_RotateCoefficient * Time.fixedDeltaTime, right * stand_RotateCoefficient * Time.fixedDeltaTime, 0.0f);
            m_rigidbody.MoveRotation(nextRot);
        }
        moveLength += speed * Time.fixedDeltaTime;
    }

    public void ChangeR(float value)
    {
        if (!startSimulator)
            return;
        if (value > 45.0f)
            right = 45.0f;
        else if (value < -45.0f)
            right = -45.0f;
        else
            right = value;
    }
}


[System.Serializable]
public class FindShip
{
    public Vector2 pos = new Vector2();
    public string guessName = ""; //054A 052C 052D CVLL
    public Vector2 moveVec = new Vector2();
    public float lostTime;

    public bool setdone;

    [SerializeField]
    public ShipWork target;
}