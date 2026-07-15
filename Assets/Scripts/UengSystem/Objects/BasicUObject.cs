using UnityEngine;

namespace UengSystem.Objects {
	public class BasicUObject : UObject {

		public override void OnFirstGet() {
			rigidbody2D    = GetComponent<Rigidbody2D>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator       = GetComponent<Animator>();
		}
	}
}