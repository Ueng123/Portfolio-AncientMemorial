using System;
using System.Collections;
using AncientMemorial.Cameras;
using AncientMemorial.Interactions;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
using AncientMemorial.Weapons;
using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UObjects;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.Managers;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.Settings;
using UengSystem.UI;
using UengSystem.UI.USliders;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;
using UnityEngineInternal;
using Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public class Player : Entity {
		
		[Header("Hand")]
		public  GameObject hand;
		public  Animator   crossbowAnimator;
		public  GameObject crossbowGuideObject;
		private Animator   crossbowGuideAnimator;
		public  Weapon     weapon;
		
		private float handRotOffset;
		private float targetHandRotOffset;
		private float handRotOffsetSpeed;
		
		private bool dashing;

		private Vector2 arrowAimDir;

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
					SendEvent(EventType.Interact_Untarget, 2, new EventValueData<Interaction>(_targetInteraction));
					_targetInteraction = null;
				}
				else {
					SendEvent(EventType.Interact_Target  , 3, new EventValueData<Interaction>(value));
					_targetInteraction = value;
				}
			}
		}
		
		// ANIMATOR //
		private static readonly int  MOVING    = Animator.StringToHash("moving");
		private static readonly int  BACKWARD  = Animator.StringToHash("backward");
		private static readonly int  SHOOTF    = Animator.StringToHash("shootF");
		private static readonly int  SHOOTB    = Animator.StringToHash("shootB");
		private static readonly int  SHOOTABLE = Animator.StringToHash("shootable");
		private                 bool isMoving;
		
		// METHOD //

		private bool CanMove() {
			if (!weapon.isCancellable) return false;
			if (dashing) return false;

			return true;
		}
		
		private bool CanJump() {
			if (dashing) return false;
			if (!isGround) return false;
			bool inKoyoteTime = unGroundedTimer.Check(0.2f);
			if (!weapon.isCancellable) return false;

			return isGround || inKoyoteTime;

			return true;
		}
		
		private bool CanDash() {
			if (!isMoving) return false;
			if (dashing) return false;
			if (dashTimer != null && dashTimer.Check(dashTime * 2f)) return false;

			return true;
		}

		private int moveSign;

		private void Move() {
			float moveDir = InputManager.inputData[InputActionType.Move].valueF;
			moveSign = (int)Mathf.Sign(moveDir);

			Vector2 moveVec = new(moveDir * entityStat.moveSpeed, rigidbody2D.linearVelocity.y);

			isMoving = (moveDir != 0);

			if (isMoving) currentExclusiveAction = null;

			if (isGround) {
				if (moveDir == 0 && rigidbody2D.linearVelocityX == 0) return;

				float step = 6 * entityStat.moveSpeed * DeltaTime;
				int velocitySign = rigidbody2D.linearVelocityX == 0
									   ? (int)moveDir
									   : (int)Mathf.Sign(rigidbody2D.linearVelocityX);
				int stepCE = velocitySign * (moveDir == 0 ? -1 : moveDir * velocitySign > 0 ? 1 : -2);

				Debug.DrawLine(transform.position                           - Vector3.up * 0.25f,
							   transform.position + Vector3.right * moveDir - Vector3.up * 0.25f, Color.red);
				Debug.DrawLine(transform.position + Vector3.up    * 0.25f,
							   transform.position + Vector3.right * velocitySign + Vector3.up * 0.25f, Color.green);
				Debug.DrawLine(transform.position,
							   transform.position + Vector3.right * velocitySign *
							   (moveDir == 0 ? -2 : moveDir * velocitySign > 0 ? 1 : -3), Color.yellow);

				float newVX = Mathf.Clamp(rigidbody2D.linearVelocityX + step * stepCE, -entityStat.moveSpeed,
										  entityStat.moveSpeed);

				rigidbody2D.linearVelocityX = moveDir == 0 && newVX is >= -0.1f and <= 0.1f ? 0 : newVX;
			}
			else {
				rigidbody2D.AddForce(new Vector2(moveVec.x*25, 0)*Time.deltaTime, ForceMode2D.Force);
			}
		}

		private       StopWatch dashTimer;
		private const float     dashTime      = 0.4f;
		private void Dash() {
			SendEvent(EventType.Entity_Behaviour_Dash, 3);
			
			currentExclusiveAction = null;
			
			dashTimer ??= new StopWatch();
			dashTimer.Tick();
			
			// dashing   =   true;
			// ForceInvincibleTrue(true);
			//
			// float oldVelocityX = rigidbody2D.linearVelocity.x;
			// float oldGravityScale = rigidbody2D.gravityScale;
			// rigidbody2D.linearVelocity = Vector2.right * (entityStat.moveSpeed * moveSign * 1.75f);
			// rigidbody2D.gravityScale = 0;
			// SpriteRenderer sr = UObjectPool.instance.Get("DashEffect", transform.position).GetComponent<SpriteRenderer>();
			// sr.flipX = spriteRenderer.flipX;
			// sr.sprite = spriteRenderer.sprite;
			
			// Debug.Log("[DASH] dash started");
			
			StartCoroutine(DashEnumerator());
			
			// for (int i = 1; i <= n-1; i++) {
			// 	new DelayedAction(dashLength*i/n, () => {
			// 		SpriteRenderer sr = UObjectPool.instance.Get("DashEffect", transform.position).GetComponent<SpriteRenderer>();
			// 		sr.flipX  = spriteRenderer.flipX;
			// 		sr.sprite = spriteRenderer.sprite;
			// 	}).Execute();
			// }
			//
			// new DelayedAction(dashLength, () => {
			// 	dashing                    = false;
			// 	rigidbody2D.linearVelocity /= 2;
			// 	rigidbody2D.gravityScale   = oldGravityScale;
			// 	
			// 	SpriteRenderer sr = UObjectPool.instance.Get("DashEffect", transform.position).GetComponent<SpriteRenderer>();
			// 	sr.flipX  = spriteRenderer.flipX;
			// 	sr.sprite = spriteRenderer.sprite;
			// }).Execute();
			//
			// new DelayedAction(dashLength+0.3f, ()=> {
			// 	if (dashing) return;
			// 	ForceInvincibleTrue(false);
			// }).Execute();
		}

		IEnumerator DashEnumerator(int dashEffectCount = 10) {
			dashing   =   true;
			
			float oldGravityScale = rigidbody2D.gravityScale;
			rigidbody2D.gravityScale   = 0;
			rigidbody2D.linearVelocity = Vector2.right * (entityStat.moveSpeed * moveSign * 1.75f);
			
			ForceInvincibleTrue(true);
			
			for (int i = 0; i <= dashEffectCount; i++) {
				SpriteRenderer sr = UObjectPool.instance.Get("DashEffect", transform.position).GetComponent<SpriteRenderer>();
				sr.flipX  = spriteRenderer.flipX;
				sr.sprite = spriteRenderer.sprite;
				yield return new WaitForSeconds(dashTime / dashEffectCount);
			}

			dashing                    = false;
			
			rigidbody2D.gravityScale   = oldGravityScale;
			rigidbody2D.linearVelocity = Vector2.right * (entityStat.moveSpeed * moveSign * 0.5f);
			
			yield return new WaitForSeconds(0.3f);
			ForceInvincibleTrue(false);
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

		private int        debugMode    = 1;
		private GameObject debugInfoObject;
		private bool       _inDebugMode = false;
		private bool inDebugMode {
			get => _inDebugMode;
			set {
				if (_inDebugMode == value) return;
				_inDebugMode = value;
				string v = value ? "enabled" : "disabled";
				InfoUUI.instance.AddInfoMessage($"[DEBUG : debugMode {v}]");
				if (debugInfoObject) {
					UObjectPool.instance.Release(debugInfoObject);
				}
				else {
					debugInfoObject = UObjectPool.instance.Get("DebugInfo", transform.position);
				}
			}
		}
		
		private void GetInput() {
			if (InputManager.inputData[InputActionType.Jump].pressType is InputPressType.Down or InputPressType.Hold && CanJump()) {
				Jump();
			}
			
			if (InputManager.inputData[InputActionType.Dash].pressType == InputPressType.Down && CanDash())
				Dash();
			
			if (InputManager.inputData[InputActionType.MouseLClick].pressType is InputPressType.Down or InputPressType.Hold && weapon.CanAttack())
				PrimaryAttack();
			
			if (InputManager.inputData[InputActionType.MouseRClick].pressType is InputPressType.Down or InputPressType.Hold)
				SecondaryAttack();
			
			if (InputManager.inputData[InputActionType.Move].valueF != 0.0f)
				SendEvent(EventType.Entity_Behaviour_Move, 3, new EventValueData<float>(InputManager.inputData[InputActionType.Move].valueF));
			
			if (InputManager.inputData[InputActionType.Interact].pressType == InputPressType.Down)
				SendEvent(EventType.Interact_Start, 3, new EventValueData<InteractTryInfo>(new InteractTryInfo() {
					objectToInteract = targetInteraction,
					byInput          = true
				}));
			
			if (InputManager.inputData[InputActionType.Interact].pressType == InputPressType.Up)
				SendEvent(EventType.Interact_Cancel, 2, new EventValueData<InteractTryInfo>(new InteractTryInfo() {
					objectToInteract = targetInteraction,
					byInput          = true
				}));

			if (InputManager.inputData[InputActionType.DebugMode1].pressType == InputPressType.Down) {
				inDebugMode = false;
				debugMode   = debugMode == 1 ? 2 : 1;
			}
			if (InputManager.inputData[InputActionType.DebugMode2].pressType == InputPressType.Down) {
				inDebugMode = false;
				debugMode   = debugMode == 2 ? 3 : 1;
			}
			if (InputManager.inputData[InputActionType.DebugMode3].pressType == InputPressType.Down) {
				inDebugMode = false;
				debugMode   = debugMode == 3 ? 4 : 1;
			}
			if (InputManager.inputData[InputActionType.DebugMode4].pressType == InputPressType.Down) {
				inDebugMode = false;
				debugMode   = debugMode == 4 ? 5 : 1;
			}
			if (InputManager.inputData[InputActionType.DebugMode5].pressType == InputPressType.Down) {
				inDebugMode = false;
				debugMode   = debugMode == 5 ? 6 : 1;
			}
			if (InputManager.inputData[InputActionType.DebugMode6].pressType == InputPressType.Down) {
				if (debugMode == 6) inDebugMode = true;
				debugMode = 1;
			}

			if (InputManager.inputData[InputActionType.Debug_DamageUp].pressType == InputPressType.Up && inDebugMode) { 
				entityStat.attackDamage += 1;
				InfoUUI.instance.AddInfoMessage($"[DEBUG : attackDamage is now {entityStat.attackDamage}]");
			}
			if (InputManager.inputData[InputActionType.Debug_DamageDown].pressType == InputPressType.Up && inDebugMode) {
				entityStat.attackDamage -= 1;
				InfoUUI.instance.AddInfoMessage($"[DEBUG : attackDamage is now {entityStat.attackDamage}]");
			}

			if (InputManager.inputData[InputActionType.Debug_HealthUp].pressType == InputPressType.Up && inDebugMode) {
				entityStat.hp += 1;
				InfoUUI.instance.AddInfoMessage($"[DEBUG : hp is now {entityStat.hp}]");
			}
			if (InputManager.inputData[InputActionType.Debug_HealthDown].pressType == InputPressType.Up && inDebugMode) {
				entityStat.hp -= 1;
				InfoUUI.instance.AddInfoMessage($"[DEBUG : hp is now {entityStat.hp}]");
			}
			
			if (InputManager.inputData[InputActionType.Debug_MaxHealthUp].pressType == InputPressType.Up && inDebugMode) {
				entityData.hp += 1;
				InfoUUI.instance.AddInfoMessage($"[DEBUG : max hp is now {entityData.hp}]");
			}
			if (InputManager.inputData[InputActionType.Debug_MaxHealthDown].pressType == InputPressType.Up && inDebugMode) {
				entityData.hp -= 1;
				InfoUUI.instance.AddInfoMessage($"[DEBUG : max hp is now {entityData.hp}]");
			}
			
			if (InputManager.inputData[InputActionType.Debug_MoveSpeedUp].pressType == InputPressType.Up && inDebugMode) {
				entityStat.moveSpeed += 1;
				InfoUUI.instance.AddInfoMessage($"[DEBUG : moveSpeed is now {entityStat.moveSpeed}]");
			}
			if (InputManager.inputData[InputActionType.Debug_MoveSpeedDown].pressType == InputPressType.Up && inDebugMode) {
				entityStat.moveSpeed -= 1;
				InfoUUI.instance.AddInfoMessage($"[DEBUG : moveSpeed is now {entityStat.moveSpeed}]");
			}
			
			if (InputManager.inputData[InputActionType.Debug_AttackSpeedUp].pressType == InputPressType.Up && inDebugMode) {
				entityStat.attackSpeed += 0.1f;
				InfoUUI.instance.AddInfoMessage($"[DEBUG : attackSpeed is now {entityStat.attackSpeed}]");
			}
			if (InputManager.inputData[InputActionType.Debug_AttackSpeedDown].pressType == InputPressType.Up && inDebugMode) {
				entityStat.attackSpeed -= 0.1f;
				InfoUUI.instance.AddInfoMessage($"[DEBUG : attackSpeed is now {entityStat.attackSpeed}]");
			}
			
			if (InputManager.inputData[InputActionType.Debug_DamageToBoss].pressType == InputPressType.Up && inDebugMode) {
				AttackArea(1, 0, InputManager.inputData[InputActionType.MousePosition].valueV, Vector2.one, ignoreInvincible:true);
				InfoUUI.instance.AddInfoMessage("[DEBUG : AttackArea(IgnoreInvincible) spawned]");
			}
		}

		// EVENT BEHAVIOUR //
		
		private void Jump() {
			SendEvent(EventType.Entity_Behaviour_Jump, 3);
			
			currentExclusiveAction = null;
			
			rigidbody2D.linearVelocityX =
				(int)Mathf.Sign(rigidbody2D.linearVelocityX) == moveSign
					? rigidbody2D.linearVelocityX
					: rigidbody2D.linearVelocityX * -0.5f;
			
			rigidbody2D.linearVelocityY = entityStat.jumpPower;
			
			StartCoroutine(JumpUtil());
		}

		private IEnumerator JumpUtil() {
			float jumpUtilLength = 0.25f;

			while (jumpUtilLength > 0f) {
				if (dashing) break;
				if (InputManager.inputData[InputActionType.Jump].pressType == InputPressType.Up) break;
				
				rigidbody2D.linearVelocityY = entityStat.jumpPower;
				
				jumpUtilLength -= Time.deltaTime;
				yield return null;
			}
		}
		
		private void PrimaryAttack() {
			if (dashing) return;
			SendEvent(EventType.Entity_Behaviour_Primary, 3);
			
			weapon.Attack();
		}
		
		// R CLICK
		private bool shootable      = true;
		private int  mapEntityLayer;
		private void SecondaryAttack() {
			
			if (dashing) return;
			if (!shootable) return;
			SendEvent(EventType.Entity_Behaviour_Secondary, 3);
			
			mapEntityLayer = mapEntityLayer == 0 ? LayerMask.GetMask("Entity", "Map") : mapEntityLayer;
			
			shootable = false;
			new DelayedAction(1.5f, () => shootable = true).Execute();
			
			SetArm();
			crossbowAnimator.SetTrigger(spriteRenderer.flipX?SHOOTB:SHOOTF);
                
			Vector2 handPos   = transform.position;
			Vector2 direction = new (Mathf.Cos(radH), Mathf.Sin(radH));

			float dist = MapManager.instance.GetMapSize().magnitude + 1f;
                
			RaycastHit2D hit  = Physics2D.Raycast(handPos, direction, dist, mapEntityLayer);
			if (hit.point == Vector2.zero) {
				Debug.Log("HOW??????????????????????");
				return;
			}
				
			Vector2 hitPos    = hit.point;
			Entity  hitEntity = hit.collider.GetComponent<Entity>();
                
			if (hitEntity) { SendAttackEvent(hitEntity, 1.5f * Random.Range(0.9f, 1.1f), false); }
				
			GameObject   arrowTail         = UObjectPool.instance.Get("ArrowTail", Vector2.zero);
			LineRenderer arrowTailRenderer = arrowTail.GetComponent<LineRenderer>();
			arrowTailRenderer.positionCount = 2;
			arrowTailRenderer.SetPosition(0, handPos);
			arrowTailRenderer.SetPosition(1, hitPos);
    
			GameObject obj = UObjectPool.instance.Get("ArrowDebris", hitPos);
                
			if (hitEntity) {
				GameObject eff = UObjectPool.instance.Get("ArrowHitEffect", hitPos);
				eff.transform.rotation = hand.transform.rotation;
			}
                
			if (hit.collider) {
				if (hitEntity) hit.collider.GetComponent<Entity>().debrisAttached.Add(obj.GetComponent<Debris>());
				obj.transform.SetParent(hit.collider.transform, true);
			}
                
			new DelayedAction(10, () => UObjectPool.instance.Release(obj), () => { }, obj.GetComponent<UObject>())
				.Execute();
                
			obj.transform.rotation = hand.transform.rotation;
			CameraBrain.instance.ShakeLerp(0.5f, 7.5f);
			CameraBrain.instance.ZoomLerp(-0.2f);
		}

		private float radH;
		private void SetArm() {
			
			Vector2 mousePos = InputManager.inputData[InputActionType.MousePosition].valueV;
			Vector2 dM       = mousePos - (Vector2)transform.position;
			Vector2 handPos  = new (0, 0);
			Vector2 dH       = mousePos - (handPos + (Vector2)transform.position);

			radH = Mathf.Atan2(dH.y, dH.x);
			float degH   = radH * Mathf.Rad2Deg;
			float offset = handRotOffset;

			handRotOffset = (Mathf.Abs(handRotOffset) <= 0.001f)?0:Mathf.Lerp(handRotOffset, targetHandRotOffset, handRotOffsetSpeed * DeltaTime);

			hand.transform.localPosition = handPos;
			hand.transform.localRotation = Quaternion.Euler(0, 0, degH + offset*Mathf.Sign(dM.x));
		}

		private void SetVisual() {
			animator.SetBool(MOVING,   isMoving);
			animator.SetBool(BACKWARD,InputManager.inputData[InputActionType.MousePosition].valueV.x < transform.position.x);
			animator.speed = entityStat.moveSpeed/3f;
				
			crossbowGuideObject.transform.position = Vector2.Lerp(
				(Vector2)transform.position+new Vector2(0.532f*(spriteRenderer.flipX ? 1 : -1), 0.157f),
				crossbowGuideObject.transform.position,
				DeltaTime*0.01f);
			crossbowGuideAnimator.SetBool(SHOOTABLE, shootable);
		}

		public Vector2 GetExpectPos(float timeToReach) {
			return (Vector2)transform.position + rigidbody2D.linearVelocity*timeToReach;
		}
		
		// OVERRIDING //
		
		public override void Stop() {
			base.Stop();
			dashTimer.Stop();
		}

		public override void Resume() {
			base.Resume();
			dashTimer.Resume();
		}

		public override EntityData GetData() {
			EntityData data = base.GetData();
			data.name = Setting.data.playerName;

			return data;
		}

		public override void OnGet() {
			if (player) throw new InvalidOperationException("ALREADY PLAYER EXIST WHY U TRYING TO MAKE SAME PEOPLE AGAIN 🥀🥀");
			player = this;
			base.OnGet();
		}

		protected override void OnRelease() {
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

		protected override void OnGrounded() { }

		public override void Initialize() {
			hand.SetActive(true);
			crossbowGuideAnimator = crossbowGuideObject.GetComponent<Animator>();
			weapon?.Initialize();
			
			base.Initialize();
		}
		
		public override void HitEffect(Entity attacker, float damage, Vector2? pushDir = null) {
			Time.timeScale =  0.05f;
			new DelayedAction(0.3f, ()=>Time.timeScale = 1f, () => { }).Execute(true);
			CameraBrain.instance.ShakeLerp(3f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
			CameraBrain.instance.ZoomLerp(-0.3f);
			
			UUI damageUI = UUIObjectPool.instance.Open("DamageUI", GameManager.instance.mainWorldCanvas).GetComponent<UUI>();
			damageUI.GetComponent<RectTransform>().anchoredPosition =
				(transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0))*80;
			UTextAction   textAction = damageUI.GetAction<UTextAction>("DamageDisplay");
			UTextAction   textSAction = damageUI.GetAction<UTextAction>("DamageDisplayShadow");
			new DelayedAction(0.5f, () => UUIObjectPool.instance.Close(damageUI.gameObject)).Execute();
			
			textAction.text = textSAction.text = new UPureString {Text = $"{Mathf.Floor(damage*100)/100f}"};
			textAction.Initialize(damageUI);
			textSAction.Initialize(damageUI);
		}

		protected override void OnHit(Entity attacker, float damage, Vector2? pushDir) {
			entityStat.hp          -= damage;
			currentExclusiveAction =  null;
		}

		protected override void OnHit(Projectile attacker, float damage, Vector2? pushDir) {
			entityStat.hp          -= damage;
			currentExclusiveAction =  null;
		}

		protected override void OnHit(float damage, Vector2? pushDir) {
			entityStat.hp          -= damage;
			currentExclusiveAction =  null;
		}

		protected override float GetRealDamage(float rawDamage) {
			return rawDamage;
		}
		
		public override bool isAttackTarget(Entity entity) {
			return entity != this && !((Enemy)entity).Friendly;
		}
		
		protected override void EarlyRoutine() {
			GetInput();
		}
		
		protected override void Routine() {
			SetTargetInteract();
			if (CanMove()) Move();
			SetVisual();
		}

		private bool dead = false;
		protected override void Death() {
			if (dead) return;
			dead = true;
			
			UUIObjectPool.instance.Open("DieUI", GameManager.instance.mainScreenCanvas);

			base.Death();
		}
	}
}