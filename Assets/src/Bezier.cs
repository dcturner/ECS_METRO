using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierPath
{
    public List<BezierPoint> points;
    private float pathLength;
    private float distance = 0f;

    public BezierPath()
    {
        points = new List<BezierPoint>();
    }

    public BezierPoint AddPoint(Vector3 _location)
    {
        BezierPoint result = new BezierPoint(_location, _location, _location);
        points.Add(result);
        if (points.Count > 1)
        {
            BezierPoint _prev = points[points.Count - 2];
            BezierPoint _current = points[points.Count - 1];
            SetHandles(_current, _prev.location);
            SetPointDistance(_current, _prev);
        }

        return result;
    }

    void SetHandles(BezierPoint _point, Vector3 _prevPointLocation)
    {
        Vector3 _pointLocation = _point.location;
        Vector3 _dist_prev_to_next = (_point.location - _prevPointLocation) / Metro.BEZIER_DISTANCE_SMOOTHING;

        _point.handle_in = _pointLocation - (_dist_prev_to_next * Metro.BEZIER_HANDLE_REACH);
        _point.handle_out = _pointLocation + (_dist_prev_to_next * Metro.BEZIER_HANDLE_REACH);     
    }

    public void SetPointDistance(BezierPoint _currentPoint, BezierPoint _prevPoint) {
            // Measure this new bezier point
            float measurementIncrement = 1f / Metro.BEZIER_MEASUREMENT_SUBDIVISIONS;
            float regionDistance = 0f;
            for (int i = 0; i < Metro.BEZIER_MEASUREMENT_SUBDIVISIONS- 1; i++)
            {
                float _CURRENT_SUBDIV = i * measurementIncrement;
                float _NEXT_SOBDIV = (i + 1) * measurementIncrement;
                regionDistance += Vector3.Distance(BezierLerp(_prevPoint, _currentPoint, _CURRENT_SUBDIV),
                    BezierLerp(_prevPoint, _currentPoint, _NEXT_SOBDIV));
            }

            distance += regionDistance;
            _currentPoint.distanceAlongPath = distance;
    }

    // Add the final region distance (from last point back to first point
    public void CloseLoop()
    {
        // easy acces vars
        int _FINAL_INDEX = points.Count - 1;
        BezierPoint _firstPoint = points[0];
        Vector3 _firstPoint_POS = _firstPoint.location;
        BezierPoint _secondPoint = points[1];
        Vector3 _secondPoint_POS = _secondPoint.location;
        BezierPoint _lastPoint = points[_FINAL_INDEX];
        
        
        // Fix the start's handles
        Vector3 dist_p0_to_p1 = _secondPoint_POS - _firstPoint_POS;
        SetHandles(_firstPoint, _firstPoint_POS - dist_p0_to_p1);
        // Fix the returnStart's handles
        int index_returnOne = Mathf.FloorToInt(points.Count / 2);
        BezierPoint _returnOnePoint = points[index_returnOne];
        BezierPoint _returnTwoPoint = points[index_returnOne+1];
        Vector3 _returnOne_POS = _returnOnePoint.location;
        Vector3 dist_r0_to_r1 = points[index_returnOne+1].location - _returnOne_POS;
        SetHandles(_returnOnePoint, _returnOne_POS - dist_r0_to_r1);
         
        
        // add final region distance (END to START)
        float measurementIncrement = 1f / Metro.BEZIER_MEASUREMENT_SUBDIVISIONS;
        float regionDistance = 0f;
        for (int i = 0; i < Metro.BEZIER_MEASUREMENT_SUBDIVISIONS - 1; i++)
        {
            float _CURRENT_SUBDIV = i * measurementIncrement;
            float _NEXT_SOBDIV = (i + 1) * measurementIncrement;
            regionDistance += Vector3.Distance(BezierLerp(_lastPoint,_firstPoint, _CURRENT_SUBDIV),
                BezierLerp(_lastPoint, _firstPoint, _NEXT_SOBDIV));
        }
        distance += regionDistance;
    }

    public Vector3 Get_NormalAtPosition(float _position)
    {
        Vector3 _current = Get_Position(_position);
        Vector3 _ahead = Get_Position((_position + 0.0001f) % 1f);
        return (_ahead - _current) / Vector3.Distance(_ahead, _current);
    }

    public Vector3 Get_TangentAtPosition(float _position)
    {
        Vector3 normal = Get_NormalAtPosition(_position);
        return new Vector3(-normal.z, normal.y, normal.x);
    }

    public Vector3 GetPoint_PerpendicularOffset(BezierPoint _point, float _offset)
    {
        return _point.location + Get_TangentAtPosition(_point.distanceAlongPath / distance) * _offset;
    }

    public Vector3 Get_Position(float _progress)
    {
        int totalPoints = points.Count;

        float progressDistance = distance * _progress;

        // Figure out the wrapper points
        BezierPoint point_region_start = points[0];
        BezierPoint point_region_end = points[1];
        int pointIndex_region_start = 0;
        int pointIndex_region_end = 0;
        for (int i = 0; i < totalPoints; i++)
        {
            BezierPoint _PT = points[pointIndex_region_start];
            if (_PT.distanceAlongPath <= _progress)
            {
                if (i == totalPoints - 1)
                {
                    // end wrap
                    pointIndex_region_start = i;
                    pointIndex_region_end = 0;
                    break;
                }
                else if (points[i + 1].distanceAlongPath >= progressDistance)
                {
                    // start < progress, end > progress <-- thats a match
                    pointIndex_region_start = i;
                    pointIndex_region_end = i + 1;
                    break;
                }
                else
                {
                    continue;
                }
            }
        }

        // get start and end bex points
        point_region_start = points[pointIndex_region_start];
        point_region_end = points[pointIndex_region_end];
        // lerp between the points to arrive at PROGRESS
        float pathProgress_start = point_region_start.distanceAlongPath / distance;
        float pathProgress_end = (pointIndex_region_end != 0) ?  point_region_end.distanceAlongPath / distance : 1f;
        float regionProgress = (_progress - pathProgress_start) / (pathProgress_end - pathProgress_start);

        // do your bezier lerps
        // Round 1 --> Origins to handles, handle to handle
        return BezierLerp(point_region_start, point_region_end, regionProgress);
    }

    public Vector3 BezierLerp(BezierPoint _pointA, BezierPoint _pointB, float _progress)
    {
        // Round 1 --> Origins to handles, handle to handle
        Vector3 l1_a_aOUT = Vector3.Lerp(_pointA.location, _pointA.handle_out, _progress);
        Vector3 l2_bIN_b = Vector3.Lerp(_pointB.handle_in, _pointB.location, _progress);
        Vector3 l3_aOUT_bIN = Vector3.Lerp(_pointA.handle_out, _pointB.handle_in, _progress);
        // Round 2 
        Vector3 l1_to_l3 = Vector3.Lerp(l1_a_aOUT, l3_aOUT_bIN, _progress);
        Vector3 l3_to_l2 = Vector3.Lerp(l3_aOUT_bIN, l2_bIN_b, _progress);
        // Final Round
        Vector3 result = Vector3.Lerp(l1_to_l3, l3_to_l2, _progress);
        return result;
    }

    public float GetPathDistance()
    {
        return distance;
    }
}

public class BezierPoint
{
    public Vector3 location, handle_in, handle_out;
    public float distanceAlongPath = 0f;
    public List<string> tags;

    public BezierPoint(Vector3 _location, Vector3 _handle_in, Vector3 _handle_out)
    {
        location = _location;
        handle_in = _handle_in;
        handle_out = _handle_out;
        tags = new List<string>();
    }
}