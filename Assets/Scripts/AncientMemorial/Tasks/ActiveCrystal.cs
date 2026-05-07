using System;
using AncientMemorial.Interactions;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.Managers;
using UnityEngine;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class ActiveCrystal : TaskComponent {
		public override void Execute(ITaskable self) {
			Debug.Log($"[TaskLog : {GetType()} - {Time.time}s - execute : {self}]");
			
			Crystal crystal = GameManager.instance.Crystal;

			crystal.interactable = true;
		}
	}
}