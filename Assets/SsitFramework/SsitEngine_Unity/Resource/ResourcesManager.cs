/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：资源管理模块                                                    
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/04/29                        
*└──────────────────────────────────────────────────────────────┘
*/

using System.IO;
using SsitEngine.DebugLog;
using SsitEngine.Unity.Config;
using SsitEngine.Unity.Scene;
using SsitEngine.Unity.WebRequest;
using UnityEngine;
using UnityEngine.Events;
using static SsitEngine.Unity.FileUtils;

namespace SsitEngine.Unity.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourcesManager : ManagerBase<ResourcesManager>, IResourceManager
    {
        
        #region Interval
        
        /// <summary>
        /// 资源辅助器
        /// </summary>
        public AssetLoaderHelper LoaderHelper { get; private set; }
        
        /// <summary>
        /// 设置资源加载辅助器
        /// </summary>
        public void SetResourecesLoaderHelper( AssetLoaderHelper helper )
        {
            LoaderHelper = helper;
        }


        #endregion

        #region load common resouces

        /// <summary>
        /// 加载通用资源
        /// </summary>
        /// <param name="resPath"></param>
        /// <param name="isAsync"></param>
        /// <param name="complete"></param>
        public void LoadAsset<T>( string resPath, bool isAsync, UnityAction<T> complete ) where T : Object
        {
            if (isAsync)
                LoaderHelper.SyncLoadAsset(resPath, complete);
            else
                LoaderHelper.LoadAsset(resPath, complete);
        }
        
        /// <summary>
        /// 加载通用资源
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isAsync"></param>
        /// <param name="complete"></param>
        public void LoadAsset<T>( int id, bool isAsync, UnityAction<T> complete ) where T : Object
        {
            if (isAsync)
                LoaderHelper.SyncLoadAsset(id, complete);
            else
                LoaderHelper.LoadAsset(id, complete);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public void UnLoadAsset<T>( Object obj )
        {
            LoaderHelper.UnloadAsset<T>(obj);
        }

        #endregion

        #region 上传/下载文件

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="fileDatas">数据</param>
        /// <param name="postPath">上传文件类型ID</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="complete"></param>
        /// <param name="failture"></param>
        /// <param name="progress"></param>
        public ulong UpLoadAsset( byte[] fileDatas, string postPath, string fileName, UnityAction<string> complete,
            UnityAction<string> failture, UnityAction<float> progress = null )
        {
            if (fileDatas.Length == 0)
            {
                SsitDebug.Error("上传数据长度为 0");
                return 0;
            }
            return Engine.Instance.Platform.AddWebRequestTask(new WebRequestInfo
            {
                Url = ConfigManager.HttpIpPort + HttpNetWorkAction.Upload_File,
                FileType = ENRequestAssetType.EN_File,
                PostData = fileDatas,
                PostFilePath = postPath,
                FileName = fileName,
                WebRequestType = EnWebRequestType.EN_POST,
                CompleteAction = o => { complete?.Invoke((string) o); },

                FailedAction = o => { failture?.Invoke(o); },

                RequestProcessAction = x => { progress?.Invoke(x); }
            });
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="postPath">文件上传路径</param>
        /// <param name="complete">完成回调</param>
        /// <param name="failture">失败回调</param>
        /// <param name="progress">进度（为实现）</param>
        public ulong UpLoadAsset( string filePath, string postPath, UnityAction<string> complete,
            UnityAction<string> failture, UnityAction<float> progress = null )
        {
            if (!IsExistFile(filePath))
            {
                SsitDebug.Error("上传文件不存在");
                return 0;
            }
            return Engine.Instance.Platform.AddWebRequestTask(new WebRequestInfo
            {
                Url = ConfigManager.HttpIpPort + HttpNetWorkAction.Upload_File,
                FileType = ENRequestAssetType.EN_File,
                PostData = File.ReadAllBytes(filePath),
                PostFilePath = postPath,
                FileName = GetFileNameFromPath(filePath),
                WebRequestType = EnWebRequestType.EN_POST,
                CompleteAction = o => { complete?.Invoke((string) o); },

                FailedAction = o => { failture?.Invoke(o); },

                RequestProcessAction = x => { progress?.Invoke(x); }
            });
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="uuid">资源uuid</param>
        /// <param name="folderPath">资源路径</param>
        /// <param name="complete"></param>
        /// <param name="failture"></param>
        /// <param name="progress"></param>
        public ulong LoadWebFile( string uuid, string folderPath, UnityAction<string> complete,
            UnityAction<string> failture, UnityAction<float> progress = null )
        {
            var tempPath = GetFileWithOutExtension(uuid, Path.Combine(PathUtils.GetPersistentDataPath(), folderPath));
            Debug.Log($"LoadWebFile tempPath {tempPath}");
            if (IsExistFile(tempPath))
                return Engine.Instance.Platform.AddWebRequestTask(new WebRequestInfo
                {
                    Url = tempPath,
                    FileType = ENRequestAssetType.EN_File,
                    Uuid = uuid,
                    WebRequestType = EnWebRequestType.EN_GETMEMORY,
                    CompleteAction = o => { complete?.Invoke((string) o); },

                    FailedAction = o => { failture?.Invoke(o); }
                });
            return Engine.Instance.Platform.AddWebRequestTask(new WebRequestInfo
            {
                Url = ConfigManager.HttpIpPort + HttpNetWorkAction.Download_File + uuid,
                FileType = ENRequestAssetType.EN_File,
                Uuid = uuid,
                WebRequestType = EnWebRequestType.EN_GET,
                CompleteAction = o => { complete?.Invoke((string) o); },

                FailedAction = o => { failture?.Invoke(o); }
            });
        }

        /// <summary>
        /// 从服务器加载sprite
        /// </summary>
        /// <param name="uuid">文件uuid</param>
        /// <param name="folderPath">文件获取文件夹路径</param>
        /// <param name="complete"></param>
        public ulong LoadWebSprite( string uuid, string folderPath, UnityAction<Sprite> complete )
        {
            if (string.IsNullOrEmpty(uuid))
            {
                SsitDebug.Warning("请求的文件uuid为空");
                return 0;
            }
            string tempPath = GetFileWithOutExtension(uuid, Path.Combine(PathUtils.GetPersistentDataPath(), folderPath));
            if (!IsExistFile(tempPath))
            {
                return Engine.Instance.Platform.AddWebRequestTask(new WebRequestInfo()
                {

                    Url = ConfigManager.HttpIpPort + HttpNetWorkAction.Download_File + uuid,
                    FileType = ENRequestAssetType.EN_Texture,
                    WebRequestType = EnWebRequestType.EN_GET,
                    Uuid = uuid,
                    CompleteAction = o =>
                    {
                        Texture2D t = o as Texture2D;

                        if (t != null)
                        {
                            Sprite icon = Sprite.Create(t, new Rect(0, 0, t.width, t.height), Vector2.zero);
                            complete.Invoke(icon);
                        }
                    }

                });
            }
            Debug.Log($" LoadWebSprite :: tempPath{ tempPath} ");
            
            return Engine.Instance.Platform.AddWebRequestTask(new WebRequestInfo()
            {
                Url = tempPath,
                FileType = ENRequestAssetType.EN_Texture,
                WebRequestType = EnWebRequestType.EN_GETMEMORY,
                Uuid = uuid,
                CompleteAction = o =>
                {
                    Texture2D t = o as Texture2D;

                    if (t != null)
                    {
                        Sprite icon = Sprite.Create(t, new Rect(0, 0, t.width, t.height), Vector2.zero);
                        complete.Invoke(icon);
                    }
                }
            });
        }
        
        /// <summary>
        /// 服务器请求下载文件
        /// </summary>
        /// <param name="uuid">文件uuid</param>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="complete">下载完成回调</param>
        /// <param name="failture">下载失败回调</param>
        /// <param name="progress">下载进度回调【未实现等待需求添加】</param>
        public void LoadWebTextFile( string uuid, string folderPath, UnityAction<string> complete, UnityAction<string> failture, UnityAction<float> progress = null )
        {
            string tempPath = GetFileWithOutExtension(uuid, Path.Combine(PathUtils.GetPersistentDataPath(), folderPath));
            if (!IsExistFile(tempPath))
            {
                Engine.Instance.Platform.AddWebRequestTask(new WebRequestInfo()
                {
                    Url = ConfigManager.HttpIpPort + HttpNetWorkAction.Download_File + uuid,
                    FileType = ENRequestAssetType.EN_Text,
                    WebRequestType = EnWebRequestType.EN_GET,
                    Uuid = uuid,
                    CompleteAction = o =>
                    {
                        complete?.Invoke((string)o);
                    },

                    FailedAction = o =>
                    {
                        failture?.Invoke(o);
                    }
                });
            }
            else
            {
                Debug.Log($" LoadWebTextFile :: tempPath{tempPath} ");

                Engine.Instance.Platform.AddWebRequestTask(new WebRequestInfo()
                {
                    Url = tempPath,
                    FileType = ENRequestAssetType.EN_Text,
                    WebRequestType = EnWebRequestType.EN_GETMEMORY,
                    Uuid = uuid,
                    CompleteAction = o =>
                    {
                        complete?.Invoke((string)o);
                    },

                    FailedAction = o =>
                    {
                        failture?.Invoke(o);
                    }
                });
            }

        }

        /// <summary>
        /// 服务器请求下载文件流 by xuxin
        /// </summary>
        /// <param name="uuid">文件uuid</param>
        /// <param name="folderPath">文件uuid</param>
        /// <param name="complete">下载完成回调</param>
        /// <param name="failture">下载失败回调</param>
        /// <param name="progress">下载进度回调【未实现等待需求添加】</param>
        public void LoadWebTextBytes( string uuid, string folderPath, UnityAction<byte[]> complete, UnityAction<string> failture, UnityAction<float> progress = null )
        {
            string tempPath = GetFileWithOutExtension(uuid, Path.Combine(PathUtils.GetPersistentDataPath(), folderPath));

            Debug.Log($" LoadWebTextBytes :: tempPath{tempPath} ");
            if (!IsExistFile(tempPath))
            {
                Engine.Instance.Platform.AddWebRequestTask(new WebRequestInfo()
                {
                    Url = ConfigManager.HttpIpPort + HttpNetWorkAction.Download_File + uuid,
                    FileType = ENRequestAssetType.EN_Text,
                    WebRequestType = EnWebRequestType.EN_GET,
                    FileName = "byte",//add tag
                    Uuid = uuid,
                    CompleteAction = o =>
                    {
                        complete?.Invoke((byte[])o);
                    },

                    FailedAction = o =>
                    {
                        failture?.Invoke(o);
                    }
                });
            }
            else
            {
                Engine.Instance.Platform.AddWebRequestTask(new WebRequestInfo()
                {
                    Url = tempPath,
                    FileType = ENRequestAssetType.EN_Text,
                    WebRequestType = EnWebRequestType.EN_GETMEMORY,
                    FileName = "byte",//add tag
                    Uuid = uuid,
                    CompleteAction = o =>
                    {
                        complete?.Invoke((byte[])o);
                    },

                    FailedAction = o =>
                    {
                        failture?.Invoke(o);
                    }
                });
            }


        }
        
        #endregion

        #region 模块管理

        public override void OnSingletonInit()
        {
        }

        public override string ModuleName => typeof(ResourcesManager).FullName;

        public override int Priority => 11;


        public override void OnUpdate( float elapseSeconds )
        {
        }

        public override void Shutdown()
        {
            base.Shutdown();
            LoaderHelper.Shutdown();
            LoaderHelper = null;
        }

        #endregion

        #region Scene

        public void UnloadScene( string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData )
        {
        }

        public void LoadScene( string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks,
            object userData )
        {
        }

        #endregion

        #region Atlas Sprite 2D 资源-接入寻址系统

        /// <summary>
        /// 加载图集资源
        /// </summary>
        /// <param name="atlasId">图集Id</param>
        public void SyncLoadAddressableSpriteAtlas( int atlasId )
        {
            LoaderHelper?.SyncLoadAddressableSpriteAtlas(atlasId);
        }
        
        /// <summary>
        /// 卸载图集
        /// </summary>
        /// <param name="atlasId">图集Id</param>
        public void UnLoadSpriteAtlas( int atlasId )
        {
            LoaderHelper?.UnLoadSpriteAtlasAsset(atlasId);
        }
        
        /// <summary>
        /// 加入寻址后获取图片精灵
        /// </summary>
        /// <param name="spriteId"></param>
        /// <param name="loadSpriteFunc"></param>
        public void LoadSpriteAsset( int spriteId, UnityAction<Sprite> loadSpriteFunc )
        {
            if (spriteId == 0)
            {
                return;
            }
            LoaderHelper.LoadSpriteAsset(spriteId, loadSpriteFunc);
        }

        #endregion
    }
}