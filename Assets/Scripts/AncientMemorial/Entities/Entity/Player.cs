using System;
using AncientMemorial.Interactions;
using AncientMemorial.Weapons;
using UengSystem.Events.EventDatas;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;
using UnityEngineInternal;
using Events_Event = UengSystem.Events.Event;
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
		public  float       maxInteractableDistance;
		private Interaction _targetInteraction;
		private Interaction targetInteraction {
			get => _targetInteraction;
			set {
				if (_targetInteraction == value) return;
				
				if (_targetInteraction) SendEvent(UengSystem.Events.EventType.Interact_Untarget, new EventValueData<Interaction>(_targetInteraction));
				if (value)              SendEvent(UengSystem.Events.EventType.Interact_Target  , new EventValueData<Interaction>(value));
				
				_targetInteraction = value;
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
			
			Vector2 moveVec = new (moveDir * entityStat.MoveSpeed, rigidbody2D.linearVelocity.y);
			
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
				
				if (cMinDistSQR < minDistSQR) {
					minDistSQR     = cMinDistSQR;
					minInteraction = interaction;
				}
			}

			targetInteraction = minInteraction;
		}

		private void GetInput() {
			if (InputManager.inputData[InputActionType.Jump].pressType == InputPressType.Down && isGround)
				SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Jump);
			
			if (InputManager.inputData[InputActionType.MouseLClick].pressType == InputPressType.Down)
				SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Primary);
			
			if (InputManager.inputData[InputActionType.MouseRClick].pressType == InputPressType.Down)
				SendEvent(UengSystem.Events.EventType.Entity_Behaviour_ChargeStart);
			
			if (InputManager.inputData[InputActionType.MouseRClick].pressType == InputPressType.Up)
				SendEvent(UengSystem.Events.EventType.Entity_Behaviour_ChargeEnd);
			
			if (InputManager.inputData[InputActionType.Move].valueF != 0.0f)
				SendEvent(UengSystem.Events.EventType.Entity_Behaviour_Move, new EventValueData<float>(InputManager.inputData[InputActionType.Move].valueF));
			
			// InteractTryInfo 필요없을뜻
			if (InputManager.inputData[InputActionType.Interact].pressType == InputPressType.Down)
				SendEvent(UengSystem.Events.EventType.Interact_Start, new EventValueData<InteractTryInfo>(new InteractTryInfo() {
					objectToInteract = targetInteraction,
					byInput          = true
				}));
			
			if (InputManager.inputData[InputActionType.Interact].pressType == InputPressType.Up)
				SendEvent(UengSystem.Events.EventType.Interact_Cancel, new EventValueData<InteractTryInfo>(new InteractTryInfo() {
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
			
			rigidbody2D.AddForce(Vector2.up * entityStat.JumpPower, ForceMode2D.Impulse);
		}

		private float primaryAttackCooldownTime => entityStat.AttackSpeed/2.5f;
		private bool  primaryAttackAble     = true;
		private DelayedAction PrimaryAttackCoolDown => new (primaryAttackCooldownTime, () => primaryAttackAble = true);
		
		private void PrimaryAttack() {
			if (stopped) return;
			if (!primaryAttackAble) return;

			primaryAttackAble = false;
			PrimaryAttackCoolDown.Execute();
			
			SetHandOffset(20, 5f);
		}

		private bool  chargeComplete = false;
		private float ChargeTime     => 2.5f / ((entityStat.AttackSpeed-1)*0.3f+1);
		private float chargeProgress => (ChargeAttack!=null)?(ChargeAttack.Executing ? ChargeAttack.GetProgress() : (chargeComplete ? 1 : 0)):0;
		private DelayedAction ChargeAttack = null;

		private void ChargeAttackAction() {
			chargeComplete = true;

			GameObject obj = UObjectPool.instance.Get("FullChargeEffect", hand.transform.position);
			obj.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));
		}
		
		private void ChargeStart() {
			if (stopped) return;
			chargeComplete = false;
			
			ChargeAttack = new DelayedAction(ChargeTime, ChargeAttackAction);
			ChargeAttack.Execute();
		}
		
		private void ChargeEnd() {
			if (stopped) { return; }
			
			SetHandOffset(30*chargeProgress, 5f);
			
			ChargeAttack.Cancel();
			chargeComplete = false;
		}
		
		private void Interact() {
			// 일단만들긴했는데뭔가여기서할게없달까약간이런게있어야겠다생각은했는데막상만들고나니까이게하는짓이없음
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
			animator.speed = entityStat.MoveSpeed/3;
			
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
		}

		public override void OnRelease() {
			player = null;
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
			
			team       = Team.Player;
			
			base.Initialize();
		}

		protected override void EarlyRoutine() {
			GetInput();
		}

		protected override void Routine() {
			SetTargetInteract();
			
			Move();
			
			SetArm();
			SetVisual();
		}

		public override void OnEvent(Events_Event e) {
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
				
				case UengSystem.Events.EventType.Interact_Start:
					Interact();
					break;
			}
		}
		
		protected override void Death() { }
	}
}