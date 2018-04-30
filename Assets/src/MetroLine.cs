﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;



public class MetroLine  {
 
    public string lineName;
    public int index;
    public Color lineColour;
    public BezierPath bezierPath;
    public List<Train> trains;
    public List<Platform> platforms;
    public int maxTrains;
    public Vector3[] railPath;
    public int carriagesPerTrain;
    public float trainCarriageSpacing = 0.1f;
    public float train_accelerationStrength = 0.0003f;
    public float train_brakeStrength = 0.01f;
    public float train_friction = 0.95f;

    public MetroLine(int _index, int _maxTrains)
    {
        index = _index;
        maxTrains = _maxTrains;
        trains = new List<Train>();
        Update_ValuesFromMetro();
    }

    void Update_ValuesFromMetro()
    {
        Metro m = Metro.INSTANCE;
        lineName = m.LineNames[index];
        lineColour = m.LineColours[index];
        carriagesPerTrain = m.carriagesPerTrain[index];
        if (carriagesPerTrain <= 0)
        {
            carriagesPerTrain = 1;
        }

        trainCarriageSpacing = m.trainCarriageSpacing[index];

//        train_accelerationStrength = m.LineNames[index];
//        train_brakeStrength= m.LineNames[index];
//        train_friction= m.LineNames[index];
    }

    public void Create_RailPath(List<RailMarker> _outboundPoints) {
        
        bezierPath = new BezierPath();
        List<BezierPoint> _POINTS = bezierPath.points;
        int total_outboundPoints = _outboundPoints.Count;
        Vector3 currentLocation = Vector3.zero;
        
        // - - - - - - - - - - - - - - - - - - - - - - - -  OUTBOUND points
        for (int i = 0; i < total_outboundPoints; i++)
        {
            BezierPoint _currentPoint = bezierPath.AddPoint(_outboundPoints[i].transform.position);
        }
        // fix the OUTBOUND handles
        for (int i = 0; i <= total_outboundPoints-1; i++)
        {
            BezierPoint _currentPoint = _POINTS[i];
            if (i == 0)
            {
                _currentPoint.SetHandles(_POINTS[1].location - _currentPoint.location);
            }else if (i == total_outboundPoints - 1)
            {
                _currentPoint.SetHandles(_currentPoint.location - _POINTS[i-1].location);
            }
            else
            {
                _currentPoint.SetHandles(_POINTS[i+1].location - _POINTS[i-1].location);
            }
        }
        bezierPath.MeasurePath();
        
        // - - - - - - - - - - - - - - - - - - - - - - - -  RETURN points
        float platformOffset = Metro.BEZIER_PLATFORM_OFFSET;
        List<BezierPoint> _RETURN_POINTS = new List<BezierPoint>();
        for (int i = total_outboundPoints - 1; i >=0; i--)
        {
            Vector3 _targetLocation = bezierPath.GetPoint_PerpendicularOffset(bezierPath.points[i], platformOffset);
            bezierPath.AddPoint(_targetLocation);
            _RETURN_POINTS.Add(_POINTS[_POINTS.Count-1]);
        }
        
        // fix the RETURN handles
        for (int i = 0; i <= total_outboundPoints-1; i++)
        {
            BezierPoint _currentPoint = _RETURN_POINTS[i];
            if (i == 0)
            {
                _currentPoint.SetHandles(_RETURN_POINTS[1].location - _currentPoint.location);
            }else if (i == total_outboundPoints - 1)
            {
                _currentPoint.SetHandles(_currentPoint.location - _RETURN_POINTS[i-1].location);
            }
            else
            {
                _currentPoint.SetHandles(_RETURN_POINTS[i+1].location - _RETURN_POINTS[i-1].location);
            }
        }
        bezierPath.MeasurePath();
        
        // now that the rails have been laid - let's put the platforms on
        int totalPoints = bezierPath.points.Count;
        for (int i = 1; i < _outboundPoints.Count; i++)
        {
            if (_outboundPoints[i].railMarkerType == RailMarkerType.PLATFORM_END &&
                _outboundPoints[i-1].railMarkerType == RailMarkerType.PLATFORM_START)
            {
                Platform _ouboundPlatform = AddPlatform(i-1, i);
                // now add an opposite platform!
                int opposite_START = totalPoints - (i-1);
                int opposite_END = totalPoints - i;
                Platform _opposirePlatform = AddPlatform(opposite_START, opposite_END);
                _opposirePlatform.transform.eulerAngles = _ouboundPlatform.transform.rotation.eulerAngles + new Vector3(0f, 180f, 0f);;
            }
        }
    }

    Platform AddPlatform(int _index_platform_START, int _index_platform_END)
    {
        BezierPoint _PT_START = bezierPath.points[_index_platform_START];
        BezierPoint _PT_END = bezierPath.points[_index_platform_END];
        GameObject platform_OBJ = (GameObject) Metro.Instantiate(Metro.INSTANCE.prefab_platform, _PT_END.location, Quaternion.identity);
        Platform platform = platform_OBJ.GetComponent<Platform>();
        platform.point_platform_START = _PT_START;
        platform.point_platform_END = _PT_END;
        platform_OBJ.transform.LookAt(bezierPath.GetPoint_PerpendicularOffset(_PT_END, -3f));
        platform.parentMetroLine = this;
        platform.SetColour();
        return platform;
    }

    public void AddTrain(float _position)
    {
        trains.Add(new Train(index, _position, carriagesPerTrain));
    }

    public void UpdateTrains()
    {
        float _carriageSpacing = Get_distanceAsRailProportion(trainCarriageSpacing);
        foreach (Train _t in trains)
        {
            _t.Update(_carriageSpacing);
        }
    }

    public Vector3 Get_PositionOnRail(float _pos)
    {
        return bezierPath.Get_Position(_pos);
    }
    public Vector3 Get_RotationOnRail(float _pos)
    {
        return bezierPath.Get_NormalAtPosition(_pos);
    }

    public float Get_distanceAsRailProportion (float _realDistance)
    {
        return _realDistance / bezierPath.GetPathDistance();
    }
}
