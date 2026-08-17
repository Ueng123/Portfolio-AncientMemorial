using System;
using AncientMemorial.Map;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class MapSizeX : UValue<float> {
		protected override bool  getIsDynamic => true;
		protected override float getValue     => MapManager.instance.GetMapSize().x;
	}
}