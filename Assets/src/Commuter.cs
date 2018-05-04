using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CommuterState
{
    WALK,
    QUEUE,
    GET_ON_TRAIN,
    GET_OFF_TRAIN,
    WAIT_FOR_STOP,
}

public struct CommuterTask
{
    public CommuterState state;
    public Vector3 destination;

    public CommuterTask(CommuterState _state, Vector3 _destination)
    {
        state = _state;
        destination = _destination;
    }
}

public class Commuter : MonoBehaviour
{
    public const float ACCELERATION_STRENGTH = 0.01f;
    public const float FRICTION = 0.8f;
    public const float ARRIVAL_THRESHOLD = 0.02f;

    public Walkway _currentWalkway;
    private Queue<CommuterTask> route_TaskList;
    private CommuterTask currentTask;
    public Platform route_START, route_END;

    private Vector3 speed = Vector3.zero;
    private Transform t;


    private void Awake()
    {
        t = transform;
    }

    public void Init(Platform _currentPlatform, Walkway _currentWalkway)
    {
        route_TaskList = new Queue<CommuterTask>();
        // visit everything on this platform

        route_TaskList.Enqueue(new CommuterTask(CommuterState.WALK, _currentWalkway.nav_TOP.transform.position));
        route_TaskList.Enqueue(new CommuterTask(CommuterState.WALK, _currentWalkway.nav_BOTTOM.transform.position));
        route_TaskList.Enqueue(new CommuterTask(CommuterState.WALK,
            _currentPlatform.stairs_BACK_CROSS.nav_BOTTOM.transform.position));
        route_TaskList.Enqueue(new CommuterTask(CommuterState.WALK,
            _currentPlatform.stairs_BACK_CROSS.nav_TOP.transform.position));
        route_TaskList.Enqueue((new CommuterTask(CommuterState.WALK, _currentPlatform.queuePoints[0])));
        route_TaskList.Enqueue((new CommuterTask(CommuterState.WALK, _currentPlatform.queuePoints[1])));
        route_TaskList.Enqueue((new CommuterTask(CommuterState.WALK, _currentPlatform.queuePoints[2])));

        currentTask = route_TaskList.Dequeue();
    }

    void SetupRoute()
    {
        // get relevant node indexes

        // now fill out the task list in detail
    }

    public void UpdateCommuter()
    {
        switch (currentTask.state)
        {
            case CommuterState.WALK:
                if (Approach.Apply(ref t, ref speed, currentTask.destination, ACCELERATION_STRENGTH,
                    ARRIVAL_THRESHOLD, FRICTION))
                {
                    NextTask();
                }
                break;
            case CommuterState.QUEUE:
                break;
            case CommuterState.GET_ON_TRAIN:
                break;
            case CommuterState.GET_OFF_TRAIN:
                break;
            case CommuterState.WAIT_FOR_STOP:
                break;
        }
    }

    void NextTask()
    {
        if (route_TaskList.Count > 0)
        {
            currentTask = route_TaskList.Dequeue();
        }
        else
        {
            Debug.Log("I'm DONE - SEE YA");
            GameObject.Destroy(gameObject);
        }
    }
}