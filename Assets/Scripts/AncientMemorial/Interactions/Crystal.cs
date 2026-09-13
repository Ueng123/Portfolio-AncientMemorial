using UnityEngine;

namespace AncientMemorial.Interactions {
	public class Crystal : Interaction {

		// 정적 프로퍼티
		private static readonly int Enabled = Animator.StringToHash("Enabled");

		// 인스턴스 프로퍼티
		public GameObject crystalModel;

		// 오버라이드 메서드
		public override void Interactable() {
			base.Interactable();
			animator?.SetBool(Enabled, true);
		}

		public override void UnInteractable() {
			base.UnInteractable();
			animator?.SetBool(Enabled, false);
		}

		protected override void OnInteractStart() { }
		protected override void OnCancel() { }
		protected override void OnInteract() { }
		protected override void OnTarget() { }
		protected override void OnUnTarget() { }
	}
}