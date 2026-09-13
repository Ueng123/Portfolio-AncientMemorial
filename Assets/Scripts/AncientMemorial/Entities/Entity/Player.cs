using System;
using System.Collections;
using AncientMemorial.Cameras;
using AncientMemorial.Interactions;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using AncientMemorial.States.PlayerStates;
using AncientMemorial.Waves;
using AncientMemorial.Weapons;
using UengSystem;
using UengSystem.Audio;
using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.SaveDatas.SettingDatas;
using UengSystem.UAction;
using UengSystem.UI;
using UengSystem.UI.UUIs;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues.UFloats;
// using UengSystem.VisualScripting.UValues.UFloats;
using UnityEngine;
using DebugManager = UengSystem.UDebug.DebugManager;
using EventType = UengSystem.Events.EventType;
using Random = UnityEngine.Random;

namespace AncientMemorial.Entities {
	public class Player : Entity {

		// 정적 프로퍼티
		private static readonly int AfterImagePrefabId = "AfterImage".GetHash();
		private static readonly int PlayerUIPrefabId = "PlayerUI".GetHash();
		private static readonly int HealEffectPrefabId = "HealEffect".GetHash();
		private static readonly int PlayerBottleOpenClipId = "playerBottleOpen".GetHash();
		private static readonly int PlayerLandClipId = "playerLand".GetHash();
		private static readonly int PlayerHitClipId = "playerHit".GetHash();
		
		
		private static readonly int  BACKWARD  = Animator.StringToHash("backward");

		// 인스턴스 프로퍼티
		private int HEALTH_POTION_COUNT = "HealthPotionCount".GetHash();
		
		[Header("Hand")]
		public GameObject hand;
		public Animator handAnimator;
		public SpriteRenderer weaponSpriteRenderer;
		public Weapon weapon;

		private UUI playerUI;
		private long PlayerUiLife;
		private bool WeaponInitialized;
		
		private bool _lookingLeft;
		
		public bool lookingLeft {
			get => _lookingLeft;
			set {
				if (_lookingLeft == value) return;
				_lookingLeft = value;
				player.animator.SetBool(BACKWARD, value);
			}
		}

		

		private Vector2 arrowAimDir;

		[Header("Interaction")]
		public  float maxInteractableDistance;
		private Interaction _targetInteraction;
		private Interaction targetInteraction {
			get => _targetInteraction;
			set {
				if (_targetInteraction == value) return;
				
				if (_targetInteraction) {
					SendEvent(EventType.Interact_Untarget, EventPriority.Remove, new ValueData<Interaction>(_targetInteraction));
				}
				
				if (value) {
					SendEvent(EventType.Interact_Target  , EventPriority.Add, new ValueData<Interaction>(value));
				}
			
				_targetInteraction = value;
			}
		}
		
		
		private StopWatch healTimer = new ();
		
		private int  moveSign;

		private StopWatch targetInteractSync     = new StopWatch();
		private float     targetInteractSyncTime = 0.1f;
		
		public bool getInput = true; 

		private float radH;

		// 인스턴스 메서드
		private bool CanJump() {
			if (!isGround) return false;
			if (entityState == PlayerState.jump) return false;
			
			return true;
		}
		
		private bool CanDash() {
			if (entityState == PlayerState.dash) return false;
			if (PlayerState.dash.stateTimer.CheckIn(Dash.dashTime * 4f)) return false;

			return true;
		}

		private bool CanHeal() {
			if (UPureFloat.GetValue(HEALTH_POTION_COUNT) <= 0) return false;
			if (weapon.isAttacking) return false;
			if (healTimer.CheckIn(0.5f)) return false;
			
			return true;
		}

		public void SpawnAfterImage() {
			SpriteRenderer sr = UObject.Get(AfterImagePrefabId, transform.position, PlayEffect: false).GetComponent<SpriteRenderer>();
			sr.flipX  = spriteRenderer.flipX;
			sr.sprite = spriteRenderer.sprite;
		}
		
		private void PrimaryAttack() {
			SendEvent(EventType.Entity_Player_PrimaryAttack, EventPriority.Action);
			
			weapon.PrimaryAttack();
		}

		// R CLICK
		
		private void SecondaryAttack() {
			SendEvent(EventType.Entity_Player_SecondaryAttack, EventPriority.Action);
			
			weapon.SecondaryAttack();
		}

