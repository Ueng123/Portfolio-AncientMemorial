using System;
using System.Collections;
using System.Collections.Generic;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.Objects.LifeCycle;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	// Per invocation state; waves can share serialized Task/TaskComponent instances.
	public sealed class TaskExecution {
		[ThreadStatic] internal static TaskExecution Current;
		private readonly UObject Owner;
		private readonly bool HasOwner;
		private readonly UObject Runner;
		private readonly long Life;
		private readonly bool Releasing;
		private readonly Stack<IEnumerator> Enumerators = new();
		private Coroutine Coroutine;
		internal Coroutine coroutine => Coroutine;
		private bool Cancelled;
		private bool Advancing;
		public bool isRunning { get; private set; } = true;
		internal bool isRealTime => Owner is UI.UUI;

		internal TaskExecution(IEnumerator Enumerator, UObject Owner, UObject Runner = null) {
			this.Owner = Owner && Owner.lifeCycle.isPrepared ? Owner : null;
			
			HasOwner = !ReferenceEquals(this.Owner, null);
			this.Runner = Runner??GlobalObject.instance;
			Life = this.Owner?.lifeNumber??0;
			Releasing = this.Owner && this.Owner.lifeCycle.phase == LifeCyclePhase.Releasing;
			
			Enumerators.Push(Enumerator);
		}



		private bool IsValid() {
			// 오너가 없다 or 오너가 있으면서 (
			
			return !Cancelled && !UObject.isStoppingLifeCycles && (!HasOwner || (Owner && Owner.lifeNumber == Life && !Owner.isReleased
										 && !Owner.lifeCycle.isShuttingDown && (Releasing || Owner.lifeCycle.phase != LifeCyclePhase.Releasing)));	
		}

		internal TaskExecution Start() {
			Owner?.RegisterTask(this);
			
			if (!Runner || !Runner.gameObject.activeInHierarchy || !IsValid()) { Cancel(); return this; }
			
			Coroutine Started = Runner.StartCoroutine(Run());
			
			if (isRunning) Coroutine = Started;
			
			return this;
		}

		internal TaskExecution StartChild(IEnumerator Enumerator) => new TaskExecution(Enumerator, Owner, Runner).Start();

		private IEnumerator Run() {
			try {
				while (Enumerators.Count > 0 && IsValid()) {
					IEnumerator Enumerator = Enumerators.Peek();
					TaskExecution Previous = Current;
					bool Next;
					object Yield;
					try {
						Current   = this;
						Advancing = true;
						Next      = Enumerator.MoveNext();
						Yield     = Next ? Enumerator.Current : null;
					}
					finally { Advancing = false; Current = Previous; }

					if (!IsValid()) yield break;
					if (!Next) { Enumerators.Pop(); (Enumerator as IDisposable)?.Dispose(); continue; }
					if (Yield is IEnumerator Nested) { Enumerators.Push(Nested); continue; }

					yield return Yield;
				}
			}
			finally {
				isRunning = false;
				Owner?.UnregisterTask(this);
				DisposeEnumerators();
			}
		}

		private void DisposeEnumerators() {
			while (Enumerators.Count > 0) (Enumerators.Pop() as IDisposable)?.Dispose();
		}

		public void Cancel() {
			if (Cancelled) return;
			
			Cancelled = true;
			isRunning = false;
			Owner?.UnregisterTask(this);
			
			if (Runner && Coroutine != null) Runner.StopCoroutine(Coroutine);
			if (!Advancing) DisposeEnumerators();
		}
	}
}
