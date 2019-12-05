using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProtPt : MonoBehaviour
{

#if UNITY_EDITOR
    private PointToCinemachinePath pointToCinemachinePath = null;

    // Update is called once per frame
    void Update()
    {
        if (this.transform.hasChanged) {
            this.transform.hasChanged = false;
            if(pointToCinemachinePath == null) {
                pointToCinemachinePath = this.GetComponentInParent<PointToCinemachinePath>();
            }
            if (pointToCinemachinePath != null) {
                pointToCinemachinePath.UpdatePath();
            }
        }
    }
#endif
}
