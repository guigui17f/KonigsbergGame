using System;

/// <summary>
/// 非泛型消息
/// </summary>
public class Signal
{
    private event Action signalEvent;

    /// <summary>
    /// 添加消息监听者
    /// </summary>
    public void AddListener(Action listener)
    {
        if (signalEvent == null)
        {
            signalEvent += listener;
        }
        else if (!Array.Exists(signalEvent.GetInvocationList(), invocation => invocation.Equals(listener)))
        {
            signalEvent += listener;
        }
    }

    /// <summary>
    /// 移除消息监听者
    /// </summary>
    public void RemoveListener(Action listener)
    {
        signalEvent -= listener;
    }

    /// <summary>
    /// 清空消息监听者
    /// </summary>
    public void ClearListeners()
    {
        signalEvent = null;
    }

    /// <summary>
    /// 触发消息
    /// </summary>
    public void Dispatch()
    {
        if (signalEvent != null)
        {
            signalEvent();
        }
    }
}

/// <summary>
/// 泛型消息一
/// </summary>
public class Signal<T>
{
    private event Action<T> signalEvent;

    /// <summary>
    /// 添加消息监听者
    /// </summary>
    public void AddListener(Action<T> listener)
    {
        if (signalEvent == null)
        {
            signalEvent += listener;
        }
        else if (!Array.Exists(signalEvent.GetInvocationList(), invocation => invocation.Equals(listener)))
        {
            signalEvent += listener;
        }
    }

    /// <summary>
    /// 移除消息监听者
    /// </summary>
    public void RemoveListener(Action<T> listener)
    {
        signalEvent -= listener;
    }

    /// <summary>
    /// 清空消息监听者
    /// </summary>
    public void ClearListeners()
    {
        signalEvent = null;
    }

    /// <summary>
    /// 触发消息
    /// </summary>
    /// <param name="arg">消息参数</param>
    public void Dispatch(T arg)
    {
        if (signalEvent != null)
        {
            signalEvent(arg);
        }
    }
}

/// <summary>
/// 泛型消息二
/// </summary>
public class Signal<T0, T1>
{
    private event Action<T0, T1> signalEvent;

    /// <summary>
    /// 添加消息监听者
    /// </summary>
    public void AddListener(Action<T0, T1> listener)
    {
        if (signalEvent == null)
        {
            signalEvent += listener;
        }
        else if (!Array.Exists(signalEvent.GetInvocationList(), invocation => invocation.Equals(listener)))
        {
            signalEvent += listener;
        }
    }

    /// <summary>
    /// 移除消息监听者
    /// </summary>
    public void RemoveListener(Action<T0, T1> listener)
    {
        signalEvent -= listener;
    }

    /// <summary>
    /// 清空消息监听者
    /// </summary>
    public void ClearListeners()
    {
        signalEvent = null;
    }

    /// <summary>
    /// 触发消息
    /// </summary>
    /// <param name="arg0">消息参数一</param>
    /// <param name="arg1">消息参数二</param>
    public void Dispatch(T0 arg0, T1 arg1)
    {
        if (signalEvent != null)
        {
            signalEvent(arg0, arg1);
        }
    }
}

/// <summary>
/// 泛型消息三
/// </summary>
public class Signal<T0, T1, T2>
{
    private event Action<T0, T1, T2> signalEvent;

    /// <summary>
    /// 添加消息监听者
    /// </summary>
    public void AddListener(Action<T0, T1, T2> listener)
    {
        if (signalEvent == null)
        {
            signalEvent += listener;
        }
        else if (!Array.Exists(signalEvent.GetInvocationList(), invocation => invocation.Equals(listener)))
        {
            signalEvent += listener;
        }
    }

    /// <summary>
    /// 移除消息监听者
    /// </summary>
    public void RemoveListener(Action<T0, T1, T2> listener)
    {
        signalEvent -= listener;
    }

    /// <summary>
    /// 清空消息监听者
    /// </summary>
    public void ClearListeners()
    {
        signalEvent = null;
    }

    /// <summary>
    /// 触发消息
    /// </summary>
    /// <param name="arg0">消息参数一</param>
    /// <param name="arg1">消息参数二</param>
    /// <param name="arg2">消息参数三</param>
    public void Dispatch(T0 arg0, T1 arg1, T2 arg2)
    {
        if (signalEvent != null)
        {
            signalEvent(arg0, arg1, arg2);
        }
    }
}

/// <summary>
/// 泛型消息四
/// </summary>
public class Signal<T0, T1, T2, T3>
{
    private event Action<T0, T1, T2, T3> signalEvent;

    /// <summary>
    /// 添加消息监听者
    /// </summary>
    public void AddListener(Action<T0, T1, T2, T3> listener)
    {
        if (signalEvent == null)
        {
            signalEvent += listener;
        }
        else if (!Array.Exists(signalEvent.GetInvocationList(), invocation => invocation.Equals(listener)))
        {
            signalEvent += listener;
        }
    }

    /// <summary>
    /// 移除消息监听者
    /// </summary>
    public void RemoveListener(Action<T0, T1, T2, T3> listener)
    {
        signalEvent -= listener;
    }

    /// <summary>
    /// 清空消息监听者
    /// </summary>
    public void ClearListeners()
    {
        signalEvent = null;
    }

    /// <summary>
    /// 触发消息
    /// </summary>
    /// <param name="arg0">消息参数一</param>
    /// <param name="arg1">消息参数二</param>
    /// <param name="arg2">消息参数三</param>
    /// <param name="arg3">消息参数四</param>
    public void Dispatch(T0 arg0, T1 arg1, T2 arg2, T3 arg3)
    {
        if (signalEvent != null)
        {
            signalEvent(arg0, arg1, arg2, arg3);
        }
    }
}