using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour {
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    [SerializeField] bool drawGizmos;

    [SerializeField] LayerMask visibleInView;
    [SerializeField] LayerMask obstacle;

    [SerializeField] float viewRadius;
    [Range(0, 360)]
    [SerializeField] int angle;

    [Range(1, 10)]
    [SerializeField] int meshDetail;
    [Range(1, 10)]
    [SerializeField] int edgeDetectionIterations;

    Mesh mesh;

    void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }

    void Start() {
        mesh = new Mesh();
        mesh.name = "View Mesh";
        meshFilter.mesh = mesh;
    }

    void LateUpdate() {
        MakeViewMesh();
    }

    void MakeViewMesh() {
        List<Vector3> viewPoints = new();
        int rayAmount = Mathf.RoundToInt((float)angle / (1f / meshDetail));
        
        viewPoints.Add(transform.position);
        
        viewPointInfo prevInfo = new();

        for (int i = 0; i <= rayAmount; i++) {
            float a = -(float)angle / 2 + ((float)angle / rayAmount) * i;
            viewPointInfo info = new viewPointInfo(a, null, GetDirection(a) * viewRadius);
            if (Physics.Raycast(transform.position, GetDirection(a), out RaycastHit hit, viewRadius, obstacle)) {
                info = new viewPointInfo(a, hit.transform, hit.point);

                if (i > 0 && info.HitTransform != prevInfo.HitTransform) {
                    edgeInfo e = DetectEdge(prevInfo, info);
                    if (e.MinPoint != Vector3.zero) {
                        viewPoints.Add(e.MinPoint);
                    }
                    if (e.MaxPoint != Vector3.zero) {
                        viewPoints.Add(e.MaxPoint);
                    }
                }
                viewPoints.Add(hit.point);
            }
            else {
                viewPoints.Add(transform.position + GetDirection(a) * viewRadius);
            }
            prevInfo = info;
        }

        Vector3[] verts = new Vector3[viewPoints.Count];
        int[] tris = new int[(verts.Length - 2) * 3];

        for (int i = 0; i < verts.Length; i++) {
            verts[i] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < verts.Length - 2) {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;
            }
        }

        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
    }

    edgeInfo DetectEdge(viewPointInfo min, viewPointInfo max) {
        float minAngle = min.Angle;
        float maxAngle = max.Angle;
        Transform minTransform = min.HitTransform;
        Transform maxTransform = max.HitTransform;
        Vector3 minPoint = min.Point;
        Vector3 maxPoint = max.Point;
        for (int i = 0; i < edgeDetectionIterations; i++) {
            float a = (minAngle + maxAngle) / 2;
            if (Physics.Raycast(transform.position, GetDirection(a), out RaycastHit hit, viewRadius, obstacle)) {
                if (min.HitTransform == hit.transform) {
                    minAngle = a;
                    minPoint = hit.point;
                }
                else if (max.HitTransform == hit.transform) {
                    maxAngle = a;
                    maxPoint = hit.point;
                }
            }
        }
        return new edgeInfo(minPoint, maxPoint);
    }

    Vector3 GetDirection(float angle) {
        angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    struct viewPointInfo {
        public float Angle;
        public Transform HitTransform;
        public Vector3 Point;

        public viewPointInfo(float angle, Transform hitTransform, Vector3 point) {
            Angle = angle;
            HitTransform = hitTransform;
            Point = point;
        }
    }

    struct edgeInfo {
        public Vector3 MinPoint;
        public Vector3 MaxPoint;

        public edgeInfo(Vector3 minPoint, Vector3 maxPoint) {
            MinPoint = minPoint;
            MaxPoint = maxPoint;
        }
    }

    void OnDrawGizmos() {
        if (!drawGizmos) {
            return;
        }
        Gizmos.color = Color.white;
        Vector3 angleA = GetDirection(-angle / 2);
        Vector3 angleB = GetDirection(angle / 2);
        Gizmos.DrawLine(transform.position, transform.position + angleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + angleB * viewRadius);
    }
}