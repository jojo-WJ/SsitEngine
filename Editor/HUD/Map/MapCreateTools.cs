/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/6 15:39:53                     
*└──────────────────────────────────────────────────────────────┘
*/
using System.Collections;
using System.IO;
using SsitEngine.Unity.HUD;
using UnityEditor;
using UnityEngine;

namespace SsitEngine.Editor.HUD.Map
{
    [ExecuteInEditMode]
    public class MapTextureCreator : MonoBehaviour
    {
        #region Variables
        public string MapName = "New_Map";

        public _TextureSize TextureSize = _TextureSize._2048;
        public Color Background = Color.black;

        public GameObject FitToObject;
        public float ObjectBoundsMultiplier = 1f;

        public bool PreviewShowBounds = true;
        public Color PreviewBoundsColor = new Color(0f, 0f, 1f, .3f);


        [HideInInspector]
        public Bounds MapBounds = new Bounds(Vector3.zero, new Vector3(500f, 100f, 500f));

        [HideInInspector]
        public Camera RenderCamera;

        [HideInInspector]
        public RenderTexture CameraRenderTexture;

        public bool _isBusy = false;
        #endregion


        #region Main Methods
        [MenuItem("Tools/Map" + "/Test/Minimap Texture Creator", false, 11)]
        public static void InitTextureCreator()
        {
            // find / instantiate TextureCreator
            MapTextureCreator textureCreator = FindObjectOfType<MapTextureCreator>();
            if (textureCreator == null)
                textureCreator = new GameObject("HNS TextureCreator").AddComponent<MapTextureCreator>();

            // set TextureCreator as active gameobject
            Selection.activeGameObject = textureCreator.gameObject;
        }


        protected virtual void OnDisable()
        {
            // destroy preview camera
            if (RenderCamera != null)
                DestroyImmediate(RenderCamera.gameObject);

            // destroy render texture
            if (CameraRenderTexture != null)
                DestroyImmediate(CameraRenderTexture);
        }


        public virtual void FitToBounds(GameObject boundsObj)
        {
            if (boundsObj == null)
                return;

            // store object's position / scale
            Vector3 objPosition = boundsObj.transform.position;
            Vector3 objScale = boundsObj.transform.localScale;

            // get bounds from object
            Terrain terrain = boundsObj.GetComponent<Terrain>();
            if (terrain != null)
            {
                // use terrain size
                objPosition += terrain.terrainData.size / 2f;
                objScale = terrain.terrainData.size;
            }
            else
            {
                // get mesh renderer
                MeshRenderer meshRenderer = boundsObj.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    // use mesh renderer bounds
                    objPosition = meshRenderer.bounds.center;
                    objScale = meshRenderer.bounds.size;
                }
                else
                {
                    // throw error if object has no mesh renderer
                    Debug.LogErrorFormat("No MeshRenderer found on object '{0}'", boundsObj.name);
                    return;
                }
            }

            // multiply bounds scale
            if (ObjectBoundsMultiplier != 0f)
                objScale = new Vector3(objScale.x * ObjectBoundsMultiplier, objScale.y, objScale.z * ObjectBoundsMultiplier);

            // update bounds center/size
            MapBounds.center = objPosition;
            MapBounds.size = objScale;
        }


        public virtual void CreateRenderCamera()
        {
            // create render camera
            if (RenderCamera == null)
            {
                // create camera gameobject
                GameObject cameraGO = new GameObject("HNS TextureCreator Camera", typeof(Camera));
                cameraGO.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                cameraGO.transform.SetParent(this.transform);
                RenderCamera = cameraGO.GetComponent<Camera>();
            }

            // setup render camera
            RenderCamera.clearFlags = CameraClearFlags.SolidColor;
            RenderCamera.backgroundColor = Background;
            RenderCamera.orthographic = true;
            RenderCamera.depth = -100f;

            // update orthographic size
            RenderCamera.orthographicSize = Mathf.Min(MapBounds.size.x, MapBounds.size.z) / 2f;
            if (MapBounds.size.x < MapBounds.size.z)
                RenderCamera.orthographicSize = Mathf.Max(MapBounds.size.x, MapBounds.size.z) / 2f;

            // update camera position
            RenderCamera.transform.position = new Vector3(MapBounds.center.x, MapBounds.max.y + .1f, MapBounds.center.z);

            // update clipping planes
            RenderCamera.nearClipPlane = .1f;
            RenderCamera.farClipPlane = RenderCamera.transform.position.y - MapBounds.min.y + .1f;

            // update camera background
            RenderCamera.backgroundColor = Background;

            // make sure it always has a target texture
            if (RenderCamera.targetTexture == null && CameraRenderTexture != null)
                RenderCamera.targetTexture = CameraRenderTexture;
        }


        public virtual void DestroyRenderCamera()
        {
            if (RenderCamera == null)
                return;

            DestroyImmediate(RenderCamera.gameObject);
        }


        public virtual void UpdateRenderTexture()
        {
            // check if we need to update the render texture
            Vector2Int renderSize = GetTotalTextureSize();
            if (CameraRenderTexture == null || CameraRenderTexture.width != renderSize.x || CameraRenderTexture.height != renderSize.y)
            {
                // release render texture from camera
                if (RenderCamera != null && RenderCamera.targetTexture != null)
                    RenderCamera.targetTexture.Release();

                // create render texture with new dimensions
                CameraRenderTexture = new RenderTexture(renderSize.x, renderSize.y, 24, RenderTextureFormat.ARGB32);

                // assign new render texture to camera
                if (RenderCamera != null)
                    RenderCamera.targetTexture = CameraRenderTexture;
            }
        }


