using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipNode : MonoBehaviour
{
    // Ship taget
    public ShipWork ship;

    public bool simulator = false;

    // Node Info
    public Text text_Name;
    public Image tag_know;
    public Text xz_Title;  
    public Text x_value;
    public Text z_value;
    public Text x_stand;
    public Text z_stand;
    public Text da_Title;
    public Text d_value;
    public Text a_value;
    public Text d_stand;
    public Text a_stand;
    public Image point;
    public Text r_value;

    private float x_data = 0.0f;
    private float z_data = 0.0f;
    private float r_data = 0.0f;

    public float set_X_value = 0.0f;
    public float set_Z_value = 0.0f;
    public float set_R_value = 0.0f;
    public float set_V_value = 30.0f;
    public bool set_Known = false;

    public void SimulatorWork(bool workset)
    {
        simulator = workset;

        if (simulator)
        {
            xz_Title.gameObject.SetActive(false);
            x_value.gameObject.SetActive(false);
            z_value.gameObject.SetActive(false);
            x_stand.gameObject.SetActive(false);
            z_stand.gameObject.SetActive(false);

            da_Title.gameObject.SetActive(true);
            d_value.gameObject.SetActive(true);
            a_value.gameObject.SetActive(true);
            d_stand.gameObject.SetActive(true);
            a_stand.gameObject.SetActive(true);

            if (ship.gameObject.activeSelf)
                ship.StartSimulator();
        }
        else
        {
            xz_Title.gameObject.SetActive(true);
            x_value.gameObject.SetActive(true);
            z_value.gameObject.SetActive(true);
            x_stand.gameObject.SetActive(true);
            z_stand.gameObject.SetActive(true);

            da_Title.gameObject.SetActive(false);
            d_value.gameObject.SetActive(false);
            a_value.gameObject.SetActive(false);
            d_stand.gameObject.SetActive(false);
            a_stand.gameObject.SetActive(false);

            ship.End();
        }
    }

    public void Reset()
    {
        ship.transform.position = new Vector3(set_X_value, 0.0f, set_Z_value);
        ship.transform.localEulerAngles = new Vector3(0.0f, set_R_value, 0.0f);
        ship.shipSpeed = set_V_value;
        ship.SetKnown(set_Known);
    }

    private void OnGUI()
    {
        if (ship == null)
            return;

        r_data = ship.transform.localEulerAngles.y;

        r_value.text = r_data.ToString("0.0");
        point.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -1 * r_data);
        tag_know.color = new Color(ship.isKnown ? 0 : 1, 1, 0);

        if (simulator)
            return;

        x_data = ship.transform.position.x;
        z_data = ship.transform.position.z;
        
        if (Mathf.Abs(x_data) >= 1000)
        {
            x_value.text = (x_data / 1000).ToString("0.0");
            x_stand.text = "Km";
        }
        else
        {
            x_value.text = x_data.ToString("0.0");
            x_stand.text = "m";
        }

        if (Mathf.Abs(z_data) >= 1000)
        {
            z_value.text = (z_data / 1000).ToString("0.0");
            z_stand.text = "Km";
        }
        else
        {
            z_value.text = z_data.ToString("0.0");
            z_stand.text = "m";
        } 
    }
}
