using System;
using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Interactions;
using AncientMemorial.Map;
using AncientMemorial.Tasks;
using AncientMemorial.Waves;
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
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AncientMemorial {
	public class GameManager : Manager<GameManager> {
		
		// NOT INITIALIZE ON RESETSTATICVARIABLES()
		
		public Crystal    Crystal;
		public UCanvas    mainScreenCanvas;
		public UCanvas    mainWorldCanvas;
		public MainCamera mainCamera;
		public GameObject sceneHider;
		
		public Color spawnColor;
		
		public int currentUpdatePhase;
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

		private Dictionary<string, Func<bool>[]> initializeFunctions = new Dictionary<string, Func<bool>[]>{
			{"Game", new Func<bool>[] {
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
				}}
			},
			{"MainMenu", new Func<bool>[] {
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
				}}
			}
		};
		
		IEnumerator InitializeGame(string SceneName) {

			if (Setting.data == null) Setting.LoadData();
			
			yield return new WaitUntil(() => {
				Debug.Log("Looking For ScreenCanvas");
				return mainScreenCanvas;
			});

			Slider loadingSlider = null;

			int   iCount    = initializeFunctions[SceneName].Length;
			float iComplete = 0;
			foreach (Func<bool> initializeFunction in initializeFunctions[SceneName]) {
				yield return new WaitForSeconds(0.1f);
				yield return new WaitUntil(initializeFunction);
				loadingSlider ??= ((USlider)UUI.GetUUI("loadingUI").GetAction<USliderAction>("Loading").component).slider;
				if (loadingSlider) {
					Destroy(sceneHider);
					loadingSlider.value = 1f*(++iComplete/iCount);
				}
				
				Debug.Log($"Initialize Completed {100f*(iComplete/iCount)}%");
			}

			yield return new WaitForSeconds(0.25f);
			
			Debug.Log("Initializing Done!");
			UUIObjectPool.instance.Close(UUI.GetUUI("loadingUI").gameObject);
			
			initialized                        = true;
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
			Entity.entities?.ClearImmediatly();
			
			// Interaction
			Interaction.InteractableInteractions?.Clear();
			
			// AnnounceQueue
			AddAnnounceQueue.announceQueue?.Clear();
			
			// InputManager
			InputManager.inputData?.Clear();
			
			// IManager
			IManager.instances?.Clear();
			
			// AdvancedUObject
			AdvancedUObject.AdvancedInstances?.ClearImmediatly();
			
			// UObject
			UObject.ResetUObjects();
			
			// UUI
			UUI.ResetUUI();
		}

		private bool s = false;
		private void Update() {
			if (!initialized) return;
			
			ApplyStaticBufferedLists();
			
			foreach (IManager manager in IManager.instances) { manager.ManagerUpdate(); }

			if (InputManager.inputData[InputActionType.MouseMClick].pressType == InputPressType.Up) {
				foreach (UObject obj in UObject.Instances) {
					Debug.Log(s?$"{obj} RESUME":$"{obj} STOPPED");
					
					if (!s) obj.Stop();
					if (s) obj.Resume();
				}
				
				s = !s;
			}
			
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
		
		public override void ManagerUpdate()      { }
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