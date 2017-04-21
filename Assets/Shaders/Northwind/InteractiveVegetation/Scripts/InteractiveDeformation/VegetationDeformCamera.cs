using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Northwind.Shaders.InteractiveVegetation
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    [AddComponentMenu("Northwind/Interactive Vegetation/Deform Camera")]
    public class VegetationDeformCamera : MonoBehaviour
    {

        public static VegetationDeformCamera main;

        public int pixelWidth = 256;

        public bool debug = false;

        private Camera _cam;
        public Camera cam
        {
            get
            {
                return _cam;
            }
        }

        private Transform _mainCameraTransform;

        private RenderTexture _renderTexture;

        void Awake()
        {
            _mainCameraTransform = Camera.main.transform;
            _cam = GetComponent<Camera>();
            main = this;
            cam.cullingMask = 1 << GetCameraLayer();
            _renderTexture = new RenderTexture(pixelWidth, pixelWidth, 0, RenderTextureFormat.ARGB32);
        }

        private void OnEnable()
        {
            _cam.orthographic = true;
            _cam.targetTexture = _renderTexture;

            _cam.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            _cam.clearFlags = CameraClearFlags.SolidColor;
        }

        public int GetCameraLayer()
        {
            return 31; //the last possible layer
        }

        void OnPreCull()
        {
            //Update camera rotation
            transform.rotation = Quaternion.Euler(90, 0, 0);
            transform.position = new Vector3(_mainCameraTransform.position.x, _mainCameraTransform.position.y + 10f, _mainCameraTransform.position.z);

            float width = (2 * cam.orthographicSize) * cam.aspect;
            float height = (2 * cam.orthographicSize) / cam.aspect;

            Vector4 renderArea = new Vector4(transform.position.x, transform.position.z, width, height);

            Shader.SetGlobalVector("_VegetationTextureArea", renderArea);
            Shader.SetGlobalTexture("_VegetationDeformTex", _renderTexture);
        }

        private void OnGUI()
        {
            if (debug)
                GUI.DrawTexture(new Rect(8, 8, 256, 256), _renderTexture);
        }
    }
}