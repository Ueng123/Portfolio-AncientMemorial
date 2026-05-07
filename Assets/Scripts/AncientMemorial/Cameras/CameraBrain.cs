using UengSystem.Objects;
using Unity.Cinemachine;

namespace AncientMemorial.Cameras {
	public class CameraBrain : AdvancedUObject {
		public static CinemachineCamera currCamera;
		public        CinemachineBrain  cameraBrain;
		
		public static void ChangeCamera(CinemachineCamera cam) {
			cam.Priority.Value        = 1;
			currCamera.Priority.Value = 0;
			currCamera                = cam;
		}

		protected override void EarlyRoutine() {
		}

		protected override void Routine() {
		}

		protected override void LateRoutine() {
			
		}

		protected override void FixedRoutine() {
		}

		public override void OnGet() {
			
		}

		public override void OnRelease() {
			
		}
	}
}