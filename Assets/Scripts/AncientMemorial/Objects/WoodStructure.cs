using System.Collections;
using AncientMemorial.Map;
using UengSystem;
using UengSystem.Objects;
using UengSystem.Objects.LifeCycle;
using UnityEngine;

namespace AncientMemorial.Objects {
	public class WoodStructure : UObject {
		private void MatchWithMapSize() {
			transform.position  = new Vector2(transform.position.x, .5f+MapManager.instance.GetMapSize().y/2);
			spriteRenderer.size = new Vector2(spriteRenderer.size.x,    MapManager.instance.GetMapSize().y);
		}

		public override void OnGet() {
			MatchWithMapSize();
		}

		public override void OnFirstGet() {
			SetDefaultStates(GettingState: new WoodGetting(this));
			base.OnFirstGet();
		}

		private sealed class WoodGetting : DefaultGetting {
			public WoodGetting(WoodStructure Target) : base(Target) { }
			protected override void OnEffectRoutine(float DeltaTime) {
				((WoodStructure)target).MatchWithMapSize();
				base.OnEffectRoutine(DeltaTime);
			}
		}

		protected override void EarlyRoutine() {
			MatchWithMapSize();
		}
	}
}
