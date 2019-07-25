using System.Collections;
using System.Collections.Generic;
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
		[SerializeField]
		protected Material[] mats = new Material[0];

		protected int id_prop;

		#region unity
		private void OnEnable() {
			id_prop = Shader.PropertyToID(property);
			time = 0f;
		}
		private void Update() {
			if (autoUpdateTime)
				time = Mathf.Repeat(time + Time.deltaTime, length);

			foreach (var m in mats)
				m.SetFloat(id_prop, time);
		}
		#endregion

	}
}
