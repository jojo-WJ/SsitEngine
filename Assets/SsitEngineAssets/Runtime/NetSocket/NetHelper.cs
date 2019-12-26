/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：网络辅助器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/6 17:31:37                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Framework.NetSocket;
using Mirror;
using SsitEngine;
using SsitEngine.DebugLog;
using Table;
using UnityEditor;

namespace Framework.Logic
{
    //文本记录表：数据库错误 = 1
    public class NetHelper
    {
        /*
        #region Delete

        /// <summary>
        /// 向服务器发送删除请求
        /// </summary>
        /// <param name="resultID">监听的返回结果的消息</param>
        /// <param name="target">删除的是哪张表的数据</param>
        /// <param name="_guid">删除对象的guid</param>
        /// <param name="deleteFlag">0为真删除，1逻辑删除,默认为真删除</param>
        /// <param name="isShowLoading"></param>
        public static void DeleteById2Server( MessageID resultID
            , SaveTarget target
            , string _guid
            , int deleteFlag = 0
            , bool isShowLoading = true )
        {
            var query = new CSCommonRemove
            {
                resultMessageID = (int) resultID,
                removeFlag = deleteFlag
            };

            var remove = new CommonRemove
            {
                target = $"{target}"
            };

            //0是逻辑真正删除，1为逻辑删除
            if (deleteFlag == 0)
            {
                var operate = new OperateById {guid = _guid};
                remove.param = JsonMapper.ToJson(operate);
            }
            else
            {
                var operate = new LogicRemoveById {guid = _guid, using_type = 0};
                remove.param = JsonMapper.ToJson(operate);
            }

            query.commonRemoveList.Add(remove);
            SendMesasge(query, MessageID.CSCommonRemove, isShowLoading);
        }
        
        /// <summary>
        /// 向服务器发送删除请求(删除多个)
        /// </summary>
        /// <param name="resultId"></param>
        /// <param name="target"></param>
        /// <param name="deleteFlag">0是逻辑真正删除，1为逻辑删除</param>
        /// <param name="isShowLoading"></param>
        /// <param name="guids"></param>
        public static void DeleteById2Server( MessageID resultId
            , SaveTarget target
            , int deleteFlag = 0
            , bool isShowLoading = true
            , params string[] guids
            )
        {
            if (guids == null || guids.Length == 0)
            {
                return;
            }
            
            var query = new CSCommonRemove
            {
                resultMessageID = (int) resultId,
                removeFlag = deleteFlag
            };

            for (int i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                var remove = new CommonRemove
                {
                    target = $"{target}"
                };

                //0是逻辑真正删除，1为逻辑删除
                if (deleteFlag == 0)
                {
                    var operate = new OperateById {guid = guid};
                    remove.param = JsonMapper.ToJson(operate);
                }
                else
                {
                    var operate = new LogicRemoveById {guid = guid, using_type = 0};
                    remove.param = JsonMapper.ToJson(operate);
                }
                query.commonRemoveList.Add(remove);
            }

            SendMesasge(query, MessageID.CSCommonRemove, isShowLoading);
        }

        #endregion

        #region System

        /// <summary>
        /// 系统错误日志处理
        /// </summary>
        /// <param name="errorCode">错误码</param>
        public static void PopSysErrorLog( SysErrorCode errorCode )
        {
            SsitDebug.Error(errorCode.ToString());
            //todo:PopTipHelper.PopTip();
        }

        #endregion

        #region Query

        /// <summary>
        /// Client to JavaServer
        /// </summary>
        /// <param name="packet">要发送的协议</param>
        /// <param name="msgId">协议ID</param>
        /// <param name="isShowLoading">是否加载Loading界面</param>
        public static void SendMesasge( object packet, MessageID msgId, bool isShowLoading = false )
        {
            var msgPackage = new MessagePackage(msgId, packet);
            Facade.Instance.SendNotification((ushort) EnSocketEvent.SendMessage, msgPackage, isShowLoading);
        }

        /// <summary>
        /// 向服务器发送请求消息
        /// </summary>
        /// <param name="resultID">返回结果的ID</param>
        /// <param name="_key">请求参数</param>
        /// <param name="isShowLoading">是否加载Loading界面</param>
        public static void Query2Server( MessageID resultID, QueryKey _key, bool isShowLoading = true )
        {
            var commonQuery = new CSCommonQuery
            {
                resultMessageID = (int) resultID
            };
            var query = new CommonQuery
            {
                key = _key.ToString()
            };
            commonQuery.commonQueryList.Add(query);
            SendMesasge(commonQuery, MessageID.CSCommonQuery, isShowLoading);
        }

        ///  <summary>
        /// 向服务器发送请求消息
        /// </summary>
        /// <param name="resultID">返回结果的ID</param>
        /// <param name="query">请求参数的封装</param>
        /// <param name="isShowLoading">是否加载Loading界面</param>
        public static void Query2Server( MessageID resultID, CommonQuery query
            , bool isShowLoading = true )
        {
            var commonQuery = new CSCommonQuery
            {
                resultMessageID = (int) resultID
            };
            commonQuery.commonQueryList.Add(query);
            SendMesasge(commonQuery, MessageID.CSCommonQuery, isShowLoading);
        }

        /// <summary>
        /// 根据guid向服务器请求数据
        /// </summary>
        /// <param name="resultID">返回结果的消息ID</param>
        /// <param name="queryKey">请求参数的封装</param>
        /// <param name="queryGuid">要请求的guid</param>
        /// <param name="isShowLoading">是否加载Loading界面</param>
        public static void QuerySchemeId2Server( MessageID resultID, QueryKey queryKey, string queryGuid,
            bool isShowLoading = true )
        {
            var operate = new OperateBySchemeId();
            operate.schemeId = queryGuid;
            var query = new CommonQuery
            {
                key = queryKey.ToString(),
                param = JsonMapper.ToJson(operate)
            };
            Query2Server(resultID, query, isShowLoading);
        }


        /// <summary>
        /// 根据输入的name向服务器查询
        /// </summary>
        /// <param name="resultID">返回的消息类型</param>
        /// <param name="key">查询的key</param>
        /// <param name="nameSearchCondition">查询条件</param>
        /// <param name="isShowLoading">是否显示loading界面</param>
        public static void QueryName2Server( MessageID resultID
            , QueryKey queryKey
            , string nameSearchCondition
            , bool isShowLoading = true )
        {
            var operate = new OperateByName
            {
                name = nameSearchCondition,
                _filterType = "name@2" //服务器必要的请求格式。
            };

            var query = new CommonQuery
            {
                key = queryKey.ToString(),
                param = JsonMapper.ToJson(operate)
            };
            Query2Server(resultID, query, isShowLoading);
        }

        /// <summary>
        /// 根据SceneId向服务器查询
        /// </summary>
        /// <param name="resultID"></param>
        /// <param name="queryKey"></param>
        /// <param name="sceneId"></param>
        /// <param name="isShowLoading"></param>
        public static void QuerySceneId2Server( MessageID resultID
            , List<QueryKey> queryKeyList
            , string sceneId
            , bool isShowLoading = true )
        {
            var operate = new OperateBySceneId
            {
                sceneGuid = sceneId
            };

            var commonQuery = new CSCommonQuery
            {
                resultMessageID = (int) resultID
            };

            for (var i = 0; i < queryKeyList.Count; i++)
            {
                var query = new CommonQuery
                {
                    key = queryKeyList[i].ToString(),
                    param = JsonMapper.ToJson(operate)
                };
                commonQuery.commonQueryList.Add(query);
            }
            SendMesasge(commonQuery, MessageID.CSCommonQuery, true);
        }


        /// <summary>
        /// 根据输入的组织机构向服务器请求对应的信息
        /// </summary>
        /// <param name="resultID">返回的结果ID</param>
        /// <param name="queryKey">请求关键字</param>
        /// <param name="organizeGuid">查询条件</param>
        /// <param name="isShowLoading">是否显示loading条</param>
        public static void QueryOrganizeGuid2Server( MessageID resultID
            , QueryKey queryKey
            , string organizeGuid
            , bool isShowLoading = true )
        {
            var operate = new OperateByOrganizeId
            {
                organizeId = organizeGuid
            };

            var query = new CommonQuery
            {
                key = queryKey.ToString(),
                param = JsonMapper.ToJson(operate)
            };
            Query2Server(resultID, query, isShowLoading);
        }

        /// <summary>
        /// 根据行业ID、公司索引、单位名称查寻场景
        /// </summary>
        /// <param name="resultID">返回的消息的ID</param>
        /// <param name="queryKey">要查的表</param>
        /// <param name="industryID">行业ID</param>
        /// <param name="companyID">公司索引</param>
        /// <param name="nameCondition">名称条件</param>
        /// <param name="isShowLoading">true</param>
        public static void QueryScenesByContidions( MessageID resultID
            , QueryKey queryKey
            , string industryID = ""
            , int companyID = -1
            , string nameCondition = ""
            , bool isShowLoading = true )
        {
            var operate = new OperateByNameAndIndustryIdAndCompanyId
            {
                name = nameCondition,
                industryId = industryID,
                companyId = companyID
            };
            operate._filterType =
                companyID == -1 ? "name@2;industryId@2;companyId@9" : "name@2;industryId@2;companyId@1";
            var query = new CommonQuery
            {
                key = queryKey.ToString(),
                param = JsonMapper.ToJson(operate)
            };
            Query2Server(resultID, query, isShowLoading);
        }

        /// <summary>
        /// 根据事故类别、事故类型、所属装置查寻事故
        /// </summary>
        /// <param name="resultID">返回的消息id</param>
        /// <param name="queryKey">查询的表</param>
        /// <param name="opKindID">事故类别 单人 协同</param>
        /// <param name="accidentType">事故类型</param>
        /// <param name="sceneID">所属装备/场景</param>
        /// <param name="nameCondition">事故名称关键字</param>
        /// <param name="isShowLoading">是否打开Loading</param>
        public static void QueryAccidentByContidions( MessageID resultID
            , QueryKey queryKey
            , int opKindID = -1
            , string accidentType = ""
            , string sceneID = ""
            , string nameCondition = ""
            , bool isShowLoading = true )
        {
            var operate = new OperateOfScheme
            {
                name = nameCondition,
                isSinglePlayer = opKindID,
                typeId = accidentType,
                sceneId = sceneID
            };
            operate._filterType = opKindID == -1
                ? "name@2;typeId@2;sceneId@2;isSinglePlayer@9"
                : "name@2;typeId@2;sceneId@2;isSinglePlayer@1";
            var query = new CommonQuery
            {
                key = queryKey.ToString(),
                param = JsonMapper.ToJson(operate)
            };
            Query2Server(resultID, query, isShowLoading);
        }


        /// <summary>
        /// 根据预案id查找对应的队伍相关的信息
        /// </summary>
        /// <param name="schemeID"></param>
        public static void QueryGroupInfo2Server(string schemeID,bool isShowLoading=true)
        {
            CSCommonQuery commonQuery = new CSCommonQuery
            {
                resultMessageID = (int)MessageID.SCGroupInfoListResult,
            };
            
            commonQuery.commonQueryList.Add(new CommonQuery
            {
                key=QueryKey.taskTypeList.ToString(), 
                param = LitJson.JsonMapper.ToJson(new OperateBySchemeId{ schemeId = schemeID})
            });
            
            commonQuery.commonQueryList.Add(new CommonQuery
            {
                key=QueryKey.taskInfoList.ToString(),
                param = LitJson.JsonMapper.ToJson(new OperateBySchemeId{ schemeId = schemeID})
            });
            
            commonQuery.commonQueryList.Add(new CommonQuery
            {
                key=QueryKey.charactorInfoList.ToString(),
                param=LitJson.JsonMapper.ToJson(new OperateBySchemeId{ schemeId=schemeID})
                
            });
            
            commonQuery.commonQueryList.Add(new CommonQuery
            {
                key=QueryKey.charactorTypeList.ToString(),
                param=LitJson.JsonMapper.ToJson(new OperateBySchemeId{schemeId = schemeID})
            });
            
            commonQuery.commonQueryList.Add(new CommonQuery
            {
                key=QueryKey.skillTypeList.ToString(),
                param=LitJson.JsonMapper.ToJson(new OperateBySchemeId{schemeId = schemeID})
            });

            commonQuery.commonQueryList.Add(new CommonQuery
            {
                key=QueryKey.skillList.ToString(),
                param=LitJson.JsonMapper.ToJson(new OperateBySchemeId{schemeId = schemeID})
            });
            
            commonQuery.commonQueryList.Add(new CommonQuery
            {
                key=QueryKey.groupInfoList.ToString(),
                param=LitJson.JsonMapper.ToJson(new OperateBySchemeId{schemeId = schemeID})
            });
            
            SendMesasge(commonQuery, MessageID.CSCommonQuery, isShowLoading);
        }

        /// <summary>
        /// 向服务器请求场景锁定状态
        /// </summary>
        public static void QuerySceneLockState2Server()
        {
            CSSceneEditLockStatList     query = new CSSceneEditLockStatList();
            SendMesasge(query,MessageID.CSSceneEditLockStatList,false);
        }
        
        /// <summary>
        /// 向服务器请求预案锁定状态
        /// </summary>
        public static void QuerySchemeLockState2Server()
        {
            CSSchemeEditLockStatList query=new CSSchemeEditLockStatList();
            SendMesasge(query,MessageID.CSSchemeEditLockStatList,false);
        }

        #endregion

        #region Change

        /// <summary>
        /// 向服务器请求添加一个队伍
        /// </summary>
        /// <param name="info"></param>
        /// <param name="isShowLoading"></param>
        public static void AddGroupInfo2Server(GroupInfo info,bool isShowLoading=true)
        {
            CSAddGroupInfo query = new CSAddGroupInfo();
            query.groupInfo = info;
            SendMesasge(query,MessageID.CSAddGroupInfo,isShowLoading);
            
        }


        public static void UpdateGroupInfo2Server(List<GroupInfo> groupInfos, bool isShowLoading = true)
        {
            if (groupInfos==null)
            {
                return;
            }
            CSUpdateGroupInfo query =new CSUpdateGroupInfo();
            query.groupInfo.AddRange(groupInfos);
            SendMesasge(query,MessageID.CSUpdateGroupInfo,isShowLoading);
        }

        /// <summary>
        /// 向服务器更新预案
        /// </summary>
        /// <param name="accidentInfo"></param>
        public static void UpdateAccidentInfo2Server(AccidentInfo accidentInfo)
        {
            CSUpdateAccident query = new CSUpdateAccident
            {
                 info=accidentInfo
            };
            SendMesasge(query,MessageID.CSUpdateAccident,true);
        }

        
        /// <summary>
        /// 保存链表数据
        /// </summary>
        /// <param name="resultId"></param>
        /// <param name="saveTarget"></param>
        /// <param name="isShowLoading"></param>
        /// <param name="para"></param>

        public static void CommonSave2Server(MessageID resultId, SaveTarget saveTarget,bool isShowLoading=true, params object[] para )
        {
            if (para==null||para.Length==0)
            {
                return;
            }
            
            var query  =new CSCommonSave(){resultMessageID=(int) resultId};
            foreach (var item in para)
            {
                var save = new CommonSave
                {
                    target = $"{saveTarget}",
                    param = LitJson.JsonMapper.ToJson(item)
                };
                query.commonSaveList.Add(save);
            }
            SendMesasge(query, MessageID.CSCommonSave, isShowLoading);
        }


        /// <summary>
        /// 向服务器发送请求/新增数据
        /// </summary>
        /// <param name="resultID">监听的返回结果的MessageID</param>
        /// <param name="target">发送到哪张表</param>
        /// <param name="jsonData">发送的数据</param>
        /// <param name="isShowLoading">是否显示loading窗口</param>
        public static void ChangeById2Server( MessageID resultID
            , SaveTarget target
            , string jsonData
            , bool isShowLoading = true )
        {
            var query = new CSCommonSave
            {
                resultMessageID = (int) resultID
            };
            var save = new CommonSave
            {
                target = $"{target}",
                param = jsonData
            };
            query.commonSaveList.Add(save);
            SendMesasge(query, MessageID.CSCommonSave, isShowLoading);
        }
        
       

        /// <summary>
        /// 向服务器发送请求/新增多条数据
        /// </summary>
        /// <param name="resultID">监听的返回结果的MessageID</param>
        /// <param name="target">发送到哪张表</param>
        /// <param name="jsonData">发送的数据</param>
        /// <param name="isShowLoading">是否显示loading窗口</param>
        public static void ChangeById2Server( MessageID resultID
            , SaveTarget target
            , List<string> jsonDataList
            , bool isShowLoading = true )
        {
            var query = new CSCommonSave
            {
                resultMessageID = (int) resultID
            };

            for (var i = 0; i < jsonDataList.Count; i++)
            {
                var save = new CommonSave
                {
                    target = $"{target}",
                    param = jsonDataList[i]
                };
                query.commonSaveList.Add(save);
            }

            SendMesasge(query, MessageID.CSCommonSave, isShowLoading);
        }
        
        /// <summary>
        /// 设置场景/预案的锁定/解锁状态
        /// </summary>
        /// <param name="lockState"></param>
        public static void SetLockState2Server(LockState lockState)
        {
            if (lockState==null)
            {
                return;
            }

            var query = new CSUpdateEditLockStat
            {
                guid = lockState.guid,
                stat = lockState.lockState,
                flag = lockState.lockFlag
            };
            SendMesasge(query,MessageID.CSUpdateEditLockStat,false);
        }

        #endregion
        */

