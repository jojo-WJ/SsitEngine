using SsitEngine.Unity.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Data
{
    /// <summary>
    /// 电话汇报配置信息
    /// </summary>[Serializable]
    public class TelReportDataInfo : DataInfo, ISerializationCallbackReceiver
    {
        //电话号码
        public string Telnumber;
        //对话信息id列表
        public List<TelreportDialogueInfo> DialogueList;

        //所有的通话内容
        public List<TelreportContentInfo> ContentList;

        public TelReportDataInfo()
        {
            DialogueList = new List<TelreportDialogueInfo>();
            ContentList = new List<TelreportContentInfo>();
        }

        public void AddDialogueInfo( TelreportDialogueInfo dialogueInfo)
        {
            DialogueList.Add(dialogueInfo);
        }
        public void DeleteDialogueInfo( string guid )
        {
            var index = DialogueList.FindIndex(x => x.guid == guid);
            DialogueList.RemoveAt(index);
        }

        public void UpdateContentInfo( string guid, string content)
        {
            var tmpContent = ContentList.Find(x => x.guid == guid);
            if (tmpContent != null)
            {
                tmpContent.content = content;
            }
        }

        public void AddContentInfo( TelreportContentInfo contentInfo )
        {
            ContentList.Add(contentInfo);
        }

        public void DeleteContentInfo( string guid)
        {
            var index = ContentList.FindIndex(x => x.guid == guid);
            ContentList.RemoveAt(index);
        }

        public string GetContentByGuid(string guid)
        {
            return ContentList.Find(x => x.guid == guid)?.content;
        }

        public void ClearData()
        {
            Telnumber = string.Empty;
            DialogueList.Clear();
            ContentList.Clear();
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            
        }
    }

    /// <summary>
    /// 电话汇报一条对话信息
    /// </summary>
    [Serializable]
    public class TelreportDialogueInfo : ISerializationCallbackReceiver
    {
        public string guid;
        //当前通话内容Id
        public string m_curMessageId;
        //自动回复内容Id
        public string m_curAutoReplyId;
        //通话内容选项Id列表 
        public List<string> m_MessageIdList;


        public TelreportDialogueInfo()
        {
            m_MessageIdList = new List<string>();
        }


        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {

        }
    }

    /// <summary>
    /// 电话汇报通话内容
    /// </summary>
    [Serializable]
    public class TelreportContentInfo : ISerializationCallbackReceiver
    {
        public string guid;
        //通话内容
        public string content;

        public void OnAfterDeserialize()
        {

        }

        public void OnBeforeSerialize()
        {

        }
    }

    /// <summary>
    /// 发布任务配置信息
    /// </summary>
    public class PublishTaskDataInfo : DataInfo, ISerializationCallbackReceiver
    {
        //正确选项
        public string rightOptionGuid;
        //接收人员id列表
        public List<string> characterIdList;
        //交流-反馈内容列表
        public List<OptionDataInfo> OptionDataList;

        public PublishTaskDataInfo()
        {
            characterIdList = new List<string>();
            OptionDataList = new List<OptionDataInfo>();
        }

        public int GetRightOptionIndex()
        {
            return OptionDataList.FindIndex(x => x.guid == rightOptionGuid);
        }

        public void OnAfterDeserialize()
        {

        }

        public void OnBeforeSerialize()
        {

        }

        public void ClearData()
        {
            rightOptionGuid = string.Empty;
            characterIdList = null;
            OptionDataList = null;
        }
    }

    /// <summary>
    /// 交流-反馈信息
    /// </summary>
    [Serializable]
    public class OptionDataInfo : ISerializationCallbackReceiver
    {
        public string guid;
        //发送内容id
        public string sendContent;
        //反馈内容id
        public string replyContent;
        public void OnAfterDeserialize()
        {

        }

        public void OnBeforeSerialize()
        {

        }
    }

}

