/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：车辆引擎                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/9/9 16:58:13                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity;

namespace Framework.SceneObject
{
    public class VechileController : MonoBase
    {
        /// <summary>
        /// 速度类型
        /// </summary>
        public enum SpeedType
        {
            /// <summary>
            /// 英里/小时
            /// </summary>
            MPH,

            /// <summary>
            /// 千米/小时
            /// </summary>
            KPH
        }

        /// <summary>
        /// 车辆引擎类型
        /// </summary>
        internal enum CarDriveType
        {
            /// <summary>
            /// 前驱
            /// </summary>
            FrontWheelDrive,

            /// <summary>
            /// 后驱
            /// </summary>
            RearWheelDrive,

            /// <summary>
            /// 四驱
            /// </summary>
            FourWheelDrive
        }

        //todo:扭矩运动
    }
}