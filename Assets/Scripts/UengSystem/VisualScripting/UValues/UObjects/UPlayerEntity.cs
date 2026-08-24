using System;
using AncientMemorial.Entities;

namespace UengSystem.VisualScripting.UValues.UObjects {
	[Serializable]
	public class UPlayerEntity : UValue<Entity> {
		protected override bool    getIsDynamic => false;
		protected override Entity getValue     => Entity.player;
	}
}