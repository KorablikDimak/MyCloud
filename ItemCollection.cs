using System.Collections.Generic;

namespace MyCloud
{
    public class ItemCollection
    {
        private Dictionary<string, object> Collection { get; set; }

        public ItemCollection()
        {
            Collection = new Dictionary<string, object>();
        }

        public void SaveItem<T>(string name, T value) where T : class
        {
            Collection.Add(name, value);
        }

        public T GetItem<T>(string name) where T : class
        {
            if (!Collection.ContainsKey(name)) return null;
            return Collection[name] as T;
        }

        public void RemoveItem(string name)
        {
            Collection.Remove(name);
        }
    }
}