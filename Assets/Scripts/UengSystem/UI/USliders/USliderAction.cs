using System;
using UengSystem.Tasks;
using UengSystem.Tasks.Logic;
using UengSystem.Tasks.Logic.Value;
using UnityEngine;

namespace UengSystem.UI.USliders {
	[Serializable]
	public class USliderAction : UUIAction {
		public bool setValue;
		[SerializeReference] [SubclassSelector]
		public UNumber value;

		public bool useValue;
		[SerializeReference][SubclassSelector]
		public ConditionalTask task;
		
		public override void Routine(ITaskable self) {
			if (setValue) ((USlider)component).SetValue(value.GetValue());
			if (useValue) task.Execute(self);
		}
	}
}