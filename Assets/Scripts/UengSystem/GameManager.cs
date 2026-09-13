using UengSystem.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Interactions;
using AncientMemorial.Map;
using AncientMemorial.Waves;
using AncientMemorial.Weapons;
using UengSystem.Audio;
using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.SaveDatas.SettingDatas;
using UengSystem.UAction;
using UengSystem.UDebug;
using UengSystem.UI;
using UengSystem.UI.USliders;
using UengSystem.VisualScripting.Tasks;
using UengSystem.VisualScripting.UVariables;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UengSystem {
    public class GameManager : Manager<GameManager> {

       // 정적 프로퍼티
       private static readonly int GameLoadingUIPrefabId = "GameLoadingUI".GetHash();
       private static float timeScale = 1f;
       
       // startTime:unscaledTime
       private static List<(float timeScale, float startTime, float duration)> timeScaleChangeChain = new();
       
       private static readonly Func<bool>[][] initializeFunctions = new[] { 
          new Func<bool>[] { 
                () => {
                   DebugManager.Log("Looking For MainScreen Canvas");
                   
                   return instance.mainScreenCanvas;
                },
                () => {
                   DebugManager.Log("Looking For UUIObjectPool...");

                   if (!UUIPool.instance) return false;
                   UUIPool.instance.Initialize();

                   UUI.Get(GameLoadingUIPrefabId, instance.mainScreenCanvas, Configure: Ui => Ui.ID = "loadingUI", PlayEffect:false);

                   return true;
                },
                () => {
                   DebugManager.Log("Looking For InputManager...");

                   if (!InputManager.instance) return false;
                   InputManager.instance.Initialize();
                   return true;
                },
                () => {
                   DebugManager.Log("Looking For MapManager...");

                   if (!MapManager.instance) return false;
                   MapManager.instance.Initialize();
                   return true;
                },
                () => {
                   DebugManager.Log("Looking For AudioManager...");

                   if (!AudioManager.instance) return false;
                   AudioManager.instance.Initialize();
                   return true;
                },
                () => {
                   DebugManager.Log("Looking For UObjectPool...");

                   if (!UObjectPool.instance) return false;
                   UObjectPool.instance.Initialize();
                   return true;
                },
                () => {
                   DebugManager.Log("Loading SaveDatas");

                   Setting.Load();
                   
                   return Setting.Loaded;
                },
                () => {
                   if (!EntityData.GetInitialized()) {
                      EntityData.Initialize();
                   }

                   return true;
                },
                () => {
                   DebugManager.Log("Looking For WaveManager...");

                   if (!WaveManager.instance) return false;
                   WaveManager.instance.Initialize();
                   return true;
                }
             },
          new Func<bool>[] {
                () => {
                   DebugManager.Log("Looking For MainScreen Canvas");
                
                   return instance.mainScreenCanvas;
                },
                () => {
                   DebugManager.Log("Looking For UUIObjectPool...");

                   if (!UUIPool.instance) return false;
                   UUIPool.instance.Initialize();

                   UUI.Get(GameLoadingUIPrefabId, instance.mainScreenCanvas, Configure: Ui => Ui.ID = "loadingUI", PlayEffect:false);

                   return true;
                },
                () => {
                   DebugManager.Log("Looking For InputManager...");

                   if (!InputManager.instance) return false;
                   InputManager.instance.Initialize();
                   return true;
                },
                () => {
                   DebugManager.Log("Looking For AudioManager...");

                   if (!AudioManager.instance) return false;
                   AudioManager.instance.Initialize();
                   return true;
                },
                () => {
                   DebugManager.Log("Looking For UObjectPool...");

                   if (!UObjectPool.instance) return false;
                   UObjectPool.instance.Initialize();
                   return true;
                },
                () => {
                   DebugManager.Log("Loading SaveDatas");

                   Setting.Load();
                   
                   return Setting.Loaded;
                },
                () => {
                   DebugManager.Log("Looking For WaveManager...");

                   if (!WaveManager.instance) return false;
                   WaveManager.instance.Initialize();
                   return true;
                }
             }
       };

       // 인스턴스 프로퍼티
       public Crystal    Crystal;
       public UCanvas    mainScreenCanvas;
       public UCanvas    mainWorldCanvas;
       public MainCamera mainCamera;
       public GameObject sceneHider;

       public Weapon[] weapons;
       
       public AudioMixer audioMixer;
       
       public Color spawnColor;
       
       public UpdateRoutineType      currentUpdatePhase;
       public FixedUpdateRoutineType currentFixedUpdatePhase;

       public  int  initializeID;
       private bool initialized = false;
       
       private DelayedAction timeRestoreAction;

       private Slider loadingSlider = null;

       // 정적 메서드
       public static void SyncronizeLists() { 
          Entity.entities.Synchronize();
       }
       
       public static void ClearFramePerLists() {
          EventManager.instance.RemoveAllEvents();
          Interaction.InteractableInteractions.Clear();
       }
       
       public static float GetTimeScale() {
          if (!Setting.GetBool(SettingType.SlowFX)) return timeScale;
          if (timeScale == 0) return 0;
          return timeScaleChangeChain.Count==0?timeScale:timeScaleChangeChain[0].timeScale;
       }
       
       public static void SetTimeScale(float timeScale) {
          GameManager.timeScale = timeScale;
       }
       
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

       public static float valueToDB(float value) {
          if (Mathf.Approximately(value, 0f)) return -80f;
          value = Mathf.Clamp(value, 0.0001f, 1f);
          return Mathf.Log10(value) * 20f;
       }

       public static void ResetStaticVariables() {
          // CameraBrain
          CameraBrain.instance = null;
          
          // GameManager
          IUValueVariable.Clear();
          
          // Entity
          Entity.player   = null;
          Entity.entities?.ClearImmediately();
          
          // Interaction
          Interaction.InteractableInteractions?.Clear();
          
          // AnnounceQueue
          AddAnnounceQueue.announceQueue?.Clear();
          
          // InputManager
          InputManager.Clear();
          
          // IManager
          IManager.instances?.Clear();
          
          // UObject
          UObject.ResetUObjects();
          
          // UUI
          UUI.ResetUUI();
       }
       
       public static void OnSceneUnload() {
          UObject.ShutdownLifeCycles();
          Time.timeScale = 1;
          SetTimeScale(1);
          ResetStaticVariables();
       }

       // 인스턴스 메서드
       private bool GetLoadingSlider() {
          if (!loadingSlider && UUI.GetUUI("loadingUI")) {
             Destroy(sceneHider);
             loadingSlider = UUI.GetUUI("loadingUI").GetAction<USliderAction>("Loading").GetComponent<USlider>().slider;
          }

          return loadingSlider;
       }

       private void OnInitializeStepComplete(float stepCompleted, float stepCount) {
          if (!GetLoadingSlider()) return;
          loadingSlider.value = 1f*(stepCompleted/stepCount);
       }
       
       private IEnumerator InitializeGame() {
          UObject.BeginSceneLifeCycles();
          
          int   iCount    = initializeFunctions[initializeID].Length;
          float iComplete = 0;
          foreach (Func<bool> initializeFunction in initializeFunctions[initializeID]) {
             yield return CacheManager.WaitForSeconds(0.01f);
             yield return new WaitUntil(initializeFunction);
             
             OnInitializeStepComplete(++iComplete, iCount);
             
             DebugManager.Log($"Initialize Completed {100f*(iComplete/iCount)}%");
          }
          
          yield return CacheManager.WaitForSeconds(0.1f);
          
          DebugManager.Log("Initializing Done!");
          UUI.GetUUI("loadingUI").Release(PlayEffect: false);
          
          initialized = true;
       }

       private void UpdateTimeScaleChain() {
          for (int i = timeScaleChangeChain.Count - 1; i >= 0; i--) {
             (float timeScale, float startTime, float duration) item = timeScaleChangeChain[i];
             if (Time.unscaledTime >= item.startTime + item.duration) timeScaleChangeChain.RemoveAt(i);
          }
       }
       
       private void Update() {
          if (!initialized) return;

          try { SyncronizeLists(); }
          catch (Exception e) {
             DebugManager.LogError("Update > GameManager.SyncronizeLists()", e, gameObject);
          }

          try { InputManager.instance.UpdateInputs(); }  
          catch (Exception e) {
             DebugManager.LogError("Update > InputManager.UpdateInputs()", e, InputManager.instance.gameObject);
          }

          foreach (IManager manager in IManager.instances) {
             try {
                manager.ManagerUpdate();
             }
             catch (Exception e) {
                DebugManager.LogError($"Update > {manager.GetType().Name}.ManagerUpdate()", e,
                                      ((MonoBehaviour)manager).gameObject);
             }
          }

          UObject.UpdateRoutine();
          
          try { EventManager.instance.EventRoutine(); }
          catch (Exception e) {
             DebugManager.LogError("Update > EventManager.EventRoutine()", e, gameObject);
          }
       }

       private void LateUpdate() {
          if (!initialized) return;
          
          UObject.LateUpdateRoutine();
          UObject.LifeCycleRoutine();
          
          try { ClearFramePerLists(); }
          catch (Exception e) {
             DebugManager.LogError("LateUpdate > GameManager.ClearFramePerLists()", e, gameObject);
          }
       }
       
       private void FixedUpdate() {
          if (!initialized) return;
          foreach (IManager manager in IManager.instances) {
             try {
                manager.ManagerFixedUpdate();
             }
             catch (Exception e) {
                DebugManager.LogError($"FixedUpdate > {manager.GetType().Name}.ManagerFixedUpdate()", e, ((MonoBehaviour)manager).gameObject);
             }
          }

          UObject.FixedUpdateRoutine();
       }

       private void OnEnable() {
          StartCoroutine(InitializeGame());
       }

       // 오버라이드 메서드
       public override void ManagerUpdate() {
          UpdateTimeScaleChain();
          
          // 적용
          Time.timeScale = GetTimeScale();
       }
    }
}
