using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveScript : MonoBehaviour
{
    public static float Speed; 
    public static float TopSpeed;
    public static int Gear;
    public static int LapNumber;
    public static bool LapChange = false;
    public static float LapTimeMinutes;
    public static float LapTimeSeconds;
    public static float RaceTimeMinutes;
    public static float RaceTimeSeconds;
    public static float BestLapTimeM;
    public static float BestLapTimeS;
    public static float LastLapM;
    public static float LastLapS;
    public static float GameTime;
    public static float LastCheckPoint1;
    public static float ThisCheckPoint1;
    public static float LastCheckPoint2;
    public static float ThisCheckPoint2;
    public static bool CheckPointPass1 = false;
    public static bool CheckPointPass2 = false;
    public static bool NewRecord = false;
    
    
  
    void Update()
    {
        if (LapChange == true)
        {
            LapChange = false;
            LapTimeMinutes = 0f;
            LapTimeSeconds = 0f;
            GameTime = 0;
        }

        if (LapNumber >= 1)
        {
            LapTimeSeconds = LapTimeSeconds + 1 * Time.deltaTime;
            RaceTimeSeconds = RaceTimeSeconds + 1 * Time.deltaTime;
            GameTime = GameTime + 1 * Time.deltaTime;
        }

        if (LapTimeSeconds > 59)
        {
            LapTimeSeconds = 0f;
            LapTimeMinutes++;
        }
        if (RaceTimeSeconds > 59)
        {
            RaceTimeSeconds = 0f;
            RaceTimeMinutes++;
        }
    }
}
