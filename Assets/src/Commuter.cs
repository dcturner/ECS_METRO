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
}

public class Commuter : MonoBehaviour
{

	private Queue<CommuterTask> route_TaskList;
	public Platform route_START, route_END;

	void EstablishRoute()
	{
		// get relevant node indexes
		
		// now fill out the task list in detail
	}
}
