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
    public TextMeshProUGUI RaceTimeMinutesText;
    public TextMeshProUGUI RaceTimeSecondsText;
    public TextMeshProUGUI BestLapTimeMinutesText;
    public TextMeshProUGUI BestLapTimeSecondsText;
    public TextMeshProUGUI CheckPointTime;
    public GameObject NewLapRecord;
    public GameObject CheckPointDisplay;//required to access gameobj to off or no

    public int TotalLaps = 3;
    // Start is called before the first frame update
    void Start()
    {
        SpeedRing.fillAmount = 0;
        SpeedText.text = "0";
        GearText.text = "1";
        LapNumberText.text = "0";
        TotalLapsText.text ="/"+ TotalLaps.ToString();
        NewLapRecord.SetActive(false);
        CheckPointDisplay.SetActive(false); 
        
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
        //RaceTime
        if (SaveScript.RaceTimeMinutes <= 9)
        {
            RaceTimeMinutesText.text = "0" + (Mathf.Round(SaveScript.RaceTimeMinutes).ToString())+":";
        }
        else if (SaveScript.RaceTimeMinutes >= 10)
        {
            RaceTimeMinutesText.text = (Mathf.Round(SaveScript.RaceTimeMinutes).ToString())+":";
        }

        if (SaveScript.RaceTimeSeconds <= 9)
        {
            RaceTimeSecondsText.text = "0" + (Mathf.Round(SaveScript.RaceTimeSeconds).ToString());
        }
        else if (SaveScript.RaceTimeSeconds>=10)
        {
            RaceTimeSecondsText.text = (Mathf.Round(SaveScript.RaceTimeSeconds).ToString());

        }
        //BestLapTime
        if (SaveScript.LapChange == true)
        {
            if (Mathf.Approximately(SaveScript.LastLapM, SaveScript.BestLapTimeM)) //if Minutes are same then only update seconds
            {
                if (SaveScript.LastLapS < SaveScript.BestLapTimeS)
                {
                    SaveScript.BestLapTimeS = SaveScript.LastLapS;
                    SaveScript.NewRecord = true;
                }
            }
            if (SaveScript.LastLapM < SaveScript.BestLapTimeM)
            {
                SaveScript.BestLapTimeM = SaveScript.LastLapM;
                SaveScript.BestLapTimeS = SaveScript.LastLapS;
                SaveScript.NewRecord = true;
            }
        }

        //Display BestLapTime
        if (SaveScript.BestLapTimeM <= 9)
        {
            BestLapTimeMinutesText.text = "0" + (Mathf.Round(SaveScript.BestLapTimeM).ToString())+":";
        }
        else if (SaveScript.RaceTimeMinutes >= 10)
        {
            BestLapTimeMinutesText.text = (Mathf.Round(SaveScript.BestLapTimeM).ToString())+":";
        }

        if (SaveScript.BestLapTimeS <= 9)
        {
            BestLapTimeSecondsText.text = "0" + (Mathf.Round(SaveScript.BestLapTimeS).ToString());
        }
        else if (SaveScript.BestLapTimeS>=10)
        {
            BestLapTimeSecondsText.text = (Mathf.Round(SaveScript.BestLapTimeS).ToString());

        }

        if (SaveScript.NewRecord == true)
        {
            NewLapRecord.SetActive(true);
            StartCoroutine(LapRecordOff());
        }
        
        //CheckPoint1
        if (SaveScript.CheckPointPass1 == true)
        {
            SaveScript.CheckPointPass1 = false;
            if (SaveScript.LapNumber > 1)
            {
                CheckPointDisplay.SetActive(true);
                if (SaveScript.ThisCheckPoint1 >
                    SaveScript.LastCheckPoint1) //so player is moving slow as compared to last
                {
                    CheckPointTime.color = Color.red;
                    CheckPointTime.text = "-" + (SaveScript.ThisCheckPoint1 - SaveScript.LastCheckPoint1).ToString();
                    StartCoroutine(
                        CheckPointOff()); //pause(yield) the script for certain amount of time before running other commandline
                }

                if (SaveScript.ThisCheckPoint1 <
                    SaveScript.LastCheckPoint1) //so player is moving slow as compared to last
                {
                    CheckPointTime.color = Color.green;
                    CheckPointTime.text = "+" + (SaveScript.LastCheckPoint1 - SaveScript.ThisCheckPoint1).ToString();
                    StartCoroutine(CheckPointOff());
                }
            }

        }
        //CheckPoint2
        if (SaveScript.CheckPointPass2 == true)
        {
            SaveScript.CheckPointPass2 = false;
            if (SaveScript.LapNumber > 1)
            {
                CheckPointDisplay.SetActive(true);
                if (SaveScript.ThisCheckPoint2 >
                    SaveScript.LastCheckPoint2) //so player is moving slow as compared to last
                {
                    CheckPointTime.color = Color.red;
                    CheckPointTime.text = "-" + (SaveScript.ThisCheckPoint2 - SaveScript.LastCheckPoint2).ToString();
                    StartCoroutine(
                        CheckPointOff()); //pause(yield) the script for certain amount of time before running other commandline
                }

                if (SaveScript.ThisCheckPoint2 <
                    SaveScript.LastCheckPoint2) //so player is moving slow as compared to last
                {
                    CheckPointTime.color = Color.green;
                    CheckPointTime.text = "+" + (SaveScript.LastCheckPoint2 - SaveScript.ThisCheckPoint2).ToString();
                    StartCoroutine(CheckPointOff());
                }
            }

        }
        
    }

    IEnumerator CheckPointOff()
    {
        yield return new WaitForSeconds(2);
        CheckPointDisplay.SetActive(false);
    }

    IEnumerator LapRecordOff()
{
    yield return new WaitForSeconds(2); // Pause the execution of this coroutine for 2 seconds
    SaveScript.NewRecord = false; // Reset the NewRecord flag in the SaveScript class
    NewLapRecord.SetActive(false); // Deactivate the NewLapRecord GameObject in the scene
}
}
