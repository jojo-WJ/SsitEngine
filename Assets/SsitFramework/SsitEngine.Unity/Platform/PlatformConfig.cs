/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/29 15:12:02                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;

namespace SsitEngine.Unity
{
    /// <summary>
    /// 平台的配置文件
    /// </summary>
    [CreateAssetMenu(menuName = "SsitEngine.Unity/Config", order = 999)]
    public class PlatformConfig : ScriptableObject
    {
        /// <summary>
        ///     网络同步
        /// </summary>
        public bool isSync = true;

        /// <summary>
        ///     移动附加速度
        /// </summary>
        public float movAttachSpeed = 5f;

        //---------
        //--Input
        //---------
        /// <summary>
        ///     移动速度
        /// </summary>
        public float movSpeed = 6f;


        /// <summary>
        ///     旋转旋转误差允许
        /// </summary>
        public Vector2 rotPermissiDelta = new Vector2(2, 2);

        /// <summary>
        ///     旋转速度
        /// </summary>
        public float rotSpeed = 200f;

        /// <summary>
        ///     最大调度
        /// </summary>
        public float rotXAxisMax = 85f;

        /// <summary>
        ///     最小角度
        /// </summary>
        public float rotXAxisMin = 5f;

        /// <summary>
        ///     选择的条目
        /// </summary>
        [SerializeField] private int selectedTab;

        /// <summary>
        ///     屏幕睡眠时间
        /// </summary>
        [Range(-1, 1000)] public int sleepTimeout = -1;

        /// <summary>
        ///     帧率
        /// </summary>
        public int targetFrameRate = 60;

        /// <summary>
        ///     计时系统任务代理的最大个数
        /// </summary>
        public ushort timeTaskAgentMaxCount;

        /// <summary>
        ///     计时系统任务代理的最大个数
        /// </summary>
        public ushort webTaskAgentMaxCount;

        /// <summary>
        ///     最大缩放
        /// </summary>
        public float zoomMax = 200f;

        /// <summary>
        ///     最小缩放
        /// </summary>
        public float zoomMin = 5f;

        /// <summary>
        ///     旋转速度
        /// </summary>
        public float zoomSpeed = 200f;

        /// <summary>
        ///     配置构造
        /// </summary>
        public PlatformConfig()
        {
            targetFrameRate = 60;
            timeTaskAgentMaxCount = 50;
            webTaskAgentMaxCount = 20;
            sleepTimeout = -1;
            zoomSpeed = 200f;
            zoomMin = 5f;
            zoomMax = 200f;
            rotXAxisMin = 5f;
            rotXAxisMax = 85f;
        }
    }
}