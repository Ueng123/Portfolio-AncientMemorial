

using System.Collections.Generic;
using AncientMemorial.Entities;
using TMPro;
using UengSystem.Events;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UColors;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UObjects;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.Objects;
using UengSystem.UI;
using UengSystem.UI.USliders;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using Events_Event = UengSystem.Events.Event;
using Slider = UnityEngine.UI.Slider;
using Image = UnityEngine.UI.Image;

namespace AncientMemorial.Interactions {
	public abstract class Interaction : UObject {
		
		public static List<Interaction> InteractableInteractions = new ();
		
		public  DelayedAction         interactAction;
		private InteractProgressValue interactProgressValue;
		private UUI                   interactUI;

		[Header("Interaction")]
		[SerializeField]
		private bool interactable ;

		public bool           showInteractUI;
		public string         interactString;
		public UValue<string> interactionText;
		public float          timeToInteract;
		public float          cooldownToInteract;
		public UCanvas        canvas;
		
		public Task onInteract;
		public Task onCancel;
		public Task onInteractStart;
		public Task onTarget;
		public Task onUnTarget;
		
		// Static Methods //
		
		// Instance Methods //

		public virtual void Interactable()   => interactable = true;
		public virtual void UnInteractable() => interactable = false;

		protected abstract void OnInteractStart();
		public void InteractStart() {
			onInteractStart.Execute(this);
			OnInteractStart();
		}

		protected abstract void OnCancel();
		public void Cancel() {
			onCancel.Execute(this);
			OnCancel();
		}
		
		protected abstract void OnInteract();
		public void Interact() {
			onInteract.Execute(this);
			OnInteract();
		}

		protected abstract void OnTarget();
		protected virtual void Targetted() {
			if (showInteractUI && !interactUI) {
				interactUI = UUIObjectPool.instance.Open("InteractUI", canvas).GetComponent<UUI>();
                interactProgressValue ??= new InteractProgressValue { interactObject = new UObjectSerialized { obj = this } };
                	
                USliderAction sliderAction = interactUI.GetAction<USliderAction>("Bar");
                ((UBasicCubicBezierValue)sliderAction.value).T = interactProgressValue;
                ((UNumberColor)sliderAction.color).a = interactProgressValue;
				
				UTextAction textAction = interactUI.GetAction<UTextAction>("TextLabel");
				interactionText ??= new UPureString { Text = interactString };
				textAction.text =   interactionText;
				textAction.Initialize(interactUI);
			}
			
			onTarget.Execute(this);
			OnTarget();
		}

		protected abstract void OnUnTarget();
		protected virtual void Untargetted() {
			if (showInteractUI && interactUI) { 
				UUIObjectPool.instance.Close(interactUI.gameObject);
				interactUI = null;
			}
			
			onUnTarget.Execute(this);
			OnUnTarget();
		}
		
		protected virtual bool CheckInteractable() {
			if (!interactable) return false;
			if (!Entity.player) return false;
			
			Player player = Entity.player;
			float distance = Vector2.Distance(player.transform.position, transform.position);
			if (distance > player.maxInteractableDistance) return false;
			
			return true;
		}

		public float GetProgress() => interactAction.GetProgress();
		
		// ETC. Override //

		public override void OnFirstGet() {
			interactAction = new DelayedAction(timeToInteract, () => { Interact(); SendEvent(UengSystem.Events.EventType.Interact_Stop, 4); }, Cancel, this);
			base.OnFirstGet();
		}
		
		// UPDATE ROUTINE //

		protected override void EarlyRoutine() {
			if (CheckInteractable()) InteractableInteractions.Add(this);
		}

		protected override void Routine() { }

		public override void EventRoutine(Events_Event e) {
			switch (e.type) {
				case UengSystem.Events.EventType.Interact_Start: {
					if (((EventValueData<InteractTryInfo>)e.data).value.objectToInteract == this) {
						InteractStart();
						interactAction.ExecuteDA();
					}

					break;
				}
				case UengSystem.Events.EventType.Interact_Cancel: {
					if (((EventValueData<InteractTryInfo>)e.data).value.objectToInteract == this) {
						if (!interactAction.Executing) return;
						interactAction.Cancel();
					}

					break;
				}
				case UengSystem.Events.EventType.Interact_Target: {
					if (((EventValueData<Interaction>)e.data).value == this) {
						Targetted();
					}

					break;
				}
				case UengSystem.Events.EventType.Interact_Untarget: {
					if (((EventValueData<Interaction>)e.data).value == this) {
						Untargetted();
						interactAction.Cancel();
					}

					break;
				}
			}
		}
	}
}