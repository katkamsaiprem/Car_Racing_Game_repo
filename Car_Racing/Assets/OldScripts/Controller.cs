using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public WheelCollider[] Wheels = new WheelCollider[4];
   
       public float torque = 200;
       public float steeringMax = 20;
   
       // Use this for initialization
      
   
       private void FixedUpdate()
       {
           TorqueForce();
           Steering();
           animataWheels();
       }

       private void TorqueForce()
       {
            if (Input.GetKey(KeyCode.W))
            {           
                for (int i = 0; 1 < Wheels.Length; i++)
                { 
                    Wheels[i].motorTorque = torque;
                }
              
            }
            else
            {
                for (int i = 0; i < Wheels.Length; i++)
                {
                    Wheels[i].motorTorque = 0;
                }
            }
       }

       private void Steering()
       {
           if (Input.GetAxis("Horizontal") != 0)
           {
               for (int i = 0; 1 < Wheels.Length-2; i++)
               {
                   Wheels[i].steerAngle= Input.GetAxis("Horizontal")* steeringMax;
               }
           }
       }

       private void animataWheels()
       {
           
       }
}
