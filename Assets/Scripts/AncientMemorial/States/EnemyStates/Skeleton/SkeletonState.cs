using System.Linq;
using AncientMemorial.Entities;
using UnityEngine;

namespace UengSystem.States.EnemyStates.Skeleton {
	public abstract class SkeletonState : GroundEnemyState {
		protected static readonly int attacking = Animator.StringToHash("attacking");
		protected static readonly int attackF   = Animator.StringToHash("attackFrontEnd");
		protected static readonly int attackB   = Animator.StringToHash("attackBackEnd");
		protected static readonly int attack    = Animator.StringToHash("attack");
	}
}