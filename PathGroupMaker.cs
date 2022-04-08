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

        var posList = path.circleDatas;
        var group = new PathGroup();
        group.groupName = path.name;

        for (int i = 0; i < posList.Count; i++)
        {           
            var circle = GameObject.Instantiate(turncircle_prefab, new Vector3(posList[i].position.x, 10, posList[i].position.y), new Quaternion().normalized, this.transform);
            circle.name = path.name + "_circle" + (i + 1);
            circle.turnMode = posList[i].turnMode;
            circle.pathGroupMaker = this;
            group.Circles.Add(circle);
        }
        pathGroups.Add(group);
        LinkPathCircles(group.groupName);
        LinkPathGroup();
    }

    //將節點連接起來的方法
    private void LinkPathCircles(string name)
    {
        for (int i = 0; i < pathGroups.Count; i++)
        {
            if ((pathGroups[i].groupName == name) && (pathGroups[i].Circles.Count > 0))
            {
                for (int j = 0; j < pathGroups[i].Circles.Count - 1; j++)
                {
                    if (j == 0)
                        pathGroups[i].Circles[j].SettingStart();

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
            int linkPoint = 0;

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
        for(int i = transform.childCount - 1; i >= 0; i--)
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