using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TrainCarriage : MonoBehaviour
{

    public const float CARRIAGE_LENGTH = 5f;
    public const float CARRIAGE_SPACING = 0.2f;
    
    public List<Commuter> passengers;
    public int passengerCount;
    public TrainCarriage_door door_LEFT;
    public TrainCarriage_door door_RIGHT;

    
    private Transform t;
    private Material mat;

    private void Start()
    {
        t = transform;
//        mat = GetComponent<Renderer>().material;
    }

    public void Set_Position(Vector3 _pos)
    {
        t.position = _pos;
    }
    public void Set_Rotation(Vector3 _rot)
    {
//        t.localEulerAngles = new Vector3(0f,_rot.x * 360f, 0f);
        t.LookAt(t.position - _rot);
    }
}
