using System.Numerics;
using UnityEngine;
using AncientMemorial.Map;
using UengSystem.Objects;
using Vector3 = UnityEngine.Vector3;

namespace AncientMemorial.Objects {
	public class CeilObject : UObject {

		// 인스턴스 프로퍼티
		public float offsetY;

		// 오버라이드 메서드
		public override void OnGet() {
			transform.SetParent(MapManager.instance.mapObjects.ceilTR, true);
			transform.localPosition = new Vector3(transform.localPosition.x, offsetY, 0);
		}
	}
}