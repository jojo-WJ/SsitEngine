using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Framework;
using Framework.Data;
using Framework.Helper;
using Framework.Logic;
using Framework.SceneObject;
using SsitEngine.EzReplay;
using SsitEngine.Unity.Avatar;
using SsitEngine.Unity.HUD;

namespace SsitEngine.Unity.SceneObject
{
    /// <summary>
    /// Npc信息
    /// </summary>
    [Serializable]
    public class CharacterAttribute : BaseAtrribute, ISerializable
    {
        public string actionName;

        //public string Name;//姓名
        public int Age; //年龄
        public int Department; //大类
        public string Gender; //性别

        /// <summary>
        /// 协助对象
        /// </summary>
        public BaseSceneInstance mAssigned;

        /// <summary>
        /// 初始装备列表
        /// </summary>
        public List<int> mEquipList = new List<int>();

        public SaveEquipInfo[] mEquips;

        /// <summary>
        /// 我当前点击对象列表
        /// </summary>
        public HashSet<BaseSceneInstance> mPointObj = new HashSet<BaseSceneInstance>();

        /// <summary>
        /// 我当前持有对象
        /// </summary>
        public BaseSceneInstance mPostObj;
        //public Dictionary<int,Vector3> mEquipOffsetMap =new Dictionary<int,Vector3>();

        /// <summary>
        /// 当前触发列表
        /// </summary>
        public HashSet<BaseSceneInstance> mTriggleObjs = new HashSet<BaseSceneInstance>();

        public string[] mUseItem;
        public string Profession; //工种

        public CharacterAttribute()
        {
        }

        #region 序列化

        public CharacterAttribute( SerializationInfo info, StreamingContext context ) : base(info, context)
        {
        }

        #endregion

        public new Character GetParent()
        {
            return m_baseObject as Character;
        }

        public override void Apply( object data )
        {
            // this is no necessary
            //base.Apply(data);

            //LogManager.Info( "合成应急单位" + data.dataId );
            var player = GetParent();

            /*if (data is CharacterInfo characterInfo)
            {
                m_DataId = Int32.Parse(characterInfo.modelId);
                m_Description = characterInfo.description;
                
                Name = characterInfo.name;
                Profession = characterInfo.professionName;
                Department = Int32.Parse(characterInfo.lableType);
            }*/
            if (data is UnitDataInfoInfo unitDataInfoInfo)
            {
                ID = unitDataInfoInfo.guid;
                m_DataId = unitDataInfoInfo.dataId;
                m_Description = unitDataInfoInfo.desc;

                // 网络数据优先
                Name = unitDataInfoInfo.name;
                Profession = string.IsNullOrEmpty(unitDataInfoInfo.postType) ? Profession : unitDataInfoInfo.postType;
                m_Description = string.IsNullOrEmpty(unitDataInfoInfo.desc) ? m_Description : unitDataInfoInfo.desc;

                //init Skill
                for (var i = 0; i < mSkillList.Count; i++)
                {
                    if (mSkillList[i] == 0) continue;
                    SsitApplication.Instance.CreateSkill(null, mSkillList[i], InternalAddSkillCallback, player);
                }

                GroupId = ConstValue.c_sDefaultNpcGroupName;
            }
            /*else if (data is KeyValuePair<NpcDataInfo, UnitDataInfoInfo> keyValue)
            {
                unitDataInfoInfo = keyValue.Value;

                ID = unitDataInfoInfo.guid;
                m_DataId = unitDataInfoInfo.dataId;

                // 网络数据优先
                Name = unitDataInfoInfo.name;
                Profession = string.IsNullOrEmpty(unitDataInfoInfo.postType) ? Profession : unitDataInfoInfo.postType;
                m_Description = string.IsNullOrEmpty(unitDataInfoInfo.desc) ? m_Description : unitDataInfoInfo.desc;
                Department = unitDataInfoInfo.department;

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
                        player);
                }
                //todo:skilllist在数量上会存在异常
                //SsitDebug.Info($"expression{mSkillList.Count}");
            }*/
            else
            {
                //init Skill
                for (var i = 0; i < mSkillList.Count; i++)
                {
                    if (mSkillList[i] == 0) continue;
                    SsitApplication.Instance.CreateSkill(null, mSkillList[i], InternalAddSkillCallback, player);
                }
            }

            //初始化装备，目前装备是前台配置，后期如果改变到后台配置，挪上去
            for (var i = 0; i < mEquipList.Count; i++)
            {
                if (mEquipList[i] == 0) continue;

                SsitApplication.Instance.CreateItem(null, mEquipList[i], InternalAddEquipCallback, player);
            }
        }

