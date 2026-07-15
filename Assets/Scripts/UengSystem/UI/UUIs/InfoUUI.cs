using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using NUnit.Framework.Internal;
using UengSystem.Logic.UValues.UColors;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Logic.UValues.UStrings;
using UengSystem.UI.UTexts;
using UengSystem.Utility;
using UnityEngine;
using Color = UnityEngine.Color;

namespace UengSystem.UI {
	public class InfoUUI : UUI {
		public static   InfoUUI                                  instance;
		public          float                                    infoDuration = 5f;
		public readonly List<(string text, StopWatch stopWatch)> infoData     = new List<(string text, StopWatch stopWatch)>();
		public void AddInfoMessage(string info) {
			StopWatch stopWatch = new StopWatch();
			stopWatch.Tick();
			
			infoData.Add((info, stopWatch));
		}

		private StopWatch n = new StopWatch();
		protected override void EarlyRoutine() {
			for (int i = 0; i < actions.Length; i++) {
				(string text, StopWatch stopWatch) info = (i+1) > infoData.Count?("", n):infoData[^(i+1)];

				float alpha = Mathf.Clamp(infoDuration - info.stopWatch.Tock(), 0f, 1f);
				((UTextAction)actions[i].action).text = new UPureString  { Text  = info.text };
				((UText)((UTextAction)actions[i].action).component).text.alpha = alpha;
			}
		}

		protected override void LateRoutine() { }

		protected override void FixedRoutine() { }

		public override void OnOpen() {
			instance = this;
		}

		public override void OnClose() {
			instance = null;
		}
	}
}