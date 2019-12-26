/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：平台主程序入口                                                    
*│　作   者：xuxin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/06 11:04:21                             
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity;

namespace Framework
{
    public class Main : SsitMain
    {
        // Start is called before the first frame update
        protected override void OnStart()
        {
            Facade.GetInstance(() => new AppFacade()).SendNotification((ushort) EnEngineEvent.OnApplicationStart, gameObject);
        }

        /*public async void AsyncStart()
        {
            AsyncOperationHandle<Texture2D> handle = Addressables.LoadAssetAsync<Texture2D>("mytexture");
            await handle.Task;

            // todo：Task has completed. Be sure to check the Status has succeeded before getting the Result

        }*/
    }
}