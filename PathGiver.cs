using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGiver : MonoBehaviour
{
    public PathMode pathMode = PathMode.FORWORD;

    public PathGiver next;

    public float target_R;
    public float target_right;

    public TurnCircle ownCircle;
    public LineRenderer enterLine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == ("MainController"))
        {
            Debug.Log("Giver_Work");

            if (next == null)
            {
                other.GetComponent<Controller>().startSimulator = false;
                if (ownCircle != null)
                    ownCircle.StartIn();
            }
            else
            {
                other.transform.position = this.transform.position;
                other.transform.localEulerAngles = new Vector3(other.transform.localEulerAngles.x, target_R, other.transform.localEulerAngles.z);

                switch (pathMode)
                {
                    case PathMode.FORWORD:
                        other.GetComponent<Controller>().ChangeR(0.0f);
                        if (!other.GetComponent<Controller>().turnStayRF)
                        {
                            other.GetComponent<Controller>().work_RF = true;
                        }
                        if (ownCircle != null)
                            ownCircle.StartOut();
                        break;
                    case PathMode.TURN:
                        other.GetComponent<Controller>().ChangeR(target_right);
                        if (!other.GetComponent<Controller>().turnStayRF)
                        {
                            other.GetComponent<Controller>().work_RF = false;
                        }
                        if (ownCircle != null)
                            ownCircle.StartIn();
                        break;
                }
            }
        }
    }
}
