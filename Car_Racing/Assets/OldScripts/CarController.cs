using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
         private Rigidbody Player;
         public WheelCollider FRWheel;
         public WheelCollider FLWheel;
         public WheelCollider RRWheel;
         public WheelCollider RLWheel;

         public MeshRenderer FRWheelMesh;
         public MeshRenderer FLWheelMesh;
         public MeshRenderer RRWheelMesh;
         public MeshRenderer RLWheelMesh;

         private float gasInput;    
         private float steeringInput;
         
         public float motorTorqueforce;
         public float brakePower;
         public float brakeInput;
         public float slipAngle;

         private  float _speed;
         public AnimationCurve steeringCurve;//to process curve,we need to use animation curve
         // Start is called before the first frame update
    void Start()
    {
        Player = gameObject.GetComponent<Rigidbody>();
       
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ApplyBrake();
        _speed = Player.velocity.magnitude;//get car speed
        CheckInput();
        ApplyMotor();
        ApplySteering();
        UpdateWheelsPosition();
        
    }
    //function to for meshes to follow Wheel collider
    void UpdateWheelsPosition()
    {
        UpdateWheel(FRWheel,FRWheelMesh);
        UpdateWheel(FLWheel,FLWheelMesh);
        UpdateWheel(RRWheel,RRWheelMesh);
        UpdateWheel(RLWheel,RLWheelMesh);
        
    }
    
    private void CheckInput()
    {
        gasInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        slipAngle = Vector3.Angle(transform.forward,Player.velocity-transform.forward);
        float movingDirection = Vector3.Dot(transform.forward, Player.velocity);
        if (Input.GetKey(KeyCode.Space))
        {
            brakeInput = 1;
        }
        else if (movingDirection < -0.5f && gasInput > 0)
        {
            brakeInput = Mathf.Abs(gasInput);
        }
        else if (movingDirection > 0.5f && gasInput < 0)
        {
            brakeInput = Mathf.Abs(gasInput);
        }
        else
        { 
            
            brakeInput = 0;
        }
        
    }
//func to rotate and steer wheels based on input force 
    void ApplyMotor()
    {
        //updating force and gasInput(direction)
        RRWheel.motorTorque = motorTorqueforce * gasInput;  
        RLWheel.motorTorque = motorTorqueforce * gasInput;
        
    }

    //steering degrees should be based on car speed curve
    void ApplySteering()
    {
        // max steerAngle should 65,if speed increase angle should decrease.so,car cant roll
        //x-axis is speed and y-axis is steerAngle in AnimationCurve which can happen in inspector
        //if under steering problem occurs,change Traction(stiffness),it means  the grip between the tires and the road surface that allows a vehicle to start, stop and/or change direction 
        float steeringAngle = steeringInput * steeringCurve.Evaluate(_speed);//steering curve is based on speed
        if (slipAngle < 120f)
        {
            steeringAngle += Vector3.SignedAngle(transform.forward, Player.velocity + transform.forward, Vector3.up);
        }
        steeringAngle = Mathf.Clamp(steeringAngle, -60f, 60f);
        FRWheel.steerAngle = steeringAngle;//apply steeringAngle to front wheels
        FLWheel.steerAngle = steeringAngle;

    }

    void UpdateWheel(WheelCollider coll, MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 position;
        //need position and rotation of wheel colliders.So, we can update to mesh
        coll.GetWorldPose(out position,out quat);//return colliders world positions
        wheelMesh.transform.position = position;//update coll position to mesh
        wheelMesh.transform.rotation = quat;//update coll rotation to mesh 
    }

    void ApplyBrake()
    {
        FRWheel.brakeTorque = brakeInput * brakePower*0.7f;
        FLWheel.brakeTorque = brakeInput * brakePower*0.7f;
        RRWheel.brakeTorque = brakeInput * brakePower*0.3f;
        RLWheel.brakeTorque = brakeInput * brakePower*0.3f;
       
    }
    
}


