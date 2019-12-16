/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：数据加载接口                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月9日                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using SsitEngine.Core;

namespace SsitEngine.Data
{
    using DataList = Dictionary<int, DataBase>;

    /// <summary>
    ///     数据加载接口
    /// </summary>
    public interface IDataLoader
    {
        /// <summary>
        ///     加载百分比
        /// </summary>
        float LoadPrecent { get; set; }

        /// <summary>
        ///     获取文件后缀名
        /// </summary>
        /// <returns></returns>
        string GetFileExt();

        /// <summary>
        ///     加载器初始化
        /// </summary>
        /// <returns></returns>
        Dictionary<int, IDataProxy> InitProxy( IDataManager dataManager );

        /// <summary>
        ///     加载数据
        /// </summary>
        /// <typeparam name="T">加载数据模型</typeparam>
        /// <param name="path">数据路径</param>
        /// <param name="isFlag">数据附加标识</param>
        /// <returns></returns>
        List<T> Load<T>( string path, bool isFlag = false ) where T : ModelBase;

        /// <summary>
        ///     加载数据
        /// </summary>
        /// <typeparam name="T">加载数据模型</typeparam>
        /// <param name="path">数据路径</param>
        /// <param name="isFlag">数据附加标识</param>
        /// <param name="func">数据附加标识</param>
        void Load<T>( string path, bool isFlag, SsitAction<Dictionary<string, T>> func ) where T : ModelBase;

        /// <summary>
        ///     保存数据
        /// </summary>
        /// <typeparam name="T">加载数据</typeparam>
        /// <param name="list">数据列表</param>
        /// <returns>返回保存数据字符串</returns>
        string Save<T>( DataList list ) where T : DataBase;
    }
}