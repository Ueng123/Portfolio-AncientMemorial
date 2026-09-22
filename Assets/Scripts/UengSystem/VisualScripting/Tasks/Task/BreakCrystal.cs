using UengSystem.Utility;
using System;
using AncientMemorial.Interactions;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UActions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class BreakCrystal : TaskComponent {

		// 정적 프로퍼티
		private static readonly int CRYSTAL_DEBRIS = "crystalDebris".GetHash();

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			Crystal Target = GameManager.instance.Crystal;
			long Life = Target.lifeNumber;
			Vector2 Position = Target.crystalModel.transform.position;
			Target.animator.Play("break");
			Target.UnInteractable();
			new DelayedAction(5, () => {
				if (!Target || !Target.Matches(Life) || !Target.isActive) return;
				for (int Index = 0; Index < 20; Index++) {
					UObject.Get(CRYSTAL_DEBRIS, Position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)), PlayEffect: false);
				}
				Target.crystalModel.SetActive(false);
			}, executor: Target).ExecuteDA();
		}
	}
}
