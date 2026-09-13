using System;
using AncientMemorial.Entities;
using UengSystem.Objects;

namespace UengSystem.VisualScripting.UValues.UObjects {
	[Serializable]
	public class UPlayerObject : UValue<UObject> {

		// 인스턴스 프로퍼티
		protected override bool    getIsDynamic => false;
		protected override UObject getValue     => Entity.player;
	}
}