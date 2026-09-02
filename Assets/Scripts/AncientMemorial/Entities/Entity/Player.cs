using System;
using System.Collections;
using AncientMemorial.Cameras;
using AncientMemorial.Interactions;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using AncientMemorial.Waves;
using AncientMemorial.Weapons;
using UengSystem;
using UengSystem.Audio;
using UengSystem.Events;
using UengSystem.Inputs;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.SaveDatas.SettingDatas;
using UengSystem.States.PlayerStates;
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
		private int HEALTH_POTION_COUNT = "HealthPotionCount".GetHash();
		
		[Header("Hand")]
		public GameObject hand;
		public Animator handAnimator;
		public SpriteRenderer weaponSpriteRenderer;
		public Weapon weapon;

		private UUI playerUI;
		
		private bool _lookingLeft;
		
		public bool lookingLeft {
			get => _lookingLeft;
			set {
				if (_lookingLeft == value) return;
				_lookingLeft = value;
				player.animator.SetBool(BACKWARD, value);
			}
		}

		// STATE CASH //

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
		
		// ANIMATOR //
		private static readonly int  BACKWARD  = Animator.StringToHash("backward");
		
		// METHOD //
		
		private bool CanJump() {
			if (!isGround) return false;
			if (state == PlayerState.jump) return false;
			
			return true;
		}
		
		private bool CanDash() {
			if (state == PlayerState.dash) return false;
			if (PlayerState.dash.stateTimer.CheckIn(Dash.dashTime * 4f)) return false;

			return true;
		}

		private bool CanHeal() {
			if (UPureFloat.GetValue(HEALTH_POTION_COUNT) <= 0) return false;
			if (healAction.Executing) return false;
			if (!weapon.isAttacking) return false;
			
			return true;
		}
		
		private int  moveSign;

		public void SpawnAfterImage() {
			SpriteRenderer sr = UObjectPool.instance.Get("AfterImage", transform.position).GetComponent<SpriteRenderer>();
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

		private DelayedAction healAction;
		private void Heal() {
			healAction??=new DelayedAction(0.4f, () => {
				PlaySFX("playerHeal");
				SendAttackEvent(this, -3, true);
			}, () => {}, this);
			
			UPureFloat.AddValue(HEALTH_POTION_COUNT, -1);
			
			handAnimator.Play(lookingLeft?"healB":"heal", 0, 0);
			SetArm();
			
			PlaySFX("playerBottleOpen");
			
			healAction.ExecuteDA();
		}

		private StopWatch targetInteractSync     = new StopWatch();
		private float     targetInteractSyncTime = 0.1f;
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
		
		public bool getInput = true; 
		private void GetInput() {
		    if (!getInput) return;
		    
		    if (InputManager.GetInput(ActionType.Jump, PressType.Down | PressType.Hold) && CanJump()) state = PlayerState.jump;
		    
		    if (InputManager.GetInput(ActionType.Dash, PressType.Down) && CanDash()) state = PlayerState.dash;
		    
		    if (InputManager.GetInput(ActionType.MouseLClick, PressType.Down | PressType.Hold) && weapon.CanPrimaryAttack()) PrimaryAttack();
		    
		    if (InputManager.GetInput(ActionType.MouseRClick, PressType.Down) && weapon.CanSecondaryAttack()) SecondaryAttack();
		    
		    if (InputManager.GetInput(ActionType.Interact, PressType.Down))
		       SendEvent(EventType.Interact_Start, EventPriority.Start, new ValueData<Interaction>(targetInteraction));
		    
		    if (InputManager.GetInput(ActionType.Interact, PressType.Up))
		       SendEvent(EventType.Interact_Cancel, EventPriority.Cancel, new ValueData<Interaction>(targetInteraction));

		    if (InputManager.GetInput(ActionType.Heal, PressType.Down) && CanHeal()) Heal();

		    if (!DebugManager.instance.debugMode) return;
		    
		    if (InputManager.GetInput(ActionType.Debug_DamageUp, PressType.Up)) { 
		       stat.attackDamage += 1;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : attackDamage is now {stat.attackDamage}]");
		    }
		    if (InputManager.GetInput(ActionType.Debug_DamageDown, PressType.Up)) {
		       stat.attackDamage -= 1;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : attackDamage is now {stat.attackDamage}]");
		    }

		    if (InputManager.GetInput(ActionType.Debug_HealthUp, PressType.Up)) {
		       SendAttackEvent(this, -5, true);
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : hp is now {stat.HP}]");
		    }
		    if (InputManager.GetInput(ActionType.Debug_HealthDown, PressType.Up)) {
		       SendAttackEvent(this, 5, true);
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : hp is now {stat.HP}]");
		    }
		    
		    if (InputManager.GetInput(ActionType.Debug_MaxHealthUp, PressType.Up)) {
		       data.HP += 5;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : max hp is now {data.HP}]");
		    }
		    if (InputManager.GetInput(ActionType.Debug_MaxHealthDown, PressType.Up)) {
		       data.HP -= 5;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : max hp is now {data.HP}]");
		    }
		    
		    if (InputManager.GetInput(ActionType.Debug_MoveSpeedUp, PressType.Up)) {
		       stat.moveSpeed += 0.5f;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : moveSpeed is now {stat.moveSpeed}]");
		    }
		    if (InputManager.GetInput(ActionType.Debug_MoveSpeedDown, PressType.Up)) {
		       stat.moveSpeed -= 0.5f;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : moveSpeed is now {stat.moveSpeed}]");
		    }
		    
		    if (InputManager.GetInput(ActionType.Debug_AttackSpeedUp, PressType.Up)) {
		       stat.attackSpeed += 0.5f;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : attackSpeed is now {stat.attackSpeed}]");
		    }
		    if (InputManager.GetInput(ActionType.Debug_AttackSpeedDown, PressType.Up)) {
		       stat.attackSpeed -= 0.5f;
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : attackSpeed is now {stat.attackSpeed}]");
		    }
		    
		    if (InputManager.GetInput(ActionType.Debug_Damage, PressType.Up)) {
		       AttackArea(1, 0, InputManager.mousePosition, Vector2.one, ignoreInvincible:true);
		       InfoUUI.instance.AddInfoMessage("[DEBUG : AttackArea(IgnoreInvincible) spawned]");
		    }
		    
		    if (InputManager.GetInput(ActionType.Debug_AddHealPotion, PressType.Up)) {
				UPureFloat.AddValue(HEALTH_POTION_COUNT, 1);
		       InfoUUI.instance.AddInfoMessage($"[DEBUG : now you have {UPureFloat.GetValue(HEALTH_POTION_COUNT)} potions");
		    }
		}

		// EVENT BEHAVIOUR //
		
		protected override void OpenEntityUI() { }

		private float radH;
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
		
		// OVERRIDING //

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

		protected override void OnGrounded() {
			player.SendEvent(EventType.Entity_Player_Land, EventPriority.Stop);
			PlaySFX("playerLand");
		}

		public override void Initialize() {
			hand.SetActive(true);

			int weaponID = Weapon.selectedWeaponID;

			weapon = GameManager.instance.weapons[weaponID];
			weapon?.Initialize();

			playerUI = UUIPool.instance.Open("PlayerUI", GameManager.instance.mainScreenCanvas).GetComponent<UUI>();
			state = PlayerState.idle;
			
			base.Initialize();
		}
		
		public override void OnHit(Entity attacker, float damage, Vector2? pushDir = null) {
			Invincible(0.2f);
			ShowDamageUI(damage);
			
			PlaySFX("playerHit");

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
			GetInput();
			lookingLeft = transform.position.x >= InputManager.mousePosition.x;
			base.EarlyRoutine();
		}
		
		protected override void Routine() {
			base.Routine();
			
			SetTargetInteract();
		}

		private bool dead = false;
		protected override void Death() {
			if (dead) return;
			dead = true;
			
			UUIPool.instance.Open("DieUI", GameManager.instance.mainScreenCanvas);
			WaveManager.instance.currentWave = null;
			AudioManager.instance.SetBGM("Dead");
			AudioManager.instance.StopAllSFX();
			GameManager.SetTimeScale(0);
			
			base.Death();
		}
	}
}
