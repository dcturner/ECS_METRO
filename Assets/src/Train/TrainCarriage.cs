using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class TrainCarriage : MonoBehaviour
{

    public const float CARRIAGE_LENGTH = 5f;
    public const float CARRIAGE_SPACING = 1f;
    public const int CARRIAGE_CAPACITY = 10;

    public float positionOnRail;
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

    public void UpdateCarriage(float _newPositionOnRail, Vector3 _newPos, Vector3 _newRotation)
    {
        positionOnRail = _newPositionOnRail;
        t.position = _newPos;
        t.LookAt(t.position - _newRotation);
    }

}
