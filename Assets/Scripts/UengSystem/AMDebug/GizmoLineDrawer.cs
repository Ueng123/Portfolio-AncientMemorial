using UnityEngine;

namespace UengSystem.AMDebug {
	public class GizmoLineDrawer : GizmoDrawer {
		
		[Header("Line Settings")]
		public Vector2 startPos;
		public Vector2 endPos;
		
		private void DrawLine(Vector2 start, Vector2 end, Color color)
		{
			Color oldColor = Gizmos.color;
            
			Gizmos.color = color;
			Gizmos.DrawLine(start, end);
            
			Gizmos.color = oldColor;
		}
		
		public void OnDrawGizmos() {
			DrawLine((offsetMode)?(Vector2)transform.position+startPos:startPos, (offsetMode)?(Vector2)transform.position+endPos:endPos, color);
		}
	}
}