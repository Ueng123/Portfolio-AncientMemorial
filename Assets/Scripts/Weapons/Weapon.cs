using UnityEngine;

namespace AncientMemorial.Weapons {
	public abstract class Weapon : ScriptableObject {

		public Sprite sprite;

		public abstract void OnPrimaryUse();
		public abstract void OnSecondaryUse();

		public abstract void Initialize();
		public abstract void Uninitialize();
	}
}