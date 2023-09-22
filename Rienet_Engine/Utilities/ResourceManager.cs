using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Content;

namespace Rienet
{
    public class ResourceManager
    {
        readonly Dictionary<int, List<object>> lists = new();
        readonly Dictionary<int, Dictionary<object, object>> dictionaries = new();

        public void AddList(int ID, List<object> list)
        {
            if (!lists.ContainsKey(ID))
                lists.Add(ID, list);
        }

        public void AddDictionary(int ID, Dictionary<object, object> dictionary)
        {
            if (dictionaries.ContainsKey(ID))
                dictionaries.Add(ID, dictionary);
        }

        public void Dispose(object obj)
        {
            //removed from both lists and dictionaries
            foreach (int key in lists.Keys)
                lists.Remove(key);

            foreach (int key in dictionaries.Keys)
                dictionaries.Remove(key);
        }

        public void Dispose(object obj, int[] existingListIDs, int[] existingDictionaryIDs)
        {
            foreach (int key in existingListIDs)
                lists.Remove(key);

            foreach (int key in existingDictionaryIDs)
                dictionaries.Remove(key);
        }
    }
}