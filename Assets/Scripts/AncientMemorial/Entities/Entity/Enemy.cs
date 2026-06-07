using System;
using System.Collections;
using System.Collections.Generic;
using AncientMemorial.Projectiles;
using UengSystem.Events;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public abstract class Enemy : Entity {
		public float markYPos;
		
		private EnemyState _state;
		private bool       stateChangeable = true;
		public EnemyState  state {
			get => _state;
			set {
				if (!stateChangeable) return;
				if (_state == value) return;
				if (value == EnemyState.Attack) Attack();
				_state = value;
			}
		}

		// 없으면 어그로 엔티티 계산에서 제외
		public Dictionary<EntityType, float> aggroC1 = new ();

		private Entity _aggroEntity;
		public Entity aggroEntity {
			get => _aggroEntity;
			set {
				if (_aggroEntity == value) return;

				if (value) AggroFounded();
				else AggroHidden();

				// 처음 적 발견시 공격 쿨다운 다시 굴리기
				if (!_aggroEntity) {
					attackable = false;
				}
				
				_aggroEntity = value;
			}
		}

		private IEnumerator StunEnumerator(float duration) {
			OnStunStartHandler();
			yield return new WaitForSeconds(duration);
		}

		private GameObject stunEffect;
		public void OnStunStartHandler() {
			OnStunStart();
			stunEffect = UObjectPool.instance.Get("StunEffect", transform.position + Vector3.up * markYPos);
			stunEffect.transform.SetParent(transform);
		}

		public void OnStunEndHandler() {
			OnStunEnd();
			UObjectPool.instance.Release(stunEffect);
			stunEffect = null;
		}
		public abstract void OnStunStart();
		public abstract void OnStunEnd();
		
		public ExclusiveAction Stun(float duration) {
			return new ExclusiveAction(StunEnumerator(duration), OnStunEndHandler, OnStunEndHandler, 0, this);
		}

		public void AggroFounded() {
			UObjectPool.instance.Get("ExcalmationMark", (Vector2)transform.position + Vector2.up * markYPos);
		}

		public void AggroHidden() {
			UObjectPool.instance.Get("QuestionMark", (Vector2)transform.position + Vector2.up * markYPos);
		}
		
		public Entity FindAggroEntity() {
			float  maxAggroValue = entityData.aggroThreshold;
			Entity aggroTarget        = null;
			
			foreach (Entity entity in entities) {
				if (entity == this) continue;
				
				if (!aggroC1.TryGetValue(entity.entityType, out float aggroCValue1)) continue;
				
				float aggroCValue = aggroCValue1/ (Vector2.Distance(entity.transform.position, transform.position)+0.5f);
				
				float distance   = Mathf.Min(0.01f, Vector2.Distance(transform.position, entity.transform.position));
				float aggroValue = aggroCValue / (distance + (entity == aggroEntity ? 0 : 1));

				if (aggroValue < maxAggroValue) continue;
				maxAggroValue  = aggroValue;
				aggroTarget    = entity;
			}

			return aggroTarget;
		}

		public override void Initialize() {
			base.Initialize();

			foreach ((string key, float value) in entityData.aggroCoefficient) {
				if (Enum.TryParse(key, true, out EntityType eT)) {
					aggroC1[eT] = value;
				}
			}
			
			currentExclusiveAction = FindingAggro;
			state                  = EnemyState.Wander;
		}
		
		private WaitForSeconds waitTime1 = new (0.2f);
		private WaitForSeconds waitTime2 = new (0.3f);
		private WaitForSeconds waitTime3 = new (0.4f);
		
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

		private   DelayedAction _turnAttackable;
		private   bool          _attackable;
		protected bool attackable {
			get => _attackable;
			set {
				if (!value) {
					_turnAttackable?.Cancel();
					_turnAttackable = new DelayedAction(entityData.attackCooldown * Random.Range(0.85f, 1.15f), () => _attackable = true);
					_turnAttackable.Execute();
				}
				_attackable = value;
			}
		}
		
		protected abstract IEnumerator   AttackEnumerator();
		protected abstract void          OnAttackDone();
		protected abstract void          OnAttackCancel();
		public void Attack() {
			if (!attackable) return;
			attackable = false;
			
			currentExclusiveAction = Attacking;
		}
		
		public ExclusiveAction FindingAggro => new (AggroEnumerator(),  () => { },      () => { },    0, this);
		public ExclusiveAction Attacking    => new (AttackEnumerator(), OnAttackCancel, OnAttackDone, 0, this);
		
		public abstract void WanderRoutine();
		public abstract void AlertRoutine();
		public abstract void AttackReadyRoutine();

		protected override void Routine() {
			switch (state) {
				case EnemyState.Wander:
					WanderRoutine();
					break;

				case EnemyState.Alert:
					AlertRoutine();
					break;
				
				case EnemyState.AttackReady:
					AttackReadyRoutine();
					break;
				
				case EnemyState.Attack:
				case EnemyState.None:
				case EnemyState.Stun:
					break;
				
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}