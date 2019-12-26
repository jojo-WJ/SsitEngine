/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：资源监听器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/28 10:28:16                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace SsitEngine.Unity.Msic
{
    /// <summary>
    ///     资源监听器
    /// </summary>
    public class ResSerializerListener
    {
        protected bool mIsCollectd;
        protected List<string> mList;
        protected ResourceBase mResource;

        public ResSerializerListener( ResourceBase res )
        {
            mResource = res;
            mList = new List<string>();
            mIsCollectd = false;
        }

        public void AddResouces( string res )
        {
            mList.Add(res);
        }

        public void SetCollectd( bool val )
        {
            mIsCollectd = val;

            if (mIsCollectd && mList.Count == 0)
            {
                mIsCollectd = false;

                mResource.SetCollected(true);
            }
        }


        public void OnLoadCompleted( string name, ResourceBase res, object data )
        {
            mList.Remove(name);

            if (mIsCollectd && mList.Count == 0)
            {
                mIsCollectd = false;
                mResource.SetCollected(true);
                processCompleted(mResource);
            }
        }

        /// Allows to do changes on mesh after it's completely loaded. For example you can generate LOD levels here.
        public virtual void processCompleted( ResourceBase resource )
        {
        }
    }
}