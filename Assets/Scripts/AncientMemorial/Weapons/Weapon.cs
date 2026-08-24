using AncientMemorial.Entities;
using UengSystem.States;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues.UFloats;
// using UengSystem.VisualScripting.UValues.UFloats;
using UnityEngine;

namespace AncientMemorial.Weapons {
	public abstract class Weapon : ScriptableObject {
		public RuntimeAnimatorController PlayerAnimatorController;
		public Sprite             WeaponSprite;

		private readonly int SKILL_COOLDOWN_TIME = "SkillCooldownTime".GetHash();
		
		public static int selectedWeaponID = 0;
		
		protected int currPrimaryAttackStage;
		protected int primaryAttackStageCount;

		protected int currSecondaryAttackStage;
		protected int secondaryAttackStageCount;
		
		protected StopWatch primaryAttackWatch;
		protected StopWatch secondaryAttackWatch;
		
		protected Player player;
		
		public UState primaryAttack => GetPrimaryAttackAction(currPrimaryAttackStage);
		public float  primaryDelay  => GetPrimaryAttackDelay (currPrimaryAttackStage);
		
		public UState secondaryAttack => GetSecondaryAttackAction(currSecondaryAttackStage);
		public float  secondaryDelay  => GetSecondaryAttackDelay (currSecondaryAttackStage);
		
		public virtual bool isAttacking => player.state == primaryAttack || player.state == secondaryAttack;
		
		public virtual bool CanPrimaryAttack()   => !primaryAttackWatch  .Check(primaryDelay);
		public virtual bool CanSecondaryAttack() => !secondaryAttackWatch.Check(secondaryDelay);
		
		public void Initialize() {
			currPrimaryAttackStage   = -1;
			currSecondaryAttackStage = -1;
			
			primaryAttackWatch = new StopWatch();
			secondaryAttackWatch = new StopWatch();
			
			player                                    = Entity.player;
			player.weaponSpriteRenderer.sprite        = WeaponSprite;
			player.animator.runtimeAnimatorController = PlayerAnimatorController;
			
			InitializeAttacks();
		}

		public virtual void Uninitialize() { }

		public virtual void PrimaryAttack() {
			bool isFirstAttack = currPrimaryAttackStage == -1;
			bool isTimeOver    = !primaryAttackWatch.Check(GetPrimaryAttackDelay(currPrimaryAttackStage) + 2);
			
			if (!isFirstAttack&&isTimeOver) OnPrimaryAttackComboEnd(); // 콤보 끊김
			else currPrimaryAttackStage = (currPrimaryAttackStage + 1) % primaryAttackStageCount;
			
			player.state = primaryAttack;
			primaryAttackWatch.Tick();
		}

		public virtual void OnPrimaryAttackComboEnd() {
			currPrimaryAttackStage = 0;
		}
		
		public virtual void SecondaryAttack() {
			bool isFirstAttack = currSecondaryAttackStage == -1;
			bool isTimeOver    = !secondaryAttackWatch.Check(GetSecondaryAttackDelay(currSecondaryAttackStage) + 10);
			
			if (!isFirstAttack&&isTimeOver) OnSecondaryAttackComboEnd(); // 콤보 끊김
			else currSecondaryAttackStage = (currSecondaryAttackStage + 1) % secondaryAttackStageCount;

			player.state = GetSecondaryAttackAction(currSecondaryAttackStage);
			UPureFloat.SetValue(SKILL_COOLDOWN_TIME, Time.time + GetSecondaryAttackDelay(currSecondaryAttackStage));
			
			secondaryAttackWatch.Tick();
		}
		
		public virtual void OnSecondaryAttackComboEnd() {
			currSecondaryAttackStage = 0;
		}
		
		protected abstract void            InitializeAttacks();
		
		protected abstract UState GetPrimaryAttackAction(int attackStage);
		protected abstract UState GetSecondaryAttackAction(int attackStage);
		
		protected abstract float GetPrimaryAttackDelay(int attackStage);
		protected abstract float GetSecondaryAttackDelay(int attackStage);
	}
}