/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/16 14:06:22                     
*└──────────────────────────────────────────────────────────────┘
*/

using DG.Tweening;
using SsitEngine.Unity.Sound;
using SsitEngine.Unity.Timer;
using System;
using System.Collections.Generic;
using Framework.SceneObject;
using Framework.Utility;
using SsitEngine.Unity.Animation;
using UnityEditor;
using UnityEngine;
using Path = System.IO.Path;

namespace Framework.Event
{
    using EventEdMap = Dictionary<int, EventEdInfo>;

    [System.Serializable]
    public class EventEdInfo
    {
        public bool isFlodout;
        public bool isDisplayPath;
        public bool isRefreshPath;
        public EventDataInfo eventInfo;
        public GameObject obj;

        public EventEdInfo()
        {
            isFlodout = true;
            isDisplayPath = false;
            isRefreshPath = true;
            obj = null;
        }
    }

    public class GUIEventWindow : EditorWindow
    {
        private string[] mEventNames;

//        private ScriptScene mAgent;
        private string defaultFolder;

        /// <summary>
        /// 动画缓存文件
        /// </summary>
        private AnimInfo mCacheAnimInfo;

        private int mChildEventType;

        private SS_Animation anim;
        private bool isPlaying = false;

        public GUIEventWindow()
        {
            mEventEds = new EventEdMap();
            mEventNames = new string[7]
            {
                "添加事件/视角缓动",
                "添加事件/界面信息展示",
                "添加事件/音效播放",
                "添加事件/界面标签展示",
                "添加事件/物体频闪",
                "添加事件/消息发送",
                "添加事件/界面标签(高亮)",
            };
            mChildEventType = 0;
            mIsScroll = true;
            isPlaying = false;
            mOnAddFunc = InternalAddCallback;
            mOnDelFunc = InternalDelCallback;
        }

        [MenuItem("Tools/Internal Tools/事故动画脚本编辑工具 #%L")]
        public static void OpenWindow()
        {
            EditorWindow.GetWindow<GUIEventWindow>();
        }


        void OnEnable()
        {
            defaultFolder = System.IO.Path.Combine(Application.dataPath, "Resources/AnimAsset");
            //Debug.Log(defaultFolder);

            OnSelectionChange();
        }

        private void OnDestroy()
        {
            defaultFolder = null;
            mOnAddFunc = null;
            mOnDelFunc = null;
            mCacheAnimInfo = null;
        }

        /// <summary>
        /// 编辑器面板选择改变
        /// </summary>
        void OnSelectionChange()
        {
            //mAgent = null;
            if (Selection.activeGameObject != null)
            {
                Debug.Log(Selection.activeGameObject.name);

                //GameObject go = Selection.activeGameObject;
                //mAgent = go.GetComponent<SceneConfig>();
                //if (mAgent != null)
                //{
                //    if (mCacheAnimInfo == null)
                //    {
                //        mCacheAnimInfo = new AnimInfo();
                //    }
                //}
            }
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            DrawLoadAndSave();

            DrawRunTime();

            DrawAnimInfo();
            //if (Event.current.isMouse || Event.current.isKey)
            //{
            //    Repaint();
            //}
            EditorGUILayout.EndVertical();
        }

        private void DrawAnimInfo()
        {
            if (mCacheAnimInfo == null)
            {
                return;
            }

            EditorGUILayout.BeginVertical("Box");

            GUI.color = Color.green;
            if (GUILayout.Button("播放"))
            {
                if (anim != null)
                {
                    isPlaying = false;
                    anim.Stop();
                }
                anim = EventHelper.ReadAnimation(mCacheAnimInfo);
                Debug.Log("expression" + anim.mEventTree.Count);
                anim.Play(( TimerEventTask eve, float x, object data ) =>
                {
                    Debug.Log("Play");
                    isPlaying = false;
                }, null);
                isPlaying = true;
            }
            GUI.color = Color.red;

            if (GUILayout.Button("停止"))
            {
                if (anim != null)
                {
                    isPlaying = false;
                    anim.Stop();
                }
            }
            GUI.color = Color.white;

            if (isPlaying)
            {
                //GUI.enabled = false;
                EditorGUILayout.LabelField(string.Format("播放时间 {0} / {1}", anim.Time, anim.length));
                EditorGUILayout.Slider("播放时间轴", anim.Time, 0, anim.length, GUILayout.MaxWidth(300));

                //GUI.enabled = true;
            }


            draw(mCacheAnimInfo);

            EditorGUILayout.EndVertical();
        }

