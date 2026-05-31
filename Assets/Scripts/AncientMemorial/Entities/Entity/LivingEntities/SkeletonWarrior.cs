using System.Collections;

namespace AncientMemorial.Entities {
	public class SkeletonWarrior : Enemy {
		protected override float aggroThreshold => 2f;
		
		protected override IEnumerator AttackEnumerator() {
			throw new System.NotImplementedException();
		}

		protected override void OnAttackDone() {
			throw new System.NotImplementedException();
		}

		protected override void OnAttackCancel() {
			throw new System.NotImplementedException();
		}

		protected override void Routine() {
			throw new System.NotImplementedException();
		}
	}
}