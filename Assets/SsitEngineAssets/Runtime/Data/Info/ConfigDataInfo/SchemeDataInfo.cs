using System;
using System.Collections.Generic;
using Framework.Logic;
using Framework.SceneObject;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;

namespace Framework.Data
{
    /// <summary>
    /// 参演单位类型
    /// </summary>
    public enum ENForceUnitType
    {
        EN_Character = 0,
        EN_ForceUnit,
        EN_Npc
    }

    /// <summary>
    /// 预案配置文件
    /// </summary>
    [Serializable]
    public class SchemeDataInfo : DataInfo, ISerializationCallbackReceiver
    {
        // 去除事故列表进行配置数据结构统一
        //public List<AccidentDataInfoInfo> AccList = new List<AccidentDataInfoInfo>();
        public List<SceneObjectData> objList = new List<SceneObjectData>();
        public string schemeId;
        public List<TaskProcedureDataInfo> taskProcedureList = new List<TaskProcedureDataInfo>();
        public List<UnitDataInfoInfo> UintList = new List<UnitDataInfoInfo>();
        public WeatherDataInfo Weather;

        public void OnBeforeSerialize()
        {
            if (Facade.Instance.RetrieveProxy(SceneInfoProxy.NAME) is SceneInfoProxy sceneInfoProxy)
            {
                Weather.Weather = sceneInfoProxy.GetWeatherInfo().Weather;
                Weather.WindDirection = sceneInfoProxy.GetWeatherInfo().WindDirection;
                Weather.WindLevel = sceneInfoProxy.GetWindLevel();
                Weather.WindVelocity =(float)sceneInfoProxy.GetWeatherInfo().WindVelocity;
            }
            
            //更新场景普通交互对象数据信息
            foreach (var data in objList)
            {
                if (data.sceneObj == null)
                    continue;
                data.position = data.sceneObj.SceneInstance.GetPosition();
                data.angle = data.sceneObj.SceneInstance.GetAngles();
                data.scale = data.sceneObj.SceneInstance.GetScale();
                data.ExtendParamList = data.ExtendParamList;
            }

            //更新参演力量的单位数据信息
            foreach (var data in UintList)
            {
                if (data.sceneObj == null)
                    continue;
                data.position = data.sceneObj.SceneInstance.GetPosition();
                data.angle = data.sceneObj.SceneInstance.GetAngles();
                data.scale = data.sceneObj.SceneInstance.GetScale();

                if (data.sceneObj.GetAttribute() is PlayerAttribute)
                {
                    var attribute = data.sceneObj.GetAttribute() as PlayerAttribute;

                    data.guid = data.sceneObj.Guid;
                    data.name = attribute.Name;
                    data.dataId = attribute.DataId;
                    data.department = attribute.Department;
                    data.postType = attribute.Profession;
                    data.desc = attribute.Description;
                    data.action = attribute.actionName;
                }
                else if (data.sceneObj.GetAttribute() is VehicleAttribute)
                {
                    var info = data.sceneObj.GetAttribute() as VehicleAttribute;

                    data.guid = data.sceneObj.Guid;
                    data.name = info.Name;
                    data.dataId = info.DataId;
                    data.department = info.department;
                    data.postType = info.profession;
                    data.desc = info.Description;
                }
            }
        }

        public void OnAfterDeserialize()
        {
        }

        //todo:其它的对象列表
    }

    /// <summary>
    /// 场景预案交互数据的基类
    /// </summary>
    [Serializable]
    public class InteractiveDataInfo
    {
        public Vector3 angle;
        public int dataId;
        public string guid;
        public Vector3 position;
        public Vector3 scale;

        [NonSerialized] public BaseObject sceneObj;

        // gameobject的构造、销毁，都需要做好关联
        [NonSerialized] public int type;

        public InteractiveDataInfo()
        {
        }

        public InteractiveDataInfo( string guid, int itemId )
        {
            this.guid = guid;
            dataId = itemId;
        }
    }

    /// <summary>
    /// 参演力量单位信息
    /// </summary>
    [Serializable]
    public class UnitDataInfoInfo : InteractiveDataInfo
    {
        public string action; // 动作
        public int department; // 类别（0：player 1：vechile 2:npc）
        public string desc;
        public string name;
        public string postType; // 岗位
    }

    /// <summary>
    /// 事故交互对象的信息
    /// </summary>
    [Serializable]
    public class AccidentDataInfoInfo : InteractiveDataInfo
    {
        //颜色
        public SerColor color;
        public SerColor smokecolor;
    }

    /// <summary>
    /// 天气信息
    /// </summary>
    [Serializable]
    public class WeatherDataInfo
    {
        public EnWeather Weather; //天气类型
        public EnWindDirection WindDirection; //风向
        public int WindLevel; //风级
        public float WindVelocity; //风速
    }

    /// <summary>
    /// 任务流程信息
    /// </summary>
    [Serializable]
    public class TaskProcedureDataInfo
    {
        public string ID;
        public List<string> taskIdList;
    }
}