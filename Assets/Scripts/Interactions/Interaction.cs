using System.Collections.Generic;
using AncientMemorial.AMObjects;
using AncientMemorial.Entities;
using AncientMemorial.Events.EventDatas;
using AncientMemorial.Tasks;
using TMPro;
using UnityEngine;
using Event = AncientMemorial.Events.Event;
using EventType = AncientMemorial.Events.EventType;
using Slider = UnityEngine.UI.Slider;
using Image = UnityEngine.UI.Image;

namespace AncientMemorial.Interactions {
	public abstract class Interaction : AdvancedAMObject {
		
		public static List<Interaction> InteractableInteractions = new ();
		
		public DelayedAction interactAction;

		[Header("Interaction")]
		public  bool   interactable;
		public  float    timeToInteract;
		public  float    cooldownToInteract;
		public  Canvas   canvas;
		public  Slider   slider;
		public  TMP_Text text;
		private Image    fill;

		public bool     ExecuteTasks;
		public TaskList TaskList;
		
		// Static Methods //
		
		// Instance Methods //

		public abstract void OnInteractStart();
		public abstract void OnCancel();
		public abstract void Interact();

		public virtual void OnTarget() {
			canvas.gameObject.SetActive(true);

			slider.gameObject.SetActive(true);
			slider.value = 0;
		}

		public virtual void OnUnTarget() {
			canvas.gameObject.SetActive(false);
		}

		public virtual bool CheckInteractable() {
			if (!interactable) return false;
			if (!Entity.player) return false;
			
			Player player = Entity.player;
			float distance = Vector2.Distance(player.transform.position, transform.position);
			if (distance > player.maxInteractableDistance) return false;
			
			return true;
		}

		public void ChangeInteractText(string newText) {
			text.text = newText;
		}
		
		// ETC. Override //

		public override void OnFirstGet() {
			interactAction = new DelayedAction(timeToInteract,
											   () => {
												   Interact();
												   if (ExecuteTasks) TaskList.ExecuteTasks(this);
												   SendEvent(EventType.Interact_Stop);
											   },
											   OnCancel);
			
			fill = slider.fillRect.GetComponent<Image>();
			
			base.OnFirstGet();
		}

		// UPDATE ROUTINE //

		protected override void EarlyRoutine() {
			if (CheckInteractable()) InteractableInteractions.Add(this);
		}

		protected override void Routine() {
			float progress = interactAction.GetProgress();
			
			slider.value = (progress==0)?
							   Mathf.Lerp(slider.value, 0, Time.deltaTime * 5):
							   Mathf.Sin(0.5f*progress*Mathf.PI);
			
			slider.gameObject.SetActive(slider.value > 0);

			fill.color = new Color(1, 1, 1, (progress + slider.value)/2);
		}

		public override void OnEvent(Event e) {
			if (e.type == EventType.Interact_Start) {
				if (((EventValueData<InteractTryInfo>)e.data).value.objectToInteract == this) {
					OnInteractStart();
					interactAction.Execute();
				}
			}

			if (e.type == EventType.Interact_Cancel) {
				if (((EventValueData<InteractTryInfo>)e.data).value.objectToInteract == this) {
					if (!interactAction.Executing) return;
					interactAction.Cancel();
				}
			}
			
			if (e.type == EventType.Interact_Target) {
				if (((EventValueData<Interaction>)e.data).value == this) {
					OnTarget();
				}
			}
			
			if (e.type == EventType.Interact_Untarget) {
				if (((EventValueData<Interaction>)e.data).value == this) {
					OnUnTarget();
					interactAction.Cancel();
				}
			}
		}

		protected override void LateRoutine() { }
		
		// FIXED UPDATE ROUTINE //
		
		protected override void FixedRoutine() { }
	}
}