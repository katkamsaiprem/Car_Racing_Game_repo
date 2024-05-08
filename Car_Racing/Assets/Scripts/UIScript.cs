using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public Image SpeedRing;
    public float DisplaySpeed;

    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI GearText;

    public TextMeshProUGUI LapNumberText;
    public TextMeshProUGUI TotalLapsText;
    public TextMeshProUGUI LapTimeMinutesText;
    public TextMeshProUGUI LapTimeSecondsText;

    public int TotalLaps = 3;
    // Start is called before the first frame update
    void Start()
    {
        SpeedRing.fillAmount = 0;
        SpeedText.text = "0";
        GearText.text = "1";
        LapNumberText.text = "0";
        TotalLapsText.text ="/"+ TotalLaps.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        //SpeedOMeter
        //convert speed into %
        DisplaySpeed = SaveScript.Speed / SaveScript.TopSpeed;//give decimal number btw 0&1
        SpeedRing.fillAmount = DisplaySpeed;
        SpeedText.text = (Mathf.Round(SaveScript.Speed).ToString());
        GearText.text = (SaveScript.Gear+1).ToString();
        //LapNumber
        LapNumberText.text = SaveScript.LapNumber.ToString();
        //LapTime
        if (SaveScript.LapTimeMinutes <= 9)
        {
            LapTimeMinutesText.text = "0" + (Mathf.Round(SaveScript.LapTimeMinutes).ToString())+":";
        }
        else if (SaveScript.LapTimeMinutes >= 10)
        {
            LapTimeMinutesText.text = (Mathf.Round(SaveScript.LapTimeMinutes).ToString())+":";
        }

        if (SaveScript.LapTimeSeconds <= 9)
        {
            LapTimeSecondsText.text = "0" + (Mathf.Round(SaveScript.LapTimeSeconds).ToString());
        }
        else if (SaveScript.LapTimeSeconds>=10)
        {
            LapTimeSecondsText.text = (Mathf.Round(SaveScript.LapTimeSeconds).ToString());

        }


    }
}
