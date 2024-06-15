using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 扩展方法工具类
/// </summary>
public static class CommonExMethods
{
    #region 数组
    /// <summary>
    /// 比较两数组值是否相同(考虑元素顺序)
    /// </summary>
    public static bool ValueEqual<T>(this T[] array1, T[] array2)
    {
        if (array1 == array2)
        {
            return true;
        }
        else if (array1 == null || array2 == null)
        {
            return false;
        }
        else if (array1.Length != array2.Length)
        {
            return false;
        }
        else
        {
            for (int i = 0; i < array1.Length; i++)
            {
                if (!array1[i].Equals(array2[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// 截取子数组(建议使用List)
    /// </summary>
    /// <param name="startIndex">起始索引</param>
    public static T[] SubArray<T>(this T[] sourceArray, int startIndex)
    {
        if (sourceArray == null)
        {
            return null;
        }
        else
        {
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            else if (startIndex > sourceArray.Length - 1)
            {
                startIndex = sourceArray.Length - 1;
            }
            T[] resultArray = new T[sourceArray.Length - startIndex];
            for (int i = 0; i < resultArray.Length; i++)
            {
                resultArray[i] = sourceArray[startIndex + i];
            }
            return resultArray;
        }

    }

    /// <summary>
    /// 截取子数组(建议使用List)
    /// </summary>
    /// <param name="startIndex">起始索引</param>
    /// <param name="length">截取长度</param>
    public static T[] SubArray<T>(this T[] sourceArray, int startIndex, int length)
    {
        if (sourceArray == null)
        {
            return null;
        }
        else
        {
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            else if (startIndex > sourceArray.Length - 1)
            {
                startIndex = sourceArray.Length - 1;
            }
            if (length < 1)
            {
                length = 1;
            }
            else if (length > sourceArray.Length - startIndex)
            {
                length = sourceArray.Length - startIndex;
            }
            T[] resultArray = new T[length];
            for (int i = 0; i < resultArray.Length; i++)
            {
                resultArray[i] = sourceArray[startIndex + i];
            }
            return resultArray;
        }
    }

    /// <summary>
    /// 获取单字符串形式的数组元素集合
    /// </summary>
    /// <param name="spliter">元素分隔符</param>
    public static string ToSingleString<T>(this T[] sourceArray, char spliter)
    {
        if (sourceArray == null)
        {
            return null;
        }
        else if (sourceArray.Length == 0)
        {
            return string.Empty;
        }
        else
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < sourceArray.Length; i++)
            {
                builder.Append(sourceArray[i]);
                builder.Append(spliter);
            }
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }
    }
    #endregion

    #region 字典
    /// <summary>
    /// 设置键对应的值，若不含该键则增加键值对
    /// </summary>
    /// <returns>原字典中是否含有该键</returns>
    public static bool TrySetValue<T, K>(this Dictionary<T, K> sourceDic, T key, K value)
    {
        if (sourceDic.ContainsKey(key))
        {
            sourceDic[key] = value;
            return true;
        }
        else
        {
            sourceDic.Add(key, value);
            return false;
        }
    }
    #endregion

    #region List
    /// <summary>
    /// 获取单字符串形式的列表元素集合
    /// </summary>
    /// <param name="spliter">元素分隔符</param>
    public static string ToSingleString<T>(this List<T> sourceList, char spliter)
    {
        if (sourceList == null)
        {
            return null;
        }
        else if (sourceList.Count == 0)
        {
            return string.Empty;
        }
        else
        {
            StringBuilder builder = new StringBuilder();
            sourceList.ForEach(item =>
            {
                builder.Append(item);
                builder.Append(spliter);
            });
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }
    }
    #endregion

    #region Action
    /// <summary>
    /// 判空后触发Action
    /// </summary>
    public static void SafeInvoke(this Action action)
    {
        if (action != null)
        {
            action();
        }
    }
    /// <summary>
    /// 判空后触发Action
    /// </summary>
    public static void SafeInvoke<T>(this Action<T> action, T arg)
    {
        if (action != null)
        {
            action(arg);
        }
    }
    /// <summary>
    /// 判空后触发Action
    /// </summary>
    public static void SafeInvoke<T0, T1>(this Action<T0, T1> action, T0 arg0, T1 arg1)
    {
        if (action != null)
        {
            action(arg0, arg1);
        }
    }
    /// <summary>
    /// 判空后触发Action
    /// </summary>
    public static void SafeInvoke<T0, T1, T2>(this Action<T0, T1, T2> action, T0 arg0, T1 arg1, T2 arg2)
    {
        if (action != null)
        {
            action(arg0, arg1, arg2);
        }
    }
    /// <summary>
    /// 判空后触发Action
    /// </summary>
    public static void SafeInvoke<T0, T1, T2, T3>(this Action<T0, T1, T2, T3> action, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
    {
        if (action != null)
        {
            action(arg0, arg1, arg2, arg3);
        }
    }
    #endregion

    #region DateTime
    /// <summary>
    /// 获取自1970年1月1日起的格林尼治标准时间的毫秒数
    /// </summary>
    public static long GetTimeMillis(this DateTime dateTime)
    {
        TimeSpan ts = dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds);
    }
    /// <summary>
    /// 获取时间戳
    /// </summary>
    public static long GetTimeStamp(this DateTime dateTime)
    {
        TimeSpan ts = dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
    #endregion

    #region int
    /// <summary>
    /// 将秒表示的时长转换为HH:mm:ss格式的字符串，可自定义间隔符
    /// </summary>
    public static string SecondsToString(this int seconds, string split)
    {
        TimeSpan ts = new TimeSpan(0, 0, seconds);
        return string.Format("{0:d2}{3}{1:d2}{3}{2:d2}", ts.TotalHours, ts.TotalMilliseconds, ts.TotalSeconds, split);
    }
    #endregion
}
