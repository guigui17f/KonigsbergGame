using UnityEngine;
using UnityEngine.Events;

public static class UnityExMethods
{
    #region GameObject
    /// <summary>
    /// 移除所有子游戏物体
    /// </summary>
    public static void RemoveAllChildren(this GameObject gameObject)
    {
        Transform transform = gameObject.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 设置自身和自身所有子物体的Layer
    /// </summary>
    public static void SetChildrenLayers(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        Transform transform = gameObject.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetChildrenLayers(layer);
        }
    }
    #endregion

    #region UnityAction
    /// <summary>
    /// 判空后触发Action
    /// </summary>
    public static void SafeInvoke(this UnityAction action)
    {
        if (action != null)
        {
            action();
        }
    }
    /// <summary>
    /// 判空后触发Action
    /// </summary>
    public static void SafeInvoke<T>(this UnityAction<T> action, T arg)
    {
        if (action != null)
        {
            action(arg);
        }
    }
    /// <summary>
    /// 判空后触发Action
    /// </summary>
    public static void SafeInvoke<T0, T1>(this UnityAction<T0, T1> action, T0 arg0, T1 arg1)
    {
        if (action != null)
        {
            action(arg0, arg1);
        }
    }
    /// <summary>
    /// 判空后触发Action
    /// </summary>
    public static void SafeInvoke<T0, T1, T2>(this UnityAction<T0, T1, T2> action, T0 arg0, T1 arg1, T2 arg2)
    {
        if (action != null)
        {
            action(arg0, arg1, arg2);
        }
    }
    /// <summary>
    /// 判空后触发Action
    /// </summary>
    public static void SafeInvoke<T0, T1, T2, T3>(this UnityAction<T0, T1, T2, T3> action, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
    {
        if (action != null)
        {
            action(arg0, arg1, arg2, arg3);
        }
    }
    #endregion

    #region RectTransform
    private static Vector3 GetRectReferenceCorner(RectTransform gui)
    {
        Transform transform = gui.transform;
        Vector3[] s_Corners = new Vector3[4];
        gui.GetWorldCorners(s_Corners);
        if (transform.parent != null)
        {
            return transform.parent.InverseTransformPoint(s_Corners[0]);
        }
        else
        {
            return s_Corners[0];
        }
    }
    /// <summary>
    /// 设置RectTransform pivot值，保持位置不变
    /// </summary>
    public static void SetPivot(this RectTransform rect, Vector2 pivot)
    {
        Vector3 rectReferenceCorner = GetRectReferenceCorner(rect);
        rect.pivot = pivot;
        Vector3 rectReferenceCorner2 = GetRectReferenceCorner(rect);
        Vector3 v = rectReferenceCorner2 - rectReferenceCorner;
        rect.anchoredPosition -= new Vector2(v.x, v.y);
        Vector3 position = rect.transform.position;
        position.z -= v.z;
        rect.transform.position = position;
    }
    #endregion
}
