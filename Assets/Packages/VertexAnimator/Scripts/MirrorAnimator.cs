using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VertexAnimater {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class MirrorAnimator : MonoBehaviour {
        public const string ANIMATION_STATE_NAME = "Take 001";
        public GameObject target;
        public float time = 0f;

        Mesh _combinedMesh;
        Mesh[] _meshes;
        SkinnedMeshRenderer[] _skines;
        Animator[] _animators;

        void OnEnable() {
            _combinedMesh = new Mesh();
            _combinedMesh.MarkDynamic ();
            GetComponent<MeshFilter> ().sharedMesh = _combinedMesh;

            _skines = target.GetComponentsInChildren<SkinnedMeshRenderer> ();
            _meshes = new Mesh[_skines.Length];
            for (var i = 0; i < _skines.Length; i++)
                _meshes [i] = new Mesh ();
            
            _animators = target.GetComponentsInChildren<Animator> ();
        }
        void OnDisable() {
            for (var i = 0; i < _meshes.Length; i++)
                Destroy (_meshes [i]);
            Destroy (_combinedMesh);
        }
        void Update() {
            time = Mathf.Clamp01 (time);
            for (var i = 0; i < _animators.Length; i++) {
                var animator = _animators [i];
                animator.speed = 0f;
                animator.Play (0, -1, time);
            }


            var combines = new CombineInstance[_meshes.Length];
            for (var i = 0; i < _skines.Length; i++) {
                var skin = _skines [i];
                var mesh = _meshes [i];
                var combine = combines [i];
                skin.BakeMesh (mesh);
                combine.mesh = mesh;
                combine.transform = skin.transform.localToWorldMatrix;
                combines [i] = combine;
            }

            _combinedMesh.CombineMeshes (combines);
        }
    }
}