using System.Collections.Generic;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;

namespace UengSystem.UI {
	public class InfoUUI : UUI {
		public static   InfoUUI                                  instance;
		public          float                                    infoDuration = 5f;
		public readonly List<(string text, StopWatch stopWatch)> infoData     = new List<(string text, StopWatch stopWatch)>();

		private void ApplyInfoText() {
			for (int i = 0; i < actions.Length; i++) {
				(string text, StopWatch stopWatch) info  = (i+1) > infoData.Count?("", n):infoData[^(i+1)];
				
				UTextAction textAction    = GetAction<UTextAction>(i);
				UText       textComponent = textAction.GetComponent<UText>();
				
				textAction.text          = new UPureString  { Text  = info.text };
				
				if (i == 0 && animator.GetCurrentAnimatorStateInfo(0).IsName("addInfo")) continue;
				float alpha = Mathf.Clamp(infoDuration - info.stopWatch.TryTock(infoDuration), 0f, 1f);
				textComponent.text.alpha = alpha;
			}
		}
		
		public void AddInfoMessage(string info) {
			StopWatch stopWatch = new StopWatch();
			stopWatch.Tick();
			
			infoData.Add((info, stopWatch));
			if (infoData.Count > actions.Length) {
				infoData.RemoveAt(0);
			}
			
			ApplyInfoText();
			
			animator.Play("addInfo", 0, 0);
		}
		
		private readonly StopWatch n = new StopWatch();

		protected override void LateRoutine() { ApplyInfoText(); }

		public override void OnOpen() {
			instance = this;
		}

		public override void OnClose() {
			instance = null;
		}
	}
}