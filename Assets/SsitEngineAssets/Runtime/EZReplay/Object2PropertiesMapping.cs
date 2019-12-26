//#define CacheEzReplay

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Framework.Data;
using Framework.EZReplay;
using Framework.Helper;
using Framework.SceneObject;
using SsitEngine.DebugLog;
using SsitEngine.Unity.SceneObject;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SsitEngine.EzReplay
{
    /// <summary>
    /// 属性映射对象
    /// </summary>
    [Serializable]
    public class Object2PropertiesMapping : ISerializable
    {
        #region Serialized Fields

        //saved states belonging to one game object
        public Dictionary<int, SavedBase> savedStates = new Dictionary<int, SavedBase>();

        //is it a parent game object?
        public bool isParentObj;

        //mapping of parent game object
        public Object2PropertiesMapping parentMapping;

        //Hack:already generator guid for scene BaseSceneObjects by xuXin  
        //load by scene for not prefab
        //public bool isLoadByScene;

        //数据id
        public int itemId;

        //prefab load path (for saving recordings)
        protected string prefabPath;

        //last frame where changes where recognized
        private int lastChangedFrame;

        //first frame where changes where recognized
        public int firstChangedFrame;

        //InstanceID of original gameObject
        protected int gameObjectInstanceID = -1;

        #endregion

        #region Not Seriablized Field

        //the game object this mapping class object is being created for
        protected GameObject gameObject;

        //the clone belonging to the gameObject
        protected GameObject gameObjectClone;
        public ISave sceneInstance;

        //way of identifying children of gameobjects in game scene hierarchy
        protected ChildIdentificationMode childIdentificationMode = ChildIdentificationMode.IDENTIFY_BY_COM;

        //name of original gameObject define child name to select
        protected string gameObjectName = "name_untraceable";

        //protected SavedState cacheSavedState;
        protected int lastRecordPositon;

        #endregion

        #region Property Accessors

        public bool IsParent => isParentObj;

        public GameObject EntityObj => gameObject;

        public GameObject CloneEntityObj => gameObjectClone;

        public int LastChangedFrame
        {
            get => lastChangedFrame;

            set => lastChangedFrame = value;
        }

        public string GameObjectName => gameObjectName;

        #endregion


        #region serialization constructor

        protected Object2PropertiesMapping( SerializationInfo info, StreamingContext context )
        {
            savedStates = (Dictionary<int, SavedBase>) info.GetValue("savedStates", typeof(Dictionary<int, SavedBase>));
            isParentObj = info.GetBoolean("isParentObj");
            parentMapping = (Object2PropertiesMapping) info.GetValue("parentMapping", typeof(Object2PropertiesMapping));
            //Hack:already generator guid for scene BaseSceneObjects by xuXin  
            //isLoadByScene = info.GetBoolean( "isLoadByScene" );
            itemId = info.GetInt32("itemId");
            prefabPath = info.GetString("prefabPath");
            lastChangedFrame = info.GetInt32("lastChangedFrame");
            firstChangedFrame = info.GetInt32("firstChangedFrame");
            //childIdentificationMode = ChildIdentificationMode.IDENTIFY_BY_NAME;
            gameObjectInstanceID = info.GetInt32("gameObjectInstanceID");

            try
            {
                gameObjectName = info.GetString("gameObjectName");
            }
            catch (SerializationException)
            {
                //file was recorded using old version of this plugin
                //childIdentificationMode = ChildIdentificationMode.IDENTIFY_BY_ORDER;
                gameObjectName = "name_untraceable";
            }
        }

        public void GetObjectData( SerializationInfo info, StreamingContext ctxt )
        {
            info.AddValue("savedStates", savedStates);
            info.AddValue("isParentObj", isParentObj);
            info.AddValue("parentMapping", parentMapping);
            info.AddValue("itemId", itemId);
            info.AddValue("prefabPath", prefabPath);

            info.AddValue("lastChangedFrame", lastChangedFrame);
            info.AddValue("firstChangedFrame", firstChangedFrame);
            info.AddValue("gameObjectName", gameObjectName);
            info.AddValue("gameObjectInstanceID", gameObjectInstanceID);
        }

        #endregion

        #region Initialization & Destruction

        public Object2PropertiesMapping()
        {
        }

        public Object2PropertiesMapping( GameObject go, bool isParent, Object2PropertiesMapping parentMapping,
            string prefabLoadPath, ChildIdentificationMode childIdentificationMode )
            : this(go, isParent, parentMapping, prefabLoadPath)
        {
            this.childIdentificationMode = childIdentificationMode;

            if (isParentObj)
            {
                // if gameObject is a parent..
                //..instantiate mappings for all children too
                //Transform[] allChildren = gameObject.GetComponentsInChildren<Transform>();
                var recordChildren = gameObject.GetComponentsInChildren<Object2RecordChild>();
                for (var i = 0; i < recordChildren.Length; i++)
                {
                    var child = recordChildren[i].gameObject;
                    if (!EZReplayManager.Instance.gOs2propMappings.ContainsKey(child))
                        //if (child != gameObject && child.name.EndsWith( EZReplayManager.S_EZR_CHILD_Suffix ))
                        EZReplayManager.Instance.gOs2propMappings.Add(child,
                            new Object2PropertiesMapping(child, false, this, "", childIdentificationMode));
                    //else if (EZReplayManager.showHints)
                    //    MonoBehaviour.print( "EZReplayManager HINT: GameObject '" + child +
                    //                        "' is already being recorded. Will not be marked for recording again." );
                }
            }
        }

        public Object2PropertiesMapping( GameObject go, ISave sceneInstance, bool isParent, /*bool isLoadByScene,*/
            string prefabPath, int itemId )
            : this(go, isParent, null, 0, prefabPath, itemId)
        {
            //Hack:already generator guid for scene BaseSceneObjects by xuXin  
            //this.isLoadByScene = isLoadByScene;
            this.sceneInstance = sceneInstance;
            childIdentificationMode = ChildIdentificationMode.IDENTIFY_BY_COM;

            if (isParentObj)
            {
                // if gameObject is a parent..
                //..instantiate mappings for all children too
                //Transform[] allChildren = gameObject.GetComponentsInChildren<Transform>();
                var recordChildren = gameObject.GetComponentsInChildren<Object2RecordChild>(true);

                for (var i = 0; i < recordChildren.Length; i++)
                {
                    var child = recordChildren[i].gameObject;

                    if (!EZReplayManager.Instance.gOs2propMappings.ContainsKey(child))
                        //if (child != gameObject && child.name.EndsWith( EZReplayManager.S_EZR_CHILD_Suffix ))
                        EZReplayManager.Instance.gOs2propMappings.Add(child,
                            new Object2PropertiesMapping(child, false, this, "", childIdentificationMode));
                    else if (EZReplayManager.showHints)
                        MonoBehaviour.print("EZReplayManager HINT: GameObject '" + child +
                                            "' is already being recorded. Will not be marked for recording again.");
                }
            }
        }

        public Object2PropertiesMapping( GameObject go, bool isParent, Object2PropertiesMapping parentMapping,
            string prefabLoadPath ) : this(go, isParent, parentMapping)
        {
            prefabPath = prefabLoadPath;
        }

        public Object2PropertiesMapping( GameObject go, bool isParent, Object2PropertiesMapping parentMapping,
            int childNo, string prefabPath, int itemId ) : this(go, isParent, parentMapping)
        {
            this.prefabPath = prefabPath;
            this.itemId = itemId;
        }

        //as this is not derived from MonoBehaviour, we have a constructor
        public Object2PropertiesMapping( GameObject go, bool isParent, Object2PropertiesMapping parentMapping )
        {
            Init();
            //setting instance variables
            gameObject = go;
            isParentObj = isParent;
            this.parentMapping = parentMapping;
            gameObjectInstanceID = go.GetInstanceID();
            gameObjectName = go.name;
        }

        #endregion

        #region Core

        // insert a new state at certain position
        public void InsertStateAtPos( int recorderPosition )
        {
            SavedBase newState = null;
            if (sceneInstance != null)
            {
                newState = sceneInstance.GeneralSaveData();
                if (newState == null) newState = new SaveObjState(gameObject);
            }
            else
            {
                newState = new SaveObjState(gameObject);
            }


            var insertFrame = true;

            if (lastChangedFrame > -1)
            {
                //TODO:去除相同状态的数据量（减轻数据量）第二版改上去了
                if (savedStates.ContainsKey(lastChangedFrame))
                    // base record info
                    lock (savedStates)
                    {
                        var lastState = savedStates[lastChangedFrame];
                        if (newState.IsDifferentTo(lastState, this))
                        {
                            if (sceneInstance != null) newState = sceneInstance.GeneralSaveData(true);
                        }
                        else
                        {
                            insertFrame = false;
                        }
                    }
            }
            else
            {
                if (sceneInstance != null)
                    newState = sceneInstance.GeneralSaveData(true);
            }
            try
            {
                if (insertFrame)
                    lock (savedStates)
                    {
                        //Debug.Log($"InsertStateAtPos new State{gameObject.name}");
                        savedStates.Add(recorderPosition, newState);
                        lastChangedFrame = recorderPosition;

                        if (firstChangedFrame == -1)
                            firstChangedFrame = recorderPosition;
                    }
            }
            catch
            {
                MonoBehaviour.print("EZReplayManager ERROR: You probably already inserted at position '" +
                                    recorderPosition + "' for game object '" + gameObject + "'.");
            }
        }

        public void PrepareObjectForReplay( UnityAction action )
        {
            if (isParentObj)
            {
                //if is a parent gameObject mapping 
                //sceneInstance = EZReplayHelper.GetOrCreateEZObject(this,/*isLoadByScene,*/ prefabPath, itemId) as ISave;
                //if (sceneInstance != null)
                //{
                //    gameObjectClone = sceneInstance.GetRepresent();
                //}

                //if (gameObjectClone == null)
                //{
                //    SsitDebug.Error("|prefabPath|" + prefabPath + "|itemId|" + itemId);
                //    return;
                //}

                //new version
                EZReplayHelper.GetOrCreateEZObject(this, prefabPath, itemId,
                    delegate( BaseObject obj, object render, object data )
                    {
                        OnCreateEZObject(obj, render, data);
                        action.Invoke();
                    });
            }
            else
            {
                // if is a child (can also be a parent in game scene hierachy but "EZReplayManager.mark4recording()" has not been called for this object specifically, so we handle it as a child
                if (parentMapping == null)
                {
                    SsitDebug.Error("parentMapping is null");
                    return;
                }
                var myParentClone = parentMapping.CloneEntityObj;
                //Debug.Log( parentMapping.gameObjectName + "expression" + parentMapping.guid );
                if (myParentClone == null) SsitDebug.Error("parentMapping.getGameObjectClone() is null");
                //filter child by childIdentify
                switch (childIdentificationMode)
                {
                    case ChildIdentificationMode.IDENTIFY_BY_NAME:
                    {
                        var allChildren = myParentClone.GetComponentsInChildren<Transform>(true);

                        for (var i = 0; i < allChildren.Length; i++)
                        {
                            var child = allChildren[i].gameObject;
                            //map child to order number or go-name

                            if (gameObjectName == child.name)
                            {
                                gameObjectClone = child;
                                break;
                            }
                        }
                    }
                        break;
                    case ChildIdentificationMode.IDENTIFY_BY_COM:
                    {
                        var recordChildren = myParentClone.GetComponentsInChildren<Object2RecordChild>(true);

                        for (var i = 0; i < recordChildren.Length; i++)
                        {
                            var child = recordChildren[i].gameObject;
                            //map child to order number or go-name

                            if (gameObjectName == child.name)
                            {
                                gameObjectClone = child;
                                break;
                            }
                        }
                    }
                        break;
                    case ChildIdentificationMode.IDENTIFY_BY_ORDER:
                        //暂不支持
                        break;
                }
                PrepareObjectForReplay();
                action.Invoke();
            }
        }

        private void OnCreateEZObject( BaseObject obj, object render, object data )
        {
            if (data != null)
            {
                sceneInstance = data as ISave;
                if (sceneInstance != null)
                    gameObjectClone = sceneInstance.GetRepresent();
            }
            else
            {
                sceneInstance = obj.SceneInstance;
                if (sceneInstance == null)
                {
                    var go = render as GameObject;
                    sceneInstance = go.GetComponent<ISave>();
                }
                if (sceneInstance != null)
                    gameObjectClone = sceneInstance.GetRepresent();
            }

            if (gameObjectClone == null)
            {
                SsitDebug.Error($"记录实体表现在Isave中未找到{prefabPath}");
                return;
            }

            PrepareObjectForReplay();
        }

        private void PrepareObjectForReplay()
        {
            /*gameObjectClone.GetInstanceID() + "_" +*/
            //gameObjectClone.name = gameObjectInstanceID + "_" + gameObjectClone.name;

            // if (gameObjectInstanceID > -1)
            // can happen when file was loaded. obviously this doesn't work with loaded files yet.
            try
            {
                EZReplayManager.Instance.instanceIDtoGO.Add(gameObjectInstanceID, this);
            }
            catch (Exception e)
            {
                Debug.LogError("expression" + gameObjectClone.name + e.Message);
                throw;
            }

            if (gameObjectClone == null) return;

            // kill all unneccessary scripts on gameObjectClone
            //Component[] allComps = gameObjectClone.GetComponentsInChildren<Component>( true );
            //List<Component> componentsToKill = new List<Component>();

            var found = false;
            for (var i = 0; i < EZReplayManager.Instance.componentsAndScriptsToKeepAtReplay.Count; i++)
            {
                var temp = gameObjectClone.GetComponentsInChildren(
                    EZReplayManager.Instance.componentsAndScriptsToKeepAtReplay[i], true);

                if (temp.Length > 0) found = true;
            }
            if (!found)
            {
                //Exclude scripts and components from removal: (this is done to preserve basic functionality and render

                // take exceptions from public array "EZReplayManager.componentsAndScriptsToKeepAtReplay"

                var colliders = gameObjectClone.GetComponentsInChildren<Collider>(true);

                if (colliders.Length > 0)
                    foreach (var cc in colliders)
                        cc.enabled = false;

                var rigidbodies = gameObjectClone.GetComponentsInChildren<Rigidbody>(true);
                if (rigidbodies.Length > 0)
                    foreach (var cc in rigidbodies)
                        cc.isKinematic = true;

                var navmesh = gameObjectClone.GetComponentsInChildren<NavMeshAgent>(true);
                if (rigidbodies.Length > 0)
                    foreach (var cc in navmesh)
                        cc.enabled = false;
            }

            var thisCloneScript = gameObjectClone.AddComponent<EZR_Clone>();
            thisCloneScript.origInstanceID = gameObjectInstanceID;
            thisCloneScript.cloneInstanceID = gameObjectClone.GetInstanceID();

            if (EZReplayManager.Instance.autoDeactivateLiveObjectsOnReplay && gameObject != null)
                gameObject.SetActive(false);

            // 重复帧的拷贝
            if (savedStates.Count <= 0)
                return;

            var mostRecent = savedStates[firstChangedFrame];
            for (var i = firstChangedFrame + 1; i <= lastChangedFrame; i++)
                if (savedStates.ContainsKey(i))
                {
                    var tt = savedStates[i];
                    tt.TrackClone(mostRecent);
                    mostRecent = savedStates[i];
                }
                else
                {
                    savedStates.Add(i, mostRecent);
                }
        }

        //synchronize gameObjectClone to a certain state at a certain  recorderPosition
        public void SynchronizeProperties( int recorderPosition, bool isReset = false )
        {
            if (firstChangedFrame > -1 && recorderPosition >= firstChangedFrame)
            {
                if (recorderPosition <= lastChangedFrame)
                {
                    try
                    {
                        var isFristFrame = recorderPosition == firstChangedFrame;
                        //bool isReset = Mathf.Abs(recorderPosition - lastRecordPositon) > 1;
                        //gameObjectClone.active = true;
                        if (sceneInstance != null)
                            sceneInstance.SynchronizeProperties(GetStateAtPos(recorderPosition), isReset, isFristFrame);
                        else
                            GetStateAtPos(recorderPosition)
                                .SynchronizeProperties(gameObjectClone, this, isReset, isFristFrame);

                        lastRecordPositon = recorderPosition;
                    }
                    catch (NullReferenceException)
                    {
                    }
                }
                else
                {
                    if (sceneInstance != null)
                        sceneInstance.SynchronizeProperties(GetStateAtPos(lastChangedFrame), isReset, false);
                    else
                        GetStateAtPos(lastChangedFrame).SynchronizeProperties(gameObjectClone, this, isReset, false);
                    lastRecordPositon = lastChangedFrame;
                }
            }
            else if (gameObjectClone && gameObjectClone.activeSelf)
            {
                gameObjectClone.SetActive(false);
            }
        }

        #endregion

        #region internal & external Members

        public int GetMaxFrames()
        {
            var maxframes = 0;
            foreach (var stateEntry in savedStates)
                if (stateEntry.Key > maxframes)
                    maxframes = stateEntry.Key;
            return maxframes;
        }

        /// <summary>
        /// executed just before stopping a replay
        /// </summary>
        public void ResetObject()
        {
            var superParent = GameObject.Find(EZReplayManager.S_PARENT_NAME);
            //destroy superParent if not yet done
            if (superParent != null)
                Object.Destroy(superParent);
            //clear clones list
            if (EZReplayManager.Instance.instanceIDtoGO.Count > 0)
                EZReplayManager.Instance.instanceIDtoGO.Clear();

            //reactivate gameObject
            if (gameObject != null && EZReplayManager.Instance.autoDeactivateLiveObjectsOnReplay)
            {
                if (savedStates.ContainsKey(lastChangedFrame) && lastChangedFrame > -1)
                    gameObject.SetActive(savedStates[lastChangedFrame].isActive);
                else
                    gameObject.SetActive(true);
            }
        }

        public void ClearStates()
        {
            savedStates.Clear();
        }

        public int GetAmountStates()
        {
            return savedStates.Count;
        }

        private void Init()
        {
            lastChangedFrame = -1;
            firstChangedFrame = -1;
            InitCacheSet();
        }

        private SavedBase GetStateAtPos( int recorderPosition )
        {
            if (!savedStates.ContainsKey(recorderPosition))
                SsitDebug.Error($"guid {prefabPath},name{gameObjectClone.name}recordPos{recorderPosition}");
            return savedStates[recorderPosition];
        }

        #endregion

        #region write track cache

        private int m_FlushSize;
        private int m_CacheSize;

        private BinaryWriter bw;
        private BinaryReader br;

        private List<int> recordIndex;

        private void InitCacheSet()
        {
            m_FlushSize = 300;
            m_CacheSize = 200;
            recordIndex = new List<int>();
        }

        public void WriteTrack()
        {
            if (savedStates.Count > m_FlushSize)
            {
                if (bw == null)
                    bw = new BinaryWriter(new FileStream(EZReplayManager.Instance.CachePath + GetHashCode(),
                        FileMode.CreateNew));
                //Debug.Log( GetHashCode() + prefabPath );
                var cacheMap = new Dictionary<int, SavedBase>();
                lock (savedStates)
                {
                    foreach (var state in savedStates)
                    {
                        if (cacheMap.Count >= m_CacheSize) break;
                        cacheMap.Add(state.Key, state.Value);
                    }
                    foreach (var state in cacheMap) savedStates.Remove(state.Key);
                }
                var memStream = new MemoryStream();
                var bFormatter = new BinaryFormatter();
                bFormatter.Serialize(memStream, cacheMap);
                var temp = memStream.GetBuffer();
                bw.Write(temp.Length);
                bw.Write(memStream.GetBuffer());
                recordIndex.Add(temp.Length);
                memStream.Dispose();
                var disposeList = cacheMap.Values.ToList();
                for (var i = 0; i < disposeList.Count; i++)
                {
                    disposeList[i].Shutdown();
                }
            }
        }

        private void ReadTrack()
        {
            if (recordIndex.Count > 0 && bw == null)
                br = new BinaryReader(new FileStream(EZReplayManager.Instance.CachePath + GetHashCode(),
                    FileMode.Open));
            var bFormatter = new BinaryFormatter();
            foreach (var record in recordIndex)
            {
                br.ReadInt32();
                var stream = new MemoryStream(br.ReadBytes(record));

                var objectToSerialize = (Dictionary<int, SavedBase>) bFormatter.Deserialize(stream);

                foreach (var state in objectToSerialize) savedStates.Add(state.Key, state.Value);
            }
            br = null;
        }

        public void ClearTrack()
        {
            if (bw != null)
            {
                bw.Close();
                bw = null;
            }
            ReadTrack();

            recordIndex = null;
        }

        private void SaveTrack()
        {
        }

        #endregion
    }
}