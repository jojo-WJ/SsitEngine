//#define ThreadCache

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Framework;
using Framework.Data;
using Framework.Logic;
using Framework.SceneObject;
using SSIT.proto;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity;
using SsitEngine.Unity.WebRequest;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SsitEngine.EzReplay
{
    /// <summary>
    /// this class handles all global setting and update
    /// </summary>
    public partial class EZReplayManager : ManagerBase<EZReplayManager>
    {
        #region Clone Serach

        public Object2PropertiesMapping GetInstanceMap( int instanceID )
        {
            Object2PropertiesMapping ret = null;
            instanceIDtoGO.TryGetValue(instanceID, out ret);
            return ret;
        }

        #endregion

        #region Inerval_GenerateCachePath

        /// <summary>
        /// create prefab's load Path by format
        /// </summary>
        /// <param name="gameObjectName"></param>
        /// <param name="prefabLoadPath"></param>
        /// <returns></returns>
        public string GenerateCachePath( string gameObjectName, string prefabLoadPath )
        {
            // todo:need can change
            var standardFilename = "go_" + gameObjectName;
            if (prefabLoadPath == "")
            {
                prefabLoadPath = S_EZR_ASSET_PATH + "/" + standardFilename;
            }

            return prefabLoadPath;
        }

        #endregion

        #region Inteval_Record

        /// <summary>
        /// execute one cycle of the current action on the current recorder position
        /// </summary>
        protected void ExecRecorderAction( bool isReset = false )
        {
            // check propertyMapping is exist
            lock (gOs2propMappings)
            {
                foreach (var entry in gOs2propMappings)
                {
                    var go = entry.Key;
                    var propMapping = entry.Value;

                    if (CurrentAction == ActionMode.RECORD && CurrentMode == ViewMode.LIVE)
                    {
                        //if recording
                        //if (recorderPosition <= (100 * (int)ActionMode.STOPPED) / (int)ActionMode.PAUSED)
                        {
                            maxPosition = recorderPosition;
                            propMapping.InsertStateAtPos(recorderPosition);
                            //propMapping.InsertStateAtPosRe( recorderPosition );
                        }
                        //else
                        //{
                        //    showingStoppedRecordingMsg = true;
                        //    StartCoroutine( exitStoppedRecordingMsg( 5f ) );
                        //    stop();
                        //}
                    }
                    else if (CurrentMode == ViewMode.REPLAY)
                    {
                        //if replaying
                        //if in between start and finish position
                        if (recorderPosition <= maxPosition && orgRecorderPositionStep > 0 ||
                            recorderPosition > 0 && orgRecorderPositionStep < 0)
                        {
                            //lerping not integrated yet
                            //float updateSyncTime = Time.realtimeSinceStartup;
                            //float lerpInterval = interval - ((updateSyncTime - updateStartingTime) % interval) ;
                            if (propMapping.CloneEntityObj != null)
                            {
                                propMapping.SynchronizeProperties(recorderPosition, isReset);
                            }
                        }
                        else
                        {
                            //else if reached the finishing position
                            Stop();
                            if (exitOnFinished)
                            {
                                if (OnEZReplayEnd != null)
                                {
                                    OnEZReplayEnd();
                                }
                                SwitchModeTo(ViewMode.LIVE);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Interval_SendCallback

        protected void SendCallback2All( string functionName, object parameter )
        {
            if (sendCallbacks && callbacksToExecute.Contains(functionName))
            {
                var gameObjects = (GameObject[]) FindObjectsOfType(typeof(GameObject));

                foreach (var go in gameObjects)
                {
                    go.SendMessage(functionName, parameter, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        #endregion

        #region Interval_Assist

        /// <summary>
        /// 显示强制预播的消息
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        protected IEnumerator StartShowPrecachingMandatoryMessage( float seconds )
        {
            showPrecachingMandatoryMessage = true;
            yield return new WaitForSeconds(seconds);
            showPrecachingMandatoryMessage = false;
        }

        //public void ResetWatch()
        //{
        //    this.mWatch.Reset();
        //    this.mWatch.Start();
        //}

        //public void StopWatch()
        //{
        //    this.mWatch.Stop();
        //}

        //public float GetSeconds()
        //{
        //    return Mathf.CeilToInt((float)this.mWatch.Elapsed.Duration().TotalSeconds);
        //}

        #endregion

        #region Variable

        // version index
        public const string EZR_VERSION = "1.0";

        // const default use of Editor
        public const bool showEorror = true;
        public bool showWarnings = true;
        public const bool showHints = true;

        // root name
        public const string S_PARENT_NAME = "EZREPLAYROOT";

        // default assets path
        public const string S_EZR_ASSET_PATH = "EZRepalyAssetPrefabs";


        // don't change these manually unless you know waht you are doing
        protected ViewMode currentMode = ViewMode.LIVE;
        private ActionMode currentAction = ActionMode.STOPPED;
        protected int recorderPosition;
        protected int maxPosition;
        protected float playingInterval;
        protected int recorderPositionStep = 1;

        // org default = 1
        protected int orgRecorderPositionStep;
        protected bool exitOnFinished;

        protected float surplus;

//        private float timeelapsed = 0.0f;
        protected bool continueCallingUpdate;
        protected bool showPrecachingMandatoryMessage;

        // default: true
        public bool compressSaves = true;

        // default: true
        public bool sendCallbacks;
        // default: false

        // default: true
        public bool autoDeactivateLiveObjectsOnReplay = true;

        // change these to configure the overall performance of the script:
        // default: 0.05f (20fps) .. if your replay is not fluent enough lower this value step by step.. try 0.04f (25fps) first, lower it then
        public float recordingInterval = 0.05f;

        // default: 3
        public const int maxSpeedSliderValue = 3;

        // default: -3
        public const int minSpeedSliderValue = 1;

        // default: 1
        private int speedSliderValue = 1;

        // default: 1
        protected int speedSliderValueBackup = 1;

        // maps

        // pre method callback
        public List<string> callbacksToExecute = new List<string>();

        // here all mappings are done from the original game objects to their replay-counterpart clones
        public Dictionary<GameObject, Object2PropertiesMapping> gOs2propMappings =
            new Dictionary<GameObject, Object2PropertiesMapping>();

        // 保持组件完整性在回放模式下
        public List<Type> componentsAndScriptsToKeepAtReplay = new List<Type>();

        // don't fill on your own. Only usable in replay mode
        public Dictionary<int, Object2PropertiesMapping> instanceIDtoGO =
            new Dictionary<int, Object2PropertiesMapping>();

        //计时器测试
        //protected Stopwatch mWatch;

        #endregion

        #region Property

        /// <summary>
        /// bad function name: not only sets replay speed but also 
        /// </summary>
        /// <param name="speed"></param>
        /// <returns>replaying speed relative to the recording speed</returns>
        public string SetReplaySpeed( int speed )
        {
            var ret = "";

            /*if (speed == minSpeedSliderValue)
            {

                playingInterval = 0.0f;
                recorderPositionStep = orgRecorderPositionStep;
                CurrentAction = ActionMode.PAUSED;
                ret = "Paused";
            }
            else*/
            if (speed >= speedSliderValueBackup)
            {
                playingInterval = Instance.recordingInterval;

                var increaser = 1;
                if (orgRecorderPositionStep < 0)
                {
                    increaser = -1;
                }

                recorderPositionStep = orgRecorderPositionStep * (speed - 1) + increaser;
                var multiplicator = (int) Mathf.Round(recorderPositionStep / orgRecorderPositionStep);
                ret = "~ " + multiplicator + "x faster";
            }
            /*取消减速慢放（没有意义）
             else if (speed < speedSliderValueBackup)
            {

                playingInterval = EZReplayManager.Instance.recordingInterval * ((speed - 1) * -2);
                recorderPositionStep = orgRecorderPositionStep;
                int divisor = (int)Mathf.Round(playingInterval / EZReplayManager.Instance.recordingInterval);
                ret = "~ " + divisor + "x slower";
            }*/
            else
            {
                /*if (speed == sliderValueBackup)*/
                //playingInterval = EZReplayManager.Instance.recordingInterval;
                //recorderPositionStep = orgRecorderPositionStep;
                //ret = "~ Recording speed";

                playingInterval = 0.0f;
                recorderPositionStep = orgRecorderPositionStep;
                CurrentAction = ActionMode.PAUSED;
                ret = "Paused";
            }

            Debug.Log($"ret{ret}speed{speed}");
            return ret;
        }

        public ActionMode CurrentAction
        {
            get => currentAction;

            set
            {
                if (currentAction != value)
                {
                    var action = currentAction;
                    currentAction = value;
                    if (OnEZReplayActionStateChange != null)
                    {
                        OnEZReplayActionStateChange(this, action, currentAction);
                    }
                }
            }
        }


        protected ViewMode CurrentMode
        {
            get => currentMode;
            private set => currentMode = value;
        }

        public long RecordBeginTime { get; private set; }


        public long RecordEndTime { get; set; }

        public string GetFileName()
        {
            return TextUtils.ConvertLongToDateTime(RecordBeginTime).ToFileTime() + ".ezr";

            //return Util.ConvertLongToDateTime( recordBeginTime ).ToString("yyyy/MM/dd").Replace( "/", "_" );
        }

        #endregion

        #region CallBack

        public UnityAction OnEZRecord;
        public UnityAction OnEZRePrepareObject;
        public UnityAction OnEZReplayEnd;
        public UnityAction<float> OnEzReplayProcess;
        public UnityAction<object, ActionMode, ActionMode> OnEZReplayActionStateChange;

        #endregion

        #region Mono

        private void Awake()
        {
#if UNITY_IPHONE
            System.Environment.SetEnvironmentVariable( "MONO_REFLECTION_SERIALIZER", "yes" );
#endif
            Init();
            sendCallbacks = true;
            callbacksToExecute.Add("__EZR_pause");
            callbacksToExecute.Add("__EZR_stop");
        }

        private void Start()
        {
            //MarkPredefinedObjects4Recording();
            componentsAndScriptsToKeepAtReplay.Add(typeof(PlayerInstance));
        }

        #endregion

        // Exterval Members Method

        #region Exterval_MarkRecord

        /// <summary>
        /// mark an object for recording, can be done while recording and while script is nonactive, 
        /// but not while replaying a recording
        /// </summary>
        /// <param name="go">mark Object</param>
        public void Mark4Recording( GameObject go )
        {
            Mark4Recording(go, "");
        }

        public void Mark4Recording( GameObject go, string prefabLoadPath )
        {
            if (CurrentMode == ViewMode.LIVE)
            {
                if (CurrentAction != ActionMode.PLAY)
                {
                    lock (gOs2propMappings)
                    {
                        if (!gOs2propMappings.ContainsKey(go))
                            //if not already existant	
                        {
                            gOs2propMappings.Add(go,
                                new Object2PropertiesMapping(go, true, null, prefabLoadPath,
                                    ChildIdentificationMode.IDENTIFY_BY_COM));
                        }
                        else if (showHints)
                        {
                            SsitDebug.Debug("EZReplayManager HINT: GameObject '" + go +
                                            "' has already been marked for recording.");
                        }
                    }
                }
                else if (showWarnings)
                {
                    SsitDebug.Warning("EZReplayManager WARNING: You cannot mark GameObject '" + go +
                                      "' for recording while a recording is being played.");
                }
            }
            else
            {
                if (showWarnings)
                {
                    SsitDebug.Warning("EZReplayManager WARNING: You cannot mark GameObject '" + go +
                                      "' for recording while in replay mode.");
                }
            }
        }

        public void Mark4Recording( GameObject go, ISave baseObj, string prefabName = "" )
        {
            if (baseObj != null)
            {
                //SsitDebug.Log( "Mark4Recording" + baseObj.LoadedByScene + "||" + baseObj.name + "||" + baseObj.ObjectGUID );
                Mark4Recording(go, baseObj, /*baseObj.LoadedByScene,*/ baseObj.Guid, baseObj.ItemID);

                return;
            }
            Mark4Recording(go, prefabName);
        }

        /// <summary>
        /// mark an object for recording, can be done while recording and while script is nonactive,
        /// but not while replaying a recording
        /// </summary>
        /// <param name="go">mark Object</param>
        /// <param name="prefabLoadPath">prefab's load path</param>
        /// <param name="childIdentificationMode">subnode's identify mode</param>
        public void Mark4Recording( GameObject go, string prefabLoadPath,
            ChildIdentificationMode childIdentificationMode )
        {
            if (CurrentMode == ViewMode.LIVE)
            {
                if (CurrentAction != ActionMode.PLAY)
                {
                    lock (gOs2propMappings)
                    {
                        if (!gOs2propMappings.ContainsKey(go))
                            //if you dont need to precache:
                        {
                            gOs2propMappings.Add(go,
                                new Object2PropertiesMapping(go, true, null, "", childIdentificationMode));
                        }
                        else if (showHints)
                        {
                            SsitDebug.Debug("EZReplayManager HINT: GameObject '" + go +
                                            "' has already been marked for recording.");
                        }
                    }
                }
                else if (showWarnings)
                {
                    SsitDebug.Warning("EZReplayManager WARNING: You cannot mark GameObject '" + go +
                                      "' for recording while a recording is being played.");
                }
            }
            else
            {
                if (showWarnings)
                {
                    SsitDebug.Warning("EZReplayManager WARNING: You cannot mark GameObject '" + go +
                                      "' for recording while in replay mode.");
                }
            }
        }

        public void Mark4Recording( GameObject go, ISave sceneInstance, /*bool isLoadByScene,*/ string guid,
            int itemId )
        {
            if (CurrentMode == ViewMode.LIVE)
            {
                if (CurrentAction != ActionMode.PLAY)
                {
                    lock (gOs2propMappings)
                    {
                        if (!gOs2propMappings.ContainsKey(go))
                            //Debug.Log($"expression go{go.name} guid {guid} itemid {itemId}");
                        {
                            gOs2propMappings.Add(go,
                                new Object2PropertiesMapping(go, sceneInstance, true /*, isLoadByScene*/, guid,
                                    itemId));
                        }
                        else if (showHints)
                        {
                            SsitDebug.Debug("EZReplayManager HINT: GameObject '" + go +
                                            "' has already been marked for recording.");
                        }
                    }
                }
                else if (showWarnings)
                {
                    SsitDebug.Warning("EZReplayManager WARNING: You cannot mark GameObject '" + go +
                                      "' for recording while a recording is being played.");
                }
            }
            else
            {
                if (showWarnings)
                {
                    SsitDebug.Warning("EZReplayManager WARNING: You cannot mark GameObject '" + go +
                                      "' for recording while in replay mode.");
                }
            }
        }

        #endregion

        #region Exterval_ViewStateChange

        /// <summary>
        /// switch to different mode.
        /// so far there are MODE_LIVE for viewing a normal game action and MODE_REPLAY for viewing a replay of a recording
        /// </summary>
        /// <param name="newMode">ViewMode</param>
        public void SwitchModeTo( ViewMode newMode )
        {
            if (newMode == ViewMode.LIVE)
            {
                //SendCallback2All( "__EZR_live_prepare", null );

                //// reset game object (i.e. rigidbody state)
                //foreach (KeyValuePair<GameObject, Object2PropertiesMapping> entry in gOs2propMappings)
                //{

                //    //GameObject go = entry.Key;
                //    Object2PropertiesMapping propMapping = entry.Value;
                //    if (propMapping.getGameObject() != null)
                //        propMapping.resetObject();

                //}

                //bool tmpWasLoading = IsLoadingSlotInUse();

                //UseRecordingSlot();

                //if (tmpWasLoading) //repeat to avoid a bug
                //    SwitchModeTo( ViewMode.LIVE );

                //// count freames
                //maxPosition = GetMaxFrames( gOs2propMappings );

                //SendCallback2All( "__EZR_live_ready", null );
            }
            else
            {
                SendCallback2All("__EZR_replay_prepare", null);
                instanceIDtoGO.Clear();
                if (maxPosition > 0)
                {
                    //lock (gOs2propMappings)
                    //{
                    //    // prepare parents first
                    //    foreach (KeyValuePair<GameObject, Object2PropertiesMapping> entry in gOs2propMappings)
                    //    {
                    //        //GameObject go = entry.Key;
                    //        Object2PropertiesMapping propMapping = entry.Value;
                    //        if (propMapping.IsParent)
                    //        {
                    //            //if (propMapping.getGameObject() != null)
                    //            propMapping.PrepareObjectForReplay();
                    //        }
                    //    }
                    //    //..then childs
                    //    foreach (KeyValuePair<GameObject, Object2PropertiesMapping> entry in gOs2propMappings)
                    //    {
                    //        //GameObject go = entry.Key;
                    //        Object2PropertiesMapping propMapping = entry.Value;
                    //        if (!propMapping.IsParent)
                    //        {
                    //            //if (propMapping.getGameObject() != null)
                    //            propMapping.PrepareObjectForReplay();
                    //        }
                    //    }
                    //}

                    SendCallback2All("__EZR_replay_ready", null);
                }
                else
                {
                    newMode = ViewMode.LIVE;
                    if (showWarnings)
                    {
                        SsitDebug.Warning(
                            "EZReplayManager WARNING: You have not recorded anything yet. Will not replay.");
                    }
                }
            }

            CurrentMode = newMode;
            Stop();
        }

        //HINT: lerping is still highly experimental, to use/develope enable in SavedState.cs
        /// <summary>
        /// 帧之间的差值移动（未启用）
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="target"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public IEnumerator MoveTo( Transform transform, Vector3 target, float time )
        {
            var start = transform.position;
            float t = 0;
            time = playingInterval;

            while (t < 1)
            {
                yield return null;
                t += Time.deltaTime / time;
                var temp = Vector3.Lerp(start, target, t);
                if (transform == null)
                {
                    yield break;
                }
                transform.position = new Vector3(temp.x, transform.position.y, temp.z);
            }
            transform.position = new Vector3(target.x, transform.position.y, target.z);
        }

        public IEnumerator MoveTo( Transform transform, Vector3 target, Quaternion rotate, float time )
        {
            var start = transform.position;
            var rot = transform.rotation;
            float t = 0;
            time = playingInterval;

            while (t < 1)
            {
                yield return null;
                t += Time.deltaTime / time;
                transform.position = Vector3.Lerp(start, target, t);
                transform.rotation = Quaternion.Lerp(rot, rotate, t);
            }
            transform.position = target;
        }

        #endregion

        #region Exterval_Replay

        public void ReadyRecord( bool value )
        {
            CurrentAction = value ? ActionMode.READY : ActionMode.STOPPED;
            if (value)
            {
                Instance.OnEZRecord = () =>
                {
                    CurrentAction = ActionMode.STOPPED;
                    Record();
                    Instance.OnEZRecord = null;
                };
            }
            else
            {
                Instance.OnEZRecord = null;
            }
        }

        /// <summary>
        /// this starts the recording of objects in runtime
        /// </summary>
        public void Record()
        {
            if (CurrentMode == ViewMode.LIVE)
            {
                if (CurrentAction == ActionMode.STOPPED)
                {
                    lock (gOs2propMappings)
                    {
                        //remove a previous recording
                        foreach (var entry in gOs2propMappings)
                        {
                            //GameObject go = entry.Key;
                            var propMapping = entry.Value;
                            propMapping.ClearStates();
                        }
                    }
                    //reset everything to standard values
                    recorderPosition = 0;
                    recorderPositionStep = 1;
                    orgRecorderPositionStep = 1;
                    //ResetWatch();
                    RecordBeginTime = TextUtils.ConvertDataTimeToLong(DateTime.Now);

                    //set new action
                    CurrentAction = ActionMode.RECORD;
                    continueCallingUpdate = true;
                    UpdateRecording(CurrentAction);

                    SendCallback2All("__EZR_record", null);

                    //InitCache();
                }
                else
                {
                    if (showWarnings)
                    {
                        SsitDebug.Warning(
                            "EZReplayManager WARNING: Ordered to record when recorder was not in stopped-state. Will not start recording.");
                    }
                }
            }
            else
            {
                if (showWarnings)
                {
                    SsitDebug.Warning(
                        "EZReplayManager WARNING: Ordered to record when recorder was in replay mode. Will not start recording.");
                }
            }
        }

        /// <summary>
        /// simple wrapper for the play-method
        /// </summary>
        /// <param name="speed"></param>
        public void Play( int speed )
        {
            Play(speed, false, false, false);
        }

        public void Play()
        {
            Play(speedSliderValue, true, false, false);
        }

        /// <summary>
        /// replays a recording.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="playImmediately"></param>
        /// <param name="backwards">is Continue play</param>
        /// <param name="exitOnFinished"></param>
        public void Play( int speed, bool playImmediately, bool backwards, bool exitOnFinished )
        {
            //switch to correct mode
            if (CurrentMode != ViewMode.REPLAY)
            {
                SwitchModeTo(ViewMode.REPLAY);
            }

            if (speed >= minSpeedSliderValue && speed <= maxSpeedSliderValue)
            {
                speedSliderValue = speed;
            }
            else
            {
                speedSliderValue = 0;
            }

            //revert playing direction if neccessary
            if (backwards && orgRecorderPositionStep > 0 || !backwards && orgRecorderPositionStep < 0)
            {
                orgRecorderPositionStep *= -1;
            }
            //set playing speed
            SetReplaySpeed(speedSliderValue);

            if (CurrentAction == ActionMode.STOPPED || CurrentAction == ActionMode.PAUSED)
            {
                if (CurrentAction != ActionMode.PAUSED)
                {
                    Stop();
                }

                if (playImmediately)
                {
                    CurrentAction = ActionMode.PLAY;
                }

                this.exitOnFinished = exitOnFinished;
                continueCallingUpdate = playImmediately;
                UpdateRecording(CurrentAction, true);

                SendCallback2All("__EZR_play", null);
            }
            else if (showHints)
            {
                print("EZReplayManager HINT: Ordered to play when not in stopped or paused state.");
            }
        }

        /// <summary>
        /// halt a replay
        /// </summary>
        public void Pause()
        {
            if (CurrentMode == ViewMode.REPLAY)
            {
                CurrentAction = ActionMode.PAUSED;
                SetReplaySpeed(0 /*minSpeedSliderValue*/);

                SendCallback2All("__EZR_pause", null);
            }
            else if (showWarnings)
            {
                SsitDebug.Warning(
                    "EZReplayManager WARNING: Ordered to pause when recorder was not in replay mode. Will not pause.");
            }
        }


        /// <summary>
        /// stopping is essential to all other actions for resetting settings before switching
        /// </summary>
        public void Stop()
        {
            continueCallingUpdate = false;
            CurrentAction = ActionMode.STOPPED;
            if (orgRecorderPositionStep < 0)
            {
                recorderPosition = maxPosition;
            }
            else
            {
                recorderPosition = 0;
            }
            surplus = 0.0f;
            //ExecRecorderAction();
            SendCallback2All("__EZR_stop", null);
        }

        public void Exit()
        {
            StopAllCoroutines();
            ClearCache();
            /*if (mEzProxy != null)
            {
                mEzProxy.ClearCacheInfo();
            }*/
            if (gOs2propMappings != null)
            {
                gOs2propMappings.Clear();
            }
            instanceIDtoGO.Clear();
            SwitchModeTo(ViewMode.LIVE);
        }

        public int GetMaxFrames( Dictionary<GameObject, Object2PropertiesMapping> go2o2pm )
        {
            var maxframes = 0;
            foreach (var entry in go2o2pm)
            {
                var tmp = entry.Value.GetMaxFrames();
                if (tmp > maxframes)
                {
                    maxframes = tmp;
                }
            }
            return maxframes;
        }

        public int GetMaxFrames( Object2PropertiesMapping o2pm )
        {
            return o2pm.GetMaxFrames();
        }

        public float GetRePlayProcess()
        {
            return (float) recorderPosition / maxPosition;
        }

        public void SetProcess( float vlaue, float offset = 30 )
        {
            var recorderPositionTemp = Mathf.CeilToInt(vlaue * maxPosition);
            if (Mathf.Abs(recorderPositionTemp - recorderPosition) > offset)
            {
                StopAllCoroutines();
                Pause();
                recorderPosition = recorderPositionTemp;
                Play();
                //Pause();
            }
        }

        #endregion

        // Interval Members Method

        #region Interval_Init

        protected void Init()
        {
            orgRecorderPositionStep = recorderPositionStep;
            //this.mWatch = new Stopwatch();
            //this.mWatch.Reset();
        }


        protected void InitScene()
        {
            SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
        }

        private void SceneManagerOnSceneLoaded( Scene scene, LoadSceneMode loadSceneMode )
        {
            //终止回放系统
            Stop();
        }

        #endregion

        #region Interval_Serialized

        //serialize and save (do not call directly, call save() instead)
        protected byte[] SerializeObject( Object2PropertiesMappingListWrapper objectToSerialize )
        {
            var memStream = new MemoryStream();
            var bFormatter = new BinaryFormatter();
            bFormatter.Serialize(memStream, objectToSerialize);
            if (compressSaves)
            {
                return CLZF2.Compress(memStream.ToArray());
            }
            return memStream.ToArray();
        }

        /// <summary>
        /// deserialize and return for loading (do not call directly, call load() instead)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected Object2PropertiesMappingListWrapper DeSerializeObject( byte[] data )
        {
            Object2PropertiesMappingListWrapper objectToSerialize = null;

            if (compressSaves)
            {
                try
                {
                    data = CLZF2.Decompress(data);
                }
                catch (OutOfMemoryException)
                {
                    if (showWarnings)
                    {
                        SsitDebug.Warning(
                            "EZReplayManager WARNING: Decompressing was unsuccessful. Trying without decompression.");
                    }
                }
                catch (OverflowException)
                {
                    if (showWarnings)
                    {
                        SsitDebug.Warning(
                            "EZReplayManager WARNING: Decompressing was unsuccessful. Trying without decompression.");
                    }
                }
            }
            var stream = new MemoryStream(data);

            var bFormatter = new BinaryFormatter();
            objectToSerialize = (Object2PropertiesMappingListWrapper) bFormatter.Deserialize(stream);

            //UnityEngine.SsitDebug.LogError( "System.GC.GetTotalMemory(): " + System.GC.GetTotalMemory( false ) );

            return objectToSerialize;
        }

        #endregion

        #region Interval UpdateRecord

        protected void UpdateRecording( ActionMode action, bool isReset = false )
        {
            var updateStartingTime = Time.realtimeSinceStartup;
            var mayBeNull = false;

            var interval = recordingInterval;

            if (CurrentAction != ActionMode.STOPPED && action == CurrentAction)
            {
                // if action has not changed sinds last update
                if (CurrentMode == ViewMode.REPLAY)
                {
                    interval = playingInterval;
                }
                // execute current recorder action
                ExecRecorderAction(isReset);

                //float updateEndingTime = Time.realtimeSinceStartup;
                var elapsed = Time.realtimeSinceStartup - updateStartingTime;
                if (elapsed < interval)
                {
                    // if updating didn't take longer than the current interval
                    //substract surplus during more than one frame cycle to come to zero surplus

                    var surplusToEliminate = 0.0f;
                    if (surplus > 0.0f)
                    {
                        //if there is interval surplus..
                        //..it has to be eliminated
                        surplusToEliminate = interval - elapsed;

                        if (surplusToEliminate > surplus)
                        {
                            surplusToEliminate = surplus;
                        }
                    }
                    //determine interval to next update
                    interval -= elapsed - surplusToEliminate;

                    mayBeNull = true;
                    if (surplusToEliminate > 0.0f)
                    {
                        surplus -= surplusToEliminate;

                        if (surplus < 0.0f)
                        {
                            surplus = 0.0f;
                        }
                    }
                }
                else
                {
                    //if updating took longer than the interval
                    surplus += elapsed - interval; //..add to surplus

                    //update immediately
                    interval = 0.0f;
                    mayBeNull = true;
                }

                /*timeelapsed += elapsed + surplus; //<-- uncomment to make timeelapsed global
                print( "timeelapsed: " + timeelapsed );*/
                //timeelapsed = elapsed; //<-- uncomment to make timeelapsed only about this cycle


                if ((recorderPosition + recorderPositionStep > -1 ||
                     recorderPosition + recorderPositionStep < maxPosition) && continueCallingUpdate)

                    //if in the "middle of something"
                    //should be the only place where to increase recorderPosition
                {
                    recorderPosition += recorderPositionStep;
                }
                else
                {
                    Stop(); //stop on finishing an action
                }
                //call back process
                if (OnEzReplayProcess != null)
                {
                    OnEzReplayProcess((float) recorderPosition / maxPosition);
                }
                if ((interval > 0.0f || mayBeNull) && CurrentAction != ActionMode.PAUSED
                                                   && CurrentAction != ActionMode.STOPPED && continueCallingUpdate)
                    //if another update can be done
                {
                    StartCoroutine(WaitForNewUpdate(interval, action)); //don't ignore surplus
                }
                //StartCoroutine(waitForNewUpdate(playingInterval,timeelapsed,action)); //ignore surplus
            }
        }

        //there is an interval to wait for before the update will be done
        protected IEnumerator WaitForNewUpdate( float delay, ActionMode action )
        {
            yield return new WaitForSeconds(delay);

            /* if (currentAction != ActionMode.STOPPED)
                 timeelapsed += delay;*/
            //print("timeelapsed: "+timeelapsed);
            if (continueCallingUpdate)
            {
                Instance.UpdateRecording(action);
            }
        }

        #endregion

        #region 模块化接口实现

        private EzReplayProxy mEzProxy;

        public override void OnSingletonInit()
        {
            //CachePath = FileUtility.GetTempPathForWindows("ezCache/");
            //FileUtility.DeleteFolder(CachePath);
            mEzProxy = new EzReplayProxy(this);
            Facade.Instance.RegisterProxy(mEzProxy);
        }

        public override string ModuleName => typeof(EZReplayManager).FullName;

        public override int Priority => -11;

        public int SpeedSliderValue
        {
            get => speedSliderValue;

            set
            {
                speedSliderValue = Mathf.Clamp(value, minSpeedSliderValue, maxSpeedSliderValue);
                SetReplaySpeed(speedSliderValue);
            }
        }

        public string CachePath { get; set; }


        public override void Shutdown()
        {
            ClearCache();

            if (gOs2propMappings != null)
            {
                gOs2propMappings.Clear();
            }
            callbacksToExecute.Clear();
            componentsAndScriptsToKeepAtReplay.Clear();
            instanceIDtoGO.Clear();

            //Facade.Instance.RemoveProxy(EzReplayProxy.NAME);
            //mEzProxy = null;

            Destroy(gameObject);
            base.Shutdown();

            //mWatch.Stop();
            //mWatch = null;
        }

        #endregion

        #region 本地化读取

        public void SaveToFile( string filename )
        {
            //string _path = Application.dataPath.Substring( 0, Application.dataPath.LastIndexOf( "/" ) ) + LocalRecordpath;

            //string file = string.Format( "{0}/{1}", Path.GetDirectoryName( _path ), filename );
            var o2pMappingListW = new Object2PropertiesMappingListWrapper();
            o2pMappingListW.maxPosition = maxPosition;
            o2pMappingListW.begineTime = RecordBeginTime;
            o2pMappingListW.endTime = RecordEndTime;
            foreach (var entry in gOs2propMappings)
            {
#if ThreadCache
                entry.Value.ClearTrack();
#endif
                o2pMappingListW.AddMapping(entry.Value);
            }
            o2pMappingListW.recordingInterval = recordingInterval;

            /*if (mEzProxy != null)
            {
                o2pMappingListW.roomInfo = mEzProxy.GetCacheRoomInfo();
                o2pMappingListW.messageInfos = mEzProxy.GetCacheMessageInfo();
                o2pMappingListW.pathInfos = mEzProxy.GetCachePathInfo();

            }*/
            var data = SerializeObject(o2pMappingListW);
            if (data == null)
            {
                return;
            }
            Stream fileStream = File.Open(filename, FileMode.Create);
            fileStream.Write(data, 0, data.Length);
            fileStream.Close();
        }

        /// <summary>
        /// 保存录制流
        /// </summary>
        /// <returns></returns>
        public void SaveToStream( MessageID msgId, string guid )
        {
            Stop();
            //ClearCache();
            byte[] ret = { };
            var fileName = Instance.GetFileName();

            // access:显示加载界面
            //Facade.Instance.SendNotification((ushort)ConstNotification.OpenForm, ConstValue.c_sLoadingForm);

            ThreadUtils.RunThread(
                delegate { ret = InternalSaveToStream(); },
                delegate
                {
                    if (ret.Length <= 0)
                    {
                        SsitDebug.Error("回放序列化失败");
                        return;
                    }
                    //上传
                    SsitApplication.Instance.AddWebRequestTask(new WebRequestInfo());

                    /*EZReplayManager.Instance.StartCoroutine(NetWorkFileTools.UpLoadFile(ret, fileName, msgId, guid,
                        delegate ()
                        {
                            //access:界面接入
                            //Facade.Instance.SendNotification(ConstNotification.c_sCloseForm, ConstValue.c_sLoadingForm);
                            Facade.Instance.SendNotification((ushort)ConstNotification.ShowMessageInfo, "本次演练回放上传成功");
                            EZReplayManager.Instance.Exit();
                            Facade.Instance.SendNotification((ushort)EnGlobalEvent.Exit);

                        }));*/
                }
            );
        }

        public byte[] InternalSaveToStream()
        {
            var o2pMappingListW = new Object2PropertiesMappingListWrapper();
            o2pMappingListW.maxPosition = maxPosition;
            o2pMappingListW.begineTime = RecordBeginTime;
            o2pMappingListW.endTime = RecordEndTime;
            foreach (var entry in gOs2propMappings)
            {
#if ThreadCache
                entry.Value.ClearTrack();
#endif
                o2pMappingListW.AddMapping(entry.Value);
            }
            o2pMappingListW.recordingInterval = recordingInterval;

            /*if (mEzProxy != null)
            {
                o2pMappingListW.roomInfo = mEzProxy.GetCacheRoomInfo();
                o2pMappingListW.messageInfos = mEzProxy.GetCacheMessageInfo();
                o2pMappingListW.pathInfos = mEzProxy.GetCachePathInfo();
            }*/
            return SerializeObject(o2pMappingListW);
        }

        public void LoadFromFile( string filename )
        {
            if (!PrepareLoading())
            {
                return;
            }
            var data = Unity.FileUtils.ReadFileBytes(filename);
            FinishLoading(DeSerializeObject(data));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void loadFromStream( byte[] data )
        {
            if (!PrepareLoading())
            {
                return;
            }

            FinishLoading(DeSerializeObject(data));
        }

        private bool PrepareLoading()
        {
            Stop();
            return true;
        }

        private void FinishLoading( Object2PropertiesMappingListWrapper reSerialized )
        {
            gOs2propMappings.Clear();
            //maxPosition = 0;
            RecordBeginTime = reSerialized.begineTime;
            RecordEndTime = reSerialized.endTime;
            maxPosition = reSerialized.maxPosition;
            recorderPosition = 0;
            recordingInterval =
                reSerialized.recordingInterval; //if you load a replay with a different recording interval, 
            //you have to reset it to the earlier value afterwards YOURSELF!

            if (reSerialized.EZR_VERSION != EZR_VERSION)
            {
                if (showWarnings)
                {
                    SsitDebug.Warning(
                        "EZReplayManager WARNING: The EZR version with which the file has been created differs from your version of the EZReplayManager. This can cause unintended behaviour.");
                }
            }
            /*rocedureManager.Instance.GetProcedure<ProcedureReplay>((int)ENProcedureType.ProcedureReplay).SubscribeEvent((int)ENProcedureStatu.SceneLoaded,
                (IProcedureManager proceduremanager, object sender, object userdata) =>
                {
                    StartCoroutine(OnSerializedLoading(reSerialized));
                });*/

            CurrentMode = ViewMode.REPLAY;
            Facade.Instance.SendNotification((ushort) EnEzReplayEvent.Init, reSerialized);

            //switchModeTo(ViewMode.REPLAY); //happens in play:
            //play (0);
        }

        private IEnumerator OnSerializedLoading( Object2PropertiesMappingListWrapper reSerialized )
        {
            var checkAllCount = 0;
            var curCount = 0;
            foreach (var entry in reSerialized.object2PropertiesMappings)
            {
                if (entry.IsParent)
                {
                    checkAllCount++;
                    entry.PrepareObjectForReplay(() =>
                    {
                        var goClone = entry.CloneEntityObj;
                        if (goClone == null)
                        {
                            curCount++;
                            return;
                        }
                        if (gOs2propMappings.ContainsKey(goClone))
                        {
                            SsitDebug.Error("对象记录重复异常" + goClone.name);
                        }
                        else
                        {
                            gOs2propMappings.Add(goClone, entry);
                        }

                        curCount++;
                    });

                    //foreach (KeyValuePair<int, SavedState> stateEntry in entry.savedStates)
                    //{
                    //    if (stateEntry.Key > maxPosition)
                    //        maxPosition = stateEntry.Key;
                    //}
                }
            }

            while (curCount != checkAllCount)
            {
                yield return null;
            }

            checkAllCount = 0;
            var subCount = 0;


            foreach (var entry in reSerialized.object2PropertiesMappings)
            {
                if (!entry.IsParent)
                {
                    checkAllCount++;
                    entry.PrepareObjectForReplay(() =>
                    {
                        var goClone = entry.CloneEntityObj;
                        gOs2propMappings.Add(goClone, entry);
                        subCount++;
                    });
                }
            }

            //foreach (KeyValuePair<int, SavedState> stateEntry in entry.savedStates)
            //{
            //    if (stateEntry.Key > maxPosition)
            //        maxPosition = stateEntry.Key;
            //}

            while (subCount != checkAllCount)
            {
                yield return null;
            }

            Instance.Play();
        }

        #endregion


        #region 缓存线程

        private Thread cacheThread;

        public void InitCache()
        {
            cacheThread = new Thread(WriteCache);
            cacheThread.Start();
        }

        public void ClearCache()
        {
            if (cacheThread != null)
            {
                cacheThread.Abort();
                cacheThread = null;
            }
        }

        private void WriteCache()
        {
            while (true)
            {
                List<Object2PropertiesMapping> tempList = null;
                lock (gOs2propMappings)
                {
                    tempList = gOs2propMappings.Values.ToList();
                }
                foreach (var propretyWarp in tempList)
                {
                    propretyWarp.WriteTrack();
                }
                Thread.Sleep(2000);
            }
        }

        #endregion
    }
}