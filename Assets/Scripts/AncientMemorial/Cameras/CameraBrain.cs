using UengSystem.Objects;
using UengSystem.Utility;
using Unity.Cinemachine;
using UnityEngine;

namespace AncientMemorial.Cameras {
    public class CameraBrain : AdvancedUObject {
       
       public static CameraBrain                        instance;
       
       public        CinemachineCamera                  currCamera;
       private       CinemachineBasicMultiChannelPerlin _noiseComponent;
       public        CinemachineBrain                   cameraBrain;
       
       // [교정] 메인 카메라 컴포넌트 참조 장전
       public        MainCamera                         mainCamera;
       
       // 고정값이 아닌 현재 오프셋 변화량만 추적한다
       private       float                              _zoomOffset; 
       private       float                              _targetOffset;
       private       float                              _zoomSpeed;
       private       bool                               _isZooming;
       
       public override void Initialize() {
          instance = this;
          _noiseComponent = currCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
          
          _zoomOffset   = 0f;
          _targetOffset = 0f;
          
          base.Initialize();
       }

       public void ChangeCameraTransform(Transform t) {
          mainCamera.mainCameraTransform = t;
       }

       private DelayedAction _shakeTimer;
       private bool          _shakeLerp;
       private float         _shakeLerpSpeed;

       public float totalIntensity;
       public void ShakeLerp(float intensity, float speed) {
          if (!_noiseComponent) return;

          _shakeTimer?.Cancel();
          _noiseComponent.AmplitudeGain = intensity;

          _shakeLerp = true;
          _shakeLerpSpeed = speed;
       }
       
       public void ZoomLerp(float zoomSize, float speed = 10f) {
          if (!currCamera || !mainCamera) return;
          _zoomOffset   = Mathf.Min(_zoomOffset+zoomSize, 2);
          _targetOffset = 0f;
          _zoomSpeed    = speed;
          _isZooming    = true;
       }

       protected override void EarlyRoutine() { }

       protected override void Routine() { }

       protected override void LateRoutine() {
          // 1. 셰이크 럴프 제어
          if (_shakeLerp && _noiseComponent) {
             _noiseComponent.AmplitudeGain = Mathf.Lerp(_noiseComponent.AmplitudeGain, 0, _shakeLerpSpeed * DeltaTime);
             if (Mathf.Approximately(_noiseComponent.AmplitudeGain, 0f)) {
                _noiseComponent.AmplitudeGain = 0f;
                _shakeLerp = false;
             }
          }

          if (!currCamera || !mainCamera) return;

          // 2. 줌 오프셋 Lerp 연산
          if (_isZooming) {
             _zoomOffset = Mathf.Lerp(_zoomOffset, _targetOffset, DeltaTime * _zoomSpeed);

             if (Mathf.Abs(_zoomOffset - _targetOffset) < 0.01f) {
                _zoomOffset = _targetOffset;
                if (Mathf.Approximately(_targetOffset, 0f)) {
                   _isZooming = false; // 순정 복구 완료 시 연산 침묵
                }
             }
          }

          // [최종 집행] 메인카메라가 준 실시간 맵 렌즈값 + 내가 연산한 줌 오프셋 = 철옹성 렌즈 세팅!
          LensSettings lens = currCamera.Lens;
          lens.OrthographicSize = mainCamera.BaseLensSize + _zoomOffset;
          currCamera.Lens = lens;
       }

       protected override void FixedRoutine() { }
    }
}