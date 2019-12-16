/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/1 12:23:15                     
*└──────────────────────────────────────────────────────────────┘
*/
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Core.ReferencePool;
using SsitEngine.DebugLog;
using SsitEngine.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// 任务状态静态管理器
    /// </summary>
    public static class QuestHelper
    {
        #region Properties & Variables

        private static bool m_isLoadingGame = false;

        /// <summary>
        /// True when loading a game, in which case quests shouldn't run their state actions again.
        /// </summary>
        public static bool IsLoadingGame
        {
            get { return m_isLoadingGame; }
            set { m_isLoadingGame = value; }
        }

        #endregion

        #region Quest List Container Registry
        // Quest List Containers include questers (QuestJournal) and quest givers (QuestGiver).
        /// <summary>
        /// Registers a QuestListContainer for easy lookup.
        /// </summary>
        /// <param name="qlc">QuestListContainer to register.</param>
        public static void RegisterQuestListContainer( IdentifiableQuestListContainer qlc )
        {
            if (qlc == null || !Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return;
            //var id = StringField.GetStringValue(qlc.id);
            if (QuestManager.Instance.QuestListContainers.ContainsKey(qlc.id))
            {
                UnityEngine.Debug.LogWarning("Quest Machine: A QuestListContainer with id '" + qlc.id + "' is already registered. Can't register " + qlc, qlc);
            }
            else
            {
                QuestManager.Instance.QuestListContainers.Add(qlc.id, qlc);
            }
        }

        /// <summary>
        /// Unregisters a QuestListContainer.
        /// </summary>
        /// <param name="qlc">QuestListContainer to unregister.</param>
        public static void UnregisterQuestListContainer( IdentifiableQuestListContainer qlc )
        {
            if (qlc == null || !Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return;
            //var id = StringField.GetStringValue(qlc.id);
            if (QuestManager.Instance.QuestListContainers.ContainsKey(qlc.id))
            {
                QuestManager.Instance.QuestListContainers.Remove(qlc.id);
            }
        }

        /// <summary>
        /// Looks up a registered QuestListContainer.
        /// </summary>
        /// <param name="id">ID of the QuestListContainer.</param>
        public static IdentifiableQuestListContainer GetQuestListContainer( string id )
        {
            if (!Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return null;
            return (!string.IsNullOrEmpty(id) && QuestManager.Instance.QuestListContainers.ContainsKey(id)) ? QuestManager.Instance.QuestListContainers[id] : null;
        }


        /// <summary>
        /// Looks up a QuestJournal.
        /// </summary>
        /// <param name="id">ID of the QuestJournal's owner, or empty for any QuestJournal.</param>
        public static QuestJournal GetQuestJournal( string id )
        {
            foreach (var kvp in QuestManager.Instance.QuestListContainers)
            {
                var journal = kvp.Value as QuestJournal;
                if (journal != null && (string.IsNullOrEmpty(id) || string.Equals(id, kvp.Key))) return journal;
            }
            return UnityEngine.Object.FindObjectOfType<QuestJournal>();
        }

        /// <summary>
        /// Looks up the first registered QuestJournal.
        /// </summary>
        public static QuestJournal GetQuestJournal()
        {
            return GetQuestJournal(string.Empty);
        }

        public static void UnregisterAllQuestListContainer()
        {
            if (!Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return;
            QuestManager.Instance.QuestListContainers.Clear();
        }
        #endregion

        #region Quest Asset Registry

        private static string GetQuestKey( Quest quest )
        {
            if (quest == null) return null;
            return string.IsNullOrEmpty(quest.Id) ? quest.GetInstanceID().ToString() : quest.Id;
        }

        /// <summary>
        /// Looks up a quest asset by ID.
        /// </summary>
        /// <param name="id">Quest ID.</param>
        /// <returns>Quest with the matching ID, or null if none is found.</returns>
        public static Quest GetQuestAsset( string id )
        {
            if (!Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return null;
            var ins = QuestManager.Instance.QuestAssets;

            if (ins.ContainsKey(id))
            {
                return ins[id];
            }
            else
            {
                if (Engine.Debug)
                    SsitDebug.Warning("Quest Machine: A quest asset with ID '" + id + "' is not registered with Quest Machine.");
                return null;
            }
        }

        #endregion

        #region Quest Instance Registry

        /// <summary>
        /// Makes Quest Machine aware of a quest instance. Each quest ID may be associated with
        /// multiple instances of a quest if there are multiple questers. This allows Quest Machine
        /// to look it up by ID.
        /// </summary>
        /// <param name="quest">Quest instance to register.</param>
        public static void RegisterQuestInstance( Quest quest )
        {
            if (quest == null || !Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return;
            if (quest.IsAsset)
            {
                UnityEngine.Debug.LogWarning("Quest Machine: " + quest.name + " ('" + quest.Id + "') is an asset. Not registering it in Quest Machine's instance list.");
                return;
            }
            var key = GetQuestKey(quest);
            if (Engine.Debug)
                SsitDebug.Debug("Quest Machine: Registering quest instance '" + quest.Id + "'.", quest);
            if (string.IsNullOrEmpty(key) && Engine.Debug)
                SsitDebug.Warning("Quest Machine: " + quest.name + " ID is blank. This may affect registration with Quest Machine.", quest);

            var ins = QuestManager.Instance.QuestInstances;

            if (!ins.ContainsKey(key))
            {
                ins.Add(key, new List<Quest>());
            }
            ins[key].Insert(0, quest);
        }

        /// <summary>
        /// Unregisters a quest instance.
        /// </summary>
        /// <param name="quest">Quest instance to unregister.</param>
        public static void UnregisterQuestInstance( Quest quest )
        {
            if (quest == null || !Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return;
            var key = GetQuestKey(quest);
            var ins = QuestManager.Instance.QuestInstances;

            if (ins.ContainsKey(key))
            {
                ins[key].Remove(quest);
                if (ins[key].Count == 0)
                {
                    ins.Remove(key);
                }
            }
        }

        /// <summary>
        /// Looks up a quest instance by ID.
        /// </summary>
        /// <param name="id">Quest ID.</param>
        /// <param name="questerID">ID of quester assigned to quest, or blank or null for any quester.</param>
        /// <returns>Quest instance with the specified quest and quester ID, or null if none is found.</returns>
        public static Quest GetQuestInstance( string id, string questerID )
        {
            if (!Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return null;
            var anyQuester = string.IsNullOrEmpty(questerID);
            var ins = QuestManager.Instance.QuestInstances;

            if (ins.ContainsKey(id) && ins[id].Count > 0)
            {
                var list = ins[id];
                for (int i = 0; i < list.Count; i++)
                {
                    var questInstance = list[i];
                    if (questInstance == null) continue;
                    if (anyQuester || string.Equals(questerID, questInstance.QuesterId))
                    {
                        return questInstance;
                    }
                }
            }
            if (Engine.Debug)
                SsitDebug.Warning("Quest Machine: A quest instance with ID '" + id + "' is not registered with Quest Machine.");
            return null;
        }

        /// <summary>
        /// Looks up a quest instance by ID.
        /// </summary>
        /// <param name="id">Quest ID.</param>
        /// <returns>A quest instance matching the specified ID, or null if none is found.</returns>
        public static Quest GetQuestInstance( string id )
        {
            return GetQuestInstance(id, string.Empty);
        }

        /// <summary>
        /// Returns a dictionary of all registered quest instances, indexed by quest ID.
        /// </summary>
        public static Dictionary<string, List<Quest>> GetAllQuestInstances()
        {
            if (!Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return null;
            return QuestManager.Instance.QuestInstances;
        }

        #endregion

        #region Quest Counters

        /// <summary>
        /// Looks up a quest's counter.
        /// </summary>
        /// <param name="questID">The quest's ID.</param>
        /// <param name="counterName">The counter name.</param>
        /// <returns>A quest counter, or null if none matches the questID and counterName.</returns>
        public static QuestCounter GetQuestCounter( string questID, string counterName, string questerID = null )
        {
            var quest = GetQuestInstance(questID, questerID);
            if (quest == null)
            {
                if (UnityEngine.Debug.isDebugBuild) UnityEngine.Debug.LogWarning("Quest Machine: GetQuestCounter(" + questID + ", " + counterName + "): Couldn't find a quest with ID '" + questID + "'.");
                return null;
            }
            var counter = quest.GetCounter(counterName);
            if (counter == null)
            {
                if (UnityEngine.Debug.isDebugBuild) UnityEngine.Debug.LogWarning("Quest Machine: GetQuestCounter(" + questID + ", " + counterName + "): Couldn't find a counter named '" + counterName + "'.");
                return null;
            }
            return counter;
        }

        #endregion

        #region Quest States

        /// <summary>
        /// Looks up a quest's state.
        /// </summary>
        /// <param name="questID">The quest's ID.</param>
        /// <returns>The quest's state, or QuestState.Inactive if no quest with the specified ID has been registered.</returns>
        public static QuestState GetQuestState( string questID, string questerID = null )
        {
            var quest = GetQuestInstance(questID, questerID);
            if (quest == null)
            {
                if (UnityEngine.Debug.isDebugBuild) UnityEngine.Debug.LogWarning("Quest Machine: GetQuestState(" + questID + "): Couldn't find a quest with ID '" + questID + "'.");
                return QuestState.WaitingToStart;
            }
            return quest.GetState();
        }


        /// <summary>
        /// Sets a quest's state.
        /// </summary>
        /// <param name="questID">The quest's ID.</param>
        /// <param name="state">The quest's new state.</param>
        public static void SetQuestState( string questID, QuestState state, string questerID = null )
        {
            var quest = GetQuestInstance(questID, questerID);
            if (quest == null)
            {
                if (UnityEngine.Debug.isDebugBuild) UnityEngine.Debug.LogWarning("Quest Machine: SetQuestState(" + questID + ", " + state + "): Couldn't find a quest with ID '" + questID + "'.");
                return;
            }
            quest.SetState(state);
        }

        /// <summary>
        /// Looks up a quest node's state.
        /// </summary>
        /// <param name="questID">The quest's ID.</param>
        /// <param name="questNodeID">The quest node's ID.</param>
        /// <returns>The quest node's state, or QuestNodeState.Inactive if no quest or quest node with the specified IDs has been registered.</returns>
        public static QuestNodeState GetQuestNodeState( string questID, string questNodeID, string questerID = null )
        {
            var quest = GetQuestInstance(questID, questerID);
            if (quest == null)
            {
                if (UnityEngine.Debug.isDebugBuild) UnityEngine.Debug.LogWarning("Quest Machine: GetQuestNodeState(" + questID + ", " + questNodeID + "): Couldn't find a quest with ID '" + questID + "'.");
                return QuestNodeState.Inactive;
            }
            var node = quest.GetNode(questNodeID);
            if (node == null)
            {
                if (UnityEngine.Debug.isDebugBuild) UnityEngine.Debug.LogWarning("Quest Machine: GetQuestNodeState(" + questID + ", " + questNodeID + "): Quest doesn't have a node with ID '" + questNodeID + "'.");
                return QuestNodeState.Inactive;
            }
            return node.GetState();
        }

        /// <summary>
        /// Sets a quest node's state.
        /// </summary>
        /// <param name="questID">The quest's ID.</param>
        /// <param name="questNodeID">The quest node's ID.</param>
        /// <param name="state">The quest node's new state.</param>
        public static void SetQuestNodeState( string questID, string questNodeID, QuestNodeState state, string questerID = null )
        {
            var quest = GetQuestInstance(questID, questerID);
            if (quest == null)
            {
                if (Debug.isDebugBuild) Debug.LogWarning("Quest Machine: SetQuestNodeState(" + questID + ", " + questNodeID + ", " + state + "): Couldn't find a quest with ID '" + questID + "'.");
                return;
            }
            var node = quest.GetNode(questNodeID);
            if (node == null)
            {
                if (Debug.isDebugBuild) Debug.LogWarning("Quest Machine: SetQuestNodeState(" + questID + ", " + questNodeID + ", " + state + "): Quest doesn't have a node with ID '" + questNodeID + "'.");
                return;
            }
            node.SetState(state);
        }
        #endregion

        #region Parameters & IDs

        /// <summary>
        /// Gets the string value of a message argument. Some arguments such as quest node IDs can be
        /// passed as string or string. This utility method simplifies retrieval of the value.
        /// </summary>
        public static string ArgToString( object arg )
        {
            var argType = (arg != null) ? arg.GetType() : null;
            if (argType == typeof(string))
                return (string)arg;
            return (arg != null) ? arg.ToString() : string.Empty;
        }

        /// <summary>
        /// Gets the int value of a message argument.
        /// </summary>
        public static int ArgToInt( object arg )
        {
            return (arg != null && arg.GetType() == typeof(int)) ? (int)arg : -1;
        }

        public static bool IsRequiredID( object obj, string id )
        {
            return string.IsNullOrEmpty(id) || string.Equals(GetID(obj), id);
        }

        public static string GetID( object obj, string defaultID = null )
        {
            if (obj is GameObject)
            {
                var go = obj as GameObject;
                var questList = go.GetComponentInChildren<IdentifiableQuestListContainer>();
                if (questList != null) return questList.id;
                //var entity = go.GetComponentInChildren<QuestEntity>();
                //if (entity != null) return entity.Id;
                questList = go.GetComponentInParent<IdentifiableQuestListContainer>();
                if (questList != null) return questList.id;
                //entity = go.GetComponentInParent<QuestEntity>();
                //if (entity != null) return entity.Id;
            }
            else if (obj is IdentifiableQuestListContainer)
            {
                return (obj as IdentifiableQuestListContainer).id;
            }
            else if (obj != null && obj.GetType() == typeof(string))
            {
                return (string)obj;
            }
            return defaultID;
        }

        /// <summary>
        /// Gets the display name of an object. This is either a QuestEntity ID, a string,
        /// or the default value.
        /// </summary>
        public static string GetDisplayName( object obj, string defaultDisplayName = null )
        {
            if (obj is GameObject)
            {
                var go = obj as GameObject;
                var questList = go.GetComponentInChildren<IdentifiableQuestListContainer>();
                if (questList != null) return questList.displayName;
                //var entity = go.GetComponentInChildren<QuestEntity>();
                //if (entity != null) return entity.DisplayName;
            }
            else if (obj is IdentifiableQuestListContainer)
            {
                return (obj as IdentifiableQuestListContainer).displayName;
            }
            else if (obj is string)
            {
                return obj as string;
            }
            else if (obj != null && obj.GetType() == typeof(string))
            {
                return (string)obj;
            }
            return defaultDisplayName;
        }
        #endregion

        #region 消息集成


        /// <summary>
        /// 创建任务系统消息体
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="sender">发送者</param>
        /// <param name="target">目标者</param>
        /// <param name="parameter">参数</param>
        /// <param name="values">可变参数值</param>
        /// <returns>任务系统消息体</returns>
        public static QuestMessageArgs CreateQuestEvent( ushort msgId, object sender, object target, string parameter, params object[] values )
        {
            if (Engine.Instance.HasModule(typeof(QuestManager).FullName))
            {
                QuestMessageArgs msg = ReferencePool.Acquire<QuestMessageArgs>();
                msg.SetQuestMessageParm(msgId, sender, target, parameter, values);
                return msg;
            }
            return null;
        }

        /// <summary>
        /// 发送任务系统消息
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="sender">发送者</param>
        /// <param name="target">目标者</param>
        /// <param name="parameter">参数</param>
        /// <param name="values">可变参数值</param>
        public static void SendNotification( ushort msgId, object sender, object target, string parameter, params object[] values )
        {
            QuestMessageArgs msg = CreateQuestEvent(msgId, sender, target, parameter, values);
            if (msg == null && Engine.Debug)
            {
                SsitDebug.Warning("创建任务系统消息体失败");
            }
            Facade.Instance.SendNotification(msgId, msg);
        }

        /// <summary>
        /// 创建复合消息体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public static void SendCompositeMessage( object sender, string message )
        {
            if (string.IsNullOrEmpty(message)) return;
            var parameter = string.Empty;
            object value = null;
            if (message.Contains(":")) // Parameter?
            {
                var colonPos = message.IndexOf(':');
                parameter = message.Substring(colonPos + 1);
                message = message.Substring(0, colonPos);

                if (parameter.Contains(":")) // Value?
                {
                    colonPos = parameter.IndexOf(':');
                    var valueString = parameter.Substring(colonPos + 1);
                    parameter = parameter.Substring(0, colonPos);
                    int valueInt;
                    bool isNumeric = int.TryParse(valueString, out valueInt);
                    if (isNumeric)
                        value = valueInt;
                    else
                        value = valueString;
                }
            }

            ushort msgId = 0;
            ushort.TryParse(message, out msgId);
            if (msgId == 0)
                return;
            if (value == null)
                SendNotification(msgId, sender, null, parameter);
            else
                SendNotification(msgId, sender, null, parameter, value);
        }

        #endregion
    }
}
