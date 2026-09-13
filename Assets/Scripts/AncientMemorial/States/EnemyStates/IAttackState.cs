namespace AncientMemorial.States.EnemyStates {
	public interface IAttackState {

		// 인스턴스 프로퍼티
		public float attackTime  { get; }
		public float attackSpeed { get; }

		// 인스턴스 메서드
		public float GetDelay(float percent);

		public bool isProgress(float percent);
	}
}