using System.Linq;
using AncientMemorial.Entities;
using UnityEngine;

namespace UengSystem.States.EnemyStates.Skeleton {
	public abstract class SkeletonState : GroundEnemyState {
		protected string[] footstepSpriteNames;
		protected override bool     isFootstep => footstepSpriteNames.Contains(enemy.spriteRenderer.sprite.name);
		
		private static string[] footstepNames = { "footstep1", "footstep2", "footstep3" };
		protected override string footstepSoundName => footstepNames[Random.Range(0, footstepNames.Length)];
		protected override Entity target => Entity.player;
	}
}