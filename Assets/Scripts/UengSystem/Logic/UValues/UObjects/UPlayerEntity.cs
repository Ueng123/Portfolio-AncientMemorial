using System;
using AncientMemorial.Entities;

namespace UengSystem.Logic.UValues.UObjects {
	[Serializable]
	public class UPlayerEntity : UValue<Entity> {
		protected override bool    getIsDynamic => false;
		protected override Entity getValue     => Entity.player;
	}
}