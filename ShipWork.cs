using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipWork : MonoBehaviour
{
    public string shipName = "";
    public bool isKnown = false;
    public bool startSimulator = false;

    public Material material_unknown;
    public Material material_known;
    public MeshRenderer knownSign;
    public ShipNode baseNode;

    public float shipSpeed = 30.0f;

    public float distance = 0.0f;
    public List<DangerRange> dangerRanges = new List<DangerRange>();

    private Transform self_trans;
    private Transform target_trans;

    public float searchRange = 0.0f;
    public Cinemachine.CinemachineVirtualCamera distory_view;
    public ParticleSystem broken_particle;

    // Start is called before the first frame update
    void Start()
    {
        self_trans = this.transform;

        if (target_trans == null)
            target_trans = GameObject.FindObjectOfType<Controller>().transform;

        if (knownSign != null)
        {
            if (isKnown)
                knownSign.material = material_known;
            else
                knownSign.material = material_unknown;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RF"))
        {
            var missile = FindObjectOfType<Controller>();

            if(this.name == "CVLL")
            {
                missile.enmyTarget = this;
                missile.RF_Finded("CVLL",new Vector2(self_trans.position.x, self_trans.position.z));
            }
            else
            {
                if (distance <= 28000)
                {
                    missile.RF_Finded("", new Vector2(self_trans.position.x, self_trans.position.z));

                    if (!isKnown)
                        SetKnown(true);
                    else
                    {
                        if (missile.enmyTarget == null)
                            missile.SettingAvoidPath(this.transform.position, searchRange);
                    }
                }
            }                     
        }
        else if(other.CompareTag("Missile"))
        {
            if (startSimulator)
            {
                startSimulator = false;
                other.GetComponent<Controller>().Broken();
                Debug.Log("The Ship [" + shipName + "] is be destroy!!");
                broken_particle.Play();
            }
        }
    }

    // Update is called once per frame
    public void SetKnown(bool setKnown)
    {
        isKnown = setKnown;
        if (isKnown)
            knownSign.material = material_known;
        else
            knownSign.material = material_unknown;
    }

    private void OnGUI()
    {
        var targetPos = new Vector2(target_trans.position.x, target_trans.position.z);
        var selfPos = new Vector2(self_trans.position.x, self_trans.position.z);

        distance = Vector2.Distance(selfPos, targetPos);

        if (startSimulator)
        {
            if (isKnown)
            {
                
                var indanger = false;
               
                if (distance > 1000.0f)
                {
                    baseNode.d_value.text = (distance / 1000.0f).ToString("0.000");
                    baseNode.d_stand.text = "Km";
                }
                else
                {
                    baseNode.d_value.text = distance.ToString("0.000");
                    baseNode.d_stand.text = "m";
                }
                if (dangerRanges.Count > 0)
                {
                    for (int i = 0; i < dangerRanges.Count; i++)
                    {
                        var max_d = dangerRanges[i].max_distance;
                        var min_d = dangerRanges[i].min_diatance;
                        var max_value = dangerRanges[i].max_dnagerValue;
                        var min_value = dangerRanges[i].min_dnagerValue;

                        if ((distance <= max_d) && (distance > min_d)) //Distance is in Range
                        {
                            indanger = true;
                            baseNode.d_value.color = dangerRanges[i].danagerColor.Evaluate((max_d - distance) / (max_d - min_d));
                            baseNode.a_value.color = dangerRanges[i].danagerColor.Evaluate((max_d - distance) / (max_d - min_d));
                            baseNode.a_value.text = (((max_d - distance) / (max_d - min_d)) * (min_value - max_value) + max_value).ToString();
                        }
                    }
                }
                if (!indanger)
                {
                    baseNode.d_value.color = Color.white;
                    baseNode.a_value.color = Color.white;
                    baseNode.a_value.text = "0";
                }
            }
        }

        #region //曲率高度設定

        float setting = Mathf.Exp(((float)((distance / 1000) / 4.12 - Mathf.Sqrt(target_trans.position.y))));

        if (setting >= 16)
        {
            transform.position = new Vector3(transform.position.x, -16.0f, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, 0.0f - setting, transform.position.z);
        }
        #endregion
    }

    public void StartSimulator()
    {
        if (startSimulator)
            return;
        startSimulator = true;
        StartCoroutine(SimulatorMoving());
    }

    public void End()
    {
        startSimulator = false;
        if (broken_particle != null)
            broken_particle.Stop();
        if(distory_view != null)
            distory_view.enabled = false;
        baseNode.Reset();
    }

    IEnumerator SimulatorMoving()
    {
        while (startSimulator)
        {
            yield return null;
            this.transform.Translate(0.0f, 0.0f, (shipSpeed * 0.5144f) / 60.0f * Time.timeScale);
            if(Vector3.Distance(target_trans.position, this.transform.position) <= 500)
                distory_view.enabled = true;
        }
    }
}

[System.Serializable]
public class DangerRange 
{
    public Gradient danagerColor;
    public float max_distance;
    public float min_diatance;
    public float max_dnagerValue;
    public float min_dnagerValue;
}
