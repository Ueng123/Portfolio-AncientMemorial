using System;
using AncientMemorial.Cameras;
using AncientMemorial.Map;
using AncientMemorial.States.EnemyStates;
using AncientMemorial.States.EnemyStates.Crystal.Phase3;
using AncientMemorial.States.EnemyStates.Crystal.Phase3.FinalAttack;
using UengSystem;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UAction;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.UI.UUIs;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues.UFloats;
using UengSystem.VisualScripting.UValues.UStrings;
using UnityEngine;

using Random = UnityEngine.Random;

namespace AncientMemorial.Entities.Enemies {
	public class CrystalPhase3 : CrystalBoss {
		private int CRYSTAL_ENERGY_DEAD = "crystalEnergyDead".GetHash();
		
		public Animator   rotatingAnimator;
		private int       attackPhase = 0;
		private int       remainingCrystalEnergy;
		private bool      gimmickActive;

		private EnemyState         semiHugeCross;
		private EnemyState         semiHugeSweep;
		private EnemyState         rayRandom;
		private EnemyState         rayCross;
		private CrystalSpawnEnergy spawnEnergy;
		private EnemyState         deathAttack;
		private EnemyState         final;

		protected void ShowCrystalPhase3DamageUI(float damage) {
			if (damage == 0) return;
			
			bool isHeal = damage < 0;
			if (isHeal) { 
				ShowDamageUI(damage);
				return;
			}
			
			UUI.Get("CrystalPhase3DamageUI", GameManager.instance.mainWorldCanvas, Configure: DamageUi => {
			DamageUi.rectTransform.anchoredPosition = (transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0))*80;
			UTextAction textAction  = DamageUi.GetAction<UTextAction>("DamageDisplay");
			UTextAction textSAction = DamageUi.GetAction<UTextAction>("DamageDisplayShadow");
			
			textAction.text = textSAction.text = new UPureString {pureValue = "!!!!!!"};
			});
		}
		
		public override void OnHit(Entity attacker, float damage, Vector2? pushDir = null) {
			if (attacker != this) return;
			
			ShowCrystalPhase3DamageUI(damage);

			if (stat.HP > 0) {
				PlaySFX("crystalHit2");
				
				if (attacker != player) return;
				GameManager.SetTimeScale(0.05f, 0.2f);
				CameraBrain.instance.ShakeLerp(2.5f, 10);
				CameraBrain.instance.ZoomLerp(-0.3f);
			}
		}
		
		

		protected override float GetRealDamage(float rawDamage) {
			return 1;
		}

		public override void OnFirstGet() {
			base.OnFirstGet();

			InitializeCrystalStateMachine(new CrystalPhase3Idle());
			spawnEnergy   = new CrystalSpawnEnergy().Init(stateMachine).To<CrystalSpawnEnergy>(); 
			semiHugeSweep = new CrystalSemiHugeSweep().Init(stateMachine);                        
			semiHugeCross = new CrystalSemiHugeCross().Init(stateMachine);                        
			rayCross      = new CrystalRayCross().Init(stateMachine);                             
			deathAttack   = new CrystalDeathAttack().Init(stateMachine);                          
			final         = new CrystalFinalA().Init(stateMachine);
		}
		
		protected override Entity GetTargetEntity() => player;
		
		public override void Attack() {
			bool isFinalPhase = (int)stat.HP == 1;
			// bool isFinalPhase = true;
			if (isFinalPhase) {
				entityState = final;
				return;
			}

			bool rayPhase    = stat.HP <= 3;
			bool isRayAttack = attackPhase is 1 or 4;
			if (!rayPhase && isRayAttack) attackPhase++; 
			
			entityState = attackPhase switch {
				0 => spawnEnergy.Setup(2+ Mathf.CeilToInt(Mathf.Abs(data.HP - stat.HP)/2)),
				1 => rayCross,
				2 => semiHugeCross,
				3 => semiHugeSweep,
				4 => rayCross,
				5 => semiHugeCross,
				6 => semiHugeSweep,
				7 => deathAttack,
				_ => null
			};
			
			
			attackPhase++;
			attackPhase%=8;
		}

		public void SpawnCrystalEnergy(Vector2 pos, bool PlayEffect) {
			GameObject crystalEnergyObject = UObject.Get("CrystalEnergy", pos, PlayEffect);
			CrystalEnergy crystalEnergy = crystalEnergyObject.GetComponent<CrystalEnergy>();

			crystalEnergy.Category = "crystalEnergy";
		}

		public void StartGimmick(int energyCount) {
			if (gimmickActive) return;
			UPureFloat.SetValue(CRYSTAL_ENERGY_DEAD, 0);
			
			remainingCrystalEnergy = energyCount;
			gimmickActive = true;
		}

		public void StopGimmick() {
			if (!gimmickActive) return;
			gimmickActive = false;

			foreach (UObject obj in GetUObjects("crystalEnergy")) {
				CrystalEnergy crystalEnergy = (CrystalEnergy)obj;
				crystalEnergy.stat.HP = 0;
			}
		}

		private void OnGimmickDone() {
			StopGimmick();
			attackPhase = 0;
			
			Stun(1);
			SendAttackEvent(this, this, 1, true);
		}

		private void GimmickRoutine() {
			if (!gimmickActive) return;
			if (UPureFloat.GetValue(CRYSTAL_ENERGY_DEAD) < remainingCrystalEnergy) return;
			OnGimmickDone();
		}

		public override void Initialize() {
			base.Initialize();
			Invincible(true);
			
			remainingCrystalEnergy = 0;
			gimmickActive = false;
			
			// 생성 이펙트 좌표 이동
			GameManager.instance.Crystal.transform.SetParent(transform);
			GameManager.instance.Crystal.transform.localPosition = Vector3.zero;
			
			InfoUUI.instance.AddInfoMessage("크리스탈의 보호막으로 인해 <color=#ff5a5a>평범한 공격</color>으로는 피해를 입힐 수 없습니다.");
		}

		protected override void Routine() {
			base.Routine();
			GimmickRoutine();
		}

		private readonly DelayedAction shakeAfterTimescale = new (1.5f, () => { CameraBrain.instance.ShakeLerp(75, 10f); }, () => { });
		protected override void Death() {
			Vector2 mapSize = MapManager.instance.GetMapSize();
			
			GameObject eff1 = UObject.Get("SlashEffect", transform.position, PlayEffect: false);
			eff1.transform.rotation   = Quaternion.Euler(0, 0, Random.Range(40, 50));
			eff1.transform.localScale = new Vector3(mapSize.x*1.1f, mapSize.y*1.2f, 1f);
			GameObject eff2 = UObject.Get("SlashEffect", transform.position, PlayEffect: false);
			eff2.transform.rotation   = Quaternion.Euler(0, 0, -Random.Range(40, 50));
			eff2.transform.localScale = new Vector3(mapSize.x*1.1f, mapSize.y*1.2f, 1f);
			
			GameManager.SetTimeScale(0.1f, 1.5f);
			shakeAfterTimescale.ExecuteDA(true);
			CameraBrain.instance.ZoomLerp(-5f);

			UObject.Get("BrokenCrystal", new Vector2(transform.position.x, 0.75f), PlayEffect: false);
			
			base.Death();
		}
	}
}
