using System;
using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
using UengSystem.Events;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using EventType = UengSystem.Events.EventType;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public class CrystalPhase3 : Enemy {

		public GameObject[] hideOnDeath;
		
		protected override      void OnGrounded() { }

		public Animator   rotatingAnimator;

		private Vector2 mapSize;
		
		private readonly string[] damageDisplayTexts = new[] { "9", "9", "9", "9", "$", "#", "@", "!" };
		private          string   getText => damageDisplayTexts[Random.Range(0, damageDisplayTexts.Length)];
		public override    void        HitEffect(Entity    attacker,   float    damage, Vector2? pushDir = null) {
			currentExclusiveAction = Stun(1);
			PlaySFX("crystalHit2");
			
			state                  = EnemyState.Stun;
			new DelayedAction(1, () => state = EnemyState.Alert).ExecuteDA();
			
			UUI damageUI = UUIObjectPool.instance.Open("DamageUI", GameManager.instance.mainWorldCanvas).GetComponent<UUI>();
			damageUI.GetComponent<RectTransform>().anchoredPosition =
				(transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0))*80;
			UTextAction   textAction = damageUI.GetAction<UTextAction>("DamageDisplay");
			UTextAction   textSAction = damageUI.GetAction<UTextAction>("DamageDisplayShadow");
			new DelayedAction(0.5f, () => UUIObjectPool.instance.Close(damageUI.gameObject)).ExecuteDA();
			
			textAction.text  = new UPureString
				{Text = $"{getText}{getText}{getText}{getText}{getText}{getText}{getText}{getText}{getText}{getText}{getText}{getText}{getText}{getText} <size=20><i>!$!</i></size>"};
			textSAction.text = textAction.text;
			textAction.Initialize(damageUI);
			textSAction.Initialize(damageUI);
			
			GameManager.SetTimeScale(0.05f, 0.5f);
			CameraBrain.instance.ShakeLerp(2f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
			CameraBrain.instance.ZoomLerp(-5f);
		}
		
		protected override void OnHit(Entity attacker, float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
		}

		protected override void OnHit(Projectile projectile, float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
		}

		protected override void OnHit(float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
		}

		protected override float GetRealDamage(float rawDamage) {
			return 1;
		}
		
		private void ShootMissile(Vector2 pos, float rot, float speed = 13.5f) {
			GameObject missile = UObjectPool.instance.Get("CrystalMissile", pos);
			
			CrystalMissile crystalMissile = missile.GetComponent<CrystalMissile>();
			crystalMissile.owner              = this;
			crystalMissile.damage             = entityStat.attackDamage;
			crystalMissile.offset             = 0;
			crystalMissile.expectAttack       = false;
			crystalMissile.TimeBeforeLockOn   = 0.1f;
			crystalMissile.LockOnDuration     = 0;
			crystalMissile.initialSpeed       = speed;
			crystalMissile.transform.rotation = Quaternion.Euler(0, 0, rot);
			crystalMissile.Category           = "crystalMissile";
		}

		private void ShootBigMissile(int shootN = 1, float spreadTiming = 0f) {
			for (int i = 0; i < shootN; i++) {
				Vector2 pos = new (
					Random.Range(-(mapSize.x / 2 - 1), mapSize.x / 2 - 1),
					mapSize.y - 1
				);
				float rot = Random.Range(175f, 185f);
				float spd = Random.Range(12f,  15f);
				
				new DelayedAction(Random.Range(0f, spreadTiming), () => { ShootBigMissile(pos, rot, spd); }, () => { }, this).ExecuteDA();
			}
		}
		
		private void ShootBigMissile(Vector2 pos, float rot, float speed = 13.5f) {
			GameObject missile = UObjectPool.instance.Get("CrystalBigMissile", pos);
			
			CrystalMissile crystalMissile = missile.GetComponent<CrystalMissile>();
			crystalMissile.owner              = this;
			crystalMissile.damage             = entityStat.attackDamage;
			crystalMissile.offset             = 0;
			crystalMissile.expectAttack       = false;
			crystalMissile.TimeBeforeLockOn   = 0.1f;
			crystalMissile.LockOnDuration     = 0;
			crystalMissile.initialSpeed       = speed;
			crystalMissile.transform.rotation = Quaternion.Euler(0, 0, rot);
			crystalMissile.Category           = "crystalMissile";
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
		
		private void ShootSemiHugeMissile(Vector2 pos, float rot, float speed = 5f) {
			Vector2      dir       = new (-Mathf.Sin(rot * Mathf.Deg2Rad), Mathf.Cos(rot * Mathf.Deg2Rad));
			RaycastHit2D hit       = Physics2D.Raycast(pos, dir, 100, LayerMask.GetMask("Map"));
			float        dist      = Vector2.Distance(pos, hit.point)-0.5f;
			float        timeToHit = dist / speed;
			
			Debug.DrawRay(pos + dir * 0.5f, dir * dist, Color.green, timeToHit);
			
			GameObject obj = UObjectPool.instance.Get("CrystalSemiHugeMissile", pos);
			obj.transform.rotation = Quaternion.Euler(0, 0, rot);
			BasicProjectile semiHugeMissile = obj.GetComponent<BasicProjectile>();
			semiHugeMissile.Category   = "crystalMissile";
			semiHugeMissile.rigidbody2D.linearVelocity = dir * speed;
			
			MissileAttackAreas.Add(AttackArea(1, timeToHit, pos, new Vector2(0.65f, 100), rot));
		}

		private void Shake(float intensity, int semiHugeMissile) {
			CameraBrain.instance.ShakeLerp((intensity+semiHugeMissile)*5, 10f);
			CameraBrain.instance.ZoomLerp(-0.7f);

			ShootBigMissile(Random.Range((int)intensity, 5 + (int)intensity), 2f);
			for (int i = 0; i < semiHugeMissile; i++) {
				Vector2 pos = new Vector2(
					Random.Range(-(mapSize.x / 2 - 1), mapSize.x / 2 - 1),
					mapSize.y - 1
				);
				ShootSemiHugeMissile(pos, Random.Range(175f, 185f), 7.5f);
			}
		}

		public override void OnStunStart() { }
		public override void OnStunEnd() { }
		
		public int  attackPhase;
		public int  julNumGiPhase = -1;
		
		private ExclusiveAction[] JulNumGi;
		public override void Attack() {
			if (!attackable) return;
			attackable = false;
			
			PlaySFX("crystalRoar", pitch:Random.Range(0.8f, 1.2f));
			
			JulNumGi ??= new [] {
				new ExclusiveAction(JulNumGi1Enumerator(), OnJulnumgiCancel, OnJulnumgiDone, 0, this),
				new ExclusiveAction(JulNumGi2Enumerator(), OnJulnumgiCancel, OnJulnumgiDone, 0, this),
				new ExclusiveAction(JulNumGi1Enumerator(), OnJulnumgiCancel, OnJulnumgiDone, 0, this),
				new ExclusiveAction(JulNumGi2Enumerator(), OnJulnumgiCancel, OnJulnumgiDone, 0, this)
			};
			
			if (attackPhase == 11) InfoUUI.instance.AddInfoMessage("크리스탈이 곧 <color=#ff5a5a>강력한 공격</color>을 사용합니다. 서두르세요!");
			
			Debug.Log($"[Crystal Attack] attack phase = {attackPhase}");
			
			currentExclusiveAction = attackPhase switch {
				0  => JulNumGi[julNumGiPhase],
				1  => SpawnCrystalEnergy,
				2  => SpawnSemiHugeU, // 10s
				3  => SpawnSemiHugeC, // 10s
				4  => Attacking,      // 1s
				5  => SpawnSemiHugeC, // 10s
				6  => SpawnSemiHugeU, // 10s
				7  => Attacking,      // 1s
				8  => SpawnSemiHugeU, // 10s
				9  => SpawnSemiHugeC, // 10s
				10 => Attacking,      // 1s
				11 => SpawnSemiHugeC, // 10s
				12 => SpawnSemiHugeU, // 10s
				13 => Attacking,      // 1s
				14 => DeathAttack,    // FKING STRONG ATTACK
				15 => CrystalUltimateRay,
				_  => throw new Exception("NO BRO THAT'S NOT WHAT I WANT :(")
			};

			if (attackPhase++ == 1) attackPhase++; // attackPhase +1 (if aP == 1 attackPhase +2)
		}
		
		// private ExclusiveAction e => new (e(), OnAttackCancel, OnAttackDone, 0, this);
		private ExclusiveAction SpawnCrystalEnergy => new (SpawnEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
		private ExclusiveAction SpawnSemiHugeU     => new (SpawnSemiHugeUEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
		private ExclusiveAction SpawnSemiHugeC     => new (SpawnSemiHugeCEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
		private ExclusiveAction DeathAttack        => new (DeathAttackEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
		private ExclusiveAction CrystalUltimateRay => new (CrystalUltimateRayEnumerator(), OnAttackCancel, OnAttackDone, 0, this);

		protected override IEnumerator AttackEnumerator() {
			Shake(15, 3);
			yield return new WaitForSeconds(1f);
		}
		
		private int[]      energyCount         = { 2, 3, 4, 4 };
		private int        spawnN              = 0;
		private bool       weCanTakeDamageInfo = false;
		private WaitAction crystalEnergyGimmick1;
		private WaitAction crystalEnergyGimmick2;
		protected  IEnumerator SpawnEnumerator() {
			moveMode             = 0;
			entityStat.moveSpeed = entityData.moveSpeed/1.5f;
			
			if (!weCanTakeDamageInfo) {
				weCanTakeDamageInfo = true;
				InfoUUI.instance.AddInfoMessage("크리스탈의 <color=#ffea5a>힘의 파편</color>을 파괴하여 크리스탈에게 피해를 입힐 수 있습니다.");
			}
			
			InfoUUI.instance.AddInfoMessage("크리스탈이 <color=#ff5a5a>강력한 공격</color>을 준비합니다. 힘의 파편을 파괴하여 저지하세요.");
			
			int n = energyCount[spawnN]; 
			GameManager.UValueFloatVariables["crystalEnergyDead"]   = new UPureNumber {number = 0};
			GameManager.UValueFloatVariables["crystalEnergyGimmick"] = new UPureNumber {number = n};
			
			Debug.Log($"[Crystal Attack] spawnN = {spawnN}");
			
			// todo
			//  체킹하는거에서 실패하면 아예 WaitAction 못나가게
			crystalEnergyGimmick1??=new WaitAction(
				()=>(int)GameManager.UValueFloatVariables["crystalEnergyGimmick"].value == 0,
				() => {
					SendAttackEvent(this, 1, true);
					attackPhase = 0;
					julNumGiPhase++;
				},
				executor:this);
			
			crystalEnergyGimmick2??=new WaitAction(
				()=>(int)GameManager.UValueFloatVariables["crystalEnergyGimmick"].value == 0,
				() => {
					SendAttackEvent(this, 1, true);
					attackPhase = 15;
				},
				executor:this);

			(spawnN == energyCount.Length - 1 ? crystalEnergyGimmick2 : crystalEnergyGimmick1).ExecuteWA();
			
			for (int i = 1; i <= n; i++) {
				float spawnPosX = -mapSize.x/2 + mapSize.x * i / (n + 1);
				float spawnPosY = 1.2f + Random.Range(-0.1f, 0.1f);
				Vector2 spawnPos = new (spawnPosX, spawnPosY);
				GameObject crystalEnergyObject = UObjectPool.instance.Get("CrystalEnergy", spawnPos, 6 - 5f*i/n);
				CrystalEnergy crystalEnergy = crystalEnergyObject.GetComponent<CrystalEnergy>();
				crystalEnergy.Category = "crystalEnergy";
				
				yield return new WaitForSeconds(5f/n);
			}
			
			spawnN++;
		}
		
		protected IEnumerator SpawnSemiHugeUEnumerator() {
			const int patternRepeatNum = 2;
			
			for (int i=0; i<patternRepeatNum; i++){
				moveMode             = transform.position.x > 0 ? 2 : 3;
				entityStat.moveSpeed = entityData.moveSpeed * 3;

				int   n  = Random.Range(8, 13);
				float un = 1f / n;

				for (int j = 0; j < n - 1; j++) {
					yield return new WaitForSeconds(pi * un);
					ShootSemiHugeMissile(transform.position, Random.Range(178, 182f), Random.Range(3f, 5f));
				}
				
				yield return new WaitForSeconds(pi * u4);
			}
			moveMode = 0; 
			yield return new WaitForSeconds(2f);
		}
		
		protected IEnumerator SpawnSemiHugeCEnumerator() {
			moveMode = 4;
			
			const int semiHugeMissileCount = 4;
			
			for (int i = 0; i < semiHugeMissileCount; i++) {
				yield return new WaitForSeconds(2f);
				float angle = Random.Range(0f, 45f);
				for (int j = 0; j < 8; j++) {
					int _j = j;
					new DelayedAction(u8*j, ()=>ShootSemiHugeMissile(transform.position, angle+45*_j,  7f)).ExecuteDA();
				}
			}
			
			yield return new WaitForSeconds(2f);
		}

		private Vector2         oldMapSize;
		private DelayedAction[] slashDelayedActions;
		protected IEnumerator JulNumGi1Enumerator() {
			moveMode = 1;
			oldMapSize = mapSize;
			MapManager.instance.SetMapSize(new Vector2(oldMapSize.x + 20, oldMapSize.y), 0.01f);
			
			yield return new WaitForSeconds(2f);
			Shake(10f, 0);

			int   barrageNum      = 10;
			float barrageSpeed     = player.entityStat.moveSpeed*5;
			float barrageTerm = 3f;
			
			const int   slashCount       = 4;
			const int   slashDivideCount = 6;
			float       slashLengthX     = oldMapSize.x / slashDivideCount;
			const float patternDuration  = 2;
			
			float endTime        = 10 / barrageSpeed + 1;
			float totalTime = endTime + barrageTerm * barrageNum;
			
			for (int i = 0; i < totalTime - 2; i+=2) {
				Vector4[] slashData = new Vector4[slashCount];
				int       lastX     = -2; // initial Value
				
				for (int j = 0; j < slashCount; j++) {
					float duration = Random.Range(2, patternDuration);
					
					lastX = Random.Range(lastX+1, slashDivideCount - slashCount + j + 2);
					Vector2 startPos = new(slashLengthX*(lastX+0.5f) - oldMapSize.x/2, 0);
					Vector2 endPos   = new(startPos.x, mapSize.y);

					Vector2 dir   = endPos - startPos;
					float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;

					Vector2 center = (startPos + endPos) * 0.5f;
					float   length = dir.magnitude;
					
					slashData[j] = new Vector4(center.x, center.y, length, angle);
					new DelayedAction(patternDuration - duration + i, () => {
						AttackArea(2, duration, center, new Vector2(slashLengthX, length + 5f), angle);
					}).ExecuteDA();
				}

				new DelayedAction(patternDuration + i, () => {
					foreach (Vector4 data in slashData) {
						GameObject obj = UObjectPool.instance.Get("SlashEffect", new Vector2(data.x, data.y + Random.Range(-2f, 2f)));
						obj.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
						obj.transform.localScale = new Vector3(3f, mapSize.y + Random.Range(-1f, 1f), 1f);
					}
					
					CameraBrain.instance.ShakeLerp(5, 3);
					CameraBrain.instance.ZoomLerp(-2f);
				}).ExecuteDA();
			}
			bool  isRight = Random.Range(0, 2) == 0;
			float xPos    = isRight ? (mapSize.x / 2 - 1) : (1 - mapSize.x / 2);
			float rot     = isRight ? 90 : 270;
			
			for (int i = 0; i < barrageNum; i++) {
				yield return new WaitForSeconds(3f);
				
				for (int j = 2; j < mapSize.y*2; j++) {
					Vector2 pos = new (xPos, j/2f);
					ShootBigMissile(pos, rot, barrageSpeed*2);
				}
				
				CameraBrain.instance.ShakeLerp(4, 10f);
				CameraBrain.instance.ZoomLerp(-0.7f);
			}

			yield return new WaitForSeconds(endTime);
		}
		
		protected IEnumerator JulNumGi2Enumerator() {
			moveMode = 1;
			oldMapSize = mapSize;
			MapManager.instance.SetMapSize(new Vector2(oldMapSize.x + 20, oldMapSize.y), 0.01f);
			
			yield return new WaitForSeconds(2f);
			Shake(10f, 0);
			
			const int   barrageNum   = 10;
			float       barrageSpeed = player.entityStat.moveSpeed*5;
			const float barrageTerm  = 3f;
			const float halfBarrageTerm = barrageTerm / 2;
			
			const int   slashCount       = 4;
			const int   slashDivideCount = 6;
			float       slashLengthX     = oldMapSize.x / slashDivideCount;
			const float patternDuration  = 2;
			
			float endTime        = 10 / barrageSpeed + 1;
			float totalTime = endTime + barrageTerm * barrageNum;
			
			for (int i = 0; i < totalTime - 2; i+=2) {
				Vector4[] slashData = new Vector4[slashCount];
				int       lastX     = -2; // initial Value
				
				for (int j = 0; j < slashCount; j++) {
					float duration = Random.Range(2, patternDuration);

					lastX = Random.Range(lastX+1, slashDivideCount - slashCount + j + 2);
					Vector2 startPos = new(slashLengthX*(lastX+0.5f) - oldMapSize.x/2, 0);
					Vector2 endPos   = new(startPos.x, mapSize.y);

					Vector2 dir   = endPos - startPos;
					float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;

					Vector2 center = (startPos + endPos) * 0.5f;
					float   length = dir.magnitude;
					
					slashData[j] = new Vector4(center.x, center.y, length, angle);
					new DelayedAction(patternDuration - duration + i, () => {
						AttackArea(2, duration, center, new Vector2(slashLengthX, length + 5f), angle);
					}).ExecuteDA();
				}
				
				new DelayedAction(patternDuration + i, () => {
					foreach (Vector4 data in slashData) {
						GameObject obj = UObjectPool.instance.Get("SlashEffect", new Vector2(data.x, data.y + Random.Range(-2f, 2f)));
						obj.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
						obj.transform.localScale = new Vector3(3f, mapSize.y + Random.Range(-1f, 1f), 1f);
					}
					
					CameraBrain.instance.ShakeLerp(5, 3);
					CameraBrain.instance.ZoomLerp(-2f);
				}).ExecuteDA();
			}
			
			// Lambda Scope Warning //
			bool  isRight         = true;
			for (int i = 0; i < barrageNum; i++) {
				yield return new WaitForSeconds(halfBarrageTerm);
				
				//bool  isRight = i%2 == 0;
				isRight = !isRight;
				float xPos    = isRight ? (mapSize.x / 2 - 1) : (1 - mapSize.x / 2);
				float rot     = isRight ? 90 : 270;
				
				for (int j = 2; j < mapSize.y*2; j++) {
					Vector2 pos = new Vector2(xPos, j/2f);
					ShootBigMissile(pos, rot, barrageSpeed*2);
				}
				
				CameraBrain.instance.ShakeLerp(4, 10f);
				CameraBrain.instance.ZoomLerp(-0.7f);
				
				yield return new WaitForSeconds(halfBarrageTerm);
			}

			yield return new WaitForSeconds(endTime);
		}
		// JulNumGi3Enumerator
		
		protected IEnumerator DeathAttackEnumerator() {
			moveMode = 4;

			crystalEnergyGimmick1.Cancel();
			crystalEnergyGimmick2.Cancel();

			foreach (UObject obj in GetUObjects("crystalEnergy")) {
				CrystalEnergy crystalEnergy = (CrystalEnergy)obj;
				crystalEnergy.entityStat.hp = 0;
			}
			
			yield return new WaitForSeconds(0.5f);

			PlaySFX("crystalDeathAttack");
			
			Vector2 hitboxPos = Vector2.up * (mapSize.y / 2);
			AttackAreaNoEffect(999, 10, hitboxPos, mapSize+Vector2.one, ignoreInvincible:true);

			GameObject uui = UUIObjectPool.instance.Open("CrystalUltEffect", GameManager.instance.mainScreenCanvas);
			
			// 
			yield return new WaitForSeconds(9.5f);
			
			GameObject eff1 = UObjectPool.instance.Get("SlashEffect", transform.position);
			eff1.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(40, 50));
			eff1.transform.localScale = new Vector3(mapSize.x*1.1f, mapSize.y*1.2f, 1f);
			GameObject eff2 = UObjectPool.instance.Get("SlashEffect", transform.position);
			eff2.transform.rotation   = Quaternion.Euler(0, 0, -Random.Range(40, 50));
			eff2.transform.localScale = new Vector3(mapSize.x*1.1f, mapSize.y*1.2f, 1f);
			
			GameManager.SetTimeScale(0f, 1f);
			CameraBrain.instance.ShakeLerp(75, 10f);
			CameraBrain.instance.ZoomLerp(-10f);
			
			ClearMissiles();
			
			UUIObjectPool.instance.Close(uui, true);
		}

		private CrystalUltRay SpawnRay(float r, float w, float o, bool showBlackBG = false) {
			GameObject    ultRayObj = UObjectPool.instance.Get("CrystalUltRay", new Vector2(0, mapSize.y/2), 0.001f);
			CrystalUltRay ultRay    = ultRayObj.GetComponent<CrystalUltRay>();
			ultRay.owner       = this;
			ultRay.r           = r;
			ultRay.w           = w;
			ultRay.o           = Mathf.Repeat(o, 2*pi);
			ultRay.showEffect = showBlackBG;
			
			return ultRay;
		}
		
		protected IEnumerator CrystalUltimateRayEnumerator() {
			moveMode = 4;
			
			CrystalUltRay ray1, ray2, ray3, ray4, ray5, ray6, ray7, ray8;
			const float   crystalMissileNum = 15;
			const float   dO                = 360f / crystalMissileNum;
			// rt ~~ 3.29
			const float   rt                = CrystalUltRay.rayShootTime;

			WaitForSeconds rtWait   = new WaitForSeconds(rt);
			WaitForSeconds rtWait2  = new WaitForSeconds(rt  -2f);
			WaitForSeconds rtWait6  = new WaitForSeconds(6f  -rt);
			WaitForSeconds rtWait8  = new WaitForSeconds(8f  -rt);
			WaitForSeconds rtWait10 = new WaitForSeconds(10f -rt);
			
			MapManager.instance.SetMapSize(new Vector2(15, 15), 0.01f);
			
			yield return new WaitForSeconds(2f);
			
			ray1 = SpawnRay(3.5f, 2f, Random.Range(0, 2*pi), true);
			
			yield return rtWait;
			float spawnOffset = Random.Range(0f, 360f);
			for (int i = 0; i < crystalMissileNum; i++) { ShootMissile(transform.position, spawnOffset+i *dO, 15); }
			yield return rtWait10;

			UObjectPool.instance.Release(ray1.gameObject, 4);

			yield return new WaitForSeconds(1.5f);

			float offset = Random.Range(0, 2 * pi);
			ray1 = SpawnRay(3.5f, 1.75f, offset, true);
			ray2 = SpawnRay(3.5f, 1.75f, offset + pi);

			yield return rtWait;
			spawnOffset = Random.Range(0f, 360f);
			for (int i = 0; i < crystalMissileNum; i++) { ShootMissile(transform.position, spawnOffset+i *dO, 15); }
			yield return rtWait10;

			UObjectPool.instance.Release(ray1.gameObject, 4);
			UObjectPool.instance.Release(ray2.gameObject, 4);
			
			yield return new WaitForSeconds(1.5f);
			
			offset = Random.Range(0, 2 * pi);
			ray1   = SpawnRay(3.5f, -1.5f, offset, true);
			ray2   = SpawnRay(3.5f, -1.5f, offset +pi*2*u3);
			ray3   = SpawnRay(3.5f, -1.5f, offset +pi*4*u3);
			
			yield return rtWait;
			spawnOffset = Random.Range(0f, 360f);
			for (int i = 0; i < crystalMissileNum; i++) { ShootMissile(transform.position, spawnOffset+i*dO, 15); }
			yield return rtWait8; // 8s
			
			offset = Random.Range(0, 2 * pi);
			ray5   = SpawnRay(3.5f, 1.5f, offset, true);
			ray6   = SpawnRay(3.5f, 1.5f, offset +pi*u2);
			ray7   = SpawnRay(3.5f, 1.5f, offset +pi);
			ray8   = SpawnRay(3.5f, 1.5f, offset + pi * 3 * u2);

			yield return new WaitForSeconds(2f);
			
			UObjectPool.instance.Release(ray1.gameObject, 4);
			UObjectPool.instance.Release(ray2.gameObject, 4);
			UObjectPool.instance.Release(ray3.gameObject, 4);

			yield return rtWait2; // rt
			spawnOffset = Random.Range(0f, 360f);
			for (int i = 0; i < crystalMissileNum; i++) { ShootMissile(transform.position, spawnOffset+i*dO, 15); }
			yield return rtWait6; // ??
			
			offset = Random.Range(0, 2 * pi);
			ray1   = SpawnRay(3.5f, -1.5f,  offset, true);
			ray2   = SpawnRay(3.5f, -1.5f, offset +pi*u2);
			ray3   = SpawnRay(3.5f, -1.5f,  offset +pi);
			ray4   = SpawnRay(3.5f, -1.5f, offset + pi * 3 * u2);
			
			yield return new WaitForSeconds(2f);
			
			UObjectPool.instance.Release(ray5.gameObject, 4);
			UObjectPool.instance.Release(ray6.gameObject, 4);
			UObjectPool.instance.Release(ray7.gameObject, 4);
			UObjectPool.instance.Release(ray8.gameObject, 4);
			
			yield return rtWait2; // rt
			spawnOffset = Random.Range(0f, 360f);
			for (int i = 0; i < crystalMissileNum; i++) { ShootMissile(transform.position, spawnOffset+i*dO, 15); }
			yield return rtWait8;

			UObjectPool.instance.Release(ray1.gameObject, 4);
			UObjectPool.instance.Release(ray2.gameObject, 4);
			UObjectPool.instance.Release(ray3.gameObject, 4);
			UObjectPool.instance.Release(ray4.gameObject, 4);
			
			ray1   = SpawnRay(3f,   1f, Random.Range(0, 2 * pi), true);
			ray2   = SpawnRay(4.5f, 2.5f, Random.Range(0, 2 * pi));

			WaitForSeconds s = new(0.5f);
			yield return rtWait;
			for (int j = 0; j < 20; j++) {
				spawnOffset = Random.Range(0f, 360f);
				for (int i = 0; i < crystalMissileNum; i++) { ShootMissile(transform.position, spawnOffset+i*dO, 15); }
				yield return s;
			}
			
			UObjectPool.instance.Release(ray1.gameObject, 4);
			UObjectPool.instance.Release(ray2.gameObject, 4);
			
			offset = Random.Range(0, 2 * pi);
			
			ray1 = SpawnRay(4.5f, 0, offset, true);
			ray2 = SpawnRay(4.5f, 0, offset +pi *u4);
			ray3 = SpawnRay(4.5f, 0, offset +pi *u2);
			ray4 = SpawnRay(4.5f, 0, offset +pi *3*u4);
			ray5 = SpawnRay(4.5f, 0, offset +pi);
			ray6 = SpawnRay(4.5f, 0, offset +pi *5 *u4);
			ray7 = SpawnRay(4.5f, 0, offset +pi *3 *u2);
			ray8 = SpawnRay(4.5f, 0, offset +pi *7 *u4);
			
			animator.Play("Death");
			PlaySFX("crystalDeath");
			
			moveMode = 5;
			
			yield return rtWait;
			spawnOffset = Random.Range(0f, 360f);
			for (int i = 0; i < crystalMissileNum; i++) { ShootMissile(transform.position, spawnOffset+i*dO, 15); }
			yield return rtWait6;
			
			UObjectPool.instance.Release(ray1.gameObject, 4);
			UObjectPool.instance.Release(ray2.gameObject, 4);
			UObjectPool.instance.Release(ray3.gameObject, 4);	
			UObjectPool.instance.Release(ray4.gameObject, 4);
			UObjectPool.instance.Release(ray5.gameObject, 4);
			UObjectPool.instance.Release(ray6.gameObject, 4);
			UObjectPool.instance.Release(ray7.gameObject, 4);
			UObjectPool.instance.Release(ray8.gameObject, 4);

			yield return new WaitForSeconds(4f);
			
			entityStat.hp = 0;
		}
		
		protected override void OnAttackDone() {
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			attackable             = false;
			
			entityStat.moveSpeed = entityData.moveSpeed;
			
			moveMode = 0;
			t        = Random.Range(0, pi);
		}
		
		protected override void OnAttackCancel() {
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			attackable             = false;
			
			entityStat.moveSpeed = entityData.moveSpeed;
			
			moveMode = 0;
			t        = Random.Range(0, pi);
		}
		
		protected void OnJulnumgiDone() {
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			attackable             = false;
			
			MapManager.instance.SetMapSize(oldMapSize, 0.02f);
			ClearMissiles();
			
			entityStat.moveSpeed = entityData.moveSpeed;
			
			moveMode = 0;
			t        = Random.Range(0, pi);
		}
		
		protected void OnJulnumgiCancel() {
			state                  = EnemyState.Alert;
			currentExclusiveAction = FindingAggro;
			attackable             = false;
			
			MapManager.instance.SetMapSize(oldMapSize, 0.02f);
			ClearMissiles();
			
			entityStat.moveSpeed = entityData.moveSpeed;
			
			moveMode = 0;
			t        = Random.Range(0, pi);
		}
		
		public override void WanderRoutine() {
			if (!aggroEntity) return;
			state           = EnemyState.Alert;
		}

		private StopWatch missileStopwatch = new StopWatch();
		private int       missileLaunched;
		public override void AlertRoutine() {
			if (!aggroEntity) {
				state = EnemyState.Wander;
				return;
			}
			
			if (attackable) {
				state = EnemyState.Attack;
				return;
			}

			if (!missileStopwatch.Check(0.25f)) {
				ShootBigMissile();
				
				missileStopwatch.Tick();
			}
		}

		public override void AttackRoutine() { }

		public  int     _moveMode;

		public int moveMode {
			get => _moveMode;
			set {
				if (_moveMode == value) return;
				t         = 0;
				_moveMode = value;
			}
		}
		
		public  Vector2 targetPos;
		private float   t;

		private const float pi  = 3.14159265358979323846f;
		private const float U   = pi*pi*pi*2 /3f - pi;
		private const float uU  = 1f / U;
		private const float uU1 = 1f / (4*pi*pi);
		private const float u2  = 0.5f;
		private const float u3 = 1f/3f;
		private const float u4 = 0.25f;
		private const float u8 = 0.125f;
		
		public void setTargetPosition() {
			t += DeltaTime * moveMode switch {
				0 => 0.3f,
				1 => 0.5f,
				2 => 2f,
				3 => 2f,
				4 => 0.3f,
				5 => 0.2f,
				_ => 1f
			};
			
			Vector2  playerPos = player?.transform.position??Vector2.zero;
			targetPos = moveMode switch {
				0 => new Vector2(mapSize.x * Mathf.Sin(2 * t)*u3, mapSize.y*u2 + mapSize.y*Mathf.Sin(3 * t)*u4 + mapSize.y*u8),
				1 => new Vector2(playerPos.x + Mathf.Sin(2 * t), 2.625f + playerPos.y + 0.5f * Mathf.Sin(t)),
				2 => new Vector2((mapSize.x*u2 - 3)*(t*t*t*u3 - t*t*pi + t + U)*uU, mapSize.y*(t-pi)*(t-pi)*uU1 + mapSize.y*u2),
				3 => new Vector2(-(mapSize.x*u2 - 3)*(t*t*t*u3 - t*t*pi + t + U)*uU, mapSize.y*(t-pi)*(t-pi)*uU1 + mapSize.y*u2),
				4 => new Vector2(Mathf.Cos(2*t)*u2, Mathf.Sin(2*t)*u2+mapSize.y/2),
				5 => new Vector2(t*Mathf.Cos(Random.Range(0, pi*2)), t*Mathf.Sin(Random.Range(0, pi*2))+mapSize.y/2),
				_ => Vector2.zero
			};
        
			transform.rotation     = Quaternion.Euler(0f, 0f, -2 * rigidbody2D.linearVelocityX);
			rotatingAnimator.speed = rigidbody2D.linearVelocity.magnitude;
		}

		protected override void EarlyRoutine() {
			base.EarlyRoutine();
			
			mapSize = MapManager.instance.GetMapSize();
			
			ForceInvincibleTrue(true);
			setTargetPosition();
		}

		protected override void FixedRoutine() {
			base.FixedRoutine();

			if (state is not (EnemyState.Alert or EnemyState.Attack)) return;
			float d                             = Vector2.Distance(transform.position, targetPos);
			if (d <= 0.001f) transform.position = targetPos;
			rigidbody2D.linearVelocity = (targetPos-(Vector2)transform.position).normalized * (entityStat.moveSpeed * (d < 4 ? d/4 : 1));
		}

		public override void AttackReadyRoutine() { }

		public override void StunRoutine() { }

		public override void Initialize() {
			base.Initialize();
			
			GameManager.instance.Crystal.transform.SetParent(transform);
			GameManager.instance.Crystal.transform.localPosition = Vector3.zero;
			
			foreach (GameObject obj in hideOnDeath) {
				obj.SetActive(true);
			}
			
			InfoUUI.instance.AddInfoMessage("크리스탈의 보호막으로 인해 <color=#ff5a5a>평범한 공격</color>으로는 피해를 입힐 수 없습니다.");
		}

		protected override void Death() {
			foreach (GameObject obj in hideOnDeath) {
				obj.SetActive(false);
			}

			PlaySFX("crystalDeath2");
			
			GameObject eff1 = UObjectPool.instance.Get("SlashEffect", transform.position);
			eff1.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(40, 50));
			eff1.transform.localScale = new Vector3(mapSize.x*1.1f, mapSize.y*1.2f, 1f);
			GameObject eff2 = UObjectPool.instance.Get("SlashEffect", transform.position);
			eff2.transform.rotation   = Quaternion.Euler(0, 0, -Random.Range(40, 50));
			eff2.transform.localScale = new Vector3(mapSize.x*1.1f, mapSize.y*1.2f, 1f);
			
			GameManager.SetTimeScale(0.1f, 1.5f);

			new DelayedAction(1.5f, ()=> {
				CameraBrain.instance.ShakeLerp(75, 10f);
			}, () => { }).ExecuteDA(true);
			CameraBrain.instance.ZoomLerp(-5f);

			GameManager.UValueFloatVariables["crystalDead"] = new UPureNumber {number = GameManager.UValueFloatVariables["crystalDead"].value + 1};
			GameManager.UValueFloatVariables["EnemyDead"]   = new UPureNumber {number = GameManager.UValueFloatVariables["EnemyDead"].value + 1};
			
			base.Death();
		}
	}
}