/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：                                                    
*│　作    者：Xuxin                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/12/26 17:29:29                             
*└──────────────────────────────────────────────────────────────┘
*/

namespace UnityEditor
{
    public class LevelAtrribute : SavedBase
    {
        public int SceneId { get; set; }

        public string SceneName { get; set; }

        public string SceneDisplayName { get; set; }

        public string ResourceName { get; set; }
        
        public LevelAtrribute()
        {
            
        }

        public override void Shutdown()
        {
            base.Shutdown();

            SceneName = null;
            SceneDisplayName = null;
            ResourceName = null;
        }
    }
}