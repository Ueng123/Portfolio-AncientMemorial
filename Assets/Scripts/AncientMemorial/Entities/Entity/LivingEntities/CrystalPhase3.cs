using System;
using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
using UengSystem.Audio;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.States;
using UengSystem.UI;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public class CrystalPhase3 : CrystalBoss {

		public GameObject[] hideOnDeath;
		protected override      void OnGrounded() { }
		public Animator   rotatingAnimator;
		private Vector2 mapSize;

		private bool shootJeonBangWuiMissile = false;
		
		private readonly string[] damageDisplayTexts = new[] {"1", "2", "3", "4", "5", "6", "7", "8", "9", "9", "9", "9", "9", "0", "$", "#", "@", "!", "$", "#", "@", "!" };
		private          string   getText => damageDisplayTexts[Random.Range(0, damageDisplayTexts.Length)];
		public override    void        OnHit(Entity    attacker,   float    damage, Vector2? pushDir = null) {
			PlaySFX("crystalHit2");
			
			ShowCriticalDamageUI(99999);
			
			GameManager.SetTimeScale(0.05f, 1f);
			CameraBrain.instance.ShakeLerp(3, 5);
			CameraBrain.instance.ZoomLerp(-2f);
		}

		protected override float GetRealDamage(float rawDamage) {
			return 1;
		}

		private void ShootOmnidirectionalMissile(int missileNum = 0, float speed = 15) {
			float crystalMissileNum = missileNum==0?Random.Range(13, 18):missileNum;
			float dO                = 360f / crystalMissileNum;
			float spawnOffset       = Random.Range(0f, dO);
			
			for (int i = 0; i < crystalMissileNum; i++) { ShootMissile(transform.position, spawnOffset+i*dO, speed); }
		}
		
		private void ShootBigMissile(int shootN = 1, float duration = 0f) {
			Vector2[] posData = new Vector2[shootN];
			float[] rotData = new float[shootN];
			float[] speedData = new float[shootN];
			
			for (int i = 0; i < shootN; i++) {
				posData[i] = new Vector2(
					Random.Range(-(mapSize.x / 2 - 1), mapSize.x / 2 - 1),
					mapSize.y - 1
				);
				rotData[i] = Random.Range(175f, 185f);
				speedData[i] = Random.Range(12f,  15f);
			}

			StartCoroutine(SpreadBigMissile(duration, posData, rotData, speedData));
		}

		private IEnumerator SpreadBigMissile(float duration, Vector2[] posData, float[] rotData, float[] speedData) {
			if (!(posData.Length == rotData.Length && rotData.Length == speedData.Length)) yield break;
			int missileCount = posData.Length;
			
			float[] spawnTimes = new float[missileCount];
			for (int i = 0; i < missileCount; i++) { spawnTimes[i] = Random.Range(0f, duration); }
			Array.Sort(spawnTimes);

			float elapsedTime = 0f;
			for (int i = 0; i < missileCount; i++) {
				float waitTime = spawnTimes[i] - elapsedTime;
				yield return new WaitForSeconds(waitTime);
				elapsedTime += waitTime;
				
				ShootBigMissile(posData[i], rotData[i], speedData[i]);
			}
		}

		private void ClearMissiles() {
			List<UObject> objs = GetUObjects("crystalMissile");
			for (int i = objs.Count - 1; i >= 0; i--) {
				UObject obj = objs[i];
				UObjectPool.instance.Release(obj.gameObject);
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
			
			MissileAttackAreas.Add(AttackArea(1, timeToHit, pos, new Vector2(0.65f, MapManager.instance.GetMapSize().magnitude*2), rot));
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

		private float LookAngleDeg(Vector2 from, Vector2 at) {
			return Mathf.Atan2(at.y - from.y, at.x - from.x) * Mathf.Rad2Deg;
		}
		
		private float LookAngleRad(Vector2 from, Vector2 at) {
			return Mathf.Atan2(at.y - from.y, at.x - from.x); // tlqkf
		}
		
		public int  attackPhase;
		public int  julNumGiPhase = -1;
		
		private UState[] JulNumGi;
		public override void Attack() {
			if (!attackable) return;
			attackable = false;
			
			PlaySFX("crystalRoar", pitch:Random.Range(0.8f, 1.2f));
			
			// JulNumGi ??= new [] {
			// 	new UState(JulNumGi1Enumerator(), OnJulnumgiCancel, OnJulnumgiDone, 0, this),
			// 	new UState(JulNumGi2Enumerator(), OnJulnumgiCancel, OnJulnumgiDone, 0, this),
			// 	new UState(JulNumGi3Enumerator(), OnJulnumgiCancel, OnJulnumgiDone, 0, this),
			// 	new UState(JulNumGi4Enumerator(), OnJulnumgiCancel, OnJulnumgiDone, 0, this)
			// };


			if (!rayAttackEnabled && attackPhase is 3 or 7 or 9 or 13) attackPhase++;
			
			if (attackPhase == 12) InfoUUI.instance.AddInfoMessage("크리스탈이 곧 <color=#ff5a5a>강력한 공격</color>을 사용합니다. 서두르세요!");
			
			Debug.Log($"[Crystal Attack] attack phase = {attackPhase}");
			//
			// ((UObject)this).state = attackPhase switch {
			// 	0  => JulNumGi[julNumGiPhase],
			// 	1  => SpawnCrystalEnergy,
			// 	// 2  => Attacking,
			// 	3  => RayCAttack,
			// 	4  => spawnSemiHuge,
			// 	5  => SpawnSemiHugeC,
			// 	// 6  => Attacking,
			// 	7  => RayRAttack,
			// 	8  => SpawnSemiHugeC,
			// 	9  => RayRAttack,
			// 	// 10 => Attacking,
			// 	11 => spawnSemiHuge,
			// 	12 => SpawnSemiHugeC,
			// 	13 => RayCAttack,
			// 	14 => DeathAttack,
			// 	15 => CrystalUltimateRay,
			// 	_  => throw new Exception("NO BRO THAT'S NOT WHAT I WANT :(")
			// };

			attackPhase++;
		}
		
		// private ExclusiveAction e => new (e(), OnAttackCancel, OnAttackDone, 0, this);
		// private UState SpawnCrystalEnergy => new (SpawnEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
		// private UState spawnSemiHuge     => new (SpawnSemiHugeUEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
		// private UState SpawnSemiHugeC     => new (SpawnSemiHugeCEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
		// private UState DeathAttack        => new (DeathAttackEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
		// private UState RayCAttack         => new (RayCAttackEnumerator(), OnRayAttackCancel, OnRayAttackDone, 0, this);
		// private UState RayRAttack         => new (RayRAttackEnumerator(), OnRayAttackCancel, OnRayAttackDone, 0, this);
		// private UState CrystalUltimateRay => new (CrystalUltimateRayEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
		
		protected override IEnumerator AttackEnumerator() {
			yield return new WaitForSeconds(1.5f);
			Shake(15, 0);
			
			Vector2 crystalPos = transform.position;
			Vector2 playerPos  = player.transform.position;
			float   playerDir  = LookAngleDeg(crystalPos, playerPos) - 90;
			
			ShootSemiHugeMissile(crystalPos, playerDir, 10f);
			yield return new WaitForSeconds(1.5f);
		}
		
		private int[]      energyCount         = { 1, 2, 3, 3 };
		private int        spawnN              = 0;
		private bool       weCanTakeDamageInfo = false;
		private WaitAction crystalEnergyGimmick1;
		private WaitAction crystalEnergyGimmick2;
		private bool       rayAttackEnabled => entityStat.hp <= 3;
		
		protected  IEnumerator SpawnEnumerator() {
			moveMode             = 0;
			entityStat.moveSpeed = entityData.moveSpeed/1.5f;
			
			if (!weCanTakeDamageInfo) {
				weCanTakeDamageInfo = true;
				InfoUUI.instance.AddInfoMessage("크리스탈의 <color=#ffea5a>힘의 파편</color>을 파괴하여 크리스탈에게 피해를 입힐 수 있습니다.");
			}
			
			InfoUUI.instance.AddInfoMessage("크리스탈이 <color=#ff5a5a>강력한 공격</color>을 준비합니다. 힘의 파편을 파괴하여 저지하세요.");
			
			int n = energyCount[spawnN]; 
			GameManager.UValueFloatVariables["crystalEnergyDead"]   = new UPureFloat {number = 0};
			GameManager.UValueFloatVariables["crystalEnergyGimmick"] = new UPureFloat {number = n};

			Debug.Log($"[Crystal Attack] spawnN = {spawnN}");
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
					PlaySFX("crystalRoar", pitch: 0.5f);
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
			shootJeonBangWuiMissile = true;
			
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
			moveMode                = 4;
			shootJeonBangWuiMissile = true;
			
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
		
		private CameraAlignType oldCameraAlignTypeX;
		private CameraAlignType oldCameraAlignTypeY;

		protected IEnumerator JulNumGi1Enumerator() {
			yield return JulNumGiEnumerator(1, 3, 5);
		}
		
		protected IEnumerator JulNumGi2Enumerator() {
			yield return JulNumGiEnumerator(4, 5, 5);
		}
		
		protected IEnumerator JulNumGi3Enumerator() {
			yield return JulNumGiEnumerator(5, 7, 6);
		}
		
		protected IEnumerator JulNumGi4Enumerator() {
			yield return JulNumGiEnumerator(7, 9, 6);
		}
		
		protected IEnumerator JulNumGiEnumerator(int slashCount, int slashDivideCount, int JulNumGiPatternCount) {
			moveMode = 0;
			oldMapSize = mapSize;
			const float sideIncreaseAmount = 10;
			shootJeonBangWuiMissile = true;
			
			MapManager.instance.SetMapSize(new Vector2(oldMapSize.x + sideIncreaseAmount*2, oldMapSize.y), 0.01f);
			// 양쪽으로 10만큼.

			oldCameraAlignTypeX = CameraBrain.instance.mainCamera.AlignTypeX;
			oldCameraAlignTypeY = CameraBrain.instance.mainCamera.AlignTypeY;
			
			CameraBrain.instance.mainCamera.AlignTypeX = CameraAlignType.Center;
			CameraBrain.instance.mainCamera.AlignTypeY = CameraAlignType.Center;
			
			yield return new WaitForSeconds(2f);
			const float    patternDuration = 2;

			float   slashLengthX       = oldMapSize.x / slashDivideCount;
			int     sideSlashCount     = Mathf.CeilToInt(sideIncreaseAmount/slashLengthX);

			int     totalSlashCount    = slashCount + sideSlashCount * 2;
			int[]   slashXPosIndexList = new int  [totalSlashCount];
			float[] slashXPositions    = new float[totalSlashCount];

			// 0~(slashDivideCount-1)까지 빈칸 뚫릴 수 있는 칸
			for (int i = 0; i < sideSlashCount; i++) {
				slashXPosIndexList[2 * i]     = -i               - 1;
				slashXPosIndexList[2 * i + 1] = slashDivideCount + i;
			}
			// slashXPos[2*sideSlashNum-2+1]까지 참
			int[] slashXPosIndexable = Shuffle.NewShuffledArray(slashDivideCount);
			int   startIndex         = 2*sideSlashCount;
			for (int i = 0; i < slashCount; i++) {
				slashXPosIndexList[startIndex + i] = slashXPosIndexable[i];
			}
			
			WaitForSeconds waitForSeconds  = new WaitForSeconds(patternDuration);
			
			for (int i = 0; i<JulNumGiPatternCount; i++) {
				for (int j = 0; j < totalSlashCount; j++) {
					float currXPos = slashXPositions[j] = slashLengthX*(slashXPosIndexList[j]+0.5f)-oldMapSize.x/2f;

					Vector2 slashPos = new (currXPos, mapSize.y/2f);
					float   length = mapSize.y;
					
					AttackArea(2, patternDuration, slashPos, new Vector2(slashLengthX, length + 5f));
				}
				
				yield return waitForSeconds;

				PlaySFX("crystalSlash");
				foreach (int currXPos in slashXPositions) {
					GameObject obj = UObjectPool.instance.Get("SlashEffect", new Vector2(currXPos, mapSize.y / 2f + Random.Range(-2f, 2f)));
					obj.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
					obj.transform.localScale = new Vector3(3f, mapSize.y + Random.Range(-1f, 1f), 1f);
				}

				CameraBrain.instance.ShakeLerp(2, 10);
				CameraBrain.instance.ZoomLerp(-1f);
				
				slashXPosIndexable = Shuffle.NewShuffledArray(slashDivideCount);
				startIndex         = 2*sideSlashCount;
				for (int j = 0; j < slashCount; j++) {
					slashXPosIndexList[startIndex + j] = slashXPosIndexable[j];
				}
			}
			
			yield return new WaitForSeconds(1);
		}
		
		protected IEnumerator DeathAttackEnumerator() {
			moveMode                = 4;
			shootJeonBangWuiMissile = false;

			crystalEnergyGimmick1.Cancel();
			crystalEnergyGimmick2.Cancel();

			List<UObject> objs = GetUObjects("crystalEnergy");
			for (int i = 0; i < objs.Count; i++) {
				CrystalEnergy crystalEnergy = (CrystalEnergy)objs[i];
				crystalEnergy.entityStat.hp = 0;
			}

			yield return new WaitForSeconds(0.5f);

			AudioManager.instance.PlaySFX("crystalDeathAttack", 1f);
			
			Vector2 hitboxPos = Vector2.up * (mapSize.y / 2);
			AttackAreaNoEffect(999, 10, hitboxPos, mapSize+Vector2.one, ignoreInvincible:true);

			GameObject uui = UUIPool.instance.Open("CrystalUltEffect", GameManager.instance.mainScreenCanvas);
			
			yield return new WaitForSeconds(9.5f);

			PlaySFX("crystalSlash");
			
			GameObject eff1 = UObjectPool.instance.Get("SlashEffect", transform.position);
			eff1.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(40, 50));
			eff1.transform.localScale = new Vector3(mapSize.x*1.1f, mapSize.y*1.2f, 1f);
			GameObject eff2 = UObjectPool.instance.Get("SlashEffect", transform.position);
			eff2.transform.rotation   = Quaternion.Euler(0, 0, -Random.Range(40, 50));
			eff2.transform.localScale = new Vector3(mapSize.x*1.1f, mapSize.y*1.2f, 1f);
			
			GameManager.SetTimeScale(0f, 0.5f);
			CameraBrain.instance.ZoomLerp(-5f);
			
			ClearMissiles();
			
			UUIPool.instance.Close(uui, true);
		}

		List<CrystalUltRay> SpawnedRays = new List<CrystalUltRay>();
		
		private void SpawnRay(float radius, float angularVelocity, float offset, Vector2 position = default, bool showEffect = false) {
			GameObject    ultRayObj = UObjectPool.instance.Get("CrystalUltRay", position==default?new Vector2(0, mapSize.y/2):position, 0.001f);
			
			CrystalUltRay ultRay    = ultRayObj.GetComponent<CrystalUltRay>();
			ultRay.owner           = this;
			ultRay.radius          = radius;
			ultRay.angularVelocity = angularVelocity;
			ultRay.angleOffset     = Mathf.Repeat(offset, 2*pi);
			ultRay.showEffect      = showEffect;
			
			SpawnedRays.Add(ultRay);
		}

		protected IEnumerator RayCAttackEnumerator() {
			moveMode                = 4;
			shootJeonBangWuiMissile = false;
			
			float offset = Random.Range(0, 2 * pi);
			SpawnRay(3.5f, -0.75f, offset, showEffect:true);
			SpawnRay(3.5f, -0.75f, offset +pi*u2);
			SpawnRay(3.5f, -0.75f, offset +pi);
			SpawnRay(3.5f, -0.75f, offset +3*pi*u2);

			for (int i = 0; i < 12; i++) {
				ShootOmnidirectionalMissile(10, 7.5f);
				yield return new WaitForSeconds(1f);
			}

			ClearRays();
		}

		private Vector2 GetRandomPosition() {
			float x = Random.Range(-mapSize.x/2, mapSize.x/2);
			float y = Random.Range(0, mapSize.y);

			return new Vector2(x, y);
		}
		
		protected IEnumerator RayRAttackEnumerator() {
			moveMode = 1;
			
			for (int i = 0; i < 3; i++) {

				int rayCount = i+2;
				
				for (int j = 0; j < rayCount; j++) {
					float x = Random.Range(-1.4f, 1.4f) * mapSize.x/2f;
					float y = Random.Range(0.5f, 1.2f) * mapSize.y;
					
					Vector2 position = new (x, y);
					float   rotation = LookAngleRad(position, player.transform.position);
					SpawnRay(0, 0, rotation, position, j==0);
				}
				
				for (int k = 0; k < 5; k++) {
					if (k % 2 == 0) {
						ShootOmnidirectionalMissile(5);
					}
					
					Vector2 position  = transform.position;
					float   direction = LookAngleDeg(position, player.transform.position) - 90f;
					ShootBigMissile(position, direction, 12.5f);
					
					yield return CacheManager.WaitForSeconds(0.75f);
				}

				ClearRays();
				
				yield return CacheManager.WaitForSeconds(1f);
			}
		}

		protected void ClearRays() {
			for (int i = SpawnedRays.Count - 1; i >= 0; i--) {
				CrystalUltRay crystalRay = SpawnedRays[i];
				UObjectPool.instance.Release(crystalRay.gameObject, 4);
			}
		}
		
		protected IEnumerator CrystalUltimateRayEnumerator() {
			moveMode                = 4;
			MapManager.instance.SetMapSize(new Vector2(mapSize.y, mapSize.y), 0.1f);
			
			shootJeonBangWuiMissile = false;

			yield return CacheManager.WaitForSeconds(3f);
			
			SpawnRay(4, 0.75f, 0,         transform.position, true);
			SpawnRay(4, 0.75f, 1 *pi *u2, transform.position);
			SpawnRay(4, 0.75f, 2 *pi *u2, transform.position);
			SpawnRay(4, 0.75f, 3 *pi *u2, transform.position);
			
			yield return CacheManager.WaitForSeconds(CrystalUltRay.rayShootTime);
			for (int i = 0; i < 10; i++) {
				if (i % 3 == 0) {
					Vector2 position  = transform.position;
					float   direction = LookAngleDeg(position, player.transform.position) - 90f;
					ShootBigMissile(position, direction, 15f);
				}
				
				ShootOmnidirectionalMissile(10, 10f);
				yield return CacheManager.WaitForSeconds(1f);
			}
			
			ClearRays();
			SpawnRay(4, -1.2f, 1 *pi *u4, transform.position, true);
			SpawnRay(4, -1.2f, 3 *pi *u4, transform.position);
			SpawnRay(4, -1.2f, 5 *pi *u4, transform.position);
			SpawnRay(4, -1.2f, 7 *pi *u4, transform.position);
			
			yield return CacheManager.WaitForSeconds(CrystalUltRay.rayShootTime);
			for (int i = 0; i < 15; i++) {
				if (i % 3 == 0) {
					Vector2 position  = transform.position;
					float   direction = LookAngleDeg(position, player.transform.position) - 90f;
					ShootBigMissile(position, direction, 15f);
				}
				
				ShootOmnidirectionalMissile(10, 12.5f);
				yield return CacheManager.WaitForSeconds(0.75f);
			}
			
			ClearRays();
			SpawnRay(4,    -0.5f, pi *u2, transform.position, true);
			SpawnRay(4.5f, +0.7f, pi *u2, transform.position);
			SpawnRay(5,    1.2f,  pi *u2, transform.position);
			SpawnRay(5.5f, -1.1f, pi *u2, transform.position);
			
			yield return CacheManager.WaitForSeconds(CrystalUltRay.rayShootTime);
			for (int i = 0; i < 7; i++) {
				ShootOmnidirectionalMissile(10);
				yield return CacheManager.WaitForSeconds(1f);
			}
			
			ClearRays();
			
			animator.Play("Death");
			PlaySFX("crystalDeath");
			
			moveMode = 5;
			
			SpawnRay(4, 0.5f, pi *u3, transform.position, true);
			SpawnRay(4, 0.5f, 2 * pi *u3, transform.position);
			SpawnRay(4, 0.5f, 3 * pi *u3, transform.position);
			SpawnRay(4, 0.5f, 4 * pi *u3, transform.position);
			SpawnRay(4, 0.5f, 5 * pi *u3, transform.position);
			SpawnRay(4, 0.5f, 0, transform.position);

			yield return CacheManager.WaitForSeconds(10f);
			
			ClearRays();
			// 움직이지 않게
			moveMode      = 7;
			transform.rotation = Quaternion.identity;
			entityStat.hp = 0;
		}
		
		protected override void OnAttackDone() {
			// state                  = EnemyState.Alert;
			attackable             = false;
			
			entityStat.moveSpeed = entityData.moveSpeed;
			
			moveMode = 0;
			t        = Random.Range(0, pi);
		}
		
		protected override void OnAttackCancel() {
			// state                  = EnemyState.Alert;
			attackable             = false;
			
			entityStat.moveSpeed = entityData.moveSpeed;
			
			moveMode = 0;
			t        = Random.Range(0, pi);
		}
		
		protected void OnJulnumgiDone() {
			MapManager.instance.SetMapSize(oldMapSize, 0.02f);
			ClearMissiles();
			
			CameraBrain.instance.mainCamera.AlignTypeX = oldCameraAlignTypeX;
			CameraBrain.instance.mainCamera.AlignTypeY = oldCameraAlignTypeY;

			OnAttackDone();
		}
		
		protected void OnJulnumgiCancel() {
			MapManager.instance.SetMapSize(oldMapSize, 0.02f);
			ClearMissiles();
			
			CameraBrain.instance.mainCamera.AlignTypeX = oldCameraAlignTypeX;
			CameraBrain.instance.mainCamera.AlignTypeY = oldCameraAlignTypeY;

			OnAttackCancel();
		}

		protected void OnRayAttackDone() {
			OnAttackDone();
		}

		protected void OnRayAttackCancel() {
			ClearRays();
			OnAttackCancel();
		}

		private StopWatch missileStopwatch = new StopWatch();
		private int       missileLaunched;
		// public override void AlertRoutine() {
		// 	if (!player) {
		// 		state = EnemyState.Wander;
		// 		return;
		// 	}
		// 	
		// 	if (attackable) {
		// 		state = EnemyState.Attack;
		// 		return;
		// 	}
		//
		// 	if (!missileStopwatch.Check(0.25f)) {
		// 		ShootBigMissile();
		// 		
		// 		missileStopwatch.Tick();
		// 	}
		// }
		
		// public override void AttackRoutine() {
		// 	if (missileStopwatch.Check(1.5f)) return;
		// 	missileStopwatch.Tick();
		// 	
		// 	if (!shootJeonBangWuiMissile) return;
		// 	ShootOmnidirectionalMissile(speed:12.5f);
		// }

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
		private const float u5 = 1f/5f;
		private const float u8 = 0.125f;
		
		public void setTargetPosition() {
			t += Time.deltaTime * moveMode switch {
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
				1 => new Vector2(playerPos.x + Mathf.Sin(2 * t), 6f + playerPos.y + 0.5f * Mathf.Sin(t)),
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

		// protected override void FixedRoutine() {
		// 	base.FixedRoutine();
		//
		// 	if (state is not (EnemyState.Alert or EnemyState.Attack)) return;
		// 	float d                             = Vector2.Distance(transform.position, targetPos);
		// 	if (d <= 0.001f) transform.position = targetPos;
		// 	rigidbody2D.linearVelocity = (targetPos-(Vector2)transform.position).normalized * (entityStat.moveSpeed * (d < 4 ? d/4 : 1));
		// }

		public override void AttackReadyRoutine() { }
		
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
			GameManager.UValueFloatVariables["crystalDead"] = new UPureFloat {number = GameManager.UValueFloatVariables["crystalDead"].value + 1};
			
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
			
			base.Death();
			
			UUI bossBar = UUI.GetUUI("CrystalBossBar");
			if (!bossBar) return;
			UUIPool.instance.Close(bossBar.gameObject);
		}
	}
}