using System;
using System.Collections.Generic;

namespace ThothBotCore.Storage.Implementations
{
    public class InMemoryStorage : IDataStorage
    {
        public InMemoryStorage()
        {

        }

        private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();

        public void StoreObject(object obj, string key)
        {
            if (_dictionary.ContainsKey(key)) return;
            _dictionary.Add(key, obj);
        }

        public T RestoreObject<T>(string key)
        {
            if (!_dictionary.ContainsKey(key))
                throw new ArgumentException($"The provided key '{key}' wasn't found.");

            return (T)_dictionary[key];
        }
    }
}
