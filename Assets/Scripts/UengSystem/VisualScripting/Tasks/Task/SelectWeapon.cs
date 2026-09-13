using System;
using AncientMemorial.Weapons;

namespace UengSystem.VisualScripting.Tasks {
	[Serializable]
	public class SelectWeapon : TaskComponent {

		// 인스턴스 프로퍼티
		public int weaponID;

		// 오버라이드 메서드
		public override void Execute(ITaskable self) {
			Weapon.selectedWeaponID = weaponID;
		}
	}
}