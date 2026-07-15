using System;
using System.Collections;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Utility {
    public class WaitAction {
       public readonly  Func<bool> checkCondition;
       private readonly Action     actionToDelay;
       private readonly Action     actionOnCancel;
       
       private Coroutine _runningCoroutine; // static 루프 대신 내 목숨줄만 관리한다
       public bool  Executing { get; private set; }
       public float startTime;
       public float timeOut;
       
       public WaitAction(Func<bool> checkCondition, Action actionToDelay, Action actionOnCancel = null, float timeOut = 0) {
          this.checkCondition = checkCondition;
          this.actionToDelay  = actionToDelay;
          this.actionOnCancel = actionOnCancel;
          this.timeOut        = timeOut;
       }

       public void Execute() {
          if (Executing) return;
          
          if (checkCondition.Invoke()) {
             actionToDelay();
             return;
          }
          
          startTime = Time.time;
          Executing = true;
          
          _runningCoroutine = GlobalCoroutineRunner.instance.StartCoroutine(WaitRoutine());
       }

       public void Cancel() {
          if (!Executing) return;
          
          Executing = false;
          if (_runningCoroutine != null) {
             GlobalCoroutineRunner.instance.StopCoroutine(_runningCoroutine);
             _runningCoroutine = null;
          }
          actionOnCancel?.Invoke();
       }

       private IEnumerator WaitRoutine() {
          yield return new WaitUntil(() => {
             if (timeOut != 0 && Time.time - startTime >= timeOut) return true;
             return checkCondition();
          });
          
          if (timeOut != 0 && Time.time - startTime >= timeOut) {
             Cancel();
          } else {
             Executing = false;
             actionToDelay.Invoke();
          }
       }
    }
}