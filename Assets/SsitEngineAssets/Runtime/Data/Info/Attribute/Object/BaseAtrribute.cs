using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Framework.Mirror;
using Framework.SceneObject;
using SsitEngine;
using SsitEngine.EzReplay;
using SsitEngine.Unity.SceneObject;
using SsitEngine.Unity.Utility;
using UnityEngine;

namespace Framework.Data
{
    /// <summary>
    /// 场景内物体信息基类
    /// </summary>
    [Serializable]
    public class BaseAtrribute : ObjectData
    {
        //挂载属性的物体的对象
        protected BaseObject m_baseObject;
        protected int m_DataId;

        //描述信息
        protected string m_Description;
        protected string m_EditorPrefabPath = "";

        //物体图标
        protected int m_Icon;

        //物体在地图上的图标路径
        protected string m_MapIconPath;

        //显示名称
        protected string m_Name;

        //物体主体颜色
        protected SerColor m_ObjectColor = Color.white;

        //物体类型
        protected string m_ObjectType;

        //物体对应预制路径
        protected string m_PrefabPath = "";

        /*
         * 系统运行字段
         */
        //分组id
        protected string mGroupId;

        //物体点穿设置
        protected bool mIsThrough = true;

        //物体技能列表
        protected List<int> mItemList = new List<int>();

        //归属
        protected string mParentId;

        //物体放置层级
        protected int mPlaceLayer;

        //物体技能列表
        protected List<int> mSkillList = new List<int>();

        //物体关联的技能列表
        protected List<int> relationSkillList;

        //物体权限关联类型
        protected ENItemRelationType relationType;

        //物体细类
        protected int subType;

        //物体类型
        protected EnItemType type;

        //用户自定义名字
        protected string userCustomName;

        //用户自定义描述
        protected string UserCustomProfile;

        // 任务列表
        //public List<int> MissionList = new List<int>();

        public BaseAtrribute()
        {
            mIsThrough = true;
            ExtendParamList = new Dictionary<En_SceneObjectExParam, string>();
        }

        #region Property

        public int DataId
        {
            get => m_DataId;
            set => m_DataId = value;
        }

        public string PrefabPath
        {
            get => m_PrefabPath;

            set => m_PrefabPath = value;
        }

        public string EditorPrefabPath
        {
            get => m_EditorPrefabPath;

            set => m_EditorPrefabPath = value;
        }

        public string Name
        {
            get => m_Name;

            set => m_Name = value;
        }

        public string Description
        {
            get => m_Description;

            set => m_Description = value;
        }

        public int Icon
        {
            get => m_Icon;

            set => m_Icon = value;
        }

        public string MapIconPath
        {
            get => m_MapIconPath;

            set => m_MapIconPath = value;
        }

        public virtual Color ObjeColor
        {
            get => m_ObjectColor;
            set => m_ObjectColor = value;
        }

        public ENItemRelationType RelationType
        {
            get => relationType;
            set => relationType = value;
        }

        public EnItemType ItemType
        {
            get => type;
            set => type = value;
        }

        public int ItemSubType
        {
            get => subType;
            set => subType = value;
        }

        public List<int> RelationSkillList
        {
            get => relationSkillList;
            set => relationSkillList = value;
        }

        public bool IsThrough
        {
            get => mIsThrough;
            set => mIsThrough = value;
        }

        public List<int> SkillList
        {
            get => mSkillList;
            set => mSkillList = value;
        }

        public List<int> ItemList
        {
            get => mItemList;
            set => mItemList = value;
        }

        public string GroupId
        {
            get => mGroupId;
            set => mGroupId = value;
        }

        public int PlaceLayer
        {
            get => mPlaceLayer;
            set => mPlaceLayer = value;
        }

        public string Authority
        {
            get => mParentId;
            set => mParentId = value;
        }

        #endregion

        #region 老版本

        /// <summary>
        /// 获取场景物体组件
        /// </summary>
        /// <returns></returns>
        public BaseSceneInstance GetSceneObject()
        {
            return m_baseObject?.SceneInstance;
        }

        /// <summary>
        /// 设置物体颜色
        /// </summary>
        /// <param name="color">颜色</param>
        public virtual void SetObjectColor( Color color )
        {
            ObjeColor = color;
            //sceneInstance?.ChangeColor(color);
        }

