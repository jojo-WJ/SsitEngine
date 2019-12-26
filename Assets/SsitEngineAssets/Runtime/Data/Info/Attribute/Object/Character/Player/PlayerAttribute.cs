using System;
using System.Runtime.Serialization;
using Framework.SceneObject;
using SsitEngine.DebugLog;
using SsitEngine.Unity.Avatar;
using SsitEngine.Unity.SceneObject;

namespace Framework.Data
{
    /// <summary>
    /// Npc信息
    /// </summary>
    [Serializable]
    public class PlayerAttribute : CharacterAttribute, ISerializable
    {
        //public Dictionary<int,Vector3> mEquipOffsetMap =new Dictionary<int,Vector3>();
        public PlayerAttribute()
        {
            mEquips = null;
            mEzInputs = new EzInputs();
            mEquips = new SaveEquipInfo[(int) InvSlot.MaxValue];
            mUseItem = new string[(int) InvUseNodeType.MaxValue];
        }

        public new Player GetParent()
        {
            return m_baseObject as Player;
        }

        public override void Apply( object data )
        {
            // this is no necessary
            base.Apply(data);

            SsitDebug.Info("合成应急单位 Player" + DataId);
        }

        #region 子类继承

        /*public override void NotifyPropertyChange(EnPropertyId propertyid, string param, object data)
        {
            base.NotifyPropertyChange(propertyid, param, data);
        }*/

        #endregion

        #region 序列化

        public PlayerAttribute( SerializationInfo info, StreamingContext context ) : base(info, context)
        {
            mEzInputs = (EzInputs) info.GetValue("inputs", typeof(EzInputs));
            mState = (EN_CharacterActionState) info.GetValue("state", typeof(EN_CharacterActionState));
            mEquips = (SaveEquipInfo[]) info.GetValue("equip", typeof(SaveEquipInfo[]));
            mUseItem = (string[]) info.GetValue("use", typeof(string[]));
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData(info, context);
            info.AddValue("inputs", mEzInputs);
            info.AddValue("state", mState);
            info.AddValue("equip", mEquips);
            info.AddValue("use", mUseItem);
        }

        #endregion
    }
}