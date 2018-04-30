using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum PathPointType
{
    PLATFORM
}

public class MetroLine  {
 
    public string lineName;
    public int index;
    public Color lineColour;
    public List<Station> stations;
    public BezierPath bezierPath;
    public List<BezierPoint> PlatformPoints;
    public List<Train> trains;
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

    public void Create_RailPath(Vector3[] _outboundPoints, int[] _designatedPlatforms) {
        
        bezierPath = new BezierPath();
        List<BezierPoint> _POINTS = bezierPath.points;
        int totalPoints = _outboundPoints.Length;
        Vector3 currentLocation = Vector3.zero;
        
        // - - - - - - - - - - - - - - - - - - - - - - - -  OUTBOUND points
        for (int i = 0; i < totalPoints; i++)
        {
            BezierPoint _currentPoint = bezierPath.AddPoint(_outboundPoints[i]);
            if (_designatedPlatforms.Contains(i))
            {
                _currentPoint.tags.Add(PathPointType.PLATFORM.ToString());
            }

        }
        // fix the OUTBOUND handles
        for (int i = 0; i <= totalPoints-1; i++)
        {
            BezierPoint _currentPoint = _POINTS[i];
            if (i == 0)
            {
                _currentPoint.SetHandles(_POINTS[1].location - _currentPoint.location);
            }else if (i == totalPoints - 1)
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
        for (int i = totalPoints - 1; i >=0; i--)
        {
            Vector3 _targetLocation = bezierPath.GetPoint_PerpendicularOffset(bezierPath.points[i], (i==totalPoints-1) ? platformOffset : platformOffset);
            bezierPath.AddPoint(_targetLocation);
            _RETURN_POINTS.Add(_POINTS[_POINTS.Count-1]);
        }
        
        // fix the RETURN handles
        for (int i = 0; i <= totalPoints-1; i++)
        {
            BezierPoint _currentPoint = _RETURN_POINTS[i];
            if (i == 0)
            {
                _currentPoint.SetHandles(_RETURN_POINTS[1].location - _currentPoint.location);
            }else if (i == totalPoints - 1)
            {
                _currentPoint.SetHandles(_currentPoint.location - _RETURN_POINTS[i-1].location);
            }
            else
            {
                _currentPoint.SetHandles(_RETURN_POINTS[i+1].location - _RETURN_POINTS[i-1].location);
            }
        }
        bezierPath.MeasurePath();
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
