using System;
using System.Runtime.Serialization;
using Framework.Mirror;
using SsitEngine.EzReplay;

namespace Framework.Data
{
    /// <summary>
    /// 客户端代理属性信息
    /// </summary>
    [Serializable]
    public class NetPlayerAttribute : BaseAtrribute
    {
        public SerVector2 input;

        private NetPlayerAgent m_agent;
        public EnInputMode mode;
        public string player;
        public float zoom;

        public NetPlayerAttribute()
        {
            input = new SerVector2(0, 0);
        }


        public void SetAgent( NetPlayerAgent netPlayerAgent )
        {
            m_agent = netPlayerAgent;
        }

        #region 回放

        public override bool IsDifferentTo( SavedBase lastState, Object2PropertiesMapping o2m )
        {
            var attribute = lastState as NetPlayerAttribute;
            if (attribute == null)
                return false;
            m_agent?.ApplyInput(this);
            var changed = base.IsDifferentTo(lastState, o2m);
            return changed;
        }

        #endregion

        #region 序列化

        public NetPlayerAttribute( SerializationInfo info, StreamingContext context ) : base(info, context)
        {
            mode = (EnInputMode) info.GetValue("mode", typeof(EnInputMode));
            player = info.GetString("player");
            input = (SerVector2) info.GetValue("input", typeof(SerVector2));
            zoom = info.GetSingle("zoom");
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData(info, context);
            info.AddValue("mode", mode);
            info.AddValue("player", player);
            info.AddValue("input", input);
            info.AddValue("zoom", zoom);
        }

        #endregion
    }
}