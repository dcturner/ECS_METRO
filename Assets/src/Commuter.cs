using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public enum CommuterState
{
    WALK,
    QUEUE,
    GET_ON_TRAIN,
    GET_OFF_TRAIN,
    WAIT_FOR_STOP,
}

public class CommuterTask
{
    public CommuterState state;
    public Vector3[] destinations;
    public int destinationIndex = 0;
    public Platform startPlatform, endPlatform;
    public Walkway walkway;

    public CommuterTask(CommuterState _state)
    {
        state = _state;
    }

    public override string ToString()
    {
        return "" + state;
    }
}

public class Commuter : MonoBehaviour
{
    public const float ACCELERATION_STRENGTH = 0.01f;
    public const float FRICTION = 0.8f;
    public const float ARRIVAL_THRESHOLD = 0.02f;
    public const float QUEUE_PERSONAL_SPACE = 0.4f;
    public const float QUEUE_MOVEMENT_DELAY = 0.25f;
    public const float QUEUE_DECISION_RATE = 1.5f;

    public float satisfaction = 1f;
    public Transform body;
    private Queue<CommuterTask> route_TaskList;
    private CommuterTask currentTask;
    public Platform currentPlatform, route_START, route_END;
    public Platform targetPlatform;
    public Platform FinalDestination;
    private Vector3 speed = Vector3.zero;
    private float acceleration;
    private float stateDelay = 0f;
    public Queue<Commuter> currentQueue;
    private int myQueueIndex;
    private int carriageQueueIndex;
    private Walkway currentWalkway;
    public Train currentTrain;
    public TrainCarriage_door currentTrainDoor;
    public CommuterNavPoint currentSeat;
    private int currentTrainCarriageIndex;
    private Transform t;


