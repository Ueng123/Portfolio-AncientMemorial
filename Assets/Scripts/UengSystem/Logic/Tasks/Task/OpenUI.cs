using System;
using AncientMemorial;
using UengSystem.Logic.UValues;
using UengSystem.Managers;
using UengSystem.Objects;
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
			
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			Debug.Log($" >>> Given Data\n"                        +
					  (ID != null ? $" > ID = {ID.value}\n" : "") +
					  (Category != null ?$" > Category = {Category.value}\n":"")     +
								   $" > targetPrefab = {TargetUUI.name}\n" +
								   $" > canvas = {canvas.name}\n"          +
								   $"");
			
			GameObject ui  = UUIObjectPool.instance.Open(TargetUUI.name, canvas);
			UUI        uui = ui.GetComponent<UUI>();

			if (ID       !=null &&ID.value       !="") uui.ID       = ID.getValue;
			if (Category !=null &&Category.value !="") uui.Category = Category.getValue;
		}
	}
}