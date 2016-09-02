using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {
    public const float X360 = 360f;

    public Vector3 speed;

	void Update () {
        transform.localRotation *= Quaternion.Euler (Time.deltaTime * X360 * speed);
	}
}
