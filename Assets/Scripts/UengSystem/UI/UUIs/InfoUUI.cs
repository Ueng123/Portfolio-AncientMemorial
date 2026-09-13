using System.Collections.Generic;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UengSystem.VisualScripting.UValues.UStrings;
// using UengSystem.VisualScripting.UValues.UStrings;
using UnityEngine;

namespace UengSystem.UI.UUIs {
	public class InfoUUI : UUI {

		// 정적 프로퍼티
		public static   InfoUUI                                  instance;

		// 인스턴스 프로퍼티
		public          float                                    infoDuration = 5f;
		public readonly List<(string text, StopWatch stopWatch)> infoData     = new List<(string text, StopWatch stopWatch)>();
		
		private readonly StopWatch n = new StopWatch();

		// 인스턴스 메서드
		private void ApplyInfoText() {
			for (int i = 0; i < actions.Length; i++) {
				(string text, StopWatch stopWatch) info  = (i+1) > infoData.Count?("", n):infoData[^(i+1)];
				
				UTextAction textAction    = GetAction<UTextAction>(i);
				UText       textComponent = textAction.GetComponent<UText>();
				
				textAction.text          = new UPureString  { pureValue  = info.text };
				
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

		// 오버라이드 메서드
		protected override void LateRoutine() { ApplyInfoText(); }

		public override void OnOpen() {
			instance = this;
		}

		public override void OnClose() {
			instance = null;
		}
	}
}