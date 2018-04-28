using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Metro : MonoBehaviour
{
    public static float CUSTOMER_SATISFACTION = 1f;
    public static float BEZIER_HANDLE_REACH = 0.1f;
    public static float BEZIER_DISTANCE_SMOOTHING = 0.95f;
    public const int BEZIER_MEASUREMENT_SUBDIVISIONS = 100;
    public static Metro INSTANCE;
    
    // PUBLICS
    public float Bezier_HandleReach = 0.3f;
    public float Bezier_DistanceSmoothing = 0.95f;
    
    public string[] LineNames;
    private int totalLines = 0;
    public Color[] LineColours;

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

    private void Start()
    {
        Metro.BEZIER_HANDLE_REACH = Bezier_HandleReach;
        Metro.BEZIER_DISTANCE_SMOOTHING = Bezier_DistanceSmoothing;
        SetupMetroLines();
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
                    _relevantMarkers.Where(m => m.isPlatform).Select(m => m.pointIndex).ToArray();


                MetroLine _newLine = new MetroLine(i);
                _newLine.lineName = LineNames[i];
                _newLine.lineColour = LineColours[i];
                _newLine.Create_RailPath(_relevantMarkers.Select(m => m.transform.position).ToArray(),
                    _platformPointIndexes);
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