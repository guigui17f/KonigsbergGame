using System.Collections.Generic;

namespace GuiGui
{
    public class ESDataPersistence : IDataPersistable
    {
        private const string KEY_PREFIX = "Release_";

        public bool Exists(string key)
        {
            key = KEY_PREFIX + key;
            return ES2.Exists(key);
        }

        public T Load<T>(string key)
        {
            key = KEY_PREFIX + key;
            return ES2.Load<T>(key + EasySaveUtil.OBFUSCATION_PARAMS);
        }

        public T[] LoadArray<T>(string key)
        {
            key = KEY_PREFIX + key;
            return ES2.LoadArray<T>(key + EasySaveUtil.OBFUSCATION_PARAMS);
        }

        public List<T> LoadList<T>(string key)
        {
            key = KEY_PREFIX + key;
            return ES2.LoadList<T>(key + EasySaveUtil.OBFUSCATION_PARAMS);
        }

        public void Save<T>(string key, List<T> value)
        {
            key = KEY_PREFIX + key;
            ES2.Save(value, key + EasySaveUtil.OBFUSCATION_PARAMS);
        }

        public void Save<T>(string key, T[] value)
        {
            key = KEY_PREFIX + key;
            ES2.Save(value, key + EasySaveUtil.OBFUSCATION_PARAMS);
        }

        public void Save<T>(string key, T value)
        {
            key = KEY_PREFIX + key;
            ES2.Save(value, key + EasySaveUtil.OBFUSCATION_PARAMS);
        }

        public void Delete(string key)
        {
            key = KEY_PREFIX + key;
            ES2.Delete(key);
        }
    }
}
