using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NiceIO;

namespace Unity.Build.DotsRuntime
{
    internal class BuildConfiguration
    {
        static List<Type> m_HasComponents = new List<Type>();

        public static bool HasComponent<T>()
        {
            return m_HasComponents.IndexOf(typeof(T)) != -1;
        }

        public static void Read(NPath file, Type configType)
        {
            if (!file.FileExists())
            {
                return;
            }
            var json = file.ReadAllText();
            var jarrays = JArray.Parse(json);
            for (int i = 0; i < 2; ++i)
            {
                var jarray = jarrays[i];
                foreach (var jobject in jarray)
                {
                    var type = jobject["$type"].Value<string>();
                    var proptype = Type.GetType(type.Substring(0, type.IndexOf(',')));
                    if (i == 0)
                    {
                        m_HasComponents.Add(proptype);
                    }

                    foreach (var settingProp in configType.GetProperties())
                    {
                        if (settingProp.PropertyType != proptype)
                        {
                            continue;
                        }
                        var settingValue = Convert.ChangeType(Activator.CreateInstance(proptype), proptype);
                        foreach (var jo in jobject.Children().OfType<JProperty>())
                        {
                            if (jo.Name == "$type")
                            {
                                continue;
                            }
                            var property = proptype.GetProperty(jo.Name);
                            var field = proptype.GetField(jo.Name);
                            var fieldType = property != null ? property.PropertyType : (field != null ? field.FieldType : null);
                            if (fieldType == null)
                            {
                                continue;
                            }
                            Object fieldValue = null;
                            if (!fieldType.IsArray)
                            {
                                if (fieldType == typeof(Version))
                                {
                                    // Newtonsoft JSON cannot deserialize System.Version type from string automatically
                                    fieldValue = new Version(jo.ToObject<String>());
                                }
                                else
                                {
                                    fieldValue = jo.ToObject(fieldType);
                                }
                            }
                            else
                            {
                                var proparray = jo.Value as JArray;
                                if (proparray == null)
                                {
                                    continue;
                                }
                                var array = Array.CreateInstance(fieldType.GetElementType(), proparray.Count);
                                for (int j = 0; j < proparray.Count; ++j)
                                {
                                    array.SetValue(proparray[j].ToObject(fieldType.GetElementType()), j);
                                }
                                fieldValue = array;
                            }
                            if (property != null)
                            {
                                property.SetValue(settingValue, fieldValue);
                            }
                            else
                            {
                                field.SetValue(settingValue, fieldValue);
                            }
                        }
                        settingProp.SetValue(null, settingValue);
                    }
                }
            }
        }
    }
}

