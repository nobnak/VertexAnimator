using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestInstancingWithTime : TestInstancing {
    Renderer[] _renderers;
    MaterialPropertyBlock[] _props;
    Random.State _state;

    protected override void Awake() {
        base.Awake ();
        _renderers = new Renderer[count];
        _props = new MaterialPropertyBlock[count];
        _state = Random.state;
    }

    protected override void InitProperties (int i, GameObject f) {
        _renderers [i] = f.GetComponent<Renderer> ();
        _props [i] = new MaterialPropertyBlock ();
    }

    protected virtual void Update() {
        Random.state = _state;
        for (var i = 0; i < _props.Length; i++) {
            var p = _props [i];
            p.SetFloat (PROP_TIME, Mathf.Repeat (timeOffset + Random.value * TIME_LENGTH, TIME_LENGTH));
            _renderers [i].SetPropertyBlock (p);
        }
    }
}
