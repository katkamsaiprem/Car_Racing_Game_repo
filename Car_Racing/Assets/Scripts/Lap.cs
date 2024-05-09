using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lap : MonoBehaviour
{
   private void OnTriggerEnter(Collider other)
   {
      if (other.gameObject.CompareTag("Player"))
      {
         SaveScript.LastLapM = SaveScript.LapTimeMinutes;
         SaveScript.LastLapS = SaveScript.LapTimeSeconds;
         SaveScript.LapNumber++;
         SaveScript.LapChange = true;
         if (SaveScript.LapNumber == 2)
         {
            SaveScript.BestLapTimeM = SaveScript.LastLapM;
            SaveScript.BestLapTimeS = SaveScript.LastLapS;
         }

         SaveScript.CheckPointPass1 = false;
         SaveScript.CheckPointPass2 = false;
         SaveScript.LastCheckPoint1 = SaveScript.ThisCheckPoint1;//lastcheckpoint saves prev time and thisCheckPoint saves curr time
         SaveScript.LastCheckPoint2 = SaveScript.ThisCheckPoint2;


      }
   }
}