        #region 上传路径

        //public readonly string RemotePath_Org_Icon = "Icon/Org";
        //public readonly string RemotePath_User_Icon = "Icon/User";

        /// <summary>
        /// 构建远程端文件路径
        /// </summary>
        /// <param name="fileTpye"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string BuildRomoteFilePath( EnGlobalValue fileTpye, params object[] value )
        {
            if (value == null)
                return DataGlobalValueProxy.GetGlobalValue(fileTpye);
            return TextUtils.Format(DataGlobalValueProxy.GetGlobalValue(fileTpye), value);
        }

        #endregion
        
        #region 通用

        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <returns></returns>s
        public static string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        /// <summary>
        /// 获取第一个可用的端口号
        /// </summary>
        /// <returns></returns>
        public int GetTwoAvailablePorts()
        {
            int MAX_PORT = 65500; //系统tcp/udp端口数最大是65535
            int BEGIN_PORT = ((TelepathyTransport) Transport.activeTransport).port + 1; //从这个端口开始检测

            for (int i = BEGIN_PORT; i < MAX_PORT - 1; i++)
            {
                if (PortIsAvailable(i) && PortIsAvailable(i + 1)) return i;
            }

            return -1;
        }

        /// <summary>
        /// 检查指定端口是否已用
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool PortIsAvailable( int port )
        {
            bool isAvailable = true;

            IList portUsed = GetAllPortUsed();

            foreach (int p in portUsed)
            {
                if (p == port)
                {
                    isAvailable = false;
                    break;
                }
            }

            return isAvailable;
        }

        /// <summary>
        /// 获取操作系统已用的端口号
        /// </summary>
        /// <returns></returns>
        static IList GetAllPortUsed()
        {
            //获取本地计算机的网络连接和通信统计数据的信息
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            //返回本地计算机上的所有Tcp监听程序
            IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();

            //返回本地计算机上的所有UDP监听程序
            IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();

            //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            IList allPorts = new ArrayList();
            foreach (IPEndPoint ep in ipsTCP) allPorts.Add(ep.Port);
            foreach (IPEndPoint ep in ipsUDP) allPorts.Add(ep.Port);
            foreach (TcpConnectionInformation conn in tcpConnInfoArray) allPorts.Add(conn.LocalEndPoint.Port);

            return allPorts;
        }

        #endregion
    }
}