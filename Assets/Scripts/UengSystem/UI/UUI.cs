using System;
using System.Collections;
using System.Collections.Generic;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.UI {
	public abstract class UUI : AdvancedUObject {
		
		private static readonly Dictionary<string, List<UUI>> UCategoryTable = new ();
		private static readonly int                           Close          = Animator.StringToHash("Close");
		private static readonly int                           Open           = Animator.StringToHash("Open");
		
		// Instance Variables //
		[Header("Identify")] 
		public  UCanvas canvas;

		protected RectTransform rectTransform;
		private   string        _UCategory;

		public Vector2 initialPosition;
		public Vector3 initialScale;

		public string UCategory {
			get => _UCategory;
			set {
				if (value == null) {
					if (_UCategory != null) {
						UCategoryTable[_UCategory].Remove(this);
						if (UCategoryTable[_UCategory].Count == 0) UCategoryTable.Remove(_UCategory);
	                    
						_UCategory = null;
					}
					
					return;
				}
				
				if (_UCategory != null) throw new InvalidOperationException("u just tried to set Category twice twin :(");
				if (!UCategoryTable.ContainsKey(value)) UCategoryTable[value] = new List<UUI>();
				
				UCategoryTable[value].Add(this);
				_UCategory = value;
			}
		}
		
		[Header("UUI")]
		public UUIActionListItem[] actions;
		private Dictionary<string, UUIAction> actionDict = new();

		public abstract void OnOpen();
		public abstract void OnClose();

		public static UUI GetUUI(string id) => GetUObject(id) as UUI;

		public static List<UUI> GetUUIs(string category) => UCategoryTable[category];

		public T GetAction<T>(string key) where T : UUIAction {
			if (actionDict.TryGetValue(key, out UUIAction action)) return (T)action;
			else throw new KeyNotFoundException(key);
		}

		public static void ResetUUI() {
			UCategoryTable.Clear();
		}

		// FUCK NAZETE WORK 안함 REALLY YAMA ROTATING FUCK
		// Done :_)
		protected override void Routine() {
			foreach (UUIAction action in actionDict.Values) {
				action.Routine(this);
			}
		}

		public override void OnGet() {
			foreach (UUIActionListItem action in actions) {
				action.action.Initialize(this);
				actionDict[action.key] = action.action;
			}
			
			rectTransform = GetComponent<RectTransform>();
			
			rectTransform.anchoredPosition = initialPosition;
			rectTransform.localScale       = initialScale;
		}

		public override void Get(float time) {
			OnGet();
			gameObject.SetActive(true);
			if (time == 0) return;
			StartCoroutine(SpawnFX(time));
		}
		
		public override void Release(float time) {
			
			gettable = false;
			ID        = null;
			Category  = null;
			
			switch (time) {
				case < 0:
					Uninitialize();
					gettable = true;
					break;
				case 0:
					AdvancedInstances.Remove(this);
					Debug.Log($"[UObject] Instance {gameObject.name} Removed");
					
					OnRelease();
					Uninitialize();
					gettable = true;
					break;
				default:
					AdvancedInstances.Remove(this);
					gameObject.SetActive(true);
					Debug.Log($"[UObject] Instance {gameObject.name} Removed");
					
					OnRelease();
					StartCoroutine(DespawnFX(time));
					break;
			}
		}

		protected override void OnRelease() {
			UCategory = null;
		}
		
		protected override IEnumerator SpawnFX(float duration) {
			yield return new WaitForSecondsRealtime(duration);
			Initialize();
		}

		protected override IEnumerator DespawnFX(float duration) {
			animator.SetTrigger(Close);
			yield return new WaitForSeconds(duration);
			UUIObjectPool.instance.Close(gameObject, true);
		}
	}
}