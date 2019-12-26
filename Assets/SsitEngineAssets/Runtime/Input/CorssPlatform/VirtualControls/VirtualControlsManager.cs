/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：虚拟操作管理器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年6月3日                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    /// Coordinates all of the virtual controls. All of the virtual controls must be a child of the VirtualControlsManager GameObject.
    /// </summary>
    public class VirtualControlsManager : MonoBehaviour
    {
        private GameObject m_CameraGameObject;

        [Tooltip("The character used by the virtual input. Can be null.")] [SerializeField]
        protected GameObject m_Character;

        private GameObject m_GameObject;

        private readonly Dictionary<string, VirtualControl> m_NameVirtualControlsMap =
            new Dictionary<string, VirtualControl>();

        public GameObject Character
        {
            get => m_Character;
            set => OnAttachCharacter(value);
        }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            if (m_Character == null)
            {
                //var camera = UnityEngineUtility.FindCamera(null);
                //if (camera != null) {
                //    m_CameraGameObject = camera.gameObject;
                //EventHandler.RegisterEvent<GameObject>(m_CameraGameObject, "OnCameraAttachCharacter", OnAttachCharacter);
                //}
            }
            else
            {
                var character = m_Character;
                m_Character = null; // Set the character to null so the assignment will occur.
                OnAttachCharacter(character);
            }

            m_GameObject.SetActive(false);
        }

        /// <summary>
        /// Attaches the component to the specified character.
        /// </summary>
        /// <param name="character">The handler to attach the camera to.</param>
        private void OnAttachCharacter( GameObject character )
        {
            if (character == m_Character) return;

            if (m_Character != null)
            {
                var unityInput = m_Character.GetComponent<CrossPlatInput>();
                if (unityInput == null) return;

                unityInput.UnegisterVirtualControlsManager();
            }

            m_Character = character;

            var activateGameObject = false;
            if (character != null)
            {
                var unityInput = m_Character.GetComponent<CrossPlatInput>();
                if (unityInput == null)
                {
                    Debug.LogError("Error: The character " + m_Character.name + " has no UnityInput component.");
                    return;
                }

                // If the virtual controls weren't registered then the virtual input type isn't selected.
                activateGameObject = unityInput.RegisterVirtualControlsManager(this);
            }

            m_GameObject.SetActive(activateGameObject);
        }

        /// <summary>
        /// Associates the input name with the virtual control object.
        /// </summary>
        /// <param name="inputName">The name to associate the virtual control object with.</param>
        /// <param name="virtualInput">The object to associate with the name.</param>
        public void RegisterVirtualControl( string inputName, VirtualControl virtualControl )
        {
            m_NameVirtualControlsMap.Add(inputName, virtualControl);
        }

        /// <summary>
        /// Removes the association with the object specified by the input name.
        /// </summary>
        /// <param name="inputName">The name of the object to remove association with.</param>
        public void UnregisterVirtualControl( string inputName )
        {
            m_NameVirtualControlsMap.Remove(inputName);
        }

        /// <summary>
        /// Returns if the button is true with the specified ButtonAction.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <param name="action">The type of action to check.</param>
        /// <returns>The status of the action.</returns>
        public bool GetButton( string name, InputBase.ButtonAction action )
        {
            VirtualControl virtualControl;
            if (!m_NameVirtualControlsMap.TryGetValue(name, out virtualControl)
            ) //Debug.LogError("Error: No virtual input object exists with the name " + name);
                return false;

            return virtualControl.GetButton(action);
        }

        /// <summary>
        /// Returns the axis of the specified button.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The axis value.</returns>
        public float GetAxis( string name )
        {
            VirtualControl virtualControl;
            if (!m_NameVirtualControlsMap.TryGetValue(name, out virtualControl)
            ) //Debug.LogError("Error: No virtual input object exists with the name " + name);
                return 0;

            return virtualControl.GetAxis(name);
        }


        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_CameraGameObject != null)
            {
                //EventHandler.UnregisterEvent<GameObject>(m_CameraGameObject, "OnCameraAttachCharacter", OnAttachCharacter);
            }
        }
    }
}