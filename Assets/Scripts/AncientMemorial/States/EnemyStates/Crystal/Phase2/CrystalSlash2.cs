using UengSystem.Objects;
using System;
using System.Collections.Generic;
using AncientMemorial.Cameras;
using AncientMemorial.Entities;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using UengSystem;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase2 {
	public class CrystalSlash2 : CrystalPhase2Attack {

		// 정적 프로퍼티
		private static readonly int SLASH_EFFECT = "SlashEffect".GetHash();
		private static readonly int CRYSTAL_SLASH = "crystalSlash".GetHash();

		// 인스턴스 프로퍼티
		public override float attackTime => 20f;

		private int slashCount = 7;

		private readonly int maxSlashCount = 10;
		private readonly int minSlashCount = 6;

		private          int                                          slashIndex;
		private readonly StopWatch                                    slashTimer     = new StopWatch();
		private readonly float[]                                      slashSpawnTime = new float[10];
		private readonly List<(Vector2 pos, float rot, float length)> slashData      = new (10);
		
		private readonly List<AttackArea> slashAttacks = new (10);
		
		private StopWatch missileTimer     = new StopWatch();
		private float     missileSpawnTime = 0;

		// 인스턴스 메서드
		public void GetTwoRandomNumbers(int min, int max, out int first, out int second) {
			int   count         = max - min;
			
			int[] shuffledArray = ShuffleUtil.NewShuffledArray(count);
			
			first = shuffledArray[0] + min;
			second = shuffledArray[1] + min;
		}
		
		public Vector2 GetWallPoint(int wallN) {
			Vector2 mapSize = MapManager.instance.GetTargetMapSize();
			
			return wallN switch {
				0 => new Vector2(-mapSize.x, Random.Range(1f,  mapSize.x /2f - 1)),
				1 => new Vector2(mapSize.x,  Random.Range(1f,  mapSize.x /2f - 1)),
				2 => new Vector2(Random.Range(-(mapSize.y -1), (mapSize.y -1)), 0),
				3 => new Vector2(Random.Range(-(mapSize.y -1), (mapSize.y -1)), mapSize.y),
				_ => Vector2.zero
			};
		}

		private void InitializeProperties() {
			slashData.Clear();
			slashTimer.Tick();
			
			float slashSpawnDelayMax                                          = GetDelay(0.15f);
			for (int i = 0; i < slashSpawnTime.Length; i++) slashSpawnTime[i] = Random.Range(0f, slashSpawnDelayMax);
			Array.Sort(slashSpawnTime);
			
			slashCount = Random.Range(minSlashCount, maxSlashCount+1);
			slashIndex = 0;
		}
		
		private void PrepareSlash() {
			InitializeProperties();
			
			for (int i = 0; i < slashCount; i++) {
				Vector2 mapSize = MapManager.instance.GetTargetMapSize();
			
				Vector2 startPos = GetWallPoint(2);
				Vector2 endPos   = new (startPos.x, mapSize.y);
			
				Vector2 slashPos = (startPos + endPos) / 2f;
				float   angle    = 180;
				float   length   = mapSize.y;
			
				slashData.Add((slashPos, angle, length));
			}
		}
		
		private void SlashEffect() {
			crystal.PlaySFX(CRYSTAL_SLASH);
			
			foreach ((Vector2 pos, float rot, float length) in slashData) {
				GameObject slash = UObject.Get(SLASH_EFFECT, pos, PlayEffect: false);
				slash.transform.rotation   = Quaternion.Euler(0, 0, rot);
				slash.transform.localScale = new Vector3(1f, length + 0.5f, 1f);
			}
			
			GameManager.SetTimeScale(0, 0.1f);
			CameraManager.instance.ShakeLerp(5, 3);
			CameraManager.instance.ZoomLerp(-1f);
			
			slashAttacks.Clear();
		}

		private void MainRoutine() {
			if (step == 0 && isProgress(0)) {
				PrepareSlash();
				step++;
			}

			if (step == 1 && isProgress(0.2f)) {
				SlashEffect();
				step++;
			}

			if (step == 2 && isProgress(0.25f)) {
				PrepareSlash();
				step++;
			}

			if (step == 3 && isProgress(0.45f)) {
				SlashEffect();
				step++;
			}

			if (step == 4 && isProgress(0.5f)) {
				PrepareSlash();
				step++;
			}

			if (step == 5 && isProgress(0.7f)) {
				SlashEffect();
				step++;
			}

			if (step == 6 && isProgress(0.75f)) {
				PrepareSlash();
				step++;
			}

			if (step == 7 && isProgress(0.95f)) {
				SlashEffect();
				step++;
			}
		}

		private void SlashSpawnRoutine() {
			if (slashIndex >= slashCount) return;
			
			float delay = slashSpawnTime[slashIndex];
			if (slashTimer.CheckOut(delay)) {
				(Vector2 pos, float rot, float length) = slashData[slashIndex];
				Vector2 size = new (0.5f, length + 5);
				
				slashAttacks.Add(Entity.AttackArea(crystal, 1, GetDelay(0.2f)-delay, pos, size, rot));
				slashIndex++;
			}
		}
		
		public void UpdateMissileSpawnTime() {
			missileSpawnTime = Random.Range(0.5f, 1.5f);
		}
		
		private void MissileRoutine() {
			if (missileTimer.CheckIn(missileSpawnTime)) return;
			
			Vector2 mapSize = MapManager.instance.GetMapSize();

			Vector2 missilePos   = new Vector2(Random.Range(-mapSize.x/2f+0.5f, mapSize.x/2f-0.5f), Random.Range(mapSize.y/2f, mapSize.y*5f/6f));
			
			if (Random.Range(0, 1) == 1) {
				ShootMissile(missilePos, Random.Range(0, 360), 13.5f, 0, 3f);
			}
			else {
				ShootBigMissile(missilePos, Random.Range(0, 360), 13.5f, 0, 3f);
			}

			missileTimer.Tick();
			UpdateMissileSpawnTime();
		}

		// 오버라이드 메서드
		public override void OnRoutine() {
			MainRoutine();
			SlashSpawnRoutine();
			MissileRoutine();
			
			if (isProgress(1)) {
				crystal.entityState = GetState();
			}
		}

		public override void OnExit() {
			base.OnExit();

			foreach (AttackArea area in slashAttacks) {
				area.Cancel();
			}
		}
	}
}
