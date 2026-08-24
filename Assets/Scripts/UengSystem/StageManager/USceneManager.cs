using UnityEngine.SceneManagement;

namespace UengSystem.StageManager {
	public static class USceneManager {
		private static string currentUSceneName = "Game";

		public static void StartStage() {
			GameManager.OnSceneUnload();
			SceneManager.LoadScene(currentUSceneName+"Bridge");
		}

		public static void LoadMainMenu() {
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