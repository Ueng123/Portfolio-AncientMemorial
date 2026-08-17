using System;
using System.Collections;
using System.Collections.Generic;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public abstract class TaskComponent {
		private List<Coroutine> _runningCoroutines = new List<Coroutine>();
		public Coroutine StartCoroutine(IEnumerator enumerator) {
			Coroutine coroutine = CoroutineRunner.instance.StartCoroutine(enumerator);
			_runningCoroutines.Add(coroutine);
			return coroutine;
		}

		public void StopCoroutine(Coroutine coroutine) {
			CoroutineRunner.instance.StopCoroutine(coroutine);
			_runningCoroutines.Remove(coroutine);
		}

		public void StopAllCoroutines() {
			foreach (Coroutine coroutine in _runningCoroutines) {
				CoroutineRunner.instance.StopCoroutine(coroutine);
			}
		}
		
		public abstract void Execute(ITaskable self);
	}
}