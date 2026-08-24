using System;
using AncientMemorial.Interactions;
using UengSystem.UDebug;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class ActiveCrystal : TaskComponent {
		public override void Execute(ITaskable self) {
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			
			Crystal crystal = GameManager.instance.Crystal;

			crystal.Interactable();
		}
	}
}