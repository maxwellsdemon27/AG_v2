using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGroupMaker : MonoBehaviour
{
    //預製物
    public PathGiver giver_prefab;
    public TurnCircle turncircle_prefab;

    //預設路徑設定
    public List<PathSetting> pathSettings = new List<PathSetting>();

    //路徑控管
    public List<PathGroup> pathGroups = new List<PathGroup>();

    //Inspector用指標
    public int editor_PS_Pointer = 0;

    //功能測試用參數
    public int pathNo = 0;
    public bool init_go = false;


    //將指定的預設路徑建置出來的方法
    public void SettingPathGroup(int targetNo)
    {
        if (targetNo >= pathSettings.Count)
            return;

        pathNo = targetNo;
        var posList = pathSettings[targetNo].circleDatas;
        var group = new PathGroup();
        group.groupName = pathSettings[targetNo].name;

        for (int i = 0; i < posList.Count; i++)
        {
            var circle = GameObject.Instantiate(turncircle_prefab, new Vector3(posList[i].position.x, 10, posList[i].position.y), new Quaternion().normalized, this.transform);
            circle.name = pathSettings[targetNo].name + "_circle" + (i + 1);
            circle.turnMode = posList[i].turnMode;
            circle.pathGroupMaker = this;
            group.Circles.Add(circle);
        }
        pathGroups.Add(group);
        LinkPathCircles(group.groupName);
    }

    //輸入的路徑建置出來的方法
    public void SettingPathGroup(PathSetting path)
    {
        if (path.circleDatas.Count == 0)
            return;

        // 所產生的所有避障圓
        var posList = path.circleDatas;
        // 新增一個PathGroup的物件group，此物件中儲存多個避障圓，預設的地毯式搜索法的避障圓就是一組PathGroup的物件
        var group = new PathGroup();
        // PathGroup物件的名稱為避障路徑名稱
        group.groupName = path.name;

        for (int i = 0; i < posList.Count; i++)
        {
            // 將所產生的避障圓依序實體化，包含該圓的圓心、迴轉方向等等
            var circle = GameObject.Instantiate(turncircle_prefab, new Vector3(posList[i].position.x, 10, posList[i].position.y), new Quaternion().normalized, this.transform);
            // 將迴轉圓依序命名，index從1開始
            circle.name = path.name + "_circle" + (i + 1);
            // 迴轉方向
            circle.turnMode = posList[i].turnMode;
            circle.pathGroupMaker = this;
            // 將圓依序新增至group物件內
            group.Circles.Add(circle);
        }
        // pathGroups是一個List用於儲存多個PathGroup，index 0為地毯式搜索的迴轉圓
        pathGroups.Add(group);
        LinkPathCircles(group.groupName);
        LinkPathGroup();
    }

    //將節點連接起來的方法
    // 
    public void LinkPathCircles(string name)
    {
        for (int i = 0; i < pathGroups.Count; i++)
        {
            // 當PathGroup的物件的名稱為避障路徑名稱，且避障圓數量大於0
            if ((pathGroups[i].groupName == name) && (pathGroups[i].Circles.Count > 0))
            {
                // 從第一個避障圓到倒數第二個，每個避障圓都與下一個相連
                for (int j = 0; j < pathGroups[i].Circles.Count - 1; j++)
                {
                    // 將第一個避障圓設定為開始
                    if (j == 0)
                        pathGroups[i].Circles[j].SettingStart();
                    // 將當前的圓與下一個相連
                    pathGroups[i].Circles[j].LinkNext(pathGroups[i].Circles[j + 1]);
                }
            }
        }
    }

    //將兩個路徑連結起來的方法
    public void LinkPathGroup()
    {
        if (pathGroups.Count == 2)
        {
            int linkPoint = pathGroups[0].Circles.Count - 1;

            for (int i = 0; i < pathGroups[0].Circles.Count; i++)
            {
                if (!pathGroups[0].Circles[i].end)
                {
                    linkPoint = i;
                    i = pathGroups[0].Circles.Count;
                }
            }
            pathGroups[1].Circles[pathGroups[1].Circles.Count - 1].LinkNext(pathGroups[0].Circles[linkPoint]);
        }
    }


    public void PathEndCheck()
    {
        for (int i = 0; i < pathGroups.Count; i++)
        {
            if (pathGroups[i].Circles[pathGroups[i].Circles.Count - 1].end)
            {
                pathGroups.RemoveAt(i);
            }
        }
    }

    //清除所有已建置的路徑的方法
    public void ResetAll()
    {
        if (transform.childCount == 0)
            return;
        for (int i = transform.childCount - 1; i >= 0; i--)
            GameObject.Destroy(transform.GetChild(i).gameObject);
        pathGroups = new List<PathGroup>();
    }

    //路徑建製測試
    void Update()
    {
        if (init_go)
        {
            init_go = false;
            ResetAll();
            SettingPathGroup(pathNo);
        }
    }
}

//路徑設定的類別
[System.Serializable]
public class PathSetting
{
    public string name;
    public Vector2 start_Pos = new Vector2();
    public float start_R = 0.0f;
    public List<CircleData> circleDatas = new List<CircleData>();
}

//迴轉圓設置需求資料類別
[System.Serializable]
public class CircleData
{
    public Vector2 position = new Vector2();
    public TurnMode turnMode = TurnMode.Right;
}

//生成後路徑控管的類別
[System.Serializable]
public class PathGroup
{
    public string groupName = "";
    public List<TurnCircle> Circles = new List<TurnCircle>();
}