using System;
using System.Collections;
using System.Collections.Generic;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.UI {
	public class UUI : UObject {
		
		private static readonly Dictionary<string, List<UUI>> UCategoryTable = new ();
		private static readonly int                           Close          = Animator.StringToHash("Close");
		private static readonly int                           Open           = Animator.StringToHash("Open");
		
		// Instance Variables //
		[Header("Identify")] 
		public  UCanvas canvas;

		public RectTransform rectTransform;
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

		public float openTime;
		public float closeTime;
		
		[Header("UUI")]
		public UUIActionListItem[] actions;
		private Dictionary<string, UUIAction> actionDict = new();

		public virtual void OnOpen()  { }
		public virtual void OnClose() { }

		public static UUI GetUUI(string id) => GetUObject(id) as UUI;

		public static List<UUI> GetUUIs(string category) => UCategoryTable[category];

		public T GetAction<T>(string key) where T : UUIAction {
			if (actionDict.TryGetValue(key, out UUIAction action)) return (T)action;
			throw new KeyNotFoundException(key);
		}
		
		public T GetAction<T>(int index) where T : UUIAction {
			return (T)actions[index].action;
		}

		public static void ResetUUI() {
			UCategoryTable.Clear();
		}
		
		protected override void Routine() {
			foreach (UUIAction action in actionDict.Values) {
				action.Routine(this);
			}
		}

		public override void Get(float time) {
			OnGet();
			gameObject.SetActive(true);
			
			if (time == 0) {
				Initialize();
				return;
			}
			
			StartCoroutine(SpawnFX(time));
		}
		
		public override void OnGet() {
			foreach (UUIActionListItem action in actions) {
				action.action.Initialize(this);
				actionDict[action.key] = action.action;
			}
			
			GetTask.Execute(this);
			
			rectTransform = GetComponent<RectTransform>();
			
			rectTransform.anchoredPosition = initialPosition;
			rectTransform.localScale       = initialScale;
		}

		protected override void OnRelease() {
			foreach (UUIActionListItem action in actions) {
				action.action.Uninitialize(this);
			}
			
			ReleaseTask.Execute(this);
			
			UCategory = null;
			base.OnRelease();
		}

		protected override void PrepareSpawnFX() { }

		protected override void FinishSpawnFX() { }

		protected override IEnumerator SpawnFX(float duration) {
			yield return new WaitForSecondsRealtime(duration);
			Initialize();
		}

		protected override void PrepareDespawnFX() { }

		protected override IEnumerator DespawnFX(float duration) {
			if (animator) animator.SetTrigger(Close);
			yield return new WaitForSeconds(duration);
			UUIPool.instance.Close(gameObject, true);
		}
	}
}