using System;
using System.Collections.Generic;
using AncientMemorial.Objects;
using AncientMemorial.States.EnemyStates;
using AncientMemorial.States.EnemyStates.Crystal.Phase2;
using UengSystem;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UI;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Entities.Enemies {
	public class CrystalPhase2 : CrystalBoss {
		
		private          int          attackPhase = 0;
		private readonly EnemyState[] attackBag   = new EnemyState[4];
		
		public override void OnFirstGet() {
			base.OnFirstGet();

			InitializeCrystalStateMachine(new CrystalPhase2Idle());
			attackBag[0] = new CrystalHugeMissile1().Init(stateMachine);
			attackBag[1] = new CrystalHugeMissile2().Init(stateMachine);
			attackBag[2] = new CrystalSlash1().Init(stateMachine);
			attackBag[3] = new CrystalSlash2().Init(stateMachine);
			attackBag.Shuffle();
		}

		public override void InitializeState() {
			entityState = new CrystalPhase2Enter().Init(stateMachine);
		}
		
		public override void Initialize() {
			base.Initialize();
			attackPhase = 0;
		}
		
		protected override Entity GetTargetEntity() => player;

		public override void Attack() {
			Debug.Log($"ATTACK PHASE = {attackPhase}");
			
			entityState = attackBag[attackPhase];
			
			attackPhase++;
			
			if (attackPhase>=attackBag.Length) {
				attackBag.Shuffle();
				attackPhase %= attackBag.Length;
			}
		}
		
		protected override void Death() {
			CrystalBoss crystal = UObject.Get("CrystalPhase3", transform.position, PlayEffect: false).GetComponent<CrystalBoss>();

			if (!string.IsNullOrEmpty(ID)) {
				string id = ID;
				ID         = null;
				crystal.ID = id;
			}
			
			if (UUI.TryGetUUI("CrystalBossBar", out UUI bossBar)) {
				bossBar.Release(PlayEffect: true);
				UUI newBossBar = UUI.Get("CrystalP3UI", GameManager.instance.mainScreenCanvas).GetComponent<UUI>();
				newBossBar.ID = "CrystalBossBar";
			}
			
			base.Death();
		}
	}
}
