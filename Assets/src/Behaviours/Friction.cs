using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Friction {

	public static void Apply(ref float _current, float _friction)
	{
		_current *= _friction;
	}
	
	public static void Apply(ref Vector3 _current, Vector3 _friction)
	{
		_current = Vector3.Scale(_current, _friction);
	}
}
