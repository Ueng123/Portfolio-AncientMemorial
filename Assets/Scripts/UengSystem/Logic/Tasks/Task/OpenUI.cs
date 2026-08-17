using System;
using AncientMemorial;
using UengSystem.Logic.UValues;
using UengSystem.UDebug;
using UengSystem.UI;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class OpenUI : TaskComponent {
		public GameObject TargetUUI;
		public UCanvas canvas;
		
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
			
			GameObject ui  = UUIPool.instance.Open(TargetUUI.name, canvas);
			UUI        uui = ui.GetComponent<UUI>();

			if (ID       !=null &&ID.value       !="") uui.ID       = ID.value;
			if (Category !=null &&Category.value !="") uui.Category = Category.value;
		}
	}
}