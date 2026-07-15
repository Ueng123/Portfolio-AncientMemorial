using System;
using AncientMemorial.Interactions;
using UengSystem.Logic.Tasks;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class BreakCrystal : TaskComponent {
		public override void Execute(ITaskable self) {
			Crystal crystal = GameManager.instance.Crystal;
			crystal.animator.Play("break");
			crystal.interactable = false;

			new DelayedAction(5, () => {
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				UObjectPool.instance.Get("crystalDebris", (Vector2)crystal.crystalModel.transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
				crystal.crystalModel.SetActive(false);
			}).Execute();
		}
	}
}