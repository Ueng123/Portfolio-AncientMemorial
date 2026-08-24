using System;
using AncientMemorial;
using UengSystem.Objects;
using UengSystem.Utility;
using UengSystem.VisualScripting.Tasks;
using UengSystem.VisualScripting.UValues;
using UengSystem.VisualScripting.UValues.UFloats;

// using UengSystem.VisualScripting.UValues.UFloats;
using UnityEngine;

namespace UengSystem.UI.USliders {
	[Serializable]
	public class USliderAction : UUIAction {
		private int SLIDER_VALUE = "SliderValue".GetHash();
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> initialValue;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> value;
		
		[SerializeReference][SubclassSelector]
		public UValue<Color> color;

		public USliderTaskCondition condition;
		public Task                taskOnValueChanged;

		private USlider     slider;
		public static UPureFloat sliderValueVariable;
		
		public override void Initialize(UObject self) {
			slider = GetComponent<USlider>();
			slider.Initialize();
			
			slider.SetValue(initialValue?.value??0);
			slider.valueChanged = false;
			
			// if (GameManager.UValueFloatVariables.TryGetValue("SliderValue", out UValue<float> v)) {
			// 	sliderValueVariable = (UPureFloat)v;
			// } 
			// else {
			// 	GameManager.UValueFloatVariables["SliderValue"] = new UPureFloat {
			// 		dynamicType = DynamicType.Dynamic,
			// 		pureValue    = 0
			// 	};
			//
			// 	sliderValueVariable = (UPureFloat)GameManager.UValueFloatVariables["SliderValue"];
			// }
			sliderValueVariable ??= UPureFloat.GetPureValue(SLIDER_VALUE).SetDynamicCache(DynamicType.Dynamic).To<UPureFloat>();
		}

		public override void Uninitialize(UObject self) {
			component.Uninitialize();
			
			if (value !=null) slider.SetValue(value.value);
			if (color !=null) slider.SetColor(color.value);
		}
		
		public override void Routine(UObject self) {
			if (value!=null) slider.SetValue(value.value);
			if (color!=null) slider.SetColor(color.value);
			
			sliderValueVariable.pureValue = slider.GetValue();
			
			switch (condition) {
				case USliderTaskCondition.Never:
					break;
				case USliderTaskCondition.OnValueChanged:
					if (slider.valueChanged) {
						slider.valueChanged = false;
						taskOnValueChanged.Execute(self);
					}
					break;
				case USliderTaskCondition.Always:
					taskOnValueChanged.Execute(self);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}