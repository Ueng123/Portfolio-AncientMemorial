using System;
using System.Collections.Generic;
using System.Linq;
using AncientMemorial.Events;
using AncientMemorial.Events.EventDatas;
using AncientMemorial.Inputs;
using AncientMemorial.Interactions;
using UnityEngine;
using Event = AncientMemorial.Events.Event;
using EventType = AncientMemorial.Events.EventType;

namespace AncientMemorial.Entities {
	public class Player : Entity {

		[Header("Hand")]
		public  GameObject           hand;
		private SpriteRenderer       handSprite;
		
		private float handRotOffset;
		private float handRotOffsetSpeed;

		[Header("Interaction")]
		public  float       maxInteractableDistance;
		private Interaction _targetInteraction;
		private Interaction targetInteraction {
			get => _targetInteraction;
			set {
				if (_targetInteraction == value) return;
				
				if (_targetInteraction) SendEvent(EventType.Interact_Untarget, new EventValueData<Interaction>(_targetInteraction));
				if (value)              SendEvent(EventType.Interact_Target  , new EventValueData<Interaction>(value));
				
				_targetInteraction = value;
			}
		}

		// ANIMATOR //
		private static readonly int  CROSSBOW = Animator.StringToHash("crossbow");
		private                 bool holdingCrossbow = false;
		
		private static readonly int  MOVING = Animator.StringToHash("moving");
		private                 bool isMoving = false;
		
		private static readonly int  FALLING = Animator.StringToHash("falling");
		
		// METHOD //
		private void Move() {
			if (stopped) return;
			
			float moveDir = InputManager.inputData[InputActionType.Move].valueF;
			
			Vector2 moveVec = new (moveDir * entityStat.MoveSpeed, rigidbody2D.linearVelocity.y);

			// var portfolio = "FUCKING SHIT";
			
			isMoving = (moveVec != Vector2.zero);
			
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
				SendEvent(EventType.Entity_Behaviour_Jump);
			
			if (InputManager.inputData[InputActionType.MouseLClick].pressType == InputPressType.Down)
				SendEvent(EventType.Entity_Behaviour_Attack);
			
			if (InputManager.inputData[InputActionType.Move].valueF != 0.0f)
				SendEvent(EventType.Entity_Behaviour_Move, new EventValueData<float>(InputManager.inputData[InputActionType.Move].valueF));
			
			// InteractTryInfo 필요없을뜻
			if (InputManager.inputData[InputActionType.Interact].pressType == InputPressType.Down)
				SendEvent(EventType.Interact_Start, new EventValueData<InteractTryInfo>(new InteractTryInfo() {
					objectToInteract = targetInteraction,
					byInput          = true
				}));
			
			if (InputManager.inputData[InputActionType.Interact].pressType == InputPressType.Up)
				SendEvent(EventType.Interact_Cancel, new EventValueData<InteractTryInfo>(new InteractTryInfo() {
					objectToInteract = targetInteraction,
					byInput          = true
				}));
			
			if (InputManager.inputData[InputActionType.HoldWeapon].pressType = InputPressType.Down) {
				ToggleCrossbow();
		}

		public void SetHandOffset(float offset, float speed) {
				// 구현
		}

		// EVENT BEHAVIOUR //
		
		private void Jump() {
			if (stopped) return;
			
			rigidbody2D.AddForce(Vector2.up * entityStat.JumpPower, ForceMode2D.Impulse);
		}
		
		private void Attack() {
			if (stopped) return;
			
			SetHandOffset(20, 0.1f);
		}

		private void ToggleCrossbow() {
			holdingCrossbow = !holdingCrossbow;
		}

		private void Interact() {
			// 일단만들긴했는데뭔가여기서할게없달까약간이런게있어야겠다생각은했는데막상만들고나니까이게하는짓이없는데미래지향적인지성의보유자인본
			// 인은일단남겨놓고나중에여길수정하는방향으로띵킹을함으로써이제남들과는차별점이있다는것을알수있다능*찡긋*
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

			hand.transform.localPosition = handPos;
			hand.transform.localRotation = Quaternion.Euler(0, 0, degH + offset);
			hand.SetActive(holdingCrossbow);
		}

		private void SetVisual() {
			if (stopped) return;

			animator.SetBool(CROSSBOW, holdingCrossbow);
			animator.SetBool(MOVING,   isMoving);
			animator.SetBool(FALLING,  !isGround);
			animator.speed = entityStat.MoveSpeed/3;
			
			spriteRenderer.flipX = InputManager.inputData[InputActionType.MousePosition].valueV.x < transform.position.x;
			handSprite.flipY     = InputManager.inputData[InputActionType.MousePosition].valueV.x < transform.position.x;
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
			handSprite = hand.GetComponent<SpriteRenderer>();
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

		public override void OnEvent(Event e) {
			base.OnEvent(e);

			switch (e.type) {
				case EventType.Entity_Behaviour_Jump:
					AddProcessToFixedUpdate(Jump);
					break;
				
				case EventType.Entity_Behaviour_Attack:
					Attack();
					break;
				
				case EventType.Interact_Start:
					Interact();
					break;
			}
		}
		
		protected override void Death() { }
	}
}