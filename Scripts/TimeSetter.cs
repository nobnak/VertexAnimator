using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VertexAnimater {

	[ExecuteAlways]
	public class TimeSetter : MonoBehaviour {

		[SerializeField]
		protected string property = "_AnimTex_T";
		[SerializeField]
		protected bool autoUpdateTime = true;
		[SerializeField]
		protected float time = 0f;
		[SerializeField]
		protected float length = 60f;

		protected int id_prop;
		protected Material[] mats = new Material[0];

		#region unity
		private void OnEnable() {
			id_prop = Shader.PropertyToID(property);

			mats = GetComponentsInChildren<Renderer>().Select(v => v.sharedMaterial).ToArray();
		}
		private void Update() {
			if (autoUpdateTime)
				time = Mathf.Repeat(time + Time.deltaTime, length);

			foreach (var m in mats)
				m?.SetFloat(id_prop, time);
		}
		#endregion

	}
}
