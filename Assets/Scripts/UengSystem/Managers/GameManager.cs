using System.Collections.Generic;
using AncientMemorial.Interactions;
using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.Logic.UValues;
using UengSystem.Objects;
using UengSystem.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.Managers {
	public class GameManager : Manager<GameManager> {
		public                                  Crystal Crystal;
		[FormerlySerializedAs("canvas")] public UCanvas mainCanvas;
		
		public Color spawnColor;
		
		public int currentUpdatePhase;
		public int currentFixedUpdatePhase;
		
		public static Dictionary<string, UValue<float  >> UValueFloatVariables   = new ();
		public static Dictionary<string, UValue<string >> UValueStringVariables  = new ();
		public static Dictionary<string, UValue<bool   >> UValueBoolVariables    = new ();
		public static Dictionary<string, UValue<Color  >> UValueColorVariables   = new ();
		public static Dictionary<string, UValue<UObject>> UValueUObjectVariables = new ();
		public static Dictionary<string, UValue<UUI    >> UValueUUIVariables     = new ();
		
		public static void ApplyStaticBufferedLists() {
			// NOTHING HERE YET :)
		}
		
		public static void ClearFramePerLists() {
			EventManager.instance.RemoveAllEvents();
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