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
        float _REAL_CARRIAGE_LENGTH = TrainCarriage.CARRIAGE_LENGTH + TrainCarriage.CARRIAGE_SPACING;
        carriages[0].UpdateCarriage(currentPosition, parentLine.Get_PositionOnRail(currentPosition), parentLine.Get_RotationOnRail(currentPosition));
        for (int i = 1; i < totalCarriages; i++)
        {
            TrainCarriage _current = carriages[i];
            TrainCarriage _prev = carriages[i - 1];
            Vector3 _prev_POS = _prev.transform.position;
            float carriageRailPosition = _prev.positionOnRail;
            Vector3 _current_POS = parentLine.Get_PositionOnRail(carriageRailPosition);
            float realDistanceFromPrevious = Vector3.Distance(_current_POS, _prev_POS);
            int attempts = 1000;
            for (int j = 0; j < attempts; j++)
            {
                if(realDistanceFromPrevious < (_REAL_CARRIAGE_LENGTH))
                {
                    carriageRailPosition -= 0.0001f;
                    if (carriageRailPosition < 0)
                    {
                        carriageRailPosition += 1f;
                    }

                    _current_POS = parentLine.Get_PositionOnRail(carriageRailPosition);
                    realDistanceFromPrevious = Vector3.Distance(_current_POS, _prev_POS);
                } else {
                    break;
                }
            }
//            while (realDistanceFromPrevious < (_REAL_CARRIAGE_LENGTH + _THRESHOLD) || attempts > 0)
//            {
//                attempts--;
//                Debug.Log("Real dist from previous: " + realDistanceFromPrevious);
//                carriageRailPosition = (carriageRailPosition - 0.1f) % 1f;
//                _current_POS = parentLine.Get_PositionOnRail(carriageRailPosition);
//                realDistanceFromPrevious = Vector3.Distance(_current_POS, _prev_POS);
//            }
            _current.UpdateCarriage(carriageRailPosition, _current_POS, parentLine.Get_RotationOnRail(carriageRailPosition));
    
        }
    }
}