        private void DrawLoadAndSave()
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("本地文件读写");
            //GUI.enabled = mCacheAnimInfo != null;
            if (GUILayout.Button(new GUIContent("Save", "Save Data To xml"), GUILayout.Width(80)))
            {
                if (mCacheAnimInfo == null)
                {
                    return;
                }
                string filePath = EditorUtility.SaveFilePanel("Save", defaultFolder, "", "xml");
                if (!string.IsNullOrEmpty(filePath))
                {
                    ObjectToFileTools<AnimInfo>.SaveXML(filePath, mCacheAnimInfo);
                }
                Debug.Log(filePath);
            }
            if (GUILayout.Button(new GUIContent("Load", "Load Data From xml"), GUILayout.Width(80)))
            {
                string filePath = EditorUtility.OpenFilePanel("Open", defaultFolder, "xml");
                if (!string.IsNullOrEmpty(filePath))
                {
                    var info = ObjectToFileTools<AnimInfo>.ReadXml(filePath);
                    onSelectedChanged(info);
                }
            }

            if (GUILayout.Button(new GUIContent("Create", "Create Data From xml"), GUILayout.Width(80)))
            {
                var info = new AnimInfo()
                    {id = Guid.NewGuid().ToString("N"), length = 10f, eventList = new List<EventDataInfo>()};
                onSelectedChanged(info);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawRunTime()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("运行时读写");

            /*if (Facade.Instance.HasProxy(RoomProxy.NAME))
            {
                if (Facade.Instance.RetrieveProxy(RoomProxy.NAME) is RoomProxy roomProxy)
                {
                    var room = roomProxy.GetRoomInfo();

                    string guid = room.SchemeId;
                    EditorGUILayout.LabelField("预案id", guid);

                    if (GUILayout.Button(new GUIContent("Save", "Save Data To xml"), GUILayout.Width(80)))
                    {
                        if (mCacheAnimInfo == null)
                        {
                            return;
                        }
                        string filePath = EditorUtility.SaveFilePanel("Save", defaultFolder, guid, "xml");
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            ObjectToFileTools<AnimInfo>.SaveXML(filePath, mCacheAnimInfo);

                        }
                        Debug.Log(filePath);
                    }
                    if (GUILayout.Button(new GUIContent("Load", "Load Data From xml"), GUILayout.Width(80)))
                    {
                        string filePath = EditorUtility.OpenFilePanel("Open", defaultFolder, "xml");
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            var info = ObjectToFileTools<AnimInfo>.ReadXml(filePath);
                            onSelectedChanged(info);

                        }
                    }

                    if (GUILayout.Button(new GUIContent("Create", "Create Data From xml"), GUILayout.Width(80)))
                    {
                        var info = new AnimInfo() { id = Guid.NewGuid().ToString("N"), length = 10f, eventList = new List<EventDataInfo>() };
                        onSelectedChanged(info);

                    }
                }
            }*/
            EditorGUILayout.EndVertical();
        }

        #region 动画编辑

        private EventEdMap mEventEds;

        private EventDataInfo mSelected;
        private EventDataInfo mDeleted;


        public delegate void OnAddFunc( EventDataInfo eve );

        public delegate void OnDelFunc( EventDataInfo eve );

        private OnAddFunc mOnAddFunc;
        private OnDelFunc mOnDelFunc;


        private bool mIsScroll = false;
        public Vector2 mScrollPosition;

        private void onSelectedChanged( AnimInfo info )
        {
            //mSelected = e;
            //if (mSelected == null)
            //    return;

            //if (string.IsNullOrEmpty(name))
            //    return;

            mEventEds.Clear();
            mSelected = null;
            mCacheAnimInfo = info;
        }

