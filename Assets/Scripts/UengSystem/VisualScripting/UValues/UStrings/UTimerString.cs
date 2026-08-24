using System;
using System.Text;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UStrings {
	[Serializable]
	public class UTimerString : UValue<string> {
		protected override bool   getIsDynamic => true;

		[SerializeReference][SubclassSelector]
		public UValue<float> targetTime;

		public int saveFloatDigit;
		
		[SerializeReference][SubclassSelector]
		public UValue<string> textOnDone;
		
		[SerializeReference][SubclassSelector]
		public UValue<string> unit;

		private readonly StringBuilder _stringBuilder = new StringBuilder(16);

		private float? currT = null;
		private string cache = "";
		private string GetString() {
			float dt = targetTime.value - Time.time;
			if (dt <= 0) return textOnDone.value;
			
			float a = Mathf.Pow(10, saveFloatDigit);
			float t = Mathf.Round(dt * a) / a;
			if (currT == null || !Mathf.Approximately(t, currT.Value)) {
				_stringBuilder.Clear();
				_stringBuilder.Append(t);
				_stringBuilder.Append(unit.value);
				cache = _stringBuilder.ToString();
			}
			
			return cache;
		}
		
		protected override string getValue => GetString();
	}
}