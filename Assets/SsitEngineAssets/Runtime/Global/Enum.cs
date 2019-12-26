/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：  系统业务通用枚举                                                  
*│　作   者：  xuxin                                            
*│　版   本：  1.0.0                                                 
*│　创建时间： 2019/10/29                           
*└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Framework
{
    /// <summary>
    /// 默认成员组
    /// </summary>
    public enum EnGroupType
    {
        EN_None = 0,

        /// <summary>
        /// 导调组
        /// </summary>
        EN_GUIDER = 1,

        /// <summary>
        /// 指挥组
        /// </summary>
        EN_ORDER = 2
    }

    /// <summary>
    /// 天气
    /// </summary>
    public enum EnWeather
    {
        SUN = 0,
        RAINY,
        SNOWY,
        FOG,
        WINDY,
        CLOUDY
    }

    /// <summary>
    /// 风向
    /// </summary>
    public enum EnWindDirection
    {
        NOWIND = 0,
        NORTH,
        NORTHEAST,
        EAST,
        SOUTHEAST,
        SOUTH,
        SOUTHWEST,
        WEST,
        NORTHWEST
    }


    public enum EnMessageType
    {
        ACTION, //行为、措施信息
        SELF, //本地发送聊天信息
        OTHER, //外部发送聊天信息
        SYSTEM //系统信息
    }


    /// <summary>
    /// 火焰粒子类型
    /// </summary>
    public enum En_FireParticleType
    {
    }

    // 火灾根据可燃物的类型和燃烧特性，分为A、B、C、D、E、F、K七类。可以来哪一类火灾的，就在灭火器上写明的，比如BC干粉，就可以灭B类和C类，ABC干粉，就可以灭A类、B类和C类。
    // A类火灾：指固体物质火灾。这种物质通常具有有机物质性质，一般在燃烧时能产生灼热的余烬。如木材、煤、棉、毛、麻、纸张等火灾。
    // B类火灾：指液体或可熔化的固体物质火灾。如煤油、柴油、原油，甲醇、乙醇、沥青、石蜡等火灾。
    // C类火灾：指气体火灾。如煤气、天然气、甲烷、乙烷、丙烷、氢气等火灾。
    // D类火灾：指金属火灾。如钾、钠、镁、铝镁合金等火灾。
    // E类火灾：带电火灾。物体带电燃烧的火灾。
    // F类火灾：烹饪器具内的烹饪物（如动植物油脂）火灾。
    // K类火灾：食用油类火灾。通常食用油的平均燃烧速率大于烃类油，与其他类型的液体火相比，食用油火很难被扑灭，由于有很多不同于烃类油火灾的行为，它被单独划分为一类火灾。
    // 干粉灭火剂的种类很多，大致可分为3类： 
    // 以碳酸氢钠(钾)为基料的干粉，用于扑灭易燃液体、气体和带电设备的火灾；即BC 
    // 以磷酸三铵、磷酸氢二铵、磷酸二氢铵及其混合物为基料的干粉，用于扑灭可燃固体、可燃液体、可燃气体及带电设备的火灾；即ABC 
    // 以氯化钠、氯化钾、氯化钡、碳酸钠等为基料的千粉，用于扑灭轻金属火灾。即BCD 

    /// <summary>
    /// 火灾类型
    /// </summary>
    [Flags]
    public enum En_FireType
    {
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,
        D = 1 << 3,
        E = 1 << 4,
        F = 1 << 5,

        K = 1 << 6
        //All = 1 << 7 - 1
    }

    /// <summary>
    /// 道具类型
    /// </summary>
    public enum EnItemType
    {
        ET_None = 0,

        /// <summary>
        /// 事故
        /// </summary>
        ET_Accident = 1,

        /// <summary>
        /// 普通类
        /// </summary>
        ET_Normal = 2,

        /// <summary>
        /// 任务类
        /// </summary>
        ET_Mission = 3,

        /// <summary>
        /// 角色模型类
        /// </summary>
        ET_Npc = 4,

        /// <summary>
        /// 后台编辑资源
        /// </summary>
        ET_EditRes = 5
    }


    public enum EnItemLayerType
    {
        EN_LA_Default = 0,
        EN_LA_Road,
        EN_LA_Ground,
        EN_LA_RoadAndGround
    }

    /// <summary>
    /// 操作组类型
    /// </summary>
    public enum ENOpGroupType
    {
        //1、普通信息查看
        //2、通用器具操作
        //3、通用载具操作
        //4、消防炮操作
        //5、通用人物操作
        //6、担架工操作

        //20、火焰操作
        //21、气体操作
        //22、障碍操作
        //23、伤员操作
        //24、远程阀门
        OP_None = 0,
        OP_looK = 1,
        OP_CAppliance = 2,
        OP_CCarry = 3,
        OP_XFP = 4,
        OP_Npc = 5,

        OP_Fire = 20,
        OP_Gas = 21,
        OP_Obstcale = 22,
        OP_Patient = 23,
        OP_RemoteSwitch = 24
    }

    /// <summary>
    /// 道具触发关联类型
    /// </summary>
    public enum ENItemRelationType
    {
        EN_None = 0,
        EN_Skill = 1,
        EN_Item = 2,
        EN_Group = 3,
        EN_OnlyClient = 4,
        EN_Guider = 5
    }

    public enum EnMoveType
    {
        STOP,
        LEFT,
        RIGHT,
        UP,
        DOWN,
        Aim
    }

    public enum EnDoMoveType
    {
        /// <summary>
        /// 操控式移动
        /// </summary>
        ED_CCMove,

        /// <summary>
        /// 导航式移动
        /// </summary>
        ED_NAVMove
    }

    public enum ENProcessState
    {
        EPS_Stay,
        EPS_Processing,
        EPS_ForceProcessing,

        EPS_Finished
    }

    public enum ENEquipmentType
    {
        //塔类
        //反 应 设 备 类 
        //储 罐 类
        //换 热 类
        //动 力 设 备 类
        //通用机械类
        EN_Tower = 0,
        EN_Reaction,
        EN_Storage,
        EN_Heat,
        EN_Power,
        EN_Mechanics
    }

    public enum En_SwitchState
    {
        Off // 0 false
        ,
        On // 1 true
        ,
        Idle // 
        ,
        Active // active
        ,
        Inactive // inactive
        ,
        Show // show means enable render
        ,
        Hide // hide means disable render

        ,
        Begin,
        Working,
        End

        // insert new state before this line
        ,
        Unknown // max
    }

    //技能类型
    public enum En_SkillType
    {
        EN_Active,
        EN_Passive
    }

    public enum EN_SkillMsgType
    {
        ///1、打开界面
        ///2、创建预设
        ///3、位置导航
        ///4、事件触发
        ///5、调度关联
        EN_OpenForm = 1,
        EN_CreateObject = 2,
        EN_Nav = 3,
        EN_EventPost = 4,
        EN_DispatchRelation = 5,
        EN_Normal = 6,

        //...
        EN_MaxValue
    }

    public enum En_SkillTriigleType
    {
        EN_None = 0,
        EN_Buttton = 1,
        EN_Toggle
    }

    public enum En_SkillConditionType
    {
        EN_None = 0,
        EN_Equip = 1,
        EN_UseEquip = 2,
        EN_UseSkillState = 3
    }


    public enum En_SceneObjectExParam
    {
        En_None = 0, // 无含义
        En_TagLogo = 1, // 标签logo大字
        En_TagDetail, // 标签小字
        En_TagId, // 标签ID
        En_TagType, // 标签类型
        En_TagName, // 标签自定义名字
        En_TagUiOrMesh, //标签界面UI或者模型mesh
        En_TagLogoName,


        //New
        En_Health, //损耗度|强度|生命度
        En_MaxHealth
    }


    public enum En_DeviceType
    {
        En_Pump // 泵
    }

    //通用操作面板状态同步
    public enum En_SyncOperatePanelType
    {
        En_Open,
        En_Close,
        En_Page,
        En_Select
    }

    public enum En_UIElementType
    {
        En_Form,
        En_Toggle,
        En_Dropdown,
        En_Button,
        En_Text,
        En_Click,
        En_ScrollRect,
        En_Slider,
        En_InputField
    }


    #region ObjectType

    public enum EnObjectType
    {
        BaseSceneObject, // BaseSceneObject
        Client, // 客户端代理
        Fire, // 火
        Valve, // 阀门
        Gas, // 气体
        Patient, // 伤员
        Obstacle, // 障碍物
        Headquarters, // 指挥部
        Door, // 门
        XFP, // 消防炮
        Vehicle, // 救护车
        SirenHand, // FireAlarm [ez]
        FireToolBox, // 灭火箱  fire extinguisher box
        Extinguisher, // 灭火器  fire extinguisher
        Annihilator, // 灭火器
        Cuvette, // 边沟
        PumpSwitch, // 泵开关
        Sandbag, // 消防沙袋
        GamePlayer, // 玩家角色
        Mound, // 警戒墩？？？
        SEntry, // 岗哨
        WaterPipe, // 消防水管
        WaterPipeHandle, // 消防水管手柄
        RallyPoint, // 集结点
        Chair, // 椅子
        Tag, // 标签
        Trigger, // 触发器
        BoundingBox, // 辅助模型
        XiYanQi, // 洗眼器
        ManualFireAnnihilator, // 手推式灭火器
        Telephone, // 电话
        MangBan, // 盲板
        WindVine, // 风向标
        AcoustoOpticAlarm, //声光报警器
        ProxyRecord // 回放代理
    }

    #endregion


    public enum En_ConfirmFormType
    {
        En_Common, //正常字体 白色
        En_Succeed, //操作成功 绿色
        En_Failed, //操作失败 红色
        En_Warning //警告 黄色
    }

    //任务步骤中，使用应急措施的步骤的数据扩展类型
    public enum En_TaskProcessExtraType
    {
        En_None,
        En_SinglePublishTask, //单人任务发布
        En_MultiPublishTask, //多人任务发布
        En_TelReport //电话汇报
    }

    //操作物体的类型
    public enum En_OperateObjectType
    {
        En_Open,
        En_Close
    }
}