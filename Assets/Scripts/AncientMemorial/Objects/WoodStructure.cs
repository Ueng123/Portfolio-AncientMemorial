﻿using System.Collections;
using AncientMemorial.Map;
using UengSystem;
using UengSystem.Objects;
using UengSystem.Objects.LifeCycle;
using UnityEngine;

namespace AncientMemorial.Objects {
	public class WoodStructure : UObject {

		// 인스턴스 메서드
		private void MatchWithMapSize() {
			transform.position  = new Vector2(transform.position.x, .5f+MapManager.instance.GetMapSize().y/2);
			spriteRenderer.size = new Vector2(spriteRenderer.size.x,    MapManager.instance.GetMapSize().y);
		}

		// 오버라이드 메서드
		public override void OnGet() {
			MatchWithMapSize();
		}

		public override void OnFirstGet() {
			SetDefaultStates(GettingState: new WoodGetting(this));
			base.OnFirstGet();
		}

		protected override void EarlyRoutine() {
			MatchWithMapSize();
		}

		// 중첩 타입
		private sealed class WoodGetting : DefaultGetting {

			// 인스턴스 메서드
			public WoodGetting(WoodStructure Target) : base(Target) { }

			// 오버라이드 메서드
			protected override void OnEffectRoutine(float DeltaTime) {
				((WoodStructure)target).MatchWithMapSize();
				base.OnEffectRoutine(DeltaTime);
			}
		}
	}
}
