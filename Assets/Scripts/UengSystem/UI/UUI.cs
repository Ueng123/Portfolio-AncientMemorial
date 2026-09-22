using System;
using System.Collections;
using System.Collections.Generic;
using UengSystem.Objects;
using UengSystem.Objects.LifeCycle;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.UI {
	public class UUI : UObject {

		// 정적 프로퍼티
		private static readonly Dictionary<string, List<UUI>> UCategoryTable = new ();
		private static readonly int                           Close          = Animator.StringToHash("Close");
		private static readonly int                           Open           = Animator.StringToHash("Open");

		// 인스턴스 프로퍼티
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
		[SerializeField] private string GettingAnimation = "open";
		public string gettingAnimation => GettingAnimation;
		public override float usingGettingDuration => openTime;
		public override float usingReleasingDuration => closeTime;
		public override float lifeCycleDeltaTime => Time.unscaledDeltaTime;
		
		[Header("UUI")]
		public UUIActionListItem[] actions;
		private Dictionary<string, UUIAction> actionDict = new();
		private readonly List<UUIAction> InitializedActions = new();
		private bool Opened;

		// 정적 메서드
		public static GameObject Get(string PrefabKey, UCanvas Canvas, bool PlayEffect = true, Action<UUI> Configure = null) {
			if (PrefabKey == null) throw new ArgumentNullException(nameof(PrefabKey));
			return Get(PrefabKey.GetHash(), Canvas, PlayEffect, Configure);
		}

		public static GameObject Get(int PrefabId, UCanvas Canvas, bool PlayEffect = true, Action<UUI> Configure = null) {
			CheckAcquireAllowed();
			UUI Target = UUIPool.instance.Acquire(PrefabId, Canvas);
			Target.BeginLife(PlayEffect, Obj => Configure?.Invoke(Obj.To<UUI>()));
			return Target.gameObject;
		}

		public static bool TryGetUUI(string id, out UUI uui) {
			bool result = TryGetUObject(id, out UObject obj);
			uui = obj.To<UUI>();
			return result;
		}

		public static UUI GetUUI(string id) => GetUObject(id) as UUI;

		public static List<UUI> GetUUIs(string category) => UCategoryTable[category];

		public static void ResetUUI() {
			UCategoryTable.Clear();
		}

		// 인스턴스 메서드
		public virtual void OnOpen()  { }
		public virtual void OnClose() { }

		public T GetAction<T>(string key) where T : UUIAction {
			if (actionDict.TryGetValue(key, out UUIAction action)) return action.To<T>();
			throw new KeyNotFoundException(key);
		}
		
		public T GetAction<T>(int index) where T : UUIAction {
			return actions[index].action.To<T>();
		}

		// 오버라이드 메서드
		public override void OnFirstGet() {
			SetDefaultStates(new UUIGetting(this), new UUIReleasing(this));
			base.OnFirstGet();
			rectTransform = GetComponent<RectTransform>();
			if (animator) animator.updateMode = AnimatorUpdateMode.UnscaledTime;
			foreach (UUIActionListItem Item in actions) actionDict.Add(Item.key, Item.action);
		}

		protected override void PrepareContext() {
			rectTransform.anchoredPosition = initialPosition;
			rectTransform.localScale = initialScale;
			Opened = false;
			if (animator) {
				animator.updateMode = AnimatorUpdateMode.UnscaledTime;
				animator.ResetTrigger(Close);
			}
		}
		
		protected override void Routine() {
			foreach (UUIAction action in actionDict.Values) {
				if (!isActive) return;
				action.Routine(this);
			}
		}

		public override void OnGet() {
			foreach (UUIActionListItem action in actions) {
				InitializedActions.Add(action.action);
				action.action.Initialize(this);
				if (lifeCycle.phase != LifeCyclePhase.Getting) return;
			}
			Opened = true;
			OnOpen();
		}

		protected override void OnRelease() {
			foreach (UUIAction Action in InitializedActions) Action.Uninitialize(this);
			InitializedActions.Clear();
			if (Opened) { Opened = false; OnClose(); }
			
			UCategory = null;
			base.OnRelease();
		}
	}
}
