using System;
using System.Collections;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.UAction {
    public class WaitAction : UAction {

       // 정적 프로퍼티
       public static int runningWaitActionCount = 0;

       // 인스턴스 프로퍼티
       public readonly  Func<bool> checkCondition;
       private readonly Action     actionToDelay;
       private readonly Action     actionOnCancel;
       
       private Coroutine process; // static 루프 대신 내 목숨줄만 관리한다
       public float startTime;
       public float timeOut;

       private IActionable executor;
       
       private WaitUntil waitUntil;

       // 인스턴스 메서드
       public WaitAction (Func<bool> checkCondition, Action actionToDelay, Action actionOnCancel = null, float timeOut = 0, IActionable executor = null) {
          this.executor = executor;
          this.checkCondition = checkCondition;
          this.actionToDelay  = actionToDelay;
          this.actionOnCancel = actionOnCancel;
          this.timeOut        = timeOut;

          waitUntil = new WaitUntil(() => checkCondition() || (timeOut != 0 && Time.time - startTime >= timeOut));
       }
       
       public WaitAction ExecuteWA() {
          if (Executing) return this;
          if (executor is Objects.UObject Owner && !Owner.canStartOwnedWork) return this;
          
          if (checkCondition.Invoke()) {
             actionToDelay();
             return this;
          }
          
          runningActionCount     += 1;
          runningWaitActionCount += 1;
          
          startTime = Time.time;
          Executing = true;
          
          Execute(executor);
          if (!Executing) return this;
          process = GlobalObject.instance.StartCoroutine(ActionEnumerator());
          return this;
       }

       public void DoneWA(bool stopCoroutine = true) {
          if (!Executing) return;
          
          runningActionCount     -= 1;
          runningWaitActionCount -= 1;
          
          if (stopCoroutine && process != null && GlobalObject.instance) GlobalObject.instance.StopCoroutine(process);
          process = null;
          Executing = false;
          executor?.UnregisterAction(this);
          actionToDelay.Invoke();
       }

       // 오버라이드 메서드
       public override void Done() { DoneWA(); }
       
       public override void Cancel() {
          if (!Executing) return;
          
          runningActionCount     -= 1;
          runningWaitActionCount -= 1;
          
          Executing = false;
          executor?.UnregisterAction(this);
          if (process != null) {
             if (GlobalObject.instance) GlobalObject.instance.StopCoroutine(process);
             process = null;
          }
          actionOnCancel?.Invoke();
       }

       protected override IEnumerator ActionEnumerator() {
          yield return waitUntil;
          
          if (timeOut != 0 && Time.time - startTime >= timeOut) {
             Cancel();
          } else {
             DoneWA(false);
          }
       }
    }
}
