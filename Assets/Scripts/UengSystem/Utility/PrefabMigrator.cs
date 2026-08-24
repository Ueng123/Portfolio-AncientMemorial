#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace UengSystem.Utility {

	public class PrefabMigrator
	{
		[MenuItem("Tools/Force Save All Prefabs")]
		public static void ForceSaveAllPrefabs()
		{
			// 프로젝트 내의 모든 프리팹 GUID 검색
			string[] guids = AssetDatabase.FindAssets("t:Prefab");

			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
            
				// 프리팹을 메모리에 로드 (이때 OnAfterDeserialize 실행되면서 c -> d 복사됨)
				GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
				if (prefab != null)
				{
					// 디스크에 물리적으로 다시 쓰도록 강제 세팅 및 Reimport
					EditorUtility.SetDirty(prefab);
					AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
				}
			}

			AssetDatabase.SaveAssets();
			Debug.Log("모든 프리팹 마이그레이션 완료!");
		}
	}
}
#endif
