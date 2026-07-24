using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using AncientMemorial.Objects;
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
using DelayedAction = UengSystem.Utility.DelayedAction;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public class CrystalPhase2 : Enemy {

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
				GameManager.SetTimeScale(0.5f, 0.5f);
				CameraBrain.instance.ShakeLerp(3f*Mathf.Max(Mathf.Log(damage+3),0.5f), 3);
				CameraBrain.instance.ZoomLerp(-1f, 7.5f);
				return;
			}
			
			PlaySFX("crystalHit1");
			
			if (groggyedEffect) {
				groggyedEffect = false;
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0, 0.5f);
				CameraBrain.instance.ShakeLerp(3f*Mathf.Max(Mathf.Log(damage+3),0.5f), 3);
				CameraBrain.instance.ZoomLerp(-1.25f);
				return;
			}
			
			if (groggy) {
				if (attacker != player) return;
				GameManager.SetTimeScale(0, 0.1f);
				CameraBrain.instance.ShakeLerp(2f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
				CameraBrain.instance.ZoomLerp(-0.5f);
				return;
			}
			
			if (attacker != player) return;
			GameManager.SetTimeScale(0, 0.05f);
			CameraBrain.instance.ShakeLerp(0.8F*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
			CameraBrain.instance.ZoomLerp(-0.5f);
		}

		private void ShootMissile(int shootN = 1, float spreadTiming = 0f) {
			for (int i = 0; i < shootN; i++) {
				// todoo
				//  Coroutine으로 완화
				//  개수만큼 Random 돌려서 타임 정렬한채로 놓고
				//  while 돌리면서 spreadTiming 지날때까지 소환 체킹 - 소환
				new DelayedAction(Random.Range(0f, spreadTiming), () => {
					Vector2 spawnPos = transform.position + new Vector3(Random.Range(-1f, 1f), 1.5f + Random.Range(-0.5f, 0.5f));
					
					GameObject missile = UObjectPool.instance.Get("CrystalMissile", spawnPos);
					CrystalMissile crystalMissile = missile.GetComponent<CrystalMissile>();
					crystalMissile.owner              = this;
					crystalMissile.damage             = entityStat.attackDamage / 2;
					crystalMissile.expectAttack       = true;
					crystalMissile.TimeBeforeLockOn   = Random.Range(0.8f, 1.5f);
					crystalMissile.LockOnDuration     = Random.Range(2f,   4f);
					crystalMissile.initialSpeed       = Random.Range(10f,  12f);
					crystalMissile.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-60f, 60f));
					crystalMissile.Category           = "crystalMissile";
				}, () => { }, this).ExecuteDA();
			}
		}

		private void ShootBigMissile(int shootN = 1, float spreadTiming = 0f) {
			for (int i = 0; i < shootN; i++) {
				new DelayedAction(Random.Range(0f, spreadTiming), () => {
					Vector2 spawnPos = new Vector2(
						transform.position.x + Random.Range(-(mapSize.x / 2 - 1), mapSize.x / 2 - 1),
						Random.Range(mapSize.y / 3f, mapSize.y * 5 / 6f));
					
					GameObject     missile        = UObjectPool.instance.Get("CrystalBigMissile", spawnPos);
					CrystalMissile crystalMissile = missile.GetComponent<CrystalMissile>();
					crystalMissile.owner              = this;
					crystalMissile.damage             = entityStat.attackDamage;
					crystalMissile.expectAttack       = true;
					crystalMissile.TimeBeforeLockOn   = 0;
					crystalMissile.LockOnDuration     = Random.Range(0.8f, 1.5f);
					crystalMissile.initialSpeed       = Random.Range(8f,   10f);
					crystalMissile.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
					crystalMissile.Category           = "crystalMissile";
				}, () => { }, this).ExecuteDA();
			}
		}

		private void ClearMissiles() {
			foreach (UObject obj in GetUObjects("crystalMissile")) {
				new DelayedAction(Random.Range(0f, 0.25f), () => {
					if (obj.isReleased) return;
					UObjectPool.instance.Release(obj.gameObject);
				}).ExecuteDA();
			}

			foreach (DelayedAction da in MissileAttackAreas) {
				da.Cancel();
			}

			MissileAttackAreas.Clear();
		}

		private List<DelayedAction> MissileAttackAreas = new List<DelayedAction>();
		
		// -1 : <- | 0 : v | 1 : ->
		private void ShootHugeMissile(float where) {
			GameObject obj = UObjectPool.instance.Get("CrystalHugeMissile", new Vector2(where*mapSize.x/3, mapSize.y-2));
			BasicProjectile hugeMissile = obj.GetComponent<BasicProjectile>();
			hugeMissile.Category = "crystalMissile";
			
			float      timeToFall = Mathf.Sqrt(20 * (obj.transform.position.y-0.5f) / Physics2D.gravity.y*-1);

			Vector2 hitboxPos  = new (mapSize.x*where/3f, (mapSize.y +1));
			Vector2 hitboxSize = new (mapSize.x/3f, (mapSize.y +1)*2);
			MissileAttackAreas.Add(AttackArea(Random.Range(90, 100), timeToFall, hitboxPos, hitboxSize));
		}
		
		private void ShootHugeMissile(float where, Vector2 hitboxPos, Vector2 hitboxSize) {
			GameObject obj = UObjectPool.instance.Get("CrystalHugeMissile", new Vector2(where*mapSize.x/4, mapSize.y-2));
			float      timeToFall = Mathf.Sqrt(20 * (obj.transform.position.y-0.5f) / Physics2D.gravity.y*-1);
			
			AttackArea(Random.Range(90, 100), timeToFall, hitboxPos, hitboxSize);
		}
		
		protected override void OnHit(Entity attacker, float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
			ShootMissile(Random.Range(0, 2));
		}

		protected override void OnHit(Projectile projectile, float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
			ShootMissile(Random.Range(0, 2));
		}

		protected override void OnHit(float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
			ShootMissile(Random.Range(0, 2));
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

		public int[] Shuffle(int n) {
			int[] array = new int[n];
			for (int i = 0; i < n; i++) { array[i] = i; }

			for (int i = n-1; i > 0; i--) {
				int index = Random.Range(0, i+1);
				(array[i], array[index]) = (array[index], array[i]);
			}

			return array;
		}
		
		private       int[] attackIndexes;
		public        int   attackPhase    = 0;
		private const int   attackPhaseNum = 4;
		public override void Attack() {
			if (!attackable) return;
			attackable    =   false;
			attackIndexes ??= Shuffle(attackPhaseNum);

			PlaySFX("crystalRoar");
			
			ClearMissiles();
			
			// random ONLY at first
			currentExclusiveAction = attackIndexes[attackPhase] switch {
				0 => Attacking,
				1 => AttackVing,
				2 => Slash,
				3 => SlashHorizontal,
				_ => throw new Exception("NO BRO THAT'S NOT WHAT I WANT :(")
			};
			
			attackPhase++;
			if (attackPhase != attackPhaseNum) return;
			attackPhase   = 0;
			attackIndexes = Shuffle(attackPhaseNum);
		}
		
		private ExclusiveAction AttackVing => new (AttackVEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
		
		private ExclusiveAction Slash => new (SlashEnumerator(), OnSlashCancel, OnSlashDone, 0, this);
		private ExclusiveAction SlashHorizontal => new (SlashHorizontalEnumerator(), OnSlashCancel, OnSlashDone, 0, this);
		
		protected override IEnumerator AttackEnumerator() {
			animator.SetBool(Spawning, true);
			groggy = true;
			
			List<int> orders = new() { -1, 0, 1 };
			for (int i = orders.Count - 1; i > 0; i--)
			{
				int rnd  = Random.Range(0, i + 1);
				(orders[i], orders[rnd]) = (orders[rnd], orders[i]);
			}
			
			ShootMissile (Random.Range(2, 5), 5f);
			ShootBigMissile(Random.Range(2, 5), 5f);

			yield return new WaitForSeconds(1f);
			
			ShootHugeMissile(orders[0]);
			
			yield return new WaitForSeconds(2f);
			
			ShootHugeMissile(orders[1]);
			
			yield return new WaitForSeconds(2f);
			
			ShootHugeMissile(orders[2]);

			yield return new WaitForSeconds(9f);
		}
		
		protected IEnumerator AttackVEnumerator() {
			animator.SetBool(Spawning, true);
			groggy = true;
			
			for (int i = Random.Range(0, 2); i < Random.Range(5, 9); i++) {
				ShootHugeMissile(
					Random.Range(-0.1f, 0.1f) + (i % 2 == 0 ? 1 : -1),
					new Vector2(mapSize.x/4f * (i % 2 == 0 ? 1 : -1), mapSize.y/2),
					new Vector2(mapSize.x/2f, mapSize.y)
					);
				yield return new WaitForSeconds(1.2f);
			}
			
			yield return new WaitForSeconds(7.8f);
		}


		// wallN
		// 0 : < | 1 : > | 2 : v | 3 : ^ //
		
		public void GetTwoRandomNumbers(out int first, out int second)
		{
			int[] numbers = { 0, 1, 2, 3 };
			
			int idx1 = Random.Range(0, 4);
			int temp = numbers[0];
			numbers[0]    = numbers[idx1];
			numbers[idx1] = temp;
			
			int idx2 = Random.Range(1, 4);
			temp          = numbers[1];
			numbers[1]    = numbers[idx2];
			numbers[idx2] = temp;

			first  = numbers[0];
			second = numbers[1];
		}
		
		public Vector2 GetWallPoint(int wallN) {
			return wallN switch {
				0 => new Vector2(-mapSize.x, Random.Range(1f,  mapSize.x /2f - 1)),
				1 => new Vector2(mapSize.x,  Random.Range(1f,  mapSize.x /2f - 1)),
				2 => new Vector2(Random.Range(-(mapSize.y -1), (mapSize.y -1)), 0),
				3 => new Vector2(Random.Range(-(mapSize.y -1), (mapSize.y -1)), mapSize.y),
				_ => Vector2.zero
			};
		}  
		
		protected IEnumerator SlashEnumerator() {
			animator.SetBool(Spawning, true);
			groggy = true;
			
			yield return new WaitForSeconds(1f);
			
			int   patternNum = Random.Range(4, 7);
			int[] slashNums  = new int[patternNum];
			for (int i = 0; i < patternNum; i++) { slashNums[i] = Random.Range(8, 12); }

			for (int j = 0; j < patternNum; j++) {
				int       slashNum        = slashNums[j];
				float     patternDuration = Random.Range(3f, 5f);
				Vector4[] slashData       = new Vector4[slashNum];

				for (int i = 0; i < slashNum; i++) {
					float duration = Random.Range(2, patternDuration);

					GetTwoRandomNumbers(out int s, out int e);
					Vector2 startPos = GetWallPoint(s);
					Vector2 endPos   = GetWallPoint(e);
					Vector2 dir      = endPos                                    - startPos;
					float   angle    = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;
					
					Vector2 center = (startPos + endPos) * 0.5f;
					float   length = dir.magnitude;
					
					slashData[i] = new Vector4(center.x, center.y, length, angle);
					new DelayedAction(patternDuration - duration, () => {
						AttackArea(2, duration, center, new Vector2(0.5f, length+5f), angle);
					}, () => { }, this).ExecuteDA();
				}

				new DelayedAction(patternDuration, () => {
					foreach (Vector4 data in slashData) {
						Vector2    slashPos = new(data.x, data.y);
						GameObject obj      = UObjectPool.instance.Get("SlashEffect", slashPos);
						obj.transform.rotation   = Quaternion.Euler(0, 0, data.w);
						obj.transform.localScale = new Vector3(1f, data.z + 0.5f, 1f);
					}

					GameManager.SetTimeScale(0, 0.1f);
					CameraBrain.instance.ShakeLerp(5, 3);
					CameraBrain.instance.ZoomLerp(-2f);
				}, () => { }, this).ExecuteDA();

				ShootMissile(Random.Range(2,  5), patternDuration);
				ShootBigMissile(Random.Range(2, 5), patternDuration);

				yield return new WaitForSeconds(patternDuration + 1f);
			}
			
			yield return new WaitForSeconds(1f);
		}
		
		
		protected IEnumerator SlashHorizontalEnumerator() {
			animator.SetBool(Spawning, true);
			groggy = true;
			
			yield return new WaitForSeconds(1f);

			int   patternNum      = Random.Range(4, 7);
			int[] slashNums       = new int[patternNum];
			for (int i = 0; i < patternNum; i++) { slashNums[i] = Random.Range(5,  10); }

			for (int j = 0; j < patternNum; j++) {
				int       slashNum        = slashNums[j];
				float     patternDuration = Random.Range(3f, 4f);
				Vector4[] slashData       = new Vector4[slashNum];

				for (int i = 0; i < slashNum; i++) {
					float duration = Random.Range(2, patternDuration);

					Vector2 startPos = GetWallPoint(2);
					Vector2 endPos   = new(startPos.x, mapSize.y);
					Vector2 dir   = endPos - startPos;
					float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;
					
					Vector2 center = (startPos + endPos) * 0.5f;
					float   length = dir.magnitude;
					
					slashData[i] = new Vector4(center.x, center.y, length, angle);
					new DelayedAction(patternDuration - duration, () => {
						AttackArea(2, duration, center, new Vector2(2f, length+5f), angle);
					}).ExecuteDA();
				}// da + slashNum

				new DelayedAction(patternDuration, () => {
					foreach (Vector4 data in slashData) {
						GameObject obj      = UObjectPool.instance.Get("SlashEffect", new Vector2(data.x, data.y+Random.Range(-2f, 2f)));
						obj.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
						obj.transform.localScale = new Vector3(3f, mapSize.y+Random.Range(-1f, 1f), 1f);
					}

					GameManager.SetTimeScale(0, 0.05f);
					CameraBrain.instance.ShakeLerp(5, 3);
					CameraBrain.instance.ZoomLerp(-2f);
				}).ExecuteDA(); // da + 1

				ShootMissile(Random.Range(2,  5), patternDuration);
				ShootBigMissile(Random.Range(2, 5), patternDuration);

				yield return new WaitForSeconds(patternDuration + 1f);
			}
			
			yield return new WaitForSeconds(1f);
		}

		protected override void OnAttackDone() {
			animator.SetBool(Spawning, false);
			ForceInvincibleTrue(false);
			groggy = false;
			
			ClearMissiles();
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			attackable             = false;
		}
		
		protected override void OnAttackCancel() {
			animator.SetBool(Spawning, false);
			ForceInvincibleTrue(false);
			groggy = false;
			
			ClearMissiles();
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			attackable             = false;
		}

		protected void OnSlashDone() {
			animator.SetBool(Spawning, false);
			ForceInvincibleTrue(false);
			groggy = false;
			
			ClearMissiles();
			
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			attackable             = false;
		}
		
		protected void OnSlashCancel() {
			animator.SetBool(Spawning, false);
			ForceInvincibleTrue(false);
			groggy = false;
			
			ClearMissiles();
			
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

			if (!missileStopwatch.Check(1f)) {
				ShootMissile(Random.Range(0,  2), spreadTiming:2f);
				ShootBigMissile(Random.Range(0, 2), spreadTiming:2f);

				missileStopwatch.Tick();
			}
		}
		
		public override void AttackRoutine() { }

		public override void AttackReadyRoutine() { }
		public override void StunRoutine()        { }

		protected override void EarlyRoutine() {
			
			mapSize = MapManager.instance.GetMapSize();
			
			base.EarlyRoutine();
		}

		public override void Initialize() {
			base.Initialize();
			mapSize = MapManager.instance.GetMapSize();
			
			foreach (GameObject obj in hideOnDeath) {
				obj.SetActive(true);
			}
			
			StartCoroutine(SpawnEffect());
		}

		private IEnumerator SpawnEffect() {
			const int n = 100;
			for (int i = 0; i < n; i++) {
				transform.Translate(Vector3.up       *(2f/n));
				yield return new WaitForSeconds(10f / n);
			}
			
			GameObject sEff      = UObjectPool.instance.Get("SlashEffect", transform.position);
			sEff.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
			sEff.transform.localScale = new Vector3(2f, 10f);
			
			GameObject sEff2      = UObjectPool.instance.Get("SlashEffect", transform.position);
			sEff2.transform.rotation   = Quaternion.Euler(0, 0, 90+Random.Range(-5f, 5f));
			sEff2.transform.localScale = new Vector3(2f, 10f);
			
			Time.timeScale = 0.2f;
			new DelayedAction(0.2f, ()=>Time.timeScale = 1f, () => { }, this).ExecuteDA(true);
			CameraBrain.instance.ShakeLerp(5, 10);
			CameraBrain.instance.ZoomLerp(-2f, 3);
			
			attackable = false;
		}

		protected override void Death() {
			foreach (GameObject obj in hideOnDeath) {
				obj.SetActive(false);
			}
			
			PlaySFX("crystalHit2");

			GameManager.UValueBoolVariables["crystalPhase2End"]   = new UPureBool {boolValue = true};

			ClearMissiles();
			
			UObjectPool.instance.Get("CrystalPhase3", transform.position);

			for (int i = 0; i < 10; i++) {
				UObjectPool.instance.Get("crystalDebris", (Vector2)transform.position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
			}
			
			base.Death();
		}
	}
}