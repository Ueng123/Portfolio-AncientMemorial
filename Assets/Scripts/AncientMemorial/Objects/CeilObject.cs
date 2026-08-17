using System.Numerics;
using UnityEngine;
using AncientMemorial.Map;
using UengSystem.Objects;
using Vector3 = UnityEngine.Vector3;

namespace AncientMemorial.Objects {
	public class CeilObject : UObject {
		public float offsetY;
		
		public override void OnGet() {
			transform.SetParent(MapManager.instance.mapObjects.ceilTR, true);
			transform.localPosition = new Vector3(transform.localPosition.x, offsetY, 0);
		}
	}
}