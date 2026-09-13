using System;
using UengSystem.Objects;
using UengSystem.VisualScripting.UValues;
using UnityEngine;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class ConfigureTransform : TaskComponent {

		// 인스턴스 프로퍼티
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

		// 오버라이드 메서드
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