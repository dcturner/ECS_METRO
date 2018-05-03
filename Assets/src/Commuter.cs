using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CommuterState
{
	WALK,
	QUEUE,
	GET_ON_TRAIN,
	GET_OFF_TRAIN
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
	public Vector3 _currentDestination;
	public Walkway _currentWalkway;
	private Queue<CommuterTask> route_TaskList;
	public Platform route_START, route_END;

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
	}

	void SetupRoute()
	{
		// get relevant node indexes
		
		// now fill out the task list in detail
	}

	public void UpdateCommuter()
	{

	}
}
