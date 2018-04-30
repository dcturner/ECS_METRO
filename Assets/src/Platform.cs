using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{

    public MetroLine parentMetroLine;
    public List<Walkway> walkways;
    public Walkway stairs_FRONT_CROSS, stairs_BACK_CROSS, stairs_UP, stairs_DOWN;
    public BezierPoint point_platform_START, point_platform_END;

    public void SetColour()
    {
        Color _LINE_COLOUR = parentMetroLine.lineColour;
        Colour.RecolourChildren(stairs_FRONT_CROSS.transform, _LINE_COLOUR);
        Colour.RecolourChildren(stairs_BACK_CROSS.transform, _LINE_COLOUR);
        Colour.RecolourChildren(stairs_UP.transform, _LINE_COLOUR);
        Colour.RecolourChildren(stairs_DOWN.transform, _LINE_COLOUR);
    }
}
