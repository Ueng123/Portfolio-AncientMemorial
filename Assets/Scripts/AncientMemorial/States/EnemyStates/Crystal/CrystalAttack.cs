using AncientMemorial.Entities;
using AncientMemorial.Entities.Enemies;
using AncientMemorial.Projectiles;
using UengSystem.ObjectPool;
using UnityEngine;

namespace UengSystem.States.EnemyStates.Crystal {
	public abstract class CrystalAttack : CrystalState, IAttackState {
		public abstract float attackTime  { get; }
		public          float attackSpeed => crystal.stat.attackSpeed;
		protected       int   step;
		
		public float GetDelay(float   percent) => percent * attackTime / attackSpeed;
		public bool  isProgress(float percent) => stateTimer.CheckOut(GetDelay(percent));

		protected override EnemyState GetState() {
			stateMachine.AttackWatchTick();
			return base.GetState();
		}
		
		public override void OnEnter() {
			step = 0;
		}
		
		public override void OnEarlyRoutine() { }

		public override void OnExit() { }
	}
}
