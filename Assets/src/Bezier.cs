using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class BezierPath
{
    public const int DISTANCE_MEASUREMENT_SUBDIVISIONS = 100;
    public const float HANDLE_STRETCH = 0.01f;
    public const float DISTANCE_SMOOTHING = 1f;
    public List<BezierPoint> points;
    private float pathLength;
    private float distance = 0f;

    public BezierPath()
    {
        points = new List<BezierPoint>();
    }

    public void AddPoint(Vector3 _location)
    {
        points.Add(new BezierPoint(_location, _location, _location, 0f));
        int totalPoints = points.Count;
        if (totalPoints >= 1)
        {
            // Get required points
            BezierPoint _prevPoint = (totalPoints > 1) ? points[totalPoints - 2] : points[0];
            BezierPoint _currentPoint = points[totalPoints - 1];
            BezierPoint _nextPoint = points[0];

            // Get locations
            Vector3 _PREV_POS = _prevPoint.location;
            Vector3 _CURRENT_POS = _currentPoint.location;
            Vector3 _NEXT_POS = _nextPoint.location;

            Vector3 _dist_prev_to_next = (_NEXT_POS - _PREV_POS) / DISTANCE_SMOOTHING;

            _currentPoint.handle_in = _CURRENT_POS - _dist_prev_to_next * HANDLE_STRETCH;
            _currentPoint.handle_out = _CURRENT_POS + _dist_prev_to_next * HANDLE_STRETCH;

            // Measure this new bezier point
            float measurementIncrement = 1f / BezierPath.DISTANCE_MEASUREMENT_SUBDIVISIONS;
            float regionDistance = 0f;
            for (int i = 0; i < BezierPath.DISTANCE_MEASUREMENT_SUBDIVISIONS - 1; i++)
            {
                float _CURRENT_SUBDIV = i * measurementIncrement;
                float _NEXT_SOBDIV = (i + 1) * measurementIncrement;
                regionDistance += Vector3.Distance(BezierLerp(_prevPoint, _currentPoint, _CURRENT_SUBDIV),
                    BezierLerp(_prevPoint, _currentPoint, _NEXT_SOBDIV));
            }

            distance += regionDistance;
            _currentPoint.distanceAlongPath = distance;
        }

        Debug.Log("BezPoint[" + points.Count + "] - starts at " + distance);
        Debug.Log("Total path distance = " + distance);
    }


    public Vector3 Get_NormalAtPosition(float _position)
    {
        Vector3 _current = Get_ProgressPosition(_position);
        Vector3 _ahead = Get_ProgressPosition((_position + 0.0001f) % 1f);
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

    public Vector3 Get_ProgressPosition(float _progress)
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
        float pathProgress_end = point_region_end.distanceAlongPath / distance;
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
    public float distanceAlongPath;

    public BezierPoint(Vector3 _location, Vector3 _handle_in, Vector3 _handle_out, float _distanceAlongPath = 0f)
    {
        location = _location;
        handle_in = _handle_in;
        handle_out = _handle_out;
        distanceAlongPath = _distanceAlongPath;
    }
}