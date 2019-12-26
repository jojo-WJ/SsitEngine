using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Framework.Helper;
using Framework.SceneObject;
using SsitEngine;
using SsitEngine.Unity.SceneObject;

namespace Framework.Data
{
    /// <summary>
    /// 应急力量单位信息
    /// </summary>
    [Serializable]
    public class VehicleAttribute : BaseAtrribute
    {
        public string action;
        public int dataId;
        public int department;
        public string desc;
        public List<int> itemList;

        public En_SwitchState m_state;
        public int playerCount;

        public List<int> playerList;
        public string profession;

        public VehicleAttribute()
        {
        }

        public new Vehicle GetParent()
        {
            return m_baseObject as Vehicle;
        }

        public override void Apply( object data )
        {
            //base.Apply(data);
            var vehicle = GetParent();
            //LogManager.Info( "合成应急单位" + data.dataId );

            if (data is UnitDataInfoInfo unitDataInfoInfo)
            {
                ID = unitDataInfoInfo.guid;
                dataId = unitDataInfoInfo.dataId;

                // 网络数据优先
                Name = unitDataInfoInfo.name;
                profession = string.IsNullOrEmpty(unitDataInfoInfo.postType) ? profession : unitDataInfoInfo.postType;
                department = unitDataInfoInfo.department;

                desc = string.IsNullOrEmpty(unitDataInfoInfo.desc) ? desc : unitDataInfoInfo.desc;

                //init Skill
                for (var i = 0; i < mSkillList.Count; i++)
                {
                    if (mSkillList[i] == 0) continue;
                    SsitApplication.Instance.CreateSkill(null, mSkillList[i], InternalAddSkillCallback, vehicle);
                }

                //GroupId = ConstValue.c_sDefaultNpcGroupName;
            }
            /*else if (data is KeyValuePair<NpcDataInfo, UnitDataInfoInfo> keyValue)
            {
                unitDataInfoInfo = keyValue.Value;

                ID = unitDataInfoInfo.guid;
                dataId = unitDataInfoInfo.dataId;

                // 网络数据优先
                Name = unitDataInfoInfo.name;
                profession = string.IsNullOrEmpty(unitDataInfoInfo.postType) ? profession : unitDataInfoInfo.postType;
                department = unitDataInfoInfo.department;

                desc = string.IsNullOrEmpty(unitDataInfoInfo.desc) ? desc : unitDataInfoInfo.desc;

                var info = keyValue.Key;

                mGroupId = info.GroupId;

                //init Skill
                //clear pre data
                mSkillList.Clear();
                //reset config
                for (var i = 0; i < info.skills.Count; i++)
                {
                    var skill = info.skills[i];
                    skill.ResetState();
                    mSkillList.Add(skill.skillId);
                    SsitApplication.Instance.CreateSkill(null, string.Empty, false, skill, InternalAddSkillCallback,
                        vehicle);
                }
            }*/
            else
            {
                //init Skill
                for (var i = 0; i < mSkillList.Count; i++)
                {
                    if (mSkillList[i] == 0) continue;
                    SsitApplication.Instance.CreateSkill(null, mSkillList[i], InternalAddSkillCallback, vehicle);
                }
            }

            //todo:init item


            //卵生车载对象
//            if (GlobalManager.Instance.IsSync && NetworkServer.active)
//            {
//                for (int i = 0; i < playerList.Count; i++)
//                {
//                    var uuid = SsitApplication.Instance.CreatePlayer(playerList[i]).Guid;
//                    SsitApplication.Instance.DestoryPlayer(uuid);
//                    m_baseObject.ChangeProperty(m_baseObject, EnPropertyId.Init, StringUtils.JointStringByFormat(uuid, playerList[i].ToString(), GroupId));
//                }
//            }
//            else if (GlobalManager.Instance.ReplayMode != ActionMode.PLAY)
            {
                //非回放进程的单机模式卵生创建
                for (var i = 0; i < playerList.Count; i++)
                    SsitApplication.Instance.CreatePlayer(null, playerList[i], InternalCreatePlayer, mGroupId);
            }
        }

        public override void NotifyPropertyChange( EnPropertyId propertyid, string param, object data )
        {
            base.NotifyPropertyChange(propertyid, param, data);
            switch (propertyid)
            {
                case EnPropertyId.Init:
                {
                    //Debug.LogError("VehicleAttribute NotifyPropertyChange Init");
                    var p = StringUtils.SplitStringByFormat(param);
                    SsitApplication.Instance.CreatePlayer(p[0], TextUtils.ToInt(p[1]), InternalCreatePlayer, p[2]);
                }
                    break;
                case EnPropertyId.OnSwitch:
                    m_state = (En_SwitchState) param.ParseByDefault(0);
                    isChange = true;
                    break;
            }
        }

        private void InternalCreatePlayer( BaseObject obj, object render, object data )
        {
            obj.GetAttribute().GroupId = (string) data;
            SsitApplication.Instance.OnSyncPlayerCreatedFunc(obj, render, data);
            obj.SetVisible(false);
            GetParent().AddPlayers((Player) obj);
            //Debug.LogError($"InternalCreatePlayer{Name} {((GameObject)render).name} {GroupId}");
        }

        /// <summary>
        /// 技能创建回调
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="render"></param>
        /// <param name="data"></param>
        private void InternalAddSkillCallback( BaseObject obj, object render, object data )
        {
            if (data is Vehicle vehicle)
                //Debug.LogError("InternalAddSkillCallback");
                vehicle.AddSkill((Skill) obj);
        }

        #region 序列化

        public VehicleAttribute( SerializationInfo info, StreamingContext context ) : base(info, context)
        {
            m_state = (En_SwitchState) info.GetValue("state", typeof(En_SwitchState));
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData(info, context);
            info.AddValue("state", m_state);
        }

        #endregion

        #region 回放

        public override bool InitBaseTrack()
        {
            mBaseTrack = new List<string>(3);
            mBaseTrack.Add(Name);
            mBaseTrack.Add(mGroupId);
            mBaseTrack.Add(profession);
            //mInputs = new Inputs();
            return base.InitBaseTrack();
        }

        public override void ResetBaseTrack( List<string> baseTrack )
        {
            var isInit = string.IsNullOrEmpty(mGroupId);
            Name = baseTrack[0];
            mGroupId = baseTrack[1];
            profession = baseTrack[2];

            var vehicle = GetParent();
            if (isInit)
                vehicle.InitHUD(vehicle.GetRepresent().transform, ObjectHelper.GetPlayerName(GetParent()), string.Empty);
            //mInputs = new Inputs();
            //((PlayerInstance)sceneInstance).PlayerIndex = PlayerIndex;
            //m_SceneObject.OnStartClient();
        }

        #endregion
    }
}