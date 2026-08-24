using System;
using AncientMemorial.Map;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UengSystem.VisualScripting.UValues.UVector2s {
	[Serializable]
	public class RandomPosition : UValue<Vector2> {
		protected override bool    getIsDynamic => true;

		[SerializeReference] [SubclassSelector]
		public UValue<float> ConstX;
		
		[SerializeReference] [SubclassSelector]
		public UValue<float> ConstY;

		protected override Vector2 getValue {
			get {
				Vector2 mapSize = MapManager.instance.GetMapSize();
				float   x       = ConstX?.value ?? Random.Range(-mapSize.x /2f, mapSize.x/2f);
				float   y       = ConstY?.value ?? Random.Range(0, mapSize.y);
				return new Vector2(x, y);
			}
		}
	}
}