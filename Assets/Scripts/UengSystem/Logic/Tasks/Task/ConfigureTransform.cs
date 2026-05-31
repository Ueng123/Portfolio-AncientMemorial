using System;
using UengSystem.Logic.UValues;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class ConfigureTransform : TaskComponent {
		[SerializeReference] [SubclassSelector]
		public UValue<UObject> obj;

		public bool changePosition;
		public bool useLocalPosition;
		[SerializeReference] [SubclassSelector]
		public UValue<Vector2> pos;
		
		public bool changeRotation;
		[SerializeReference] [SubclassSelector]
		public UValue<float> rot;
		
		public bool changeSize;
		[SerializeReference] [SubclassSelector]
		public UValue<Vector2> size;
		
		public override void Execute(ITaskable self) {
			if (changePosition) {
				if (useLocalPosition) obj.value.transform.position      = pos.value;
				else                  obj.value.transform.localPosition = pos.value;
			}

			if (changeRotation) obj.value.transform.rotation      = Quaternion.Euler(0, 0, rot.value);
			if (changeSize)     obj.value.transform.localScale = size.value;
		}
	}
}