using System.Collections.Generic;
using System.Globalization;
using Framework.Helper;
using SsitEngine.Unity.Action;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace Framework.SceneObject
{
    /// <summary>
    /// 简单的UI交互行为
    /// </summary>
    [DisallowMultipleComponent]
    public class ScriptHitEffect : ActionBase
    {
        [AddDescribe("当前生命值")] public int current = 100;

        private bool isActive;

        public float m_attackRange = 20; // emergency range
        public float m_damage = 10;

        //Test 1
        //[Tooltip("检索类型（多个类型请自己扩展）")]
        //private EnFactoryType m_factoryType = EnFactoryType.SceneObjectFactory;
        //[Tooltip("检索关联（多个类型请自己扩展）")]
        //public ObjectManager.SearchRelation mSearchRelation = ObjectManager.SearchRelation.CR_Fire;
        //[Tooltip("检索区域类型")]
        //public ObjectManager.SearchAreaType msSearchAreaType = ObjectManager.SearchAreaType.SA_Fan;
        //[Tooltip("检索区域参数【球面： vector（distance,angle）】【矩形：vector(高度/2，长度/2)】")]
        //public Vector2 mAreaParam = Vector2.zero;

        private RaycastHit m_hit;
        protected Collider m_xFTarget;

        [AddDescribe("最大损耗值")] public int maxHealth = 100;

        public BaseSceneInstance mOwner;

        [Header("seriable Field")] [Tooltip("粒子特效")]
        public ParticleSystem Ps;

        [AddDescribe("成长周期率 1/s for health")] public int recoveryTickRate = -1;

        public Collider XfTarget
        {
            get => m_xFTarget;
            set
            {
                if (value != null) m_xFTarget = value;
            }
        }

        public int Current
        {
            get => Mathf.Min(current, maxHealth);
            set
            {
                var temp = Mathf.Clamp(value, 0, maxHealth);
                current = temp;
                mOwner.LinkObject.ChangeProperty(mOwner, EnPropertyId.OnConsume,
                    current.ToString(CultureInfo.InvariantCulture));
            }
        }

        #region Main Method

        private void Start()
        {
            if (Ps == null) Ps = GetComponentInChildren<ParticleSystem>();
        }

        #endregion

        #region 子类实现

        public override void Init( BaseObject obj )
        {
            if (obj != null)
            {
                mOwner = obj.SceneInstance;
                mOwner.LinkObject.GetAttribute()[En_SceneObjectExParam.En_Health] = current.ToString();
                mOwner.LinkObject.GetAttribute()[En_SceneObjectExParam.En_MaxHealth] = maxHealth.ToString();
            }
        }

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            if (Ps == null || mOwner == null || !mOwner.isLink)
                return;
            var state = (En_SwitchState) actionParam.ParseByDefault(0);

            //只有回放中才这样传
            if (sender == data)
            {
                switch (state)
                {
                    case En_SwitchState.Working:
                        //todo:检测
                        EnableEffect(true);
                        break;
                    case En_SwitchState.Hide:
                        EnableEffect(false);
                        break;
                }
                return;
            }

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

            if (ObjectHelper.RaycastToVirtualEffect(Ps.transform, Ps.transform.forward, m_attackRange, out m_hit))
            {
                //检测到对象后，将对象给予粒子触发装置--精确对象检测
                m_xFTarget = m_hit.collider;
                if (m_xFTarget != null)
                    Ps.trigger.SetCollider(0, m_xFTarget);
            }
        }

        //粒子碰撞触发
        private float OnHitTrigger( Collider obj )
        {
            Debug.Log($"触发了{m_xFTarget.name}");

            //获取当前的损耗程度
            //

            //获取
            //return currentLoss + attachValue;

            //获取对象
            var enemyScriptHealth = m_xFTarget.transform.GetComponent<ScriptHealth>();
            if (enemyScriptHealth)
                enemyScriptHealth.OnHit(mOwner.LinkObject.GetParent(), GetDamage(), m_hit.point, m_hit.normal);
            return 0;
        }

        private bool CheckUseable( bool isLocalPlayer )
        {
            var currentLoss = mOwner.LinkObject.GetAttribute()[En_SceneObjectExParam.En_Health];
            if (string.IsNullOrEmpty(currentLoss)) return false;
            current = currentLoss.ParseByDefault(0);

            if (current <= 0)
            {
                if (isLocalPlayer)
                {
                    //todo:通知UI显示提示（灭火器已空）
//                    Facade.Instance.SendNotification((ushort)UIMsg.OpenForm, En_UIForm.TipForm, new TipContent()
//                    {
//                        content = "灭火器已空",
//                        formType = En_ConfirmFormType.En_Succeed,
//                    });
                }

                //关闭灭火器状态
                mOwner.LinkObject.ChangeProperty(mOwner, EnPropertyId.OnSwitch, ((int) En_SwitchState.Hide).ToString());
                return false;
            }

            return true;
        }

        private void EnableEffect( bool value, bool isServer = false )
        {
            Ps.gameObject.SetActive(value);
            if (isServer)
            {
                isActive = value;
                if (isActive)
                {
                    InvokeRepeating("Recover", 0, 1);
                    InvokeRepeating("GetTargetInRange", 0, 0.1f);
                }
                else
                {
                    CancelInvoke("Recover");
                    CancelInvoke("GetTargetInRange");
                }
            }
        }


        public void Recover()
        {
            if (recoveryTickRate == 0)
                return;

            if (current > 0)
            {
                // calculate over/underflowing value (might be <0 or >max)
                var next = current + recoveryTickRate;

                // assign current in range [0,max]
                Current = next;
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
    }
}