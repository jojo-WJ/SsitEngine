using Framework.Data;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;

namespace SsitEngine.Unity.Avatar
{
    /// <summary>
    /// 装备巢
    /// </summary>
    public enum InvSlot
    {
        [EnumLabel("装备巢空")] InvSlotNone = -1,
        [EnumLabel("帽子")] InvHatNode = 0, // 帽子
        [EnumLabel("耳塞")] InvEarplugs, // 耳塞
        [EnumLabel("眼罩")] InvEyepatch, // 眼罩
        [EnumLabel("面具、口罩")] InvMask, // 面具、口罩
        [EnumLabel("眼镜")] InvGlasses, // 眼镜
        [EnumLabel("左胸包")] InvChestBagLeft, // 左胸包
        [EnumLabel("右胸包")] InvChestBagRight, // 右胸包
        [EnumLabel("背部辅助")] InvBackAFramework, // 背部辅助（呼吸器） 
        [EnumLabel("手套")] InvGlove, // 手套
        [EnumLabel("手部")] InvHand, // 手部
        [EnumLabel("大腿外包")] InvThighBag, // 大腿外包（对讲机）
        [EnumLabel("鞋子")] InvShoes, // 鞋子 
        [EnumLabel("衣服")] InvCloth = 12, // 衣服
        [EnumLabel("扳手")] InvSpanner = 13, // 扳手

        MaxValue
    }

    /// <summary>
    /// 装备使用类型
    /// </summary>
    public enum InvUseSlot
    {
        [EnumLabel("空")] InvUseNone = -1,
        [EnumLabel("手部")] InvUseHand = 0

        //...根据对象的
    }

    /// <summary>
    /// 装备使用的挂点类型
    /// </summary>
    public enum InvUseNodeType
    {
        [EnumLabel("空")] InvNone = -1,
        [EnumLabel("手部默认挂点")] InvUsHandNode, // 手部默认挂点
        [EnumLabel("对讲机挂点")] InvDJJNode, // 对讲机挂点

        [EnumLabel("灭火器挂点")] InvAnnihilatorNode, // 灭火器挂点
        [EnumLabel("消防软管挂点")] InvWaterPipeNode, // 消防软管挂点
        [EnumLabel("通用非挂点")] InvGeneryNode,

        [EnumLabel("通用搀扶挂点")] InvAssignNode,

        [EnumLabel("通用担架挂点")] InvStrechPatientNode,

        [EnumLabel("担架辅助人员挂点")] InvStrechNode,

        MaxValue
    }

    public enum InvCombineType
    {
        Inv_Normal = 0,
        Inv_Combine = 1,
        Inv_SimpleCombine = 2,
        Inv_SimpleBone = 3,

        Inv_ComMaterial = 4
    }

    public enum InvQuality
    {
        Inv_Quality_Normal = 0,
        Inv_Quality_Mid,
        Inv_Quality_High,
        Inv_Quality_Super
    }

    public class ItemAtrribute : BaseAtrribute
    {
        #region New Vesrsion 1.0

        protected string mName;

        //新版版本后期可能都由resoucesid共同维护，后面修正
        protected string mResources;

        /// <summary>
        /// 装备合并索引
        /// </summary>
        protected int mCombineIndex;

        /// <summary>
        /// 装备巢
        /// </summary>
        protected InvSlot mSlot;

        /// <summary>
        /// 使用巢
        /// </summary>
        protected InvUseSlot mUseSlot;

        //同一个插巢类型下，可能存在不同的挂点类型（其实不要这个也可以但是需要对装备道具配置，挂点偏移量【游戏中多使用此做法】
        //对于日前项目挂点数量相对较少，可以使用此方式，后期最好优化此项设置 ）
        protected InvUseNodeType mUseNodeType;

        /// <summary>
        /// 我的归属id
        /// </summary>
        protected Item mParent;

        /// <summary>
        /// 源数据id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public new string Name
        {
            get => mName;
            set => mName = value;
        }

        public override SavedBase Clone( bool isInit = true )
        {
            return base.Clone(isInit);
        }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string Resources
        {
            get => mResources;
            set => mResources = value;
        }

        /// <summary>
        /// 是否合并mesh
        /// </summary>
        public int CombineIndex
        {
            get => mCombineIndex;
            set => mCombineIndex = value;
        }

        /// <summary>
        /// 装备巢
        /// </summary>
        public InvSlot Slot
        {
            get => mSlot;
            set => mSlot = value;
        }

        /// <summary>
        /// 使用巢
        /// </summary>
        public InvUseSlot UseSlot
        {
            get => mUseSlot;
            set => mUseSlot = value;
        }

        public InvUseNodeType UseNodeType
        {
            get => mUseNodeType;
            set => mUseNodeType = value;
        }

        #endregion
    }
}