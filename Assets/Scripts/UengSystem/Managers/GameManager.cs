using AncientMemorial.Interactions;
using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Managers {
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
			AdvancedUObject.UpdateRoutine();
			AdvancedUObject.EventRoutine();
		}

		private void LateUpdate() {
			AdvancedUObject.LateUpdateRoutine();
			
			ClearFramePerLists();
		}
		
		private void FixedUpdate() {
			foreach (IManager manager in IManager.instances) { manager.ManagerFixedUpdate(); }
			
			AdvancedUObject.FixedUpdateRoutine();
		}
		
		public override void ManagerUpdate()      {  }
		public override void ManagerFixedUpdate() {  }
	}
}