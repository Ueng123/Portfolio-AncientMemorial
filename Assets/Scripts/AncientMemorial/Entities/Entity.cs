using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
using Newtonsoft.Json;
using UengSystem.Events;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UObjects;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.ObjectPool;
using UengSystem.Objects;
using UengSystem.UI;
using UengSystem.UI.USliders;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;
using Events_Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.Entities {
	public abstract class Entity : AdvancedUObject {

		public static Player player;
		public static BufferedList<Entity> entities = new BufferedList<Entity>();
		
		public EntityType entityType;
		public EntityData entityData;
		public Team       team;
		
		public UUI     entityUI;
		public UCanvas entityUICanvas;
		public int     entityUIHeight;
		
		public EntityStat entityStat;
		
		public List<Debris> debrisAttached = new List<Debris>();
		
		protected StopWatch unGroundedTimer = new ();
		protected int       jumpCount       = 1;
		private   bool      _isGround;
		public bool isGround {
			get => _isGround;
			set {
				if (_isGround == value) return;
				if ( value) {jumpCount = 1;}
				if (!value) {unGroundedTimer.Tick();}
				_isGround = value;
			}
		}

		private Vector2 groundBoxOffset;
		private Vector2 groundBoxSize;
		
		// Static Methods //
		
		// Instance Methods //
		protected virtual void Death() {
			float closeTime = UUIObjectPool.instance.prefabData.FirstOrDefault((data) => data.prefab.name == entityUI.name)!.closeTime;
			UObjectPool.instance.Release(gameObject, closeTime+0.1f);
			((USlider)entityUI.GetAction<USliderAction>("EntityHP").component).SetValue(0);
		}
		
		// ETC. Override //
		
		public override void Initialize() {
			base.Initialize();
		}

		public override void Uninitialize() {
			entityStat = null;
			entityData = default;
			
			base.Uninitialize();
		}

		private int testest = 0;
		public override void OnGet() {
			Debug.Log(testest++);
			
			string path = Path.Combine(Application.streamingAssetsPath, $"EntityData/{entityType}.json");
			if (!File.Exists(path)) {
				Debug.LogError($"NO FILE FOUND : [{entityType}] BRO;((((");
				return;
			}
			
			string jsonText = File.ReadAllText(path);
			entityData = JsonConvert.DeserializeObject<EntityData>(jsonText);
			
			entityStat      = new EntityStat(
				entityData.hp,
				entityData.moveSpeed,
				entityData.jumpPower,
				entityData.attackSpeed,
				entityData.attackDamage
			);
			
			groundBoxOffset = new Vector2(
				entityData.groundBoxOffsetX,
				entityData.groundBoxOffsetY
			);
			
			groundBoxSize   = new Vector2(
				entityData.groundBoxSizeX,
				entityData.groundBoxSizeY
			);
			
			entityUI = UUIObjectPool.instance.Open("EntityUI", entityUICanvas).GetComponent<UUI>();
			entityUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, entityUIHeight, 0);
			
			UTextAction   entityTextAction = entityUI.GetAction<UTextAction>("EntityName");
			USliderAction entityHPAction   = entityUI.GetAction<USliderAction>("EntityHP");
			
			entityTextAction.text = new UPureString {Text = entityData.name};
			entityTextAction.Initialize(entityUI);
			
			entityHPAction.component.GetComponent<RectTransform>().sizeDelta = new Vector2(100*Mathf.Log(entityData.hp, 2) , 30);
			entityHPAction.value = new UDiv {
				dynamicType = DynamicType.Dynamic,
				A = new UEntityStat {
					dynamicType = DynamicType.Dynamic,
					targetEntity = new UEntityByUObject {
						obj = new UObjectSerialized {
							obj = this
						}
					},
					type = EntityDataType.HP
				},
				B = new UPureNumber { number = entityData.hp }
			};
			
			base.OnGet();
			entities.Add(this);
		}

		public override void OnRelease() {
			UUIObjectPool.instance.Close(entityUI.gameObject);
			
			foreach (Debris debris in debrisAttached) {
				Debug.Log("deleting debris!!");
				debris.transform.SetParent(null);
				if (debris.isReleased) continue;
				UObjectPool.instance.Release(debris.gameObject);
			}

			debrisAttached.Clear();
			
			entities.Remove(this);
			base.OnRelease();
		}

		protected override IEnumerator DespawnFX(float duration) {
			spriteRenderer.color = new Color(
				spriteRenderer.color.r,
				spriteRenderer.color.g,
				spriteRenderer.color.b,
				0);
			
			yield return new WaitForSeconds(duration);
			
			UObjectPool.instance.Release(gameObject, -1);
			spriteRenderer.color = new Color(
				spriteRenderer.color.r,
				spriteRenderer.color.g,
				spriteRenderer.color.b,
				1);
		}

		// UPDATE ROUTINE //
		
		protected override void EarlyRoutine() { }

		public abstract void OnHit(Entity     attacker,   float    damage, Vector2? pushDir);
		public abstract void OnHit(Projectile projectile, float    damage, Vector2? pushDir);
		public abstract void OnHit(float      damage,     Vector2? pushDir);
		
		public override void OnEvent(Events_Event e) {
			base.OnEvent(e);
			switch (e.type) {
				case EventType.Entity_Behaviour_Hit:
					EntityHitData hitData = (EntityHitData)e.data;
					if (hitData.reciever != this) return;
					Debug.Log("[HIT RECIEVE] omg Event Recieved brohs");
					if (hitData.attackedEntity) OnHit(hitData.attackedEntity,         hitData.damage, hitData.pushDir);
					if (hitData.attackedProjectile) OnHit(hitData.attackedProjectile, hitData.damage, hitData.pushDir);
					if (!hitData.attackedEntity&&!hitData.attackedProjectile)   OnHit(hitData.damage, hitData.pushDir);
					break;
			}
		}

		protected override void LateRoutine() {
			if (entityStat.hp <= 0) {
				Death();
			}
		}
		
		// FIXED UPDATE ROUTINE //
		
		protected override void FixedRoutine() {
			isGround = (bool)Physics2D.OverlapBoxAll(
				point: (Vector2)transform.position + groundBoxOffset,
				size:  groundBoxSize,
				angle: 0
				).FirstOrDefault(c => c.GetComponent<Standable>()!=null);
		}
	}
}