namespace UengSystem.Objects {
	public interface IStoppable {
		public bool stopped { get; set; }

		public void Stop();
		public void Resume();
	}
}