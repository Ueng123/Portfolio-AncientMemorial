using System;
using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using AncientMemorial.Projectiles;
using UengSystem.Audio;
using UengSystem.Events;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UBools;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public class CrystalPhase1 : Enemy {

		public GameObject[] hideOnDeath;
		
		private Vector2 mapSize;
		
		private static readonly int  Spawning = Animator.StringToHash("spawning");
		protected override      void OnGrounded() { }

		private bool groggy;
		private bool groggyedEffect;
		public override    void        HitEffect(Entity    attacker,   float    damage, Vector2? pushDir = null) {
			UUI damageUI = UUIObjectPool.instance.Open("DamageUI", GameManager.instance.mainWorldCanvas).GetComponent<UUI>();
			damageUI.GetComponent<RectTransform>().anchoredPosition =
				(transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0))*80;
			UTextAction   textAction = damageUI.GetAction<UTextAction>("DamageDisplay");
			UTextAction   textSAction = damageUI.GetAction<UTextAction>("DamageDisplayShadow");
			new DelayedAction(0.5f, () => UUIObjectPool.instance.Close(damageUI.gameObject)).ExecuteDA();
			
			textAction.text = new UPureString {Text = (groggy)?$"{Mathf.Floor(damage*100)/100f}<size=20><i> !!</i></size>":$"{Mathf.Floor(damage*100)/100f}"};
			textSAction.text = textAction.text;
			textAction.Initialize(damageUI);
			textSAction.Initialize(damageUI);
			
			if (entityStat.hp <= 0) {
				GameManager.SetTimeScale(0, 0.5f);
				CameraBrain.instance.ShakeLerp(3f*Mathf.Max(Mathf.Log(damage+3),0.5f), 3);
				CameraBrain.instance.ZoomLerp(-1f, 7.5f);
				return;
			}

			PlaySFX("crystalHit1");
			
			if (groggyedEffect) {
				groggyedEffect = false;
				GameManager.SetTimeScale(0, 0.5f);
				CameraBrain.instance.ShakeLerp(3f*Mathf.Max(Mathf.Log(damage+3),0.5f), 3);
				CameraBrain.instance.ZoomLerp(-0.75f);
				return;
			}
			
			if (groggy) {
				GameManager.SetTimeScale(0, 0.1f);
				CameraBrain.instance.ShakeLerp(2f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
				CameraBrain.instance.ZoomLerp(-0.5f, 20f);
				return;
			}
			
			GameManager.SetTimeScale(0, 0.05f);
			CameraBrain.instance.ShakeLerp(0.8F*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
			CameraBrain.instance.ZoomLerp(-0.5f, 20f);
		}

		private void ShootMissile(int shootN = 1) {
			for (int i = 0; i < shootN; i++) {
				Vector2 spawnPos = transform.position + new Vector3(Random.Range(-1f, 1f), 1.5f + Random.Range(-0.5f, 0.5f));
				
				GameObject     missile            = UObjectPool.instance.Get("CrystalMissile", spawnPos);
				CrystalMissile crystalMissile     = missile.GetComponent<CrystalMissile>();
				crystalMissile.owner              = this;
				crystalMissile.damage             = entityStat.attackDamage/2;
				crystalMissile.Category           = "crystalMissile";
				crystalMissile.expectAttack       = true;
				crystalMissile.TimeBeforeLockOn   = Random.Range(0.8f, 1.5f);
				crystalMissile.LockOnDuration     = Random.Range(2f,   4f);
				crystalMissile.initialSpeed       = Random.Range(10f,  12f);
				crystalMissile.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-40f, 40f));
			}
		}

		private void ShootMissile2(int shootN = 1) {
			for (int i = 0; i < shootN; i++) {
				Vector2 spawnPos = new Vector2(
					transform.position.x + Random.Range(-(mapSize.x - 1), mapSize.x - 1),
					transform.position.y + Random.Range(4,                mapSize.y - 4));
				
				GameObject     missile        = UObjectPool.instance.Get("CrystalMissile", spawnPos);
				CrystalMissile crystalMissile = missile.GetComponent<CrystalMissile>();
				crystalMissile.owner              = this;
				crystalMissile.damage             = entityStat.attackDamage/2;
				crystalMissile.Category           = "crystalMissile";
				crystalMissile.expectAttack       = true;
				crystalMissile.TimeBeforeLockOn   = 0;
				crystalMissile.LockOnDuration     = Random.Range(1f, 1.5f);
				crystalMissile.initialSpeed       = Random.Range(8f, 15f);
				crystalMissile.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
			}
		}
		
		private void ClearMissiles() {
			foreach (UObject obj in GetUObjects("crystalMissile")) {
				new DelayedAction(Random.Range(0f, 0.5f), () => {
					if (obj.isReleased) return;
					UObjectPool.instance.Release(obj.gameObject);
				}).ExecuteDA();
			}
		}
		
		protected override void OnHit(Entity attacker, float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
			if (Random.Range(0, 4)==0) ShootMissile(Random.Range(0, 3));
		}

		protected override void OnHit(Projectile projectile, float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
			if (Random.Range(0, 4)==0) ShootMissile(Random.Range(0, 3));
		}

		protected override void OnHit(float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
			if (Random.Range(0, 4)==0) ShootMissile(Random.Range(0, 3));
		}

		protected override float       GetRealDamage(float rawDamage) {
			if (groggyedEffect) {
				groggyedEffect = false;
				new DelayedAction(5f, ()=>groggy=false).ExecuteDA();
				
				currentExclusiveAction = Stun(5);
				state                  = EnemyState.Stun;
				new DelayedAction(5, () => state = EnemyState.Alert).ExecuteDA();
				
				return entityData.hp / 20f;
			}
			if (groggy) return rawDamage*2;
			return rawDamage;
		}

		public override void OnStunStart() { }
		public override void OnStunEnd() {
			currentExclusiveAction = FindingAggro;
		}
		
		protected override IEnumerator AttackEnumerator() {
			animator.SetBool(Spawning, true);
			PlaySFX("crystalRoar");
			
			ForceInvincibleTrue(true);
			
			InfoUUI.instance.AddInfoMessage("크리스탈이 기억속 잔재를 불러옵니다.");

			GameManager.UValueFloatVariables["EnemyDead"] = new UPureNumber { number = 0 };

			int entityToSpawn = Random.Range(6, 10);
			Debug.Log($"[CrystalAttack] entityToSpawn = {entityToSpawn+1}");
			
			for (int i = 0; i < entityToSpawn; i++) {
				Debug.Log($"[CrystalAttack] spawning entity {i}");
				new DelayedAction(Random.Range(1, 6), () => {
					string  spawnEntityID = Random.Range(0, 2) == 0 ? "SkeletonWarriorC" : "SkeletonArcherC";
					Vector3 spawnPosOffset = new (Random.Range(2, 6) * (Random.Range(0, 2) == 0 ? 1 : -1), Random.Range(1, 1.5f));
					UObjectPool.instance.Get(spawnEntityID, transform.position + spawnPosOffset, 2.5f);
				}).ExecuteDA();
			}
			
			yield return new WaitUntil(()=>GameManager.UValueFloatVariables["EnemyDead"].value >= entityToSpawn);

			groggy         = true;
			groggyedEffect = true;
			SendAttackEvent(this, 1, true);
		}

		// protected IEnumerator Attack2Enumerator() {
		// 	animator.SetBool(Spawning, true);
		// 	ForceInvincibleTrue(true);
		//
		// 	InfoUUI.instance.AddInfoMessage("크리스탈이 기억속 강력한 잔재를 꺼내옵니다.");
		// 	
		// 	GameManager.UValueFloatVariables["skeletonTankDead"] = new UPureNumber { number = 0 };
		//
		// 	int entityToSpawn = 1;
		// 	Debug.Log($"[CrystalAttack] entityToSpawn = {entityToSpawn+1}");
		// 	
		// 	for (int i = 0; i < entityToSpawn; i++) {
		// 		Debug.Log($"[CrystalAttack] spawning entity {i}");
		// 		new DelayedAction(Random.Range(1, 6), () => {
		// 			UObjectPool.instance.Get(
		// 				"SkeletonTankC",
		// 				(Vector2)transform.position
		// 				+ new Vector2(Random.Range(2, 6) * (Random.Range(0, 2) == 0 ? 1 : -1),
		// 							  Random.Range(1, 1.5f)),
		// 				2.5f);
		// 		}).Execute();
		// 	}
		// 	
		// 	yield return new WaitUntil(()=>GameManager.UValueFloatVariables["skeletonTankDead"].value >= entityToSpawn);
		//
		// 	groggy         = true;
		// 	groggyedEffect = true;
		// 	AddProcessToUpdate(()=>SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Hit, 10, new EntityHitData(
		// 										 this,
		// 										 null,
		// 										 this,
		// 										 0,
		// 										 Vector2.zero
		// 									 )));
		// }
		
		protected IEnumerator Attack3Enumerator() {
			animator.SetBool(Spawning, true);
			groggy         = true;
			
			for (int i = 0; i < 10; i++) {
				ShootMissile(Random.Range(0, 2));
				ShootMissile2(Random.Range(0, 2));
				yield return new WaitForSeconds(2f);
			}

			yield return new WaitForSeconds(3f);
			
			for (int i = 0; i < 10; i++) {
				ShootMissile();
				ShootMissile2(Random.Range(0, 2));
				yield return new WaitForSeconds(1.5f);
			}
			
			yield return new WaitForSeconds(3f);
		}

		protected override void OnAttackDone() {
			animator.SetBool(Spawning, false);
			ForceInvincibleTrue(false);
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			attackable             = false;
		}

		// 지금까진 이론상 그런거 없음
		protected override void OnAttackCancel() {
			animator.SetBool(Spawning, false);
			ForceInvincibleTrue(false);
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			attackable             = false;
		}
		
		public override void WanderRoutine() {
			if (!aggroEntity) return;
			state           = EnemyState.Alert;
		}

		StopWatch missileStopwatch = new StopWatch();
		public override void AlertRoutine() {
			if (!aggroEntity) {
				state = EnemyState.Wander;
				return;
			}
			
			if (attackable) {
				state            = EnemyState.Attack;
				return;
			}

			if (!missileStopwatch.Check(4f)) {
				ShootMissile(Random.Range(0, 3));

				missileStopwatch.Tick();
			}
		}

		public override void AttackRoutine() {
			ForceInvincibleTrue(true);

			if (missileStopwatch.Check(2f)) return;
			ShootMissile(Random.Range(0, 3));
				
			missileStopwatch.Tick();
		}

		public override void AttackReadyRoutine() { }
		public override void StunRoutine() { }

		protected override void EarlyRoutine() {
			base.EarlyRoutine();
			
			mapSize = MapManager.instance.GetMapSize();
		}

		public override void Initialize() {
			foreach (GameObject obj in hideOnDeath) {
				obj.SetActive(true);
			}
			
			GameObject sEff      = UObjectPool.instance.Get("SlashEffect", transform.position);
			sEff.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
			sEff.transform.localScale = new Vector3(2f, 10f);
			
			GameObject sEff2      = UObjectPool.instance.Get("SlashEffect", transform.position);
			sEff2.transform.rotation   = Quaternion.Euler(0, 0, 90+Random.Range(-5f, 5f));
			sEff2.transform.localScale = new Vector3(2f, 10f);
			
			CameraBrain.instance.ShakeLerp(5, 10);
			CameraBrain.instance.ZoomLerp(-2f);
			
			base.Initialize();
		}

		protected override void Death() {
			foreach (GameObject obj in hideOnDeath) {
				obj.SetActive(false);
			}
			
			PlaySFX("crystalHit2");

			GameManager.UValueBoolVariables["crystalPhase1End"]   = new UPureBool {boolValue = true};
			
			ClearMissiles();
			
			UObjectPool.instance.Get("CrystalPhase2", transform.position);
			ShootMissile(5);
			ShootMissile2(3);

			for (int i = 0; i < 10; i++) {
				UObjectPool.instance.Get("crystalDebris", (Vector2)transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
			}
			
			base.Death();
		}
	}
}