using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DubinsPathsTutorial;
using Utils;

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

    public System.Numerics.Vector3 execute_avoid_ship;

    public bool turnStayRF = false;

    public Toggle set_turnStayRF;

    public FormationPredictor predictor = new FormationPredictor();

    public bool predicted_CV = false;

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
    }

    public void TurnStayRF(bool arg0)
    {
        turnStayRF = arg0;
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
        StartCoroutine(PathRecord());
        StartCoroutine(RFWork());
        StartCoroutine(UpdateShip());
        if (navigate)
            StartCoroutine(SimulatorNavigate());
        else
            StartCoroutine(SimulatorMoving());
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
            work_RF = true;
            predicted_CV = false;

            if (pathTrace.transform.childCount == 0)
                return;

            for (int i = pathTrace.transform.childCount - 1; i > 0; i--)
                GameObject.Destroy(pathTrace.transform.GetChild(i).gameObject);
        }
    }

    public void Broken()
    {
        if (startSimulator)
        {
            startSimulator = false;
            speed = 0;
        }
    }

    IEnumerator SimulatorMoving()
    {
        while (startSimulator)
        {
            yield return null;
            if (enmyTarget != null)
            {
                right = 0.0f;
                this.transform.LookAt(new Vector3(enmyTarget.transform.position.x, 10, enmyTarget.transform.position.z));
                this.transform.Translate(0.0f, 0.0f, speed / 60.0f * Time.timeScale);
            }
            else
            {
                this.transform.Translate(0.0f, 0.0f, speed / 60.0f * Time.timeScale);
                this.transform.Rotate(0.0f, right * stand_RotateCoefficient / 60.0f * Time.timeScale, 0.0f);
            }
            moveLength += speed / 60.0f * Time.timeScale;
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

    IEnumerator RFWork()
    {
        if ((startSimulator) && (work_RF))
        {
            model_RF.SetActive(true);

            yield return new WaitForSeconds(0.05f);
            StartCoroutine(RFRest());
        }
    }
    IEnumerator RFRest()
    {
        if (startSimulator)
        {
            model_RF.SetActive(false);
            yield return new WaitForSeconds(4.0f);
            StartCoroutine(RFWork());
        }
    }

    IEnumerator UpdateShip()
    {
        yield return new WaitForSeconds(1.0f);
        if (findShips.Count > 0)
        {
            for (int i = 0; i < findShips.Count; i++)
            {
                findShips[i].lostTime += 1.0f;
            }
        }
        if (startSimulator)
            StartCoroutine(UpdateShip());
    }

    public void RF_WORK()
    {
        work_RF = true;
        StartCoroutine(RFWork());
    }

    public void SettingAvoidPath()
    {
        List<System.Numerics.Vector3> DetectedShips = new List<System.Numerics.Vector3>();
        System.Numerics.Vector3 detected_ship;
        for (int i = 0; i < findShips.Count; i++)
        {
            System.Numerics.Vector3 ship_position = new System.Numerics.Vector3(x: findShips[i].pos.x + findShips[i].lostTime * findShips[i].moveVec.x,
                                                                                y: 0.0f,
                                                                                z: findShips[i].pos.y + findShips[i].lostTime * findShips[i].moveVec.y) / 1000.0f;
            // if (findShips[i].lostTime == 0)
            // {
            //     detected_ship = ship_position;
            // }
            DetectedShips.Add(ship_position);
        }

        if (this.right != 0)
        {
            execute_avoid_tatic = true;
        }
        else if (this.right == 0)
        {
            
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

            // List<System.Numerics.Vector3> DetectedShips = new List<System.Numerics.Vector3>();
            // for (int i = 0; i < this.findShips.Count; i++)
            // {
            //     System.Numerics.Vector3 ship_position = new System.Numerics.Vector3(x: this.findShips[i].pos.x + this.findShips[i].lostTime * this.findShips[i].moveVec.x,
            //                                                                         y: 0.0f,
            //                                                                         z: this.findShips[i].pos.y + this.findShips[i].lostTime * this.findShips[i].moveVec.y) / 1000.0f;
            //     DetectedShips.Add(ship_position);
            // }

            // List<ShipPermutation> sp_candidates, sp_predictions;
            // if (DetectedShips.Count == 3)
            // {
            //     double course = -(180 * Mathf.Atan2(this.findShips[0].moveVec.y, this.findShips[0].moveVec.x) / Mathf.PI - 90);
            //     course = -course - 90;
            //     List<Ship> ships = new List<Ship>{
            //         new Ship(DetectedShips[0].X, DetectedShips[0].Z),
            //         new Ship(DetectedShips[1].X, DetectedShips[1].Z),
            //         new Ship(DetectedShips[2].X, DetectedShips[2].Z),
            //     };
            //     (sp_candidates, sp_predictions) = this.predictor.predict(new ShipPermutation(mode: "inference", ships: ships), current_course: course);

            //     Debug.Log($"Type={sp_predictions[0].formation}, CV=({sp_predictions[0].ship_position_predict["CVLL"].x}, {sp_predictions[0].ship_position_predict["CVLL"].y})");
            // }

            // avoid_path是從新的迴轉圓開始，所以要將當前迴轉圓insert到第0的位置
            (List<System.Tuple<MathFunction.Circle, char>> avoid_path, int push_circle_Index) = GeneratePath.GeneratePathFunc(startPos, startHeading, DetectedShips, InitialDiamondCircle);

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
                            break;
                        }

                        if (ori_center_second_center_dist > ori_center_new_center_dist)
                        {
                            avoid_path.Insert(1, new System.Tuple<MathFunction.Circle, char>(new_avoid_center, avoid_path[1].Item2));
                            avoid_path.RemoveAt(2);
                            break;
                        }
                        else
                        {
                            break;

                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            // for (int i = 1; i < avoid_path.Count - 1; i++)
            // {
            //     if (MathFunction.Distance(avoid_path[0].Item1.center, avoid_path[2].Item1.center) < 7.225f * 2)
            //     {
            //         avoid_path.RemoveAt(1);
            //     }
            //     else
            //     {
            //         if (MathFunction.Distance(avoid_path[0].Item1.center, avoid_path[1].Item1.center) >= 7.225f * 2)
            //         {
            //             break;
            //         }
            //         else
            //         {
            //         System.Numerics.Vector2 circle1_circle2_vec = System.Numerics.Vector2.Normalize(
            //                                                         new System.Numerics.Vector2(x: avoid_path[1].Item1.center.X - avoid_path[0].Item1.center.X,
            //                                                                                     y: avoid_path[1].Item1.center.Y - avoid_path[0].Item1.center.Y));

            //         System.Drawing.PointF new_center = new System.Drawing.PointF(x: avoid_path[0].Item1.center.X + 15f * circle1_circle2_vec.X,
            //                                                                     y: avoid_path[0].Item1.center.Y + 15f * circle1_circle2_vec.Y);

            //         MathFunction.Circle new_avoid_circle = new MathFunction.Circle(new_center, stand_HalfLength / 1000.0f);

            //         avoid_path.Insert(2, new System.Tuple<MathFunction.Circle, char>(new_avoid_circle, avoid_path[1].Item2));
            //         avoid_path.RemoveAt(1);
            //         break;

            //         }
            //     }

            // }


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

    public (List<ShipPermutation>, List<ShipPermutation>) Predict_CV(Vector2 Ship_move_vec)
    {
        List<System.Numerics.Vector3> DetectedShips = new List<System.Numerics.Vector3>();
        for (int i = 0; i < findShips.Count; i++)
        {
            System.Numerics.Vector3 ship_position = new System.Numerics.Vector3(x: findShips[i].pos.x + findShips[i].lostTime * findShips[i].moveVec.x,
                                                                                y: 0.0f,
                                                                                z: findShips[i].pos.y + findShips[i].lostTime * findShips[i].moveVec.y) / 1000.0f;
            DetectedShips.Add(ship_position);
        }
        List<ShipPermutation> sp_candidates, sp_predictions;

        double course = -(180 * Mathf.Atan2(Ship_move_vec.y, Ship_move_vec.x) / Mathf.PI - 90);
        course = -course - 90;
        List<Ship> ships = new List<Ship>{
            new Ship(DetectedShips[0].X, DetectedShips[0].Z),
            new Ship(DetectedShips[1].X, DetectedShips[1].Z),
            new Ship(DetectedShips[2].X, DetectedShips[2].Z),
        };
        (sp_candidates, sp_predictions) = this.predictor.predict(new ShipPermutation(mode: "inference", ships: ships), current_course: course);

        return (sp_candidates, sp_predictions);
    }


    public void SettingHitPath(System.Numerics.Vector2 CV_pos)
    {
        // 飛彈當前位置
        System.Numerics.Vector3 startPos = new System.Numerics.Vector3(x: this.transform.position.x, y: this.transform.position.y, z: this.transform.position.z) / 1000.0f;
        System.Drawing.PointF startPos_point = new System.Drawing.PointF(x: startPos.X, y: startPos.Z);

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
        MathFunction.Circle return_circle = new MathFunction.Circle(center: return_center, radius: 7.225f);

        MathFunction.Circle target_circle = new MathFunction.Circle(center: CV_point, radius: 0.1f);

        MathFunction.Line out_tangent_line_1, out_tangent_line_2;
        (out_tangent_line_1, out_tangent_line_2) = MathFunction.CalculateForDifferentRadius(return_circle, target_circle);

        int choose_line = MathFunction.SideOfVector(startPos_point, out_tangent_line_1.PointA, out_tangent_line_1.PointB);

        Vector3 waypoint_1, waypoint_2;
        if (choose_line == CV_side)
        {
            waypoint_1 = new Vector3(x: out_tangent_line_1.PointA.X, y: 0.0f, z: out_tangent_line_1.PointA.Y);
            waypoint_2 = new Vector3(x: out_tangent_line_1.PointB.X, y: 0.0f, z: out_tangent_line_1.PointB.Y);
        }
        else
        {
            waypoint_1 = new Vector3(x: out_tangent_line_2.PointA.X, y: 0.0f, z: out_tangent_line_2.PointA.Y);
            waypoint_2 = new Vector3(x: out_tangent_line_2.PointB.X, y: 0.0f, z: out_tangent_line_2.PointB.Y);
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

        // 將避障路徑物件丟到PathGroupMaker中的SettingPathGroup
        GameObject.FindObjectOfType<PathGroupMaker>().SettingPathGroup(avoidPath);

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
        Vector2 find_pos = new Vector2(ship.transform.position.x, ship.transform.position.z);

        var newData = true;
        for (int i = 0; i < findShips.Count; i++)
        {
            /*
            if (findShips[i].first)
            {
                var dis = Vector2.Distance(find_pos, findShips[i].pos);
                if (dis < 100)
                {
                    findShips[i].first = false;
                    findShips[i].moveVec = (find_pos - findShips[i].pos) / 4;
                    findShips[i].pos = find_pos;
                    newData = false;
                    findShips[i].lostTime = 0.0f;
                    i = findShips.Count;
                }
            }
            else
            {*/
            var dis = Vector2.Distance(find_pos, findShips[i].pos + (findShips[i].lostTime * findShips[i].moveVec));
            var vecLen = Vector2.Distance(new Vector2(0, 0), findShips[i].moveVec);

            if (dis <= (vecLen * 1.5))
            {
                findShips[i].pos = find_pos;
                findShips[i].lostTime = 0.0f;
                newData = false;
                i = findShips.Count;
            }
            // }
        }

        if (newData)
        {
            if (name != "")
            {
                var newShip = new FindShip();
                newShip.guessName = name;
                newShip.pos = find_pos;
                newShip.moveVec = new Vector2(ship.transform.forward.x, ship.transform.forward.z) * ship.shipSpeed * 0.5144f;
                newShip.lostTime = 0.0f;
                findShips.Add(newShip);
            }
            else
            {
                var newShip = new FindShip();
                newShip.guessName = "052C";
                newShip.pos = find_pos;
                newShip.moveVec = new Vector2(ship.transform.forward.x, ship.transform.forward.z) * ship.shipSpeed * 0.5144f;
                newShip.lostTime = 0.0f;
                findShips.Add(newShip);
            }
        }

        for (int i = 0; i < findShips.Count - 1; i++)
        {
            if (Vector2.Equals(findShips[i].moveVec, new Vector2(0, 0)) && findShips[i].lostTime != 0)
            {
                findShips.RemoveAt(i);
                break;
            }
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
        // if ((right == 0.0f) && (execute_avoid_tatic) && startSimulator)
        //     SettingAvoidPath();

        if (work)
        {
            work = false;
            StartSimulator();
        }
        x_value = this.transform.transform.position.x;
        z_value = this.transform.transform.position.z;
        h_value = this.transform.transform.position.y;


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
}