using System.Collections.Generic;
using AncientMemorial.AMObjects;
using AncientMemorial.Entities;
using AncientMemorial.Events;
using AncientMemorial.Inputs;
using AncientMemorial.Interactions;
using UnityEngine;

namespace AncientMemorial {
	public class GameManager : Manager<GameManager> {

		public Crystal Crystal;

		public Color spawnColor;
		
		public int currentUpdatePhase;
		public int currentFixedUpdatePhase;

		public static void ApplyStaticBufferedLists() {
			// NOTHING HERE YET :)
		}

		public static void ClearFramePerLists() {
			EventManager.instance.events.Clear();
			Interaction.InteractableInteractions.Clear();
		}

		private void Start() { foreach (IManager manager in IManager.instances) manager.Initialize(); }
		
		private void Update() {
			ApplyStaticBufferedLists();
			
			foreach (IManager manager in IManager.instances) { manager.ManagerUpdate(); }
			
			InputManager.instance.UpdateInputs();
			AdvancedAMObject.UpdateRoutine();
			AdvancedAMObject.EventRoutine();
		}

		private void LateUpdate() {
			AdvancedAMObject.LateUpdateRoutine();
			
			ClearFramePerLists();
		}
		
		private void FixedUpdate() {
			foreach (IManager manager in IManager.instances) { manager.ManagerFixedUpdate(); }
			
			AdvancedAMObject.FixedUpdateRoutine();
		}
		
		public override void ManagerUpdate()      {  }
		public override void ManagerFixedUpdate() {  }
	}
}