using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilsWithoutDirection;

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

    private Transform target_trans;

    private Transform m_transform;
    private Rigidbody m_rigidbody;

    public float searchRange = 0.0f;
    public Cinemachine.CinemachineVirtualCamera distory_view;
    public ParticleSystem broken_particle;

    // Start is called before the first frame update
    void Start()
    {

        if (target_trans == null)
            target_trans = GameObject.FindObjectOfType<Controller>().transform;

        if (knownSign != null)
        {
            if (isKnown)
                knownSign.material = material_known;
            else
                knownSign.material = material_unknown;
        }
        m_transform = this.transform;
        m_rigidbody = this.GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!startSimulator)
            return;

        if (other.CompareTag("RF"))
        {
            var missile = FindObjectOfType<Controller>();

            if (this.name == "CVLL")
            {
                missile.enmyTarget = this;
                missile.RF_Finded(this);

                // Debug.Log($"Find Ships={missile.findShips.Count}, Missile Pos=({missile.transform.position.x}, {missile.transform.position.z})");

                float predict2real_dist = System.Numerics.Vector2.Distance(missile.predicted_CV_pos, new System.Numerics.Vector2(this.transform.position.x, this.transform.position.z) / 1000.0f);
                // 若偵測到的CV和預測的CV差距超過5公里，則更新目標位置，並重新規劃攻擊路徑
                if (predict2real_dist > 5.0f)
                {
                    // (var CV_pos, var Frigate_pos, var sp_predictions) = missile.Predict_CV();
                    (var CV_pos, var Frigate_pos) = missile.Reorganize_ships();
                    // 將預測的CV更新為真實觀測到的CV
                    missile.predicted_CV_pos = new System.Numerics.Vector2(x: CV_pos.X, y: CV_pos.Y);
                    missile.predicted_Frigate_pos = new List<System.Tuple<System.Numerics.Vector2, float>>(Frigate_pos);
                    Debug.Log($"直接看到航母, CV=({CV_pos.X}, {CV_pos.Y})");
                    // 規劃攻擊路線打擊航母
                    missile.Hit_CV(CV_pos, Frigate_pos);
                }

            }
            else
            {
                if (distance <= 28000)
                {
                    missile.RF_Finded(this);

                    // Debug.Log($"Find Ships={missile.findShips.Count}, Missile Pos=({missile.transform.position.x}, {missile.transform.position.z})");

                    if (!isKnown)
                    {
                        SetKnown(true);
                    }

                    // if (missile.findShips.Count >= 3 && missile.predicted_CV == false && missile.enmyTarget == null)
                    // 若已偵測到3~5艘護衛艦、以及偵測到新船艦、以及還沒有偵測到航母、以及猜陣型功能開啟
                    if (missile.findShips.Count >= 3 && missile.findShips.Count <= 5 && missile.findShips.Count > missile.find_Ships_count && missile.enmyTarget == null && missile.guess_formation == true)
                    {
                        // 執行陣型推估功能：輸出CV可能位置、陣型、以及全部船艦的位置
                        (var CV_pos, var Frigate_pos, var sp_predictions) = missile.Predict_CV();

                        // 更新可能的CV位置
                        missile.predicted_CV_pos = new System.Numerics.Vector2(x: CV_pos.X, y: CV_pos.Y);
                        // 更新可能船團的船艦位置
                        missile.predicted_Frigate_pos = new List<System.Tuple<System.Numerics.Vector2, float>>(Frigate_pos);

                        // for(int i = 0;i<sp_predictions.Count;i++)
                        // {   
                        //     ShipPermutation sp = sp_predictions[i];
                        //     Debug.Log($"Top {i+1}, Type={sp.formation}");
                        //     foreach (KeyValuePair<string, Position> item in sp.ship_position_predict)
                        //     {
                        //         string ship_name = item.Key;
                        //         Position p = item.Value;
                        //         Debug.Log($"Ship name: {ship_name}, Ship position: ({p.x}, {p.y})");
                        //     }
                        // }

                        missile.predicted_CV = true;
                        // 規劃攻擊路線打擊航母
                        missile.Hit_CV(missile.predicted_CV_pos, Frigate_pos);

                    }
                    // 如果偵測到新船艦，同時也尚未偵測到航母
                    else if (missile.findShips.Count > missile.find_Ships_count && missile.enmyTarget != null)
                    {
                        // 重新整理可能的CV位置(當前應該沒有CV的位置)、護衛艦的位置
                        (var CV_pos, var Frigate_pos) = missile.Reorganize_ships();
                        missile.predicted_CV_pos = new System.Numerics.Vector2(x: CV_pos.X, y: CV_pos.Y);
                        missile.predicted_Frigate_pos = new List<System.Tuple<System.Numerics.Vector2, float>>(Frigate_pos);
                        // 規劃攻擊路線打擊航母
                        missile.Hit_CV(CV_pos, Frigate_pos);
                    }
                    // 若尚未看到航母、也沒有經過預測的位置、同時避障策略開啟
                    else if (missile.enmyTarget == null && missile.predicted_CV == false && missile.avoid_static == true)
                    {
                        // 複雜避障
                        if (missile.complicated_static == true)
                        {
                            missile.SettingAvoidPath(this.transform.position);
                        }
                        // 簡易避障
                        else
                        {
                            missile.SimpleAvoidPath(this.transform.position);
                        }
                    }
                }
            }
        }
        else if (other.CompareTag("Missile"))
        {
            if (startSimulator)
            {
                startSimulator = false;
                other.GetComponent<Controller>().Broken();
                Debug.Log("The Ship [" + shipName + "] is be destroy!!");
                broken_particle.Play();
                var missile = GameObject.FindObjectOfType<Controller>();
                Debug.Log($"攻擊的平均威脅 = {missile.threat_total_value / missile.threat_total_time}");
                Debug.Log($"路徑的最大威脅 = {missile.threat_max_value}");
                FindObjectOfType<Timer>().TimerEnd();
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
        var selfPos = new Vector2(m_transform.position.x, m_transform.position.z);

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
    }

    public void End()
    {
        startSimulator = false;
        if (broken_particle != null)
            broken_particle.Stop();
        if (distory_view != null)
            distory_view.enabled = false;
        baseNode.Reset();
    }

    private void FixedUpdate()
    {
        if (!startSimulator)
            return;

        var nextPos = m_transform.position + m_transform.forward * shipSpeed * 0.5144f * Time.fixedDeltaTime;
        m_rigidbody.MovePosition(nextPos);
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
