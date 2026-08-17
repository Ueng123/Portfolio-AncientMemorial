using System;
using UnityEngine;

namespace UengSystem.Logic.UValues.UStrings {
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

		protected override string getValue {
			get {
				float dt = targetTime.value - Time.time;
				float a  = Mathf.Pow(10, saveFloatDigit);
				return dt <= 0 ? textOnDone.value : $"{Mathf.Round(dt*a)/a}{unit.value}";
			}
		}
	}
}