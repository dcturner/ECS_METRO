using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Metro : MonoBehaviour
{
    public static float CUSTOMER_SATISFACTION = 1f;
    public static float BEZIER_HANDLE_REACH = 0.1f;
    public static float BEZIER_PLATFORM_OFFSET = 3f;
    public const int BEZIER_MEASUREMENT_SUBDIVISIONS = 2000;
    public static Metro INSTANCE;
    
    
    
    // PUBLICS
    public GameObject prefab_trainCarriage;
    public GameObject prefab_platform;
    [Range(0f,1f)]
    public float Bezier_HandleReach = 0.3f;
    public float Bezier_PlatformOffset = 3f;
    [Header("Trains")]
    public float Train_accelerationStrength = 0.001f;
    public float Train_brakeStrength = 0.01f;
    public float Train_railFriction = 0.99f;
    public float Train_delay_doors_OPEN = 2f;
    public float Train_delay_doors_CLOSE = 1f;
    public float Train_delay_departure = 1f;
    [Header("Commuters")]
    // walk speed etc
    
    [Header("MetroLines")]
    public string[] LineNames;
    public int[] maxTrains;
    public int[] carriagesPerTrain;
    public float[] trainCarriageSpacing;
    public float[] maxTrainSpeed;
    private int totalLines = 0;
    public Color[] LineColours;

    [HideInInspector]
    public MetroLine[] metroLines;

    public static string GetLine_NAME_FromIndex(int _index)
    {
        string result = "";
        INSTANCE = FindObjectOfType<Metro>();
        if (INSTANCE != null)
        {
            if (INSTANCE.LineNames.Length - 1 >= _index)
            {
                result = INSTANCE.LineNames[_index];
            }
        }

        return result;
    }

    public static Color GetLine_COLOUR_FromIndex(int _index)
    {
        Color result = Color.black;
        INSTANCE = FindObjectOfType<Metro>();
        if (INSTANCE != null)
        {
            if (INSTANCE.LineColours.Length - 1 >= _index)
            {
                result = INSTANCE.LineColours[_index];
            }
        }

        return result;
    }

    private void Awake()
    {
        INSTANCE = this;
    }

    private void Start()
    {
        BEZIER_HANDLE_REACH = Bezier_HandleReach;
        BEZIER_PLATFORM_OFFSET = Bezier_PlatformOffset;
        SetupMetroLines();
        SetupTrains();
    }

    private void Update()
    {
        for (int i = 0; i < totalLines; i++)
        {
            if (metroLines[i] != null)
            {
                metroLines[i].UpdateTrains();
            }
        }
    }

    void SetupMetroLines()
    {
        totalLines = LineNames.Length;
        metroLines = new MetroLine[totalLines];
        for (int i = 0; i < totalLines; i++)
        {
            // Find all of the relevant RailMarkers in the scene for this line
            List<RailMarker> _relevantMarkers = FindObjectsOfType<RailMarker>().Where(m => m.metroLineID == i)
                .OrderBy(m => m.pointIndex).ToList();

            // Only continue if we have something to work with
            if (_relevantMarkers.Count > 1)
            {
                int[] _platformPointIndexes =
                    _relevantMarkers.Where(m => m.railMarkerType == RailMarkerType.PLATFORM_START).Select(m => m.pointIndex).ToArray();


                MetroLine _newLine = new MetroLine(i, maxTrains[i]);
                _newLine.Create_RailPath(_relevantMarkers);
                metroLines[i] = _newLine;
            }
            else
            {
                Debug.LogWarning("Insufficient RailMarkers found for line: " + i +", you need to add the outbound points");
            }
        }
        // now destroy all RailMarkers
        foreach (RailMarker _RM in FindObjectsOfType<RailMarker>())
        {
            Destroy(_RM);
        }
    }

    void SetupTrains()
    {
        for (int i = 0; i < totalLines; i++)
        {
            if (metroLines[i] != null)
            {
                MetroLine _ML = metroLines[i];
                float trainSpacing = 1f / _ML.maxTrains;
                for (int trainIndex = 0; trainIndex < _ML.maxTrains; trainIndex++)
                {
                    _ML.AddTrain(trainIndex * trainSpacing);
                }
            }
        }
    }

    #region ------------------------- < GIZMOS

    private void OnDrawGizmos()
    {
        for (int i = 0; i < totalLines; i++)
        {
            MetroLine _tempLine = metroLines[i];
            if (_tempLine != null)
            {
                BezierPath _path = _tempLine.bezierPath;
                if (_path != null)
                {
                    for (int pointIndex = 0; pointIndex < _path.points.Count; pointIndex++)
                    {
                        BezierPoint _CURRENT_POINT = _path.points[pointIndex];

                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireSphere(_CURRENT_POINT.location, 0.1f);

                        Gizmos.color = Color.red;
                        Gizmos.DrawWireCube(_CURRENT_POINT.handle_in, Vector3.one * 0.025f);
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireCube(_CURRENT_POINT.handle_out, Vector3.one * 0.025f);

                        BezierPoint _NEXT_POINT = _path.points[(pointIndex + 1) % _path.points.Count];
                        // Link them up
                        Handles.DrawBezier(_CURRENT_POINT.location, _NEXT_POINT.location, _CURRENT_POINT.handle_out,
                            _NEXT_POINT.handle_in, GetLine_COLOUR_FromIndex(i), null, 3f);
                    }
                }
            }
        }
        
        
    }

    #endregion ------------------------ GIZMOS >

}