using System;
using UengSystem.UDebug;
using UengSystem.UI;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class OpenUI : TaskComponent {
		public GameObject TargetUUI;
		public UCanvas canvas;
		public bool PlayEffect = true;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) {
			if (!canvas || canvas == default) canvas = GameManager.instance.mainScreenCanvas;
			
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			DebugManager.Log($" >>> Given Data\n"                        +
							 (ID != null ? $" > ID = {ID.value}\n" : "") +
							 (Category != null ?$" > Category = {Category.value}\n":"")     +
							 $" > targetPrefab = {TargetUUI.name}\n" +
							 $" > canvas = {canvas.name}\n"          +
							 $"");
			
			UUI.Get(TargetUUI.name, canvas, PlayEffect, Ui => {
				if (!string.IsNullOrWhiteSpace(ID?.value)) Ui.ID = ID.value;
				if (!string.IsNullOrWhiteSpace(Category?.value)) Ui.Category = Category.value;
			});
		}
	}
}