		private void Heal() {
			healTimer.Tick();
			SendAttackEvent(this, -3, true);
			UPureFloat.AddValue(HEALTH_POTION_COUNT, -1);
			Get(HealEffectPrefabId, transform.position, PlayEffect: false);
			PlaySFX(PlayerBottleOpenClipId);
		}
		private void SetTargetInteract() {
			if (targetInteractSync.CheckIn(targetInteractSyncTime)) return;
			targetInteractSync.Tick();
			
			float minDistSQR = float.MaxValue;
			Interaction minInteraction = null;
			
			Vector2 mPos = InputManager.mousePosition;
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
		    if (!getInput) return;
		    
		    if (InputManager.GetInput(ActionType.Jump, InputState.Down | InputState.Hold) && CanJump()) entityState = PlayerState.jump;
		    
		    if (InputManager.GetInput(ActionType.Dash, InputState.Down) && CanDash()) entityState = PlayerState.dash;
		    
		    if (InputManager.GetInput(ActionType.MouseLClick, InputState.Down | InputState.Hold) && weapon.CanPrimaryAttack()) PrimaryAttack();
		    
		    if (InputManager.GetInput(ActionType.MouseRClick, InputState.Down) && weapon.CanSecondaryAttack()) SecondaryAttack();
		    
		    if (InputManager.GetInput(ActionType.Interact, InputState.Down))
		       SendEvent(EventType.Interact_Start, EventPriority.Start, new ValueData<Interaction>(targetInteraction));
		    
		    if (InputManager.GetInput(ActionType.Interact, InputState.Up))
		       SendEvent(EventType.Interact_Cancel, EventPriority.Cancel, new ValueData<Interaction>(targetInteraction));

		    if (InputManager.GetInput(ActionType.Heal, InputState.Down) && CanHeal()) Heal();

		    if (!DebugManager.instance.debugMode) return;
		    
		    if (InputManager.GetInput(ActionType.Debug_DamageUp, InputState.Up)) { 
		       stat.attackDamage += 1;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : attackDamage is now {stat.attackDamage}]");
		    }
		    if (InputManager.GetInput(ActionType.Debug_DamageDown, InputState.Up)) {
		       stat.attackDamage -= 1;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : attackDamage is now {stat.attackDamage}]");
		    }

