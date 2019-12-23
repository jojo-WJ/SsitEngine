public class SoundTools
{
    //[MenuItem("Assets/Create/SoundConection")]

    public static void Execute()
    {
        //实例化SysData               
/*

        SoundConnection sd = ScriptableObject.CreateInstance<SoundConnection>();

        //随便设置一些数据给content                 


        // SysData将创建为一个对象，这时在project面板上会看到这个对象。                 

        UnityEngine.Object[] curFolder = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
        string sceneName = EditorSceneManager.GetActiveScene().name;
        var path = "Assets/SoundConection.asset";
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (curFolder.Length > 0)
            {
                path = AssetDatabase.GetAssetPath(curFolder[0]);
            }
            var fileName = sceneName + "Sound.asset";
            path = Path.Combine(path,fileName);
        }
       
        AssetDatabase.CreateAsset(sd, path);*/

        //        Object o = AssetDatabase.LoadAssetAtPath(p, typeof(SysData));

        //打包为SysData.assetbundle文件。                 

        //        BuildPipeline.BuildAssetBundle(o, null, "SysData.assetbundle");

        //删除面板上的那个临时对象               

        //AssetDatabase.DeleteAsset(p);
    }
}