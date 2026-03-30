using AncientMemorial;
using AncientMemorial.AMObjects;
using Unity.Cinemachine;

namespace AncientMemorial.Cameras {
	public class CameraBrain : AdvancedAMObject {
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
	}
}