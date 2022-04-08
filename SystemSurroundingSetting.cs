using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SystemSurroundingSetting : MonoBehaviour
{
    public Slider slider_SystemSpeed;
    public InputField Input_SystemSpeed;

    public float systemSpeed = 1.0f;

    private bool gameStop = false;

    // Start is called before the first frame update
    void Start()
    {
        systemSpeed = Time.timeScale;

        if (slider_SystemSpeed != null)
        {
            slider_SystemSpeed.value = systemSpeed;
            slider_SystemSpeed.onValueChanged.AddListener(SpeedSetting);
        }
            
        if (Input_SystemSpeed != null)
        {
            Input_SystemSpeed.text = systemSpeed.ToString();
            Input_SystemSpeed.onEndEdit.AddListener(SpeedSetting);
        }
    }

    private void SpeedSetting(float arg0)
    {
        systemSpeed = arg0;

        Time.timeScale = systemSpeed;
        Input_SystemSpeed.text = systemSpeed.ToString();
    }

    private void SpeedSetting(string arg0)
    {
        if (Input_SystemSpeed.text == "-")
            Input_SystemSpeed.text = "-1";

        systemSpeed =  Single.Parse(Input_SystemSpeed.text);

        if (systemSpeed < 0)
            systemSpeed = 0;
        else if (systemSpeed > 20)
            systemSpeed = 20;

        slider_SystemSpeed.value = systemSpeed;
    }

    public void StopGame(Text btn_text)
    {
        if (gameStop)
        {
            btn_text.text = "暫停模擬";
            gameStop = false;
            Time.timeScale = systemSpeed;
        }
        else
        {
            btn_text.text = "繼續模擬";
            gameStop = true;
            Time.timeScale = 0;
        }
    }

    public void TryResetStop(Text btn_text)
    {
        if (gameStop)
        {
            btn_text.text = "暫停模擬";
            gameStop = false;
            Time.timeScale = systemSpeed;
        }
    }
}
