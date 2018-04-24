using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierPath {

    public List<BezierPoint> points;
    private float pathLength;
    private float distance = 0f;
    public BezierPath(){
        points = new List<BezierPoint>();
    }
    public void AddPoint(Vector3 _location, Vector3 _handle_in, Vector3 _handle_out){
        points.Add(new BezierPoint(_location, _handle_in, _handle_out));
        if(points.Count > 1){
            distance += Vector3.Distance(points[points.Count - 2].location, _location);
            Debug.Log("New Path Distance: " + distance);
        }
    }
    public Vector3 GetPosition(float _progress){
        return new Vector3();
    }
    public float GetPathDistance(){
        return distance;
    }
}
public class BezierPoint {
    public Vector3 location, handle_in, handle_out;
    public BezierPoint (Vector3 _location, Vector3 _handle_in, Vector3 _handle_out){
        location = _location;
        handle_in = _handle_in;
        handle_out = _handle_out;
    }
}
