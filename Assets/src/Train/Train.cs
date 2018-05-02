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
    public int trainIndex;
    public List<TrainCarriage> carriages;
    public int totalCarriages;
    public List<Commuter> passengers;
    public int passengerCount;
    public int passengersLeavingAtNextStop = 0;
    private float currentPosition = 0f;
    private int currentRegion;
    public float speed = 0f;
    public float speed_on_platform_arrival = 0f;
    public float accelerationStrength, brakeStrength, railFriction;
    public float stateDelay = 0f;
    public int parentLineIndex;
    public bool isOutbound;
    public TrainState state;
    public MetroLine parentLine;
    public Platform platform_NEXT;
    public Train trainAheadOfMe;

    public Train(int _trainIndex, int _parentLineIndex, float _startPosition, int _totalCarriages)
    {
        trainIndex = _trainIndex;
        parentLineIndex = _parentLineIndex;
        parentLine = Metro.INSTANCE.metroLines[parentLineIndex];
        currentPosition = _startPosition;
        state = TrainState.EN_ROUTE;
        totalCarriages = _totalCarriages;
        SetupCarriages();
        speed = 0f;
        accelerationStrength = Metro.INSTANCE.Train_accelerationStrength * parentLine.speedRatio;
        brakeStrength = Metro.INSTANCE.Train_brakeStrength;
        railFriction = Metro.INSTANCE.Train_railFriction;
        ChangeState(TrainState.DEPARTING);
    }

    void SetupCarriages()
    {
        carriages = new List<TrainCarriage>();
        for (int i = 0; i < totalCarriages; i++)
        {
            GameObject _tempCarriage_OBJ = (GameObject) Metro.Instantiate(Metro.INSTANCE.prefab_trainCarriage);
            TrainCarriage _TC = _tempCarriage_OBJ.GetComponent<TrainCarriage>();
            carriages.Add(_TC);
        }
    }

    void Update_NextPlatform()
    {
        platform_NEXT = parentLine.Get_NextPlatform(currentPosition);
    }


    void ChangeState(TrainState _newState)
    {
        state = _newState;
        switch (_newState)
        {
            case TrainState.EN_ROUTE:
                // keep current speed
                break;
            case TrainState.ARRIVING:
                // slow down and then stop at the end of the platform
                // tell commuters on platform about available carriage spaces
                Debug.Log(trainIndex + ": ARRIVING");
                speed_on_platform_arrival = speed;
                break;
            case TrainState.DOORS_OPEN:
                // slight delay, then open the required door
                Debug.Log(trainIndex + " STOPPED - DOORS_OPENING");
                speed = 0f;
                stateDelay = Metro.INSTANCE.Train_delay_doors_OPEN;
                break;
            case TrainState.UNLOADING:
                // tell commuters they can leave
                Debug.Log(trainIndex + ": UNLOADING");
                stateDelay = 1f;
                // wait until totalPassengers == (totalPassengers - passengersLeavingAtNextStop)
                break;
            case TrainState.LOADING:
                Debug.Log(trainIndex + ": LOADING");
                // tell commuters on platform that they are able to board now
                stateDelay = 1f;
                break;
            case TrainState.DOORS_CLOSE:
                Debug.Log(trainIndex + ": DOORS_CLOSING");
                // once totalPassengers == (totalPassengers + (waitingToBoard - availableSpaces)) - shut the doors
                stateDelay = Metro.INSTANCE.Train_delay_doors_CLOSE;
                // sort out vars for next stop (nextPlatform, door side, passengers wanting to get off etc)
                break;
            case TrainState.DEPARTING:
                // slight delay
                // Determine next platform / station we'll be stopping at
                Debug.Log(trainIndex + ": DEPARTING");
                Update_NextPlatform();
                // get list of passengers who wish to depart at the next stop
                stateDelay = Metro.INSTANCE.Train_delay_departure;
                break;
            case TrainState.EMERGENCY_STOP:
                break;
        }
    }

    public void Update(float _carriageSpacing)
    {
        switch (state)
        {
            case TrainState.EN_ROUTE:
                if (speed <= parentLine.maxTrainSpeed)
                {
                    speed += accelerationStrength;
                }

                if (parentLine.Get_RegionIndex(currentPosition) == platform_NEXT.point_platform_START.index)
                {
                    ChangeState(TrainState.ARRIVING);
                }

                break;
            case TrainState.ARRIVING:

                float _platform_start = platform_NEXT.point_platform_START.distanceAlongPath;
                float _platform_end = platform_NEXT.point_platform_END.distanceAlongPath;
                float _platform_length = _platform_end - _platform_start;
                float arrivalProgress = (parentLine.Get_proportionAsDistance(currentPosition) - _platform_start) /
                                        _platform_length;
                arrivalProgress = 1f - Mathf.Cos(arrivalProgress * Mathf.PI * 0.5f);
                speed = speed_on_platform_arrival * (1f - arrivalProgress);

                if (arrivalProgress >= Metro.PLATFORM_ARRIVAL_THRESHOLD)
                {
                    ChangeState(TrainState.DOORS_OPEN);
                }

                break;
            case TrainState.DOORS_OPEN:

                if (Timer.TimerReachedZero(ref stateDelay))
                {
                    bool allReady = true;
                    foreach (TrainCarriage _CARRIAGE in carriages)
                    {
                        if (!_CARRIAGE.Doors_OPEN())
                        {
                            allReady = false;
                        }
                    }
                    if (allReady)
                    {
                        ChangeState(TrainState.UNLOADING);
                    }
                }
                break;
            case TrainState.UNLOADING:
                // alert passengers in departing list
                // get list of passengers that will be boarding
                break;
            case TrainState.LOADING:
                // when all boardees are inside
                break;
            case TrainState.DOORS_CLOSE:
                if (Timer.TimerReachedZero(ref stateDelay))
                {
                    bool allReady = true;
                    foreach (TrainCarriage _CARRIAGE in carriages)
                    {
                        if (!_CARRIAGE.Doors_CLOSED())
                        {
                            allReady = false;
                        }
                    }
                    if (allReady)
                    {
                        ChangeState(TrainState.DEPARTING);
                    }
                }
                break;
            case TrainState.DEPARTING:
                // slight delay
                // Determine next platform / station we'll be stopping at
                // get list of passengers who wish to depart at the next stop
                if (Timer.TimerReachedZero(ref stateDelay))
                {
                    ChangeState(TrainState.EN_ROUTE);
                }

                break;
            case TrainState.EMERGENCY_STOP:
                break;
        }

        currentPosition = ((currentPosition += speed) % 1f);
        isOutbound = currentPosition <= 0.5f;
        speed *= railFriction;
        UpdateCarriages();
    }

    void UpdateCarriages()
    {
        float carriageLength_asRailDistance = parentLine.Get_distanceAsRailProportion(TrainCarriage.CARRIAGE_LENGTH);
        float _REAL_CARRIAGE_LENGTH = TrainCarriage.CARRIAGE_LENGTH + TrainCarriage.CARRIAGE_SPACING;
        carriages[0].UpdateCarriage(currentPosition, parentLine.Get_PositionOnRail(currentPosition),
            parentLine.Get_RotationOnRail(currentPosition));
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
                if (realDistanceFromPrevious < (_REAL_CARRIAGE_LENGTH))
                {
                    carriageRailPosition -= 0.0001f;
                    if (carriageRailPosition < 0)
                    {
                        carriageRailPosition += 1f;
                    }

                    _current_POS = parentLine.Get_PositionOnRail(carriageRailPosition);
                    realDistanceFromPrevious = Vector3.Distance(_current_POS, _prev_POS);
                }
                else
                {
                    break;
                }
            }

            _current.UpdateCarriage(carriageRailPosition, _current_POS,
                parentLine.Get_RotationOnRail(carriageRailPosition));
        }
    }
}