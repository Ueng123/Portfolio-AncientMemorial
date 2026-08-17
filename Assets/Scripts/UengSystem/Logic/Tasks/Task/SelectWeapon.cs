using System;
using AncientMemorial.Weapons;

namespace UengSystem.Logic.Tasks {
	[Serializable]
	public class SelectWeapon : TaskComponent {
		public int weaponID;
		
		public override void Execute(ITaskable self) {
			Weapon.selectedWeaponID = weaponID;
		}
	}
}