using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DubinsPathsTutorial;

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

    public bool turnStayRF = false;

    public Toggle set_turnStayRF;

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

    public void SettingAvoidPath(Vector3 ship_pos, float range)
    {
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

        #region #Old Work
        /*
        if (CheckAvoidNeed(tar_2D_pos, range))
        {
            Vector2 vec_ST = tar_2D_pos - self_2D_pos;
               
            var f = self_f.x * vec_ST.y - vec_ST.x * self_f.y;

            Vector2 vec_n = new Vector2();

            if (f > 0) //右轉
                vec_n = new Vector2(self_f.y, -self_f.x);
            else       //左轉
                vec_n = new Vector2(-self_f.y, self_f.x);

            // 新增避障路徑物件，以及命名該避障路徑名稱
            var avoidPath = new PathSetting();
            avoidPath.name = "avoidPath1";

            var C1 = new CircleData();
            C1.position = self_2D_pos + (vec_n * self_r);               
            if (f > 0) //右轉
                C1.turnMode = TurnMode.Right;
            else
                C1.turnMode = TurnMode.Left;

            var C2 = new CircleData();
            C2.position = tar_2D_pos + (vec_n * avoid_R);
            if (f > 0) //右轉
                C2.turnMode = TurnMode.Left;
            else
                C2.turnMode = TurnMode.Right;

            var C3 = new CircleData();
            var A = (self_f.x * self_f.x + self_f.y * self_f.y);
            var B = (tar_2D_pos.x * self_f.x + tar_2D_pos.y * self_f.y - self_2D_pos.x * self_f.x - self_2D_pos.y * self_f.y);
           
            var a = B/A;

            C3.position = C1.position + 2 *a* self_f;

            if (f > 0) //右轉                  
                C3.turnMode = TurnMode.Right;
            else
                C3.turnMode = TurnMode.Left;

            // 將生成的避障圓資訊依序新增到避障路徑物件內
            avoidPath.circleDatas.Add(C1);
            avoidPath.circleDatas.Add(C2);
            avoidPath.circleDatas.Add(C3);

            // 將避障路徑物件丟到PathGroupMaker中的SettingPathGroup
            GameObject.FindObjectOfType<PathGroupMaker>().SettingPathGroup(avoidPath);
        }
        else
        {
            Debug.Log("Not need to avoid!");
        }
        */
        #endregion

        #region #New tradegy
        List<PathGroup> pathGroups = GameObject.FindObjectOfType<PathGroupMaker>().pathGroups;
        List<System.Tuple<System.Numerics.Vector3, char>> InitialDiamondCircle = new List<System.Tuple<System.Numerics.Vector3, char>>();

        if (pathGroups.Count > 1)
        {
            for (int i = 0; i < pathGroups[1].Circles.Count; i++)
            {
                if (i == pathGroups[1].Circles.Count - 1 && pathGroups[1].Circles[i].end != true)
                {
                    char turn_side = pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].turnMode.ToString()[0];
                    System.Tuple<System.Numerics.Vector3, char> inital_circle = new System.Tuple<System.Numerics.Vector3, char>(
                                                                                new System.Numerics.Vector3(x: pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].transform.position.x,
                                                                                                            y: pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].transform.position.y,
                                                                                                            z: pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].transform.position.z) / 1000.0f,
                                                                                turn_side);
                    InitialDiamondCircle.Add(inital_circle);

                }
                else
                {
                    pathGroups[1].Circles[i].end = true;
                    pathGroups[1].Circles[i].gameObject.active = false;
                }
            }
        }

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

        List<System.Numerics.Vector3> DetectedShips = new List<System.Numerics.Vector3>();
        for (int i = 0; i < this.findShips.Count; i++)
        {
            System.Numerics.Vector3 ship_position = new System.Numerics.Vector3(x: this.findShips[i].pos.x, y: 0.0f, z: this.findShips[i].pos.y) / 1000.0f;
            DetectedShips.Add(ship_position);
        }

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

        avoid_path.Insert(0, new System.Tuple<MathFunction.Circle, char>(first_avoid_circle, avoid_path[0].Item2));


        avoid_path.RemoveAt(1);

        for (int i = 1; i < avoid_path.Count - 1; i++)
        {
            if (MathFunction.Distance(avoid_path[0].Item1.center, avoid_path[2].Item1.center) < 7.225f * 2)
            {
                avoid_path.RemoveAt(1);
            }
            else
            {
                if (MathFunction.Distance(avoid_path[0].Item1.center, avoid_path[1].Item1.center) >= 7.225f * 2)
                {
                    break;
                }
                else
                {
                System.Numerics.Vector2 circle1_circle2_vec = System.Numerics.Vector2.Normalize(
                                                                new System.Numerics.Vector2(x: avoid_path[1].Item1.center.X - avoid_path[0].Item1.center.X,
                                                                                            y: avoid_path[1].Item1.center.Y - avoid_path[0].Item1.center.Y));

                System.Drawing.PointF new_center = new System.Drawing.PointF(x: avoid_path[0].Item1.center.X + 15f * circle1_circle2_vec.X,
                                                                            y: avoid_path[0].Item1.center.Y + 15f * circle1_circle2_vec.Y);

                MathFunction.Circle new_avoid_circle = new MathFunction.Circle(new_center, stand_HalfLength / 1000.0f);

                avoid_path.Insert(2, new System.Tuple<MathFunction.Circle, char>(new_avoid_circle, avoid_path[1].Item2));
                avoid_path.RemoveAt(1);
                break;

                }
            }

        }


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

    public void RF_Finded(string name, Vector2 find_pos)
    {
        var newData = true;
        for (int i = 0; i < findShips.Count; i++)
        {
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
            {
                var dis = Vector2.Distance(find_pos, findShips[i].pos + (findShips[i].lostTime * findShips[i].moveVec));
                var vecLen = Vector2.Distance(new Vector2(0, 0), findShips[i].moveVec);

                if (dis <= (vecLen * 1.5))
                {
                    findShips[i].pos = find_pos;
                    findShips[i].lostTime = 0.0f;
                    newData = false;
                    i = findShips.Count;
                }
            }
        }

        if (newData)
        {
            if (name != "")
            {
                var newShip = new FindShip();
                newShip.guessName = name;
                newShip.first = true;
                newShip.pos = find_pos;
                newShip.lostTime = 0.0f;
                findShips.Add(newShip);
            }
            else
            {
                var newShip = new FindShip();
                newShip.guessName = "052C";
                newShip.first = true;
                newShip.pos = find_pos;
                newShip.lostTime = 0.0f;
                findShips.Add(newShip);
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
    public bool first = true;
}