using System;
using UengSystem.Logic.UValues;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.UI;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class OpenUI : TaskComponent {
		public GameObject TargetUUI;
		public UCanvas    canvas;
		
		[SerializeReference][SubclassSelector] public UValue<string> ID;
		[SerializeReference][SubclassSelector] public UValue<string> Category;
		
		public override void Execute(ITaskable self) {
			if (!canvas || canvas == default) canvas = GameManager.instance.mainCanvas;
			
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			Debug.Log($" >>> Given Data\n"                    +
					  $" > ID = {ID.value}\n"                 +
					  $" > Category = {Category.value}\n"     +
					  $" > targetPrefab = {TargetUUI.name}\n" +
					  $" > canvas = {canvas.name}\n"          +
					  $"");
			
			GameObject ui  = UUIObjectPool.instance.Open(TargetUUI.name, canvas);
			UUI        uui = ui.GetComponent<UUI>();

			if (ID.value       != "") uui.ID        = ID.value;
			if (Category.value != "") uui.UCategory = Category.value;
		}
	}
}