        private void InternalDelCallback( EventDataInfo eve )
        {
            if (mCacheAnimInfo == null)
                return;
            if (eve.parentId != -1)
            {
                mSelected = mEventEds[eve.parentId].eventInfo;
            }
            else
            {
                mSelected = null;
            }

            if (mSelected != null)
            {
                if (mSelected.startEvents != null)
                    mSelected.startEvents.Remove(eve);

                if (mSelected.endEvents != null)
                    mSelected.endEvents.Remove(eve);
            }
            else
            {
                mCacheAnimInfo.eventList.Remove(eve);
            }

            if (mEventEds.ContainsKey(eve.id))
            {
                mEventEds.Remove(eve.id);
            }

            mSelected = null;
        }

        private void InternalAddCallback( EventDataInfo eve )
        {
            if (mCacheAnimInfo == null)
                return;

            if (mSelected == null)
            {
                mCacheAnimInfo.eventList.Add(eve);
            }
            else
            {
                if (mChildEventType == 0)
                {
                    if (mSelected.startEvents == null)
                        mSelected.startEvents = new List<EventDataInfo>();
                    mSelected.startEvents.Add(eve);
                }
                else if (mChildEventType == 1)
                {
                    if (mSelected.endEvents == null)
                        mSelected.endEvents = new List<EventDataInfo>();
                    mSelected.endEvents.Add(eve);
                }
            }

            mSelected = null;
        }


        public void draw( AnimInfo animation, float offset = 0 )
        {
            UnityEngine.Event aEvent = UnityEngine.Event.current;
            switch (aEvent.type)
            {
                case UnityEngine.EventType.ContextClick:
                {
                    showRightMenu();
                    UnityEngine.Event.current.Use();
                    break;
                }
            }
            if (aEvent.alt)
            {
                if (aEvent.shift)
                {
                    foreach (var info in mEventEds.Values)
                    {
                        info.isFlodout = true;
                    }
                }
                else
                {
                    foreach (var info in mEventEds.Values)
                    {
                        info.isFlodout = false;
                    }
                }
                UnityEngine.Event.current.Use();
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(offset);
                float length = EditorGUILayout.FloatField("动画总长", animation.length);
                animation.length = length;
            }
            EditorGUILayout.EndHorizontal();

            mScrollPosition = GUILayout.BeginScrollView(mScrollPosition, new GUILayoutOption[0]);
            {
                for (int i = 0; i < animation.eventList.Count; ++i)
                {
                    EventDataInfo eve = animation.eventList[i];
                    eve.parentId = -1;
                    DrawChildEvent(eve, offset);
                }

                if (mDeleted != null && mOnDelFunc != null)
                {
                    mOnDelFunc(mDeleted);
                    mDeleted = null;
                }
            }

            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
        }

        private void DrawChildEvent( EventDataInfo eve, float offset )
        {
            EventEdInfo info = null;

            if (mEventEds.ContainsKey(eve.id))
            {
                info = mEventEds[eve.id];
            }
            else
            {
                info = new EventEdInfo() {eventInfo = eve};
                mEventEds.Add(eve.id, info);
            }

            switch (eve.type)
            {
                case EventType.EVE_TWEEN:
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(offset);

                        info.isFlodout = EditorGUILayout.Foldout(info.isFlodout, "相机缓动");
                    }
                    EditorGUILayout.EndHorizontal();

                    if (!info.isFlodout)
                        return;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(offset);
                    DrawTweenEvent((EventTweenInfo) eve, offset);
                    EditorGUILayout.EndHorizontal();

                    break;
                }
                case EventType.EVE_UIHINT:
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(offset);

                        info.isFlodout = EditorGUILayout.Foldout(info.isFlodout, "文本界面");
                    }
                    EditorGUILayout.EndHorizontal();

                    if (!info.isFlodout)
                        return;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(offset);
                    DrawUIContentEvent((EventUIContentInfo) eve, offset);

