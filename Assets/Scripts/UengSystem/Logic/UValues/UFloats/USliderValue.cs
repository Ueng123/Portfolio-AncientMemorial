using System;
using UengSystem.UI.UInputFields;
using UengSystem.UI.USliders;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class USliderValue : UValue<float> {
		public override bool  getIsDynamic => true;
		public override float getValue     => USliderAction.sliderValueVariable.value;
	}
}