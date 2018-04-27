using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TrainState
{
    EN_ROUTE,
    ARRIVING,
    DOORS_OPEN,
    UNLOADING,
    LOADING,
    DOORS_CLOSE,
    DEPARTING
}

public class Train : MonoBehaviour
{
    public List<TrainCarriage> carriages;
    public List<Commuter> passengers;
    public int passengerCount;
    private float currentPosition = 0f;
    public Vector3 speed, accelerationStrength, brakeStrength, friction;
    
    void Update()
    {
        Vector3 _POS = transform.position;
        Vector3 _ARRIVE = Vector3.one * 0.001f;
        if (!Approach.Apply(
            ref _POS,
            ref speed,
            new Vector3(10f, 1f, 5f),
            accelerationStrength,
            accelerationStrength,
            _ARRIVE))
        {
            transform.position = _POS;
        }
        Friction.Apply(ref speed, friction);
    }
}