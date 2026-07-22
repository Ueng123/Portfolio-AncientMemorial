using System;
using System.Collections;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Utility {
    public class WaitAction : UAction {
       public readonly  Func<bool> checkCondition;
       private readonly Action     actionToDelay;
       private readonly Action     actionOnCancel;
       
       private Coroutine process; // static 루프 대신 내 목숨줄만 관리한다
       public float startTime;
       public float timeOut;

       private IActionable executor;
       
       public WaitAction (Func<bool> checkCondition, Action actionToDelay, Action actionOnCancel = null, float timeOut = 0, IActionable executor = null) {
          this.executor = executor;
          this.checkCondition = checkCondition;
          this.actionToDelay  = actionToDelay;
          this.actionOnCancel = actionOnCancel;
          this.timeOut        = timeOut;
       }
       
       public WaitAction ExecuteWA() {
          if (Executing) return this;
          
          if (checkCondition.Invoke()) {
             actionToDelay();
             return this;
          }
          
          startTime = Time.time;
          Executing = true;
          
          Execute(executor);
          process = GlobalCoroutineRunner.instance.StartCoroutine(ActionEnumerator());
          return this;
       }

       public override void Done() { DoneWA(); }

       public void DoneWA(bool stopCoroutine = true) {
          if (!Executing) return;
          
          if (stopCoroutine) GlobalCoroutineRunner.instance.StopCoroutine(process);
          Executing = false;
          actionToDelay.Invoke();
       }
       
       public override void Cancel() {
          if (!Executing) return;
          
          Executing = false;
          if (process != null) {
             GlobalCoroutineRunner.instance.StopCoroutine(process);
             process = null;
          }
          actionOnCancel?.Invoke();
       }

       protected override IEnumerator ActionEnumerator() {
          yield return new WaitUntil(() => {
             if (timeOut != 0 && Time.time - startTime >= timeOut) return true;
             return checkCondition();
          });
          
          if (timeOut != 0 && Time.time - startTime >= timeOut) {
             Cancel();
          } else {
             DoneWA(false);
          }
       }
    }
}