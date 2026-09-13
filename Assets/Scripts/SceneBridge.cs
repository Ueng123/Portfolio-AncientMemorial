using System.Collections;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneBridge : MonoBehaviour {

	// 인스턴스 프로퍼티
	public string sceneToLoad;
	public Slider loadingSlider;

	// 인스턴스 메서드
	public void Start() {
		StartCoroutine(LoadAsyncRoutine(sceneToLoad));
	}

	private IEnumerator LoadAsyncRoutine(string sceneName) {
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
		asyncOperation.allowSceneActivation = false;
		
		while (!asyncOperation.isDone)
		{
			float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
			
			loadingSlider.value = progress;
			
			if (asyncOperation.progress >= 0.9f) {
				loadingSlider.value = 1;
				yield return CacheManager.WaitForSeconds(0.25f);
				asyncOperation.allowSceneActivation = true;
			}

			yield return null;
		}
	}
}
