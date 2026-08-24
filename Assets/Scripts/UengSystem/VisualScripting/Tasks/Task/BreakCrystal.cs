using System;
using AncientMemorial.Interactions;
using UengSystem.ObjectPool;
using UengSystem.UAction;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class BreakCrystal : TaskComponent {
		private static DelayedAction action = new DelayedAction(5, () => {
			for (int i = 0; i < 20; i++) {
				UObjectPool.instance.Get("crystalDebris",
										 (Vector2)GameManager.instance.Crystal.crystalModel.transform.position +
										 new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
			}

			GameManager.instance.Crystal.crystalModel.SetActive(false);
		});
		
		public override void Execute(ITaskable self) {
			Crystal crystal = GameManager.instance.Crystal;
			crystal.animator.Play("break");
			crystal.UnInteractable();

			action.ExecuteDA();
		}
	}
}