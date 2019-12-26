/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：对象管理器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/27 11:31:40                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Data;
using Framework.Logic;
using Framework.Utility;
using SSIT.proto;
using SsitEngine;
using SsitEngine.Core.ReferencePool;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.Unity;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace Framework.SceneObject
{
    using ObjectList = Dictionary<string, BaseObject>;

    /// <summary>
    /// 对象管理器
    /// </summary>
    public class ObjectManager : ManagerBase<ObjectManager>
    {
        protected Creator mCreator;

        /// <summary>
        /// 物体列表
        /// </summary>
        protected Dictionary<EnFactoryType, ObjectList> mObjectMap;

        #region 初始化

        public override void OnSingletonInit()
        {
            base.OnSingletonInit();

            mObjectMap = new Dictionary<EnFactoryType, ObjectList>();

            mCreator = new Creator();
            mCreator.AddFactory(new SceneObjectFactory());
            mCreator.AddFactory(new PlayerFactory());
            mCreator.AddFactory(new NpcFactory());

            mCreator.AddFactory(new ItemFactory());
            mCreator.AddFactory(new SkillFactory());

            mCreator.AddFactory(new VehicleFactory());

            //mCreator.AddFactory(new MissileFactory());
            //mCreator.AddFactory(new NpcFactory());
        }

        #endregion

        #region Internal Members

        private void InternalProperyChange( BaseObject obj, EnPropertyId propertyId, string param, object data = null )
        {
            switch (propertyId)
            {
                case EnPropertyId.SwitchControl:
                    obj.OnChangeProperty(obj, propertyId, param, obj);
                    break;
                case EnPropertyId.SwitchIK:
                case EnPropertyId.OnSwitch:
                    obj.OnChangeProperty(obj, propertyId, param, obj.GetParent());
                    break;
                default:
                    obj.OnChangeProperty(obj, propertyId, param, data);
                    break;
            }
        }

        #endregion

        #region 模块接口实现

        /// <summary>
        /// 计时器模块优先级
        /// </summary>
        public override int Priority => (int) EnModuleType.ENMODULEENTITY;

        /// <summary>
        /// 计时器刷新
        /// </summary>
        /// <param name="elapsed">逻辑流逝时间</param>
        public override void OnUpdate( float elapsed )
        {
            //List<BaseObject> objs = new List<BaseObject>();

            var itor = mObjectMap.GetEnumerator();
            while (itor.MoveNext())
            {
                var objIt = itor.Current.Value.GetEnumerator();
                while (objIt.MoveNext())
                {
                    var obj = objIt.Current.Value;
                    /*if (expr)
                    {
                        //hack:统一卸载
                    }*/
                    // hit:here can justment this object is need auto destory,if need add to objs, else simple to onupdate.
                    switch (obj.LoadStatu)
                    {
                        case EnLoadStatu.Inited:
                            obj.OnUpdate(elapsed);
                            break;
                    }
                }
                objIt.Dispose();
            }
            itor.Dispose();

            /*var delIt = objs.GetEnumerator();
            while (delIt.MoveNext())
            {
                if (delIt.Current != null)
                {
                    DestroyObject(delIt.Current.Guid, delIt.Current.FactoryType);
                }
            }
            delIt.Dispose();*/
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            if (isShutdown)
                return;
            var itor = mObjectMap.GetEnumerator();
            while (itor.MoveNext())
            {
                var listIt = itor.Current.Value.GetEnumerator();
                while (listIt.MoveNext())
                {
                    listIt.Current.Value.Shutdown();
                    itor.Dispose();
                }
                listIt.Dispose();
            }
            mObjectMap.Clear();

            mCreator.Shutdown();
            mCreator = null;

            base.Shutdown();
            isShutdown = true;
        }

        #endregion

        #region 消息处理

        private void OnEnable()
        {
            m_msgList = new[]
            {
                (ushort) EnObjectEvent.SyncSceneObjInfoResult,
                (ushort) EnObjectEvent.SpawnSceneObjectResult,
                (ushort) EnObjectEvent.DestorySceneObjectResult,
                (ushort) EnObjectEvent.SpawnPlayer,
                (ushort) EnObjectEvent.SyncPlayerInfoResult,
                (ushort) EnObjectEvent.SyncSyncTransResult,
                (ushort) EnObjectEvent.RecordAccidentInfo,
                (ushort) EnGlobalEvent.FollowClient,
                (ushort) EnGlobalEvent.OnFollowClient,

                //动画事件
                (ushort) EnAnimationEvent.HideAccident,
                (ushort) EnAnimationEvent.OnHideAccident
            };
            RegisterMsg(m_msgList);
        }

        private void OnDisable()
        {
            UnRegisterMsg(m_msgList);
        }

        /// <inheritdoc />
        public override void HandleNotification( INotification notification )
        {
            switch (notification.Id)
            {
                case (ushort) EnObjectEvent.SyncSceneObjInfoResult:
                    OnSyncSceneObjInfoCallBack(notification);
                    break;
                case (ushort) EnObjectEvent.SpawnSceneObjectResult:
                    OnSpawnSceneObjectCallBack(notification);
                    break;
                case (ushort) EnObjectEvent.SyncPlayerInfoResult:
                    OnSyncPlayerInfoResultCallBack(notification);
                    break;
                case (ushort) EnObjectEvent.SyncAssignClientAuthorityResult:
                    OnAssignClientAuthorityResultCallBack(notification);
                    break;
                case (ushort) EnObjectEvent.DestorySceneObjectResult:
                    OnDestorySceneObjectCallBack(notification);
                    break;
                case (ushort) EnObjectEvent.SpawnPlayer:
                    OnSpawnPlayerCallBack(notification);
                    break;
                case (ushort) EnObjectEvent.RecordAccidentInfo:
                    OnRecordAccidentInfoCallBack(notification);
                    break;
                case (ushort) EnObjectEvent.SyncSyncTransResult:
                    OnSyncSyncTransResult(notification);
                    break;
                case (ushort) EnGlobalEvent.FollowClient:
                case (ushort) EnGlobalEvent.OnFollowClient:
                    OnFollowClient(notification);
                    break;
                /*动画脚本绑定事件*/
                case (ushort) EnAnimationEvent.HideAccident:
                    OnHideAccident(false);
                    break;
                case (ushort) EnAnimationEvent.OnHideAccident:
                    OnHideAccident(true);
                    break;
            }
        }


        /// <summary>
        /// 属性信息同步回调
        /// </summary>
        /// <param name="notification">消息体</param>
        private void OnSyncSceneObjInfoCallBack( INotification notification )
        {
            if (notification.Body is SCSyncSceneObjInfoResult result)
                for (var i = 0; i < result.sceneObjInfo.Count; i++)
                {
                    var syncInfos = result.sceneObjInfo[i];
                    var obj = Instance.GetObject(syncInfos.guid);
                    if (obj == null)
                    {
                        SsitDebug.Error("请求对象不存在");
                        return;
                    }

                    for (var j = 0; j < syncInfos.vars.Count; j++)
                    {
                        var syncInfo = syncInfos.vars[j];
                        InternalProperyChange(obj, (EnPropertyId) syncInfo.id, syncInfo.param);
                    }
                }
        }


        private void OnSyncSyncTransResult( INotification notification )
        {
            if (notification.Body is SCSyncTransResult result)
            {
                var syncInfos = result.transInfo;
                var obj = Instance.GetObject<Vehicle>(syncInfos.guid, EnFactoryType.VehicleFactory);
                if (obj == null)
                {
                    SsitDebug.Error("请求对象不存在");
                    return;
                }
                obj.OnChangeProperty(obj, EnPropertyId.Input, null, syncInfos);
            }
        }

        /// <summary>
        /// 卵生对象网络回调
        /// </summary>
        /// <param name="notification">消息体</param>
        private void OnSpawnSceneObjectCallBack( INotification notification )
        {
            var result = notification.Body as SCSpawnSceneObjectResult;
            var info = result?.spawnInfo;
            if (info == null)
            {
                SsitDebug.Error("mirror spawn msseage is exception");
                return;
            }
            SsitApplication.Instance.CreateObject(result.guid, info.dataId, InternalCreatedCallBack, info);
        }

        private void InternalCreatedCallBack( BaseObject obj, object render, object data )
        {
            SsitApplication.Instance.OnCreatedFunc(obj, render, data);
            //Facade.Instance.SendNotification((ushort)ConstNotification.FinishAddObject);
        }

        /// <summary>
        /// 权限请求回调
        /// </summary>
        /// <param name="notification"></param>
        private void OnAssignClientAuthorityResultCallBack( INotification notification )
        {
            var result = notification.Body as SCAssignClientAuthorityResult;
            if (result == null)
            {
                SsitDebug.Error("mirror spawn msseage is exception");
                return;
            }
            var info = result.guid;
            var obj = Instance.GetObject<Player>(info, EnFactoryType.PlayerFactory);
            if (obj == null)
            {
                SsitDebug.Error("请求对象不存在");
                return;
            }

            obj.OnChangeProperty(this, EnPropertyId.Authority, result.state.ToString());
        }

        /// <summary>
        /// 角色卵生回调
        /// </summary>
        /// <param name="notification"></param>
        private void OnSpawnPlayerCallBack( INotification notification )
        {
            if (notification is MvEventArgs args)
            {
                var obj = GetObject(args.StringValue);
                //
                if (obj == null) Debug.Log("OnSpawnPlayerCallBack" + args.StringValue);
                obj?.SetRepresent((GameObject) notification.Body);
            }
        }

        /// <summary>
        /// 物体销毁回调
        /// </summary>
        /// <param name="notification">消息体</param>
        private void OnDestorySceneObjectCallBack( INotification notification )
        {
            if (notification.Body is SCDestorySceneObjectResult result)
            {
                SsitApplication.Instance.DestoryObject(result.guid);
                //Facade.Instance.SendNotification((ushort)ConstNotification.FinishDeleteObject);
                return;
            }
            SsitDebug.Error("OnDestorySceneObjectCallBack messageboay is exception");
        }

        /// <summary>
        /// 角色信息同步回调
        /// </summary>
        /// <param name="notification">消息体</param>
        private void OnSyncPlayerInfoResultCallBack( INotification notification )
        {
            if (notification.Body is SCSyncPlayerInfoResult result)
            {
                var info = result.playerInfo;

                // 找到具体同步对象角色
                var player = GetObject<Player>(info.guid, EnFactoryType.PlayerFactory);

                //同步属性字段 there is not deal,to objectManager
                for (var i = 0; i < info.vars.Count; i++)
                {
                    var syncVar = info.vars[i];
                    player.OnChangeProperty(this, (EnPropertyId) syncVar.id, syncVar.param);
                }

                //同步装备列表
                void InternalAddEquipCallback( BaseObject obj, object render, object data )
                {
                    player.PutOnEquipment((Item) obj);
                }

                for (var i = 0; i < info.equips.Count; i++)
                {
                    var equip = info.equips[i];
                    if (string.IsNullOrEmpty(equip.guid))
                    {
                        // exception
                        SsitDebug.Error("OnSyncPlayerInfoResultCallBack message is exception");
                    }
                    else
                    {
                        var item = GetObject<Item>(equip.guid, EnFactoryType.ItemFactory);

                        switch ((EnItemState) equip.state)
                        {
                            case EnItemState.IS_Normal:
                                if (item != null)
                                    //this is server
                                    SsitApplication.Instance.CreateItem(item, InternalAddEquipCallback);
                                else
                                    SsitApplication.Instance.CreateItem(equip.guid, equip.dataId,
                                        InternalAddEquipCallback);
                                break;
                            case EnItemState.IS_Using:
                                player.UseEquip(item);
                                break;
                            case EnItemState.Is_Destroy:
                                player.PutOffEquipment(item);
                                break;
                        }
                    }
                }

                //同步交互列表
                for (var i = 0; i < info.userEquips.Count; i++)
                {
                    var equip = info.equips[i];
                    if (!string.IsNullOrEmpty(equip.guid))
                    {
                        var sceneObject = GetObject<SceneObject>(equip.guid, EnFactoryType.SceneObjectFactory);

                        if (sceneObject != null)
                            switch ((EnItemState) equip.state)
                            {
                                case EnItemState.IS_Normal:
                                    //player.AddInteractionObject(sceneObject);

                                    break;
                                case EnItemState.IS_Using:
                                    //player.RemoveInteractionObject(sceneObject);
                                    break;
                            }
                    }
                }

                //同步状态 there is not deal,to objectManager
                if (info.state != -1) player.OnChangeProperty(this, EnPropertyId.State, info.state.ToString());
            }
            //  SsitDebug.Error("OnDestorySceneObjectCallBack messageboay is exception");
        }

        /// <summary>
        /// 收到险情汇报的回调
        /// </summary>
        /// <param name="notification"></param>
        private void OnRecordAccidentInfoCallBack( INotification notification )
        {
        }

        /// <summary>
        /// 单位跟随
        /// </summary>
        /// <param name="notification"></param>
        private void OnFollowClient( INotification notification )
        {
            if (notification.Body is SCFollowClientResult result)
            {
                //var syncInfos = result.userID;
                var obj = Instance.GetObject<Player>(result.userID, EnFactoryType.PlayerFactory);
                if (obj == null)
                {
                    SsitDebug.Error($"请求对象不存在{result.userID}");
                    return;
                }
                obj.OnChangeProperty(obj, EnPropertyId.Follow, null, result);
            }
        }

        //事故隐藏回调
        private void OnHideAccident( bool p0 )
        {
            var acc = GetAccidentList();

            foreach (var baseObject in acc)
                if (baseObject != null)
                    baseObject.GetRepresent().SetActive(p0);
        }

        #endregion

        #region 数据查询接口

        /// <summary>
        /// 获取所有的物体列表
        /// </summary>
        /// <returns>返回物体的列表</returns>
        public Dictionary<EnFactoryType, ObjectList> getObjectMap()
        {
            return mObjectMap;
        }

        /// <summary>
        /// 获取指定类型的物体列表
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public ObjectList GetObjectCollection( EnFactoryType typeName )
        {
            ObjectList list = null;

            if (mObjectMap.ContainsKey(typeName))
            {
                list = mObjectMap[typeName];
            }
            else
            {
                list = new ObjectList();
                mObjectMap.Add(typeName, list);
            }
            return list;
        }

        /// <summary>
        /// 获取指定类型的物体列表
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public List<T> GetObjectCollection<T>( EnFactoryType typeName ) where T : BaseObject
        {
            ObjectList list = null;

            if (mObjectMap.ContainsKey(typeName))
            {
                list = mObjectMap[typeName];
            }
            else
            {
                list = new ObjectList();
                mObjectMap.Add(typeName, list);
            }
            return list.Values.ToList().ConvertAll(x => x as T);
        }

        /// <summary>
        /// 查找某一类型的物体的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetAttributesByItemType<T>( EnItemType itemType ) where T : BaseSceneInstance
        {
            var list = GetObjectCollection(EnFactoryType.SceneObjectFactory);
            var dataList = new List<T>();
            foreach (var item in list.Values)
                if (item.GetAttribute().ItemType == itemType)
                    dataList.Add(item.SceneInstance as T);

            return dataList;
        }


        /// <summary>
        /// 根据类型获取某一类的所有物体
        /// </summary>
        /// <param name="itemType">道具类型</param>
        /// <param name="itemSubType">子类型（如火/气/伤员/障碍物或者通用类型/应急力量/灭火器/消防炮</param>
        /// <returns></returns>
        public List<BaseObject> GetObjectsByType( int itemType, int itemSubType = -1 )
        {
            var list = new List<BaseObject>();
            var collection = GetObjectCollection<BaseObject>(EnFactoryType.SceneObjectFactory);
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (current == null) continue;
                if (current.GetAttribute().ItemType == (EnItemType) itemType)
                {
                    if (itemSubType == -1)
                    {
                        list.Add(current);
                        continue;
                    }
                    if (current.GetAttribute().ItemSubType == itemSubType) list.Add(current);
                }
            }
            enumerator.Dispose();
            return list;
        }

        /// <summary>
        /// 根据类型获取某一类的所有物体的属性
        /// </summary>
        /// <param name="itemType">道具类型</param>
        /// <param name="itemSubType">子类型（如火/气/伤员/障碍物或者通用类型/应急力量/灭火器/消防炮</param>
        /// <returns></returns>
        public List<BaseAtrribute> GetObjectsAttributeByType( int itemType, int itemSubType = -1 )
        {
            var sceneObjects = GetObjectsByType(itemType, itemSubType);
            return sceneObjects.ConvertAll(x => x.GetAttribute());
        }


        /// <summary>
        /// 获取物体
        /// </summary>
        /// <param name="id">指定数据的guid</param>
        /// <returns>返回指定物体</returns>
        public BaseObject GetObject( string id )
        {
            BaseObject obj = null;
            var itor = mObjectMap.GetEnumerator();
            while (itor.MoveNext())
                if (itor.Current.Value.ContainsKey(id))
                {
                    obj = itor.Current.Value[id];
                    itor.Dispose();
                    break;
                }
            return obj;
        }

        public T GetObject<T>( string id ) where T : BaseObject
        {
            BaseObject obj = null;
            var itor = mObjectMap.GetEnumerator();
            while (itor.MoveNext())
                if (itor.Current.Value.ContainsKey(id))
                {
                    obj = itor.Current.Value[id];
                    itor.Dispose();
                    break;
                }
            return obj as T;
        }

        public T GetObject<T>( string guid, EnFactoryType type ) where T : class
        {
            if (mObjectMap.ContainsKey(type))
            {
                var list = mObjectMap[type];
                if (list.ContainsKey(guid)) return list[guid] as T;
            }
            return null;
        }

        public BaseObject FindObjectByRepresent( GameObject represent )
        {
            BaseObject obj = null;
            var itor = mObjectMap.GetEnumerator();
            while (itor.MoveNext())
            {
                var factory = mCreator.GetFactory(itor.Current.Key);
                if (factory == null)
                    continue;
                var listIt = itor.Current.Value.GetEnumerator();
                while (listIt.MoveNext())
                {
                    if (listIt.Current.Value.GetRepresent().Equals(represent)) obj = listIt.Current.Value;
                    itor.Dispose();
                }
                listIt.Dispose();
            }
            return obj;
        }


        public int FindSameResourceCount( string resName )
        {
            var count = 0;
            var itor = mObjectMap.GetEnumerator();
            while (itor.MoveNext())
            {
                var factory = mCreator.GetFactory(itor.Current.Key);
                if (factory == null)
                    continue;
                var listIt = itor.Current.Value.GetEnumerator();
                while (listIt.MoveNext())
                {
                    if (listIt.Current.Value.GetResourceName() == resName) count++;
                    itor.Dispose();
                }
                listIt.Dispose();
            }
            return count;
        }

        #endregion

        #region 对象创建与销毁

        /// <summary>
        /// 创建物体
        /// </summary>
        /// <param name="guid">指定对象guid</param>
        /// <param name="type">指定对象类型</param>
        /// <param name="warpObj">指定实体对象</param>
        /// <returns>返回指定的对象</returns>
        public BaseObject CreateObject( string guid, EnFactoryType type )
        {
            var factory = mCreator.GetFactory(type);
            if (factory == null)
                return null;


            BaseObject obj = null;
            var list = GetObjectCollection(type);
            if (list.ContainsKey(guid))
            {
                SsitDebug.Debug(
                    TextUtils.Format("A object of id {0} already exists. ObjectManager::createObject", guid));
            }
            else
            {
                obj = (BaseObject) factory.CreateInstance(guid);
                list.Add(guid, obj);
            }


            return obj;
        }

        /// <summary>
        /// 创建物体
        /// </summary>
        /// <param name="type">指定对象类型</param>
        /// <returns>返回指定的对象</returns>
        public BaseObject CreateObject( EnFactoryType type )
        {
            return CreateObject(Guid.NewGuid().ToString("N"), type);
        }

        /// <summary>
        /// 销毁物体
        /// </summary>
        /// <param name="id">指定对象guid</param>
        public void DestroyObject( string id )
        {
            BaseObject obj = null;
            var factoryType = EnFactoryType.None;

            var itor = mObjectMap.GetEnumerator();
            while (itor.MoveNext())
                if (itor.Current.Value.ContainsKey(id))
                {
                    obj = itor.Current.Value[id];
                    factoryType = itor.Current.Key;
                    itor.Dispose();
                    break;
                }
            if (obj != null)
            {
                var list = GetObjectCollection(factoryType);
                var factory = mCreator.GetFactory(factoryType);
                list.Remove(id);
                factory.DestroyInstance(obj);
            }
        }

        /// <summary>
        /// 销毁物体
        /// </summary>
        /// <param name="id">指定对象guid</param>
        /// <param name="type">指定对象类型</param>
        public void DestroyObject( string id, EnFactoryType type )
        {
            var factory = mCreator.GetFactory(type);
            if (factory == null)
                return;

            var list = GetObjectCollection(type);
            if (list.ContainsKey(id))
            {
                var obj = list[id];
                list.Remove(id);
                factory.DestroyInstance(obj);
            }
        }

        public void DestoryAllObject()
        {
            var itor = mObjectMap.GetEnumerator();
            while (itor.MoveNext())
            {
                var factory = mCreator.GetFactory(itor.Current.Key);
                if (factory == null)
                    continue;
                var listIt = itor.Current.Value.GetEnumerator();
                while (listIt.MoveNext())
                {
                    factory.DestroyInstance(listIt.Current.Value);
                    itor.Dispose();
                }
                listIt.Dispose();
            }
            mObjectMap.Clear();
        }

        #endregion

        #region 内部消息封装

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="msgId">消息id</param>
        public void SendNotification( ushort msgId, BaseSceneInstance sender )
        {
            var args = ReferencePool.Acquire<MvEventArgs>();
            args.SetEventArgs(msgId);
            sender.HandleNotification(args);
            ReferencePool.Release(args);
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="body">消息体</param>
        public void SendNotification( ushort msgId, BaseSceneInstance sender, object body )
        {
            var args = ReferencePool.Acquire<MvEventArgs>();
            args.SetEventArgs(msgId, body);
            sender.HandleNotification(args);
            ReferencePool.Release(args);
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="body">消息体</param>
        /// <param name="values">可变消息参数组</param>
        public void SendNotification( ushort msgId, BaseSceneInstance sender, object body, params object[] values )
        {
            var args = ReferencePool.Acquire<MvEventArgs>();
            args.SetEventArgs(msgId, body, values);
            sender.HandleNotification(args);
            ReferencePool.Release(args);
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="body">消息体</param>
        public void Send2AllSceneObject( ushort msgId, object body )
        {
            var args = ReferencePool.Acquire<MvEventArgs>();
            args.SetEventArgs(msgId, body);

            if (mObjectMap.ContainsKey(EnFactoryType.SceneObjectFactory))
            {
                var list = mObjectMap[EnFactoryType.SceneObjectFactory];
                var enu = list.GetEnumerator();
                while (enu.MoveNext()) enu.Current.Value?.SceneInstance?.HandleNotification(args);
                enu.Dispose();
            }

            ReferencePool.Release(args);
        }

        #endregion

        #region 扩展查询

        public enum SearchRelation
        {
            CR_Team,
            CR_All,
            CR_CanControl,
            CR_Fire,
            CR_Gas,
            CR_AllAccitdent
        }

        public enum SearchAreaType
        {
            SA_None,
            SA_Point,
            SA_Fan,
            SA_Rect,
            SA_Chain
        }

        public List<BaseObject> GetSceneObject( BaseObject c, EnFactoryType factoryType, SearchRelation relation,
            SearchAreaType areaType, Vector2 val )
        {
            var list = new List<BaseObject>();
            if (areaType == SearchAreaType.SA_None || areaType == SearchAreaType.SA_Point)
                return list;
            var obj = c.GetRepresent();
            if (obj == null)
                return list;

            var direction = c.GetRepresent().transform.forward;
            var collection = GetObjectCollection<BaseObject>(factoryType);
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (current == null) continue;
                var go = current.GetRepresent();
                if (go == null) continue;
                // check has active or other condition
                if (go.activeSelf)
                {
                    switch (relation)
                    {
                        case SearchRelation.CR_Team:
                            if (current.SceneInstance.Type == EnObjectType.GamePlayer &&
                                c.SceneInstance.Type == EnObjectType.GamePlayer)
                            {
                                var player = c.GetAttribute() as PlayerAttribute;
                                var targetPlayer = current.GetAttribute() as PlayerAttribute;
                                if (player != null && targetPlayer != null && player.GroupId != targetPlayer.GroupId)
                                    continue;
                            }
                            break;
                        case SearchRelation.CR_CanControl:
                        {
                            if (!Utilitys.CheckAuthiroty(current.SceneInstance))
                            {
                                continue;
                            }
                        }
                            break;
                        case SearchRelation.CR_Fire:
                        {
                            if (!current.GetVisible()) continue;
                            if (current.SceneInstance.Type != EnObjectType.Fire) continue;
                        }
                            break;
                        case SearchRelation.CR_Gas:
                        {
                            if (!current.GetVisible()) continue;
                            if (current.SceneInstance.Type != EnObjectType.Gas) continue;
                        }
                            break;
                        case SearchRelation.CR_AllAccitdent:
                        {
                            if (!current.GetVisible()) continue;
                            if (current.SceneInstance.Type != EnObjectType.Gas ||
                                current.SceneInstance.Type != EnObjectType.Fire)
                                continue;
                        }
                            break;
                        case SearchRelation.CR_All:
                            break;
                    }
                    switch (areaType)
                    {
                        case SearchAreaType.SA_Fan:
                            var pos = obj.transform.position;
                            var targetPos = go.transform.position;
                            //获取两者间距的平方（计算更快不用根号）
                            var sqrDis = Vector3.SqrMagnitude(targetPos - pos);

                            if (val.y == 360)
                            {
                                if (sqrDis <= Mathf.Pow(val.x, 2)) list.Add(current);
                                break;
                            }
                            if (sqrDis <= Mathf.Pow(val.x, 2))
                            {
                                var num = Mathf.Acos(Vector3.Dot(pos.normalized, targetPos.normalized) * Mathf.Rad2Deg);
                                if (num >= direction.y - val.y * 0.5 && num <= direction.y + val.y * 0.5)
                                    list.Add(current);
                            }
                            break;
                        case SearchAreaType.SA_Rect:
                            //世界方向正前方旋转对象旋转角度 * 自身前方x轴的分量（绕Y轴旋转的角度）
                            var vector3_1 = obj.transform.rotation * Vector3.forward * direction.x;
                            var vector3_2 = go.transform.position - obj.transform.position;
                            //计算目标向量的cos余弦
                            var num1 = Vector3.Dot(vector3_1.normalized, vector3_2.normalized);
                            //计算宽度（半径）
                            var num2 = vector3_2.magnitude * num1;
                            //计算长度（半径）
                            var num3 = vector3_2.sqrMagnitude - num2 * num2;
                            if (num2 <= val.x && num3 <= Mathf.Pow(val.y, 2)) list.Add(current);
                            break;
                    }
                }
            }
            enumerator.Dispose();
            return list;
        }

        public List<BaseObject> GetSceneObject( GameObject c, EnFactoryType factoryType, SearchRelation relation,
            SearchAreaType areaType, Vector2 val )
        {
            var list = new List<BaseObject>();
            if (areaType == SearchAreaType.SA_None || areaType == SearchAreaType.SA_Point)
                return list;
            var obj = c;
            if (obj == null)
                return list;

            var direction = c.transform.forward;
            var collection = GetObjectCollection<BaseObject>(factoryType);
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (current == null) continue;
                var go = current.GetRepresent();
                if (go == null) continue;
                // check has active or other condition
                if (go.activeSelf)
                {
                    switch (relation)
                    {
                        case SearchRelation.CR_CanControl:
                        {
                            if (!current.SceneInstance.HasAuthority) continue;
                        }
                            break;
                        case SearchRelation.CR_Fire:
                        {
                            var attr = current.GetAttribute();
                            if (attr == null || !current.GetVisible()) continue;
                            if (attr.ItemType != EnItemType.ET_Accident ||
                                (ENAccidentType) attr.ItemSubType != ENAccidentType.AD_Fire) continue;
                        }
                            break;
                        case SearchRelation.CR_Gas:
                        {
                            var attr = current.GetAttribute();
                            if (attr == null || !current.GetVisible()) continue;
                            if (attr.ItemType != EnItemType.ET_Accident ||
                                (ENAccidentType) attr.ItemSubType != ENAccidentType.AD_Gas) continue;
                        }
                            break;
                        case SearchRelation.CR_AllAccitdent:
                        {
                            var attr = current.GetAttribute();

                            if (attr == null || !current.GetVisible()) continue;
                            if (attr.ItemType != EnItemType.ET_Accident
                                || (ENAccidentType) attr.ItemSubType != ENAccidentType.AD_Gas
                                || (ENAccidentType) attr.ItemSubType != ENAccidentType.AD_Fire)
                                continue;
                        }
                            break;
                        case SearchRelation.CR_All:
                            break;
                    }
                    switch (areaType)
                    {
                        case SearchAreaType.SA_Fan:
                            var pos = obj.transform.position;
                            var targetPos = go.transform.position;
                            //获取两者间距的平方（计算更快不用根号）
                            var sqrDis = Vector3.SqrMagnitude(targetPos - pos);

                            if (val.y == 360)
                            {
                                if (sqrDis <= Mathf.Pow(val.x, 2)) list.Add(current);
                                break;
                            }
                            if (sqrDis <= Mathf.Pow(val.x, 2))
                            {
                                var num = Mathf.Acos(Vector3.Dot(pos.normalized, targetPos.normalized) * Mathf.Rad2Deg);
                                if (num >= direction.y - val.y * 0.5 && num <= direction.y + val.y * 0.5)
                                    list.Add(current);
                            }
                            break;
                        case SearchAreaType.SA_Rect:
                            //世界方向正前方旋转对象旋转角度 * 自身前方x轴的分量
                            var vector3_1 = obj.transform.rotation * Vector3.forward * direction.x;
                            var vector3_2 = go.transform.position - obj.transform.position;
                            //计算目标向量的cos余弦
                            var num1 = Vector3.Dot(vector3_1.normalized, vector3_2.normalized);
                            //计算宽度（半径）
                            var num2 = vector3_2.magnitude * num1;
                            //计算长度（半径）
                            var num3 = vector3_2.sqrMagnitude - num2 * num2;
                            if (num2 <= val.x && num3 <= Mathf.Pow(val.y, 2))
                            {
                                list.Add(current);
                            }
                            break;
                    }
                }
            }
            enumerator.Dispose();
            return list;
        }


        public List<BaseObject> GetAccidentList()
        {
            var list = new List<BaseObject>();
            var collection = GetObjectCollection<BaseObject>(EnFactoryType.SceneObjectFactory);
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (current != null && current.GetAttribute().ItemType == EnItemType.ET_Accident) list.Add(current);
            }
            enumerator.Dispose();
            return list;
        }

        public List<BaseObject> GetPatientList()
        {
            var list = new List<BaseObject>();
            var collection = GetObjectCollection<BaseObject>(EnFactoryType.SceneObjectFactory);
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                var attr = current?.GetAttribute();
                if (attr != null && attr.ItemType == EnItemType.ET_Accident &&
                    attr.ItemSubType == (int) ENAccidentType.AD_Hurt) list.Add(current);
            }
            enumerator.Dispose();
            return list;
        }

        /// <summary>
        /// 获取无关人员的列表
        /// </summary>
        /// <returns></returns>
        public List<Player> GetNpcList()
        {
            var list = new List<Player>();
            var collection = GetObjectCollection<Player>(EnFactoryType.PlayerFactory);
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                BaseObject current = enumerator.Current;
                if (current != null && current.GetAttribute().GroupId == ConstValue.c_sDefaultNpcGroupName)
                    list.Add((Player) current);
            }
            enumerator.Dispose();
            return list;
        }

        //查找查找同组内有无协助担架的成员
        public Player GetOtherStrechPlayer( BaseObject curPlayer, List<int> checkSkills )
        {
            var collection = GetObjectCollection<Player>(EnFactoryType.PlayerFactory);
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                BaseObject current = enumerator.Current;
                if (current != null
                    && !current.Equals(curPlayer)
                    && current.GetVisible()
                    //&& current.GetAttribute().GroupId == curPlayer.GetAttribute().GroupId
                    && (current as Player)?.State == EN_CharacterActionState.EN_CHA_Stay)
                {
                    /*拥有担架工技能*/
                    //List<int> tempSkillIds = current.GetAttribute().SkillList;
                    var tempSkillIds = current.GetSkills().ConvertAll(x => { return x.GetSkillInfo().skillId; });

                    if (tempSkillIds.Intersect(checkSkills).Any()) return current as Player;
                }
            }
            enumerator.Dispose();
            return null;
        }


        public List<BaseAtrribute> GetAccidentAtrributeList()
        {
            var tt = GetAccidentList();
            return tt.ConvertAll(x => x.GetAttribute());
        }

        public List<BaseAtrribute> GetPatientAtrributeList()
        {
            var tt = GetPatientList();

            return tt.ConvertAll(x => x.GetAttribute());
        }

        #endregion
    }
}