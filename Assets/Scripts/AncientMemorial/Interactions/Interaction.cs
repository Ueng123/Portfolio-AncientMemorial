

using System.Collections.Generic;
using AncientMemorial.Entities;
using TMPro;
using UengSystem.Events.EventDatas;
using UengSystem.Objects;
using UengSystem.Tasks;
using UengSystem.Tasks.Logic.UValues;
using UengSystem.Tasks.Logic.UValues.UStrings;
using UengSystem.UI;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using Events_Event = UengSystem.Events.Event;
using Slider = UnityEngine.UI.Slider;
using Image = UnityEngine.UI.Image;

namespace AncientMemorial.Interactions {
	public abstract class Interaction : AdvancedUObject {
		
		public static List<Interaction> InteractableInteractions = new ();
		
		public DelayedAction interactAction;

		[Header("Interaction")]
		public bool    interactable;
		public float          timeToInteract;
		public float          cooldownToInteract;
		public UCanvas        canvas;
		public UValue<string> InteractText;
		
		public Task onInteract;
		public Task onCancel;
		public Task onInteractStart;
		public Task onTarget;
		public Task onUnTarget;
		
		// Static Methods //
		
		// Instance Methods //

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

		protected virtual void OnTarget() {
			onTarget.Execute(this);
		}

		protected virtual void OnUnTarget() {
			onUnTarget.Execute(this);
		}
		
		protected virtual bool CheckInteractable() {
			if (!interactable) return false;
			if (!Entity.player) return false;
			
			Player player = Entity.player;
			float distance = Vector2.Distance(player.transform.position, transform.position);
			if (distance > player.maxInteractableDistance) return false;
			
			return true;
		}

		public void ChangeInteractText(UValue<string> newText) {
			InteractText = newText;
		}

		public float GetProgress() => interactAction.GetProgress();
		
		// ETC. Override //

		public override void OnFirstGet() {
			interactAction =
				new DelayedAction(timeToInteract,
								  () => {
									  Interact();
									  SendEvent(UengSystem.Events.EventType.Interact_Stop);
								  },
								  Cancel);
			
			base.OnFirstGet();
		}

		// UPDATE ROUTINE //

		protected override void EarlyRoutine() {
			if (CheckInteractable()) InteractableInteractions.Add(this);
		}

		protected override void Routine() {  }

		public override void OnEvent(Events_Event e) {
			switch (e.type) {
				case UengSystem.Events.EventType.Interact_Start: {
					if (((EventValueData<InteractTryInfo>)e.data).value.objectToInteract == this) {
						InteractStart();
						interactAction.Execute();
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
						OnTarget();
					}

					break;
				}
				case UengSystem.Events.EventType.Interact_Untarget: {
					if (((EventValueData<Interaction>)e.data).value == this) {
						OnUnTarget();
						interactAction.Cancel();
					}

					break;
				}
			}
		}

		protected override void LateRoutine() { }
		
		// FIXED UPDATE ROUTINE //
		
		protected override void FixedRoutine() { }
	}
}