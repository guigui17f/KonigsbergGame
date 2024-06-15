using I2.Loc;

/// <summary>
/// I2插件配套的多语言文本
/// </summary>
public struct I2String
{
    private string m_TextKey;

    public I2String(string key)
    {
        m_TextKey = key;
    }

    /// <summary>
    /// 当前语言对应的字符串
    /// </summary>
    public string CurrentString
    {
        get
        {
            return ScriptLocalization.Get(m_TextKey);
        }
    }
}
