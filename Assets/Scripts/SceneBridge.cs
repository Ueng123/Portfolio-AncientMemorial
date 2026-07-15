using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneBridge : MonoBehaviour {
	public string sceneToLoad;
	public Slider loadingSlider;
	
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
				yield return new WaitForSeconds(0.25f);
				asyncOperation.allowSceneActivation = true;
			}

			yield return null;
		}
	}
}