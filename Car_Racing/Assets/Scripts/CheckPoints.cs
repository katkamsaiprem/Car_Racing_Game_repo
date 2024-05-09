using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoints : MonoBehaviour
{
    //we use rigidbody for quick and effective detection of obj
    public bool CheckPoint1 = true;
    public bool CheckPoint2 = false;//to findout which checkpoint we are using

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (CheckPoint1 == true)
            {//record gameTime when we pass through checkpoint1
                SaveScript.ThisCheckPoint1 = SaveScript.GameTime;//we are constantly updating gameTime ,so we  made snapshot  of the gameTime when player passed through checkpoint1
                SaveScript.CheckPointPass1 = true;
            }

            if (CheckPoint2 == true)
            {
                SaveScript.ThisCheckPoint2 = SaveScript.GameTime;
                SaveScript.CheckPointPass2 = true;
            }
        }
    }
}
