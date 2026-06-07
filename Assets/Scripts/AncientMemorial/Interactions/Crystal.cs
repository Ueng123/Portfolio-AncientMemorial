using UnityEngine;

namespace AncientMemorial.Interactions {
	public class Crystal : Interaction {
		private static readonly int Enabled = Animator.StringToHash("Enabled");

		public GameObject crystalModel;
		
		protected override void OnInteractStart() { }
		protected override void OnCancel() { }
		protected override void OnInteract() { }
		protected override void OnTarget() {
			animator.SetBool(Enabled, true);
		}
		protected override void OnUnTarget() {
			animator.SetBool(Enabled, false);
		}
	}
}