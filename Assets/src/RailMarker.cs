using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class RailMarker : MonoBehaviour
{

	public int metroLineID;
	public int pointIndex;
	public bool isPlatform = false;
	[HideInInspector]
	public Material mat;
	private Transform t;
	
	void Awake ()
	{
		t = transform;
		mat = GetComponent<Renderer>().sharedMaterial;
	}

	void Update()
	{
		Colour.Set(ref mat, Metro.GetLine_COLOUR_FromIndex(metroLineID));
	}

	public void OnDrawGizmos()
	{
//		GUI.color = mat.color;
		GUI.Label (new Rect (100, 100, 400, 30), "Connected to server");
//		Handles.Label(transform.position + new Vector3(2f,0f,0f), metroLineID+"_"+pointIndex);
//		Handles.Label(t.position + new Vector3(2f,0f,0f), "TEST TEST TEST TEST");
	}
}
