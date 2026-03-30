using System.Collections.Generic;
using UnityEngine;
using Event = AncientMemorial.Events.Event;

namespace AncientMemorial.AMObjects {
	public class BasicAMObject : AMObject {

		public override void OnFirstGet() {
			stopped = false;
			
			rigidbody2D    = GetComponent<Rigidbody2D>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			animator       = GetComponent<Animator>();
		}

		public override void Initialize() {
			Resume();
		}

		public override void Uninitialize() {
			Resume();
		}
	}
}