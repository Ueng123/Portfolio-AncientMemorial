using System;
using AncientMemorial.Entities;

namespace UengSystem.VisualScripting.UValues.UObjects {
	[Serializable]
	public class UPlayerEntity : UValue<Entity> {

		// 인스턴스 프로퍼티
		protected override bool    getIsDynamic => false;
		protected override Entity getValue     => Entity.player;
	}
}