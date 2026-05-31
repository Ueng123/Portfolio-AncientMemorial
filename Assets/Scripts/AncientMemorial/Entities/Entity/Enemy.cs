using System;
using System.Collections;
using System.Collections.Generic;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public abstract class Enemy : LivingEntity {

		public float deathTime;
		
		private EnemyState _state;
		private bool       stateChangeable;
		public EnemyState state {
			get => _state;
			set {
				if (!stateChangeable) return;
				_state = value;
			}
		}

		public void ChangeState(EnemyState newState, float fixTime) {
			if (!stateChangeable) return;
			
			state = newState;

			if (fixTime == 0) return;
			stateChangeable = false;
			new DelayedAction(fixTime, () => {
				stateChangeable = true;
			}).Execute();
		}

		// 없으면 어그로 엔티티 계산에서 제외
		public Dictionary<EntityType, int> aggroC;

		private Entity _aggroEntity;
		public Entity aggroEntity {
			get => _aggroEntity;
			set {
				if (_aggroEntity == value) return;
				
				_aggroEntity = value;
			}
		}

		protected abstract float aggroThreshold { get; }
		public Entity FindAggroEntity() {
			BufferedList<Entity> entities = livingEntities;

			float  maxAggroValue = aggroThreshold;
			Entity aggroTarget        = null;
			
			foreach (Entity entity in entities) {
				if (entity == this) continue;
				if (!aggroC.TryGetValue(entity.entityType, out int aggroCValue)) continue;
				
				float distance   = Vector2.Distance(transform.position, entity.transform.position);
				float aggroValue = aggroCValue / distance;

				if (aggroValue < maxAggroValue) continue;
				maxAggroValue  = aggroValue;
				aggroTarget    = entity;
			}

			return aggroTarget;
		}

		private WaitForSeconds waitTime1 = new (1f);
		private WaitForSeconds waitTime2 = new (1.5f);
		private WaitForSeconds waitTime3 = new (2f);
		
		public IEnumerator AggroEnumerator() {
			while (true) {
				yield return Random.Range(1, 4) switch {
					1 => waitTime1,
					2 => waitTime2,
					3 => waitTime3,
					_ => null
				};
				
				aggroEntity = FindAggroEntity();
			}
		}
		
		protected abstract IEnumerator AttackEnumerator();
		protected abstract void        OnAttackDone();
		protected abstract void        OnAttackCancel();
		
		public ExclusiveAction FindingAggro;
		public ExclusiveAction Attacking;
		
		public override void OnGet() {
			base.OnGet();
			FindingAggro = new ExclusiveAction(AggroEnumerator (), () => { },      () => { },    0   , this);
			Attacking    = new ExclusiveAction(AttackEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
		}

		public override void Initialize() {
			base.Initialize();

			currentExclusiveAction = FindingAggro;
		}

		protected abstract override void Routine();

		protected override void Death() {
			animator.SetTrigger("Death");
			new DelayedAction(deathTime + 0.2f, 
							  () => UObjectPool.instance.Release(gameObject)
							  ).Execute();
		}
	}
}