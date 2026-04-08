using System;
using System.Collections.Generic;
using UengSystem.Events;
using UengSystem.Managers;
using UengSystem.Utility;
using UnityEngine;
using Events_Event = UengSystem.Events.Event;

namespace UengSystem.Objects {
	public abstract class AdvancedUObject : UObject {

		public static readonly BufferedList<AdvancedUObject> Instances = new ();
		
		private List<Action>[] processToUpdate;
		private List<Action>[] processToFixedUpdate;
		
		private        Action[] updatePhaseRoutines;
		private        Action[] fixedUpdatePhaseRoutine;
		
		private const int updatePhaseCount      = 3;
		private const int fixedUpdatePhaseCount = 2;
		
		public static void UpdateRoutine() {
			for (int i = 0; i < updatePhaseCount; i++) {
				GameManager.instance.currentUpdatePhase = i;
				
				Instances.Apply();
				foreach (AdvancedUObject ins in Instances) {
					ins.updatePhaseRoutines[i].Invoke();
				}
			}
		}
		
		public static void LateUpdateRoutine() {
			foreach (AdvancedUObject ins in Instances) {
				ins.LateRoutine();
			}
		}
		
		public static void FixedUpdateRoutine() {
			for (int i = 0; i < fixedUpdatePhaseCount; i++) {
				GameManager.instance.currentFixedUpdatePhase = i;
				
				Instances.Apply();
				foreach (AdvancedUObject ins in Instances) {
					ins.fixedUpdatePhaseRoutine[i].Invoke();
				}
			}
		}

		public static void EventRoutine() {
			List<Events_Event> events = EventManager.instance.GetEvents();
			
			foreach (Events_Event e in events) {
				foreach (AdvancedUObject ins in Instances) {
					ins.OnEvent(e);
				}
			}
		}
		
		protected abstract void EarlyRoutine();
		protected abstract void Routine();
		protected abstract void LateRoutine();
		protected abstract void FixedRoutine();
		public    override void OnEvent(Events_Event e) { }
		
		// Process System //
		
		protected void AddProcessToUpdate(Action process, bool notAllowedToOverlapped = false) {
			if (notAllowedToOverlapped) {
				if (processToUpdate[GameManager.instance.currentFixedUpdatePhase].Contains(process)) return;
			}
			processToUpdate[GameManager.instance.currentFixedUpdatePhase].Add(process);
		}

		protected void AddProcessToFixedUpdate(Action process, bool notAllowedToOverlapped = false) {
			if (notAllowedToOverlapped) {
				if (processToFixedUpdate[GameManager.instance.currentUpdatePhase].Contains(process)) return;
			}
			processToFixedUpdate[GameManager.instance.currentFixedUpdatePhase].Add(process);
		}
		
		private void ExecuteUpdateProcess() {
			if (stopped) return;
			
			foreach (List<Action> processes in processToUpdate) {
				foreach (Action process in processes) process.Invoke();
				processes.Clear();
			}
		}
		private void ExecuteFixedUpdateProcess() {
			if (stopped) return;
			
			foreach (List<Action> processes in processToFixedUpdate) {
				foreach (Action process in processes) process.Invoke();
				processes.Clear();
			}
		}
		
		// IObjectPoolable //
		
		public override void OnFirstGet() {
			updatePhaseRoutines = new Action[] {
				ExecuteUpdateProcess,
				EarlyRoutine,
				Routine
			};

			fixedUpdatePhaseRoutine = new Action[] {
				ExecuteFixedUpdateProcess,
				FixedRoutine
			};
			
			processToUpdate      = new List<Action>[fixedUpdatePhaseCount]; // sent from FixedUpdate
			processToFixedUpdate = new List<Action>[updatePhaseCount];      // sent from Update
			
			for (int i = 0; i < fixedUpdatePhaseCount; i++) processToUpdate[i]      = new List<Action>();
			for (int i = 0; i < updatePhaseCount     ; i++) processToFixedUpdate[i] = new List<Action>();
			
			stopped = false;
			
			rigidbody2D    = GetComponent<Rigidbody2D>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator       = GetComponent<Animator>();
		}

		public override void Release(float time) {
			Instances.Remove(this);
			
			base.Release(time);
		}

		public override void Initialize() { Instances.Add(this); }

		public override void Uninitialize() { }
	}
}