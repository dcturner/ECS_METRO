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
        int totalPoints = _outboundPoints.Length;
        Vector3 currentLocation = Vector3.zero;
        // create OUT points
        for (int i = 0; i < totalPoints; i++)
        {
            BezierPoint _currentPoint = bezierPath.AddPoint(_outboundPoints[i]);
            if (_designatedPlatforms.Contains(i))
            {
                _currentPoint.tags.Add(PathPointType.PLATFORM.ToString());
            }

        }
        // create matching RETURN points
        float platformOffset = Metro.BEZIER_PLATFORM_OFFSET;
        for (int i = totalPoints - 1; i >=0; i--)
        {
            Vector3 _targetLocation = bezierPath.GetPoint_PerpendicularOffset(bezierPath.points[i], (i==totalPoints-1) ? -platformOffset : platformOffset);
            bezierPath.AddPoint(_targetLocation);
        }

        bezierPath.CloseLoop();
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
