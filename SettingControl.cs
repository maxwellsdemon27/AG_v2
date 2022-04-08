using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using System.IO;

public class SettingControl : MonoBehaviour
{
    public Controller target;

    public InputField input_X;
    public InputField input_Z;
    public InputField input_H;
    public InputField input_R;
    public InputField input_V;

    public InputField input_Roll;
    public InputField input_Pitch;
    public Slider slider_Roll;
    public Slider slider_Pitch;

    public Text text_3rdView;
    public Toggle toggle_3rdView;
    public Camera camera_3rd;

    public string activeMode = "";

    public Dropdown dropdown_Mode;
    public Button button_Import;
    public Button button_Setting;

    public Image R_Image;

    private float set_X_value = 0.0f;
    private float set_Z_value = 0.0f;
    private float set_H_value = 0.0f;
    private float set_R_value = 0.0f;
    private float set_V_value = 0.0f;
    private float set_Roll_value = 0.0f;
    private float set_Pitch_value = 0.0f;

    #region #inport excel
    private string _path;
    public Vector3[] positionList;
    public LineRenderer lineRenderer;
    #endregion

    #region #指定模擬
    public StanderPath standerPath = StanderPath.Start_W;
    public LineRenderer path_Ori;
    public PathMaker pathMaker;
    private Vector3[] Path_W = new Vector3[22];
    private Vector3[] Path_S = new Vector3[22];
    private Vector3[] Path_E = new Vector3[22];
    private Vector3[] Path_N = new Vector3[22];

    private Vector3[] SPath_W = new Vector3[20];
    private Vector3[] SPath_S = new Vector3[20];
    private Vector3[] SPath_E = new Vector3[20];
    private Vector3[] SPath_N = new Vector3[20];

    public PathGroupMaker pathGroupMaker;
    public int now_path = 0;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (target == null)
            return;
        else
            target.setting = this;

        if (input_X != null)
        {
            Changed_X((target.transform.position.x / 1000).ToString());
            input_X.onEndEdit.AddListener(Changed_X);
        }
        if (input_Z != null)
        {
            Changed_Z((target.transform.position.z / 1000).ToString());
            input_Z.onEndEdit.AddListener(Changed_Z);
        }
        if (input_H != null)
        {
            Changed_H(target.transform.position.y.ToString());
            input_H.onEndEdit.AddListener(Changed_H);
        }
        if (input_R != null)
        {
            Changed_R(target.transform.eulerAngles.y.ToString());
            input_R.onEndEdit.AddListener(Changed_R);
        }
        if (input_V != null)
        {          
            Changed_V(target.speed.ToString());
            input_V.onEndEdit.AddListener(Changed_V);
        }
        if(input_Roll != null)
        {           
            Changed_Roll(target.right);
            slider_Roll.onValueChanged.AddListener(Changed_Roll);
            input_Roll.onEndEdit.AddListener(Changed_Roll);
        }       
        if(input_Pitch != null)
        {
            Changed_Pitch(target.up);
            slider_Pitch.onValueChanged.AddListener(Changed_Pitch);
            input_Pitch.onEndEdit.AddListener(Changed_Pitch);
        }
        if (toggle_3rdView != null)
        {
            Camera3rd_Act(toggle_3rdView.isOn);
            toggle_3rdView.onValueChanged.AddListener(Camera3rd_Act);
        }
       
