using System;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UObjects;
using UengSystem.UI;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class CloseUI : TaskComponent {

		public                                        bool        closeThis;
		[SerializeReference][SubclassSelector] public UValue<UUI> TargetUUI;
		
		public override void Execute(ITaskable self) {
			UUI target = closeThis? (UUI)self : TargetUUI.value;
			
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			Debug.Log($" >>> Given Data\n"                      +
					  $" > TargetUUI : {target.name}\n"         +
					  $" > --- TargetUUI DATA ---\n"            +
					  $" > --- ID = {target.ID}\n"              +
					  $" > --- Category = {target.Category}\n"  +
					  $" > --- canvas = {target.canvas.name}\n" +
					  $"");

			if (target.isReleased) {
				Debug.LogWarning($"[TaskLog : {GetType()}] Object given is released already");
				return;
			}
			UUIObjectPool.instance.Close(target.gameObject);
		}
	}
}