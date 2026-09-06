using UengSystem.UI;
using System;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UI.UUIs;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues.UFloats;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase1 {
	public class CrystalSpawn : CrystalAttack {
		private static          int CRYSTAL_ENEMY_DEAD = "CrystalEnemyDead".GetHash();
		private static readonly int ATTACKING          = "attacking".GetHash();

		public override float attackTime => 2f;
		
		private StopWatch missileTimer     = new StopWatch();
		private float     missileSpawnTime = 0;

		private int   enemyCountToSpawn;
		private float[] entitySpawnDelay = new float[10];

		public void UpdateMissileSpawnTime() {
			missileSpawnTime = Random.Range(0.5f, 1f);
		}

		public override void OnEnter() {
			base.OnEnter();
			crystal.Invincible(true);
			crystal.animator.SetBool(ATTACKING, true);
			
			missileTimer.Tick();
			UpdateMissileSpawnTime();
			
			InfoUUI.instance.AddInfoMessage("크리스탈이 기억속 잔재를 불러옵니다.");
			UPureFloat.SetValue(CRYSTAL_ENEMY_DEAD, 0);
			
			enemyCountToSpawn    = Random.Range(6, 10);
			
			for (int i = 0; i < entitySpawnDelay.Length; i++) entitySpawnDelay[i] = Random.value;
			Array.Sort(entitySpawnDelay);
		}

		public void MissileRoutine() {
			if (missileTimer.CheckIn(missileSpawnTime)) return;
			
			Vector2 missilePos   = crystal.transform.position + new Vector3(0, 1.5f, 0);
			float   missileReady = Random.Range(0.3f, 0.5f);
			ShootMissile(missilePos, 0, 15f, missileReady, 3f);
			
			missileTimer.Tick();
			UpdateMissileSpawnTime();
		}

		public void SpawnCrystalSkeleton() {
			string  spawnEntityID = Random.Range(0, 2) == 0 ? "SkeletonWarriorC" : "SkeletonArcherC";
			Vector3 spawnPosOffset = new (Random.Range(2, 6) * (Random.Range(0, 2) == 0 ? 1 : -1), Random.Range(1, 1.5f));
			GameObject entityObject = UObject.Get(spawnEntityID, crystal.transform.position + spawnPosOffset, PlayEffect: true);
			entityObject.GetComponent<UObject>().Category = "CrystalEnemy";
		}
		
		public void SpawnRoutine() {
			if (isProgress(entitySpawnDelay[step]) && step < enemyCountToSpawn) {
				SpawnCrystalSkeleton();
				step++;
			}
		}
		
		public override void OnRoutine() {
			MissileRoutine();
			SpawnRoutine();

			if (UPureFloat.GetValue(CRYSTAL_ENEMY_DEAD) >= enemyCountToSpawn) crystal.Stun(8);
		}

		public override void OnExit() {
			base.OnExit();
			crystal.Invincible(false);
			crystal.animator.SetBool(ATTACKING, false);
		}
	}
}
