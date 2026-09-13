#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AncientMemorial.Map {
	[CustomEditor(typeof(MapManager))]
	public class MapEditor : Editor
	{

		// 오버라이드 메서드
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI(); // 기본 인스펙터 요소 출력

			MapManager script = (MapManager)target;

			if (GUILayout.Button("맵 생성"))
			{
				script.mapObjects.Move(script.GetTargetMapSize());
			}
		}
	}
}
#endif