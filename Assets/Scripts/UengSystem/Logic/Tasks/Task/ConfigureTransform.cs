using System;
using UengSystem.Logic.UValues;
using UengSystem.Objects;
using UnityEngine;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class ConfigureTransform : TaskComponent {

		public bool thisObject;
		[SerializeReference] [SubclassSelector]
		public UValue<UObject> obj;
		
		public bool useLocalPosition;
		[SerializeReference] [SubclassSelector]
		public UValue<Vector2> pos;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> rot;
		
		[SerializeReference] [SubclassSelector]
		public UValue<Vector2> size;
		
		public override void Execute(ITaskable self) {
			UObject uObject = thisObject ? (UObject)self : obj.value;
			
			if (pos!=null) {
				if (useLocalPosition) uObject.transform.position      = pos.value;
				else                  uObject.transform.localPosition = pos.value;
			}
			if (rot  !=null) uObject.transform.rotation       = Quaternion.Euler(0, 0, rot.value);
			if (size !=null) uObject.transform.localScale = size.value;
		}
	}
}