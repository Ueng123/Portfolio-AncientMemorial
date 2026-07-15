using System;
using System.Collections.Generic;
using System.Linq;
using AncientMemorial;
using UengSystem.Events;
using UengSystem.Managers;
using UengSystem.Utility;
using UnityEngine;
using Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;

namespace UengSystem.Objects {
	public abstract class AdvancedUObject : UObject {

		public static readonly BufferedList<AdvancedUObject> AdvancedInstances = new ();
		
		private List<Action>[] processToUpdate;
		private List<Action>[] processToFixedUpdate;
		
		private        Action[] updatePhaseRoutines;
		private        Action[] fixedUpdatePhaseRoutines;
		
		private const int updatePhaseCount      = 3;
		private const int fixedUpdatePhaseCount = 2;
		
		public static void UpdateRoutine() {
			for (int i = 0; i < updatePhaseCount; i++) {
				GameManager.instance.currentUpdatePhase = i;
				
				AdvancedInstances.Apply();
				foreach (AdvancedUObject ins in AdvancedInstances) {
					if (ins.stopped) return;
					
					ins.updatePhaseRoutines[i].Invoke();
				}
			}
		}
		
		public static void LateUpdateRoutine() {
			foreach (AdvancedUObject ins in AdvancedInstances) {
				if (ins.stopped) return;
				
				ins.LateRoutine();
			}
		}
		
		public static void FixedUpdateRoutine() {
			for (int i = 0; i < fixedUpdatePhaseCount; i++) {
				GameManager.instance.currentFixedUpdatePhase = i;
				
				AdvancedInstances.Apply();
				foreach (AdvancedUObject ins in AdvancedInstances) {
					if (ins.stopped) return;
					
					ins.fixedUpdatePhaseRoutines[i].Invoke();
				}
			}
		}

		public static void EventRoutine() {
			for (int i = 0; i < EventManager.instance.events.Count; i++) {
				List<Event> events = EventManager.instance.GetEvents(i);
				//Debug.Log($"i = {i}; events = [ {events.Aggregate("", (c, e) => c+e.type)} ]");
				if (events.Count == 0) continue;
				
				foreach (Event e in events) {
					foreach (AdvancedUObject ins in AdvancedInstances) {
						if (ins.stopped) return;
						
						ins.OnEvent(e);
					}
				}
			}
		}
		
		protected abstract void EarlyRoutine();
		protected abstract void Routine();
		protected abstract void LateRoutine();
		protected abstract void FixedRoutine();
		public    override void OnEvent(Event e) { }
		
		// Process System //
		
		public void AddProcessToUpdate(Action process, bool notAllowedToOverlapped = false) {
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

			fixedUpdatePhaseRoutines = new Action[] {
				ExecuteFixedUpdateProcess,
				FixedRoutine
			};
			
			processToUpdate      = new List<Action>[fixedUpdatePhaseCount]; // sent from FixedUpdate
			processToFixedUpdate = new List<Action>[updatePhaseCount];      // sent from Update
			
			for (int i = 0; i < fixedUpdatePhaseCount; i++) processToUpdate[i]      = new List<Action>();
			for (int i = 0; i < updatePhaseCount     ; i++) processToFixedUpdate[i] = new List<Action>();
			
			base.OnFirstGet();
		}

		public override void Release(float time) {
			AdvancedInstances.Remove(this);

			foreach (List<Action> pU in processToUpdate) {
				pU.Clear();
			}
			
			foreach (List<Action> pFU in processToFixedUpdate) {
				pFU.Clear();
			}
			
			//Debug.Log($"[UObject] Instance {gameObject.name} Removed");
			
			base.Release(time);
		}

		public override void Initialize() {
			base.Initialize();
			AdvancedInstances.Add(this);
			//Debug.Log($"[UObject] Instance {gameObject.name} Added");
		}
	}
}