/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/21 11:22:56                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Globalization;
using System.Linq;
using Mirror;
using SsitEngine.Unity.Action;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 生命改变回调
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="curValue"></param>
    /// <param name="maxValue"></param>
    public delegate void HealthChangeHandler( ScriptHealth sender, int curValue, int maxValue );


    /// <summary>
    /// 影响道具生命周期的附加接口
    /// </summary>
    public interface IHealthBonus
    {
        int GetHealthBonus();
        int GetHealthRecoveryBonus();
    }

    [DisallowMultipleComponent]
    public class ScriptHealth : ActionBase
    {
        [AddDescribe("最大生命周长")] public int baseHealth = 100;

        // cache components that give a bonus (attributes, effect, etc.)
        private IHealthBonus[] bonusComponents;

        // current value
        // set & get: keep between min and max
        [AddDescribe("当前生命值")] public int current;

        public bool isShowUI = true;

        [Header("基础成长速率")]
        //public AnimationCurve pointsCurve = new AnimationCurve();
        private BaseObject m_owner;

        // (1/s for health, but might need to be less for something like temperature
        //  where we don't want to reduce it by -1C per second, etc.)
        [AddDescribe("成长周期率 1/s for health")] public int recoveryTickRate = 1;

        public int Current
        {
            get => Mathf.Min(current, Max);
            set
            {
                //bool emptyBefore = current == 0;

                var temp = Mathf.Clamp(value, 0, Max);
                if (current != temp)
                    m_owner.ChangeProperty(m_owner, EnPropertyId.Consume,
                        current.ToString(CultureInfo.InvariantCulture));
                current = temp;
            }
        }

        public bool Enable
        {
            get
            {
                if (null == m_owner) return false;
                return !m_owner.IsDirty;
            }
        }

        // calculate max
        public int Max
        {
            get
            {
                baseHealth = m_owner.GetAttribute()[En_SceneObjectExParam.En_MaxHealth].ParseByDefault(100);
                return baseHealth;
                //int bonus = bonusComponents != null ? bonusComponents.Sum(b => b.GetHealthBonus()) : 0;
                //return baseHealth + bonus;
            }
        }

        // recovery per tick (may depend on buffs, items etc.)
        // -> 'recoveryRate' sounds like 1/s, but 'PerTick' makes it 100% clear
        public int RecoveryPerTick
        {
            get
            {
                var bonus = bonusComponents != null ? bonusComponents.Sum(b => b.GetHealthRecoveryBonus()) : 0;
                var baseRecoveryPerTick = /*pointsCurve.Evaluate(Current / Max) * */recoveryTickRate;
                return baseRecoveryPerTick + bonus;
            }
        }

        public BaseObject Owner
        {
            get => m_owner;
            set => m_owner = value;
        }

        //void Awake()
        //{
        //    bonusComponents = GetComponentsInChildren<IHealthBonus>();
        //}

        public override void Init( BaseObject obj )
        {
            if (obj != null)
                m_owner = obj;
            //m_owner.GetAttribute()[En_SceneObjectExParam.En_Health] = current.ToString();
            //m_owner.GetAttribute()[En_SceneObjectExParam.En_MaxHealth] = Max.ToString();
            //bool islocal = !GlobalManager.Instance.IsSync || NetworkServer.active;
            //if (islocal)
            //{
            //    InvokeRepeating("Recover", 0, 1);
            //}
        }

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            var islocal = !GlobalManager.Instance.IsSync || NetworkServer.active;
            if (islocal) Current = actionParam.ParseByDefault(0);
        }

        private void OnDestory()
        {
            if (IsInvoking("Recover")) CancelInvoke("Recover");
        }


        /// <summary>
        /// 生命百分比
        /// </summary>
        /// <returns></returns>
        public float Percent()
        {
            return Current != 0 && Max != 0 ? Current / (float) Max : 0;
        }

        // recover once a second
        // note: when stopping the server with the networkmanager gui, it will
        //       generate warnings that Recover was called on client because some
        //       entites will only be disabled but not destroyed. let's not worry
        //       about that for now.

        public void Recover()
        {
            if (Enable && current > 0)
            {
                // calculate over/underflowing value (might be <0 or >max)
                var next = Current + RecoveryPerTick;

                // assign current in range [0,max]
                Current = next;
            }
        }

        public void OnHit( BaseObject obj, float damage, Vector3 hitPoint, Vector3 hitNormal )
        {
            var loss = (int) damage;
            var cur = Current - loss;


            //通知服务器改变
            m_owner.ChangeProperty(m_owner, EnPropertyId.OnConsume, current.ToString(CultureInfo.InvariantCulture));

            //todo:通知界面刷新
            //Access:灭火进度控制
            //通知UI进度条(Client)
            if (isShowUI)
                //Facade.Instance.SendNotification(ConstNotification.c_sOpenForm, ConstValue.c_sUICommonProcessForm);
                //Facade.Instance.SendNotification(()ENProcessMediatorMsg.EnSetProcessValue.ToString(), sender.Percent());
                return;

            if (cur == 0)
                //Facade.Instance.SendNotification((ushort)ConstNotification.SyncPopTips, MessageInfo.Generate(EnMessageType.SYSTEM, "火势已熄灭！"));

                return;
        }
    }
}