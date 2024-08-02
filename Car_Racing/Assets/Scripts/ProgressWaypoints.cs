using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressWaypoints : MonoBehaviour
{
    //this script used to deal with  waypointdata
    public int WPNumber;

    public int CarTracking = 0;
    
//give rigid to progress to give separate physics
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("progress"))
        {
            
        }
    }
}
