using System;
using AncientMemorial;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UengSystem.Logic.UValues.UFloats;
using UengSystem.Objects;
using UnityEngine;
using UnityEngine.Serialization;

namespace UengSystem.UI.USliders {
	[Serializable]
	public class USliderAction : UUIAction {
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> initialValue;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> value;
		
		[SerializeReference][SubclassSelector]
		public UValue<Color> color;

		public USliderTaskCondition condition;
		public Task                taskOnValueChanged;

		private USlider     slider;
		public static UPureNumber sliderValueVariable;
		
		public override void Initialize(UObject self) {
			component.Initialize();
			slider = (USlider)component;
			
			if (initialValue!=null) slider.SetValue(initialValue.value);
			
			if (GameManager.UValueFloatVariables.TryGetValue("SliderValue", out UValue<float> v)) {
				sliderValueVariable = (UPureNumber)v;
			} 
			else {
				GameManager.UValueFloatVariables["SliderValue"] = new UPureNumber {
					dynamicType = DynamicType.Dynamic,
					number = 0
				};
			}
		}

		public override void Routine(UObject self) {
			if (value!=null) slider.SetValue(value.value);
			if (color!=null) slider.SetColor(color.value);
			
			sliderValueVariable.number = slider.GetValue();
			
			switch (condition) {
				case USliderTaskCondition.Never:
					break;
				case USliderTaskCondition.OnValueChanged:
					if (slider.valueChanged) taskOnValueChanged.Execute(self);
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