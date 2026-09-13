using UnityEngine;

namespace UengSystem.UDebug {
	public class GizmoBoxDrawer : GizmoDrawer {

		// 인스턴스 프로퍼티
		[Header("Box Setting")]
		public Vector2 size;

		// 인스턴스 메서드
		private void DrawOverlapBox(Vector2 center, Vector2 size, float angle, Color color)
		{
			Color oldColor  = Gizmos.color;
			Matrix4x4 oldMatrix = Gizmos.matrix;

			Gizmos.color  = color;
			Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
            
			Gizmos.DrawWireCube(Vector3.zero, size);
            
			Gizmos.matrix = oldMatrix;
			Gizmos.color  = oldColor;
		}
		
		public void OnDrawGizmos() {
			DrawOverlapBox((offsetMode)?(Vector2)transform.position+pos:pos, size, rot, color);
		}
	}
}