    private void Awake()
    {
        t = transform;

        // random size
        Vector3 _SCALE = t.localScale;
        _SCALE.y = Random.Range(0.25f, 1.5f);
        t.localScale = _SCALE;

        // random speed
        acceleration = ACCELERATION_STRENGTH * Random.Range(0.8f, 2f);

        // random Colour
        body.GetComponent<Renderer>().material.color =
            new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    public void Init(Platform _platform_START, Platform _platform_DESTINATION)
    {
        currentPlatform = _platform_START;
        FinalDestination = _platform_DESTINATION;

        SetupRoute();
    }


    void Add_TrainConnection(Platform _start, Platform _end)
    {
        route_TaskList.Enqueue(new CommuterTask(CommuterState.QUEUE) {startPlatform = _start});
        route_TaskList.Enqueue(new CommuterTask(CommuterState.GET_ON_TRAIN));
        route_TaskList.Enqueue(new CommuterTask(CommuterState.WAIT_FOR_STOP)
        {
            destinationIndex = _end.point_platform_END.index
        });
        route_TaskList.Enqueue(new CommuterTask(CommuterState.GET_OFF_TRAIN));
    }

    void Add_MetroLineChange()
    {

    }

    void Add_WalkwayConnection(Walkway _walkway)
    {
    }

    void Add_WalkToOppositePlatform(Platform _start, Platform _end)
    {
        route_TaskList.Enqueue(new CommuterTask(CommuterState.WALK) {
            startPlatform =  _start,
            endPlatform = _end,
            destinations = new Vector3[]{
                _start.walkway_FRONT_CROSS.nav_START.transform.position,
                _start.walkway_FRONT_CROSS.nav_END.transform.position,
                _end.walkway_BACK_CROSS.nav_END.transform.position,
                _end.walkway_BACK_CROSS.nav_START.transform.position}
        });
    }

    void SetupRoute()
    {
        route_TaskList = new Queue<CommuterTask>();
        Debug.Log(">> NEW COMMUTER");
        Debug.Log("start: " + currentPlatform.point_platform_END.index);
        Debug.Log("end: " + FinalDestination.point_platform_END.index);
        Platform _CURRENT_PLATFORM = currentPlatform;
        Platform _OPPOSITE_PLATFORM = currentPlatform.oppositePlatform;
        
        Walkway _WALK = _CURRENT_PLATFORM.HasWalkwayTo(FinalDestination);
        if (_WALK != null)
        {
            if (FinalDestination == _OPPOSITE_PLATFORM)
            {
                Debug.Log("Crossing to opposite platform");
                Add_WalkToOppositePlatform(_CURRENT_PLATFORM, FinalDestination);
            }
            else
            {
            Add_WalkwayConnection(_WALK);
            }

        }
        else
        {
            // are the platforms on the same line?
            if (_CURRENT_PLATFORM.parentMetroLine == FinalDestination.parentMetroLine)
            {
                Debug.Log("Origin and Destination are on the same line");
                int stopsFromCurrentPlatform = _CURRENT_PLATFORM.Get_NumberOfStopsTo(FinalDestination); 
                int stopsFromOppositePlatform = _OPPOSITE_PLATFORM.Get_NumberOfStopsTo(FinalDestination);
                Debug.Log("stops from CURRENT platform: " + stopsFromCurrentPlatform);
                Debug.Log("stops from OPPOSITE platform: " + stopsFromOppositePlatform);
                // is it quicker to go forwards or back?
                if ( stopsFromCurrentPlatform > stopsFromOppositePlatform)
                {
                    Debug.Log("Crossing over - shorter journey from opposite platform");
                   Add_WalkToOppositePlatform(_CURRENT_PLATFORM, _OPPOSITE_PLATFORM);
                Add_TrainConnection(_OPPOSITE_PLATFORM, FinalDestination);
                }
                else
                {
                Add_TrainConnection(_CURRENT_PLATFORM, FinalDestination);
                }
            }
            else
            {
                // not on the same line - we need a walkway connection
            }
        }
        NextTask();
    }

    public void BoardTrain(Train _train, TrainCarriage_door _carriageDoor, CommuterNavPoint _assignedSeat)
    {
        currentTrain = _train;
        currentTrainDoor = _carriageDoor;
        currentSeat = _assignedSeat;
        if (currentTask.state == CommuterState.QUEUE)
        {
            NextTask();
        }
    }

    public void UpdateCommuter()
    {
        switch (currentTask.state)
        {
            case CommuterState.WALK:
                if (Approach.Apply(ref t, ref speed, currentTask.destinations[currentTask.destinationIndex],
                    acceleration,
                    ARRIVAL_THRESHOLD, FRICTION))
                {
                    currentTask.destinationIndex++;
                    if (currentTask.destinationIndex > currentTask.destinations.Length - 1)
                    {
                        currentPlatform = currentTask.endPlatform;
                        NextTask();
                    }
                }

                break;
            case CommuterState.QUEUE:

                Vector3 queueOffset = new Vector3(currentQueue.Count * QUEUE_PERSONAL_SPACE, 0f, 0f);
                Vector3 _DEST = currentPlatform.queuePoints[carriageQueueIndex].transform.position + queueOffset;
                if (!currentQueue.Contains(this))
                {
                    if (Approach.Apply(ref t, ref speed, _DEST, acceleration, ARRIVAL_THRESHOLD, FRICTION))
                    {
                        myQueueIndex = currentQueue.Count;
                        currentPlatform.platformQueues[carriageQueueIndex].Enqueue(this);
                    }
                    else
                    {
                        if (Timer.TimerReachedZero(ref stateDelay))
                        {
                            carriageQueueIndex = currentPlatform.Get_ShortestQueue();
                            currentQueue = currentPlatform.platformQueues[carriageQueueIndex];
                            stateDelay = QUEUE_DECISION_RATE;
                        }
                    }
                }

                ;
                break;
            case CommuterState.GET_ON_TRAIN:
                // brief wait before boarding
                if (Timer.TimerReachedZero(ref stateDelay))
                {
                    // walk to each destination in turn (door, seat)
                    if (Approach.Apply(ref t, ref speed, currentTask.destinations[currentTask.destinationIndex],
                        acceleration,
                        ARRIVAL_THRESHOLD, FRICTION))
                    {
                        currentTask.destinationIndex++;
                        // if this is the last destination - go to next task (WAIT_FOR_STOP)
                        if (currentTask.destinationIndex > currentTask.destinations.Length - 1)
                        {
                            currentTrain.Commuter_EMBARK(this, currentTrainCarriageIndex);
                            NextTask();
                        }
                    }
                }

                break;
            case CommuterState.WAIT_FOR_STOP:
                break;
            case CommuterState.GET_OFF_TRAIN:
                // walk to each destination in turn (door, platform)
                if (Approach.Apply(ref t, ref speed, currentTask.destinations[currentTask.destinationIndex],
                    acceleration,
                    ARRIVAL_THRESHOLD, FRICTION))
                {
                    currentTask.destinationIndex++;
                    // if this is the last destination - go to next task
                    if (currentTask.destinationIndex > currentTask.destinations.Length - 1)
                    {
                        currentTrain.Commuter_DISEMBARK(this, currentTrainCarriageIndex);
                        NextTask();
                    }
                }

                break;
        }
    }

    void NextTask()
    {
        if (route_TaskList.Count > 0)
        {
            currentTask = route_TaskList.Dequeue();
            if (currentTask.startPlatform != null)
            {
            currentPlatform = currentTask.startPlatform;
                Debug.Log("Current platform is now: " + currentPlatform.point_platform_END.index);
            }

            switch (currentTask.state)
            {
                case CommuterState.WALK:

                    break;
                case CommuterState.QUEUE:
                    // pick shortest queue
                    stateDelay = QUEUE_DECISION_RATE;
                    carriageQueueIndex = currentPlatform.Get_ShortestQueue();
                    currentQueue = currentPlatform.platformQueues[carriageQueueIndex];
                    myQueueIndex = currentQueue.Count;
                    currentTrainCarriageIndex = carriageQueueIndex;
                    currentTask.destinations = new Vector3[] {currentPlatform.queuePoints[carriageQueueIndex].transform.position};
                    break;
                case CommuterState.GET_ON_TRAIN:
                    // delay movement - stagger by queueIndex
                    stateDelay = QUEUE_MOVEMENT_DELAY * myQueueIndex;
                    currentTask.destinationIndex = 0;
                    currentTask.destinations = new Vector3[]
                    {
                        currentPlatform.queuePoints[carriageQueueIndex].transform.position,
                        currentTrainDoor.door_navPoint.transform.position, currentSeat.transform.position
                    };
                    break;
                case CommuterState.WAIT_FOR_STOP:
                    break;
                case CommuterState.GET_OFF_TRAIN:
                    currentTask.destinationIndex = 0;
                    currentTask.destinationIndex = 0;
                    currentTask.destinations = new Vector3[]
                    {
                        currentTrainDoor.door_navPoint.transform.position,
                        targetPlatform.queuePoints[currentTrainCarriageIndex].transform.position
                    };
                    break;
            }
        }
        else
        {
            Debug.Log("SEE YA");
            Metro.INSTANCE.Remove_Commuter(this);
        }
    }

    public void LeaveTrain()
    {
        if (currentTask.state == CommuterState.WAIT_FOR_STOP)
        {
            NextTask();
        }
    }
}