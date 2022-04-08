using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnCircle : MonoBehaviour
{
    public PathGroupMaker pathGroupMaker;

    public PathGiver pointIn;
    public PathGiver pointOut;

    public TurnCircle NextCircle;

    public TurnMode turnMode = TurnMode.Right;

    private float self_r =  7225.0f;

    public bool end;

    public void LinkNext(TurnCircle next)
    {
        NextCircle = next;

        var self_pos = new Vector2(this.transform.position.x, this.transform.position.z);
        var next_pos = new Vector2(next.transform.position.x, next.transform.position.z);

        var self_turnM = this.turnMode;
        var next_turnM = next.turnMode;

        var middle_pos = (self_pos + next_pos) / 2;

        if (self_turnM == next_turnM) //模式相同 (求外公切線)
        {
            var vectSN = (next_pos - self_pos) / Vector2.Distance(next_pos, self_pos);

            if (self_turnM == TurnMode.Right) //左外公切線
            {
                var vec_n = new Vector2(-vectSN.y, vectSN.x);

                var self_pO_pos = self_pos + (vec_n * self_r);
                var next_pI_pos = next_pos + (vec_n * self_r);

                pointOut = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(self_pO_pos.x, 10, self_pO_pos.y), new Quaternion().normalized, this.transform);
                if (next.pointIn == null)
                    next.pointIn = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(next_pI_pos.x, 10, next_pI_pos.y), new Quaternion().normalized, next.transform);
                else
                    next.pointIn.transform.position = new Vector3(next_pI_pos.x, 10, next_pI_pos.y);
            }
            else //右外公切線
            {
                var vec_n = new Vector2(vectSN.y, -vectSN.x);

                var self_pO_pos = self_pos + (vec_n * self_r);
                var next_pI_pos = next_pos + (vec_n * self_r);

                pointOut = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(self_pO_pos.x, 10, self_pO_pos.y), new Quaternion().normalized, this.transform);
                if (next.pointIn == null)
                    next.pointIn = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(next_pI_pos.x, 10, next_pI_pos.y), new Quaternion().normalized, next.transform);
                else
                    next.pointIn.transform.position = new Vector3(next_pI_pos.x, 10, next_pI_pos.y);
            }
        }
        else //模式相異 (求內公切線)
        {
            if((self_pos.x- next_pos.x) <= 1000) //需反轉(X<->Y)
            {
                var a = self_pos.y;
                var b = self_pos.x;
                var c = next_pos.y;
                var d = next_pos.x;
                var r1 = self_r;
                var r2 = NextCircle.self_r;

                var IntComTans = InternalCommonTangentGet(a, b, c, d, r1, r2);

                if (self_turnM == TurnMode.Right) //左內公切線
                {
                    pointOut = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(IntComTans[1].start.y, 10, IntComTans[1].start.x), new Quaternion().normalized, this.transform);
                    if (next.pointIn == null)
                        next.pointIn = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(IntComTans[1].end.y, 10, IntComTans[1].end.x), new Quaternion().normalized, next.transform);
                    else
                        next.pointIn.transform.position = new Vector3(IntComTans[1].end.y, 10, IntComTans[1].end.x);
                }
                else
                {
                    pointOut = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(IntComTans[0].start.y, 10, IntComTans[0].start.x), new Quaternion().normalized, this.transform);
                    if (next.pointIn == null)
                        next.pointIn = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(IntComTans[0].end.y, 10, IntComTans[0].end.x), new Quaternion().normalized, next.transform);
                    else
                        next.pointIn.transform.position = new Vector3(IntComTans[0].end.y, 10, IntComTans[0].end.x);
                }
            }
            else
            {
                var a = self_pos.x;
                var b = self_pos.y;
                var c = next_pos.x;
                var d = next_pos.y;
                var r1 = self_r;
                var r2 = NextCircle.self_r;

                var IntComTans = InternalCommonTangentGet(a, b, c, d, r1, r2);

                if (self_turnM == TurnMode.Right) //左內公切線
                {
                    pointOut = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(IntComTans[0].start.x, 10, IntComTans[0].start.y), new Quaternion().normalized, this.transform);
                    if (next.pointIn == null)
                        next.pointIn = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(IntComTans[0].end.x, 10, IntComTans[0].end.y), new Quaternion().normalized, next.transform);
                    else
                        next.pointIn.transform.position = new Vector3(IntComTans[0].end.x, 10, IntComTans[0].end.y);
                }
                else
                {
                    pointOut = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(IntComTans[1].start.x, 10, IntComTans[1].start.y), new Quaternion().normalized, this.transform);
                    if (next.pointIn == null)
                        next.pointIn = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(IntComTans[1].end.x, 10, IntComTans[1].end.y), new Quaternion().normalized, next.transform);
                    else
                        next.pointIn.transform.position = new Vector3(IntComTans[1].end.x, 10, IntComTans[1].end.y);
                }
            }
        }

        if (pointIn != null)
            pointIn.next = pointOut;

        pointOut.next = next.pointIn;
        pointOut.pathMode = PathMode.FORWORD;

        next.pointIn.pathMode = PathMode.TURN;
        if (next.turnMode == TurnMode.Right)
            next.pointIn.target_right = 38.6f;
        else
            next.pointIn.target_right = -38.6f;

        var vec_1 = next.pointIn.transform.position - pointOut.transform.position;
        pointOut.target_R = Quaternion.LookRotation(vec_1).eulerAngles.y;
        next.pointIn.target_R = Quaternion.LookRotation(vec_1).eulerAngles.y;

        pointOut.GetComponent<BoxCollider>().enabled = false;
        next.pointIn.GetComponent<BoxCollider>().enabled = false;

        next.pointIn.enterLine.SetPosition(0, pointOut.transform.position);
        next.pointIn.enterLine.SetPosition(1, next.pointIn.transform.position);

        pointOut.ownCircle = this;
        next.pointIn.ownCircle = next;
    }

    public void SettingStart()
    {
        var controller = GameObject.FindObjectOfType<Controller>();

        var controller_pos = new Vector2(controller.transform.position.x, controller.transform.position.z);
        var vec_f = new Vector2(controller.transform.forward.x, controller.transform.forward.z);

        if (turnMode == TurnMode.Right)
        {
            var vec_n = new Vector2(-vec_f.y, vec_f.x);
            pointIn = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(this.transform.position.x + vec_n.x * self_r, 10, this.transform.position.z + vec_n.y * self_r), new Quaternion().normalized, this.transform);
            pointIn.pathMode = PathMode.TURN;
            pointIn.target_right = 38.6f;
            pointIn.target_R = controller.transform.rotation.eulerAngles.y;
            pointIn.ownCircle = this;
            pointIn.enterLine.SetPosition(0, controller.transform.position);
            pointIn.enterLine.SetPosition(1, pointIn.transform.position);
        }
        else
        {
            var vec_n = new Vector2(vec_f.y, -vec_f.x);
            pointIn = GameObject.Instantiate(pathGroupMaker.giver_prefab, new Vector3(this.transform.position.x + vec_n.x * self_r, 10, this.transform.position.z + vec_n.y * self_r), new Quaternion().normalized, this.transform);
            pointIn.pathMode = PathMode.TURN;
            pointIn.target_right = -38.6f;
            pointIn.target_R = controller.transform.rotation.eulerAngles.y;
            pointIn.ownCircle = this;
            pointIn.enterLine.SetPosition(0, controller.transform.position);
            pointIn.enterLine.SetPosition(1, pointIn.transform.position);
        }
    }

    public void StartIn()
    {
        pointIn.gameObject.SetActive(false);
        if (pointOut != null)
            pointOut.GetComponent<BoxCollider>().enabled = true;
        else
        {
            end = true;
            this.gameObject.SetActive(false);
        }
    }

    public void StartOut()
    {
        if (NextCircle != null)
            NextCircle.pointIn.GetComponent<BoxCollider>().enabled = true;
        end = true;
        this.gameObject.SetActive(false);
        pathGroupMaker.PathEndCheck();
    }

    private List<InternalCommonTangent> InternalCommonTangentGet(float a,float b,float c,float d,float r1,float r2)
    {
        var r3 = r1 + r2;
        var sigma_1 = Mathf.Sqrt(a * a - 2 * a * c + b * b - 2 * b * d + c * c + d * d - r3 * r3);
        var sigma_2 = (a - c) * (a * a - 2 * a * c + b * b - 2 * b * d + c * c + d * d);
        var sigma_3 = (-a * a + a * c - b * b + d * b + r3 * r3) / (a - c);
        var sigma_4 = 2 * b * b * d;

        //計算C3切點(x3_1,y3_1),(x3_2,y3_2)
        var x3_1 = -sigma_3 - (b - d) * (a * a * b + b * c * c + b * d * d - sigma_4 - b * r3 * r3 + d * r3 * r3 + b * b * b + a * r3 * sigma_1 - c * r3 * sigma_1 - 2 * a * b * c) / sigma_2;
        var x3_2 = -sigma_3 - (b - d) * (a * a * b + b * c * c + b * d * d - sigma_4 - b * r3 * r3 + d * r3 * r3 + b * b * b - a * r3 * sigma_1 + c * r3 * sigma_1 - 2 * a * b * c) / sigma_2;

        sigma_1 = Mathf.Sqrt(a * a - 2 * a * c + b * b - 2 * b * d + c * c + d * d - r3 * r3);
        sigma_2 = a * a - 2 * a * c + b * b - 2 * b * d + c * c + d * d;
        sigma_3 = 2 * b * b * d;

        var y3_1 = (a * a * b + b * c * c + b * d * d - sigma_3 - b * r3 * r3 + d * r3 * r3 + b * b * b + a * r3 * sigma_1 - c * r3 * sigma_1 - 2 * a * b * c) / sigma_2;
        var y3_2 = (a * a * b + b * c * c + b * d * d - sigma_3 - b * r3 * r3 + d * r3 * r3 + b * b * b - a * r3 * sigma_1 + c * r3 * sigma_1 - 2 * a * b * c) / sigma_2;

        //計算C1切點(x1_1,y1_1,x1_2,y1_2)
        var landa = r2 / r3;
        var x1_1 = landa * a + (1 - landa) * x3_1;
        var y1_1 = landa * b + (1 - landa) * y3_1;
        var x1_2 = landa * a + (1 - landa) * x3_2;
        var y1_2 = landa * b + (1 - landa) * y3_2;

        //計算C2切點(x2_1,y2_1,x2_2,y2_2)
        var x2_1 = x1_1 - x3_1 + c;
        var y2_1 = y1_1 - y3_1 + d;
        var x2_2 = x1_2 - x3_2 + c;
        var y2_2 = y1_2 - y3_2 + d;

        var returnLine = new List<InternalCommonTangent>();

        var left_line = new InternalCommonTangent();
        var right_line = new InternalCommonTangent();

        left_line.start = new Vector3(x1_2, y1_2);
        left_line.end = new Vector3(x2_2, y2_2);
        returnLine.Add(left_line);

        right_line.start = new Vector3(x1_1, y1_1);
        right_line.end = new Vector3(x2_1, y2_1);
        returnLine.Add(right_line);
        return returnLine;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}

public enum TurnMode
{
    Right,Left
}

[System.Serializable]
public class InternalCommonTangent
{
    public Vector2 start = new Vector2();
    public Vector2 end = new Vector2();
}