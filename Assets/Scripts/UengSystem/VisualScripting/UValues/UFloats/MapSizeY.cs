using System;
using AncientMemorial.Map;

namespace UengSystem.VisualScripting.UValues.UFloats {
	[Serializable]
	public class MapSizeY : UValue<float> {

		// 인스턴스 프로퍼티
		protected override bool  getIsDynamic => true;
		protected override float getValue     => MapManager.instance.GetMapSize().y;
	}
}