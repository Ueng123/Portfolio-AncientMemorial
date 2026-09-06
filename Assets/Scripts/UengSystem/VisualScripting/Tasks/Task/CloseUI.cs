using System;
using UengSystem.UDebug;
using UengSystem.UI;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class CloseUI : TaskComponent {
		public bool PlayEffect = true;

		public                                        bool        closeThis;
		[SerializeReference][SubclassSelector] public UValue<UUI> TargetUUI;
		
		public override void Execute(ITaskable self) {
			UUI target = closeThis? (UUI)self : TargetUUI.value;
			if (!target) {
				DebugManager.LogWarning($"[TaskLog : {GetType()}] No Target");
				return;
			}
			
			DebugManager.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			DebugManager.Log($" >>> Given Data\n"                      +
							 $" > TargetUUI : {target.name}\n"         +
							 $" > --- TargetUUI DATA ---\n"            +
							 $" > --- ID = {target.ID}\n"              +
							 $" > --- Category = {target.Category}\n"  +
							 $" > --- canvas = {target.canvas.name}\n" +
							 $"");

			if (target.isReleased) {
				DebugManager.LogWarning($"[TaskLog : {GetType()}] Object given is released already");
				return;
			}
			
			target.Release(PlayEffect);
		}
	}
}
