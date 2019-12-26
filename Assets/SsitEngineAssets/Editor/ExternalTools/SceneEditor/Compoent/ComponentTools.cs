/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：                                                    
*│　作    者：Xuxin                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/12/26 16:19:49                             
*└──────────────────────────────────────────────────────────────┘
*/
using UnityEditor;
using UnityEngine;

public class CopyComponent : Editor
{

    static Component[] compoentArr;

    [MenuItem("Tools/SceneTools/拷贝所有组件(包含位置旋转)",priority = 0)]
    static void DoCopyComponent()
    {
        compoentArr = Selection.activeGameObject.GetComponents<Component>();
    }

    [MenuItem("Tools/SceneTools/粘贴所有组件", priority = 1)]
    static void DoPasteComponent()
    {
        if (compoentArr == null)
        {
            return;
        }

        GameObject targetObject = Selection.activeGameObject;
        if (targetObject == null)
        {
            return;
        }

        for (int i = 0; i < compoentArr.Length; i++)
        {
            Component newComponent = compoentArr[i];
            if (newComponent == null)
            {
                continue;
            }
            UnityEditorInternal.ComponentUtility.CopyComponent(newComponent);
            Component oldComponent = targetObject.GetComponent(newComponent.GetType());
            if (oldComponent != null)
            {
                if (UnityEditorInternal.ComponentUtility.PasteComponentValues(oldComponent))
                {
                    Debug.Log("Paste Values " + newComponent.GetType().ToString() + " Success");
                }
                else
                {
                    Debug.Log("Paste Values " + newComponent.GetType().ToString() + " Failed");
                }
            }
            else
            {
                if (UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetObject))
                {
                    Debug.Log("Paste New Values " + newComponent.GetType().ToString() + " Success");
                }
                else
                {
                    Debug.Log("Paste New Values " + newComponent.GetType().ToString() + " Failed");
                }
            }
        }
    }
    
    [MenuItem("Tools/SceneTools/覆盖粘贴所有组件", priority = 2)]
    static void DoRePasteComponent()
    {
        if (compoentArr == null)
        {
            return;
        }

        GameObject targetObject = Selection.activeGameObject;
        if (targetObject == null)
        {
            return;
        }

        RemoveAllComponent(targetObject);

        for (int i = 0; i < compoentArr.Length; i++)
        {
            Component newComponent = compoentArr[i];
            if (newComponent == null)
            {
                continue;
            }
            UnityEditorInternal.ComponentUtility.CopyComponent(newComponent);
            Component oldComponent = targetObject.GetComponent(newComponent.GetType());
            if (oldComponent != null)
            {
                if (UnityEditorInternal.ComponentUtility.PasteComponentValues(oldComponent))
                {
                    Debug.Log("Paste Values " + newComponent.GetType().ToString() + " Success");
                }
                else
                {
                    Debug.Log("Paste Values " + newComponent.GetType().ToString() + " Failed");
                }
            }
            else
            {
                if (UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetObject))
                {
                    Debug.Log("Paste New Values " + newComponent.GetType().ToString() + " Success");
                }
                else
                {
                    Debug.Log("Paste New Values " + newComponent.GetType().ToString() + " Failed");
                }
            }
        }
    }
    
    [MenuItem("Tools/SceneTools/保留粘贴所有组件(不包含位置旋转)", priority = 3)]
    static void DoPasteComponentNotAll()
    {
        if (compoentArr == null)
        {
            return;
        }

        GameObject targetObject = Selection.activeGameObject;
        if (targetObject == null)
        {
            return;
        }

        for (int i = 0; i < compoentArr.Length; i++)
        {
            Component newComponent = compoentArr[i];
            if (newComponent == null || newComponent.GetType() == typeof(Transform))
            {
                continue;
            }
            UnityEditorInternal.ComponentUtility.CopyComponent(newComponent);
            Component oldComponent = targetObject.GetComponent(newComponent.GetType());
            if (oldComponent != null)
            {
                if (UnityEditorInternal.ComponentUtility.PasteComponentValues(oldComponent))
                {
                    Debug.Log("Paste Values " + newComponent.GetType().ToString() + " Success");
                }
                else
                {
                    Debug.Log("Paste Values " + newComponent.GetType().ToString() + " Failed");
                }
            }
            else
            {
                if (UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetObject))
                {
                    Debug.Log("Paste New Values " + newComponent.GetType().ToString() + " Success");
                }
                else
                {
                    Debug.Log("Paste New Values " + newComponent.GetType().ToString() + " Failed");
                }
            }
        }
    }

    [MenuItem("Tools/SceneTools/覆盖粘贴所有组件(不包含位置旋转)", priority = 4)]
    static void DoRePasteComponentNotAll()
    {
        if (compoentArr == null)
        {
            return;
        }

        GameObject targetObject = Selection.activeGameObject;
        if (targetObject == null)
        {
            return;
        }

        RemoveAllComponent(targetObject);

        for (int i = 0; i < compoentArr.Length; i++)
        {
            Component newComponent = compoentArr[i];
            if (newComponent == null || newComponent.GetType() == typeof(Transform))
            {
                continue;
            }
            UnityEditorInternal.ComponentUtility.CopyComponent(newComponent);
            Component oldComponent = targetObject.GetComponent(newComponent.GetType());
            if (oldComponent != null)
            {
                if (UnityEditorInternal.ComponentUtility.PasteComponentValues(oldComponent))
                {
                    Debug.Log("Paste Values " + newComponent.GetType().ToString() + " Success");
                }
                else
                {
                    Debug.Log("Paste Values " + newComponent.GetType().ToString() + " Failed");
                }
            }
            else
            {
                if (UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetObject))
                {
                    Debug.Log("Paste New Values " + newComponent.GetType().ToString() + " Success");
                }
                else
                {
                    Debug.Log("Paste New Values " + newComponent.GetType().ToString() + " Failed");
                }
            }
        }
    }


    private static void RemoveAllComponent(GameObject childGameObject)
    {
        var coms = childGameObject.GetComponents<Component>();
        for (int i = coms.Length - 1; i >= 0; i--)
        {
            if (coms[i].GetType() == typeof(Transform))
                continue;
            DestroyImmediate(coms[i]);
        }
    }

    static void DoPasteComponent(GameObject targetObject)
    {
        if (compoentArr == null)
            return;

        for (int i = 0; i < compoentArr.Length; i++)
        {
            Component newComponent = compoentArr[i];
            if (newComponent == null)
                continue;

            if (newComponent.GetType() == typeof(Transform))
                continue;

            UnityEditorInternal.ComponentUtility.CopyComponent(newComponent);
            Component oldComponent = targetObject.GetComponent(newComponent.GetType());
            if (oldComponent != null)
            {
                if (UnityEditorInternal.ComponentUtility.PasteComponentValues(oldComponent))
                {
                    Debug.Log("Paste Values " + newComponent.GetType().ToString() + " Success");
                }
                else
                {
                    Debug.Log("Paste Values " + newComponent.GetType().ToString() + " Failed");
                }
            }
            else
            {
                if (UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetObject))
                {
                    Debug.Log("Paste New Values " + newComponent.GetType().ToString() + " Success");
                }
                else
                {
                    Debug.Log("Paste New Values " + newComponent.GetType().ToString() + " Failed");
                }
            }
        }
    }

}
