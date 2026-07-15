using System;
using System.Collections;
using UengSystem.Managers;
using UnityEngine;

namespace UengSystem.Utility {
	public class ExclusiveAction {
		public bool  executing;
		public float changeDelay;

		private MonoBehaviour coroutineRunner;
		private IEnumerator   enumerator;

		private Action onCancel;
		private Action onDone;
		
		private Coroutine mainCoroutine;
		private Coroutine coroutineHandler;
		
		public ExclusiveAction(IEnumerator enumerator, Action onCancel, Action onDone, float changeDelay, MonoBehaviour coroutineRunner = null) {
			this.enumerator      = enumerator;
			this.onCancel        = onCancel;
			this.onDone          = onDone;
			this.coroutineRunner = coroutineRunner ?? GlobalCoroutineRunner.instance;
			this.changeDelay     = changeDelay;
		}

		public void Execute() {
			if (executing) return;

			executing        = true;
			coroutineHandler = coroutineRunner.StartCoroutine(Run());
		}
		
		public void Cancel(Action afterCancel = null) {
			if (!executing) return;
			executing = false;
			
			if (mainCoroutine!=null) coroutineRunner.StopCoroutine(mainCoroutine);
			if (coroutineHandler!=null) coroutineRunner.StopCoroutine(coroutineHandler);
			onCancel.Invoke();
		}

		private IEnumerator Run() {
			mainCoroutine = coroutineRunner.StartCoroutine(enumerator);
			yield return mainCoroutine;
			executing = false;
			onDone.Invoke();
		}
	}
}