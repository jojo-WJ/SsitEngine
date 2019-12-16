/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：视图层基类                                                    
*│　作   者：Jusam                                        
*│　版   本：1.0.0                                                 
*│　创建时间：2019/04/29                        
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.PureMVC.Interfaces;

namespace SsitEngine.UI
{
    /// <summary>
    ///     OverLay中介者统一接口
    /// </summary>
    public interface IBaseView
    {
        /// <summary>
        ///     视图名称
        /// </summary>
        string ViewName { get; set; }

        /// <summary>
        ///     视图中介层
        /// </summary>
        IMediator Mediator { get; set; }

        /// <summary>
        ///     视图层
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetOverLay<T>() where T : IBaseView;

        /// <summary>
        ///     视图初始化
        /// </summary>
        void Init();

        /// <summary>
        ///     视图显示
        /// </summary>
        void Display();

        /// <summary>
        ///     视图隐藏
        /// </summary>
        void Hiding();

        /// <summary>
        /// 视图动画显示
        /// </summary>
        //void DisplayAnimation();

        /// <summary>
        /// 视图动画隐藏
        /// </summary>
        //void HidingAnimation();
    }
}