using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Smoothing factors for camera movement and rotation
    public float moveSmoothness;
    public float rotSmoothness;

    // Offset for camera movement and rotation relative to the car
    public Vector3 moveOffset;
    public Vector3 rotOffset;

    // Reference to the car's transform, which the camera will follow
    public Transform carTarget;
    

    void FixedUpdate()
    {
        // Call the method to follow the target (car)
        FollowTarget();
    }

    void FollowTarget()
    {
        // Handle camera movement and rotation
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
       
        // Calculate the target position for the camera
        Vector3 targetPos = new Vector3();
        targetPos = carTarget.TransformPoint(moveOffset);
        // Smoothly move the camera towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);
    }
    void HandleRotation()
    {
        // Calculate the direction from the camera to the car
        var direction = carTarget.position - transform.position;
        var rotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);

        // Calculate the desired rotation angle, clamping to limit the rotation angle
        Quaternion targetRotation = Quaternion.Euler(
            Mathf.Clamp(rotation.eulerAngles.x, -20f, 20f), // Limit the rotation angle between -20 and 20 degrees
            rotation.eulerAngles.y,
            rotation.eulerAngles.z
        );
        // Apply the rotation with smoothing
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotSmoothness * Time.deltaTime);
    }
}