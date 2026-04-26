using System;
using UengSystem.Logic.Tasks;
using UengSystem.Logic.UValues;
using UnityEngine;

namespace UengSystem.UI.USliders {
	[Serializable]
	public class USliderAction : UUIAction {
		public bool setValue;
		[SerializeReference] [SubclassSelector]
		public UValue<float> value;

		public bool useValue;
		[SerializeReference][SubclassSelector]
		public ConditionalTask task;

		public bool setColor;
		[SerializeReference][SubclassSelector]
		public UValue<Color> color;
		
		public override void Routine(ITaskable self) {
			if (setValue) ((USlider)component).SetValue(value.value);
			if (useValue) task.Execute(self);
			if (setColor) ((USlider)component).SetColor(color.value);
		}
	}
}