                    EditorGUILayout.EndHorizontal();
                    break;
                }
                case EventType.EVE_SOUND:
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(offset);

                        info.isFlodout = EditorGUILayout.Foldout(info.isFlodout, "音效");
                    }
                    EditorGUILayout.EndHorizontal();

                    if (!info.isFlodout)
                        return;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(offset);
                    DrawSoundEvent((EventSoundInfo) eve, offset);


                    EditorGUILayout.EndHorizontal();
                    break;
                }
                case EventType.EVE_UITIP:
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(offset);

                        info.isFlodout = EditorGUILayout.Foldout(info.isFlodout, "UI标签");
                    }
                    EditorGUILayout.EndHorizontal();

                    if (!info.isFlodout)
                        return;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(offset);
                    DrawUITipEvent((EventUITagInfo) eve, offset);

                    EditorGUILayout.EndHorizontal();
                    break;
                }
                case EventType.EVE_FLASH:
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(offset);

                        info.isFlodout = EditorGUILayout.Foldout(info.isFlodout, "高光");
                    }
                    EditorGUILayout.EndHorizontal();

                    if (!info.isFlodout)
                        return;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(offset);
                    DrawFlashEvent((EventFlashInfo) eve, offset);
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                case EventType.EVE_FLASHAdvance:
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(offset);

                        info.isFlodout = EditorGUILayout.Foldout(info.isFlodout, "高光复合标签");
                    }
                    EditorGUILayout.EndHorizontal();

                    if (!info.isFlodout)
                        return;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(offset);
                    DrawFlashAdvanceEvent((EventFlashAdvanceInfo) eve, offset);
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                case EventType.EVE_MESSAGE:
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(offset);

                        info.isFlodout = EditorGUILayout.Foldout(info.isFlodout, "MVC消息");
                    }
                    EditorGUILayout.EndHorizontal();

                    if (!info.isFlodout)
                        return;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(offset);
                    DrawMessageEvent((EventMessageInfo) eve, offset);
                    EditorGUILayout.EndHorizontal();
                    break;
                }
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(offset);
                if (GUILayout.Button("添加触发前事件", new GUILayoutOption[0]))
                {
                    mChildEventType = 0;
                    mSelected = eve;

                    showRightMenu();
                }
            }
            EditorGUILayout.EndHorizontal();

            List<EventDataInfo> list = eve.startEvents;
            if (list != null)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    list[i].parentId = eve.id;
                    DrawChildEvent(list[i], offset + 7);
                }
            }


            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(offset);
                if (GUILayout.Button("添加触发后事件", new GUILayoutOption[0]))
                {
                    mChildEventType = 1;
                    mSelected = eve;

                    showRightMenu();
                }
            }
            EditorGUILayout.EndHorizontal();

            list = eve.endEvents;
            if (list != null)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    list[i].parentId = eve.id;
                    DrawChildEvent(list[i], offset + 7);
                }
            }
        }

        private void DrawMessageEvent( EventMessageInfo e, float offset )
        {
            EditorGUILayout.BeginVertical();
            {
                e.name = EditorGUILayout.TextField("描述", e.name);
                float delay = EditorGUILayout.FloatField("播放时间(s)", e.timer);
                e.timer = delay;
                e.messageName = (EnEventMessage) EditorGUILayout.EnumPopup("消息名称", e.messageName);
                if (e.messageName == EnEventMessage.None)
                {
                    EditorGUILayout.HelpBox("消息名称为空", MessageType.Warning);
                }

                e.value = EditorGUILayout.TextField("消息参数", e.value);
                if (string.IsNullOrEmpty(e.value))
                {
                    EditorGUILayout.HelpBox("消息名称为空", MessageType.Warning);
                }

                GUI.color = Color.red;
                if (GUILayout.Button("删除", new GUILayoutOption[0]))
                {
                    mDeleted = e;
                }
                GUI.color = Color.white;
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawFlashEvent( EventFlashInfo e, float offset )
        {
            EditorGUILayout.BeginVertical();
            {
                e.name = EditorGUILayout.TextField("描述", e.name);

                float delay = EditorGUILayout.FloatField("播放时间(s)", e.timer);
                e.timer = delay;

                float duration = EditorGUILayout.FloatField("持续时长(s)", e.duration);
                e.duration = duration;

                float frequency = EditorGUILayout.FloatField("闪动频率", e.frequency);
                e.frequency = frequency;

                EditorGUILayout.HelpBox("闪动频率为零时进行高亮", MessageType.Info);

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                if (!string.IsNullOrEmpty(e.binderId))
                {
                    EditorGUILayout.TextField("绑定对象guid", e.binderId);
                }
                else if (!string.IsNullOrEmpty(e.BindName))
                {
                    EditorGUILayout.TextField("绑定对象名称", e.BindName);
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                GameObject bind = null;
                bind = EditorGUILayout.ObjectField("Select", bind, typeof(GameObject), true,
                    GUILayout.ExpandWidth(true)) as GameObject;

                if (GUI.changed)
                {
                    if (bind != null)
                    {
                        var baseObj = bind.GetComponent<BaseSceneInstance>();
                        if (baseObj)
                        {
                            e.binderId = baseObj.LinkObject.Guid;
                        }
                        else
                        {
                            e.BindName = bind.name;
                        }
                    }
                }
                if (string.IsNullOrEmpty(e.BindName) && string.IsNullOrEmpty(e.binderId))
                {
                    EditorGUILayout.HelpBox("资源路径无效 ", MessageType.Error);
                }

                Color cc = EditorGUILayout.ColorField("NormalColor", e.nomalColor);
                e.nomalColor = new Color(cc.r, cc.g, cc.b, 1);
                cc = EditorGUILayout.ColorField("LightColor", e.lightColor);
                e.lightColor = new Color(cc.r, cc.g, cc.b, 1);


                GUI.color = Color.red;
                if (GUILayout.Button("删除", new GUILayoutOption[0]))
                {
                    mDeleted = e;
                }
                GUI.color = Color.white;
            }
            EditorGUILayout.EndVertical();
        }


        private void DrawFlashAdvanceEvent( EventFlashAdvanceInfo e, float offset )
        {
            EditorGUILayout.BeginVertical();
            {
                e.name = EditorGUILayout.TextField("描述", e.name);

                float delay = EditorGUILayout.FloatField("播放时间(s)", e.timer);
                e.timer = delay;

                float duration = EditorGUILayout.FloatField("持续时长(s)", e.duration);
                e.duration = duration;

                float frequency = EditorGUILayout.FloatField("闪动频率", e.frequency);
                e.frequency = frequency;

                EditorGUILayout.HelpBox("闪动频率为零时进行高亮", MessageType.Info);

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                if (!string.IsNullOrEmpty(e.binderId))
                {
                    EditorGUILayout.TextField("绑定对象guid", e.binderId);
                }
                else if (!string.IsNullOrEmpty(e.BindName))
                {
                    EditorGUILayout.TextField("绑定对象名称", e.BindName);
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                GameObject bind = null;
                bind = EditorGUILayout.ObjectField("Select", bind, typeof(GameObject), true,
                    GUILayout.ExpandWidth(true)) as GameObject;

                if (GUI.changed)
                {
                    if (bind != null)
                    {
                        var baseObj = bind.GetComponent<BaseSceneInstance>();
                        if (baseObj)
                        {
                            e.binderId = baseObj.LinkObject.Guid;
                        }
                        else
                        {
                            e.BindName = bind.name;
                        }
                    }
                }

                if (string.IsNullOrEmpty(e.BindName) && string.IsNullOrEmpty(e.binderId))
                {
                    EditorGUILayout.HelpBox("资源路径无效 ", MessageType.Error);
                }

                Color cc = EditorGUILayout.ColorField("NormalColor", e.nomalColor);
                e.nomalColor = new Color(cc.r, cc.g, cc.b, 1);
                cc = EditorGUILayout.ColorField("LightColor", e.lightColor);
                e.lightColor = new Color(cc.r, cc.g, cc.b, 1);

                e.offset = EditorGUILayout.Vector3Field("偏移量", e.offset);
                e.content = EditorGUILayout.TextField("文本", e.content);

                GUI.color = Color.red;
                if (GUILayout.Button("删除", new GUILayoutOption[0]))
                {
                    mDeleted = e;
                }

                GUI.color = Color.white;

                EditorGUILayout.EndVertical();
            }
        }


        private void DrawUITipEvent( EventUITagInfo e, float offset )
        {
            EditorGUILayout.BeginVertical();
            {
                e.name = EditorGUILayout.TextField("描述", e.name);
                float delay = EditorGUILayout.FloatField("播放时间(s)", e.timer);
                e.timer = delay;

                float duration = EditorGUILayout.FloatField("持续时长", e.duration);
                e.duration = duration;

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                if (!string.IsNullOrEmpty(e.binderId))
                {
                    EditorGUILayout.TextField("绑定对象guid", e.binderId);
                }
                else if (!string.IsNullOrEmpty(e.BindName))
                {
                    EditorGUILayout.TextField("绑定对象名称", e.BindName);
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                GameObject bind = null;
                bind = EditorGUILayout.ObjectField("Select", bind, typeof(GameObject), true,
                    GUILayout.ExpandWidth(true)) as GameObject;

                if (GUI.changed)
                {
                    if (bind != null)
                    {
                        var baseObj = bind.GetComponent<BaseSceneInstance>();
                        if (baseObj)
                        {
                            e.binderId = baseObj.LinkObject.Guid;
                        }
                        else
                        {
                            e.BindName = bind.name;
                        }
                    }
                }
                if (string.IsNullOrEmpty(e.BindName) && string.IsNullOrEmpty(e.binderId))
                {
                    EditorGUILayout.HelpBox("资源路径无效 ", MessageType.Error);
                }

                e.offset = EditorGUILayout.Vector3Field("偏移量", e.offset);
                e.content = EditorGUILayout.TextField("文本", e.content);

                GUI.color = Color.red;
                if (GUILayout.Button("删除", new GUILayoutOption[0]))
                {
                    mDeleted = e;
                }
                GUI.color = Color.white;
            }
            EditorGUILayout.EndVertical();
        }


        private void DrawTweenEvent( EventTweenInfo e, float offset )
        {
            EditorGUILayout.BeginVertical();
            {
                e.name = EditorGUILayout.TextField("描述", e.name);
                float delay = EditorGUILayout.FloatField("播放时间(s)", e.timer);
                e.timer = delay;

                float duration = EditorGUILayout.FloatField("运动周期(s)", e.duration);
                e.duration = duration;

                EditorGUILayout.BeginHorizontal();
                e.tweenType = (Ease) EditorGUILayout.EnumPopup("运动模式", e.tweenType);
                e.duration = duration;

                if (GUILayout.Button("测试"))
                {
                    var eve = EventManager.Instance.CreateEvent(EventType.EVE_TWEEN, 1, e);
                    eve.Execute(data: null);

                    eve.SetOnEndFunc(( Ss_Event ee ) => { EventManager.Instance.DestroyEvent(eve); });
                }
                EditorGUILayout.EndHorizontal();

                DrawPath("路径点", e, offset);

                GUI.color = Color.red;
                if (GUILayout.Button("删除", new GUILayoutOption[0]))
                {
                    mDeleted = e;
                }
                GUI.color = Color.white;
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawUIContentEvent( EventUIContentInfo e, float offset )
        {
            EditorGUILayout.BeginVertical();
            {
                e.name = EditorGUILayout.TextField("描述", e.name);
                float delay = EditorGUILayout.FloatField("播放时间(s)", e.timer);
                e.timer = delay;

                string content = EditorGUILayout.TextField("文本内容", e.content, GUILayout.MinHeight(20));
                e.content = content;

                //var scroll = EditorGUILayout.BeginScrollView(scroll);

                //e.content = EditorGUILayout.TextArea(e.content, GUILayout.Height(position.height - 30));

                //EditorGUILayout.EndScrollView();

                bool state = EditorGUILayout.Toggle("状态", e.state);
                e.state = state;

                GUI.color = Color.red;
                if (GUILayout.Button("删除", new GUILayoutOption[0]))
                {
                    mDeleted = e;
                }
                GUI.color = Color.white;
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSoundEvent( EventSoundInfo eve, float offset )
        {
            EditorGUILayout.BeginVertical();
            {
                eve.name = EditorGUILayout.TextField("描述", eve.name);
                float delay = EditorGUILayout.FloatField("播放时间(s)", eve.timer);
                eve.timer = delay;

                eve.volume = EditorGUILayout.FloatField("音量(s)", eve.volume);

                eve.setting = (SoundDuckingSetting) EditorGUILayout.EnumPopup("声音抑制设置", eve.setting);

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                string clipName = EditorGUILayout.TextField("音效名称", eve.clipName);
                GUI.enabled = true;
                AudioClip clip = null;
                clip =
                    EditorGUILayout.ObjectField("Select", clip, typeof(AudioClip), true, GUILayout.ExpandWidth(true)) as
                        AudioClip;

                if (GUI.changed)
                {
                    if (clip != null)
                    {
                        string filePath = AssetDatabase.GetAssetPath(clip);
                        string filter = SoundManager.Instance.resourcesPath;
                        if (filePath.Contains(filter))
                        {
                            string split = filePath.Substring(filePath.LastIndexOf(filter) + filter.Length + 1);
                            split = split.Replace(Path.GetExtension(split), "");
                            Debug.Log("filePath split::" + split);
                            clipName = split;
                        }
                    }
                }

                eve.clipName = clipName;
                EditorGUILayout.EndHorizontal();


                if (string.IsNullOrEmpty(clipName))
                {
                    EditorGUILayout.HelpBox("资源路径无效 need special SoundManager.Instance.resourcesPath",
                        MessageType.Error);
                }

                GUI.color = Color.red;
                if (GUILayout.Button("删除", new GUILayoutOption[0]))
                {
                    mDeleted = eve;
                }
                GUI.color = Color.white;
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPath( string label, EventTweenInfo eventTweenInfo, float offset )
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField(label);

                Transform cam = Camera.main.transform;

                if (cam == null)
                {
                    EditorGUILayout.HelpBox("主相机不存在", MessageType.Error);
                    return;
                }

                if (eventTweenInfo.path != null)
                {
                    for (int i = 0; i < eventTweenInfo.path.Count; i++)
                    {
                        var pathInfo = eventTweenInfo.path[i];
                        DrawTrans(pathInfo, cam);
                    }
                }

                //if (eventTweenInfo.path != null && eventTweenInfo.path.Count > 0)
                //{
                //    GUI.enabled = false;
                //}

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("添加", GUILayout.MaxWidth(60)))
                    {
                        if (eventTweenInfo.path == null)
                        {
                            eventTweenInfo.path = new List<TrasInfo>();
                        }
                        eventTweenInfo.path.Add(new TrasInfo()
                            {postion = Vector3.zero, rotate = Quaternion.identity.QuaternionVec4()});
                    }

                    if (GUILayout.Button("删除", GUILayout.MaxWidth(60)) && eventTweenInfo.path != null &&
                        eventTweenInfo.path.Count > 0)
                    {
                        eventTweenInfo.path.RemoveAt(eventTweenInfo.path.Count - 1);
                    }
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawTrans( TrasInfo pathInfo, Transform cam )
        {
            EditorGUILayout.BeginHorizontal();
            {
                pathInfo.postion = EditorGUILayout.Vector3Field("位置", pathInfo.postion);
                pathInfo.rotate = EditorGUILayout.Vector3Field("旋转", pathInfo.rotate);

                if (GUILayout.Button("重新定位"))
                {
                    pathInfo.postion = cam.position;
                    pathInfo.rotate = cam.localEulerAngles;
                }

                if (GUILayout.Button("相机定位"))
                {
                    cam.position = pathInfo.postion;
                    cam.localEulerAngles = pathInfo.rotate;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion


        #region IntervalHelper

        /// <summary>
        /// 显示右键菜单
        /// </summary>
        public void showRightMenu()
        {
            Vector2 pos = UnityEngine.Event.current.mousePosition;
            if (mIsScroll)
                pos -= mScrollPosition;

            if (focusedWindow != null)
            {
                pos.x += focusedWindow.position.xMin;
                pos.y += focusedWindow.position.yMin;
            }

            if ((focusedWindow == null) || focusedWindow.position.Contains(pos))
            {
                GenerateContextMenu().ShowAsContext();
                UnityEngine.Event.current.Use();
            }
        }

        private GenericMenu GenerateContextMenu()
        {
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < mEventNames.Length; ++i)
                menu.AddItem(new GUIContent(string.Format("{0}", mEventNames[i])), false, onMenuFunc, i);

            return menu;
        }

        private void onMenuFunc( object userData )
        {
            if (mCacheAnimInfo == null)
            {
                return;
            }
            EventManager em = EventManager.Instance;

            EventDataInfo eve = null;
            int index = System.Convert.ToInt32(userData);
            int lastEventIndex = 0;
            EventHelper.GetAnimationLastNode(mCacheAnimInfo, ref lastEventIndex);
            lastEventIndex++;
            switch (index)
            {
                case 0:
                {
                    eve = em.CreateEventInfo(EventType.EVE_TWEEN, 1, lastEventIndex);
                    break;
                }
                case 1:
                {
                    eve = em.CreateEventInfo(EventType.EVE_UIHINT, 1, lastEventIndex);
                    break;
                }
                case 2:
                {
                    eve = em.CreateEventInfo(EventType.EVE_SOUND, 1, lastEventIndex);

                    break;
                }
                case 3:
                {
                    eve = em.CreateEventInfo(EventType.EVE_UITIP, 1, lastEventIndex);

                    break;
                }
                case 4:
                {
                    eve = em.CreateEventInfo(EventType.EVE_FLASH, 1, lastEventIndex);
                    break;
                }
                case 5:
                {
                    eve = em.CreateEventInfo(EventType.EVE_MESSAGE, 1, lastEventIndex);
                    break;
                }
                case 6:
                {
                    eve = em.CreateEventInfo(EventType.EVE_FLASHAdvance, 1, lastEventIndex);
                    break;
                }
            }
            mOnAddFunc(eve);
        }


        /// <summary>
        /// 说明框
        /// </summary>
        /// <param name="message"></param>
        void HelpLable( string message )
        {
            if (!mIsHelper)
            {
                return;
            }
            GUI.color = Color.yellow;
            GUILayout.Label(message, "label");
            GUI.color = Color.white;
        }

        #endregion

        #region Editor Helper Module

        private readonly Dictionary<EventDataInfo, Dictionary<string, object>> EditorPropertyDic =
            new Dictionary<EventDataInfo, Dictionary<string, object>>();

        public bool mIsHelper;

        public object GetEditorProperty( EventDataInfo eve, string fieldName, object defaultValue )
        {
            if (EditorPropertyDic.ContainsKey(eve))
            {
                if (!EditorPropertyDic[eve].ContainsKey(fieldName))
                {
                    EditorPropertyDic[eve].Add(fieldName, defaultValue);
                }
            }
            else
            {
                EditorPropertyDic.Add(eve, new Dictionary<string, object>());
                EditorPropertyDic[eve].Add(fieldName, defaultValue);
            }
            return EditorPropertyDic[eve][fieldName];
        }

        public void SetEditorProperty( EventDataInfo eve, string fieldName, object value )
        {
            if (EditorPropertyDic.ContainsKey(eve))
            {
                if (EditorPropertyDic[eve].ContainsKey(fieldName))
                {
                    EditorPropertyDic[eve][fieldName] = value;
                }
                else
                {
                    EditorPropertyDic[eve].Add(fieldName, value);
                }
            }
            else
            {
                EditorPropertyDic.Add(eve, new Dictionary<string, object>());
                EditorPropertyDic[eve].Add(fieldName, value);
            }
        }

        #endregion
    }
}