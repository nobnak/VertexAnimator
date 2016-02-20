using UnityEngine;
using System.Collections.Generic;

namespace VertexAnimater {
    public class CombinedMeshSampler : System.IDisposable {
        public readonly GameObject Target;
        public readonly Mesh CombinedMesh;
        public readonly float Length = 0f;

        Mesh[] _meshes;
        SkinnedMeshRenderer[] _skines;
        Animation[] _animations;
        AnimationState[] _state;

        public CombinedMeshSampler(GameObject target) {
            CombinedMesh = new Mesh();
            CombinedMesh.MarkDynamic ();

            _skines = target.GetComponentsInChildren<SkinnedMeshRenderer> ();
            _meshes = new Mesh[_skines.Length];
            for (var i = 0; i < _skines.Length; i++)
                _meshes [i] = new Mesh ();
            
            _animations = target.GetComponentsInChildren<Animation> ();
            _state = new AnimationState[_animations.Length];
            for (var i = 0; i < _animations.Length; i++) {
                var animation = _animations [i];
                var state = _state [i] = animation [animation.clip.name];
                state.speed = 0f;
                Length = Mathf.Max (Length, state.length);
                animation.Play (state.name);
            }
        }

        public void Dispose() {
            if (_meshes != null) {
                for (var i = 0; i < _meshes.Length; i++)
                    Object.Destroy (_meshes [i]);
                _meshes = null;
            }
            Object.Destroy (CombinedMesh);
        }

        public Mesh Sample(float time) {
            time = Mathf.Clamp (time, 0f, Length);
            for (var i = 0; i < _animations.Length; i++) {
                _state [i].time = time;
                _animations [i].Sample ();
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

            CombinedMesh.CombineMeshes (combines);
            return CombinedMesh;
        }
    }
}