		    if (InputManager.GetInput(ActionType.Debug_HealthUp, InputState.Up)) {
		       SendAttackEvent(this, -5, true);
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : hp is now {stat.HP}]");
		    }
		    if (InputManager.GetInput(ActionType.Debug_HealthDown, InputState.Up)) {
		       SendAttackEvent(this, 5, true);
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : hp is now {stat.HP}]");
		    }
		    
		    if (InputManager.GetInput(ActionType.Debug_MaxHealthUp, InputState.Up)) {
		       data.HP += 5;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : max hp is now {data.HP}]");
		    }
		    if (InputManager.GetInput(ActionType.Debug_MaxHealthDown, InputState.Up)) {
		       data.HP -= 5;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : max hp is now {data.HP}]");
		    }
		    
		    if (InputManager.GetInput(ActionType.Debug_MoveSpeedUp, InputState.Up)) {
		       stat.moveSpeed += 0.5f;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : moveSpeed is now {stat.moveSpeed}]");
		    }
		    if (InputManager.GetInput(ActionType.Debug_MoveSpeedDown, InputState.Up)) {
		       stat.moveSpeed -= 0.5f;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : moveSpeed is now {stat.moveSpeed}]");
		    }
		    
		    if (InputManager.GetInput(ActionType.Debug_AttackSpeedUp, InputState.Up)) {
		       stat.attackSpeed += 0.5f;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : attackSpeed is now {stat.attackSpeed}]");
		    }
		    if (InputManager.GetInput(ActionType.Debug_AttackSpeedDown, InputState.Up)) {
		       stat.attackSpeed -= 0.5f;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : attackSpeed is now {stat.attackSpeed}]");
		    }
		    
		    if (InputManager.GetInput(ActionType.Debug_Damage, InputState.Up)) {
		       AttackArea(1, 0, InputManager.mousePosition, Vector2.one, ignoreInvincible:true);
		       InfoUUI.instance.AddInfoMessage("[DEBUG : AttackArea(IgnoreInvincible) spawned]");
		    }
		    
		    if (InputManager.GetInput(ActionType.Debug_AddHealPotion, InputState.Up)) {
				UPureFloat.AddValue(HEALTH_POTION_COUNT, 1);
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : now you have {UPureFloat.GetValue(HEALTH_POTION_COUNT)} potions");
		    }
		}
		public void SetArm() {
			Vector2 mousePos = InputManager.mousePosition;
			Vector2 dM       = mousePos - (Vector2)transform.position;
			Vector2 handPos  = new (0, 0);
			Vector2 dH       = mousePos - (handPos + (Vector2)transform.position);

			radH = Mathf.Atan2(dH.y, dH.x);
			float degH   = radH * Mathf.Rad2Deg;
			
			hand.transform.localPosition = handPos;
			hand.transform.localRotation = Quaternion.Euler(0, 0, degH + Mathf.Sign(dM.x));
		}

		// 오버라이드 메서드
		protected override void OpenEntityUI() { }
		
		

		protected override AttackArea AttackArea(float damageMult, float delay, Vector2 hitboxPos, Vector2 hitboxSize, float angle = 0, int maxTargetNum = -1, bool  awareLerpX = true, bool awareLerpY = false, bool ignoreInvincible = false) {
			AttackArea attackArea = base.AttackArea(damageMult, delay, hitboxPos, hitboxSize, angle, maxTargetNum,
													  awareLerpX, awareLerpY, ignoreInvincible);
			attackArea.targetColor         = new Color(0.3536254f, 0.945098f, 0.2431372f, 0.6156863f);
			attackArea.InitializeColor(new Color(0.1843137f, 0.509804f, 0.2422917f, 0));
			
			return attackArea;
		}

		protected override EntityData GetData() {
			EntityData data = base.GetData();
			data.name = Setting.GetString(SettingType.playerName);

			return data;
		}

		public override void OnGet() {
			hand.SetActive(false);
			if (player) throw new InvalidOperationException("ALREADY PLAYER EXIST WHY U TRYING TO MAKE SAME PEOPLE AGAIN 🥀🥀");
			player = this;
			base.OnGet();
		}

		protected override void OnRelease() {
			hand.SetActive(false);
			entityState = null;
			if (WeaponInitialized) { WeaponInitialized = false; weapon?.Uninitialize(); }
			if (playerUI && playerUI.lifeNumber == PlayerUiLife) playerUI.Release();
			playerUI = null;
			base.OnRelease();
			if (player == this) player = null;
		}

		protected override void OnGrounded() {
			player.SendEvent(EventType.Entity_Player_Land, EventPriority.Stop);
			PlaySFX(PlayerLandClipId);
		}

		public override void Initialize() {
			hand.SetActive(true);

			int weaponID = Weapon.selectedWeaponID;

			weapon = GameManager.instance.weapons[weaponID];
			WeaponInitialized = weapon != null;
			weapon?.Initialize();

			playerUI = UUI.Get(PlayerUIPrefabId, GameManager.instance.mainScreenCanvas).GetComponent<UUI>();
			PlayerUiLife = playerUI.lifeNumber;
			entityState = PlayerState.idle;
			
			base.Initialize();
		}
		
		public override void OnHit(Entity attacker, float damage, Vector2? pushDir = null) {
			Invincible(0.2f);
			ShowDamageUI(damage);
			
			PlaySFX(PlayerHitClipId);

			bool isHurt = damage > data.HP * 0.3f;
			playerUI.animator.Play(isHurt?"hitHard":"hit", 0, 0);
			
			GameManager.SetTimeScale(0.05f, 0.3f);
			CameraBrain.instance.ShakeLerp(3f*Mathf.Max(Mathf.Log(damage+3),0.5f), 5);
			CameraBrain.instance.ZoomLerp(-0.3f);
		}

		protected override void OnHeal(float amount) {
			ShowDamageUI(-amount);
		}

		protected override float GetRealDamage(float rawDamage) {
			return rawDamage;
		}
		
		public override bool isAttackTarget(Entity entity) {
			return entity != this && !entity.Friendly;
		}
		
		protected override void EarlyRoutine() {
			lookingLeft = transform.position.x >= InputManager.mousePosition.x;
			GetInput();
			
			base.EarlyRoutine();
		}
		
		protected override void Routine() {
			base.Routine();
			
			SetTargetInteract();
		}
		
		protected override void Death() {
			WaveManager.instance.GameOver();
			
			base.Death();
		}
	}
}
