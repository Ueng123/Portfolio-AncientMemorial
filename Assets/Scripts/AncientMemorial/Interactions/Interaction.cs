

using System;
using System.Collections.Generic;
using AncientMemorial.Entities;
using UengSystem.Events;
using UengSystem.Objects;
using UengSystem.UAction;
using UengSystem.UI;
using UengSystem.UI.USliders;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UengSystem.VisualScripting.Tasks;
using UengSystem.VisualScripting.UValues;
using UengSystem.VisualScripting.UValues.UColors;
using UengSystem.VisualScripting.UValues.UFloats;
using UengSystem.VisualScripting.UValues.UObjects;
using UengSystem.VisualScripting.UValues.UStrings;
// using UengSystem.VisualScripting.UValues.UColors;
// using UengSystem.VisualScripting.UValues.UObjects;
// using UengSystem.VisualScripting.UValues.UStrings;
using UnityEngine;
using Event = UengSystem.Events.Event;
using EventType = UengSystem.Events.EventType;

namespace AncientMemorial.Interactions {
	public class Interaction : UObject {
		
		public static List<Interaction> InteractableInteractions = new ();
		
		public  DelayedAction         interactAction;
		private UInteractProgressValue _uInteractProgressValue;
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

		protected virtual void OnInteractStart() { }

		public void InteractStart() {
			onInteractStart.Execute(this);
			OnInteractStart();
		}

		protected virtual void OnCancel() { }

		public void Cancel() {
			onCancel.Execute(this);
			OnCancel();
		}

		protected virtual void OnInteract() { }

		public void Interact() {
			onInteract.Execute(this);
			OnInteract();
		}

		protected virtual void OnTarget() { }

		protected virtual void Targetted() {
			if (showInteractUI && !interactUI) {
				interactUI = UUIPool.instance.Open("InteractUI", canvas).GetComponent<UUI>();
                _uInteractProgressValue ??= new UInteractProgressValue { interactObject = new UPureObject { pureValue = this } };
                	
                USliderAction sliderAction = interactUI.GetAction<USliderAction>("Bar");
                sliderAction.value = _uInteractProgressValue;
                sliderAction.color.To<UNumberColor>().a = _uInteractProgressValue;
				
				UTextAction textAction = interactUI.GetAction<UTextAction>("TextLabel");
				interactionText ??= new UPureString { pureValue = interactString };
				textAction.text =   interactionText;
				textAction.Initialize(interactUI);
			}
			
			onTarget.Execute(this);
			OnTarget();
		}

		protected virtual void OnUnTarget() {  }

		protected virtual void Untargetted() {
			if (showInteractUI && interactUI) { 
				UUIPool.instance.Close(interactUI.gameObject);
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

		private bool prefabInteractable;
		public override void OnFirstGet() {
			interactAction = new DelayedAction(timeToInteract, () => { Interact(); SendEvent(UengSystem.Events.EventType.Interact_Stop, EventPriority.Stop); }, Cancel, this);
			prefabInteractable = interactable;
			base.OnFirstGet();
		}

		public override void Initialize() {
			interactable = prefabInteractable;

			interactStart    ??= InteractStartEvent;
			interactCancel   ??= InteractCancelEvent;
			interactTarget   ??= InteractTargetEvent;
			interactUntarget ??= InteractUntargetEvent;
			
			EventType.Interact_Start.AddListener(interactStart);
			EventType.Interact_Cancel.AddListener(interactCancel);
			EventType.Interact_Target.AddListener(interactTarget);
			EventType.Interact_Untarget.AddListener(interactUntarget);
    
			base.Initialize();
		}

		protected override void OnRelease() {
			interactable = false;
    
			EventType.Interact_Start.RemoveListener(interactStart);
			EventType.Interact_Cancel.RemoveListener(interactCancel);
			EventType.Interact_Target.RemoveListener(interactTarget);
			EventType.Interact_Untarget.RemoveListener(interactUntarget);
    
			base.OnRelease();
    
			if (interactUI) Untargetted();
		}
		
		public override void Uninitialize() {
			base.Uninitialize();

			if (!interactUI) return;
			Destroy(interactUI.gameObject);
			interactUI = null;
		}

		// UPDATE ROUTINE //

		protected override void EarlyRoutine() {
			if (CheckInteractable()) InteractableInteractions.Add(this);
		}

		// EVENT METHODS //

		private Action<Event> interactStart;
		private Action<Event> interactCancel;
		private Action<Event> interactTarget;
		private Action<Event> interactUntarget;
		
		private void InteractStartEvent(Event e) {
			if (e.GetData<ValueData<Interaction>>().value != this) return;
			
			InteractStart();
			interactAction.ExecuteDA();
		}

		private void InteractCancelEvent(Event e) {
			if (e.GetData<ValueData<Interaction>>().value != this) return;
			
			if (!interactAction.Executing) return;
			interactAction.Cancel();
		}

		private void InteractTargetEvent(Event e) {
			if (e.GetData<ValueData<Interaction>>().value != this) return;
			
			Targetted();
		}

		private void InteractUntargetEvent(Event e) {
			if (e.GetData<ValueData<Interaction>>().value != this) return;
			
			Untargetted();
			interactAction.Cancel();
		}
	}
}
