using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public int carriageCount;
    public MetroLine parentMetroLine;
    public List<Walkway> walkways;
    public Walkway stairs_FRONT_CROSS, stairs_BACK_CROSS, stairs_UP, stairs_DOWN;
    public BezierPoint point_platform_START, point_platform_END;
    public Queue<Commuter>[] platformQueues;
    public Vector3[] queuePoints;

    public void SetupPlatform(MetroLine _parentMetroLine, BezierPoint _start, BezierPoint _end)
    {
        parentMetroLine = _parentMetroLine;
        point_platform_START = _start;
        point_platform_END = _end;
        carriageCount = parentMetroLine.carriagesPerTrain;
        SetColour();
        
        // setup queue lists and spacing
        platformQueues = new Queue<Commuter>[carriageCount];
        queuePoints = new Vector3[carriageCount];
        Vector3 frontQueuePoint = new Vector3(0f,0f,2f);
        for (int i = 0; i < carriageCount; i++)
        {
            platformQueues[i] = new Queue<Commuter>();
            queuePoints[i] = frontQueuePoint + (i * new Vector3(TrainCarriage.CARRIAGE_LENGTH, 0f, 0f));
        }
    }

    public void SetColour()
    {
        Color _LINE_COLOUR = parentMetroLine.lineColour;
        Colour.RecolourChildren(stairs_FRONT_CROSS.transform, _LINE_COLOUR);
        Colour.RecolourChildren(stairs_BACK_CROSS.transform, _LINE_COLOUR);
        Colour.RecolourChildren(stairs_UP.transform, _LINE_COLOUR);
        Colour.RecolourChildren(stairs_DOWN.transform, _LINE_COLOUR);
    }

    public void AddCommuter(Walkway _journeyStart, Walkway _journeyEnd)
    {
        GameObject commuter_OBJ =(GameObject) Instantiate(Metro.INSTANCE.prefab_commuter, transform.position, transform.rotation);
    }

    public int Get_QueueLength(int _queueIndex)
    {
        return platformQueues[_queueIndex].Count;
    }

    public Vector3 Get_QueuePosition(int _queueIndex)
    {
        return queuePoints[_queueIndex];
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
        }

        return shortest;
    }

    public Commuter Get_LastInQueue(int _queueIndex)
    {
        return platformQueues[_queueIndex].Peek();
    }
}
