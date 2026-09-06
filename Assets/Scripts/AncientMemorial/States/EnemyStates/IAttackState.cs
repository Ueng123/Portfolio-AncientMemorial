namespace AncientMemorial.States.EnemyStates {
	public interface IAttackState {
		public float attackTime  { get; }
		public float attackSpeed { get; }

		public float GetDelay(float percent);

		public bool isProgress(float percent);
	}
}