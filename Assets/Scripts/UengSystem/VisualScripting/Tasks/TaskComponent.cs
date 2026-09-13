using System;
using System.Collections;
using System.Collections.Generic;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public abstract class TaskComponent : InspectorItem {
		private readonly List<Coroutine> runningCoroutines = new();

		public Coroutine StartCoroutine(IEnumerator enumerator) {
			Coroutine coroutine = GlobalObject.instance.StartCoroutine(enumerator);
			runningCoroutines.Add(coroutine);
			return coroutine;
		}

		public void StopCoroutine(Coroutine coroutine) {
			if (coroutine == null) return;
			GlobalObject.instance.StopCoroutine(coroutine);
			runningCoroutines.Remove(coroutine);
		}

		public void StopAllCoroutines() {
			foreach (Coroutine coroutine in runningCoroutines) {
				if (coroutine != null) GlobalObject.instance.StopCoroutine(coroutine);
			}
			runningCoroutines.Clear();
		}

		public abstract void Execute(ITaskable self);
	}
}
