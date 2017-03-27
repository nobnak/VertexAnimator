using UnityEngine;
using System.Collections.Generic;

namespace VertexAnimater {
    public class SingleMeshSampler : IMeshSampler {
        public GameObject Target { get; private set; }
        public Mesh Output { get; private set; }
        public float Length {get; private set; }

        SkinnedMeshRenderer _skin;
        Animation _animation;
        AnimationState _state;

        public SingleMeshSampler(GameObject target) {
            Output = new Mesh();

            _skin = target.GetComponentInChildren<SkinnedMeshRenderer> ();
            
            _animation = target.GetComponentInChildren<Animation> ();
            _state = _animation[_animation.clip.name];
            _state.speed = 0f;
            Length = _state.length;
            _animation.Play (_state.name);
        }

        public void Dispose() {
            Object.Destroy (Output);
        }

        public Mesh Sample(float time, out Matrix4x4 mpos, out Matrix4x4 mnorm) {
            time = Mathf.Clamp (time, 0f, Length);
            _state.time = time;
            _animation.Sample ();
            _skin.BakeMesh (Output);
            mpos = _skin.localToWorldMatrix;
            mnorm = _skin.worldToLocalMatrix.transpose;
            return Output;
        }
    }

    public interface IMeshSampler : System.IDisposable {
        GameObject Target { get; }
        Mesh Output { get; }
        float Length { get; }
        Mesh Sample (float time, out Matrix4x4 mpos, out Matrix4x4 mnorm);
    }
}