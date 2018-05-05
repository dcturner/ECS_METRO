using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CommuterState
{
    USE_WALKWAY,
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

    public CommuterTask(CommuterState _state)
    {
        state = _state;
    }
}

public class Commuter : MonoBehaviour
{
    public const float ACCELERATION_STRENGTH = 0.01f;
    public const float FRICTION = 0.8f;
    public const float ARRIVAL_THRESHOLD = 0.02f;
    public const float QUEUE_PERSONAL_SPACE = 0.4f;
    public const float QUEUE_MOVEMENT_DELAY = 0.25f;

    public Transform body;
    private float mySpeedFactor = 1f;
    private Queue<CommuterTask> route_TaskList;
    private CommuterTask currentTask;
    public Platform currentPlatform, route_START, route_END;
    public Platform targetPlatform;
    private Vector3 speed = Vector3.zero;
    private float acceleration;
    private float stateDelay = 0f;
    public Queue<Commuter> currentQueue;
    private int myQueueIndex;
    private int currentPlatformQueueIndex;
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
        acceleration = ACCELERATION_STRENGTH * Random.Range(0.5f, 2f);

        // random Colour
        body.GetComponent<Renderer>().material.color =
            new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    public void Init(Platform _currentPlatform, Walkway _currentWalkway)
    {
        currentPlatform = _currentPlatform;
        currentWalkway = _currentWalkway;
        route_TaskList = new Queue<CommuterTask>();

        // down stairs
        route_TaskList.Enqueue(new CommuterTask(CommuterState.USE_WALKWAY)
        {
            destinations = new Vector3[]
                {_currentWalkway.nav_END.transform.position, _currentWalkway.nav_START.transform.position}
        });
        // Queue for train
        Add_TrainConnection(_currentPlatform.parentMetroLine.platforms[2]);

        currentTask = route_TaskList.Dequeue();
    }

    void Add_TrainConnection(Platform _destinationPlatform)
    {
        int _targetPlatformIndex = _destinationPlatform.parentMetroLine.platforms.IndexOf(_destinationPlatform);
        route_TaskList.Enqueue(new CommuterTask(CommuterState.QUEUE) {destinationIndex = _targetPlatformIndex});
        route_TaskList.Enqueue(new CommuterTask(CommuterState.GET_ON_TRAIN));
        route_TaskList.Enqueue(new CommuterTask(CommuterState.WAIT_FOR_STOP)
        {
            destinationIndex = _destinationPlatform.point_platform_END.index
        });
        route_TaskList.Enqueue(new CommuterTask(CommuterState.GET_OFF_TRAIN));
    }

    void SetupRoute()
    {
        // get relevant node indexes

        // now fill out the task list in detail
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
            case CommuterState.USE_WALKWAY:
                if (Approach.Apply(ref t, ref speed, currentTask.destinations[currentTask.destinationIndex],
                    acceleration,
                    ARRIVAL_THRESHOLD, FRICTION))
                {
                    currentTask.destinationIndex++;
                    if (currentTask.destinationIndex > currentTask.destinations.Length - 1)
                    {
                        NextTask();
                    }
                }

                break;
            case CommuterState.QUEUE:

                Vector3 queueOffset = new Vector3(currentQueue.Count * QUEUE_PERSONAL_SPACE, 0f, 0f);
                Vector3 _DEST = currentPlatform.queuePoints[currentPlatformQueueIndex] + queueOffset;
                if (!currentQueue.Contains(this))
                {
                    if (Approach.Apply(ref t, ref speed, _DEST, acceleration, ARRIVAL_THRESHOLD, FRICTION))
                    {
                        myQueueIndex = currentQueue.Count;
                        currentPlatform.platformQueues[currentPlatformQueueIndex].Enqueue(this);
                        Debug.Log("joined queue: " + currentPlatformQueueIndex + ", length: " + currentQueue.Count +
                                  ", myQueueIndex: " + myQueueIndex + ",  offset: " + queueOffset);
                    }
                    else
                    {
                        if (Timer.TimerReachedZero(ref stateDelay))
                        {
                            currentPlatformQueueIndex = currentPlatform.Get_ShortestQueue();
                            currentQueue = currentPlatform.platformQueues[currentPlatformQueueIndex];
                            stateDelay = 0.5f;
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
                currentTrain.carriages[currentTrainCarriageIndex].VacateSeat(currentSeat);
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
            switch (currentTask.state)
            {
                case CommuterState.USE_WALKWAY:

                    break;
                case CommuterState.QUEUE:
                    // pick shortest queue
                    stateDelay = 0.1f;
                    targetPlatform = currentPlatform.parentMetroLine.platforms[currentTask.destinationIndex];
                    currentPlatformQueueIndex = currentPlatform.Get_ShortestQueue();
                    currentQueue = currentPlatform.platformQueues[currentPlatformQueueIndex];
                    myQueueIndex = currentQueue.Count;
                    currentTrainCarriageIndex = currentPlatformQueueIndex;
                    currentTask.destinations = new Vector3[] {currentPlatform.queuePoints[currentPlatformQueueIndex]};
                    break;
                case CommuterState.GET_ON_TRAIN:
                    // delay movement - stagger by queueIndex
                    stateDelay = QUEUE_MOVEMENT_DELAY * myQueueIndex;
                    currentTask.destinationIndex = 0;
                    currentTask.destinations = new Vector3[]
                    {
                        currentPlatform.queuePoints[currentPlatformQueueIndex],
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
                        targetPlatform.queuePoints[currentTrainCarriageIndex]
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