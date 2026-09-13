using UengSystem.Objects;
using System.Collections.Generic;
using AncientMemorial.Entities;
using AncientMemorial.Entities.Enemies;
using AncientMemorial.Map;
using AncientMemorial.Objects;
using AncientMemorial.Projectiles;
using UengSystem.ObjectPool;
using UengSystem.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AncientMemorial.States.EnemyStates.Crystal.Phase3 {
    public abstract class CrystalPhase3State : CrystalState {

        // 정적 프로퍼티
        private static readonly int MissilePoolKey = "CrystalSemiHugeMissile".GetHash();
        private static readonly int RayPoolKey = "CrystalUltRay".GetHash();
        private const string MapLayerName   = "Map";
        private const float SnapDistance    = 0.01f;
        private const float ResnapDistance  = 0.1f;
        
        private const float XCurveNorm    = Mathf.PI * Mathf.PI * Mathf.PI * 2 / 3f - Mathf.PI; // 곡선의 적당한 곡률을 위한 숫자 (x성분)
        private const float InvXCurveNorm = 1f / XCurveNorm;
        private const float YCurveNorm    = 1f / (4 * Mathf.PI * Mathf.PI); // 곡선의 적당한 곡률을 위한 숫자 (y성분)

        // 인스턴스 프로퍼티
        private readonly List<AttackArea> missileAttackAreas = new();
        
        private CrystalMoveMode moveMode;
        private StopWatch       moveWatch = new StopWatch().TryTick();
        private float           t => moveWatch.Tock();

        private bool isSnapped;

        private List<CrystalRay> rays = new List<CrystalRay>(20);

        // 인스턴스 메서드
        protected void ShootSemiHugeMissile(Vector2 pos, float rot, float speed = 5f) {
            Vector2      dir       = new (-Mathf.Sin(rot * Mathf.Deg2Rad), Mathf.Cos(rot * Mathf.Deg2Rad));
            RaycastHit2D hit       = Physics2D.Raycast(pos, dir, 100, LayerMask.GetMask(MapLayerName));
            float        dist      = Vector2.Distance(pos, hit.point) - 0.5f;
            float        timeToHit = dist / speed;
			
            GameObject obj = UObject.Get(MissilePoolKey, pos, PlayEffect: false);
            obj.transform.rotation = Quaternion.Euler(0, 0, rot);
            
            BasicProjectile semiHugeMissile = obj.GetComponent<BasicProjectile>();
            semiHugeMissile.Category                   = "crystalMissile";
            semiHugeMissile.rigidbody2D.linearVelocity = dir * speed;
            
            missileAttackAreas.Add(Entity.AttackArea(crystal, 1, timeToHit, pos, new Vector2(0.65f, 100), rot));
        }

        protected void ShootBarrageMissile(Vector2 pos, int missileCount, float speed = 13.5f, float? rotateOffset = null, float timeBeforeLockOn = 0, float lockOnDuration = 0) {
            // dTheta = 2pi/missileCount
            rotateOffset ??= Random.Range(0, 360);
            for (int i = 0; i < missileCount; i++) {
                float rot = 720f * i / missileCount + rotateOffset.Value;
                ShootMissile(pos, rot, speed, timeBeforeLockOn, lockOnDuration);
            }
        }
        

        protected float GetAngleToTarget(Vector2 from, bool isUpZero) {
            Entity  target = stateMachine.getTarget.Invoke();
            Vector2 to     = target.transform.position;
            Vector2 dir    = to - from;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return angle + (isUpZero ? -90 : 0);
        }
        
        protected CrystalRay SpawnRay(float radius, float angularVelocity, float offsetDegrees, Vector2? pos = null, bool effect = false) {
            Vector2 mapSize = MapManager.instance.GetMapSize();
            
            GameObject    ultRayObj = UObject.Get(RayPoolKey, pos ?? new Vector2(0, mapSize.y / 2), PlayEffect: true);
            CrystalRay ray    = ultRayObj.GetComponent<CrystalRay>();
            ray.owner           = crystal;
            ray.radius          = radius;
            ray.angularVelocity = angularVelocity;
            ray.angleOffset     = Mathf.Repeat(offsetDegrees*Mathf.Deg2Rad, 2*Mathf.PI);
            ray.showEffect      = effect;
			
            rays.Add(ray);
            return ray;
        }

        protected void ClearRays() {
            foreach (CrystalRay ray in rays) {
                ray.Release(PlayEffect: true);
            }
            
            rays.Clear();
        }

        protected void ReleaseRay(CrystalRay ray) {
            rays.Remove(ray);
            
            ray.Release(PlayEffect: true);
        } 

        protected void SetMoveMode(CrystalMoveMode moveMode) {
            this.moveMode = moveMode;
            moveWatch.Tick();
        }
        
        private Vector2 GetTargetPos_LissajousPath() {
            const float speed  = 0.3f;
            const float period = 2f * Mathf.PI / speed; // sin(2nt), sin(3nt) 공통 주기
            
            Vector2 mapSize = MapManager.instance.GetMapSize();
            float   nt      = speed * (t % period);
            return new Vector2(
                mapSize.x * Mathf.Sin(2                    * nt) / 3f,
                mapSize.y * 0.5f + mapSize.y * Mathf.Sin(3 * nt) * 0.25f + mapSize.y * 0.125f
            );
        }

        private Vector2 GetTargetPos_FollowTarget(Vector2 playerPos) {
            const float speed  = 0.5f;
            const float period = 2f * Mathf.PI / speed; // sin(2nt), sin(nt) 공통 주기
            
            float   nt      = speed * (t % period);
            return new Vector2(
                playerPos.x + Mathf.Sin(2 * nt),
                2.625f      + playerPos.y + 0.5f * Mathf.Sin(nt)
            );
        }

        private Vector2 GetTargetPos_CurveFromLToR() {
            Vector2 mapSize = MapManager.instance.GetMapSize();
            float   nt      = Mathf.Min(2f * t, Mathf.PI * 2f);
            return new Vector2(
                (mapSize.x * 0.5f - 3) * (nt * nt * nt / 3f - nt * nt * Mathf.PI + nt + XCurveNorm) * InvXCurveNorm,
                mapSize.y * (nt - Mathf.PI) * (nt - Mathf.PI) * YCurveNorm + mapSize.y * 0.5f
            );
        }

        private Vector2 GetTargetPos_CurveFromRToL() {
            Vector2 mapSize = MapManager.instance.GetMapSize();
            float   nt      = Mathf.Min(2f * t, Mathf.PI * 2f);
            return new Vector2(
                -(mapSize.x * 0.5f - 3) * (nt * nt * nt / 3f - nt * nt * Mathf.PI + nt + XCurveNorm) * InvXCurveNorm,
                mapSize.y * (nt    - Mathf.PI) * (nt         - Mathf.PI) * YCurveNorm + mapSize.y * 0.5f
            );
        }

        private Vector2 GetTargetPos_CircleOnCenter() {
            const float speed   = 0.3f;
            const float period  = Mathf.PI / speed; // cos(2nt), sin(2nt) 주기
            
            Vector2     mapSize = MapManager.instance.GetMapSize();
            float       nt     = speed * (t % period);
            return new Vector2(
                Mathf.Cos(2 * nt) * 0.5f,
                Mathf.Sin(2 * nt) * 0.5f + mapSize.y / 2
            );
        }

        private Vector2 GetTargetPos_Death() {
            Vector2 mapSize = MapManager.instance.GetMapSize();
            float   nt      = 0.2f * t; // 주기 없음
            return new Vector2(
                nt * Mathf.Cos(Random.Range(0, Mathf.PI * 2)),
                nt * Mathf.Sin(Random.Range(0, Mathf.PI * 2)) + mapSize.y / 2
            );
        }
        
        public Vector2 GetTargetPosition() {
            Entity  target  = stateMachine.getTarget.Invoke();
            
            Vector2 playerPos = target?.transform.position??Vector2.zero;
            Vector2 targetPos = moveMode switch {
                CrystalMoveMode.LissajousPath  => GetTargetPos_LissajousPath(),
                CrystalMoveMode.FollowTarget   => GetTargetPos_FollowTarget(playerPos),
                CrystalMoveMode.CurveFromLToR  => GetTargetPos_CurveFromLToR(),
                CrystalMoveMode.CurveFromRToL  => GetTargetPos_CurveFromRToL(),
                CrystalMoveMode.CircleOnCenter => GetTargetPos_CircleOnCenter(),
                CrystalMoveMode.Death          => GetTargetPos_Death(),
                _                              => Vector2.zero
            };
            
            return targetPos;
        }

        protected virtual void Move() {
            Vector2 targetPos = GetTargetPosition();

            float distanceToTarget = Vector2.Distance(crystal.transform.position, targetPos);
            float snapDistance = isSnapped ? ResnapDistance : SnapDistance;
            isSnapped = distanceToTarget <= snapDistance;
            
            if (isSnapped) {
                crystal.transform.position = targetPos;
                crystal.rigidbody2D.linearVelocity = Vector2.zero;
            }
            else {
                float speedMultiplier = distanceToTarget < 4f ? distanceToTarget / 4f : 1f;
                crystal.rigidbody2D.linearVelocity =
                    (targetPos - (Vector2)crystal.transform.position).normalized *
                    (crystal.stat.moveSpeed * speedMultiplier);
            }
            
            crystal.transform.rotation     = Quaternion.Euler(0f, 0f, -2 * crystal.rigidbody2D.linearVelocityX);
            crystal.To<CrystalPhase3>().rotatingAnimator.speed = crystal.rigidbody2D.linearVelocity.magnitude;
        }

        // 오버라이드 메서드
        protected override void ClearMissiles() {
            base.ClearMissiles();
            foreach (AttackArea attackArea in missileAttackAreas) {
                attackArea.Cancel();
            }
        }
        
        public override void OnEnter() {
            base.OnEnter();
            SetMoveMode(CrystalMoveMode.LissajousPath);
        }
        
        public override void OnFixedRoutine() {
            base.OnFixedRoutine();
            Move();
        }
    }
}
