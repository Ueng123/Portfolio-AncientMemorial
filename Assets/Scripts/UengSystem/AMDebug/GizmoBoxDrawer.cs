using UnityEngine;

namespace UengSystem.AMDebug {
	public class GizmoBoxDrawer : GizmoDrawer {

		[Header("Box Setting")]
		public Vector2 size;
		
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