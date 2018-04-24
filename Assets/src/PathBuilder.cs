using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathBuilder: MonoBehaviour {

    public GameObject prefab_rail;
    public BezierPath path;
    public int totalPoints = 10;
    public float handleStretch = 0.5f;
    public float distanceSmoothingRatio = 0.5f;

	void Start () {
        
        path = new BezierPath();
        Vector3 currentLocation = Vector3.zero;
        path.AddPoint(currentLocation, Vector3.zero, Vector3.zero);
        // create random points
        for (int i = 1; i < totalPoints; i++)
        {
            currentLocation.x += Random.Range(-3f, 3f);
            currentLocation.z += Random.Range(-3f, 3f);
            path.AddPoint(currentLocation, currentLocation, currentLocation);
        }
        // make the handles nicer
        // start
        BezierPoint bz_START = path.points[0];
        bz_START.handle_in = bz_START.location;
        bz_START.handle_out = Vector3.Lerp(bz_START.location, path.points[1].location, handleStretch);
        // end
        BezierPoint bz_END = path.points[totalPoints-1];
        bz_END.handle_in = Vector3.Lerp(bz_END.location, path.points[totalPoints - 2].location, handleStretch);
        bz_END.handle_out = bz_END.location;
        for (int i = 1; i < totalPoints-1; i++)
        {
            BezierPoint _CURRENT_POINT = path.points[i];
            Vector3 _PREV_POS       = path.points[i - 1].location;
            Vector3 _CURRENT_POS    = _CURRENT_POINT.location;
            Vector3 _NEXT_POS       = path.points[i + 1].location;

            Vector3 _dist_prev_to_next = (_NEXT_POS - _PREV_POS) / distanceSmoothingRatio;

            _CURRENT_POINT.handle_in    = _CURRENT_POS - _dist_prev_to_next * handleStretch;
            _CURRENT_POINT.handle_out   = _CURRENT_POS + _dist_prev_to_next * handleStretch;
        }

        // connect the locations


	}
	public void OnGUI()
	{
        
	}
	public void OnDrawGizmos()
	{
        if (path != null)
        {
            for (int i = 0; i < totalPoints; i++)
            {
                BezierPoint _CURRENT_POINT = path.points[i];

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(_CURRENT_POINT.location, Vector3.one * 0.2f);

                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(_CURRENT_POINT.handle_in, Vector3.one * 0.05f);
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(_CURRENT_POINT.handle_out, Vector3.one * 0.05f);

                if (i < totalPoints-1 )
                {
                    BezierPoint _NEXT_POINT = path.points[i + 1];
                    // Link them up
                    Handles.DrawBezier(_CURRENT_POINT.location, _NEXT_POINT.location, _CURRENT_POINT.handle_out, _NEXT_POINT.handle_in, Color.cyan,null , 3f);
                }
            }
        }
        

	}
}
