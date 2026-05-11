using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace WpfApp.Services
{
    public class JsonRepository<T>
    {
        private readonly string _filePath;
        private static readonly object _lock = new object();

        public JsonRepository(string fileName)
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, fileName);
        }

        public List<T> Load()
        {
            lock (_lock)
            {
                if (!File.Exists(_filePath)) return new List<T>();
                var json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json)) return new List<T>();
                var list = JsonConvert.DeserializeObject<List<T>>(json);
                return list ?? new List<T>();
            }
        }

        public void Save(IEnumerable<T> items)
        {
            lock (_lock)
            {
                var json = JsonConvert.SerializeObject(items, Formatting.Indented);
                File.WriteAllText(_filePath, json);
            }
        }
    }
}
