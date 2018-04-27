using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colour {

	public static void Set(ref Material _material, Color _newColour)
	{
		_material.color = _newColour;
	}
}
