using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public int carriageCount;
    public MetroLine parentMetroLine;
    public List<Walkway> walkways;
    public Walkway walkway_FRONT_CROSS, walkway_BACK_CROSS, walkway_UP, walkway_DOWN;
    public BezierPoint point_platform_START, point_platform_END;
    public Platform oppositePlatform;
    public Queue<Commuter>[] platformQueues;
    public CommuterNavPoint[] queuePoints;
    public Train currentTrainAtPlatform;

    public void SetupPlatform(MetroLine _parentMetroLine, BezierPoint _start, BezierPoint _end)
    {
        parentMetroLine = _parentMetroLine;
        point_platform_START = _start;
        point_platform_END = _end;
        carriageCount = parentMetroLine.carriagesPerTrain;
        SetColour();
        
        // setup queue lists and spacing
        platformQueues = new Queue<Commuter>[carriageCount];
        for (int i = 0; i < carriageCount; i++)
        {
            platformQueues[i] = new Queue<Commuter>();
        }
        Setup_Walkways();
    }

    public void Setup_Walkways()
    {
        foreach (Walkway _WALKWAY in GetComponentsInChildren<Walkway>())
        {
            _WALKWAY.connects_FROM = this;
        }
    }
    
    public void PairWithOppositePlatform(Platform _oppositePlatform)
    {
        oppositePlatform = _oppositePlatform;
        walkway_FRONT_CROSS.connects_TO = oppositePlatform;
        walkway_BACK_CROSS.connects_TO = oppositePlatform;
    }

    public void SetColour()
    {
        Color _LINE_COLOUR = parentMetroLine.lineColour;
        Colour.RecolourChildren(walkway_FRONT_CROSS.transform, _LINE_COLOUR);
        Colour.RecolourChildren(walkway_BACK_CROSS.transform, _LINE_COLOUR);
        Colour.RecolourChildren(walkway_UP.transform, _LINE_COLOUR);
        Colour.RecolourChildren(walkway_DOWN.transform, _LINE_COLOUR);
    }

    public int Get_NumberOfStopsTo(Platform _destination)
    {
        int count = 1;
        int startIndex = point_platform_END.index;
        int targetIndex = _destination.point_platform_END.index;
        BezierPath _PATH = parentMetroLine.bezierPath;
        int totalPoints = _PATH.points.Count;
        for (int i = 1; i < totalPoints; i++)
        {
            int _TEST_INDEX = (startIndex + i) % totalPoints;
            if (parentMetroLine.IsPlatformEndPoint(_TEST_INDEX))
            {
                count++;
                if (_TEST_INDEX == targetIndex)
                {
                    break;
                }
            }
        }

        return count;
    }
    
    public Walkway HasWalkwayTo(Platform _platform)
    {
        foreach (Walkway _WALKWAY in walkways)
        {
            if (_WALKWAY.connects_TO == _platform)
            {
                return _WALKWAY;
            }
        }

        return null;
    }

    public int Get_ShortestQueue()
    {
        int shortest = 0;
        // if all are the same length, go for the second
        int queueLength = 999;
        for (int i = 0; i < carriageCount; i++)
        {
            if (platformQueues[i].Count < queueLength)
            {
                queueLength = platformQueues[i].Count;
                shortest = i;
            }
        }
        
        if (queueLength == 0)
        {
            // all are zero - do something random maybe?
            shortest =  Mathf.FloorToInt(Random.Range(0, carriageCount));
        }

        return shortest;
    }
}