        public void CreateMapTexture()
        {
            _isBusy = true;
            StartCoroutine(CreateMapTextureRoutine());
        }


        public Vector2Int GetTotalTextureSize()
        {
            Vector3Int intMapBounds = Vector3Int.RoundToInt(MapBounds.size);
            int min = Mathf.Min(intMapBounds.x, intMapBounds.z);
            int max = Mathf.Max(intMapBounds.x, intMapBounds.z);
            int textureSize = (int)TextureSize;
            int smallSide = Mathf.RoundToInt(textureSize / (max / (float)min));

            Vector2Int finalSize = new Vector2Int(textureSize, smallSide);
            if (intMapBounds.x < intMapBounds.z)
                finalSize = new Vector2Int(smallSide, textureSize);

            return finalSize;
        }
        #endregion


        #region Utility Methods
        IEnumerator CreateMapTextureRoutine()
        {
            // create render camera
            CreateRenderCamera();

            // update render texture
            UpdateRenderTexture();

            // create map texture
            string mapTexturePath = GetTexturePath();
            Vector2Int mapTextureSize = GetTotalTextureSize();
            Texture2D mapTexture = new Texture2D(mapTextureSize.x, mapTextureSize.y, TextureFormat.RGB24, false);

            // TextureCreator
            try
            {
                while (_isBusy)
                {
                    // manually render camera view
                    RenderCamera.Render();

                    // set active render texture
                    RenderTexture.active = CameraRenderTexture;

                    // read pixels from current tile
                    mapTexture.ReadPixels(new Rect(Vector2.zero, mapTextureSize), 0, 0);

                    // apply changes to final texture
                    mapTexture.Apply();

                    // unset active render texture
                    RenderTexture.active = null;

                    // write final map texture
                    byte[] bytes = mapTexture.EncodeToPNG();
                    using (Stream stream = File.Create(mapTexturePath))
                        stream.Write(bytes, 0, bytes.Length);

                    _isBusy = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                yield break;
            }
            finally
            {
                RenderTexture.active = null;
                DestroyRenderCamera();
                _isBusy = false;
            }

            // refresh asset database
            AssetDatabase.Refresh();

            string relativeTexturePath = mapTexturePath.Substring(mapTexturePath.IndexOf("Assets"));
            TextureImporter textureImporter = AssetImporter.GetAtPath(relativeTexturePath) as TextureImporter;
            if (textureImporter != null)
            {
                // set texture as UI sprite
                if (textureImporter.textureType != TextureImporterType.Sprite)
                    textureImporter.textureType = TextureImporterType.Sprite;

                // update texture size
                if (textureImporter.maxTextureSize != (int)TextureSize)
                    textureImporter.maxTextureSize = (int)TextureSize;

                // clamp texture
                if (textureImporter.wrapMode != TextureWrapMode.Clamp)
                    textureImporter.wrapMode = TextureWrapMode.Clamp;

                // disable POT scaling
                if (mapTextureSize.x != mapTextureSize.y && textureImporter.npotScale != TextureImporterNPOTScale.None)
                    textureImporter.npotScale = TextureImporterNPOTScale.None;

                // save changes
                textureImporter.SaveAndReimport();

                // create map profile
                CreateProfile(relativeTexturePath, mapTextureSize, MapBounds);

                // refresh asset database
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("TextureCreator couldn't update the texture import settings. Please adjust the texture size manually within the texture import settings.");
            }

            // finish operation
            EditorUtility.DisplayDialog("HNS TextureCreator", string.Format("Successfully created profile '{0}':\n\n{1}", MapName, mapTexturePath), "OK");

            // destroy render camera
            DestroyRenderCamera();

            yield return null;
        }


        string GetTexturePath()
        {
            // check for empty filename
            if (MapName.Length <= 0)
                MapName = "Unnamed";

            // check path and create directory
            string path = Path.Combine(Application.dataPath, "HNS TextureCreator/" + MapName).Replace('\\', '/');
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }

            // return final path and filename
            return string.Format("{0}/{1}_{2}_Map.png", path, MapName, System.DateTime.Now.ToString("yyyyMMddHHmmss"));
        }


        void OnDrawGizmos()
        {
            if (Selection.activeGameObject != this.gameObject)
                return;

            // draw map bounds
            if (PreviewShowBounds)
            {
                Gizmos.color = PreviewBoundsColor;
                Gizmos.DrawCube(MapBounds.center, MapBounds.size);
            }
        }

        public static void CreateProfile(string mapTexturePath, Vector2Int mapTextureSize, Bounds mapBounds)
        {
            // check given path
            if (mapTexturePath.Length <= 0)
                return;

            // get map texture
            Sprite mapTexture = AssetDatabase.LoadAssetAtPath<Sprite>(mapTexturePath);

            // create map profile
            MapAsset profile = ScriptableObject.CreateInstance<MapAsset>();
            profile.Init(mapTexture, mapTextureSize, mapBounds);

            // create asset & save changes
            string path = Path.Combine(Path.GetDirectoryName(mapTexturePath), Path.GetFileNameWithoutExtension(mapTexturePath).Replace("_Map", "")).Replace('\\', '/');
            string profilePath = AssetDatabase.GenerateUniqueAssetPath(path + "_Profile.asset");
            AssetDatabase.CreateAsset(profile, profilePath);
            AssetDatabase.SaveAssets();

            // highlight profile in project window
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = profile;
        }
        #endregion


        #region Subclasses
        [System.Serializable]
        public enum _TextureSize { _512 = 512, _1024 = 1024, _2048 = 2048, _4096 = 4096, _8192 = 8192 };
        #endregion

    }
}
