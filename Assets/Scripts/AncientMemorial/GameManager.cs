using System;
using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Interactions;
using AncientMemorial.Map;
using AncientMemorial.Tasks;
using AncientMemorial.Waves;
using UengSystem.Audio;
using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UBools;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Settings;
using UengSystem.UI;
using UengSystem.UI.USliders;
using UengSystem.Utility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AncientMemorial {
	public class GameManager : Manager<GameManager> {
		
		public Crystal    Crystal;
		public UCanvas    mainScreenCanvas;
		public UCanvas    mainWorldCanvas;
		public MainCamera mainCamera;
		public GameObject sceneHider;

		public AudioMixer audioMixer;
		
		public Color spawnColor;
		
		public int currentUpdatePhase;
		public int currentLateUpdatePhase;
		public int currentFixedUpdatePhase;
		
		public static readonly Dictionary<string, UValue<float  >> UValueFloatVariables   = new ();
		public static readonly Dictionary<string, UValue<string >> UValueStringVariables  = new ();
		public static readonly Dictionary<string, UValue<bool   >> UValueBoolVariables    = new ();
		public static readonly Dictionary<string, UValue<Color  >> UValueColorVariables   = new ();
		public static readonly Dictionary<string, UValue<UObject>> UValueUObjectVariables = new ();
		public static readonly Dictionary<string, UValue<UUI    >> UValueUUIVariables     = new ();

		private bool initialized = false;
		
		public static void ApplyStaticBufferedLists() {
			UObject.Instances.Apply();
			Entity.entities.Apply();
		}
		
		public static void ClearFramePerLists() {
			EventManager.instance.RemoveAllEvents();
			Interaction.InteractableInteractions.Clear();
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
			StartCoroutine(InitializeGame(scene.name));
		}

		private static float timeScale = 1f;
		
		// startTime:unscaledTime
		private static List<(float timeScale, float startTime, float duration)> timeScaleChangeChain = new();
		
		public static float GetTimeScale() {
			return timeScaleChangeChain.Count==0?timeScale:timeScaleChangeChain[0].timeScale;
		}
		
		public static void SetTimeScale(float timeScale) {
			GameManager.timeScale = timeScale;
		}
		
		private DelayedAction timeRestoreAction;
		public static void SetTimeScale(float timeScale, float duration) {
			float                 startTime = Time.unscaledTime;
			(float, float, float) thisItem  = (timeScale, startTime, duration);
			
			if (timeScaleChangeChain.Count == 0) {
				timeScaleChangeChain.Add(thisItem);
				return;
			}

			for (int i = 0; i < timeScaleChangeChain.Count; i++) {
				(float timeScale, float startTime, float duration) item = timeScaleChangeChain[i];

				if (!(duration < item.duration)) continue;
				timeScaleChangeChain.Insert(i, thisItem);
				return;
			}
			
			timeScaleChangeChain.Add(thisItem);
		}
		
		private static readonly Dictionary<string, Func<bool>[]> initializeFunctions = new() { 
			{ "Game", new Func<bool>[] { 
					() => {
						Debug.Log("Looking For MainScreen Canvas");
						
						return instance.mainScreenCanvas;
					},
					() => {
						Debug.Log("Looking For UUIObjectPool...");

						if (!UUIObjectPool.instance) return false;
						UUIObjectPool.instance.Initialize();

						GameObject gameLoadObj = UUIObjectPool.instance.Open("GameLoadingUI", instance.mainScreenCanvas);
						UUI        gameLoadUI  = gameLoadObj.GetComponent<UUI>();
						gameLoadUI.ID = "loadingUI";

						return true;
					},
					() => {
						Debug.Log("Looking For InputManager...");

						if (!InputManager.instance) return false;
						InputManager.instance.Initialize();
						return true;
					},
					() => {
						Debug.Log("Looking For MapManager...");

						if (!MapManager.instance) return false;
						MapManager.instance.Initialize();
						return true;
					},
					() => {
						Debug.Log("Looking For AudioManager...");

						if (!AudioManager.instance) return false;
						AudioManager.instance.Initialize();
						return true;
					},
					() => {
						Debug.Log("Looking For UObjectPool...");

						if (!UObjectPool.instance) return false;
						UObjectPool.instance.Initialize();
						return true;
					},
					() => {
						Debug.Log("Looking For WaveManager...");

						if (!WaveManager.instance) return false;
						WaveManager.instance.Initialize();
						return true;
					},
					() => {
						if (Setting.data != null) return true;
						Debug.Log("Loading Settings...");

						Setting.LoadData();
						return true;
					},
					() => {
						if (!EntityData.GetInitialized()) {
							EntityData.Initialize();
						}

						return true;
					}
				}
			}, 
			{ "MainMenu", new Func<bool>[] {
					() => {
						Debug.Log("Looking For MainScreen Canvas");
					
						return instance.mainScreenCanvas;
					},
					() => {
						Debug.Log("Looking For UUIObjectPool...");

						if (!UUIObjectPool.instance) return false;
						UUIObjectPool.instance.Initialize();

						GameObject gameLoadObj = UUIObjectPool.instance.Open("GameLoadingUI", instance.mainScreenCanvas);
						UUI        gameLoadUI  = gameLoadObj.GetComponent<UUI>();
						gameLoadUI.ID = "loadingUI";

						return true;
					},
					() => {
						Debug.Log("Looking For InputManager...");

						if (!InputManager.instance) return false;
						InputManager.instance.Initialize();
						return true;
					},
					() => {
						Debug.Log("Looking For AudioManager...");

						if (!AudioManager.instance) return false;
						AudioManager.instance.Initialize();
						return true;
					},
					() => {
					Debug.Log("Looking For UObjectPool...");

					if (!UObjectPool.instance) return false;
					UObjectPool.instance.Initialize();
					return true;
					},
					() => {
						Debug.Log("Looking For WaveManager...");

						if (!WaveManager.instance) return false;
						WaveManager.instance.Initialize();
						return true;
					},
					() => {
						if (Setting.data != null) return true;
						Debug.Log("Loading Settings...");

						Setting.LoadData();
						return true;
					}
				}
			}
		};

		public static float valueToDB(float value) {
			if (Mathf.Approximately(value, 0f)) return -80f;
			value = Mathf.Clamp(value, 0.0001f, 1f);
			return Mathf.Log10(value) * 20f;
		}

		private IEnumerator InitializeGame(string SceneName) {
			SetTimeScale(1);

			Slider loadingSlider = null;
			
			int   iCount    = initializeFunctions[SceneName].Length;
			float iComplete = 0;
			foreach (Func<bool> initializeFunction in initializeFunctions[SceneName]) {
				yield return new WaitForSeconds(0.01f);
				yield return new WaitUntil(initializeFunction);
				if (!loadingSlider && UUI.GetUUI("loadingUI")) loadingSlider ??= ((USlider)UUI.GetUUI("loadingUI").GetAction<USliderAction>("Loading").component).slider;
				if (loadingSlider) {
					Destroy(sceneHider);
					loadingSlider.value = 1f*(++iComplete/iCount);
				}
				
				Debug.Log($"Initialize Completed {100f*(iComplete/iCount)}%");
			}
			
			yield return new WaitForSeconds(0.1f);
			
			Debug.Log("Initializing Done!");
			UUIObjectPool.instance.Close(UUI.GetUUI("loadingUI").gameObject);
			
			initialized = true;
		}

		public static void ResetStaticVariables() {
			Time.timeScale = 1f;
			
			// CameraBrain
			CameraBrain.instance = null;
			
			// GameManager
			UValueFloatVariables.Clear();
			UValueStringVariables.Clear();
			UValueBoolVariables.Clear();
			UValueColorVariables.Clear();
			UValueUObjectVariables.Clear();
			UValueUUIVariables.Clear();
			
			// Entity
			Entity.player   = null;
			Entity.entities?.ClearImmediately();
			
			// Interaction
			Interaction.InteractableInteractions?.Clear();
			
			// AnnounceQueue
			AddAnnounceQueue.announceQueue?.Clear();
			
			// InputManager
			InputManager.inputData?.Clear();
			
			// IManager
			IManager.instances?.Clear();
			
			// AdvancedUObject
			AdvancedUObject.AdvancedInstances?.ClearImmediately();
			
			// UObject
			UObject.ResetUObjects();
			
			// UUI
			UUI.ResetUUI();
		}
		
		private void Update() {
			if (!initialized) return;
			
			ApplyStaticBufferedLists();
			
			foreach (IManager manager in IManager.instances) { manager.ManagerUpdate(); }
			
			InputManager.instance.UpdateInputs();
			AdvancedUObject.UpdateRoutine();
			AdvancedUObject.EventRoutine();
		}
		
		private void LateUpdate() {
			if (!initialized) return;
			
			AdvancedUObject.LateUpdateRoutine();
			
			ClearFramePerLists();
		}
		
		private void FixedUpdate() {
			if (!initialized) return;
			
			foreach (IManager manager in IManager.instances) { manager.ManagerFixedUpdate(); }
			
			AdvancedUObject.FixedUpdateRoutine();
		}

		public override void ManagerUpdate() {
			// 검토
			for (int i = timeScaleChangeChain.Count - 1; i >= 0; i--) {
				(float timeScale, float startTime, float duration) item = timeScaleChangeChain[i];
				if (Time.unscaledTime >= item.startTime + item.duration) timeScaleChangeChain.RemoveAt(i);
			}
			
			// 적용
			Time.timeScale = GetTimeScale();
		}
		
		public override void ManagerFixedUpdate() { }

		private void OnEnable() {
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnDisable() {
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		private void OnDestroy() {
			ResetStaticVariables();
		}
	}
}