using UnityEngine.SceneManagement;

namespace UengSystem.StageManager {
	public static class USceneManager {

		// 정적 프로퍼티
		private static string currentUSceneName = "Game";

		// 정적 메서드
		public static void StartStage() {
			GameManager.OnSceneUnload();
			SceneManager.LoadScene(currentUSceneName+"Bridge");
		}

		public static void LoadMainMenu() {
			GameManager.OnSceneUnload();
			SceneManager.LoadScene("MainMenuBridge");
		}
		
		public static void SetStage(string sceneName) {
			currentUSceneName = sceneName;
		}

		public static void SetStage(UScenes uScenes) {
			currentUSceneName = uScenes.ToString();
		}
	}
}