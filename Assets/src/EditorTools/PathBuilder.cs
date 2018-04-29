using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;

public class PathBuilder: MonoBehaviour {

    public GameObject prefab_rail;
    public BezierPath path;
    public int totalPoints = 10;
    public float handleStretch = 0.5f;
    public float distanceSmoothingRatio = 0.5f;
    [Range(0f, 1f)]
    public float currentLocationOnPath = 0f;
    [Range(0f, 0.1f)]
    public float speed = 0.001f;

	public float trainLineLength = 1f;

	public void CreateLine() {
        
        path = new BezierPath();
        Vector3 currentLocation = Vector3.zero;
        // create OUT points
        for (int i = 0; i < totalPoints; i++)
        {
Vector3 _POS = new Vector3(Mathf.Lerp(0f, 5f, (float)i/totalPoints) * 5f, 0f, Random.Range(-1f, 1f));
            path.AddPoint(_POS);
	        
        }
	    // create matching RETURN points
	    for (int i = totalPoints - 1; i >=0; i--)
	    {
		    Vector3 _targetLocation = path.GetPoint_PerpendicularOffset(path.points[i], (i==totalPoints-1) ? -1f : 1f);
	        path.AddPoint(_targetLocation);
	    }

		path.CloseLoop();
	}
	
	private void Update()
	{
        currentLocationOnPath = (currentLocationOnPath + speed) % 1;
	}
	public void OnDrawGizmos()
	{
        if (path != null)
        {
            for (int i = 0; i < path.points.Count; i++)
            {
                BezierPoint _CURRENT_POINT = path.points[i];

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_CURRENT_POINT.location, 0.1f);

                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(_CURRENT_POINT.handle_in, Vector3.one * 0.025f);
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(_CURRENT_POINT.handle_out, Vector3.one * 0.025f);

                BezierPoint _NEXT_POINT = path.points[(i + 1) % path.points.Count];
                // Link them up
                Handles.DrawBezier(_CURRENT_POINT.location, _NEXT_POINT.location, _CURRENT_POINT.handle_out, _NEXT_POINT.handle_in, Color.cyan,null , 3f);
                
            }
        float gap = 1f / 10f;
        for (int i = 0; i < 10; i++)
        {
	        float iProgress = (currentLocationOnPath + (i * gap)) % 1f;
            Vector3 _POS = path.Get_Position(iProgress);
	        Gizmos.color = Color.yellow;
	        Gizmos.DrawCube(_POS, Vector3.one * 0.25f);

        }
        }
        

	}
}
