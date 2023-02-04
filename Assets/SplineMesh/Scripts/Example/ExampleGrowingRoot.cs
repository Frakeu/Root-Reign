﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SplineMesh {
    /// <summary>
    /// Example of component to show the deformation of the mesh on a changing
    /// interval and changing spline nodes.
    /// 
    /// In this example, as the MeshBender is working on spline space, it will update
    /// the mesh if one of the curve change. Each change make the MeshBender "dirty" and
    /// it will compute the mesh only once on it's next update call.
    /// 
    /// This component is only for demo purpose and is not intended to be used as-is.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Spline))]
    public class ExampleGrowingRoot : MonoBehaviour {
        private GameObject generated;
        private Spline spline;
        private float rate = 0;
        private MeshBender meshBender;

        public Mesh mesh;
        public Material material;
        public Vector3 rotation;
        public Vector3 scale;

        public float startScale = 1;

        public float DurationInSecond;

        private void OnEnable() {
            rate = 0;
            Init();
#if UNITY_EDITOR
            //EditorApplication.update += EditorUpdate;
#endif
        }

        void OnDisable() {
#if UNITY_EDITOR
            //EditorApplication.update -= EditorUpdate;
#endif
        }

        private void OnValidate() {
            Init();
        }

        private void Update() {
            EditorUpdate();
        }

        void EditorUpdate() {
            if(rate <= 1)
                rate += Time.deltaTime / DurationInSecond;
            
            if (rate > 1) {
                //rate --;
            }
            Contort();
        }

        private void Contort() {
            float nodeDistance = 0;
            int i = 0;
            foreach (var n in spline.nodes) {
                float nodeDistanceRate = nodeDistance / spline.Length;
                float nodeScale = startScale * (rate - nodeDistanceRate);
                n.Scale = new Vector2(nodeScale, nodeScale);
                if (i < spline.curves.Count) {
                    nodeDistance += spline.curves[i++].Length;
                }
            }

            if (generated != null) {
                meshBender.SetInterval(spline, 0, spline.Length * rate);
                meshBender.ComputeIfNeeded();
            }
        }

        private void Init() {
            string generatedName = "generated by " + GetType().Name;
            var generatedTranform = transform.Find(generatedName);
            generated = generatedTranform != null ? generatedTranform.gameObject : UOUtility.Create(generatedName, gameObject,
                typeof(MeshFilter),
                typeof(MeshRenderer),
                typeof(MeshBender));

            generated.GetComponent<MeshRenderer>().material = material;

            meshBender = generated.GetComponent<MeshBender>();
            spline = GetComponent<Spline>();

            meshBender.Source = SourceMesh.Build(mesh)
                .Rotate(Quaternion.Euler(rotation))
                .Scale(scale);
            meshBender.Mode = MeshBender.FillingMode.StretchToInterval;
            meshBender.SetInterval(spline, 0, 0.01f);
        }
    }
}
