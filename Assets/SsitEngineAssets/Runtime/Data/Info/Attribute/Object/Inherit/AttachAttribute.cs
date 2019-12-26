using System;
using System.Globalization;
using System.Runtime.Serialization;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;

namespace Framework.Data
{
     /// <summary>
    /// 交互道具属性基类
    /// </summary>
    [Serializable]
    public class AttachAttribute : BaseAtrribute
    {
        public En_SwitchState mState;

        #region 子类实现

        public override void NotifyPropertyChange(EnPropertyId propertyid, string param, object data)
        {
            switch (propertyid)
            {
                case EnPropertyId.SwitchIK:
                case EnPropertyId.Switch:
                case EnPropertyId.OnSwitch:
                case EnPropertyId.Trigger:
                    {
                        mState = (En_SwitchState)param.ParseByDefault(-1);
                        isChange = true;
                    }
                    break;
                case EnPropertyId.Authority:
                    {
                        mParentId = param;
                        m_baseObject.SetParent(param);
                        isChange = true;
                    }
                    break;
                case EnPropertyId.OnConsume:
                    {
                        //获取当前的损耗程度
                        float lossValue = param.ParseByDefault(0.0f);
                        this[En_SceneObjectExParam.En_Health] = lossValue.ToString(CultureInfo.InvariantCulture);
                    }
                    break;
            }
        }

        public override string GetProperty(EnPropertyId propertyId)
        {
            switch (propertyId)
            {
                case EnPropertyId.Switch:
                    {
                        return ((int)mState).ToString();
                    }
            }
            return String.Empty;
        }

        public override void Apply(object data)
        {
            base.Apply(data);

            //根据inspector面板初始化序列化字段
            if (m_baseObject.SceneInstance is BaseInteractiveInstance interativeInstace)
            {
                mState = interativeInstace.m_state;
            }

            //初始化
            m_baseObject.OnChangeProperty(m_baseObject, EnPropertyId.OnSwitch, ((int)mState).ToString());
        }

        #endregion

        #region 序列化
        public AttachAttribute()
        {

        }

        public AttachAttribute(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.mParentId = info.GetString("owner");
            this.mState = (En_SwitchState)info.GetValue("state", typeof(En_SwitchState));

        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("owner", mParentId);
            info.AddValue("state", mState);

        }

        #endregion
    }
}