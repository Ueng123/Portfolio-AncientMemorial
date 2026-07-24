using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AncientMemorial.Objects;
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
				_state = value;
				
				if (value == EnemyState.Attack) Attack();
			}
		}

		// 없으면 어그로 엔티티 계산에서 제외
		public Dictionary<EntityType, float> aggroC = new ();

		private Entity _aggroEntity;
		public Entity aggroEntity {
			get => _aggroEntity;
			set {
				if (_aggroEntity == value) return;

				if (!value) AggroHidden();

				// 처음 적 발견시 공격 쿨다운 다시 굴리기
				if (!_aggroEntity && value) {
					attackable = false;
					AggroFounded();
				}

				string valueName = value ? value.name : "null";
				string aggroEntityName = _aggroEntity ? _aggroEntity.name : "null";
				Debug.Log($"[AggroEntity] AggroEntity Changed : {aggroEntityName} -> {valueName}");
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
			Entity aggroTarget   = null;
			
			foreach (Entity entity in entities) {
				if (entity == this) continue;
				
				if (!aggroC.TryGetValue(entity.entityType, out float aggroCValue)) continue;
				
				float distance   = Mathf.Max(0.01f, Vector2.Distance(transform.position, entity.transform.position));
				float aggroValue = (aggroCValue + (entity==aggroEntity?1:0)) / distance;

				if (aggroValue < maxAggroValue) continue;
				maxAggroValue  = aggroValue;
				aggroTarget    = entity;
			}

			return aggroTarget;
		}

		public bool isTargettable(EntityType entity) {
			return aggroC.ContainsKey(entity);
		}

		public override void Initialize() {
			base.Initialize();

			foreach ((string key, float value) in entityData.aggroCoefficient) {
				if (Enum.TryParse(key, true, out EntityType eT)) {
					aggroC[eT] = value;
				}
			}
			
			currentExclusiveAction = FindingAggro;
			state                  = EnemyState.Wander;
		}

		public override void Uninitialize() {
			base.Uninitialize();
			
			_aggroEntity = null;
		}

		private WaitForSeconds waitTime1;
		private WaitForSeconds waitTime2;
		private WaitForSeconds waitTime3;
		
		public IEnumerator AggroEnumerator() {
			waitTime1 = new WaitForSeconds(0.2f);
			waitTime2 = new WaitForSeconds(0.3f);
			waitTime3 = new WaitForSeconds(0.4f);
			
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

		protected StopWatch attackableTimer = new ();
		private   float     attackableCoeff;
		protected bool attackable {
			get => !attackableTimer.Check(entityData.attackCooldown * attackableCoeff);
			set {
				if (value) return;
				attackableCoeff = Random.Range(1f, 1.1f);
				attackableTimer.Tick();
			}
		}
		
		protected abstract IEnumerator   AttackEnumerator();
		protected abstract void          OnAttackDone();
		protected abstract void          OnAttackCancel();
		public virtual void Attack() {
			if (!attackable) return;
			attackable = false;
			
			currentExclusiveAction = Attacking;
		}
		
		public ExclusiveAction FindingAggro => new (AggroEnumerator(),  () => { },      () => { },    0, this);
		public ExclusiveAction Attacking    => new (AttackEnumerator(), () => {
			attackable = false;
			OnAttackCancel();
		}, () => {
			attackable = false;
			OnAttackDone();
		}, 0, this);

		public abstract void WanderRoutine();
		public abstract void AlertRoutine();
		public abstract void AttackReadyRoutine();
		public abstract void AttackRoutine();
		public abstract void StunRoutine();
		
		public override bool isAttackTarget(Entity entity) {
			return entity != this && aggroC.Keys.ToList().Contains(entity.entityType);
		}

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
					AttackRoutine();
					break;
				
				case EnemyState.Stun:
					StunRoutine();
					break;
				
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}