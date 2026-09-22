using AncientMemorial.Map;
using UengSystem.Managers;
using UengSystem.SaveDatas.SettingDatas;
using Unity.Cinemachine;
using UnityEngine;

namespace AncientMemorial.Cameras {
    public class CameraManager : Manager<CameraManager> {

       // 인스턴스 프로퍼티
       public UCamera          uCamera;
       public CinemachineBrain cameraBrain;

       public float currentShakeGain { get; private set; }
       public float currentZoomOffset { get; private set; }

       private float ShakeGain;
       private float ShakeSpeed;
       private bool  IsShaking;

       private float ZoomOffset;
       private float ZoomSpeed;
       private bool  IsZooming;

       // 인스턴스 메서드
       public void Initialize(UCamera TargetCamera) {
          uCamera = TargetCamera;
          cameraBrain ??= GetComponent<CinemachineBrain>();

          ShakeGain         = 0f;
          ShakeSpeed        = 0f;
          IsShaking         = false;
          currentShakeGain  = 0f;
          ZoomOffset        = 0f;
          ZoomSpeed         = 0f;
          IsZooming         = false;
          currentZoomOffset = 0f;

          uCamera.cameraManager  = this;
          uCamera.mapManager     = MapManager.instance;
          uCamera.noiseComponent = uCamera.cam
             ? uCamera.cam.GetComponent<CinemachineBasicMultiChannelPerlin>()
             : null;

          base.Initialize();
       }

       public void ShakeLerp(float Intensity, float Speed) {
          ShakeGain  = Intensity;
          ShakeSpeed = Speed;
          IsShaking  = true;
       }

       public void ZoomLerp(float ZoomSize, float Speed = 10f) {
          ZoomOffset = Mathf.Min(ZoomOffset + ZoomSize, 2);
          ZoomSpeed  = Speed;
          IsZooming  = true;
       }

       private float EvaluateShakeGain() {
          if (!IsShaking) return 0;
          ShakeGain = Mathf.Lerp(ShakeGain, 0, Time.deltaTime * ShakeSpeed);

          if (Mathf.Abs(ShakeGain) < 0.01f) {
             ShakeGain = 0;
             IsShaking = false;
          }

          if (!Setting.GetBool(SettingType.ShakeFX)) return 0;
          return ShakeGain;
       }

       private float EvaluateZoomOffset() {
          if (!IsZooming) return 0;
          ZoomOffset = Mathf.Lerp(ZoomOffset, 0, Time.deltaTime * ZoomSpeed);

          if (Mathf.Abs(ZoomOffset) < 0.01f) {
             ZoomOffset = 0;
             IsZooming  = false;
          }

          if (!Setting.GetBool(SettingType.ZoomFX)) return 0;
          return ZoomOffset;
       }

       // 오버라이드 메서드
       public override void ManagerRoutine() {
          currentShakeGain  = EvaluateShakeGain();
          currentZoomOffset = EvaluateZoomOffset();
       }
    }
}
