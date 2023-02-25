using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml;

namespace SilverCraft.AvaloniaUtils
{
    public interface IEnvBackend
    {
        public T? GetEnv<T>(string EnvvarName) where T : class;
        public E? GetEnum<E>(string EnvvarName) where E : struct;

        public bool? GetBool(string EnvvarName);
        public byte? GetByte(string EnvvarName);
        public decimal? GetDecimal(string EnvvarName);
        public float? GetSingle(string EnvvarName);
        public string? GetString(string EnvvarName);
        public int? GetInt(string EnvvarName);



        public void SetEnv<T>(string EnvvarName, T? Value) where T : class;
        public void SetEnum<E>(string EnvvarName, E? Value) where E : struct;

        public void SetBool(string EnvvarName, bool? Value);
        public void SetByte(string EnvvarName, byte? Value);
        public void SetDecimal(string EnvvarName, decimal? Value);
        public void SetSingle(string EnvvarName, float? Value);
        public void SetString(string EnvvarName, string? Value);
        public void SetInt(string EnvvarName, int? Value);



    }
    public class ModernDotFileBackend : IEnvBackend
    {
        string FileLoc;
        private FileSystemWatcher _watcher;
        public ModernDotFileBackend(string loc)
        {
            FileLoc = loc;
            var dir = Path.GetDirectoryName(loc);
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                _watcher = new(dir)
                {
                    Filter = Path.GetFileName(loc),
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.LastWrite
                };
                _watcher.Changed+=WatcherOnChanged;
            }
            
        }
        
        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            WindowExtensions.OnStyleChange.Invoke(this,new ( null, GetString("SAPColor"), ((IEnvBackend)this).GetEnum<WindowTransparencyLevel>("SAPTransparency")));
        }

        public JsonElement? GetJsonElement(string EnvvarName)
        {
            if (!File.Exists(FileLoc)) return null;
            var jsonData = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(FileLoc));
            if (jsonData.TryGetProperty(EnvvarName, out var val))
            {
                return val;
            }
            return null;
        }
        public bool? GetBool(string EnvvarName)
        {
            return GetJsonElement(EnvvarName)?.GetBoolean();
        }

        public byte? GetByte(string EnvvarName)
        {
            return GetJsonElement(EnvvarName)?.GetByte();
        }

        public decimal? GetDecimal(string EnvvarName)
        {
            return GetJsonElement(EnvvarName)?.GetDecimal();
        }

        public T? GetEnv<T>(string EnvvarName) where T : class
        {
            if (!File.Exists(FileLoc)) return null;
            var jsonData = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(FileLoc));
            return jsonData.TryGetProperty(EnvvarName, out var val) ? val.Deserialize<T>() : null;
        }

        public int? GetInt(string EnvvarName)
        {
            return GetJsonElement(EnvvarName)?.GetInt32();
        }

        public float? GetSingle(string EnvvarName)
        {
            return GetJsonElement(EnvvarName)?.GetSingle();
        }

        public string? GetString(string EnvvarName)
        {
            return GetJsonElement(EnvvarName)?.GetString();
        }

        public void SetBool(string EnvvarName, bool? Value)
        {
            JsonObject jsonData = File.Exists(FileLoc) ? JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(FileLoc)) : new();
            if (jsonData.ContainsKey(EnvvarName))
            {
                jsonData[EnvvarName]=Value;
            }
            else
            {
                jsonData.Add(new(EnvvarName, Value));
            }
            File.WriteAllText(FileLoc, JsonSerializer.Serialize(jsonData));
        }

        public void SetByte(string EnvvarName, byte? Value)
        {
            JsonObject jsonData = File.Exists(FileLoc) ? JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(FileLoc)) : new();

            if (jsonData.ContainsKey(EnvvarName))
            {
                jsonData[EnvvarName] = Value;
            }
            else
            {
                jsonData.Add(new(EnvvarName, Value));
            }
            File.WriteAllText(FileLoc, JsonSerializer.Serialize(jsonData));
        }

        public void SetDecimal(string EnvvarName, decimal? Value)
        {
            JsonObject jsonData = File.Exists(FileLoc) ? JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(FileLoc)) : new();
            if (jsonData.ContainsKey(EnvvarName))
            {
                jsonData[EnvvarName] = Value;
            }
            else
            {
                jsonData.Add(new(EnvvarName, Value));
            }
            File.WriteAllText(FileLoc, JsonSerializer.Serialize(jsonData));
        }

        public void SetEnv<T>(string EnvvarName, T? Value) where T : class
        {
            var jsonData = File.Exists(FileLoc) ? JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(FileLoc)) : new();
            if (jsonData.ContainsKey(EnvvarName))
            {
                jsonData[EnvvarName] = JsonSerializer.SerializeToNode(Value);
            }
            else
            {
                jsonData.Add(new(EnvvarName, JsonSerializer.SerializeToNode(Value)));
            }
            File.WriteAllText(FileLoc, JsonSerializer.Serialize(jsonData));

        }

        public void SetInt(string EnvvarName, int? Value)
        {
            var jsonData = File.Exists(FileLoc) ? JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(FileLoc)) : new();
            if (jsonData.ContainsKey(EnvvarName))
            {
                jsonData[EnvvarName] = JsonSerializer.SerializeToNode(Value);
            }
            else
            {
                jsonData.Add(new(EnvvarName, JsonSerializer.SerializeToNode(Value)));
            }
            File.WriteAllText(FileLoc, JsonSerializer.Serialize(jsonData));
        }

        public void SetSingle(string EnvvarName, float? Value)
        {
            var jsonData = File.Exists(FileLoc) ? JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(FileLoc)) : new();
            if (jsonData.ContainsKey(EnvvarName))
            {
                jsonData[EnvvarName] = JsonSerializer.SerializeToNode(Value);
            }
            else
            {
                jsonData.Add(new(EnvvarName, JsonSerializer.SerializeToNode(Value)));
            }
            File.WriteAllText(FileLoc, JsonSerializer.Serialize(jsonData));
        }

        public void SetString(string EnvvarName, string? Value)
        {
            SetEnv(EnvvarName, Value);

        }

        E? IEnvBackend.GetEnum<E>(string EnvvarName)
        {
            if (!File.Exists(FileLoc)) return null;
            var jsonData = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(FileLoc));
            if (jsonData.TryGetProperty(EnvvarName, out var val))
            {
                return Enum.Parse<E>(val.GetString());
            }
            return null;
        }

        void IEnvBackend.SetEnum<E>(string EnvvarName, E? Value) 
        {
            var jsonData= File.Exists(FileLoc) ? JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(FileLoc)) : new();
            if (jsonData.ContainsKey(EnvvarName))
            {
                jsonData[EnvvarName] = JsonSerializer.SerializeToNode(Enum.GetName(typeof(E),Value));
            }
            else
            {
                jsonData.Add(new(EnvvarName, Enum.GetName(typeof(E),Value)));
            }
            File.WriteAllText(FileLoc, JsonSerializer.Serialize(jsonData));
        }
    }
    public class LegacyEnviromentBackend : IEnvBackend
    {
        public T? GetEnv<T>(string EnvvarName) where T : class
        {
            if (typeof(T).IsAssignableFrom(typeof(string)))
            {
                return GetStringEnv(EnvvarName) as T;
            }
            if (typeof(T).IsAssignableFrom(typeof(bool)))
            {
                return XmlConvert.ToBoolean(GetStringEnv(EnvvarName)) as T;
            }
            if (typeof(T).IsAssignableFrom(typeof(byte)))
            {
                return XmlConvert.ToByte(GetStringEnv(EnvvarName)) as T;
            }
            if (typeof(T).IsAssignableFrom(typeof(decimal)))
            {
                return XmlConvert.ToDecimal(GetStringEnv(EnvvarName)) as T;
            }
            if (typeof(T).IsAssignableFrom(typeof(double)))
            {
                return XmlConvert.ToDouble(GetStringEnv(EnvvarName)) as T;
            }
            if (typeof(T).IsAssignableFrom(typeof(float)))
            {
                return XmlConvert.ToSingle(GetStringEnv(EnvvarName)) as T;
            }
            throw new NotImplementedException();
        }

        public string? GetStringEnv(string EnvvarName)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return Environment.GetEnvironmentVariable(EnvvarName, EnvironmentVariableTarget.User);
            }
            return Environment.GetEnvironmentVariable(EnvvarName);
        }

      

        public void SetStringEnv(string EnvvarName, string? Value)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Environment.SetEnvironmentVariable(EnvvarName, Value, EnvironmentVariableTarget.User);
            }
            else
            {
                Environment.SetEnvironmentVariable(EnvvarName, Value);
            }
        }

        public void SetEnv<T>(string EnvvarName, T? Value) where T : class
        {
            if (typeof(T).IsAssignableFrom(typeof(string)))
            {
                SetStringEnv(EnvvarName, Value as string) ;
                return;
            }
            if (typeof(T).IsAssignableFrom(typeof(bool)))
            {
                SetStringEnv(EnvvarName, XmlConvert.ToString((bool)(Value as bool?)));
                return;
            }
            if (typeof(T).IsAssignableFrom(typeof(byte)))
            {
                SetStringEnv(EnvvarName, XmlConvert.ToString((byte)(Value as byte?)));
                return;
            }
            if (typeof(T).IsAssignableFrom(typeof(decimal)))
            {
                SetStringEnv(EnvvarName, XmlConvert.ToString((decimal)(Value as decimal?)));
                return;
            }
            if (typeof(T).IsAssignableFrom(typeof(double)))
            {
                SetStringEnv(EnvvarName, XmlConvert.ToString((double)(Value as double?)));
                return;
            }
            if (typeof(T).IsAssignableFrom(typeof(float)))
            {
                SetStringEnv(EnvvarName, XmlConvert.ToString((float)(Value as float?)));
                return;
            }
            throw new NotImplementedException();
        }

        public bool? GetBool(string EnvvarName)
        {
            return XmlConvert.ToBoolean(GetStringEnv(EnvvarName));
        }

        public byte? GetByte(string EnvvarName)
        {
            return XmlConvert.ToByte(GetStringEnv(EnvvarName));

        }

        public decimal? GetDecimal(string EnvvarName)
        {
            return XmlConvert.ToDecimal(GetStringEnv(EnvvarName));

        }

        public float? GetSingle(string EnvvarName)
        {
            return XmlConvert.ToSingle(GetStringEnv(EnvvarName));

        }

        public string? GetString(string EnvvarName)
        {
            return GetStringEnv(EnvvarName);

        }

        public void SetBool(string EnvvarName, bool? Value)
        {
            SetStringEnv(EnvvarName, XmlConvert.ToString((bool)Value));

        }

        public void SetByte(string EnvvarName, byte? Value)
        {
            SetStringEnv(EnvvarName, XmlConvert.ToString((byte)Value));

        }

        public void SetDecimal(string EnvvarName, decimal? Value)
        {
            SetStringEnv(EnvvarName, XmlConvert.ToString((decimal)Value));

        }

        public void SetSingle(string EnvvarName, float? Value)
        {
            SetStringEnv(EnvvarName, XmlConvert.ToString((float)Value));

        }

        public void SetString(string EnvvarName, string? Value)
        {
            SetStringEnv(EnvvarName,Value);
        }

        public int? GetInt(string EnvvarName)
        {
            return XmlConvert.ToInt32(GetStringEnv(EnvvarName));
        }

        public void SetInt(string EnvvarName, int? Value)
        {
            SetStringEnv(EnvvarName, XmlConvert.ToString((int)Value));

        }

        public E? GetEnum<E>(string EnvvarName) where E : struct
        {
            return Enum.Parse<E>(GetStringEnv(EnvvarName));
        }

        public void SetEnum<E>(string EnvvarName, E? Value) where E : struct
        {
            SetStringEnv(EnvvarName, Enum.GetName(typeof(E), Value));
        }
    }
}
