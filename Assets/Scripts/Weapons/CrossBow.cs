using AncientMemorial.Entities;
using AncientMemorial.ObjectPool;
using UnityEngine;

namespace AncientMemorial.Weapons {
	[CreateAssetMenu(fileName = "Crossbow", menuName = "Weapon/Crossbow")]
	public class CrossBow : Weapon {
		private Player player;
		
		public override void OnPrimaryUse() {
			// 화살 발싸
			GameObject arrow = AMObjectPool.instance.Get("Arrow", player.transform.position);
			
		}

		public override void OnSecondaryUse() {
			// 뭐 할꺼 없스샘
		}
		
		public override void Initialize() {
			player = Entity.player;
		}

		public override void Uninitialize() {
			// UI 제거
		}
	}
}