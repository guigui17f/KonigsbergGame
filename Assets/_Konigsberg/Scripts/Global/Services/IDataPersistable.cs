using System.Collections.Generic;

namespace GuiGui
{
    public interface IDataPersistable
    {
        bool Exists(string key);
        void Save<T>(string key, T value);
        void Save<T>(string key, T[] value);
        void Save<T>(string key, List<T> value);
        T Load<T>(string key);
        T[] LoadArray<T>(string key);
        List<T> LoadList<T>(string key);
        void Delete(string key);
    }
}
