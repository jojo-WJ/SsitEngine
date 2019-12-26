using System.Collections.Generic;
using Framework;
using Framework.Helper;
using Framework.SceneObject;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace SsitEngine.Unity.Action
{
    /// <summary>
    /// 简单的UI交互行为
    /// </summary>
    public class ScriptCurveHitEffect : ActionBase
    {
        private bool isActive;

        public float m_attackRange = 20; // emergency range
        public float m_damage = 10;

        private RaycastHit m_hit;
        protected Collider m_xFTarget;
        public BaseSceneInstance mOwner;

        [Header("seriable Field")] [Tooltip("粒子特效")]
        public ParticleSystem Ps;

        [Range(1, 25)] [Tooltip("曲线线段长度")] public int stepSize = 4;

        public Collider XfTarget
        {
            get => m_xFTarget;
            set
            {
                if (value != null) m_xFTarget = value;
            }
        }

        #region Main Method

        private void Start()
        {
            if (Ps == null) Ps = GetComponentInChildren<ParticleSystem>();

            //if (mOwner)
            //{
            //    mOwner = GetComponent<BaseSceneInstance>();
            //}
        }

        #endregion

        #region 子类实现

        public override void Init( BaseObject obj )
        {
            if (obj != null) mOwner = obj.SceneInstance;
        }

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            if (Ps == null || mOwner == null || !mOwner.isLink)
                return;
            var player = data as Player;
            if (player == null)
                return;
            //属性改变计算
            //var consume = actionParam.ParseByDefault(0.0f);
            //var curConsume = OnCalcHit(mOwner, m_consume);


            //var player = data as Player;
            //if (player != null)
            //{
            //    mOwner.OnChangeProperty(sender, EnPropertyId.OnConsume, curConsume.ToString(CultureInfo.InvariantCulture), data);
            //}

            var state = (En_SwitchState) actionParam.ParseByDefault(0);
            var isLocalPlayer = player.SceneInstance.HasAuthority;
            switch (state)
            {
                case En_SwitchState.Working:
                {
                    //todo:检测
                    if (CheckUseable(isLocalPlayer)) EnableEffect(true, isLocalPlayer);
                }
                    break;
                case En_SwitchState.Hide:
                    EnableEffect(false, isLocalPlayer);
                    break;
            }
        }

        #endregion

        #region Members Method

        //获取参数范围内的目标对象
        private void GetTargetInRange()
        {
            if (Ps == null || !isActive) return;
            //test 1、全局检索
            //var checkList = ObjectManager.Instance.GetSceneObject(Ps.gameObject, m_factoryType, mSearchRelation, msSearchAreaType, mAreaParam);
            //Debug.LogError(checkList.Count);

            //test 2、射线
            AdjustForEarlyCollisions();
        }

        //粒子碰撞触发
        private float OnHitTrigger( Collider obj )
        {
            if (m_xFTarget == null) return 0;
            Debug.Log($"触发了{m_xFTarget.name}");

            //获取当前的损耗程度
            //

            //获取
            //return currentLoss + attachValue;

            //获取对象
            var enemyScriptHealth = m_xFTarget.transform.GetComponent<ScriptHealth>();
            if (enemyScriptHealth)
            {
                //player.DealDamageAt(enemyHealth, damage, hit.point, hit.normal, hit.collider);
            }
            return 0;
        }

        private bool CheckUseable( bool isLocalPlayer )
        {
            return true;
        }

        private void EnableEffect( bool value, bool isServer = false )
        {
            Ps.gameObject.SetActive(value);
            if (isServer)
            {
                isActive = value;
                if (isActive)
                    InvokeRepeating("GetTargetInRange", 0, 0.1f);
                else
                    CancelInvoke("GetTargetInRange");
            }
        }

        public float GetDamage()
        {
            if (gameObject.activeSelf) return m_damage;
            return 0;
        }

        #endregion


        #region 粒子触发回调

        private readonly List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();

        //粒子触发的回调函数
        private void OnParticleTrigger()
        {
            //只要勾选了粒子系统的trigger，程序运行后会一直打印

            //官方示例，拿来说明
            //ParticleSystem ps = transform.GetComponent<ParticleSystem>();

            //particleSystemTriggerEventType为枚举类型，Enter,Exit,Inside,Outside,对应粒子系统属性面板上的四个选项
            var numEnter = Ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
            //进入触发器，粒子变为红色
            if (numEnter > 0) OnHitTrigger(m_xFTarget);

            for (var i = 0; i < numEnter; i++)
            {
                var p = enter[i];
                p.startColor = Color.red;
                enter[i] = p;
            }
            Ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        }

        #endregion

        #region 曲线粒子检测

        [SerializeField] private float factor = 1f;

        [SerializeField] private float Offset = 5f;

        private List<Vector3> GetPoint()
        {
            var ret = new List<Vector3>();

            //获取粒子发射速度
            var speed = Ps.main.startSpeedMultiplier;

            //获取粒子存在周期时间
            var duration = Ps.main.duration;

            //获取粒子随时间X方向的偏移量
            var orbitalXMultiplier = Ps.velocityOverLifetime.orbitalXMultiplier;

            //获取粒子的发射方向
            var direction = transform.forward;

            //确定当前粒子发射距离
            var distance = speed + Offset; /** duration*/
            ;

            var disPos = transform.position + direction * distance;

            if (stepSize >= distance)
            {
                ret.Add(transform.position);
                ret.Add(disPos);
            }
            else
            {
                ret.Add(transform.position);

                var count = Mathf.FloorToInt(distance / stepSize);
                for (var i = 1; i < count + 1; i++)
                {
                    //计算当前位置
                    var curPos = transform.position + direction * (stepSize * i);

                    //计算水平距离
                    var s = curPos.z - transform.position.z;

                    var t = s / speed * (distance / factor);

                    var h = 9.8f * 0.5f * t * t * orbitalXMultiplier;
                    var tempPos = curPos + new Vector3(0, h, 0);
                    ret.Add(tempPos);
                }
            }

            return ret;
        }

        protected virtual void AdjustForEarlyCollisions()
        {
            var checkPoints = GetPoint().ToArray();
            var checkCount = checkPoints.Length;
            if (checkCount % 2 != 0) checkCount--;
            for (var i = 0; i < checkCount; i += 2)
            {
                var currentPoint = checkPoints[i];
                var nextPoint = checkPoints[i + 1];
                if (CheckRay(currentPoint, nextPoint)) return;
            }
            if (checkCount < checkPoints.Length) CheckRay(checkPoints[checkCount - 1], checkPoints[checkCount]);
        }

        private bool CheckRay( Vector3 curPos, Vector3 nextPos )
        {
            var currentPoint = curPos;
            var nextPoint = nextPos;
            var nextPointDirection = (nextPoint - currentPoint).normalized;
            var nextPointDistance = Vector3.Distance(currentPoint, nextPoint);

            var checkCollisionRay = new Ray(currentPoint, nextPointDirection);

            if (ObjectHelper.RaycastToVirtualEffect(Ps.transform, checkCollisionRay, nextPointDistance, out m_hit))
            {
                //检测到对象后，将对象给予粒子触发装置--精确对象检测
                m_xFTarget = m_hit.collider;
                if (m_xFTarget != null)
                    Ps.trigger.SetCollider(0, m_xFTarget);
                return true;
            }
            return false;
        }

        #endregion

        //[SerializeField]
        //private bool isResetCalc = false;


        //private void OnDrawGizmos()
        //{
        //    if (Ps == null)
        //    {
        //        return;
        //    }
        //    Vector3[] lines = GetPoint().ToArray();
        //    if (isResetCalc)
        //    {
        //        isResetCalc = false;
        //        Debug.Log($"expression{lines.Length}startSpeedMultiplier{Ps.main.startSpeedMultiplier}duration{Ps.main.duration}orbitalOffsetXMultiplier{Ps.velocityOverLifetime.orbitalXMultiplier}");
        //        return;
        //    }
        //    UnityEditor.Handles.color = Color.red;

        //    //for (int i = 0; i < lines.Length; i++)
        //    //{
        //    //    UnityEditor.Handles.SphereHandleCap(99,lines[i],Quaternion.identity, 5f,EventType.Ignore);
        //    //}
        //    //if (lines.Length % 2 == 0)
        //    //{
        //    //    UnityEditor.Handles.DrawLines(lines);
        //    //}
        //    //else
        //    //{
        //    //    UnityEditor.Handles.DrawLines(lines, new int[lines.Length - 1]);

        //    //    UnityEditor.Handles.color = Color.green;
        //    //    UnityEditor.Handles.DrawLine(lines[lines.Length - 2], lines[lines.Length - 1]);
        //    //}

        //    for (int i = 1; i < lines.Length; i++)
        //    {
        //        UnityEditor.Handles.DrawLine(lines[0], lines[i]);
        //    }
        //    UnityEditor.Handles.color = Color.white;
        //}
    }
}