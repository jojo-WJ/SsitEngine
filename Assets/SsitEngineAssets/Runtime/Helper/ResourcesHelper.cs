/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：                                                    
*│　作    者：xuxin                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/12/04 16:01:09                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SsitEngine;
using SsitEngine.DebugLog;
using SsitEngine.Unity.Data;
using SsitEngine.Unity.Resource;
using SsitEngine.Unity.Resource.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace Framework.Helper
{
    public class ResourceHelper : AssetLoaderHelper
    {
        /// <summary>
        /// 图集资源(资源id)
        /// </summary>
        public static readonly Dictionary<int, AsyncOperationHandle<SpriteAtlas>> m_atlasMaps =
            new Dictionary<int, AsyncOperationHandle<SpriteAtlas>>();

        /// <inheritdoc />
        public override void UnloadAsset<T>( Object obj )
        {
        }

        #region 寻址接入

        /// <summary>
        /// 资源寻址加载
        /// </summary>
        /// <param name="resKey">资源key</param>
        public override void SyncLoadAddressableSpriteAtlas( int resKey )
        {
            //todo:查询所有所有respath

            //string companyName = ConfigManager.Instance.DataIndex
            //Addressables.LoadResourceLocationsAsync(new List<object>() {resKey，label}, Addressables.MergeMode.UseFirst);

            var atlasData = DataManager.Instance.GetData<IAtlasData>((int) EnDataType.DATA_ATLAS, resKey);

            if (atlasData == null) throw new SsitEngineException("Atlas 资源配置不存在：" + resKey);

            var resData =
                DataManager.Instance.GetData<IResourceData>((int) EnDataType.DATA_Res, atlasData.ResourceId);
            if (resData == null) throw new SsitEngineException("Resources 资源配置不存在：" + atlasData.ResourceId);

            var handler = Addressables.LoadAssetAsync<SpriteAtlas>(resData.BundleId);

            handler.Completed += handle =>
            {
                if (handler.Status == AsyncOperationStatus.Succeeded)
                    m_atlasMaps.Add(resKey, handler);
                else
                    SsitDebug.Error("SyncAddressableLoad load exception");
            };
        }

        #endregion


        public override void Shutdown()
        {
            base.Shutdown();
            m_atlasMaps.Clear();
        }

        #region Sprite Loader

        /// <summary>
        /// 加载图集资源
        /// </summary>
        /// <param name="spriteId">精灵图片Id</param>
        /// <param name="loadSpriteFunc">加载回调</param>
        public override void LoadSpriteAsset( int spriteId, UnityAction<Sprite> loadSpriteFunc )
        {
            var textureData = DataManager.Instance.GetData<ITextureData>((int) EnDataType.DATA_TEXTURE, spriteId);

            if (textureData == null) throw new SsitEngineException("TextureData 资源配置不存在：" + spriteId);

            var resData =
                DataManager.Instance.GetData<IResourceData>((int) EnDataType.DATA_Res, textureData.ResourceId);
            if (resData == null) throw new SsitEngineException("Resources 资源配置不存在：" + textureData.ResourceId);

            LoadSpriteAtlasAsset(textureData.AtlasId, resData.BundleId, loadSpriteFunc);
        }

        /// <summary>
        /// 加载图集资源
        /// </summary>
        /// <param name="atlasId">图集Id</param>
        /// <param name="spriteName">精灵图片名称</param>
        /// <param name="loadSpriteFunc">加载回调</param>
        public override void LoadSpriteAtlasAsset( int atlasId, string spriteName, UnityAction<Sprite> loadSpriteFunc )
        {
            if (m_atlasMaps.TryGetValue(atlasId, out var atlas))
            {
                var sprite = atlas.Result.GetSprite(spriteName);
                if (sprite == null) SsitDebug.Debug($"{spriteName} is not {atlas.Result.name}");
                loadSpriteFunc?.Invoke(sprite);
            }
            else
            {
                SsitDebug.Error("请在进程中预加载进程图集");
            }
        }

        /// <summary>
        /// 卸载图集
        /// </summary>
        /// <param name="atlasId">图集Id</param>
        public override void UnLoadSpriteAtlasAsset( int atlasId )
        {
            if (m_atlasMaps.TryGetValue(atlasId, out _))
            {
                Addressables.Release(m_atlasMaps[atlasId]);
                m_atlasMaps.Remove(atlasId);
            }
        }

        #endregion
    }

    public static class ResourceUtility
    {
        public static TaskAwaiter<T> GetAwaiter<T>( this AsyncOperationHandle<T> ap )
        {
            var tcs = new TaskCompletionSource<T>();
            ap.Completed += op => tcs.TrySetResult(op.Result);
            return tcs.Task.GetAwaiter();
        }
    }
}