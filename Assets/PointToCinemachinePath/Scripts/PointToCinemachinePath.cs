using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Cinemachine;

// ポイントを繋げて角丸のCinemachineパスを作成する。
[RequireComponent(typeof(CinemachinePath))]
public class PointToCinemachinePath : MonoBehaviour
{
    [SerializeField] CinemachinePath cinemachinePath = null;
    [SerializeField] bool looped = true;
    [SerializeField] List<Transform> points = null;

    [SerializeField] float offset = 0.5f;
    [SerializeField] float round = 0.5f;

    [SerializeField] float gizmo_tangentPtRadius = 0.2f;

    [SerializeField] ProtPt protPtPrefab = null;

    [SerializeField] bool autoUpdate = true;

    [ContextMenu("Create CreatePath")]
    public void CreatePath() {
        if (cinemachinePath == null) {
            cinemachinePath = this.GetComponent<CinemachinePath>();
        }

        if (offset.Equals(0) && round.Equals(0)) {
            CreatePathLinear();
        }
        else {
            CreatePathRounded();
        }
    }

    void CreatePathLinear() {
        if (cinemachinePath == null || points.Count < 1) { return; }

        cinemachinePath.m_Waypoints = null;
        cinemachinePath.m_Looped = looped;

        List<CinemachinePath.Waypoint> waypoints = new List<CinemachinePath.Waypoint>();
        Vector3 basePt;
        for (int i = 0; i < points.Count; i++) {
            basePt = points[i].position;
            CinemachinePath.Waypoint prePt = new CinemachinePath.Waypoint();
            prePt.position = cinemachinePath.transform.InverseTransformPoint(basePt);
            prePt.tangent = Vector3.zero;
            waypoints.Add(prePt);
        }

        cinemachinePath.m_Waypoints = waypoints.ToArray();
        cinemachinePath.InvalidateDistanceCache();
    }

    void CreatePathRounded() {
        if (cinemachinePath == null || points.Count < 1) { return; }

        cinemachinePath.m_Waypoints = null;
        cinemachinePath.m_Looped = looped;

        List<CinemachinePath.Waypoint> waypoints = new List<CinemachinePath.Waypoint>();

        if (!cinemachinePath.Looped) {
            CinemachinePath.Waypoint pt = new CinemachinePath.Waypoint();
            pt.position = cinemachinePath.transform.InverseTransformPoint(points[0].position);
            pt.tangent = cinemachinePath.transform.InverseTransformDirection(round * (points[1].position - points[0].position).normalized);
            waypoints.Add(pt);
        }

        int startIndex = cinemachinePath.Looped ? 0 : 1;
        Vector3 basePt;
        for (int i = startIndex; i < points.Count; i++) {
            basePt = points[i].position;
            int nextIndex = (i == points.Count - 1) ? 0 : i + 1;
            int preIndex = (i == 0) ? points.Count - 1 : i - 1;
            Vector3 nextDirection = (points[nextIndex].position - points[i].position).normalized;
            Vector3 preDirection = (points[i].position - points[preIndex].position).normalized;

            CinemachinePath.Waypoint prePt = new CinemachinePath.Waypoint();
            prePt.position = cinemachinePath.transform.InverseTransformPoint(basePt - offset * preDirection);
            prePt.tangent = cinemachinePath.transform.InverseTransformDirection(round * preDirection);
            waypoints.Add(prePt);

            CinemachinePath.Waypoint nextPt = new CinemachinePath.Waypoint();
            nextPt.position = cinemachinePath.transform.InverseTransformPoint(basePt + offset * nextDirection);
            nextPt.tangent = cinemachinePath.transform.InverseTransformDirection(round * nextDirection);
            waypoints.Add(nextPt);
        }

        cinemachinePath.m_Waypoints = waypoints.ToArray();
        cinemachinePath.InvalidateDistanceCache();
    }

    public void UpdatePath() {
#if UNITY_EDITOR
        if (autoUpdate) {
            points = new List<Transform>(this.transform.GetComponentsInChildren<Transform>());
            points.Remove(this.transform);

            CreatePath();
        }
#endif
    }

    [ContextMenu("Add ProtPt")]
    public void AddProtPt() {
#if UNITY_EDITOR
        Vector3 createPos = this.transform.localPosition;
        if (points != null && points.Count > 0) {
            createPos = points[points.Count - 1].position;
        }
        GameObject gObj = ((ProtPt)(UnityEditor.PrefabUtility.InstantiatePrefab(protPtPrefab))).gameObject;
        gObj.transform.SetParent(this.transform);
        gObj.transform.position = createPos;

        points.Add(gObj.transform);
#endif
    }

    private void OnDrawGizmosSelected() {
        if (cinemachinePath == null) { return; }

        Gizmos.color = Color.red;
        int endIndex = cinemachinePath.Looped ? points.Count : points.Count - 1;
        for (int i = 0; i < endIndex; i++) {
            int nextIndex = i + 1;
            if (cinemachinePath.Looped && i == endIndex - 1) {
                nextIndex = 0;
            }
            Gizmos.DrawLine(points[i].position, points[nextIndex].position);
        }

        // waypoints
        Gizmos.color = Color.yellow;
        for (int i = 0; i < cinemachinePath.m_Waypoints.Length; i++) {
            CinemachinePath.Waypoint wp = cinemachinePath.m_Waypoints[i];
            Vector3 pt = cinemachinePath.transform.TransformPoint(wp.position);
            Vector3 ptTanNext = pt + cinemachinePath.transform.TransformDirection(wp.tangent);
            Vector3 ptTanPre = pt - cinemachinePath.transform.TransformDirection(wp.tangent);
            Gizmos.DrawSphere(pt, gizmo_tangentPtRadius);

            Gizmos.DrawSphere(ptTanNext, gizmo_tangentPtRadius * 0.5f);
            Gizmos.DrawSphere(ptTanPre, gizmo_tangentPtRadius * 0.5f);
        }
    }

    private void OnValidate() {
        UpdatePath();
    }

    public bool IsUseAutoUpdate() {
        return autoUpdate;
    }

}


#if UNITY_EDITOR
[CustomEditor(typeof(PointToCinemachinePath))]
public class PointToCinemachinePathEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        PointToCinemachinePath _me = (PointToCinemachinePath)target;

        if (GUILayout.Button("Add Point")) {
            _me.AddProtPt();
        }

        GUI.enabled = !_me.IsUseAutoUpdate();
        if (GUILayout.Button("Update Path")) {
            _me.CreatePath();
        }
    }
}
#endif
