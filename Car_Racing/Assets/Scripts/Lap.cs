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
         SaveScript.LapNumber++;
         SaveScript.LapChange = true;

      }
   }
}
