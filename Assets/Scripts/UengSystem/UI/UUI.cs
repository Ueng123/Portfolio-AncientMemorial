using System;
using System.Collections;
using System.Collections.Generic;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Objects.LifeCycle;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.UI {
	public class UUI : UObject {

		// 정적 프로퍼티
		private static readonly Dictionary<string, List<UUI>> UCategoryTable = new ();
		private static readonly int                           Close          = Animator.StringToHash("Close");

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

		public             float  openTime;
		public             float  closeTime;
		public             string gettingAnimation = "open";
		public override    float  usingGettingDuration   => openTime;
		public override    float  usingReleasingDuration => closeTime;
		protected override float  DeltaTime              => Time.unscaledDeltaTime;
		
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
			UUI Target = UUIPool.instance.Get(PrefabId, Canvas);
			Target.StartGet(PlayEffect, Obj => Configure?.Invoke(Obj.To<UUI>()));
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
			throw new KeyNotFoundException($"{key} action not found");
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

		protected override void ObjectConfigure() {
			rectTransform.anchoredPosition = initialPosition;
			rectTransform.localScale = initialScale;
			Opened = false;
			if (animator) {
				animator.updateMode = AnimatorUpdateMode.UnscaledTime;
			}
		}
		
		protected override void Routine() {
			foreach (UUIAction action in actionDict.Values) {
				if (!isActive) return;
				action.Routine(this);
			}
		}

		protected override void OnGet() {
			foreach (UUIActionListItem action in actions) {
				InitializedActions.Add(action.action);
				action.action.Initialize(this);
				if (lifeStateMachine.phase != LifePhase.Getting) return;
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
