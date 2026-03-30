using System;
using AncientMemorial.Cameras;
using AncientMemorial.Map;

namespace AncientMemorial.Tasks {
	[Serializable]
	public class SetCameraSetting : Task {

		public bool  changeLensOffset;
		public float TargetCamLensOffset;

		public bool            changeCameraAlign;
		public CameraAlignType TargetAlignTypeX;
		public CameraAlignType TargetAlignTypeY;

		public bool             changeCameraSizing;
		public CameraSizingType SizingType;
		
		public override void Execute(ITaskable self) {
			MapManager.instance.CamLensOffset = TargetCamLensOffset;
		}
	}
}