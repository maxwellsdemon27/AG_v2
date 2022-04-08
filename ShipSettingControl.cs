using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using SFB;
using System.IO;

public class ShipSettingControl : MonoBehaviour
{
    public Transform shipList;
    public ShipNode shipNode;

    public Dropdown dropdownList;
    public int pointer = -1;

    public List<ShipWork> shipData = new List<ShipWork>();

    public List<string> selects = new List<string>();

    public List<ShipNode> ships = new List<ShipNode>();

    public InputField input_X;
    public InputField input_Z;
    public InputField input_R;
    public InputField input_V;
    public Image R_Image;

    public Button same_R;
    public Button button_Delete;
    public Button button_Known;

    private int maxship = 12;

    private int teamType = -1;
    public Animator animator_Answer;

    #region #inport excel
    private string _path;
    private float cv_R = 0.0f;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        pointer = -1;
        if (dropdownList != null)
        {
            dropdownList.onValueChanged.AddListener(Selected);
        }
        if (input_X != null)
        {
            input_X.interactable = false;
            input_X.onEndEdit.AddListener(Changed_X);
        }
        if (input_Z != null)
        {
            input_Z.interactable = false;
            input_Z.onEndEdit.AddListener(Changed_Z);
        }
        if (button_Delete != null)
        {
            button_Delete.interactable = false;
            button_Delete.onClick.AddListener(Delete);
        }
        if (button_Known != null)
        {
            button_Known.interactable = false;
            button_Known.image.color = new Color(1,1,1);
            button_Known.onClick.AddListener(Changed_Known);
        }
        if (input_R != null)
        {
            input_R.interactable = false;
            input_R.onEndEdit.AddListener(Change_R);
        }
        if (same_R != null)
        {
            same_R.gameObject.SetActive(false);
            same_R.onClick.AddListener(Same_R);
        }
        if (input_V != null)
        {
            input_V.interactable = false;
            input_V.onEndEdit.AddListener(Changed_V);
        }
    }

    public void ShipAdd(int no)
    {
        if (ships.Count + 1 > maxship)
            Debug.Log("Can't add ships : it will over the max count");
        else
        {
            ShipNode newNode = GameObject.Instantiate(shipNode, shipList);
            newNode.ship = GameObject.Instantiate(shipData[no]);
            newNode.ship.baseNode = newNode;
            newNode.text_Name.text = newNode.ship.shipName;
           
            ships.Add(newNode);
            selects = new List<string>();
            selects.Add(ships[ships.Count - 1].ship.shipName);

            dropdownList.AddOptions(selects);
        }    
    }

    private void Selected(int arg0)
    {
        pointer = arg0 - 1;
        if (pointer == -1)
        {
            input_X.text = "";
            input_X.interactable = false;
            input_Z.text = "";
            input_Z.interactable = false;           
            input_R.text = "";
            input_R.interactable = false;
            input_V.text = "";
            input_V.interactable = false;
            button_Delete.interactable = false;
            button_Known.image.color = new Color(1, 1, 1);
            button_Known.interactable = false;

            same_R.gameObject.SetActive(false);
        }
        else if (pointer == 0)
        {
            var target = ships[pointer];

            input_X.interactable = true;
            input_X.text = (target.set_X_value / 1000).ToString("0.0");
            input_Z.interactable = true;
            input_Z.text = (target.set_Z_value / 1000).ToString("0.0");
            input_R.interactable = true;
            input_R.text = target.set_R_value.ToString("0.0");         
            input_V.interactable = true;
            input_V.text = target.set_V_value.ToString("0.0");
            R_Image.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -1 * target.set_R_value);
            button_Delete.interactable = false;
            button_Known.image.color = new Color(0, 1, 0);
            button_Known.interactable = false;

            same_R.gameObject.SetActive(false);
        }
        else
        {
            var target = ships[pointer];

            input_X.interactable = true;
            input_X.text = (target.set_X_value / 1000).ToString("0.0");
            input_Z.interactable = true;
            input_Z.text = (target.set_Z_value / 1000).ToString("0.0");
            input_R.interactable = true;
            input_R.text = target.set_R_value.ToString("0.0");
            input_V.interactable = true;
            input_V.text = target.set_V_value.ToString("0.0");
            R_Image.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -1 * target.set_R_value);
            
            button_Known.image.color = new Color(target.ship.isKnown ? 0 : 1, 1, 0);
            button_Known.interactable = true;

            button_Delete.interactable = true;
            same_R.gameObject.SetActive(true);         
        }
    }

    private void Changed_X(string arg0)
    {
        if (arg0 == "-")
            arg0 = "-1";
        var target = ships[pointer];
        if(pointer == 0)
        {
            if (Single.Parse(arg0) * 1000 > 60000)
            {
                input_X.text = "60.0";
                target.set_X_value = 60000;
            }
            else if(Single.Parse(arg0) * 1000 < -60000)
            {
                input_X.text = "-60.0";
                target.set_X_value = -60000;
            }
            else
            {
                input_X.text = arg0;
                target.set_X_value = Single.Parse(arg0) * 1000;
            }           
        }
        else
        {
            if (Single.Parse(arg0) * 1000 > 100000)
            {
                input_X.text = "100.0";
                target.set_X_value = 100000;
            }
            else if (Single.Parse(arg0) * 1000 < -100000)
            {
                input_X.text = "-100.0";
                target.set_X_value = -100000;
            }
            else
            {
                input_X.text = arg0;
                target.set_X_value = Single.Parse(arg0) * 1000;
            }                
        }
        target.ship.transform.position = new Vector3(target.set_X_value, target.ship.transform.position.y, target.ship.transform.position.z);
    }

    private void Changed_Z(string arg0)
    {
        if (arg0 == "-")
            arg0 = "-1";
        var target = ships[pointer];
        if (pointer == 0)
        {
            if (Single.Parse(arg0) * 1000 > 60000)
            {
                input_Z.text = "60.0";
                target.set_Z_value = 60000;
            }
            else if (Single.Parse(arg0) * 1000 < -60000)
            {
                input_Z.text = "-60.0";
                target.set_Z_value = -60000;
            }
            else
            {
                input_Z.text = arg0;
                target.set_Z_value = Single.Parse(arg0) * 1000;
            }              
        }
        else
        {
            if (Single.Parse(arg0) * 1000 > 100000)
            {
                input_Z.text = "100.0";
                target.set_Z_value = 100000;
            }
            else if (Single.Parse(arg0) * 1000 < -100000)
            {
                input_Z.text = "-100.0";
                target.set_Z_value = -100000;
            }
            else
            {
                input_Z.text = arg0;
                target.set_Z_value = Single.Parse(arg0) * 1000;
            }             
        }
        target.ship.transform.position = new Vector3(target.ship.transform.position.x, target.ship.transform.position.y, target.set_Z_value);
    }

    private void Change_R(string arg0)
    {
        if (arg0 == "-")
            arg0 = "-1";
        var target = ships[pointer];
        target.ship.transform.localEulerAngles = new Vector3(target.ship.transform.localEulerAngles.x, Single.Parse(arg0), target.ship.transform.localEulerAngles.z);
        target.set_R_value = target.ship.transform.localEulerAngles.y;
        input_R.text = target.set_R_value.ToString("0.0");
        R_Image.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -1 * target.set_R_value);
    }

    private void Changed_V(string arg0)
    {
        if (arg0 == "-")
            arg0 = "-1";
        var target = ships[pointer];
        if (Single.Parse(arg0) <= 0)
            target.set_V_value = 0;
        else
            target.set_V_value = Single.Parse(arg0);
        target.ship.shipSpeed = target.set_V_value;
        input_V.text = target.set_V_value.ToString("0.0");
    }

    private void Changed_Known()
    {
        var target = ships[pointer];
        target.set_Known = !target.set_Known;
        target.ship.SetKnown(target.set_Known);    
        button_Known.image.color = new Color(target.set_Known ? 0 : 1, 1, 0);
    }
    private void Same_R()
    {
        var target = ships[pointer];
        target.set_R_value = ships[0].set_R_value;
        input_R.text = target.set_R_value.ToString("0.0");
        target.transform.localEulerAngles = new Vector3(target.transform.localEulerAngles.x, target.set_R_value, target.transform.localEulerAngles.z);
        R_Image.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -1 * target.set_R_value);
    }

    private void Delete()
    {
        GameObject.Destroy(ships[pointer].ship.gameObject);
        GameObject.Destroy(ships[pointer].gameObject);
        ships.RemoveAt(pointer);

        dropdownList.ClearOptions();

        selects = new List<string>();
        selects.Add("None");
        for(int i = 0; i < ships.Count; i++)
        {
            selects.Add(ships[i].ship.shipName);
        }
        dropdownList.AddOptions(selects);
        dropdownList.onValueChanged.Invoke(0);
    }

    public void ClearAll()
    {
        for(int i = ships.Count-1; i > 0; i--)
        {
            GameObject.Destroy(ships[i].ship.gameObject);
            GameObject.Destroy(ships[i].gameObject);
            ships.RemoveAt(i);
        }

        dropdownList.ClearOptions();

        selects = new List<string>();
        selects.Add("None");
        for (int i = 0; i < ships.Count; i++)
        {
            selects.Add(ships[i].ship.shipName);
        }
        dropdownList.AddOptions(selects);
        dropdownList.onValueChanged.Invoke(0);
    }

    public void QuickAdd(string type)
    {
        if (ships.Count + 5 > maxship)
        {
            Debug.Log("Can't add ships : it will over the max count");
        }
        else
        {
            float cv_R = ships[0].set_R_value;

            switch (type)
            {
                case "warship":
                    ShipAdd(1, cv_R, 20, 40);
                    ShipAdd(1, cv_R, -20, 40);
                    ShipAdd(2, cv_R, 70, 20);
                    ShipAdd(2, cv_R, -70, 20);
                    ShipAdd(3, cv_R, 0, 20);
                    break;
                case "sky":
                    ShipAdd(1, cv_R, 30, 20);
                    ShipAdd(1, cv_R, -30, 20);
                    ShipAdd(2, cv_R, 80, 40);
                    ShipAdd(2, cv_R, -80, 40);
                    ShipAdd(3, cv_R, 180, 40);
                    break;
                case "land":
                    ShipAdd(1, cv_R, 30, 20);
                    ShipAdd(1, cv_R, -30, 20);
                    ShipAdd(2, cv_R, 80, 20);
                    ShipAdd(2, cv_R, -80, 20);
                    ShipAdd(3, cv_R, 0, 20);
                    break;
                case "water":
                    ShipAdd(1, cv_R, 25, 20);
                    ShipAdd(1, cv_R, -25, 20);
                    ShipAdd(2, cv_R, 100, 20);
                    ShipAdd(2, cv_R, -100, 20);
                    ShipAdd(3, cv_R, 180, 20);
                    break;
            }
        }
    }

    public void ShipAdd(int no,float cv_R,float target_R,float length)
    {
        ShipNode newNode = GameObject.Instantiate(shipNode, shipList);
        newNode.ship = GameObject.Instantiate(shipData[no]);
        newNode.ship.baseNode = newNode;
        newNode.text_Name.text = newNode.ship.shipName;

        ships.Add(newNode);
        selects = new List<string>();
        var target = ships[ships.Count - 1];

        selects.Add(target.ship.shipName);

        dropdownList.AddOptions(selects);

        target.set_X_value = (float)Math.Cos(Math.PI * ((90 - cv_R - target_R) / 180)) * length * 1000 + ships[0].set_X_value;
        target.set_Z_value = (float)Math.Sin(Math.PI * ((90 - cv_R - target_R) / 180)) * length * 1000 + ships[0].set_Z_value;
  
        target.set_R_value = cv_R;

        target.ship.transform.position = new Vector3(target.set_X_value, target.ship.transform.position.y, target.set_Z_value);
        target.ship.transform.eulerAngles = new Vector3(0.0f, target.set_R_value,0.0f);
    }

    public void ShipAdd(int no, float cv_R, Vector2 vector_XZ, bool known)
    {
        ShipNode newNode = GameObject.Instantiate(shipNode, shipList);
        newNode.ship = GameObject.Instantiate(shipData[no]);
        newNode.ship.baseNode = newNode;
        newNode.text_Name.text = newNode.ship.shipName;

        ships.Add(newNode);
        selects = new List<string>();
        var target = ships[ships.Count - 1];

        selects.Add(target.ship.shipName);

        dropdownList.AddOptions(selects);

        target.set_X_value = vector_XZ.x;
        target.set_Z_value = vector_XZ.y;

        target.set_R_value = cv_R;

        target.ship.transform.position = new Vector3(target.set_X_value, target.transform.position.y, target.set_Z_value);
        target.ship.transform.eulerAngles = new Vector3(0.0f, target.set_R_value, 0.0f);

        target.ship.SetKnown(known);
    }

    #region # ML_相關
    public void RadonInitShips(int typeNo, float cv_R, int knownMax)
    {
        teamType = typeNo;

        ships[0].set_R_value = cv_R;
        ships[0].ship.transform.eulerAngles = new Vector3(0.0f, cv_R, 0.0f);

        switch (typeNo)
        {
            case 1:
                ShipAdd(1, cv_R, 20, 40);
                ShipAdd(1, cv_R, -20, 40);
                ShipAdd(2, cv_R, 70, 20);
                ShipAdd(2, cv_R, -70, 20);
                ShipAdd(3, cv_R, 0, 20);
                break;
            case 2:
                ShipAdd(1, cv_R, 30, 20);
                ShipAdd(1, cv_R, -30, 20);
                ShipAdd(2, cv_R, 80, 40);
                ShipAdd(2, cv_R, -80, 40);
                ShipAdd(3, cv_R, 180, 40);
                break;
            case 3:
                ShipAdd(1, cv_R, 30, 20);
                ShipAdd(1, cv_R, -30, 20);
                ShipAdd(2, cv_R, 80, 20);
                ShipAdd(2, cv_R, -80, 20);
                ShipAdd(3, cv_R, 0, 20);
                break;
            case 4:
                ShipAdd(1, cv_R, 25, 20);
                ShipAdd(1, cv_R, -25, 20);
                ShipAdd(2, cv_R, 100, 20);
                ShipAdd(2, cv_R, -100, 20);
                ShipAdd(3, cv_R, 180, 20);
                break;
        }

        var oriArray = new List<int> { 1, 2, 3, 4, 5 };
        var knowArray = new int[knownMax];
        var count = 0;

        while (count < knownMax)
        {
            var select = UnityEngine.Random.Range(0, oriArray.Count - 1);
            knowArray[count] = oriArray[select];
            oriArray.RemoveAt(select);
            count++;
        }

        for(int i = 0; i < knowArray.Length; i++)
        {
            ships[knowArray[i]].ship.SetKnown(true);
        }
    }


    public bool Answer(int guessTypeNo)
    {
        if (guessTypeNo == teamType)
        {
            Debug.Log("You Right!");
            animator_Answer.SetTrigger("End");
            return true;
        }
        else
        {
            Debug.Log("You Fall!");
            animator_Answer.SetTrigger("End");
            return false;
        }

    }

    public List<TypeAndPosition> GetShipsData()
    {
        var returnData = new List<TypeAndPosition>();

        for (int i = 0; i < ships.Count; i++)
        {
            var data = new TypeAndPosition();

            if (ships[i].ship.isKnown)
            {
                switch (ships[i].ship.shipName)
                {
                    case "CVLL":
                        data.type = 1;
                        break;
                    case "054A":
                        data.type = 1;
                        break;
                    case "052C":
                        data.type = 1;
                        break;
                    case "052D":
                        data.type = 1;
                        break;
                }
                data.pos = new Vector2(ships[i].set_X_value, ships[i].set_Z_value);
            }
            else
            {
                data.type = -1;
                data.pos = new Vector2();          
            }
            returnData.Add(data);
        }
        return returnData;
    }
    #endregion


    public void StartSimulator()
    {
        for(int i = 0; i < ships.Count; i++)
            ships[i].SimulatorWork(true);
    }

    public void End()
    {
        for (int i = 0; i < ships.Count; i++)
            ships[i].SimulatorWork(false);
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

            if(stringArray[0] == "position")
            {
                #region # 清空舊船團資料

                #endregion

                string[] fileData = File.ReadAllLines(filepath);

                string[] set_sailValue = fileData[3].Split(',');
                cv_R = float.Parse(set_sailValue[1]);

                string[] shipCountValue = fileData[4].Split(',');
                int shipCount = int.Parse(shipCountValue[0]);

                for (int i = 5; i < (shipCount + 5); i++)
                {
                    string[] shipInfos = fileData[i].Split(',');

                    switch (shipInfos[0])
                    {
                        case "054A":
                            ShipAdd(1, cv_R, new Vector2(float.Parse(shipInfos[1]) * (1000f), float.Parse(shipInfos[2]) * (1000f)), int.Parse(shipInfos[4]) == 1);
                            break;
                        case "052C":
                            ShipAdd(2, cv_R, new Vector2(float.Parse(shipInfos[1]) * (1000f), float.Parse(shipInfos[2]) * (1000f)), int.Parse(shipInfos[4]) == 1);
                            break;
                        case "052D":
                            ShipAdd(3, cv_R, new Vector2(float.Parse(shipInfos[1]) * (1000f), float.Parse(shipInfos[2]) * (1000f)), int.Parse(shipInfos[4]) == 1);
                            break;
                        case "CVLL":
                            ships[0].set_R_value = cv_R;
                            ships[0].ship.transform.eulerAngles = new Vector3(0.0f, cv_R, 0.0f);
                            break;
                    }
                }
            }
        }
        else
        {
            Debug.Log("系統不支援此檔案類型!");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
