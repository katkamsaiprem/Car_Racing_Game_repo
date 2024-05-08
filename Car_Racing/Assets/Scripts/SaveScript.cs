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
    
    // Start is called before the first frame update
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        if (LapChange == true)
        {
            LapChange = false;
            LapTimeMinutes = 0f;
            LapTimeSeconds = 0f;
        }

        if (LapNumber >= 1)
        {
            LapTimeSeconds = LapTimeSeconds + 1 * Time.deltaTime;
        }

        if (LapTimeSeconds > 59)
        {
            LapTimeSeconds = 0f;
            LapTimeMinutes++;
        }
    }
}
