using System;
using AncientMemorial.Map;

namespace UengSystem.Logic.UValues.UFloats {
	[Serializable]
	public class MapSizeY : UValue<float> {
		protected override bool  getIsDynamic => true;
		protected override float getValue     => MapManager.instance.GetMapSize().y;
	}
}