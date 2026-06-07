using System;
using AncientMemorial.Interactions;
using AncientMemorial.Projectiles;
using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Managers;
using UengSystem.ObjectPool; 
using UengSystem.Utility;
using UnityEngine;
using UnityEngineInternal;
using Event = UengSystem.Events.Event;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public class Player : Entity {
		
		[Header("Hand")]
		public  GameObject     hand;
		public  GameObject     handW;
		private SpriteRenderer handSprite;
		private SpriteRenderer handWSprite;
		
		private float handRotOffset;
		private float handRotOffsetSpeed;

		[Header("Interaction")]
		public  float maxInteractableDistance;
		private bool targetInteractChangeable = true;
		private Interaction   _targetInteraction;
		private Interaction targetInteraction {
			get => _targetInteraction;
			set {
				if (_targetInteraction == value) return;
				if (!targetInteractChangeable) return;

				targetInteractChangeable = false;
				new DelayedAction(0.1f, () => { targetInteractChangeable = true; }).Execute();

				if (_targetInteraction) {
					SendEvent(UengSystem.Events.EventType.Interact_Untarget, 2, new EventValueData<Interaction>(_targetInteraction));
					_targetInteraction = null;
				}
				else {
					SendEvent(UengSystem.Events.EventType.Interact_Target  , 3, new EventValueData<Interaction>(value));
					_targetInteraction = value;
				}
			}
		}

		// ANIMATOR //
		private static readonly int  CROSSBOW = Animator.StringToHash("crossbow");
		private                 bool holdingCrossbow = true;
		
		private static readonly int  MOVING = Animator.StringToHash("moving");
		private                 bool isMoving = false;
		
		private static readonly int  FALLING = Animator.StringToHash("falling");
		
		// METHOD //
		private void Move() {
			if (stopped) return;
			
			float moveDir = InputManager.inputData[InputActionType.Move].valueF;
			
			Vector2 moveVec = new (moveDir * entityStat.moveSpeed, rigidbody2D.linearVelocity.y);
			
			isMoving = (moveDir != 0);
			
			if (isGround) {
				rigidbody2D.linearVelocity = moveVec;
			}
			else {
				rigidbody2D.AddForce(new Vector2(moveVec.x*25, 0)*Time.deltaTime, ForceMode2D.Force);
			}
		}

		private void SetTargetInteract() {
			float minDistSQR = float.MaxValue;
			Interaction minInteraction = null;
			
			Vector2 mPos = InputManager.inputData[InputActionType.MousePosition].valueV;
			Vector2 pPos = transform.position;
			
			foreach (Interaction interaction in Interaction.InteractableInteractions) {
				Vector2 iPos = interaction.transform.position;
				
				float mDistSQR    = (iPos - mPos).sqrMagnitude;
				float pDistSQR    = (iPos - pPos).sqrMagnitude;
				float cMinDistSQR = (mDistSQR < pDistSQR) ? mDistSQR : pDistSQR;

				if (!(cMinDistSQR < minDistSQR)) continue;
				minDistSQR     = cMinDistSQR;
				minInteraction = interaction;
			}

			targetInteraction = minInteraction;
		}
		
		private void GetInput() {
			if (InputManager.inputData[InputActionType.Jump].pressType == InputPressType.Down
				&& (isGround || unGroundedTimer.Check(1.5f))
				&& jumpCount!=0) {
				
				jumpCount-=1;
				SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Jump, 3);
			}
			
			if (InputManager.inputData[InputActionType.MouseLClick].pressType == InputPressType.Down)
				SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Primary, 3);
			
			if (InputManager.inputData[InputActionType.MouseRClick].pressType == InputPressType.Down)
				SendEvent(UengSystem.Events.EventType.Entity_Behaviour_ChargeStart, 3);
			
			if (InputManager.inputData[InputActionType.MouseRClick].pressType == InputPressType.Up)
				SendEvent(UengSystem.Events.EventType.Entity_Behaviour_ChargeEnd, 3);
			
			if (InputManager.inputData[InputActionType.Move].valueF != 0.0f)
				SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Move, 3, new EventValueData<float>(InputManager.inputData[InputActionType.Move].valueF));
			
			// InteractTryInfo 필요없을뜻
			if (InputManager.inputData[InputActionType.Interact].pressType == InputPressType.Down)
				SendEvent(UengSystem.Events.EventType.Interact_Start, 3, new EventValueData<InteractTryInfo>(new InteractTryInfo() {
					objectToInteract = targetInteraction,
					byInput          = true
				}));
			
			if (InputManager.inputData[InputActionType.Interact].pressType == InputPressType.Up)
				SendEvent(UengSystem.Events.EventType.Interact_Cancel, 2, new EventValueData<InteractTryInfo>(new InteractTryInfo() {
					objectToInteract = targetInteraction,
					byInput          = true
				}));
		}

		public void SetHandOffset(float offset, float speed = 0) {
			handRotOffset = offset;
			handRotOffsetSpeed = (speed==0)?handRotOffsetSpeed:speed;
		}

		// EVENT BEHAVIOUR //
		
		private void Jump() {
			if (stopped) return;
			
			rigidbody2D.AddForce(Vector2.up * entityStat.jumpPower, ForceMode2D.Impulse);
		}

		[Header("Attack Tasks")]
		public Task primaryAttackTask;
		public Task ChargeAttackTask;
		
		// L CLICK
		private float         primaryAttackCooldownTime => entityStat.attackSpeed/2.5f;
		private bool          primaryAttackAble     = true;
		private DelayedAction PrimaryAttackCoolDown => new (primaryAttackCooldownTime, () => primaryAttackAble = true);
		private void PrimaryAttack() {
			if (stopped) return;
			if (ChargeAttack is { Executing: true } || chargeComplete) return;
			if (!primaryAttackAble) return;

			primaryAttackAble = false;
			PrimaryAttackCoolDown.Execute();

			primaryAttackTask.Execute(this);
			
			SetHandOffset(20, 5f);
		}
		
		// R CLICK
		private bool  chargeComplete = false;
		private bool  chargeAble = true;
		private float ChargeTime     => 1.25f / ((entityStat.attackSpeed-1)*0.3f+1);
		private float chargeProgress => (ChargeAttack!=null)?(ChargeAttack.Executing ? ChargeAttack.GetProgress() : (chargeComplete ? 1 : 0)):0;
		private DelayedAction ChargeAttack = null;
		private void ChargeAttackAction() {
			chargeComplete = true;

			GameObject obj = UObjectPool.instance.Get("FullChargeEffect", hand.transform.position);
			obj.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));
		}
		
		private void ChargeStart() {
			if (!chargeAble) return;
			if (stopped) return;
			chargeComplete = false;
			
			ChargeAttack = new DelayedAction(ChargeTime, ChargeAttackAction);
			ChargeAttack.Execute();
		}
		
		private void ChargeEnd() {
			if (!chargeAble) return;
			if (stopped) return;
			if (chargeProgress <= 0.2f) {
				
				ChargeAttack.Cancel();
				chargeComplete = false;
				new DelayedAction(0.2f, () => chargeAble = true).Execute();
				
				return;
			}

			SetHandOffset(30*chargeProgress, 5f);

			GameManager.UValueFloatVariables["chargeProgress"] = new UPureNumber {number = chargeProgress};
			ChargeAttackTask.Execute(this);
			
			ChargeAttack.Cancel();
			chargeComplete = false;
			
			chargeAble     = false;
			new DelayedAction(0.2f, () => chargeAble = true).Execute();
		}

		private void SetArm() {
			if (stopped) return;
			
			Vector2 mousePos = InputManager.inputData[InputActionType.MousePosition].valueV;
			Vector2 dM       = mousePos - (Vector2)transform.position;
			Vector2 handPos  = new (-0.2809999f*Mathf.Sign(dM.x), 0.141f);
			Vector2 dH       = mousePos - (handPos + (Vector2)transform.position);
			
			float degH = Mathf.Atan2(dH.y, dH.x) * Mathf.Rad2Deg;
			
			float offset   = handRotOffset;

			handRotOffset = (Mathf.Abs(handRotOffset) <= 0.001f)?0:Mathf.Lerp(handRotOffset, 0, handRotOffsetSpeed * Time.deltaTime);

			float randomDeg = Random.Range(0, 360f);
			
			hand.transform.localPosition = handPos + new Vector2(Mathf.Cos(randomDeg), Mathf.Sin(randomDeg)) *
										   (chargeProgress * 0.05f);
			hand.transform.localRotation = Quaternion.Euler(0, 0, degH + offset*Mathf.Sign(dM.x));
			hand.SetActive(holdingCrossbow);
		}

		private void SetVisual() {
			if (stopped) return;

			animator.SetBool(CROSSBOW, holdingCrossbow);
			animator.SetBool(MOVING,   isMoving);
			animator.SetBool(FALLING,  !isGround);
			animator.speed = entityStat.moveSpeed/3;
			
			spriteRenderer.flipX = InputManager.inputData[InputActionType.MousePosition].valueV.x < transform.position.x;
			handSprite.flipY = handWSprite.flipY = InputManager.inputData[InputActionType.MousePosition].valueV.x < transform.position.x;

			const float n      = 250;
			float       aValue = (Mathf.Pow(n, chargeProgress) - 1) / (n - 1);
			handWSprite.color = new Color(1, 1, 1, aValue);
		}
		
		// OVERRIDING //

		public override void OnGet() {
			if (player) throw new InvalidOperationException("ALREADY PLAYER EXIST WHY U TRYING TO MAKE SAME PEOPLE AGAIN 🥀🥀");
			player = this;
			base.OnGet();
		}

		public override void OnRelease() {
			player = null;
			base.OnRelease();
		}
		
		public override void Get(float     time) {
			hand.SetActive(false);
			
			base.Get(time);
		}

		public override void Release(float time) {
			hand.SetActive(false);
			
			base.Release(time);
		}
		
		public override void Initialize() {
			hand.SetActive(true);
			handW.SetActive(true);
			
			handSprite = hand.GetComponent<SpriteRenderer>();
			handWSprite = handW.GetComponent<SpriteRenderer>();
			
			team = Team.Player;
			
			base.Initialize();
		}

		protected override void EarlyRoutine() {
			GetInput();
		}
		
		public override void OnHit(Entity attacker, float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
		}
		
		public override void OnHit(Projectile attacker, float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
		}
		
		public override void OnHit(float damage, Vector2? pushDir) {
			entityStat.hp -= damage;
		}

		protected override void Routine() {
			SetTargetInteract();
			
			Move();
			
			SetArm();
			SetVisual();
		}

		public override void OnEvent(Event e) {
			base.OnEvent(e);
			switch (e.type) {
				case UengSystem.Events.EventType.Entity_Behaviour_Jump:
					AddProcessToFixedUpdate(Jump);
					break;
				
				case UengSystem.Events.EventType.Entity_Behaviour_Primary:
					PrimaryAttack();
					break;
				
				case UengSystem.Events.EventType.Entity_Behaviour_ChargeStart:
					ChargeStart();
					break;
				
				case UengSystem.Events.EventType.Entity_Behaviour_ChargeEnd:
					ChargeEnd();
					break;
			}
		}

		protected override void Death() {
			// 연출 하고 게임 오버 넣기
		}
	}
}