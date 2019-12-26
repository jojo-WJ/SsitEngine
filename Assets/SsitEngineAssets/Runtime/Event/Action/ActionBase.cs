/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/27 16:44:48                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.SceneObject;
using Framework.SceneObject.Trigger;
using SsitEngine.Unity.SceneObject;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SsitEngine.Unity.Action
{
    public enum EnActionType
    {
        [EnumLabel("空")] ActionNone = -1,
        [EnumLabel("缓动行为")] ActionTween = 0,
        [EnumLabel("拾取行为")] ActionPick = 1,
        [EnumLabel("开关行为")] ActionSwitch = 2,
        [EnumLabel("组件激活行为")] ActionActiveHelper = 3,
        [EnumLabel("摇杆控制行为")] ActionJoyControl
    }

    /// <summary>
    /// 交互行为基类
    /// </summary>
    public abstract class ActionBase : MonoBehaviour
    {
        [Tooltip("行为ID")] [SerializeField] protected EnPropertyId m_actionId;

        [Tooltip("行为参数")] [SerializeField] protected string m_actionParam;

        //[SerializeField]
        [EnumLabel("事件类型")] public EnActionType m_actionType;

        [Tooltip("是否直属行为")] [SerializeField] protected bool m_DirectDescend = true;

        [Header("行为回调")] [FormerlySerializedAs("onAction")] [SerializeField]
        public ActionEvent m_onAction;

        public UnityEvent m_onCancel;
        public UnityEvent m_onComplete;

        public EnPropertyId ActionId => m_actionId;

        /// <summary>
        /// 直属行为（直接归属实体响应）
        /// </summary>
        public bool DirectDescend => m_DirectDescend;


        public void Awake()
        {
            //m_onAction.AddListener(Execute);
        }

        /// <summary>
        /// 节点初始化
        /// </summary>
        public virtual void Init( BaseObject obj )
        {
        }

        /// <summary>
        /// 行为执行
        /// </summary>
        public abstract void Execute( object sender, EnPropertyId m_actionId, string m_actionParam,
            object data = null );
    }
}