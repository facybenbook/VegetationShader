using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Northwind.Shaders.InteractiveVegetation
{
    [ExecuteInEditMode]
    [AddComponentMenu("Northwind/Interactive Vegetation/Deform Renderer")]
    public class VegetationDeformRenderer : MonoBehaviour
    {

        public float minVertexDistance = 0.5f;
        public float lifetime = 2f;

        public AnimationCurve radius = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
        public AnimationCurve strength = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

        [Range(0, 4)]
        public int endSubDiv = 0;

        [HideInInspector]
        public int layer;
        public Material material;

        public bool debug = false;

        private Vector3 up = Vector3.up;
        private Vector3 pos
        {
            get
            {
                return transform.position;
            }
        }

        private List<TrailPoint> _trailHistory = new List<TrailPoint>();
        private Mesh _mesh;

        private List<Vector3> _vertices = new List<Vector3>();
        private List<int> _triangles = new List<int>();
        private List<Vector3> _normals = new List<Vector3>();
        private List<Color> _colors = new List<Color>();

        private Vector3 oldPos;

        // Use this for initialization
        void Start()
        {
            _mesh = new Mesh();
            oldPos = pos;
            layer = VegetationDeformCamera.main.GetCameraLayer();
        }

        // Update is called once per frame
        void Update()
        {
            //Remove points that are older than lifetime
            while (_trailHistory.Count > 0)
            {
                if ((Time.time - _trailHistory[0].creationTime) > lifetime)
                {
                    _trailHistory.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            bool addedPoint = false;

            //Add new point if list is empty or distance to last point is bigger than minVertexDistance
            if (Vector3.Distance(oldPos, pos) > minVertexDistance)
            {
                _trailHistory.Add(new TrailPoint(pos, Time.time));
                addedPoint = true;
                oldPos = pos;
            }

            List<TrailPoint> renderPoints = new List<TrailPoint>(_trailHistory);

            //If no point was added this frame, use the current position as point
            if (!addedPoint)
            {
                renderPoints.Add(new TrailPoint(pos, Time.time));
            }

            UpdateMesh(renderPoints);

            _mesh.RecalculateBounds();

            Graphics.DrawMesh(_mesh, Matrix4x4.identity, material, layer, debug ? null : VegetationDeformCamera.main.cam);
        }

        private void UpdateMesh(List<TrailPoint> renderPoints)
        {
            _mesh.Clear();

            //Clear lists
            _vertices.Clear();
            _triangles.Clear();
            _normals.Clear();
            _colors.Clear();

            if (renderPoints.Count < 2)
            {
                AddEnd(new TrailPoint(pos, Time.time), transform.forward);
                AddEnd(new TrailPoint(pos, Time.time), -transform.forward);

                _mesh.vertices = _vertices.ToArray();
                _mesh.triangles = _triangles.ToArray();
                _mesh.normals = _normals.ToArray();
                _mesh.colors = _colors.ToArray();
                return;
            }


            //Start
            AddEnd(new TrailPoint(renderPoints[0].pos, renderPoints[0].creationTime), (renderPoints[0].pos - renderPoints[1].pos).normalized);


            int start = _vertices.Count;

            for (int p = 0; p < renderPoints.Count; p++)
            {
                Vector3 lDir = Vector3.zero;
                if (p < renderPoints.Count - 1)
                {
                    lDir = (renderPoints[p + 1].pos - renderPoints[p].pos).normalized;
                }
                else
                {
                    lDir = (renderPoints[p].pos - renderPoints[p - 1].pos).normalized;
                }

                float lLifePercent = (Time.time - renderPoints[p].creationTime) / lifetime;
                float lRadius = radius.Evaluate(lLifePercent);
                float lStrength = strength.Evaluate(lLifePercent);

                _vertices.Add(renderPoints[p].pos);
                _normals.Add(up);
                _colors.Add(new Color(0f, -1f, 0f, 1f) * lStrength);

                Vector3 lNDir = Rotate2D(lDir, -90f);
                _vertices.Add(renderPoints[p].pos + lNDir * lRadius);
                _normals.Add(lNDir * lStrength);
                _colors.Add(new Color(lNDir.x, lNDir.y, lNDir.z, 0f) * lStrength);

                lNDir = Rotate2D(lDir, 90f);
                _vertices.Add(renderPoints[p].pos + lNDir * lRadius);
                _normals.Add(lNDir * lStrength);
                _colors.Add(new Color(lNDir.x, lNDir.y, lNDir.z, 0f) * lStrength);

                if (p > 0)
                {
                    int lPos = start + (p - 1) * 3;

                    _triangles.Add(lPos);
                    _triangles.Add(lPos + 1);
                    _triangles.Add(lPos + 4);

                    _triangles.Add(lPos + 4);
                    _triangles.Add(lPos + 3);
                    _triangles.Add(lPos);

                    _triangles.Add(lPos);
                    _triangles.Add(lPos + 3);
                    _triangles.Add(lPos + 2);

                    _triangles.Add(lPos + 2);
                    _triangles.Add(lPos + 3);
                    _triangles.Add(lPos + 5);
                }
            }

            //Finish
            AddEnd(new TrailPoint(pos, Time.time), transform.forward);
            AddEnd(new TrailPoint(pos, Time.time), -transform.forward);
            //AddEnd(new TrailPoint(renderPoints[renderPoints.Count - 1].pos, renderPoints[renderPoints.Count - 1].creationTime), (renderPoints[renderPoints.Count - 1].pos - renderPoints[renderPoints.Count - 2].pos).normalized);
            //AddEnd(new TrailPoint(renderPoints[renderPoints.Count - 1].pos, renderPoints[renderPoints.Count - 1].creationTime), (renderPoints[renderPoints.Count - 2].pos - renderPoints[renderPoints.Count - 1].pos).normalized);

            _mesh.vertices = _vertices.ToArray();
            _mesh.triangles = _triangles.ToArray();
            _mesh.normals = _normals.ToArray();
            _mesh.colors = _colors.ToArray();
        }

        private void AddEnd(TrailPoint point, Vector3 dir)
        {
            float lLifePercent = (Time.time - point.creationTime) / lifetime;
            float lRadius = radius.Evaluate(lLifePercent);
            float lStrength = strength.Evaluate(lLifePercent);

            int start = _vertices.Count;
            _vertices.Add(point.pos);
            _normals.Add(up);
            _colors.Add(new Color(0f, -1f, 0f, 1f) * lStrength);

            int divs = 3 + endSubDiv;

            for (int s = 0; s < 3 + endSubDiv; s++)
            {
                Vector3 lDir = Rotate2D(dir, -90f + (180f / (divs - 1) * s));
                _vertices.Add(point.pos + lDir * lRadius);
                _normals.Add(lDir * lStrength);
                _colors.Add(new Color(lDir.x, lDir.y, lDir.z, 0f) * lStrength);

                if (s > 0)
                {
                    _triangles.Add(start);
                    _triangles.Add(start + s);
                    _triangles.Add(start + s + 1);
                }
            }
        }

        private Vector3 Rotate2D(Vector3 direction, float angle)
        {
            return Quaternion.Euler(0f, angle, 0f) * direction;
        }
    }

    public struct TrailPoint
    {
        public Vector3 pos;
        public float creationTime;

        public TrailPoint(Vector3 pos, float creationTime)
        {
            this.pos = pos;
            this.creationTime = creationTime;
        }
    }
}