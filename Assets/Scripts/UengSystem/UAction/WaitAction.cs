using System;
using System.Collections;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Utility {
    public class WaitAction : UAction {
       public static int runningWaitActionCount = 0;
       
       public readonly  Func<bool> checkCondition;
       private readonly Action     actionToDelay;
       private readonly Action     actionOnCancel;
       
       private Coroutine process; // static 루프 대신 내 목숨줄만 관리한다
       public float startTime;
       public float timeOut;

       private IActionable executor;
       
       private WaitUntil waitUntil;
       
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
          
          if (checkCondition.Invoke()) {
             actionToDelay();
             return this;
          }
          
          runningActionCount     += 1;
          runningWaitActionCount += 1;
          
          startTime = Time.time;
          Executing = true;
          
          Execute(executor);
          process = CoroutineRunner.instance.StartCoroutine(ActionEnumerator());
          return this;
       }

       public override void Done() { DoneWA(); }

       public void DoneWA(bool stopCoroutine = true) {
          if (!Executing) return;
          
          runningActionCount     -= 1;
          runningWaitActionCount -= 1;
          
          if (stopCoroutine) CoroutineRunner.instance.StopCoroutine(process);
          Executing = false;
          actionToDelay.Invoke();
       }
       
       public override void Cancel() {
          if (!Executing) return;
          
          runningActionCount     -= 1;
          runningWaitActionCount -= 1;
          
          Executing = false;
          if (process != null) {
             CoroutineRunner.instance.StopCoroutine(process);
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