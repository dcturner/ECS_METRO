using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public int carriageCount, platformIndex;
    public MetroLine parentMetroLine;
    public List<Walkway> walkways;
    public Walkway walkway_FRONT_CROSS, walkway_BACK_CROSS;
    public BezierPoint point_platform_START, point_platform_END;
    public Platform oppositePlatform;
    public Platform nextPlatform;
    public List<Platform> adjacentPlatforms;
    public Queue<Commuter>[] platformQueues;
    public CommuterNavPoint[] queuePoints;
    public Train currentTrainAtPlatform;

    public int temporary_routeDistance = 0;

    public void SetupPlatform(MetroLine _parentMetroLine, BezierPoint _start, BezierPoint _end)
    {
        parentMetroLine = _parentMetroLine;
        point_platform_START = _start;
        point_platform_END = _end;
        carriageCount = parentMetroLine.carriagesPerTrain;
        adjacentPlatforms = new List<Platform>();
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

    public void Add_AdjacentPlatform(Platform _platform)
    {
        if (!adjacentPlatforms.Contains(_platform))
        {
            Debug.Log(parentMetroLine.metroLine_index + "_" + point_platform_END.index + "   is adjacent to  " 
                      +_platform.parentMetroLine.metroLine_index+"_"+_platform.point_platform_END.index);
            adjacentPlatforms.Add(_platform);
            _platform.Add_AdjacentPlatform(this);
        }
    }

    public void SetColour()
    {
        Color _LINE_COLOUR = parentMetroLine.lineColour;
        Colour.RecolourChildren(walkway_FRONT_CROSS.transform, _LINE_COLOUR);
        Colour.RecolourChildren(walkway_BACK_CROSS.transform, _LINE_COLOUR);
    }

    public int Get_NumberOfStopsTo(Platform _destination)
    {
        return _destination.platformIndex - platformIndex;
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
