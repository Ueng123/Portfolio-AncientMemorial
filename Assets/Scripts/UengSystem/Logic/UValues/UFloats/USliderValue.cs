using System;
using UengSystem.UI.USliders;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class USliderValue : UValue<float> {
		protected override bool  getIsDynamic => true;
		protected override float getValue     => USliderAction.sliderValueVariable.value;
	}
}