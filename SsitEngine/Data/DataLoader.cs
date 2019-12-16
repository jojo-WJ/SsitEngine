/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/9 12:01:40                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using SsitEngine.Core;

namespace SsitEngine.Data
{
    using DataList = Dictionary<int, DataBase>;

    /// <summary>
    ///     数据加载器的抽象类
    /// </summary>
    public abstract class DataLoader : AllocatedObject, IDataLoader
    {
        /// <inheritdoc />
        public abstract string GetFileExt();

        /// <inheritdoc />
        public abstract Dictionary<int, IDataProxy> InitProxy( IDataManager dataManager );

        /// <inheritdoc />
        public abstract List<T> Load<T>( string path, bool isFlag = false ) where T : ModelBase;

        public abstract void Load<T>( string path, bool isFlag, SsitAction<Dictionary<string, T>> func )
            where T : ModelBase;

        /// <inheritdoc />
        public abstract string Save<T>( DataList list ) where T : DataBase;

        public virtual float LoadPrecent { get; set; }
    }
}