        #region #Path_W
        Path_W[0] = new Vector3(-100000.0f, 0, 0);
        Path_W[1] = new Vector3(32775.0f, 0, 0);
        Path_W[2] = new Vector3(35539.88f, 0, 549.98f);
        Path_W[3] = new Vector3(37883.84f, 0, 2116.16f);
        Path_W[4] = new Vector3(39450.02f, 0, 4460.12f);
        Path_W[5] = new Vector3(40000.0f, 0, 7225.0f);
        Path_W[6] = new Vector3(40000.0f, 0, 32775.0f);
        Path_W[7] = new Vector3(39450.02f, 0, 35539.88f);
        Path_W[8] = new Vector3(37883.84f, 0, 37883.84f);
        Path_W[9] = new Vector3(35539.88f, 0, 39450.02f);
        Path_W[10] = new Vector3(32775.0f, 0, 40000.0f);
        Path_W[11] = new Vector3(-32775.0f, 0, 40000.0f);
        Path_W[12] = new Vector3(-35539.88f, 0, 39450.02f);
        Path_W[13] = new Vector3(-37883.84f, 0, 37883.84f);
        Path_W[14] = new Vector3(-39450.02f, 0, 35539.88f);
        Path_W[15] = new Vector3(-40000.0f, 0, 32775.0f);
        Path_W[16] = new Vector3(-40000.0f, 0, -32775.0f);
        Path_W[17] = new Vector3(-39450.02f, 0, -35539.88f);
        Path_W[18] = new Vector3(-37883.84f, 0, -37883.84f);
        Path_W[19] = new Vector3(-35539.88f, 0, -39450.02f);
        Path_W[20] = new Vector3(-32775.0f, 0, -40000.0f);
        Path_W[21] = new Vector3(60000, 0, -40000.0f);
        #endregion

        #region #Path_S
        for (int i = 0; i < Path_S.Length; i++)
            Path_S[i] = new Vector3(-1 * Path_W[i].z, 0, Path_W[i].x);
        #endregion

        #region #Path_E
        for (int i = 0; i < Path_E.Length; i++)
            Path_E[i] = new Vector3(-1 * Path_W[i].x, 0, -1 * Path_W[i].z);
        #endregion

        #region #Path_N
        for (int i = 0; i < Path_N.Length; i++)
            Path_N[i] = new Vector3(Path_W[i].z, 0, -1 * Path_W[i].x);
        #endregion

        #region #SPath_W
        SPath_W[0] = new Vector3(-100000.0f, 0, 0);
        SPath_W[1] = new Vector3(-62992.69f, 0, 0);
        SPath_W[2] = new Vector3(-57883.85f, 0, -2116.15f);
        SPath_W[3] = new Vector3(-5108.84f, 0, -54891.16f);
        SPath_W[4] = new Vector3(-2764.89f, 0, -56457.34f);
        SPath_W[5] = new Vector3(0, 0, -57007.31f);
        SPath_W[6] = new Vector3(2764.89f, 0, -56457.34f);
        SPath_W[7] = new Vector3(5108.84f, 0, -54891.16f);
        SPath_W[8] = new Vector3(54891.16f, 0, -5108.84f);
        SPath_W[9] = new Vector3(56457.34f, 0, -2764.89f);
        SPath_W[10] = new Vector3(57007.31f, 0, 0);
        SPath_W[11] = new Vector3(56457.34f, 0, 2764.89f);
        SPath_W[12] = new Vector3(54891.16f, 0, 5108.84f);
        SPath_W[13] = new Vector3(5108.84f, 0, 54891.16f);
        SPath_W[14] = new Vector3(2764.89f, 0, 56457.34f);
        SPath_W[15] = new Vector3(0, 0, 57007.31f);
        SPath_W[16] = new Vector3(-2764.89f, 0, 56457.34f);
        SPath_W[17] = new Vector3(-5108.84f, 0, 54891.16f);
        SPath_W[18] = new Vector3(-54891.16f, 0, 5108.84f);
        SPath_W[19] = new Vector3(-60000.0f, 0, 0);
        #endregion

        #region #SPath_S
        for (int i = 0; i < SPath_S.Length; i++)
            SPath_S[i] = new Vector3(-1 * SPath_W[i].z, 0, SPath_W[i].x);
        #endregion

        #region #SPath_E
        for (int i = 0; i < SPath_E.Length; i++)
            SPath_E[i] = new Vector3(-1 * SPath_W[i].x, 0, -1 * SPath_W[i].z);
        #endregion

        #region #SPath_N
        for (int i = 0; i < SPath_N.Length; i++)
            SPath_N[i] = new Vector3(SPath_W[i].z, 0, -1 * SPath_W[i].x);
        #endregion
      
