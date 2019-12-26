using System;
using System.Collections.Generic;
using SsitEngine.Unity.Utility;
using UnityEngine;

namespace Framework.Data
{
    /// <summary>
    /// 场景配置信息
    /// </summary>
    [Serializable]
    public class SceneDataInfo : DataInfo, ISerializationCallbackReceiver
    {
        public string SceneId;

        //毗邻区域列表
        public List<SceneRegionData> AroundRegionList = new List<SceneRegionData>();

        //功能分区列表
        public List<SceneRegionData> FunctionRegionList = new List<SceneRegionData>();

        public List<SceneObjectData> SceneObjectDataList = new List<SceneObjectData>();

        public void OnBeforeSerialize()
        {
            //更新场景普通交互对象数据信息
            foreach (var data in SceneObjectDataList)
            {
                if (data.sceneObj == null)
                    continue;
                data.position = data.sceneObj.SceneInstance.GetPositionAroud();
                data.angle = data.sceneObj.SceneInstance.GetAngles();
                data.scale = data.sceneObj.SceneInstance.GetScale();
                data.DisplayName = data.sceneObj.GetAttribute().Name;
            }
        }

        public void OnAfterDeserialize()
        {
        }

        public void DeleteById( string guid )
        {
            for (var i = 0; i < FunctionRegionList.Count; i++)
                if (FunctionRegionList[i].Guid == guid)
                {
                    FunctionRegionList.Remove(FunctionRegionList[i]);
                    break;
                }
        }
    }

    /// <summary>
    /// 功能分区信息
    /// </summary>
    [Serializable]
    public class SceneRegionData
    {
        public string AreaType;
        public string Desc;
        public string DisplayName;
        public string Guid;
        public string HideModel;
        public string LightModel;
        public string Postion;
        public string Rotation;
        public string SceneGuid;

        public SceneRegionData()
        {
            Guid = "";
            DisplayName = "";
            Desc = "";
            Postion = "";
            Rotation = "";
            HideModel = "";
            LightModel = "";
            AreaType = "";
            SceneGuid = "";
        }
    }

    [Serializable]
    public class SceneObjectData : InteractiveDataInfo, ISerializationCallbackReceiver
    {
        public string Desc;
        public string DisplayName;
        public Dictionary<En_SceneObjectExParam, string> ExtendParamList;

        [SerializeField] private string ExtendParamStr;

        public SceneObjectData()
        {
            ExtendParamList = new Dictionary<En_SceneObjectExParam, string>();
        }

        public SceneObjectData( string guid, int itemId ) : base(guid, itemId)
        {
            ExtendParamList = new Dictionary<En_SceneObjectExParam, string>();
        }

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

        public void OnBeforeSerialize()
        {
            if (ExtendParamList != null || ExtendParamList.Count > 0)
                ExtendParamStr = JsonUtility.ToJson(new Serialization<En_SceneObjectExParam, string>(ExtendParamList));
        }

        public void OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(ExtendParamStr))
                ExtendParamList = JsonUtility.FromJson<Serialization<En_SceneObjectExParam, string>>(ExtendParamStr)
                    .ToDictionary();
            else
                ExtendParamList = new Dictionary<En_SceneObjectExParam, string>();

            ExtendParamStr = null;
        }
    }
}