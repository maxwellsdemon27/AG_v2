using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMaker : MonoBehaviour
{
    public GameObject base_point;
    
    public PathPointer[] Path_W = new PathPointer[9];
    public PathPointer[] Path_S = new PathPointer[9];
    public PathPointer[] Path_E = new PathPointer[9];
    public PathPointer[] Path_N = new PathPointer[9];

    public PathPointer[] SPath_W = new PathPointer[9];
    public PathPointer[] SPath_S = new PathPointer[9];
    public PathPointer[] SPath_E = new PathPointer[9];
    public PathPointer[] SPath_N = new PathPointer[9];

    public List<PathGiver> pathList = new List<PathGiver>();
    public bool ready = false;

    // Start is called before the first frame update
    void Start()
    {
        #region #Path_W
        Path_W[0].pos = new Vector3(32775.0f, 0, 0);
        Path_W[0].data.target_right = -38.6f;
        Path_W[0].data.target_R = 90.0f;

        Path_W[1].pos = new Vector3(40000.0f, 0, 7225.0f);
        Path_W[1].data.target_right = 0.0f;
        Path_W[1].data.target_R = 0.0f;

        Path_W[2].pos = new Vector3(40000.0f, 0, 32775.0f);
        Path_W[2].data.target_right = -38.6f;
        Path_W[2].data.target_R = 0.0f;

        Path_W[3].pos = new Vector3(32775.0f, 0, 40000.0f);
        Path_W[3].data.target_right = 0.0f;
        Path_W[3].data.target_R = -90.0f;

        Path_W[4].pos = new Vector3(-32775.0f, 0, 40000.0f);
        Path_W[4].data.target_right = -38.6f;
        Path_W[4].data.target_R = -90.0f;

        Path_W[5].pos = new Vector3(-40000.0f, 0, 32775.0f);
        Path_W[5].data.target_right = 0.0f;
        Path_W[5].data.target_R = -180.0f;

        Path_W[6].pos = new Vector3(-40000.0f, 0, -32775.0f);
        Path_W[6].data.target_right = -38.6f;
        Path_W[6].data.target_R = -180.0f;

        Path_W[7].pos = new Vector3(-32775.0f, 0, -40000.0f);
        Path_W[7].data.target_right = 0.0f;
        Path_W[7].data.target_R = -270.0f;

        Path_W[8].pos = new Vector3(60000, 0, -40000.0f);
        #endregion

        #region #Path_S
        for (int i = 0; i < Path_S.Length; i++)
        {
            Path_S[i].pos = new Vector3(-1 * Path_W[i].pos.z, 0, Path_W[i].pos.x);
            Path_S[i].data.target_right= Path_W[i].data.target_right;
            Path_S[i].data.target_R = Path_W[i].data.target_R - 90.0f;
        }
        #endregion

        #region #Path_E
        for (int i = 0; i < Path_E.Length; i++)
        {
            Path_E[i].pos = new Vector3(-1 * Path_W[i].pos.x, 0, -1 * Path_W[i].pos.z);
            Path_E[i].data.target_right = Path_W[i].data.target_right;
            Path_E[i].data.target_R = Path_W[i].data.target_R - 180.0f;
        }
        #endregion

        #region #Path_N
        for (int i = 0; i < Path_N.Length; i++)
        {
            Path_N[i].pos = new Vector3(Path_W[i].pos.z, 0, -1 * Path_W[i].pos.x);
            Path_N[i].data.target_right = Path_W[i].data.target_right;
            Path_N[i].data.target_R = Path_W[i].data.target_R - 270.0f;
        }
        #endregion

        #region #SPath_W
        SPath_W[0].pos = new Vector3(-62992.69f, 0, 0);
        SPath_W[0].data.target_right = 38.6f;
        SPath_W[0].data.target_R = 90.0f;

        SPath_W[1].pos = new Vector3(-57883.85f, 0, -2116.15f);
        SPath_W[1].data.target_right = 0.0f;

        SPath_W[2].pos = new Vector3(-5108.84f, 0, -54891.16f);
        SPath_W[2].data.target_right = -38.6f;

        var vec_1 = SPath_W[2].pos - SPath_W[1].pos;
        SPath_W[1].data.target_R = Quaternion.LookRotation(vec_1).eulerAngles.y;
        SPath_W[2].data.target_R = SPath_W[1].data.target_R;

        SPath_W[3].pos = new Vector3(5108.84f, 0, -54891.16f);
        SPath_W[3].data.target_right = 0.0f;
     
        SPath_W[4].pos = new Vector3(54891.16f, 0, -5108.84f);
        SPath_W[4].data.target_right = -38.6f;

        var vec_2 = SPath_W[4].pos - SPath_W[3].pos;  
        SPath_W[3].data.target_R = Quaternion.LookRotation(vec_2).eulerAngles.y;
        SPath_W[4].data.target_R = SPath_W[3].data.target_R;

        SPath_W[5].pos = new Vector3(54891.16f, 0, 5108.84f);
        SPath_W[5].data.target_right = 0.0f;

        SPath_W[6].pos = new Vector3(5108.84f, 0, 54891.16f);
        SPath_W[6].data.target_right = -38.6f;

        var vec_3 = SPath_W[6].pos - SPath_W[5].pos;
        SPath_W[5].data.target_R = Quaternion.LookRotation(vec_3).eulerAngles.y;
        SPath_W[6].data.target_R = SPath_W[5].data.target_R;


        SPath_W[7].pos = new Vector3(-5108.84f, 0, 54891.16f);
        SPath_W[7].data.target_right = 0.0f;

        SPath_W[8].pos = new Vector3(-60000.0f, 0, 0);

        var vec_4 = SPath_W[8].pos - SPath_W[7].pos;
        SPath_W[7].data.target_R = Quaternion.LookRotation(vec_4).eulerAngles.y;
        #endregion

        #region #SPath_S
        for (int i = 0; i < SPath_S.Length; i++)
        {
            SPath_S[i].pos = new Vector3(-1 * SPath_W[i].pos.z, 0, SPath_W[i].pos.x);
            SPath_S[i].data.target_right = SPath_W[i].data.target_right;
            SPath_S[i].data.target_R = SPath_W[i].data.target_R - 90.0f;
        }
        #endregion

        #region #SPath_E
        for (int i = 0; i < SPath_E.Length; i++)
        {
            SPath_E[i].pos = new Vector3(-1 * SPath_W[i].pos.x, 0, -1 * SPath_W[i].pos.z);
            SPath_E[i].data.target_right = SPath_W[i].data.target_right;
            SPath_E[i].data.target_R = SPath_W[i].data.target_R -180.0f;
        }
        #endregion

        #region #SPath_N
        for (int i = 0; i < SPath_N.Length; i++)
        {
            SPath_N[i].pos = new Vector3(SPath_W[i].pos.z, 0, -1 * SPath_W[i].pos.x);
            SPath_N[i].data.target_right = SPath_W[i].data.target_right;
            SPath_N[i].data.target_R = SPath_W[i].data.target_R - 270.0f;
        }
        #endregion

        ready = true;
    }

    public void TrySetStandardPath(StanderPath standerPath)
    {
        StartCoroutine(TrySetPath(standerPath));
    }

    public void SetStandardPath(StanderPath standerPath)
    {
        ClearOldPoint();
        switch (standerPath)
        { 
            case StanderPath.Start_W:
                for(int i = 0; i < Path_W.Length; i++)
                {
                    var point = GameObject.Instantiate(base_point, new Vector3(Path_W[i].pos.x, 10, Path_W[i].pos.z), new Quaternion().normalized, this.transform);
                    point.name = "P" + (i + 1);
                    var pg = point.GetComponent<PathGiver>();

                    if (Path_W[i].data.target_right == 0)
                        pg.pathMode = PathMode.FORWORD;
                    else
                    {
                        pg.target_right = Path_W[i].data.target_right;
                        pg.pathMode = PathMode.TURN;
                    }
                    pg.target_R = Path_W[i].data.target_R;
                    pathList.Add(pg);
                }
                break;
            case StanderPath.Start_S:
                for (int i = 0; i < Path_S.Length; i++)
                {
                    var point = GameObject.Instantiate(base_point, new Vector3(Path_S[i].pos.x, 10, Path_S[i].pos.z), new Quaternion().normalized, this.transform);
                    point.name = "P" + (i + 1);
                    var pg = point.GetComponent<PathGiver>();

                    if (Path_S[i].data.target_right == 0)               
                        pg.pathMode = PathMode.FORWORD;
                    else
                    {
                        pg.target_right = Path_S[i].data.target_right;
                        pg.pathMode = PathMode.TURN;
                    }
                    pg.target_R = Path_S[i].data.target_R;
                    pathList.Add(pg);
                }
                break;
            case StanderPath.Start_E:
                for (int i = 0; i < Path_E.Length; i++)
                {
                    var point = GameObject.Instantiate(base_point, new Vector3(Path_E[i].pos.x, 10, Path_E[i].pos.z), new Quaternion().normalized, this.transform);
                    point.name = "P" + (i + 1);
                    var pg = point.GetComponent<PathGiver>();

                    if (Path_E[i].data.target_right == 0)
                        pg.pathMode = PathMode.FORWORD;
                    else
                    {
                        pg.target_right = Path_E[i].data.target_right;
                        pg.pathMode = PathMode.TURN;
                    }
                    pg.target_R = Path_E[i].data.target_R;
                    pathList.Add(pg);
                }
                break;
            case StanderPath.Start_N:
                for (int i = 0; i < Path_N.Length; i++)
                {
                    var point = GameObject.Instantiate(base_point, new Vector3(Path_N[i].pos.x, 10, Path_N[i].pos.z), new Quaternion().normalized, this.transform);
                    point.name = "P" + (i + 1);
                    var pg = point.GetComponent<PathGiver>();

                    if (Path_N[i].data.target_right == 0)                 
                        pg.pathMode = PathMode.FORWORD;
                    else
                    {
                        pg.target_right = Path_N[i].data.target_right;
                        pg.pathMode = PathMode.TURN;
                    }
                    pg.target_R = Path_N[i].data.target_R;
                    pathList.Add(pg);
                }
                break;
            case StanderPath.Start_SW:
                for (int i = 0; i < SPath_W.Length; i++)
                {
                    var point = GameObject.Instantiate(base_point, new Vector3(SPath_W[i].pos.x, 10, SPath_W[i].pos.z), new Quaternion().normalized, this.transform);
                    point.name = "P" + (i + 1);
                    var pg = point.GetComponent<PathGiver>();

                    if (SPath_W[i].data.target_right == 0)
                        pg.pathMode = PathMode.FORWORD;
                    else
                    {
                        pg.target_right = SPath_W[i].data.target_right;
                        pg.pathMode = PathMode.TURN;
                    }
                    pg.target_R = SPath_W[i].data.target_R;
                    pathList.Add(pg);
                }
                break;
            case StanderPath.Start_SS:
                for (int i = 0; i < SPath_S.Length; i++)
                {
                    var point = GameObject.Instantiate(base_point, new Vector3(SPath_S[i].pos.x, 10, SPath_S[i].pos.z), new Quaternion().normalized, this.transform);
                    point.name = "P" + (i + 1);
                    var pg = point.GetComponent<PathGiver>();

                    if (SPath_S[i].data.target_right == 0)
                        pg.pathMode = PathMode.FORWORD;
                    else
                    {
                        pg.target_right = SPath_S[i].data.target_right;
                        pg.pathMode = PathMode.TURN;
                    }
                    pg.target_R = SPath_S[i].data.target_R;
                    pathList.Add(pg);
                }
                break;
            case StanderPath.Start_SE:
                for (int i = 0; i < SPath_E.Length; i++)
                {
                    var point = GameObject.Instantiate(base_point, new Vector3(SPath_E[i].pos.x, 10, SPath_E[i].pos.z), new Quaternion().normalized, this.transform);
                    point.name = "P" + (i + 1);
                    var pg = point.GetComponent<PathGiver>();

                    if (SPath_E[i].data.target_right == 0)
                        pg.pathMode = PathMode.FORWORD;
                    else
                    {
                        pg.target_right = SPath_E[i].data.target_right;
                        pg.pathMode = PathMode.TURN;
                    }
                    pg.target_R = SPath_E[i].data.target_R;
                    pathList.Add(pg);
                }
                break;
            case StanderPath.Start_SN:
                for (int i = 0; i < SPath_N.Length; i++)
                {
                    var point = GameObject.Instantiate(base_point, new Vector3(SPath_N[i].pos.x, 10, SPath_N[i].pos.z), new Quaternion().normalized, this.transform);
                    point.name = "P" + (i + 1);
                    var pg = point.GetComponent<PathGiver>();

                    if (SPath_N[i].data.target_right == 0)
                        pg.pathMode = PathMode.FORWORD;
                    else
                    {
                        pg.target_right = SPath_N[i].data.target_right;
                        pg.pathMode = PathMode.TURN;
                    }
                    pg.target_R = SPath_N[i].data.target_R;
                    pathList.Add(pg);
                }
                break;
        }
        PathLink();
    }

    IEnumerator TrySetPath(StanderPath standerPath)
    {
        if (ready)
            SetStandardPath(standerPath);          
        else
        {
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(TrySetPath(standerPath));
        }
    }

    public void ClearOldPoint()
    {
        for (int i = this.transform.childCount - 1; i >=0; i--)
            GameObject.Destroy(this.transform.GetChild(i).gameObject);
        pathList.Clear();
    }

    public void PathLink()
    {
        for (int i = 0; i < pathList.Count - 1; i++)
            pathList[i].next = pathList[i + 1];
    }
}

[System.Serializable]
public class PathPointer
{
    public Vector3 pos = new Vector3(0,0,0);  
    public PathData data = new PathData();
    public PathPointer next = null;
}

[System.Serializable]
public class PathData
{
    public float target_right = 0.0f;
    public float target_R = 0.0f;
}

public enum PathMode
{
    FORWORD, TURN
}