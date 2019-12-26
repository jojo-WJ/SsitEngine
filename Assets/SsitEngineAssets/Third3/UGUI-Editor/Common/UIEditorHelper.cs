using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEngine;

namespace U3DExtends
{
    public static class UIEditorHelper
    {
        private static readonly Vector2 HalfVec = new Vector2(0.5f, 0.5f);

        private static MethodInfo s_GetInstanceIDFromGUID;

        private static Texture2D mBackdropTex;

        public static Texture2D backdropTexture
        {
            get
            {
                if (mBackdropTex == null)
                    mBackdropTex = CreateCheckerTex(
                        new Color(0.1f, 0.1f, 0.1f, 0.5f),
                        new Color(0.2f, 0.2f, 0.2f, 0.5f));
                return mBackdropTex;
            }
        }

        public static void SetImageByPath( string assetPath, Image image, bool isNativeSize = true )
        {
            var newImg = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite));
            Undo.RecordObject(image, "Change Image"); //有了这句才可以用ctrl+z撤消此赋值操作
            image.sprite = newImg as Sprite;
            if (isNativeSize)
                image.SetNativeSize();
            EditorUtility.SetDirty(image);
        }

        [MenuItem("Edit/Copy Names " + Configure.ShortCut.CopyNodesName, false, 2)]
        public static void CopySelectWidgetName()
        {
            var result = "";
            foreach (var item in Selection.gameObjects)
            {
                var item_name = item.name;
                var root_trans = item.transform.parent;
                while (root_trans != null && root_trans.GetComponent<Canvas>() == null)
                {
                    if (root_trans.parent != null && root_trans.parent.GetComponent<Canvas>() == null)
                        item_name = root_trans.name + "/" + item_name;
                    else
                        break;
                    root_trans = root_trans.parent;
                }
                result = result + "\"" + item_name + "\",";
            }

            //复制到系统全局的粘贴板上
            GUIUtility.systemCopyBuffer = result;
            Debug.Log("Copy Nodes Name Succeed！");
        }

        public static Transform GetRootLayout( Transform trans )
        {
            Transform result = null;
            var canvas = trans.GetComponentInParent<Canvas>();
            if (canvas != null)
                foreach (var item in canvas.transform.GetComponentsInChildren<RectTransform>())
                    if (item.GetComponent<Decorate>() == null && canvas.transform != item)
                    {
                        result = item;
                        break;
                    }
            return result;
        }

        public static GameObject GetUITestRootNode()
        {
            var testUI = GameObject.Find(Configure.UITestNodeName);
            if (!testUI)
            {
                testUI = new GameObject(Configure.UITestNodeName, typeof(RectTransform));
                var trans = testUI.GetComponent<RectTransform>();
                trans.position = Configure.UITestNodePos;
                trans.sizeDelta = Configure.UITestNodeSize;
                testUI.AddComponent<ReopenLayoutOnExitGame>();
            }
            return testUI;
        }

        public static Transform GetContainerUnderMouse( Vector3 mouse_abs_pos, GameObject ignore_obj = null )
        {
            var testUI = GetUITestRootNode();
            var list = new List<RectTransform>();
            var containers = Object.FindObjectsOfType<Canvas>();
            var corners = new Vector3[4];
            foreach (var item in containers)
            {
                if (ignore_obj == item.gameObject || item.transform.parent != testUI.transform)
                    continue;
                var trans = item.transform as RectTransform;
                if (trans != null)
                {
                    //获取节点的四个角的世界坐标，分别按顺序为左下左上，右上右下
                    trans.GetWorldCorners(corners);
                    if (mouse_abs_pos.x >= corners[0].x && mouse_abs_pos.y <= corners[1].y &&
                        mouse_abs_pos.x <= corners[2].x && mouse_abs_pos.y >= corners[3].y) list.Add(trans);
                }
            }
            if (list.Count <= 0)
                return null;
            list.Sort(( a, b ) =>
                {
                    return a.GetSiblingIndex() == b.GetSiblingIndex() ? 0 :
                        a.GetSiblingIndex() < b.GetSiblingIndex() ? 1 : -1;
                }
            );
            return GetRootLayout(list[0]);
        }

        public static GameObject CreatNewLayout( bool isNeedLayout = true )
        {
            var testUI = GetUITestRootNode();

            var file_path = Path.Combine(Configure.ResAssetsPath, "Canvas_Editor.prefab");
            file_path = FileUtil.GetProjectRelativePath(file_path);
            var layout_prefab = AssetDatabase.LoadAssetAtPath(file_path, typeof(Object)) as GameObject;
            var layout = Object.Instantiate(layout_prefab);
            layout.transform.SetParent(testUI.transform);
            var last_pos = layout.transform.localPosition;
            layout.transform.localPosition = new Vector3(last_pos.x, last_pos.y, 0);
            if (!isNeedLayout)
            {
                var child = layout.transform.GetChild(0);
                layout.transform.DetachChildren();
                Undo.DestroyObjectImmediate(child.gameObject);
            }
            else
            {
                var layoutCanvas = layout.GetComponent<Canvas>();
                if (layoutCanvas) layoutCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            Selection.activeGameObject = layout;
            var trans = layout.transform as RectTransform;

            SceneView.lastActiveSceneView.MoveToView(trans);

            return layout;
        }

        public static bool SelectPicForDecorate( Decorate decorate )
        {
            if (decorate != null)
            {
                var default_path = PathSaver.GetInstance().GetLastPath(PathType.OpenDecorate);
                var spr_path = EditorUtility.OpenFilePanel("加载外部图片", default_path, "");
                if (spr_path.Length > 0)
                {
                    decorate.SprPath = spr_path;
                    PathSaver.GetInstance().SetLastPath(PathType.OpenDecorate, spr_path);
                    return true;
                }
            }
            return false;
        }

        public static Decorate CreateEmptyDecorate( Transform parent )
        {
            var file_path = Path.Combine(Configure.ResAssetsPath, "Decorate.prefab");
            file_path = FileUtil.GetProjectRelativePath(file_path);
            var decorate_prefab = AssetDatabase.LoadAssetAtPath(file_path, typeof(Object)) as GameObject;
            var decorate = Object.Instantiate(decorate_prefab);
            decorate.transform.SetParent(parent);
            var rectTrans = decorate.transform as RectTransform;
            rectTrans.SetAsFirstSibling();
            rectTrans.localPosition = Vector3.zero;
            rectTrans.localScale = Vector3.one;
            var decor = rectTrans.GetComponent<Decorate>();
            return decor;
        }

        public static void CreateDecorate()
        {
            if (Selection.activeTransform != null)
            {
                var canvas = Selection.activeTransform.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    var decor = CreateEmptyDecorate(canvas.transform);
                    Selection.activeTransform = decor.transform;

                    if (Configure.OpenSelectPicDialogWhenAddDecorate)
                    {
                        var isSucceed = SelectPicForDecorate(decor);
                        if (!isSucceed)
                            Object.DestroyImmediate(decor.gameObject);
                    }
                }
            }
        }

        // [MenuItem("UIEditor/清空界面 " + Configure.ShortCut.ClearAllCanvas)]
        public static void ClearAllCanvas()
        {
            var isDeleteAll = EditorUtility.DisplayDialog("警告", "是否清空掉所有界面？", "干！", "不了");
            if (isDeleteAll)
            {
                var test = GameObject.Find(Configure.UITestNodeName);
                if (test != null)
                {
                    var allLayouts = test.transform.GetComponentsInChildren<LayoutInfo>(true);
                    foreach (var item in allLayouts) Undo.DestroyObjectImmediate(item.gameObject);
                    // GameObject.DestroyImmediate(test);
                }
            }
        }

        public static void LoadLayoutWithFolder()
        {
            var default_path = PathSaver.GetInstance().GetLastPath(PathType.SaveLayout);
            var select_path = EditorUtility.OpenFolderPanel("Open Layout", default_path, "");
            PathSaver.GetInstance().SetLastPath(PathType.SaveLayout, select_path);
            if (select_path.Length > 0)
            {
                var file_paths = Directory.GetFiles(select_path, "*.prefab");
                foreach (var path in file_paths) LoadLayoutByPath(path);
            }
            UILayoutTool.ResortAllLayout();
        }

        private static GameObject GetLoadedLayout( string layoutPath )
        {
            var testUI = GetUITestRootNode();
            if (testUI != null)
            {
                var layoutInfos = testUI.GetComponentsInChildren<LayoutInfo>(true);
                foreach (var item in layoutInfos)
                    if (item.LayoutPath == layoutPath)
                        return item.gameObject;
            }
            return null;
        }

        //从界面的Canvas里取到真实的界面prefab
        public static Transform GetRealLayout( GameObject anyObj )
        {
            var layoutInfo = anyObj.GetComponentInParent<LayoutInfo>();
            Transform real_layout = null;
            if (layoutInfo == null)
                return real_layout;
            if (layoutInfo.LayoutPath != string.Empty)
            {
                var just_name = Path.GetFileNameWithoutExtension(layoutInfo.LayoutPath);
                for (var i = 0; i < layoutInfo.transform.childCount; i++)
                {
                    var child = layoutInfo.transform.GetChild(i);
                    if (child.name.StartsWith(just_name))
                    {
                        real_layout = child;
                        break;
                    }
                }
            }
            else
            {
                //界面是新建的,未保存过的情况下取其子节点
                var layout = anyObj.GetComponentInParent<Canvas>();
                for (var i = 0; i < layout.transform.childCount; i++)
                {
                    var child = layout.transform.GetChild(i);
                    if (child.GetComponent<Decorate>() != null)
                        continue;

                    real_layout = child.transform;
                    break;
                }
            }
            return real_layout;
        }

        public static void DelayReLoadLayout( GameObject o, bool isQuiet )
        {
            Action<PlayModeStateChange> p = null;
            p = c =>
            {
                Debug.Log("reload !");
                ReLoadLayout(o, isQuiet);
                EditorApplication.playModeStateChanged -= p;
            };
            EditorApplication.playModeStateChanged += p;
        }

        public static void ReLoadLayout( GameObject o, bool isQuiet )
        {
            var saveObj = o == null ? Selection.activeGameObject : o;
            if (saveObj == null)
                return;
            var layoutInfo = saveObj.GetComponentInParent<LayoutInfo>();
            if (layoutInfo != null && layoutInfo.LayoutPath != string.Empty)
            {
                var is_reopen = isQuiet || EditorUtility.DisplayDialog("警告", "是否重新加载？", "来吧", "不了");
                if (is_reopen)
                {
                    var just_name = Path.GetFileNameWithoutExtension(layoutInfo.LayoutPath);
                    var real_layout = GetRealLayout(layoutInfo.gameObject);

                    if (real_layout)
                    {
                        var select_path = FileUtil.GetProjectRelativePath(layoutInfo.LayoutPath);
                        var prefab = AssetDatabase.LoadAssetAtPath(select_path, typeof(Object));
                        var new_view = PrefabUtility.InstantiateAttachedAsset(prefab) as GameObject;
                        new_view.transform.SetParent(layoutInfo.transform);

                        if (new_view.transform is RectTransform trans && trans.anchorMax == Vector2.one &&
                            trans.anchorMin == Vector2.zero) trans.sizeDelta = Vector2.zero;

                        new_view.transform.localPosition = real_layout.localPosition;
                        new_view.transform.localScale = Vector3.one;

                        new_view.name = just_name;
                        PrefabUtility.UnpackPrefabInstance(new_view, PrefabUnpackMode.Completely,
                            InteractionMode.AutomatedAction);
                        //PrefabUtility.DisconnectPrefabInstance(new_view);//链接中的话删里面的子节点时会报警告，所以还是一直失联的好，保存时直接覆盖pref
                        Undo.DestroyObjectImmediate(real_layout.gameObject);
                        Debug.Log("Reload Layout Succeed!");
                        layoutInfo.ApplyConfig(select_path);
                    }
                }
            }
            else
            {
                Debug.Log("Try to reload unsaved layout failed");
            }
        }

        public static Transform LoadLayoutByPath( string select_path )
        {
            //Debug.Log("select_path : "+select_path);
            var new_layout = CreatNewLayout(false);
            new_layout.transform.localPosition = new Vector3(new_layout.transform.localPosition.x,
                new_layout.transform.localPosition.y, 0);
            var layoutInfo = new_layout.GetComponent<LayoutInfo>();
            layoutInfo.LayoutPath = select_path;
            if (!File.Exists(select_path))
            {
                Debug.Log("UIEditorHelper:LoadLayoutByPath cannot find layout file:" + select_path);
                return null;
            }
            var asset_relate_path = select_path;
            if (!select_path.StartsWith("Assets/"))
                asset_relate_path = FileUtil.GetProjectRelativePath(select_path);

            var prefab = AssetDatabase.LoadAssetAtPath(asset_relate_path, typeof(Object));
            var new_view = PrefabUtility.InstantiateAttachedAsset(prefab) as GameObject;
            new_view.transform.SetParent(new_layout.transform);

            if (new_view.transform is RectTransform trans && trans.anchorMax == Vector2.one &&
                trans.anchorMin == Vector2.zero) trans.sizeDelta = Vector2.zero;
            new_view.transform.localPosition = Vector3.zero;
            new_view.transform.localScale = Vector3.one;
            var just_name = Path.GetFileNameWithoutExtension(asset_relate_path);
            new_view.name = just_name;
            new_layout.gameObject.name = just_name + "_Canvas";
            try
            {
                PrefabUtility.UnpackPrefabInstance(new_view, PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
                //PrefabUtility.DisconnectPrefabInstance(new_view);//链接中的话删里面的子节点时会报警告，所以还是一直失联的好，保存时直接覆盖prefab就行了 //打开界面时,从项目临时文件夹找到对应界面的参照图配置,然后生成参照图 layoutInfo.ApplyConfig(asset_relate_path);
            }
            catch (Exception e)
            {
            }
            ReopenLayoutOnExitGame.RecordOpenLayout(select_path, new_layout.transform.localPosition);
            return new_layout.transform;
        }

        //[MenuItem("UIEditor/加载界面 " + Configure.ShortCut.LoadUIPrefab, false, 1)]
        public static void LoadLayout()
        {
            var default_path = PathSaver.GetInstance().GetLastPath(PathType.SaveLayout);
            var select_path = EditorUtility.OpenFilePanel("Open Layout", default_path, "prefab");
            PathSaver.GetInstance().SetLastPath(PathType.SaveLayout, select_path);
            if (select_path.Length > 0)
            {
                //检查是否已打开同名界面
                var loaded_layout = GetLoadedLayout(select_path);
                if (loaded_layout != null)
                {
                    var is_reopen = EditorUtility.DisplayDialog("警告", "已打开同名界面,是否重新加载？", "来吧", "不了");
                    if (is_reopen)
                        //Undo.DestroyObjectImmediate(loaded_layout);
                        ReLoadLayout(loaded_layout, true);
                    return;
                }
                LoadLayoutByPath(select_path);
            }
        }

        //[MenuItem("UIEditor/Operate/锁定")]
        public static void LockWidget()
        {
            if (Selection.gameObjects.Length > 0) Selection.gameObjects[0].hideFlags = HideFlags.NotEditable;
        }

        //[MenuItem("UIEditor/Operate/解锁")]
        public static void UnLockWidget()
        {
            if (Selection.gameObjects.Length > 0) Selection.gameObjects[0].hideFlags = HideFlags.None;
        }

        //是否支持解体
        public static bool IsNodeCanDivide( GameObject obj )
        {
            if (obj == null)
                return false;
            return obj.transform != null && obj.transform.childCount > 0 && obj.GetComponent<Canvas>() == null &&
                   obj.transform.parent != null && obj.transform.parent.GetComponent<Canvas>() == null;
        }

        public static bool SaveTextureToPNG( Texture inputTex, string save_file_name )
        {
            var temp = RenderTexture.GetTemporary(inputTex.width, inputTex.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(inputTex, temp);
            var ret = SaveRenderTextureToPNG(temp, save_file_name);
            RenderTexture.ReleaseTemporary(temp);
            return ret;
        }

        //将RenderTexture保存成一张png图片  
        public static bool SaveRenderTextureToPNG( RenderTexture rt, string save_file_name )
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            var bytes = png.EncodeToPNG();
            var directory = Path.GetDirectoryName(save_file_name);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var file = File.Open(save_file_name, FileMode.Create);
            var writer = new BinaryWriter(file);
            writer.Write(bytes);
            file.Close();
            Object.DestroyImmediate(png);
            png = null;
            RenderTexture.active = prev;
            return true;
        }

        public static Texture2D LoadTextureInLocal( string file_path )
        {
            //创建文件读取流
            var fileStream = new FileStream(file_path, FileMode.Open, FileAccess.Read);
            fileStream.Seek(0, SeekOrigin.Begin);
            //创建文件长度缓冲区
            var bytes = new byte[fileStream.Length];
            //读取文件
            fileStream.Read(bytes, 0, (int) fileStream.Length);
            //释放文件读取流
            fileStream.Close();
            fileStream.Dispose();
            fileStream = null;

            //创建Texture
            var width = 300;
            var height = 372;
            var texture = new Texture2D(width, height);
            texture.LoadImage(bytes);
            return texture;
        }

        //加载外部资源为Sprite
        public static Sprite LoadSpriteInLocal( string file_path )
        {
            if (!File.Exists(file_path))
            {
                Debug.Log("LoadSpriteInLocal() cannot find sprite file : " + file_path);
                return null;
            }
            var texture = LoadTextureInLocal(file_path);
            //创建Sprite
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), HalfVec);
            return sprite;
        }

        public static Texture GetAssetPreview( GameObject obj )
        {
            GameObject canvas_obj = null;
            var clone = Object.Instantiate(obj);
            var cloneTransform = clone.transform;
            var isUINode = false;
            if (cloneTransform is RectTransform)
            {
                //如果是UGUI节点的话就要把它们放在Canvas下了
                canvas_obj = new GameObject("render canvas", typeof(Canvas));
                var canvas = canvas_obj.GetComponent<Canvas>();
                cloneTransform.SetParent(canvas_obj.transform);
                cloneTransform.localPosition = Vector3.zero;

                canvas_obj.transform.position = new Vector3(-1000, -1000, -1000);
                canvas_obj.layer = 21; //放在21层，摄像机也只渲染此层的，避免混入了奇怪的东西
                isUINode = true;
            }
            else
            {
                cloneTransform.position = new Vector3(-1000, -1000, -1000);
            }

            var all = clone.GetComponentsInChildren<Transform>();
            foreach (var trans in all) trans.gameObject.layer = 21;

            var bounds = GetBounds(clone);
            var Min = bounds.min;
            var Max = bounds.max;
            var cameraObj = new GameObject("render camera");

            var renderCamera = cameraObj.AddComponent<Camera>();
            renderCamera.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            renderCamera.clearFlags = CameraClearFlags.Color;
            renderCamera.cameraType = CameraType.Preview;
            renderCamera.cullingMask = 1 << 21;
            if (isUINode)
            {
                cameraObj.transform.position = new Vector3((Max.x + Min.x) / 2f, (Max.y + Min.y) / 2f,
                    cloneTransform.position.z - 100);
                var center = new Vector3(cloneTransform.position.x + 0.01f, (Max.y + Min.y) / 2f,
                    cloneTransform.position.z); //+0.01f是为了去掉Unity自带的摄像机旋转角度为0的打印，太烦人了
                cameraObj.transform.LookAt(center);

                renderCamera.orthographic = true;
                var width = Max.x - Min.x;
                var height = Max.y - Min.y;
                var max_camera_size = width > height ? width : height;
                renderCamera.orthographicSize = max_camera_size / 2; //预览图要尽量少点空白
            }
            else
            {
                cameraObj.transform.position =
                    new Vector3((Max.x + Min.x) / 2f, (Max.y + Min.y) / 2f, Max.z + (Max.z - Min.z));
                var center = new Vector3(cloneTransform.position.x + 0.01f, (Max.y + Min.y) / 2f,
                    cloneTransform.position.z);
                cameraObj.transform.LookAt(center);

                var angle = (int) (Mathf.Atan2((Max.y - Min.y) / 2, Max.z - Min.z) * 180 / 3.1415f * 2);
                renderCamera.fieldOfView = angle;
            }
            var texture = new RenderTexture(128, 128, 0, RenderTextureFormat.Default);
            renderCamera.targetTexture = texture;

            Undo.DestroyObjectImmediate(cameraObj);
            Undo.PerformUndo(); //不知道为什么要删掉再Undo回来后才Render得出来UI的节点，3D节点是没这个问题的，估计是Canvas创建后没那么快有效？
            renderCamera.RenderDontRestore();
            var tex = new RenderTexture(128, 128, 0, RenderTextureFormat.Default);
            Graphics.Blit(texture, tex);

            Object.DestroyImmediate(canvas_obj);
            Object.DestroyImmediate(cameraObj);
            return tex;
        }

        public static Bounds GetBounds( GameObject obj )
        {
            var Min = new Vector3(99999, 99999, 99999);
            var Max = new Vector3(-99999, -99999, -99999);
            var renders = obj.GetComponentsInChildren<MeshRenderer>();
            if (renders.Length > 0)
            {
                for (var i = 0; i < renders.Length; i++)
                {
                    if (renders[i].bounds.min.x < Min.x)
                        Min.x = renders[i].bounds.min.x;
                    if (renders[i].bounds.min.y < Min.y)
                        Min.y = renders[i].bounds.min.y;
                    if (renders[i].bounds.min.z < Min.z)
                        Min.z = renders[i].bounds.min.z;

                    if (renders[i].bounds.max.x > Max.x)
                        Max.x = renders[i].bounds.max.x;
                    if (renders[i].bounds.max.y > Max.y)
                        Max.y = renders[i].bounds.max.y;
                    if (renders[i].bounds.max.z > Max.z)
                        Max.z = renders[i].bounds.max.z;
                }
            }
            else
            {
                var rectTrans = obj.GetComponentsInChildren<RectTransform>();
                var corner = new Vector3[4];
                for (var i = 0; i < rectTrans.Length; i++)
                {
                    //获取节点的四个角的世界坐标，分别按顺序为左下左上，右上右下
                    rectTrans[i].GetWorldCorners(corner);
                    if (corner[0].x < Min.x)
                        Min.x = corner[0].x;
                    if (corner[0].y < Min.y)
                        Min.y = corner[0].y;
                    if (corner[0].z < Min.z)
                        Min.z = corner[0].z;

                    if (corner[2].x > Max.x)
                        Max.x = corner[2].x;
                    if (corner[2].y > Max.y)
                        Max.y = corner[2].y;
                    if (corner[2].z > Max.z)
                        Max.z = corner[2].z;
                }
            }

            var center = (Min + Max) / 2;
            var size = new Vector3(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);
            return new Bounds(center, size);
        }

        //[MenuItem("UIEditor/另存为 ")]
        public static void SaveAnotherLayoutMenu()
        {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("Warning", "I don't know which prefab you want to save", "Ok");
                return;
            }
            var layout = Selection.activeGameObject.GetComponentInParent<Canvas>();
            for (var i = 0; i < layout.transform.childCount; i++)
            {
                var child = layout.transform.GetChild(i);
                if (child.GetComponent<Decorate>() != null)
                    continue;
                var child_obj = child.gameObject;
                //Debug.Log("child type :" + PrefabUtility.GetPrefabType(child_obj));

                //判断选择的物体，是否为预设  
                var cur_prefab_type = PrefabUtility.GetPrefabType(child_obj);
                SaveAnotherLayout(layout, child);
            }
        }

        public static void SaveAnotherLayout( Canvas layout, Transform child )
        {
            if (child.GetComponent<Decorate>() != null)
                return;
            var child_obj = child.gameObject;
            //Debug.Log("child type :" + PrefabUtility.GetPrefabType(child_obj));

            //判断选择的物体，是否为预设  
            var cur_prefab_type = PrefabUtility.GetPrefabType(child_obj);
            //不是预设的话说明还没保存过的，弹出保存框
            var default_path = PathSaver.GetInstance().GetLastPath(PathType.SaveLayout);
            var save_path = EditorUtility.SaveFilePanel("Save Layout", default_path, "prefab_name", "prefab");
            if (save_path == "")
                return;
            var full_path = save_path;
            PathSaver.GetInstance().SetLastPath(PathType.SaveLayout, save_path);
            save_path = FileUtil.GetProjectRelativePath(save_path);
            if (save_path == "")
            {
                Debug.Log("wrong path to save layout, is this project path? : " + full_path);
                EditorUtility.DisplayDialog("error", "wrong path to save layout, is this project path? : " + full_path,
                    "ok");
                return;
            }

            var new_prefab = PrefabUtility.CreateEmptyPrefab(save_path);
            PrefabUtility.ReplacePrefab(child_obj, new_prefab, ReplacePrefabOptions.ConnectToPrefab);
            var layoutInfo = layout.GetComponent<LayoutInfo>();
            if (layoutInfo != null)
                layoutInfo.LayoutPath = full_path;
            var just_name = Path.GetFileNameWithoutExtension(save_path);
            child_obj.name = just_name;
            layout.gameObject.name = just_name + "_Canvas";
            //刷新  
            AssetDatabase.Refresh();
            if (Configure.IsShowDialogWhenSaveLayout)
                EditorUtility.DisplayDialog("Tip", "Save Succeed!", "Ok");

            //保存时先记录一下,如果是运行游戏时保存了,结束游戏时就要重新加载界面了,不然会重置回运行游戏前的
            var reloadCom = layout.GetComponent<ReloadLayoutOnExitGame>();
            if (reloadCom)
                reloadCom.SetHadSaveOnRunTime(true);
            Debug.Log("Save Succeed!");
            layoutInfo.SaveToConfigFile();
        }

        //[MenuItem("UIEditor/保存 " + Configure.ShortCut.SaveUIPrefab, false, 2)]
        public static void SaveLayout( GameObject o, bool isQuiet )
        {
            var saveObj = o == null ? Selection.activeGameObject : o;
            if (saveObj == null)
            {
                EditorUtility.DisplayDialog("Warning", "I don't know which prefab you want to save", "Ok");
                return;
            }
            var layout = saveObj.GetComponentInParent<Canvas>();
            if (layout == null)
            {
                EditorUtility.DisplayDialog("Warning", "select any layout below UITestNode/canvas to save", "Ok");
                return;
            }
            var real_layout = GetRealLayout(saveObj);
            if (real_layout != null)
            {
                var child_obj = real_layout.gameObject;
                //判断选择的物体，是否为预设  
                var cur_prefab_type = PrefabUtility.GetPrefabType(child_obj);
                if (PrefabUtility.GetPrefabType(child_obj) == PrefabType.PrefabInstance ||
                    cur_prefab_type == PrefabType.DisconnectedPrefabInstance)
                {
                    var parentObject = PrefabUtility.GetPrefabParent(child_obj);
                    //替换预设,Note:只能用ConnectToPrefab,不然会重复加多几个同名控件的
                    PrefabUtility.ReplacePrefab(child_obj, parentObject, ReplacePrefabOptions.ConnectToPrefab);
                    //刷新  
                    AssetDatabase.Refresh();
                    if (Configure.IsShowDialogWhenSaveLayout && !isQuiet)
                        EditorUtility.DisplayDialog("Tip", "Save Succeed!", "Ok");

                    //保存时先记录一下,如果是运行游戏时保存了,结束游戏时就要重新加载界面了,不然会重置回运行游戏前的
                    var reloadCom = layout.GetComponent<ReloadLayoutOnExitGame>();
                    if (reloadCom)
                        reloadCom.SetHadSaveOnRunTime(true);
                    Debug.Log("Save Succeed!");
                    var layoutInfo = layout.GetComponent<LayoutInfo>();
                    if (layoutInfo != null)
                        layoutInfo.SaveToConfigFile();
                }
                else
                {
                    SaveAnotherLayout(layout, real_layout);
                }
            }
            else
            {
                Debug.Log("save failed!are you select any widget below canvas?");
            }
        }

        public static string ObjectToGUID( Object obj )
        {
            var path = AssetDatabase.GetAssetPath(obj);
            return !string.IsNullOrEmpty(path) ? AssetDatabase.AssetPathToGUID(path) : null;
        }

        public static Object GUIDToObject( string guid )
        {
            if (string.IsNullOrEmpty(guid)) return null;

            /*if (s_GetInstanceIDFromGUID == null)
                s_GetInstanceIDFromGUID = typeof(AssetDatabase).GetMethod("GetInstanceIDFromGUID", BindingFlags.Static | BindingFlags.NonPublic);
            int id = (int)s_GetInstanceIDFromGUID.Invoke(null, new object[] { guid });
            if (id != 0) return EditorUtility.InstanceIDToObject(id);*/
            //AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid)).GetInstanceID()
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath(path, typeof(Object));
        }

        public static T GUIDToObject<T>( string guid ) where T : Object
        {
            var obj = GUIDToObject(guid);
            if (obj == null) return null;

            var objType = obj.GetType();
            if (objType == typeof(T) || objType.IsSubclassOf(typeof(T))) return obj as T;

            if (objType == typeof(GameObject) && typeof(T).IsSubclassOf(typeof(Component)))
            {
                var go = obj as GameObject;
                return go.GetComponent(typeof(T)) as T;
            }
            return null;
        }

        public static void SetEnum( string name, Enum val )
        {
            EditorPrefs.SetString(name, val.ToString());
        }

        public static T GetEnum<T>( string name, T defaultValue )
        {
            var val = EditorPrefs.GetString(name, defaultValue.ToString());
            var names = Enum.GetNames(typeof(T));
            var values = Enum.GetValues(typeof(T));

            for (var i = 0; i < names.Length; ++i)
                if (names[i] == val)
                    return (T) values.GetValue(i);
            return defaultValue;
        }

        public static void DrawTiledTexture( Rect rect, Texture tex )
        {
            GUI.BeginGroup(rect);
            {
                var width = Mathf.RoundToInt(rect.width);
                var height = Mathf.RoundToInt(rect.height);

                for (var y = 0; y < height; y += tex.height)
                for (var x = 0; x < width; x += tex.width)
                    GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
            }
            GUI.EndGroup();
        }

        private static Texture2D CreateCheckerTex( Color c0, Color c1 )
        {
            var tex = new Texture2D(16, 16);
            tex.name = "[Generated] Checker Texture";
            tex.hideFlags = HideFlags.DontSave;

            for (var y = 0; y < 8; ++y)
            for (var x = 0; x < 8; ++x)
                tex.SetPixel(x, y, c1);
            for (var y = 8; y < 16; ++y)
            for (var x = 0; x < 8; ++x)
                tex.SetPixel(x, y, c0);
            for (var y = 0; y < 8; ++y)
            for (var x = 8; x < 16; ++x)
                tex.SetPixel(x, y, c0);
            for (var y = 8; y < 16; ++y)
            for (var x = 8; x < 16; ++x)
                tex.SetPixel(x, y, c1);

            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return tex;
        }

        private static Transform GetGoodContainer( Transform trans )
        {
            if (trans == null)
                return null;
            if (trans.GetComponent<Canvas>() != null || trans.GetComponent<Decorate>() != null)
                return GetRealLayout(trans.gameObject);
            return trans;
        }

        public static void AddImageComponent()
        {
            if (Selection.activeGameObject == null)
                return;
            var old_img = Selection.activeGameObject.GetComponent<Image>();
            if (old_img != null)
            {
                var isOk = EditorUtility.DisplayDialog("警告", "该GameObject已经有Image组件了,你想替换吗?", "来吧", "算了");
                if (isOk)
                {
                    //Selection.activeGameObject.
                }
            }
            var img = Selection.activeGameObject.AddComponent<Image>();
            img.raycastTarget = false;
        }

        public static void AddHorizontalLayoutComponent()
        {
            if (Selection.activeGameObject == null)
                return;
            var layout = Selection.activeGameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
        }

        public static void AddVerticalLayoutComponent()
        {
            if (Selection.activeGameObject == null)
                return;
            var layout = Selection.activeGameObject.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
        }

        public static void AddGridLayoutGroupComponent()
        {
            if (Selection.activeGameObject == null)
                return;
            var layout = Selection.activeGameObject.AddComponent<GridLayoutGroup>();
        }

        public static void CreateEmptyObj()
        {
            if (Selection.activeGameObject == null)
                return;
            var go = new GameObject(CommonHelper.GenerateUniqueName(Selection.activeGameObject, "GameObject"),
                typeof(RectTransform));
            go.transform.SetParent(GetGoodContainer(Selection.activeTransform), false);
            Selection.activeGameObject = go;
        }

        public static void CreateImageObj()
        {
            if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                var go = new GameObject(CommonHelper.GenerateUniqueName(Selection.activeGameObject, "Image"),
                    typeof(Image));
                go.GetComponent<Image>().raycastTarget = false;
                go.transform.SetParent(GetGoodContainer(Selection.activeTransform), false);
                Selection.activeGameObject = go;
            }
        }

        public static void CreateRawImageObj()
        {
            if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                var go = new GameObject(CommonHelper.GenerateUniqueName(Selection.activeGameObject, "RawImage"),
                    typeof(RawImage));
                go.GetComponent<RawImage>().raycastTarget = false;
                go.transform.SetParent(GetGoodContainer(Selection.activeTransform), false);
                Selection.activeGameObject = go;
            }
        }

        public static void CreateButtonObj()
        {
            if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                var last_trans = Selection.activeTransform;
                var isOk = EditorApplication.ExecuteMenuItem("GameObject/UI/Button");
                if (isOk)
                {
                    Selection.activeGameObject.name =
                        CommonHelper.GenerateUniqueName(Selection.activeGameObject, "Button");
                    Selection.activeTransform.SetParent(GetGoodContainer(last_trans), false);
                }
            }
        }

        public static void CreateTextObj()
        {
            if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                var go = new GameObject(CommonHelper.GenerateUniqueName(Selection.activeGameObject, "Text"),
                    typeof(Text));
                var txt = go.GetComponent<Text>();
                txt.raycastTarget = false;
                txt.text = "I am a Text";
                go.transform.SetParent(GetGoodContainer(Selection.activeTransform), false);
                go.transform.localPosition = Vector3.zero;
                Selection.activeGameObject = go;
            }
        }

        private static void InitScrollView( bool isHorizontal )
        {
            var scroll = Selection.activeTransform.GetComponent<ScrollRect>();
            if (scroll == null)
                return;
            var img = Selection.activeTransform.GetComponent<Image>();
            if (img != null)
                Object.DestroyImmediate(img);
            scroll.horizontal = isHorizontal;
            scroll.vertical = !isHorizontal;
            scroll.horizontalScrollbar = null;
            scroll.verticalScrollbar = null;
            var horizontalObj = Selection.activeTransform.Find("Scrollbar Horizontal");
            if (horizontalObj != null)
                Object.DestroyImmediate(horizontalObj.gameObject);
            var verticalObj = Selection.activeTransform.Find("Scrollbar Vertical");
            if (verticalObj != null)
                Object.DestroyImmediate(verticalObj.gameObject);
            var viewPort = Selection.activeTransform.Find("Viewport") as RectTransform;
            if (viewPort != null)
            {
                viewPort.offsetMin = new Vector2(0, 0);
                viewPort.offsetMax = new Vector2(0, 0);
            }
        }

        public static void CreateHScrollViewObj()
        {
            if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                var last_trans = Selection.activeTransform;
                var isOk = EditorApplication.ExecuteMenuItem("GameObject/UI/Scroll View");
                if (isOk)
                {
                    Selection.activeGameObject.name =
                        CommonHelper.GenerateUniqueName(Selection.activeGameObject, "ScrollView");
                    Selection.activeTransform.SetParent(GetGoodContainer(last_trans), false);
                    InitScrollView(true);
                }
            }
        }

        public static void CreateVScrollViewObj()
        {
            if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                var last_trans = Selection.activeTransform;
                var isOk = EditorApplication.ExecuteMenuItem("GameObject/UI/Scroll View");
                if (isOk)
                {
                    Selection.activeGameObject.name =
                        CommonHelper.GenerateUniqueName(Selection.activeGameObject, "ScrollView");
                    Selection.activeTransform.SetParent(GetGoodContainer(last_trans), false);
                    InitScrollView(false);
                }
            }
        }

        public static string GenMD5String( string str )
        {
            var md5 = new MD5CryptoServiceProvider();
            str = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(str)), 4, 8);
            return str.Replace("-", "");
        }

        public static void SaveAnotherLayoutContextMenu()
        {
            SaveAnotherLayoutMenu();
        }

        public static void SaveLayoutForMenu()
        {
            SaveLayout(null, false);
        }

        public static void CreatNewLayoutForMenu()
        {
            CreatNewLayout();
        }

        public static void ReLoadLayoutForMenu()
        {
            ReLoadLayout(null, false);
        }
    }
}
#endif