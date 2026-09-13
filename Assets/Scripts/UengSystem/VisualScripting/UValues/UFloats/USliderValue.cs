using System;
using UengSystem.UI.USliders;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class USliderValue : UValue<float> {

		// 인스턴스 프로퍼티
		protected override bool  getIsDynamic => true;
		protected override float getValue     => USliderAction.sliderValueVariable.value;
	}
}