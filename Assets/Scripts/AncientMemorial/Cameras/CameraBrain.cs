using UengSystem.Objects;
using UengSystem.SaveDatas.SettingDatas;
using Unity.Cinemachine;
using UnityEngine;

namespace AncientMemorial.Cameras {
    public class CameraBrain : UObject {

       // 정적 프로퍼티
       public static CameraBrain                        instance;

       // 인스턴스 프로퍼티
       public        CinemachineCamera                  currCamera;
       private       CinemachineBasicMultiChannelPerlin noiseComponent;
       public        CinemachineBrain                   cameraBrain;
       
       // [교정] 메인 카메라 컴포넌트 참조 장전
       public        MainCamera                         mainCamera;
       
       // 고정값이 아닌 현재 오프셋 변화량만 추적한다
       private float zoomOffset;
       private float zoomSpeed;
       private bool  isZooming;

       private float shakeGain;
       private float shakeSpeed;
       private bool  isShaking;

       // 인스턴스 메서드
       public void ChangeCameraTransform(Transform t) {
          mainCamera.mainCameraTransform = t;
       }
       
       public void ShakeLerp(float intensity, float speed) {
          if (!noiseComponent) return;
          shakeGain = intensity;
          shakeSpeed = speed;
          isShaking  = true;
       }
       
       public void ZoomLerp(float zoomSize, float speed = 10f) {
          if (!currCamera || !mainCamera) return;
          zoomOffset   = Mathf.Min(zoomOffset+zoomSize, 2);
          zoomSpeed    = speed;
          isZooming    = true;
       }

       private float EvaluateShakeGain() {
          if (!isShaking) return 0;
          shakeGain = Mathf.Lerp(shakeGain, 0, Time.deltaTime * shakeSpeed);
          
          if (Mathf.Abs(shakeGain) < 0.01f) {
             shakeGain = 0;
             isZooming  = false;
          }
          
          if (!Setting.GetBool(SettingType.ShakeFX)) return 0;
          return shakeGain;
       }
       
       private void ShakeLerpRoutine() {
          float currShakeGain = EvaluateShakeGain();
          
          noiseComponent.AmplitudeGain = currShakeGain;
       }

       private float EvaluateZoomOffset() {
          if (!isZooming) return 0;
          zoomOffset = Mathf.Lerp(zoomOffset, 0, Time.deltaTime * zoomSpeed);
          
          if (Mathf.Abs(zoomOffset) < 0.01f) {
             zoomOffset = 0;
          }
          
          if (!Setting.GetBool(SettingType.ZoomFX)) return 0;
          return zoomOffset;
       }
       
       private void ZoomLerpRoutine() {
          float currZoomOffset = EvaluateZoomOffset();
          
          LensSettings lens = currCamera.Lens;
          lens.OrthographicSize = mainCamera.BaseLensSize + currZoomOffset;
          currCamera.Lens       = lens;
       }

       // 오버라이드 메서드
       public override void Initialize() {
          instance = this;
          noiseComponent = currCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
          
          zoomOffset   = 0f;
          
          base.Initialize();
       }

       protected override void EarlyRoutine() { }

       protected override void Routine() { }
       
       protected override void LateRoutine() {
          if (!currCamera || !mainCamera) return;
          
          ShakeLerpRoutine();
          ZoomLerpRoutine();
       }

       protected override void FixedRoutine() { }
    }
}