        if (dropdown_Mode != null)
        {
            ModeSelect(dropdown_Mode.value);
            dropdown_Mode.onValueChanged.AddListener(ModeSelect);
        }
    }

    private void Changed_X(string arg0)
    {
        if (arg0 == "-") 
            arg0 = "-1";
        if (Single.Parse(arg0) > 100)
        {
            input_X.text = "100.0";
            set_X_value = 100000;
        }
        else if (Single.Parse(arg0) < -100)
        {
            input_X.text = "-100.0";
            set_X_value = -100000;
        }
        else
        {
            input_X.text = arg0;
            set_X_value = Single.Parse(arg0) * 1000;
        }
        target.transform.position = new Vector3(set_X_value, target.transform.position.y, target.transform.position.z);
    }
    private void Changed_Z(string arg0)
    {
        if (arg0 == "-")
            arg0 = "-1";
        if (Single.Parse(arg0)  > 100)
        {
            input_Z.text = "100.0";
            set_Z_value = 100000;
        }
        else if (Single.Parse(arg0)  < -100)
        {
            input_Z.text = "-100.0";
            set_Z_value = -100000;
        }
        else
        {
            input_Z.text = arg0;
            set_Z_value = Single.Parse(arg0) * 1000;
        }
        target.transform.position = new Vector3(target.transform.position.x, target.transform.position.y, set_Z_value);
    }
    private void Changed_H(string arg0)
    {
        if (arg0 == "-")
            arg0 = "-1";
        input_H.text = arg0;
        set_H_value = Single.Parse(arg0) ;
        target.transform.position = new Vector3(target.transform.position.x, set_H_value, target.transform.position.z);
    }
    private void Changed_R(string arg0)
    {
        if (arg0 == "-")
            arg0 = "-1";
        target.transform.localEulerAngles = new Vector3(target.transform.localEulerAngles.x, Single.Parse(arg0), target.transform.localEulerAngles.z);
        set_R_value = target.transform.localEulerAngles.y;
        input_R.text = set_R_value.ToString("0.0");
        R_Image.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -1 * set_R_value);
    }
    private void Changed_V(string arg0)
    {
        if (arg0 == "-")
            arg0 = "-1";
        input_V.text = arg0;
        set_V_value = Single.Parse(arg0);
        target.speed = set_V_value;
    }
    private void Changed_Roll(string arg0)
    {
        if (arg0 == "-")
            arg0 = "-1";
        Changed_Roll(Single.Parse(arg0));
    }
    private void Changed_Pitch(string arg0)
    {
        if (arg0 == "-")
            arg0 = "-1";
        Changed_Pitch(Single.Parse(arg0));       
    }
    private void Changed_Roll(float arg0)
    {
        slider_Roll.value = arg0;
        set_Roll_value = slider_Roll.value;
        target.right = set_Roll_value;
        input_Roll.text = set_Roll_value.ToString("0.0");
    }
    private void Changed_Pitch(float arg0)
    {
        slider_Pitch.value = arg0;
        set_Pitch_value = slider_Pitch.value;
        target.up = set_Pitch_value;
        input_Pitch.text = set_Pitch_value.ToString("0.0");
    }
    private void Camera3rd_Act(bool arg0)
    {
        if (arg0)
        {
            text_3rdView.text = "第三人稱視角：啟用";
            camera_3rd.enabled = true;
        }
        else
        {
            text_3rdView.text = "第三人稱視角：關閉";
            camera_3rd.enabled = false;
        }
    }
    private void ModeSelect(int arg0)
    {
        activeMode = dropdown_Mode.options[arg0].text;

        if (activeMode == "導控模擬")
        {
            button_Import.gameObject.SetActive(true);
            button_Setting.gameObject.SetActive(false);
            target.navigate = true;

            path_Ori.enabled = false;

        }
        else if (activeMode == "指定模擬")
        {
            button_Import.gameObject.SetActive(false);
            button_Setting.gameObject.SetActive(true);
            target.navigate = false;

            if (pathGroupMaker == null)
            {
                path_Ori.enabled = true;

                standerPath = StanderPath.Start_W;
                Changed_X("-100");
                Changed_Z("0");
                Changed_H("10");
                Changed_R("90.0");
                Changed_V("238.0");
                Changed_Roll(0.0f);
                Changed_Pitch(0.0f);
                path_Ori.SetPositions(Path_W);
                pathMaker.TrySetStandardPath(standerPath);
            }
            else
            {
                pathGroupMaker.ResetAll();
                Changed_X((pathGroupMaker.pathSettings[now_path].start_Pos.x / 1000).ToString());
                Changed_Z((pathGroupMaker.pathSettings[now_path].start_Pos.y / 1000).ToString());
                Changed_R(pathGroupMaker.pathSettings[now_path].start_R.ToString());
                Changed_H("10");
                Changed_V("238.0");
                Changed_Roll(0.0f);
                Changed_Pitch(0.0f);
                pathGroupMaker.SettingPathGroup(now_path);
            }
        }
        else //自由模擬
        {
            button_Import.gameObject.SetActive(false);
            button_Setting.gameObject.SetActive(false);
            target.navigate = false;

            path_Ori.enabled = false;
        }
    }



    public void Reset()
    {
        target.transform.position = new Vector3(set_X_value, set_H_value, set_Z_value);
        target.transform.localEulerAngles = new Vector3(0.0f, set_R_value, 0.0f);
        
        target.speed = set_V_value;
        target.right = set_Roll_value;
        target.up = set_Pitch_value;

        pathGroupMaker.ResetAll();
        pathGroupMaker.SettingPathGroup(now_path);
    }

    public void OpenExcel()
    {
        WriteResult(StandaloneFileBrowser.OpenFilePanel("載入Excel資料檔", "", "", false));
        ReadExcel(_path);
    }
    private void WriteResult(string[] paths)
    {
        if (paths.Length == 0)
            return;
        _path = "";
        foreach (var p in paths)
            _path += p;
    }
    private void ReadExcel(string filepath)
    {
        var fileType = Path.GetExtension(filepath);
        if (fileType == ".csv")
        {
            string fileName = Path.GetFileName(filepath);
            var stringArray = fileName.Split("_"[0]);
            if (stringArray[0] == "Route")
            {
                if (positionList.Length > 0)
                    positionList = new Vector3[0];
                string[] fileData = File.ReadAllLines(filepath);
                positionList = new Vector3[fileData.Length - 1];
                for (int i = 1; i < fileData.Length; i++)
                {
                    string[] inputValue = fileData[i].Split(',');
                    var inputPosition = new Vector3 { x = float.Parse(inputValue[2]) * 1f, y = 10.0f, z = float.Parse(inputValue[1]) };
                    positionList[i-1] = inputPosition;                   
                }
                lineRenderer.positionCount = positionList.Length;
                lineRenderer.SetPositions(positionList);
                input_X.text = (positionList[0].x /1000).ToString("0.0");
                input_Z.text = (positionList[0].z /1000).ToString("0.0");
                Changed_X(input_X.text);
                Changed_Z(input_Z.text);
                //target.navgateRoute =  new Vector3[positionList.Length];
                target.navigateRoute = positionList;
                target.transform.LookAt(positionList[1]);
                input_R.text = target.transform.localEulerAngles.y.ToString("0.0");
                Changed_R(input_R.text);
                slider_Roll.value = 0.0f;
                Changed_Roll(slider_Roll.value);
            }
        }
        else
            Debug.Log("系統不支援此檔案類型!");
    }

    public void AutoSetting()
    {
        if(pathGroupMaker == null)
        {
            switch (standerPath)
            {
                case StanderPath.Start_W:
                    standerPath = StanderPath.Start_S;
                    Changed_X("0");
                    Changed_Z("-100");
                    Changed_H("10");
                    Changed_R("0.0");
                    Changed_V("238.0");
                    Changed_Roll(0.0f);
                    Changed_Pitch(0.0f);
                    path_Ori.positionCount = Path_S.Length;
                    path_Ori.SetPositions(Path_S);
                    break;
                case StanderPath.Start_S:
                    standerPath = StanderPath.Start_E;
                    Changed_X("100");
                    Changed_Z("0");
                    Changed_H("10");
                    Changed_R("-90.0");
                    Changed_V("238.0");
                    Changed_Roll(0.0f);
                    Changed_Pitch(0.0f);
                    path_Ori.positionCount = Path_E.Length;
                    path_Ori.SetPositions(Path_E);
                    break;
                case StanderPath.Start_E:
                    standerPath = StanderPath.Start_N;
                    Changed_X("0");
                    Changed_Z("100");
                    Changed_H("10");
                    Changed_R("180.0");
                    Changed_V("238.0");
                    Changed_Roll(0.0f);
                    Changed_Pitch(0.0f);
                    path_Ori.positionCount = Path_N.Length;
                    path_Ori.SetPositions(Path_N);
                    break;
                case StanderPath.Start_N:
                    standerPath = StanderPath.Start_SW;
                    Changed_X("-100");
                    Changed_Z("0");
                    Changed_H("10");
                    Changed_R("90.0");
                    Changed_V("238.0");
                    Changed_Roll(0.0f);
                    Changed_Pitch(0.0f);
                    path_Ori.positionCount = SPath_W.Length;
                    path_Ori.SetPositions(SPath_W);
                    break;
                case StanderPath.Start_SW:
                    standerPath = StanderPath.Start_SS;
                    Changed_X("0");
                    Changed_Z("-100");
                    Changed_H("10");
                    Changed_R("0.0");
                    Changed_V("238.0");
                    Changed_Roll(0.0f);
                    Changed_Pitch(0.0f);
                    path_Ori.positionCount = SPath_S.Length;
                    path_Ori.SetPositions(SPath_S);
                    break;
                case StanderPath.Start_SS:
                    standerPath = StanderPath.Start_SE;
                    Changed_X("100");
                    Changed_Z("0");
                    Changed_H("10");
                    Changed_R("-90.0");
                    Changed_V("238.0");
                    Changed_Roll(0.0f);
                    Changed_Pitch(0.0f);
                    path_Ori.positionCount = SPath_E.Length;
                    path_Ori.SetPositions(SPath_E);
                    break;
                case StanderPath.Start_SE:
                    standerPath = StanderPath.Start_SN;
                    Changed_X("0");
                    Changed_Z("100");
                    Changed_H("10");
                    Changed_R("180.0");
                    Changed_V("238.0");
                    Changed_Roll(0.0f);
                    Changed_Pitch(0.0f);
                    path_Ori.positionCount = SPath_N.Length;
                    path_Ori.SetPositions(SPath_N);
                    break;
                case StanderPath.Start_SN:
                    standerPath = StanderPath.Start_W;
                    Changed_X("-100");
                    Changed_Z("0");
                    Changed_H("10");
                    Changed_R("90.0");
                    Changed_V("238.0");
                    Changed_Roll(0.0f);
                    Changed_Pitch(0.0f);
                    path_Ori.positionCount = Path_W.Length;
                    path_Ori.SetPositions(Path_W);
                    break;
            }
            pathMaker.TrySetStandardPath(standerPath);
        }
        else
        {
            pathGroupMaker.ResetAll();

            now_path++;
            if (now_path >= pathGroupMaker.pathSettings.Count)
                now_path = 0;

            Changed_X((pathGroupMaker.pathSettings[now_path].start_Pos.x / 1000).ToString());
            Changed_Z((pathGroupMaker.pathSettings[now_path].start_Pos.y / 1000).ToString());
            Changed_R(pathGroupMaker.pathSettings[now_path].start_R.ToString());
            Changed_H("10");
            Changed_V("238.0");
            Changed_Roll(0.0f);
            Changed_Pitch(0.0f);

            pathGroupMaker.SettingPathGroup(now_path);
        }    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum StanderPath
{
    Start_W,
    Start_S,
    Start_E,
    Start_N,
    Start_SW,
    Start_SS,
    Start_SE,
    Start_SN
}