using UnityEngine;

namespace AncientMemorial.Interactions {
	public class Crystal : Interaction {
		private static readonly int Enabled = Animator.StringToHash("Enabled");

		public GameObject crystalModel;

		public override void Interactable() {
			base.Interactable();
			animator.SetBool(Enabled, true);
		}

		public override void UnInteractable() {
			base.UnInteractable();
			animator.SetBool(Enabled, false);
		}

		protected override void OnInteractStart() { }
		protected override void OnCancel() { }
		protected override void OnInteract() { }
		protected override void OnTarget() { }
		protected override void OnUnTarget() { }
	}
}