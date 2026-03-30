using System;
using AncientMemorial.AMObjects;
using UnityEngine;

namespace AncientMemorial.Interactions {
	public class Crystal : Interaction {

		public GameObject crystalModel;
		
		public override void OnInteractStart() {
		}

		public override void OnCancel() {
		}

		public override void Interact() {
			
		}

		public override void OnTarget() {
			base.OnTarget();
		}

		public override void OnUnTarget() {
			base.OnUnTarget();
		}
	}
}