        /// <summary>
        /// 获取物体颜色
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public Color GetObjectColor( Color color )
        {
            return ObjeColor;
        }

        #endregion

        #region 新版本

        /// <summary>
        /// 参数列表
        /// </summary>
        public Dictionary<En_SceneObjectExParam, string> ExtendParamList;

        /// <summary>
        /// 操作组类型
        /// </summary>
        public ENOpGroupType OperateGroupType;

        private NetPlayerAgent mAgent;

        /// <summary>
        /// 扩展参数访问设置器
        /// </summary>
        /// <param name="exParam"></param>
        /// <returns></returns>
        public string this[ En_SceneObjectExParam exParam ]
        {
            set
            {
                if (ExtendParamList.ContainsKey(exParam))
                    ExtendParamList[exParam] = value;
                else
                    ExtendParamList.Add(exParam, value);
            }

            get
            {
                if (!ExtendParamList.ContainsKey(exParam)) return "";
                return ExtendParamList[exParam];
            }
        }

        /// <summary>
        /// 设置属性归属
        /// </summary>
        /// <param name="owner"></param>
        public void SetParent( BaseObject owner )
        {
            m_baseObject = owner;
        }

        public BaseObject GetParent()
        {
            return m_baseObject;
        }

        public string GetAuthority()
        {
            return mParentId;
        }

        /// <summary>
        /// 应用网络数据
        /// </summary>
        /// <param name="data"></param>
        public virtual void Apply( object data )
        {
            if (data is SceneObjectData sceneData)
            {
                Name = sceneData.DisplayName;
                Description = sceneData.Desc;

                ExtendParamList = sceneData.ExtendParamList;

                // show tag
                var is3DTag = sceneData[En_SceneObjectExParam.En_TagUiOrMesh];
                if (is3DTag.ParseByDefault(false, SsitFrameUtils.DefaultPriority.ToString()))
                    m_baseObject.SetProperty(EnPropertyId.Show3DTag, null, data);
            }

            //init Skill
            for (var i = 0; i < mSkillList.Count; i++)
            {
                if (mSkillList[i] == 0) continue;
                SsitApplication.Instance.CreateSkill(null, mSkillList[i], InternalAddSkillCallback, m_baseObject);
            }
        }

        /// <summary>
        /// 内部添加技能回调
        /// </summary>
        private void InternalAddSkillCallback( BaseObject obj, object render, object data )
        {
            m_baseObject.AddSkill((Skill) obj);
        }


        /// <summary>
        /// 属性改变回调
        /// </summary>
        public virtual void NotifyPropertyChange( EnPropertyId propertyid, string param, object data )
        {
            switch (propertyid)
            {
                case EnPropertyId.Authority:
                {
                    mParentId = param;
                    m_baseObject.SetParent(param);
                    isChange = true;
                }
                    break;
            }
        }

        /// <summary>
        /// 获取指定的属性id映射的属性字段
        /// </summary>
        public virtual string GetProperty( EnPropertyId propertyId )
        {
            switch (propertyId)
            {
                case EnPropertyId.Position:
                    return m_baseObject?.SceneInstance?.GetPosition().ToString();
                case EnPropertyId.Rotate:
                    return m_baseObject?.SceneInstance?.GetOrientation().ToString();
                case EnPropertyId.Scale:
                    return m_baseObject?.SceneInstance?.GetScale().ToString();
                case EnPropertyId.Authority:
                    return mParentId;
                default:
                    return null;
            }
        }

        #endregion


        #region 序列化

        public BaseAtrribute( SerializationInfo info, StreamingContext context ) : base(info, context)
        {
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData(info, context);
        }

        #endregion

        #region 回放

        public BaseAtrribute( GameObject go ) : base(go)
        {
            mIsThrough = true;
            ExtendParamList = new Dictionary<En_SceneObjectExParam, string>();
        }

        public override SavedBase Clone( bool isInit = true )
        {
            return SerializationUtils.Clone(this);
        }

        public override bool IsDifferentTo( SavedBase state, Object2PropertiesMapping o2m )
        {
            if (IsChange)
                return IsChange;

            return base.IsDifferentTo(state, o2m);
        }

        #endregion
    }

}