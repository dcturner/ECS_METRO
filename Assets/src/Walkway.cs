using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Walkway : MonoBehaviour {

    public Platform connects_FROM, connects_TO;
    public Transform nav_BOTTOM, nav_TOP;

    private void OnDrawGizmos()
    {
        if (nav_TOP != null && nav_BOTTOM != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(nav_BOTTOM.position, nav_TOP.position);
        }
    }
}