        /// <summary>
        /// 通过当前的装备巢获取初始装备的ID  
        /// </summary>
        /// <param name="slot">要查询的装备巢</param>
        /// <returns>该装备巢上的原始装备</returns> 
        public Item GetOrignEquipBySlot( int slot )
        {
            foreach (var item in mEquipList)
                if (SsitApplication.Instance.GetEquipSlot(item) == slot)
                    return SsitApplication.Instance.CreateItem(null, item, InternalAddEquipCallback, GetParent());

            return null;
        }

        /// <summary>
        /// 是否含有该种装备的装备巢
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <returns></returns>
        public bool HasSlot( int slotIndex )
        {
            //return EquipInfos != null&&slotIndex<EquipInfos.Length&&EquipInfos[slotIndex]!=null;
            return mEquips != null && slotIndex < mEquips.Length && mEquips[slotIndex] != null;
        }

        #region 子类继承

        public override void NotifyPropertyChange( EnPropertyId propertyid, string param, object data )
        {
            base.NotifyPropertyChange(propertyid, param, data);
            switch (propertyid)
            {
                case EnPropertyId.State:
                {
                    mState = (EN_CharacterActionState) param.ParseByDefault(0);
                    ((Player) m_baseObject).ChangeState(mState);
                }
                    break;
                case EnPropertyId.OnState:
                {
                    mState = (EN_CharacterActionState) param.ParseByDefault(0);
                    isChange = true;
                }
                    break;
                case EnPropertyId.Interaction:
                {
                    var p = StringUtils.SplitStringByFormat(param);
                    //SsitApplication.Instance.CreatePlayer(p[0], TextUtils.ToInt(p[1]), InternalCreatePlayer, p[2]);
                    var player = GetParent();

                    if (p.Length <= 1)
                    {
                        player.RemoveInteractionObject((InvUseNodeType) p[0].ParseByDefault((double) 0), null);
                    }
                    else
                    {
                        var obj = Framework.SceneObject.ObjectManager.Instance.GetObject<BaseObject>(p[1]);
                        if (null != obj)
                            player.AddInteractionObject((InvUseNodeType) p[0].ParseByDefault(0), obj);
                    }
                }
                    break;
            }
        }

        #endregion

        #region 回放

        // 行为状态
        public EN_CharacterActionState mState = EN_CharacterActionState.EN_CHA_Stay;
        public EzInputs mEzInputs;

        public override bool InitBaseTrack()
        {
            mBaseTrack = new List<string>(3);
            mBaseTrack.Add(Name);
            mBaseTrack.Add(mGroupId);
            mBaseTrack.Add(Profession);
            //mInputs = new Inputs();
            return base.InitBaseTrack();
        }

        public override void ResetBaseTrack( List<string> mBaseTrack )
        {
            var isInit = string.IsNullOrEmpty(mGroupId);
            Name = mBaseTrack[0];
            mGroupId = mBaseTrack[1];
            Profession = mBaseTrack[2];
            var player = GetParent();
            if (isInit)
            {
                var tt = player.GetRepresent();
                player.InitHUD(tt.transform, ObjectHelper.GetPlayerName(GetParent()), string.Empty);
                if (tt.gameObject.activeSelf)
                {
                    player.Hud.IsActive = false;
                    player.Hud.SetHUDActive(false);
                }
            }
        }

        public override bool IsDifferentTo( SavedBase lastState, Object2PropertiesMapping o2m )
        {
            var attribute = lastState as PlayerAttribute;
            if (attribute == null)
                return false;
            GetParent().PlayerController.GetInputs(ref mEzInputs);

            var changed = base.IsDifferentTo(lastState, o2m);

            if (!changed && mEzInputs.isDifferentTo(attribute.mEzInputs))
                changed = true;

            return changed;
        }

        #endregion

        #region Internal Members

