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
    DEPARTING,
    EMERGENCY_STOP
}

public class Train
{
    public List<TrainCarriage> carriages;
    public int totalCarriages;
    public List<Commuter> passengers;
    public int passengerCount;
    public int passengersLeavingAtNextStop = 0;
    private float currentPosition = 0f;
    public float speed = 0f;
    public int parentLineIndex;
    public TrainState state;
    public float timeInCurrentState = 0f;
    public MetroLine parentLine;
    public Platform platform_NEXT;

    public Train(int _parentLineIndex, float _startPosition, int _totalCarriages)
    {
        parentLineIndex = _parentLineIndex;
        parentLine = Metro.INSTANCE.metroLines[parentLineIndex];
        currentPosition = _startPosition;
        state = TrainState.EN_ROUTE;
        totalCarriages = _totalCarriages;
        SetupCarriages();
    }

    void SetupCarriages()
    {
        carriages = new List<TrainCarriage>();
        for (int i = 0; i < totalCarriages; i++)
        {
            GameObject _tempCarriage_OBJ =  (GameObject) Metro.Instantiate(Metro.INSTANCE.prefab_trainCarriage);
            TrainCarriage _TC = _tempCarriage_OBJ.GetComponent<TrainCarriage>();
            carriages.Add(_TC);
        }
    }

    void ChangeState(TrainState _newState)
    {
        switch (_newState)
        {
            case TrainState.EN_ROUTE:
                // keep current speed
                break;
            case TrainState.ARRIVING:
                // slow down and then stop at the end of the platform
                // tell commuters on platform about available carriage spaces
                break;
            case TrainState.DOORS_OPEN:
                // slight delay, then open the required door
                break;
            case TrainState.UNLOADING:
                // tell commuters they can leave
                // wait until totalPassengers == (totalPassengers - passengersLeavingAtNextStop)
                break;
            case TrainState.LOADING:
                // tell commuters on platform that they are able to board now
                break;
            case TrainState.DOORS_CLOSE:
                // once totalPassengers == (totalPassengers + (waitingToBoard - availableSpaces)) - shut the doors
                // sort out vars for next stop (nextPlatform, door side, passengers wanting to get off etc)
                break;
            case TrainState.DEPARTING:
                // slight delay, then increase up to top speed
                break;
            case TrainState.EMERGENCY_STOP:
                break;
        }

        timeInCurrentState = 0f;
    }

    public void Update(float _carriageSpacing)
    {
        switch (state)
        {
            case TrainState.EN_ROUTE:
                break;
            case TrainState.ARRIVING:
                break;
            case TrainState.DOORS_OPEN:
                break;
            case TrainState.UNLOADING:
                break;
            case TrainState.LOADING:
                break;
            case TrainState.DOORS_CLOSE:
                break;
            case TrainState.DEPARTING:
                break;
            case TrainState.EMERGENCY_STOP:
                break;
        }

        timeInCurrentState += Time.deltaTime;

        currentPosition = (currentPosition + parentLine.train_accelerationStrength) % 1f;
        
        float carriageLength_asRailDistance = parentLine.Get_distanceAsRailProportion(TrainCarriage.CARRIAGE_LENGTH);
        for (int i = 0; i < totalCarriages; i++)
        {
            float carriageLengths = i * carriageLength_asRailDistance;
            float carriageSpacings = i * _carriageSpacing;
            float carriagePosition = currentPosition + (carriageLengths + carriageSpacings);
            TrainCarriage _TC = carriages[i];
            
            _TC.Set_Position(parentLine.Get_PositionOnRail(carriagePosition));
            _TC.Set_Rotation(parentLine.Get_RotationOnRail(carriagePosition));
        }
    }
}