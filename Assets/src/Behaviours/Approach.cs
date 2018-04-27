using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Approach {

	public static bool Apply(ref float _current, ref float _speed, float _target, float _acceleration, float _brake, float _arrivalThreshold)
	{
		if (_current < (_target - _arrivalThreshold))
		{
			_speed += _acceleration;
			_current += _speed;
			return false;
		}else if (_current > (_target + _arrivalThreshold))
		{
			_speed -= _brake;
			_current += _speed;
			return false;
		}
		else
		{
			return true;
		}
	}

	public static bool Apply(ref Vector3 _current, ref Vector3 _speed, Vector3 _target, Vector3 _acceleration,
		Vector3 _brake, Vector3 _arrivalThreshold)
	{
		bool arrivedX = Approach.Apply(ref _current.x, ref _speed.x, _target.x, _acceleration.x, _brake.x, _arrivalThreshold.x);
		bool arrivedY = Approach.Apply(ref _current.y, ref _speed.y, _target.y, _acceleration.y, _brake.y, _arrivalThreshold.y);
		bool arrivedZ = Approach.Apply(ref _current.z, ref _speed.z, _target.z, _acceleration.z, _brake.z, _arrivalThreshold.z);
		
		return (arrivedX && arrivedY && arrivedZ);
	}
}
