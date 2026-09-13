using UnityEngine;

namespace UengSystem.UDebug {
	public class GizmoCircleDrawer : GizmoDrawer {

		// 인스턴스 프로퍼티
		[Header("Circle Settings")]
		public float   radius   = 2.0f;
		public int     segments = 32;

		// 인스턴스 메서드
		private void OnDrawGizmos() {
			Gizmos.color = color;

			Vector3 center = (offsetMode)?(Vector2)transform.position+pos:pos;

			float angleStep = 360f / segments;

			float angle = 0f;
			Vector3 startPoint = center + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
													  Mathf.Sin(angle * Mathf.Deg2Rad) * radius, 0f);
			Vector3 lastPoint = startPoint;

			for (int i = 1; i <= segments; i++) {
				angle = i * angleStep;
				Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
														 Mathf.Sin(angle * Mathf.Deg2Rad) * radius, 0f);

				Gizmos.DrawLine(lastPoint, nextPoint);
				lastPoint = nextPoint;
			}
		}
	}
}