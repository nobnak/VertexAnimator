using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestInstancing : MonoBehaviour {
    public const string PROP_TIME = "_AnimTex_T";
    public const float TIME_LENGTH = 60f;

    public int count = 10000;
    public float radius = 10f;
    public GameObject[] fabs;
    public float timeOffset = 0f;

    GameObject[] _flowers;

    protected virtual void Awake() {
        _flowers = new GameObject[count];
    }
    protected virtual void Start() {
        for (var i = 0; i < _flowers.Length; i++) {
            var f = Instantiate (fabs [Random.Range (0, fabs.Length)]);
            f.transform.SetParent (transform);
            f.transform.localPosition = radius * Random.insideUnitSphere;
            f.transform.localRotation = Random.rotationUniform;

            _flowers [i] = f;
            InitProperties (i, f);
        }
    }
    protected virtual void InitProperties(int i, GameObject f) {}
}
