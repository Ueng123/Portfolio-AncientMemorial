namespace UengSystem.Objects {
	public interface IStoppable {
		public bool  stopped   { get; set; }
		public float DeltaTime { get; }

		public void Stop();
		public void Resume();
	}
}