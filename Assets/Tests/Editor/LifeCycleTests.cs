using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using AncientMemorial.Entities;
using AncientMemorial.Projectiles;
using UengSystem.Events;
using UengSystem;
using UengSystem.Managers;
using UengSystem.Objects;
using UengSystem.Objects.LifeCycle;
using UengSystem.ObjectPool;
using UengSystem.UAction;
using UengSystem.Utility;
using UengSystem.UI;
using UengSystem.VisualScripting.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace UengSystem.Tests {
	public sealed class ProjectileHitProbe : Projectile {
		public void Hit(Entity Target, Action Effect, bool CanPierce = false) => SendCollisionHit(Target, 7, Effect, CanPierce);
		public void FinishHits() => LateRoutine();
		protected override void OnCollideEntity(Entity Target) { }
		protected override void OnCollideObject(UObject Target) { }
	}

	public sealed class ProjectileTargetProbe : Entity {
		public int Hits;
		public float DamageTaken;
		public override void OnGet() { }
		public override void Initialize() { Invincible(false); }
		public override void Uninitialize() { }
		protected override void OnRelease() { }
		public override bool isAttackTarget(Entity Target) => true;
		protected override void OnGrounded() { }
		public override void ChangeHP(float Amount) => DamageTaken -= Amount;
		public override void OnHit(Entity Attacker, float Damage, Vector2? PushDir = null) => Hits++;
		protected override void OnHeal(float Amount) { }
		protected override float GetRealDamage(float Damage) => Damage;
		public void Receive(UengSystem.Events.Event Hit) => typeof(Entity).GetMethod("HitEvent", BindingFlags.Instance | BindingFlags.NonPublic)
			.Invoke(this, new object[] { Hit });
	}

	public class LifeCycleProbe : UObject {
		public readonly List<string> Trace = new();
		public Action OnInitializing;
		public Action OnUninitializing;
		public Action OnGetting;
		public Action OnReleasing;
		public int FirstGets;
		public override void OnFirstGet() { FirstGets++; base.OnFirstGet(); }
		public override void OnGet() { Trace.Add("get"); OnGetting?.Invoke(); }
		public override void Initialize() {
			Assert.AreEqual(LifeCyclePhase.Active, lifeCycle.phase);
			Assert.Contains(this, UObject.instances.ToList());
			Trace.Add("initialize");
			OnInitializing?.Invoke();
		}
		protected override void OnRelease() {
			Assert.AreEqual(LifeCyclePhase.Releasing, lifeCycle.phase);
			Assert.IsFalse(UObject.instances.Contains(this));
			Trace.Add("release");
			OnReleasing?.Invoke();
		}
		public override void Uninitialize() { Trace.Add("uninitialize"); OnUninitializing?.Invoke(); }
		protected override void EarlyRoutine() { Trace.Add("early"); }
		protected override void Routine() { Trace.Add("routine"); }
		protected override void LateRoutine() { Trace.Add("late"); }
		protected override void FixedRoutine() { Trace.Add("fixed"); }
	}

	public sealed class LifeCycleUiProbe : UUI {
		public readonly List<string> Trace = new();
		public int Opens;
		public int Closes;
		public int Initializations;
		public int Uninitializations;
		public override void OnOpen() { Opens++; Assert.IsNotNull(canvas); Assert.AreEqual(initialPosition, rectTransform.anchoredPosition); }
		public override void OnClose() { Closes++; }
		public override void Initialize() { Initializations++; }
		public override void Uninitialize() { Uninitializations++; }
	}

	public sealed class ProbeGetting : Getting {
		public int Starts;
		public int Clears;
		public int Exits;
		public bool ThrowOnClear;
		public Action Finish;
		public Action UpdateEffect;
		public Action Clearing;
		public ProbeGetting(UObject Target) : base(Target) { }
		protected override float duration => 1;
		protected override void OnStartEffect() { Starts++; Finish = CaptureCompletion(); }
		protected override void OnEffectRoutine(float DeltaTime) { UpdateEffect?.Invoke(); }
		protected override void ClearEffect() { Clears++; Clearing?.Invoke(); if (ThrowOnClear) throw new InvalidOperationException("test cleanup failure"); }
		protected override void OnStateExit() { Exits++; }
		public void CompleteWithoutToken() => Complete();
	}

	public sealed class ProbeReleasing : Releasing {
		public int Starts;
		public int Clears;
		public int Exits;
		public Action Finish;
		public bool ThrowOnClear;
		public ProbeReleasing(UObject Target) : base(Target) { }
		protected override float duration => 1;
		protected override void OnStartEffect() { Starts++; Finish = CaptureCompletion(); }
		protected override void ClearEffect() { Clears++; if (ThrowOnClear) throw new InvalidOperationException("test release cleanup failure"); }
		protected override void OnStateExit() { Exits++; }
	}

	[Serializable]
	public sealed class TraceTask : TaskComponent {
		public string Marker;
		public override void Execute(ITaskable Self) {
			if (Self is LifeCycleProbe Probe) Probe.Trace.Add(Marker);
			if (Self is LifeCycleUiProbe Ui) Ui.Trace.Add(Marker);
		}
	}

	[TestFixture]
	public class LifeCycleTests {
		private readonly List<GameObject> Objects = new();
		private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
		private UObjectPool PreviousPool;
		private UUIPool PreviousUiPool;
		private GameManager PreviousManager;
		private float PreviousTimeScale;
		private EventManager PreviousEvents;
		private GlobalObject PreviousGlobal;

		[SetUp]
		public void SetUp() {
			PreviousPool = UObjectPool.instance;
			PreviousUiPool = UUIPool.instance;
			PreviousManager = GameManager.instance;
			PreviousTimeScale = Time.timeScale;
			PreviousEvents = EventManager.instance;
			PreviousGlobal = GlobalObject.instance;
		}

		[TearDown]
		public void TearDown() {
			foreach (GameObject Obj in Objects.AsEnumerable().Reverse()) {
				if (!Obj) continue;
				foreach (MonoBehaviour Component in Obj.GetComponents<MonoBehaviour>()) {
					if (Component is IManager Manager) IManager.instances.Remove(Manager);
				}
				Object.DestroyImmediate(Obj);
			}
			Objects.Clear();
			UObjectPool.instance = PreviousPool;
			UUIPool.instance = PreviousUiPool;
			GameManager.instance = PreviousManager;
			Time.timeScale = PreviousTimeScale;
			EventManager.instance = PreviousEvents;
			GlobalObject.instance = PreviousGlobal;
		}

		private T Create<T>() where T : Component {
			GameObject Obj = new("LifeCycle test " + typeof(T).Name);
			Obj.hideFlags = HideFlags.HideAndDontSave;
			Objects.Add(Obj);
			return Obj.AddComponent<T>();
		}
		private static void Invoke(UObject Target, string Name, params object[] Args) => typeof(UObject).GetMethod(Name, Hidden).Invoke(Target, Args);
		private static void Begin(UObject Target, bool PlayEffect) => Invoke(Target, "BeginLife", PlayEffect, null);
		private static void Tick(UObject Target, float DeltaTime = 1) => typeof(LifeCycleStateMachine).GetMethod("Tick", Hidden)
			.Invoke(Target.lifeCycle, new object[] { Target.lifeCycle.executionNumber, DeltaTime });
		private LifeCycleProbe Probe(out ProbeGetting Getting, out ProbeReleasing Releasing) {
			LifeCycleProbe Target = Create<LifeCycleProbe>();
			Getting = new ProbeGetting(Target);
			Releasing = new ProbeReleasing(Target);
			Target.SetStates(Getting, Releasing);
			Target.OnFirstGet();
			return Target;
		}

		[Test]
		public void DefaultStatesCompleteAndRetainComponents() {
			LifeCycleProbe Target = Create<LifeCycleProbe>();
			Rigidbody2D Body = Target.gameObject.AddComponent<Rigidbody2D>();
			Collider2D Collider = Target.gameObject.AddComponent<BoxCollider2D>();
			Target.Get();
			Assert.IsInstanceOf<DefaultGetting>(Target.lifeCycle.currentState);
			Assert.IsFalse(Collider.enabled);
			Tick(Target, 10);
			Assert.IsTrue(Target.isActive);
			Assert.IsTrue(Collider.enabled);
			Target.Release();
			Assert.IsInstanceOf<DefaultReleasing>(Target.lifeCycle.currentState);
			Tick(Target, 10);
			Assert.IsTrue(Target.isReleased);
			Assert.AreSame(Body, Target.rigidbody2D);
			CollectionAssert.AreEqual(new[] { "get", "initialize", "release", "uninitialize" }, Target.Trace);
		}

		[Test]
		public void ConfigurationRejectsDifferentOwnersAndLateChanges() {
			LifeCycleProbe Target = Create<LifeCycleProbe>();
			LifeCycleProbe Other = Create<LifeCycleProbe>();
			Assert.Throws<ArgumentException>(() => Target.SetGettingState(new ProbeGetting(Other)));
			ProbeGetting Custom = new(Target);
			Target.SetGettingState(Custom);
			Target.OnFirstGet();
			Assert.Throws<InvalidOperationException>(() => Target.SetGettingState(new ProbeGetting(Target)));
			Begin(Target, true);
			Assert.AreSame(Custom, Target.lifeCycle.currentState);
			Target.Release();
			Assert.IsInstanceOf<DefaultReleasing>(Target.lifeCycle.currentState);
		}

		[Test]
		public void ExplicitStatesSurviveUpperLevelDefaults() {
			LifeCycleProbe Target = Create<LifeCycleProbe>();
			ProbeGetting Custom = new(Target);
			Target.SetGettingState(Custom);
			Target.lifeCycle.SetDefaultStates(new DefaultGetting(Target), new DefaultReleasing(Target));
			Target.OnFirstGet();
			Begin(Target, true);
			Assert.AreSame(Custom, Target.lifeCycle.currentState);
		}

		[Test]
		public void ImmediateRequestsRunHooksWithoutStartingEffects() {
			LifeCycleProbe Target = Probe(out ProbeGetting Getting, out ProbeReleasing Releasing);
			Begin(Target, false);
			Assert.IsTrue(Target.isActive);
			Target.Release(false);
			Assert.IsTrue(Target.isReleased);
			Assert.AreEqual(0, Getting.Starts + Releasing.Starts);
			Assert.AreEqual(1, Getting.Clears);
			Assert.AreEqual(1, Getting.Exits);
			Assert.AreEqual(1, Releasing.Clears);
			Assert.AreEqual(1, Releasing.Exits);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GettingCancellationNeverInitializes(bool PlayRelease) {
			LifeCycleProbe Target = Probe(out ProbeGetting Getting, out ProbeReleasing Releasing);
			Begin(Target, true);
			Target.Release(PlayRelease);
			Tick(Target);
			Assert.IsTrue(Target.isReleased);
			Assert.IsFalse(Target.Trace.Contains("initialize"));
			Assert.AreEqual(1, Getting.Clears);
			Assert.AreEqual(1, Getting.Exits);
			Assert.IsFalse(Getting.isComplete);
			Assert.AreEqual(1, Releasing.Clears);
		}

		[Test]
		public void RepeatedReleaseCompletesTheExistingExecutionOnlyOnce() {
			LifeCycleProbe Target = Probe(out ProbeGetting Getting, out ProbeReleasing Releasing);
			int Returns = 0;
			Invoke(Target, "BindPool", (Action<UObject>)(_ => Returns++));
			Begin(Target, false);
			Target.Release();
			long Execution = Target.lifeCycle.executionNumber;
			Target.Release();
			Assert.AreEqual(Execution, Target.lifeCycle.executionNumber);
			Target.Release(false);
			Target.Release(false);
			Releasing.Finish();
			Assert.AreEqual(1, Releasing.Starts);
			Assert.AreEqual(1, Releasing.Clears);
			Assert.AreEqual(1, Releasing.Exits);
			Assert.AreEqual(1, Returns);
		}

		[Test]
		public void InitializeCanImmediatelyReleaseAndReacquire() {
			LifeCycleProbe Target = Probe(out ProbeGetting Getting, out ProbeReleasing Releasing);
			int Returns = 0;
			Invoke(Target, "BindPool", (Action<UObject>)(_ => { Returns++; Begin(Target, true); }));
			Target.OnInitializing = () => { Target.OnInitializing = null; Target.Release(false); };
			Begin(Target, false);
			Assert.AreEqual(1, Returns);
			Assert.AreEqual(2, Target.lifeNumber);
			Assert.AreEqual(LifeCyclePhase.Getting, Target.lifeCycle.phase);
			Tick(Target);
			Assert.IsTrue(Target.isActive);
			Assert.AreEqual(2, Target.Trace.Count(Item => Item == "initialize"));
		}

		[Test]
		public void UninitializeBlocksDuplicateRequests() {
			LifeCycleProbe Target = Probe(out _, out ProbeReleasing Releasing);
			Target.OnUninitializing = () => {
				Target.Release(false);
				Assert.Throws<InvalidOperationException>(() => Target.Get(false));
			};
			Begin(Target, false);
			Target.Release(false);
			Assert.AreEqual(1, Releasing.Clears);
			Assert.AreEqual(1, Target.Trace.Count(Item => Item == "uninitialize"));
		}

		[Test]
		public void OldCompletionCannotFinishNewExecution() {
			LifeCycleProbe Target = Probe(out ProbeGetting Getting, out _);
			Begin(Target, true);
			Action Old = Getting.Finish;
			Target.Release(false);
			Begin(Target, true);
			Old();
			Getting.CompleteWithoutToken();
			Assert.AreEqual(LifeCyclePhase.Getting, Target.lifeCycle.phase);
			Assert.AreEqual(0, Getting.elapsed);
			Assert.IsFalse(Getting.isComplete);
			Getting.Finish();
			Assert.IsTrue(Target.isActive);
		}

		[Test]
		public void ReleaseFromOnGetCancelsBeforeTheEffectStarts() {
			LifeCycleProbe Target = Probe(out ProbeGetting Getting, out _);
			Target.OnGetting = () => Target.Release(false);
			Begin(Target, true);
			Assert.IsTrue(Target.isReleased);
			Assert.AreEqual(0, Getting.Starts);
			Assert.IsFalse(Target.Trace.Contains("initialize"));
		}

		[Test]
		public void ImmediateReleaseDuringReleaseHookFinishesAfterMandatoryCleanup() {
			LifeCycleProbe Target = Probe(out _, out ProbeReleasing Releasing);
			Target.OnReleasing = () => Target.Release(false);
			Begin(Target, false);
			Target.Release();
			Assert.IsTrue(Target.isReleased);
			Assert.AreEqual(0, Releasing.Starts);
			Assert.AreEqual(1, Releasing.Clears);
		}

		[Test]
		public void CleanupExceptionQuarantinesInsteadOfActivatingOrReturning() {
			LifeCycleProbe Target = Probe(out ProbeGetting Getting, out _);
			int Returns = 0;
			Invoke(Target, "BindPool", (Action<UObject>)(_ => Returns++));
			Getting.ThrowOnClear = true;
			LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("test cleanup failure"));
			Begin(Target, true);
			Tick(Target);
			Assert.IsTrue(Target.lifeCycle.isFaulted);
			Assert.IsFalse(Target.gameObject.activeSelf);
			Assert.AreEqual(0, Returns);
			Assert.IsFalse(Target.Trace.Contains("initialize"));
			Assert.AreEqual(1, Getting.Clears);
		}

		[Test]
		public void SceneShutdownCancelsGettingAndNeverReturns() {
			LifeCycleProbe Target = Probe(out ProbeGetting Getting, out _);
			int Returns = 0;
			Invoke(Target, "BindPool", (Action<UObject>)(_ => Returns++));
			Begin(Target, true);
			Target.lifeCycle.Shutdown();
			Target.lifeCycle.Shutdown();
			Getting.Finish();
			Assert.AreEqual(0, Returns);
			Assert.IsFalse(Target.Trace.Contains("initialize"));
			Assert.AreEqual(1, Getting.Clears);
		}

		[Test]
		public void SceneObjectDeactivatesAndCanBeAcquiredAgain() {
			LifeCycleProbe Target = Create<LifeCycleProbe>();
			Target.Get(false);
			Target.Release(false);
			Assert.IsFalse(Target.gameObject.activeSelf);
			Target.Get(false);
			Assert.IsTrue(Target.isActive);
			Assert.AreEqual(2, Target.lifeNumber);
			Assert.AreEqual(1, Target.FirstGets);
		}

		[TestCase(false)]
		[TestCase(true)]
		public void DestructionCleanupDoesNotStartReleaseTasksOnInactiveTargets(bool InactiveParent) {
			LifeCycleProbe Target = Probe(out _, out _);
			LifeCycleProbe Other = Probe(out _, out ProbeReleasing Releasing);
			Other.ReleaseTask = MakeTask(Mark("release-task"));
			int Returns = 0;
			Invoke(Other, "BindPool", (Action<UObject>)(Item => Returns++));
			Begin(Target, false);
			Begin(Other, false);
			if (InactiveParent) {
				Transform Parent = Create<LifeCycleProbe>().transform;
				Other.transform.SetParent(Parent);
				Parent.gameObject.SetActive(false);
			}
			else Other.gameObject.SetActive(false);
			Target.OnReleasing = () => Other.Release();
			// EditMode probes do not receive normal Play Mode destruction messages.
			Invoke(Target, "OnDestroy");
			Object.DestroyImmediate(Target.gameObject);
			Assert.IsTrue(Other.lifeCycle.isShuttingDown);
			Assert.IsTrue(Other.isReleased);
			Assert.AreEqual(0, Releasing.Starts);
			Assert.AreEqual(0, Returns);
			Assert.IsFalse(Other.Trace.Contains("release-task"));
			Assert.AreEqual(1, Other.Trace.Count(Item => Item == "release"));
			LogAssert.NoUnexpectedReceived();
		}

		[Test]
		public void DestroyingReturnedObjectDoesNotRepeatReleaseHook() {
			LifeCycleProbe Target = Probe(out _, out _);
			Begin(Target, false);
			Target.Release(false);
			List<string> Trace = Target.Trace;
			Invoke(Target, "OnDestroy");
			Object.DestroyImmediate(Target.gameObject);
			Assert.AreEqual(1, Trace.Count(Item => Item == "release"));
			Assert.AreEqual(1, Trace.Count(Item => Item == "uninitialize"));
			LogAssert.NoUnexpectedReceived();
		}

		private static Task MakeTask(params TaskListItem[] Items) {
			Task Result = new();
			typeof(Task).GetField("tasks", Hidden).SetValue(Result, Items);
			return Result;
		}
		private static TaskListItem Mark(string Marker, float Time = 0) => new() {
			time = Time, tasks = new TaskComponent[] { new TraceTask { Marker = Marker } }
		};

		[Test]
		public void InactiveRunnerCancelsTaskWithoutLeavingItRegistered() {
			LifeCycleProbe Owner = Probe(out _, out _);
			LifeCycleProbe Runner = Probe(out _, out _);
			Begin(Owner, false);
			Begin(Runner, false);
			Runner.gameObject.SetActive(false);
			TaskExecution Execution = (TaskExecution)Activator.CreateInstance(typeof(TaskExecution), Hidden, null,
				new object[] { MakeTask(Mark("unexpected-task")).ExecuteEnumerator(Owner), Owner, Runner }, null);
			typeof(TaskExecution).GetMethod("Start", Hidden).Invoke(Execution, null);
			Assert.IsFalse(Execution.isRunning);
			Assert.IsFalse(Owner.Trace.Contains("unexpected-task"));
			Assert.IsEmpty((IEnumerable)typeof(UObject).GetField("Tasks", Hidden).GetValue(Owner));
			LogAssert.NoUnexpectedReceived();
		}

		[Test]
		public void ApplicationQuitBlocksNewTasksAndReentrantReleaseEffects() {
			LifeCycleProbe Target = Probe(out _, out _);
			LifeCycleProbe Other = Probe(out _, out ProbeReleasing Releasing);
			Begin(Target, false);
			Begin(Other, false);
			Other.ReleaseTask = MakeTask(Mark("release-task"));
			Target.OnReleasing = () => Other.Release();
			PropertyInfo Flag = typeof(UObject).GetProperty("isStoppingLifeCycles", BindingFlags.Static | BindingFlags.Public);
			bool Previous = UObject.isStoppingLifeCycles;
			try {
				Invoke(Target, "OnApplicationQuit");
				Assert.IsTrue(UObject.isStoppingLifeCycles);
				Assert.IsFalse(Other.canStartOwnedWork);
				TaskExecution Execution = (TaskExecution)Activator.CreateInstance(typeof(TaskExecution), Hidden, null,
					new object[] { MakeTask(Mark("unexpected-task")).ExecuteEnumerator(Other), null, Other }, null);
				typeof(TaskExecution).GetMethod("Start", Hidden).Invoke(Execution, null);
				Assert.IsFalse(Execution.isRunning);
				Invoke(Target, "OnDestroy");
				Object.DestroyImmediate(Target.gameObject);
				Assert.IsTrue(Other.lifeCycle.isShuttingDown);
				Assert.IsTrue(Other.isReleased);
				Assert.AreEqual(0, Releasing.Starts);
				Assert.IsFalse(Other.Trace.Contains("release-task"));
				Assert.IsFalse(Other.Trace.Contains("unexpected-task"));
				LogAssert.NoUnexpectedReceived();
			}
			finally { Flag.SetValue(null, Previous); }
		}

		[Test]
		public void GetAndReleaseTasksExecuteOnceInHookOrder() {
			LifeCycleProbe Target = Probe(out _, out _);
			Target.GetTask = MakeTask(Mark("get-task"));
			Target.ReleaseTask = MakeTask(Mark("release-task"));
			Begin(Target, false);
			Target.Release(false);
			CollectionAssert.AreEqual(new[] { "get", "get-task", "initialize", "release", "release-task", "uninitialize" }, Target.Trace);
		}

		[Test]
		public void SuspendedTaskCannotModifyTheNextLife() {
			LifeCycleProbe Target = Probe(out _, out _);
			Begin(Target, false);
			Task Work = MakeTask(Mark("started"), Mark("stale", 1));
			IEnumerator Iterator = Work.ExecuteEnumerator(Target);
			Assert.IsTrue(Iterator.MoveNext());
			Assert.Contains("started", Target.Trace);
			Target.Release(false);
			Begin(Target, false);
			Assert.IsFalse(Iterator.MoveNext());
			Assert.IsFalse(Target.Trace.Contains("stale"));
		}

		[Test]
		public void OwnedTaskCancellationDisposesSuspendedWorkOnce() {
			LifeCycleProbe Target = Probe(out _, out _);
			Begin(Target, false);
			int Disposals = 0;
			int Resumptions = 0;
			IEnumerator Work() {
				try { yield return null; Resumptions++; }
				finally { Disposals++; }
			}
			TaskExecution Execution = (TaskExecution)Activator.CreateInstance(typeof(TaskExecution), Hidden, null,
				new object[] { Work(), Target, Target }, null);
			Invoke(Target, "RegisterTask", Execution);
			IEnumerator Run = (IEnumerator)typeof(TaskExecution).GetMethod("Run", Hidden).Invoke(Execution, null);
			Assert.IsTrue(Run.MoveNext());
			Target.Release(false);
			Begin(Target, false);
			Assert.IsFalse(Run.MoveNext());
			Execution.Cancel();
			Assert.AreEqual(1, Disposals);
			Assert.AreEqual(0, Resumptions);
		}

		[Test]
		public void CancellationCannotStartImmediateOwnedActions() {
			LifeCycleProbe Target = Probe(out _, out _);
			Begin(Target, false);
			int Calls = 0;
			bool Started = false;
			DelayedAction Delayed = new(0, () => Calls++, executor: Target);
			WaitAction Waiting = new(() => true, () => Calls++, executor: Target);
			IEnumerator Work() {
				try { yield return null; }
				finally { Started = true; Delayed.ExecuteDA(); Waiting.ExecuteWA(); }
			}
			TaskExecution Execution = (TaskExecution)Activator.CreateInstance(typeof(TaskExecution), Hidden, null,
				new object[] { Work(), Target, Target }, null);
			Invoke(Target, "RegisterTask", Execution);
			IEnumerator Run = (IEnumerator)typeof(TaskExecution).GetMethod("Run", Hidden).Invoke(Execution, null);
			Assert.IsTrue(Run.MoveNext());
			Target.Release(false);
			Assert.IsTrue(Started);
			Assert.AreEqual(0, Calls);
			Assert.IsFalse(Target.lifeCycle.isFaulted);
		}

		[Test]
		public void ReleaseCleanupExceptionNeverReturnsToPool() {
			LifeCycleProbe Target = Probe(out _, out ProbeReleasing Releasing);
			int Returns = 0;
			Invoke(Target, "BindPool", (Action<UObject>)(Item => Returns++));
			Begin(Target, false);
			Releasing.ThrowOnClear = true;
			LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("test release cleanup failure"));
			Target.Release(false);
			Assert.IsTrue(Target.lifeCycle.isFaulted);
			Assert.AreEqual(0, Returns);
			Assert.AreEqual(1, Releasing.Clears);
			Assert.IsFalse(Target.Trace.Contains("uninitialize"));
		}

		[Test]
		public void ShutdownInsideEffectCleanupCannotActivate() {
			LifeCycleProbe Target = Probe(out ProbeGetting Getting, out _);
			Getting.Clearing = Target.lifeCycle.Shutdown;
			Begin(Target, true);
			Tick(Target);
			Assert.IsTrue(Target.lifeCycle.isShuttingDown);
			Assert.IsFalse(Target.Trace.Contains("initialize"));
			Assert.IsFalse(UObject.instances.Contains(Target));
		}

		[Test]
		public void SceneShutdownGuardAlsoStopsReentrantSiblingReturns() {
			LifeCycleProbe Target = Probe(out _, out _);
			LifeCycleProbe Other = Probe(out _, out _);
			int Returns = 0;
			Invoke(Other, "BindPool", (Action<UObject>)(Item => Returns++));
			Begin(Target, false);
			Begin(Other, true);
			Target.OnReleasing = () => Other.Release(false);
			PropertyInfo Flag = typeof(UObject).GetProperty("isStoppingLifeCycles", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			object Previous = Flag.GetValue(null);
			try {
				Flag.SetValue(null, true);
				Target.lifeCycle.Shutdown();
				Assert.IsTrue(Other.lifeCycle.isShuttingDown);
				Assert.AreEqual(0, Returns);
				Assert.Throws<InvalidOperationException>(() => Target.Get(false));
			}
			finally { Flag.SetValue(null, Previous); }
		}

		[Test]
		public void AnimatorFreezeRestoresDisabledComponent() {
			LifeCycleProbe Target = Create<LifeCycleProbe>();
			Animator Animator = Target.gameObject.AddComponent<Animator>();
			Animator.enabled = false;
			Animator.speed = 0.7f;
			Target.Get();
			Tick(Target, 10);
			Assert.IsFalse(Animator.enabled);
			Assert.AreEqual(0.7f, Animator.speed);
		}

		[Test]
		public void PrefabHashAndRuntimeIdAreIndependentAndPoolKeySurvivesRename() {
			LifeCycleProbe Template = Create<LifeCycleProbe>();
			Template.gameObject.SetActive(false);
			UObjectPool Pool = Create<UObjectPool>();
			typeof(UObjectPool).GetField("prefabs", Hidden).SetValue(Pool, new[] { Template.gameObject });
			UObjectPool.instance = Pool;
			Pool.Initialize();
			int PrefabId = Template.name.GetHash();
			Assert.IsTrue(Pool.Contains(PrefabId));
			Assert.IsTrue(Pool.Contains(Template.name));
			UObject Target = UObject.Get(PrefabId, new Vector2(7, 8), false, Item => Item.ID = "runtime-test-id").GetComponent<UObject>();
			Assert.AreEqual(new Vector2(7, 8), (Vector2)Target.transform.position);
			Assert.AreSame(Target, UObject.GetUObject("runtime-test-id"));
			Target.name = "different runtime name";
			Target.Release(false);
			Assert.AreSame(Target, UObject.Get(Template.name, Vector2.zero, false).GetComponent<UObject>());
			Assert.Throws<KeyNotFoundException>(() => UObject.Get("missing-prefab".GetHash(), Vector2.zero));
			Assert.Throws<KeyNotFoundException>(() => UObject.Get("missing-prefab", Vector2.zero));
			Target.Release(false);
			Assert.AreSame(Target, UObject.Get(PrefabId, Vector2.zero, false).GetComponent<UObject>());
			Target.Release(false);
		}

		[Test]
		public void DuplicatePrefabKeysFailBeforePoolCreation() {
			LifeCycleProbe Template = Create<LifeCycleProbe>();
			UObjectPool Pool = Create<UObjectPool>();
			typeof(UObjectPool).GetField("prefabs", Hidden).SetValue(Pool, new[] { Template.gameObject, Template.gameObject });
			Assert.Throws<InvalidOperationException>(Pool.Initialize);
		}

		[Test]
		public void RapidReacquisitionTicksOnlyTheNewLifeCycleEntry() {
			LifeCycleProbe Target = Probe(out ProbeGetting Getting, out _);
			Begin(Target, true);
			Target.Release(false);
			Begin(Target, true);
			UObject.LifeCycleRoutine();
			Assert.AreEqual(Time.deltaTime, Getting.elapsed);
			Assert.AreEqual(1, Getting.Clears);
		}

		[Test]
		public void UiReturnsToItsOwnPoolAndUsesUnscaledTime() {
			UCanvas Canvas = Create<UCanvas>();
			GameObject Prefab = new("LifeCycle UI prefab", typeof(RectTransform), typeof(LifeCycleUiProbe));
			Objects.Add(Prefab);
			LifeCycleUiProbe Template = Prefab.GetComponent<LifeCycleUiProbe>();
			Template.actions = Array.Empty<UUIActionListItem>();
			Template.initialScale = Vector3.one;
			Template.initialPosition = new Vector2(12, 34);
			Template.openTime = 0.1f;
			Template.closeTime = 0.1f;
			Template.GetTask = MakeTask(Mark("get-task"));
			Template.ReleaseTask = MakeTask(Mark("release-task"));
			UUIPool Pool = Create<UUIPool>();
			UUIPool.instance = Pool;
			Pool.prefabList = new[] { Prefab };
			Pool.Initialize();
			Time.timeScale = 0;
			LifeCycleUiProbe Target = UUI.Get(Prefab.name, Canvas, false).GetComponent<LifeCycleUiProbe>();
			Assert.AreEqual(Time.unscaledDeltaTime, Target.lifeCycleDeltaTime);
			Target.Release(false);
			Assert.AreEqual(1, Target.Opens);
			Assert.AreEqual(1, Target.Closes);
			Assert.AreEqual(1, Target.Initializations);
			Assert.AreEqual(1, Target.Uninitializations);
			CollectionAssert.AreEqual(new[] { "get-task", "release-task" }, Target.Trace);
			UUI Reused = UUI.Get(Prefab.name, Canvas, false).GetComponent<UUI>();
			Assert.AreSame(Target, Reused);
			Assert.AreEqual(2, Target.Opens);
			Target.Release(false);
		}

		[Test]
		public void UiEffectsFinishThroughTheCentralRoutineWhilePaused() {
			UCanvas Canvas = Create<UCanvas>();
			GameObject Obj = new("Paused UI", typeof(RectTransform), typeof(LifeCycleUiProbe));
			Objects.Add(Obj);
			LifeCycleUiProbe Target = Obj.GetComponent<LifeCycleUiProbe>();
			Target.actions = Array.Empty<UUIActionListItem>();
			Target.canvas = Canvas;
			Target.initialScale = Vector3.one;
			Target.openTime = Target.closeTime = 0.03f;
			Time.timeScale = 0;
			Assert.Greater(Time.unscaledDeltaTime, 0, "The editor must provide an unscaled frame delta for this integration check.");
			Target.Get();
			for (int Pass = 0; Pass < 100 && !Target.isActive; Pass++) UObject.LifeCycleRoutine();
			Assert.IsTrue(Target.isActive);
			Target.Release();
			for (int Pass = 0; Pass < 100 && !Target.isReleased; Pass++) UObject.LifeCycleRoutine();
			Assert.IsTrue(Target.isReleased);
			Assert.AreEqual(1, Target.Opens);
			Assert.AreEqual(1, Target.Closes);
		}

		[Test]
		public void NewLifeCycleRegistrationsWaitForNextLifeCyclePass() {
			LifeCycleProbe First = Probe(out ProbeGetting Getting, out _);
			LifeCycleProbe Second = Probe(out ProbeGetting OtherGetting, out _);
			Getting.UpdateEffect = () => { Getting.UpdateEffect = null; Begin(Second, true); };
			Begin(First, true);
			UObject.LifeCycleRoutine();
			Assert.AreEqual(LifeCyclePhase.Getting, Second.lifeCycle.phase);
			Assert.AreEqual(0, OtherGetting.elapsed);
			UObject.LifeCycleRoutine();
			Assert.AreEqual(Time.deltaTime, OtherGetting.elapsed);
		}

		private ProjectileHitProbe HitProbe(out ProjectileTargetProbe Target, out EventManager Events) {
			Events = Create<EventManager>();
			EventManager.instance = Events;
			Events.Initialize();
			Create<GlobalObject>().Get(false);
			Target = Create<ProjectileTargetProbe>();
			Target.Get(false);
			Events.AddListener(UengSystem.Events.EventType.Entity_Hit, Target.Receive);
			ProjectileHitProbe Projectile = Create<ProjectileHitProbe>();
			Projectile.gameObject.AddComponent<Rigidbody2D>();
			Projectile.Get(false);
			return Projectile;
		}

		private static void DispatchHits(ProjectileHitProbe Projectile, EventManager Events) {
			object Processes = typeof(UObject).GetField("ProcessToUpdate", Hidden).GetValue(Projectile);
			Invoke(Projectile, "ExecuteProcesses", Processes);
			Events.EventRoutine();
		}

		[Test]
		public void ProjectileHitWaitsForDamageAndAllListenersBeforeReturning() {
			ProjectileHitProbe Projectile = HitProbe(out ProjectileTargetProbe Target, out EventManager Events);
			int Effects = 0;
			int Observations = 0;
			Events.AddListener(UengSystem.Events.EventType.Entity_Hit, Hit => {
				Assert.IsTrue(Hit.isValid);
				Assert.AreEqual(1, Target.Hits);
				Observations++;
			});
			Projectile.Hit(Target, () => { Effects++; Projectile.Release(false); });
			Projectile.FinishHits();
			Assert.IsTrue(Projectile.isActive, "Must wait until the queued send has actually run.");
			DispatchHits(Projectile, Events);
			Assert.AreEqual(7, Target.DamageTaken);
			Assert.AreEqual(1, Observations);
			Assert.AreEqual(0, Effects);
			Projectile.FinishHits();
			Assert.AreEqual(1, Effects);
			Assert.IsTrue(Projectile.isReleased);
		}

		[TestCase(false)]
		[TestCase(true)]
		public void ProjectileHitReturnsWithoutSuccessEffectForInvincibleOrReusedTarget(bool ReuseTarget) {
			ProjectileHitProbe Projectile = HitProbe(out ProjectileTargetProbe Target, out EventManager Events);
			int Effects = 0;
			Projectile.Hit(Target, () => Effects++);
			if (ReuseTarget) { Target.Release(false); Target.Get(false); }
			else Target.Invincible(true);
			DispatchHits(Projectile, Events);
			Projectile.FinishHits();
			Assert.AreEqual(0, Target.Hits);
			Assert.AreEqual(0, Effects);
			Assert.IsTrue(Projectile.isReleased);
		}

		[Test]
		public void PiercingProjectileProcessesDistinctTargetsOnceAndResumesVelocity() {
			ProjectileHitProbe Projectile = HitProbe(out ProjectileTargetProbe First, out EventManager Events);
			ProjectileTargetProbe Second = Create<ProjectileTargetProbe>();
			Second.Get(false);
			Events.AddListener(UengSystem.Events.EventType.Entity_Hit, Second.Receive);
			Vector2 Velocity = new(4, 2);
			Projectile.rigidbody2D.linearVelocity = Velocity;
			int Effects = 0;
			Projectile.Hit(First, () => Effects++, true);
			Projectile.Hit(First, () => Effects++, true);
			Projectile.Hit(Second, () => Effects++, true);
			DispatchHits(Projectile, Events);
			Projectile.FinishHits();
			Assert.AreEqual(1, First.Hits);
			Assert.AreEqual(1, Second.Hits);
			Assert.AreEqual(2, Effects);
			Assert.IsTrue(Projectile.isActive);
			Assert.AreEqual(Velocity, Projectile.rigidbody2D.linearVelocity);
		}

		[Test]
		public void ReusedProjectileIgnoresOldHitCallback() {
			ProjectileHitProbe Projectile = HitProbe(out ProjectileTargetProbe Target, out EventManager Events);
			int Effects = 0;
			Projectile.Hit(Target, () => Effects++);
			DispatchHits(Projectile, Events);
			Action OldCallback = Events.GetEvents((int)EventPriority.Hit)[0].GetData<HitData>().onHit;
			Projectile.Release(false);
			Projectile.Get(false);
			OldCallback();
			Projectile.FinishHits();
			Assert.AreEqual(0, Effects);
			Assert.IsTrue(Projectile.isActive);
		}

		[UnityTest]
		public IEnumerator ActiveRoutinesBeginOnNextFrameAndOldRegistrationsCannotDuplicateThem() {
			GameManager.instance = Create<GameManager>();
			LifeCycleProbe Target = Probe(out _, out _);
			Begin(Target, false);
			Target.Release(false);
			Begin(Target, false);
			int Frame = Time.frameCount;
			UObject.UpdateRoutine();
			UObject.LateUpdateRoutine();
			UObject.FixedUpdateRoutine();
			Assert.IsFalse(Target.Trace.Contains("routine"));
			for (int Attempt = 0; Attempt < 30 && Time.frameCount == Frame; Attempt++) yield return null;
			if (Time.frameCount == Frame) Assert.Ignore("Editor did not advance the Unity frame clock; run in Play Mode to verify this boundary.");
			UObject.UpdateRoutine();
			Assert.AreEqual(1, Target.Trace.Count(Item => Item == "routine"));
			Target.Release();
			UObject.UpdateRoutine();
			Assert.AreEqual(1, Target.Trace.Count(Item => Item == "routine"));
		}
	}
}
