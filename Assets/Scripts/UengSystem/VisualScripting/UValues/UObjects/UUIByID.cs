using System;
using UengSystem.UI;
using UnityEngine;

namespace UengSystem.VisualScripting.UValues.UObjects {
	[Serializable]
	public class UUIByID : UValue<UUI> {

		// 인스턴스 프로퍼티
		protected override bool getIsDynamic {
			get {
				ID.parent = this;
				return ID.isDynamic;
			}
		}
		
		[SerializeReference][SubclassSelector]
		public UValue<string> ID;

		protected override UUI getValue => UUI.GetUUI(ID.value);
	}
}