using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFollow : MonoBehaviour
{
    public GameObject CarMarker;

    public GameObject Vehicle;

    public float Speed;
    

    
    void Update()
    {
        CarMarker.transform.position =
            Vector3.MoveTowards(CarMarker.transform.position, Vehicle.transform.position+new Vector3(0f,21f,0f), Speed);
    }
}
