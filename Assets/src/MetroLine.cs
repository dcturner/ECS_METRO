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
    public Vector3[] railPath;

    public MetroLine(int _index)
    {
        index = _index;
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
        for (int i = totalPoints - 1; i >=0; i--)
        {
            Vector3 _targetLocation = bezierPath.GetPoint_PerpendicularOffset(bezierPath.points[i], (i==totalPoints-1) ? -1f : 1f);
            bezierPath.AddPoint(_targetLocation);
        }

        bezierPath.CloseLoop();
    }
    
}
