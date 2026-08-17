using System;
using AncientMemorial.Entities;
using UengSystem.Objects;

namespace UengSystem.Logic.UValues.UObjects {
	[Serializable]
	public class UPlayerObject : UValue<UObject> {
		protected override bool    getIsDynamic => false;
		protected override UObject getValue     => Entity.player;
	}
}