using System;
using UengSystem.Tasks;
using UengSystem.Tasks.Logic;
using UengSystem.Tasks.Logic.UValues;
using UengSystem.Tasks.Logic.UValues.UBools;
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
		
		[SerializeReference][SubclassSelector]
		public UValue<Color> color;
		
		public override void Routine(ITaskable self) {
			if (setValue) ((USlider)component).SetValue(value.getValue);
			if (useValue) task.Execute(self);
		}
	}
}