        public void NotificationEquipChange( int slot, Item item )
        {
            if (mEquips == null) mEquips = new SaveEquipInfo[(int) InvSlot.MaxValue];

            if (item != null)
            {
                if (mEquips[slot] == null)
                {
                    mEquips[slot] = new SaveEquipInfo
                    {
                        guid = item.Guid,
                        dataId = item.GetAttribute().Id,
                        state = (int) item.GetState()
                    };
                }
                else
                {
                    mEquips[slot].guid = item.Guid;
                    mEquips[slot].dataId = item.GetAttribute().Id;
                    mEquips[slot].state = (int) item.GetState();
                }
            }
            else
            {
                mEquips[slot] = null;
            }
            isChange = true;
        }

        public void NotificationUseChange( int slot, string guid )
        {
            if (mUseItem == null) mUseItem = new string[(int) InvUseNodeType.MaxValue];
            mUseItem[slot] = guid;
            isChange = true;
        }

        /// <summary>
        /// 装备创建回调
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="render"></param>
        /// <param name="data"></param>
        private void InternalAddEquipCallback( BaseObject obj, object render, object data )
        {
            if (data is Character player)
                player.PutOnEquipment((Item) obj);
        }

        /// <summary>
        /// 技能创建回调
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="render"></param>
        /// <param name="data"></param>
        private void InternalAddSkillCallback( BaseObject obj, object render, object data )
        {
            if (data is Player player)
                //Debug.LogError("InternalAddSkillCallback");
                player.AddSkill((Skill) obj);
        }

        #endregion

        #region Old Version

        //private ItemAtrribute[] equipInfos;
        //public ItemAtrribute[] mInitEquipInfosCache;
        //public ItemAtrribute[] EquipInfos
        //{
        //    get
        //    {
        //        return equipInfos;
        //    }

        //    set
        //    {
        //        equipInfos = value;
        //        if (EZReplayManager.Instance.CurrentAction == ActionMode.RECORD
        //        || EZReplayManager.Instance.getCurrentMode() == ViewMode.REPLAY)
        //        {
        //            equips_ezr = new int[value.Length];
        //        }
        //    }
        //}

        //public void InitEquipInfo()
        //{
        //  //  int count = (int)InvSlot.MaxValue;
        //    //EquipInfos = new ItemAtrribute[count];

        //    //if (EZReplayManager.Instance.CurrentAction == ActionMode.RECORD
        //    // || EZReplayManager.Instance.getCurrentMode() == ViewMode.REPLAY)
        //    //{
        //    //    equips_ezr = new int[count];
        //    //}
        //}

        //public void AddEquipInfo(int slot, ItemAtrribute item)
        //{
        //  //  EquipInfos[slot] = item;
        //    //if (EZReplayManager.Instance.CurrentAction == ActionMode.RECORD
        //    //|| EZReplayManager.Instance.getCurrentMode() == ViewMode.REPLAY)
        //    //{
        //    //    equips_ezr[slot] = item != null ? item.DataId : -1;
        //    //}
        //}

        #endregion
    }

    [Serializable]
    public class SaveEquipInfo
    {
        public int dataId;
        public string guid;
        public int state;

        public bool ExEquals( SaveEquipInfo obj )
        {
            if (obj == null)
                return false;

            if (obj.dataId != dataId || obj.state != state) return false;

            return true;
        }
    }

    [Serializable]
    public class EzInputs
    {
        public bool crouchInput;
        public SerVector2 input;
        public bool jumpInput;
        public bool rollInput;
        public bool sprintInput;
        public bool strafeInput;

        public float timeStamp;

        public EzInputs()
        {
            input = new SerVector2(0, 0);
        }

        public bool isDifferentTo( EzInputs ezInputs )
        {
            var changed = input.isDifferentTo(ezInputs.input);

            if (!changed && jumpInput != ezInputs.jumpInput)
                changed = true;

            if (!changed && rollInput != ezInputs.rollInput)
                changed = true;

            if (!changed && strafeInput != ezInputs.strafeInput)
                changed = true;

            if (!changed && sprintInput != ezInputs.sprintInput)
                changed = true;

            if (!changed && crouchInput != ezInputs.crouchInput)
                changed = true;

            return changed;
        }
    }
}