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
		private string  _UCategory;

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

		protected abstract void OnOpen();
		protected abstract void OnClose();

		public override void Initialize() {
			foreach (UUIActionListItem action in actions) {
				action.action.component.Initialize();
				actionDict[action.key] = action.action;
			}
			base.Initialize();
		}

		public static UUI GetUUI(string id) => GetUObject(id) as UUI;
		
		public static List<UUI> GetUUIs(string category) {
			return UCategoryTable[category];
		}

		public UUIAction GetAction(string key) {
			if (actionDict.TryGetValue(key, out UUIAction action)) return action;
			else throw new KeyNotFoundException(key);
		}

		protected override void Routine() {
			Debug.Log("HELLO :) I AM FUCKING HANDSOMEGUY DO U KNOW TAHT MAMERL IS FUCING SHORT????");
			
			foreach (UUIAction action in actionDict.Values) {
				action.Routine(this);
			}
		}

		public override void OnGet() { OnOpen(); }
		public override void OnRelease() { OnClose(); }

		protected override IEnumerator SpawnFX(float duration) {
			animator.SetTrigger(Open);
			yield return new WaitForSeconds(duration);
			Initialize();
		}

		public override    IEnumerator ReleaseFX(float duration) {
			animator.SetTrigger(Close);
			yield return new WaitForSeconds(duration);
			UUIManager.instance.Close(gameObject, true);
		}
	}
}