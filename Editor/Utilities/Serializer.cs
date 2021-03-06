using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Editor.Utilities
{
    public static class Serializer
    {
        public static void ToFile<T>(T instance, string path)
        {
            try
            {
                using FileStream fs = new(path, FileMode.Create);
                DataContractSerializer serializer = new(typeof(T));
                serializer.WriteObject(fs, instance);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to serialize {instance} to {path}");
                throw;
            }
        }

        public static T FromFile<T>(string path)
        {
            try
            {
                using FileStream fs = new(path, FileMode.Open);
                DataContractSerializer serializer = new(typeof(T));
                T instance = (T)serializer.ReadObject(fs);
                return instance;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to deserialize {path}");
                throw;
            }
        }
    }
}
