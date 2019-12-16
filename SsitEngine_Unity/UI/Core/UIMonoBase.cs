/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：UIMonoBehaviour基类                                                    
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/04/29                             
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using SsitEngine.UI;
using SsitEngine.Unity.Msic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SsitEngine.Unity.UI
{
    /// <summary>
    ///     UIMonoBehaviour基类
    /// </summary>
    public class UIMonoBase : SsitMonoBase
    {
        /// <summary>
        ///     UIPanel控件映射表
        ///     Key: 控件在Hierarchy面板中的名称
        ///     Value: 控件的GameObject
        /// </summary>
        private Dictionary<string, GameObject> m_widghtDic;

        /// <summary>
        ///     RectTrans
        /// </summary>
        public RectTransform RectTrans { get; private set; }

        /// <summary>
        ///     ParentTrans
        /// </summary>
        public RectTransform ParentTrans => transform.parent as RectTransform;

        /// <summary>
        ///     UI Mono 初始化
        /// </summary>
        public void Initial()
        {
            RectTrans = transform as RectTransform;

            m_widghtDic = new Dictionary<string, GameObject>(10);
            // 添加自身
            if (transform.CompareTag("_UIWidght") && !m_widghtDic.ContainsKey(transform.name))
                m_widghtDic.Add(transform.name, transform.gameObject);

            AddComponentInChildren(transform);
        }

        /// <summary>
        ///     添加控件物体至映射表中
        ///     需要在代码中进行事件响应、控件获取等操作会添加至映射表中
        ///     控件需要打上 _UIWidght   标签 以区分管理
        /// </summary>
        /// <param name="root">面板的根节点</param>
        private void AddComponentInChildren( Transform root )
        {
            //添加子节点
            for (var i = 0; i < root.childCount; i++)
            {
                if (root.GetChild(i).CompareTag("_UIWidght") && !m_widghtDic.ContainsKey(root.GetChild(i).name))
                    m_widghtDic.Add(root.GetChild(i).name, root.GetChild(i).gameObject);

                if (root.GetChild(i).childCount > 0) AddComponentInChildren(root.GetChild(i));
            }
        }

        /// <summary>
        ///     获取UI控件的GameObject组件
        /// </summary>
        /// <param name="name">控件在Hierarchy面板中的名称</param>
        /// <returns>如果目标控件不存在返回null，否则返回该控件的GameObject组件</returns>
        protected GameObject GetGameObject( string name )
        {
            if (m_widghtDic.ContainsKey(name)) return m_widghtDic[name];
            throw new SsitEngineException("要获取的组件没有添加 '_UIWidght' 标签: " + name);
        }

        /// <summary>
        ///     控制UI控件的激活状态
        /// </summary>
        /// <param name="name">控件在Hierarchy面板中的名称</param>
        /// <param name="active">需要设置的目标控件的激活状态</param>
        protected void SetActive( string name, bool active )
        {
            if (m_widghtDic.ContainsKey(name))
            {
                m_widghtDic[name].SetActive(active);
                return;
            }
            throw new SsitEngineException("要获取的组件没有添加 '_UIWidght' 标签: " + name);
        }

        /// <summary>
        ///     获取UI控件的RectTransform组件
        /// </summary>
        /// <param name="name">控件在Hierarchy面板中的名称</param>
        /// <returns>如果目标控件不存在返回null，否则返回该控件的RectTransform组件</returns>
        protected RectTransform GetUITransform( string name )
        {
            var go = GetGameObject(name);
            if (go == null) throw new SsitEngineException("要获取的组件为空：" + name);

            return go.GetComponent<RectTransform>();
        }

        /// <summary>
        ///     获取UI控件的指定组件
        /// </summary>
        /// <typeparam name="T">要获取的目标组件类型</typeparam>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <returns>目标组件类型实例,如果目标控件不存在返回null</returns>
        [Obsolete("this function is abandon,use AddBindPath Attribute instead!!!")]
        protected T GetUIComponet<T>( string name ) where T : Component
        {
            var go = GetGameObject(name);
            var componet = go.GetComponent<T>();
            return componet;
        }

        /// <summary>
        ///     向目标UI控件添加指定类型的组件
        /// </summary>
        /// <typeparam name="T">所需添加的组件类型</typeparam>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <returns>所添加的组件类型，如果添加失败返回null</returns>
        protected T AddUIComponet<T>( string name ) where T : Component
        {
            var go = GetGameObject(name);
            var componet = go.GetComponent<T>();
            if (componet == null) componet = go.AddComponent<T>();

            return componet;
        }

        protected void InitUIWidght( BaseUIForm form )
        {
            foreach (var kv in m_widghtDic)
            {
                var temp = kv.Value.GetComponent<IUIWidgit>();
                temp?.Init(form);
            }
        }


        /// <summary>
        ///     销毁UI面板时的回调
        /// </summary>
        protected void Destroy()
        {
            if (m_widghtDic == null) return;


            m_widghtDic.Clear();
        }

        #region Base Method

        /// <summary>
        ///     设置文本内容
        ///     如果Text控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="uiText">目标Text控件</param>
        /// <param name="value">文本内容</param>
        protected void SetText( Text uiText, string value )
        {
            if (uiText)
                uiText.text = value;
            else
                throw new SsitEngineException("The Text: " + name + "is null");
        }

        #endregion

        #region Add widght event

        /// <summary>
        ///     添加Button按钮点击事件
        ///     如果按钮没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="clickCallBack">按钮点击事件无参回调</param>
        protected void AddButtonEvent( string name, UnityAction clickCallBack )
        {
            var btn = GetUIComponet<Button>(name);
            if (btn)
                btn.onClick.AddListener(clickCallBack);
            else
                throw new SsitEngineException("The button: " + name + "is null");
        }

        /// <summary>
        ///     添加Button按钮点击事件
        ///     如果按钮没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="clickCallBack">按钮点击事件有参回调，参数为所点击的按钮的GameObject组件</param>
        [Obsolete("this funtion is abandon,use AddBindPath Attribute instead")]
        protected void AddButtonEvent( string name, UnityAction<GameObject> clickCallBack )
        {
            var btn = GetUIComponet<Button>(name);
            if (btn)
                btn.onClick.AddListener(() => { clickCallBack.Invoke(btn.gameObject); });
            else
                throw new SsitEngineException("The button: " + name + "is null");
        }


        /// <summary>
        ///     添加Toggle开关点击事件
        ///     如果Toggle没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">Toggle控件在Hierarchy面板中的名称</param>
        /// <param name="clickCallBack">Toggle开关点击事件有参回调，参数为所点击的开关的GameObject组件、开关的状态 true/false</param>
        protected void AddToggleEvent( string name, UnityAction<GameObject, bool> clickCallBack )
        {
            var toggle = GetUIComponet<Toggle>(name);
            if (toggle)
                toggle.onValueChanged.AddListener(isOn => { clickCallBack(toggle.gameObject, isOn); });
            else
                throw new SsitEngineException("The toggle: " + name + "is null");
        }

        /// <summary>
        ///     设置文本内容
        ///     如果Text控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标Text控件在Hierarchy面板中的名称</param>
        /// <param name="value">文本内容</param>
        protected void SetText( string name, string value )
        {
            var t = GetUIComponet<Text>(name);
            if (t)
                t.text = value;
            else
                throw new SsitEngineException("The Text: " + name + "is null");
        }

        /// <summary>
        ///     添加 InputField 响应事件
        ///     如果 InputField控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标InputField控件在Hierarchy面板中的名称</param>
        /// <param name="onValueChangedAction">InputField输入时的回调，回调参数为目标InputField的GameObject控件、用户输入的文本</param>
        /// <param name="onEndEditAction">InputField结束输入时的回调，回调参数为目标InputField的GameObject控件、用户输入的文本。参数可为空</param>
        protected void AddInputFieldEvent( string name, UnityAction<GameObject, string> onValueChangedAction,
            UnityAction<GameObject, string> onEndEditAction = null )
        {
            var inputField = GetUIComponet<InputField>(name);
            if (inputField)
            {
                inputField.onValueChanged.AddListener(delegate( string str )
                {
                    onValueChangedAction(inputField.gameObject, str);
                });
                if (onEndEditAction != null)
                    inputField.onEndEdit.AddListener(str => { onEndEditAction(inputField.gameObject, str); });
            }
            else
            {
                throw new SsitEngineException("The inputField: " + name + "is null");
            }
        }

        /// <summary>
        ///     添加 Slider 响应事件
        ///     如果 Slider 控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标 Slider 控件在Hierarchy面板中的名称</param>
        /// <param name="onValueChangedAction">Slider滑动时的回调，回调参数为目标Slider的GameObject控件、Slider的当前进度值</param>
        protected void AddSliderEvent( string name, UnityAction<GameObject, float> onValueChangedAction )
        {
            var slider = GetUIComponet<Slider>(name);
            if (slider)
                slider.onValueChanged.AddListener(value => { onValueChangedAction(slider.gameObject, value); });
            else
                throw new SsitEngineException("The Slider: " + name + "is null");
        }

        /// <summary>
        ///     添加 ScrollBar 滚动条响应事件
        ///     如果 ScrollBar 控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标 ScrollBar 控件在Hierarchy面板中的名称</param>
        /// <param name="onValueChangedAction">ScrollBar 滑动时的回调，回调参数为目标ScrollBar的GameObject控件、ScrollBar的当前进度值</param>
        protected void AddScrollBarEvent( string name, UnityAction<GameObject, float> onValueChangedAction )
        {
            var scrollbar = GetUIComponet<Scrollbar>(name);
            if (scrollbar)
                scrollbar.onValueChanged.AddListener(value => { onValueChangedAction(scrollbar.gameObject, value); });
            else
                throw new SsitEngineException("The Scrollbar: " + name + "is null");
        }

        /// <summary>
        ///     添加 DropDown 下拉列表响应事件
        ///     如果 DropDown 控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标 DropDown 控件在Hierarchy面板中的名称</param>
        /// <param name="onValueChangedAction">DropDown 滑动时的回调，回调参数为目标 DropDown 的GameObject控件、DropDown的当前选中的条目ID索引</param>
        protected void AddDropDownEvent( string name, UnityAction<GameObject, int> onValueChangedAction )
        {
            var dropdown = GetUIComponet<Dropdown>(name);
            if (dropdown)
                dropdown.onValueChanged.AddListener(value =>
                {
                    onValueChangedAction.Invoke(dropdown.gameObject, value);
                });
            else
                throw new SsitEngineException("The Dropdown: " + name + "is null");
        }

        /// <summary>
        ///     添加 ScrollView 滚动视图响应事件
        ///     如果 ScrollView 控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标 ScrollView 控件在Hierarchy面板中的名称</param>
        /// <param name="onValueChangedAction">ScrollView 滑动时的回调，回调参数为目标 ScrollView 的GameObject控件、ScrollView的当前二维(水平/垂直)方向的偏移量</param>
        protected void AddScrollViewEvent( string name, UnityAction<GameObject, Vector2> onValueChangedAction )
        {
            var scrollRect = GetUIComponet<ScrollRect>(name);
            if (scrollRect)
                scrollRect.onValueChanged.AddListener(pos =>
                {
                    onValueChangedAction.Invoke(scrollRect.gameObject, pos);
                });
            else
                throw new SsitEngineException("The ScrollRect: " + name + "is null");
        }

        #region Trigger Event

        #region Pointer Trigger Event

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 进入控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="pointerEnterAction">指针进入时回调，回调参数为所进入的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddPointerEnterEvent( string name, UnityAction<GameObject, PointerEventData> pointerEnterAction )
        {
            AddTriggerEvent(name, EventTriggerType.PointerEnter, pointerEnterAction);
        }

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 退出控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="pointerExitAction">指针进入时回调，回调参数为所退出的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddPointerExitEvent( string name, UnityAction<GameObject, PointerEventData> pointerExitAction )
        {
            AddTriggerEvent(name, EventTriggerType.PointerExit, pointerExitAction);
        }

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 按下控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="pointerDownAction">指针按下时回调，回调参数为所退出的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddPointerDownEvent( string name, UnityAction<GameObject, PointerEventData> pointerDownAction )
        {
            AddTriggerEvent(name, EventTriggerType.PointerDown, pointerDownAction);
        }

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等  抬起事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="pointerUpAction">指针抬起时回调，回调参数为抬起时所悬浮的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddPointerUpEvent( string name, UnityAction<GameObject, PointerEventData> pointerUpAction )
        {
            AddTriggerEvent(name, EventTriggerType.PointerUp, pointerUpAction);
        }

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 点击控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="pointerClickAction">指针点击时回调，回调参数为所点击的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddPointerClickEvent( string name, UnityAction<GameObject, PointerEventData> pointerClickAction )
        {
            AddTriggerEvent(name, EventTriggerType.PointerClick, pointerClickAction);
        }

        #endregion

        #region Drag Trigger Event

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 开始拖拽 控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="onBeginDragAction">指针 开始拖拽 时回调，回调参数为所拖拽的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddBeginDragEvent( string name, UnityAction<GameObject, PointerEventData> onBeginDragAction )
        {
            AddTriggerEvent(name, EventTriggerType.BeginDrag, onBeginDragAction);
        }

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 拖拽中 控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="onDragAction">指针 拖拽中 时回调，回调参数为所拖拽的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddOnDragEvent( string name, UnityAction<GameObject, PointerEventData> onDragAction )
        {
            AddTriggerEvent(name, EventTriggerType.Drag, onDragAction);
        }

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 结束拖拽 控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="endDragAction">指针 结束拖拽 时回调，回调参数为所拖拽的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddEndDragEvent( string name, UnityAction<GameObject, PointerEventData> endDragAction )
        {
            AddTriggerEvent(name, EventTriggerType.EndDrag, endDragAction);
        }

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 拖拽 控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="onDragAction">指针 拖拽中 时回调，回调参数为所拖拽的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        /// <param name="beginDragAction">指针 开始拖拽 时回调，回调参数为所拖拽的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        /// <param name="endDragAction">指针 结束拖拽 时回调，回调参数为所拖拽的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddDragEvent( string name, UnityAction<GameObject, PointerEventData> onDragAction,
            UnityAction<GameObject, PointerEventData> beginDragAction = null,
            UnityAction<GameObject, PointerEventData> endDragAction = null )
        {
            AddBeginDragEvent(name, beginDragAction);
            AddOnDragEvent(name, onDragAction);
            AddEndDragEvent(name, endDragAction);
        }

        #endregion

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 放下 控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="pointerDropAction">指针 放下 时回调，回调参数为所放下的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddPointerDropEvent( string name, UnityAction<GameObject, PointerEventData> pointerDropAction )
        {
            AddTriggerEvent(name, EventTriggerType.Drop, pointerDropAction);
        }

        #region select deselect

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 选择 控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="pointerSelectAction">指针 选择放下 时回调，回调参数为所选择的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddPointerSelectEvent( string name,
            UnityAction<GameObject, PointerEventData> pointerSelectAction )
        {
            AddTriggerEvent(name, EventTriggerType.Select, pointerSelectAction);
        }

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 取消选择 控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="pointerDeselectAction">指针 取消选择 时回调，回调参数为所选择的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddPointerDeselectEvent( string name,
            UnityAction<GameObject, PointerEventData> pointerDeselectAction )
        {
            AddTriggerEvent(name, EventTriggerType.Deselect, pointerDeselectAction);
        }

        #endregion

        #region submit cancel

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 确认 控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="pointerSubmitAction">指针 确认 时回调，回调参数为所选择的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddPointerSubmitEvent( string name,
            UnityAction<GameObject, PointerEventData> pointerSubmitAction )
        {
            AddTriggerEvent(name, EventTriggerType.Submit, pointerSubmitAction);
        }

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 取消 控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="pointerCancelAction">指针 取消 时回调，回调参数为所选择的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddPointerCancelEvent( string name,
            UnityAction<GameObject, PointerEventData> pointerCancelAction )
        {
            AddTriggerEvent(name, EventTriggerType.Cancel, pointerCancelAction);
        }

        #endregion

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等 滚动 控件事件
        ///     如果目标控件没有添加至映射表中，会抛出错误异常
        ///     异常处理方法: 向该控件添加 _UIWidght 组件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="pointerScrollAction">指针 滚动 时回调，回调参数为所悬浮的控件的GameObject组件、指针当前数据，具体数据详见Unity官方API</param>
        protected void AddPointerScrollEvent( string name,
            UnityAction<GameObject, PointerEventData> pointerScrollAction )
        {
            AddTriggerEvent(name, EventTriggerType.Scroll, pointerScrollAction);
        }

        /// <summary>
        ///     添加鼠标指针/iTouch触摸模拟指针等操作控件事件
        /// </summary>
        /// <param name="name">目标控件在Hierarchy面板中的名称</param>
        /// <param name="eventId">指针操作类型</param>
        /// <param name="action">回调方法</param>
        private void AddTriggerEvent( string name, EventTriggerType eventId,
            UnityAction<GameObject, PointerEventData> action )
        {
            if (action == null) return;

            var trigger = GetUIComponet<EventTrigger>(name);

            if (trigger == null)
                trigger = GetGameObject(name).AddComponent<EventTrigger>();

            var entry = new EventTrigger.Entry();

            entry.eventID = eventId;

            entry.callback = new EventTrigger.TriggerEvent();

            entry.callback.AddListener(value => { action.Invoke(trigger.gameObject, value as PointerEventData); });


            trigger.triggers.Add(entry);
        }

        #endregion

        #endregion


        #region Remove Widght Event

        #endregion
    }
}