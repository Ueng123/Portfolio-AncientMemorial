using UnityEngine;

namespace UengSystem.Objects {
	public class BasicUObject : UObject {

		public override void OnFirstGet() {
			stopped = false;
			
			rigidbody2D    = GetComponent<Rigidbody2D>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator       = GetComponent<Animator>();
		}

		public override void OnGet() { }

		public override void OnRelease() { }

		public override void Initialize() { Resume(); }

		public override void Uninitialize() { Resume(); }
	}
}