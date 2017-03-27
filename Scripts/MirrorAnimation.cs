using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VertexAnimater {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class MirrorAnimation : MonoBehaviour {
        public GameObject target;
        public float time = 0f;

        CombinedMeshSampler _sampler;
        MeshFilter _filter;

        void OnEnable() {
            _sampler = new CombinedMeshSampler (target);
            _filter = GetComponent<MeshFilter>();
        }
        void OnDisable() {
            if (_sampler != null)
                _sampler.Dispose ();
        }
        void Update() {
            Matrix4x4 mpos, mnorm;
            _filter.sharedMesh = _sampler.Sample (time, out mpos, out mnorm);
        }
    }
}