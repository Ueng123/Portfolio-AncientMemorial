using AncientMemorial.Entities;
using UengSystem.Utility;
using UnityEngine;

namespace AncientMemorial.Weapons {
	public abstract class Weapon : ScriptableObject {
		protected int       currAttackStage;
		protected int       attackStageCount;
		protected StopWatch attackWatch;
		protected Player    player;
		public    bool      isCancellable  => currAttackStage == -1 || !attackWatch.Check(GetAttackDelay(currAttackStage)/1.8f);

		public virtual bool CanAttack() => currAttackStage == -1 || !attackWatch.Check(GetAttackDelay(currAttackStage));
		
		public void Initialize() {
			currAttackStage = -1;
			attackWatch = new StopWatch();
			player          = Entity.player;
			InitializeAttacks();
		}

		public virtual void Attack() {
			if (!isCancellable) return; // 아직 쿨 안돔
			
			currAttackStage = (currAttackStage+1)%attackStageCount;
			if (!attackWatch.Check(GetAttackDelay(currAttackStage) + 1)) currAttackStage = 0; // 콤보 끊김
			
			player.currentExclusiveAction = GetAttackAction(currAttackStage);
			attackWatch.Tick();
		}
		
		protected abstract void            InitializeAttacks();
		protected abstract ExclusiveAction GetAttackAction(int attackStage);
		protected abstract float           GetAttackDelay(int attackStage);
	}
}