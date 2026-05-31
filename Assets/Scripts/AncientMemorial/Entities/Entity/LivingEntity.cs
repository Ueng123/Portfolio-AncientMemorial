using UengSystem.Utility;

namespace AncientMemorial.Entities {
	public class LivingEntity : Entity {
		
		public static BufferedList<Entity> livingEntities = new BufferedList<Entity>();

		public override void Initialize() {
			base.Initialize();
			livingEntities.Add(this);
		}
		
		public override void OnRelease() {
			base.OnRelease();
			livingEntities.Remove(this);
		}

		protected override void Routine() {
			
		}

		protected override void Death() {
			
		}
	}
}