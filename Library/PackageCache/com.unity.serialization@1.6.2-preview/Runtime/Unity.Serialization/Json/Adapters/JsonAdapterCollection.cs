#if !NET_DOTS
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Properties;
using Unity.Properties.Internal;
using Unity.Serialization.Json.Unsafe;

namespace Unity.Serialization.Json.Adapters
{
    struct JsonAdapterCollection
    {
        public JsonAdapter InternalAdapter;
        public List<IJsonAdapter> Global;
        public List<IJsonAdapter> UserDefined;

        public bool TrySerialize<TValue>(JsonWriter writer, ref TValue value)
        {
            if (null != UserDefined && UserDefined.Count > 0)
            {
                foreach (var adapter in UserDefined)
                {
                    if (TrySerializeAdapter(adapter, writer, ref value))
                    {
                        return true;
                    }
                }
            }

            if (null != Global && Global.Count > 0)
            {
                foreach (var adapter in Global)
                {
                    if (TrySerializeAdapter(adapter, writer, ref value))
                    {
                        return true;
                    }
                }
            }

            if (TrySerializeAdapter(InternalAdapter, writer, ref value))
            {
                return true;
            }

#if UNITY_EDITOR
            if (TrySerializeLazyLoadReference(writer, ref value))
            {
                return true;
            }
#endif

            return false;
        }

        static bool TrySerializeAdapter<TValue>(IJsonAdapter adapter, JsonWriter writer, ref TValue value)
        {
            if (adapter is IJsonAdapter<TValue> typed)
            {
                using (var buffer = new JsonStringBuffer(16, Allocator.TempJob))
                {
                    typed.Serialize(buffer, value);
                    
                    unsafe
                    {
                        writer.AsUnsafe().WriteValueLiteral(buffer.GetUnsafePtr(), buffer.Length);
                    }
                }
                return true;
            }

            if (adapter is Adapters.Contravariant.IJsonAdapter<TValue> typedContravariant)
            {
                using (var buffer = new JsonStringBuffer(16, Allocator.TempJob))
                {
                    typedContravariant.Serialize(buffer, value);
                    
                    unsafe
                    {
                        writer.AsUnsafe().WriteValueLiteral(buffer.GetUnsafePtr(), buffer.Length);
                    }
                }
                return true;
            }

            return false;
        }

        public bool TryDeserialize<TValue>(UnsafeValueView view, ref TValue value, List<DeserializationEvent> events)
        {
            if (null != UserDefined && UserDefined.Count > 0)
            {
                foreach (var adapter in UserDefined)
                {
                    if (TryDeserializeAdapter(adapter, view, ref value, events))
                    {
                        return true;
                    }
                }
            }

            if (null != Global && Global.Count > 0)
            {
                foreach (var adapter in Global)
                {
                    if (TryDeserializeAdapter(adapter, view, ref value, events))
                    {
                        return true;
                    }
                }
            }

            if (TryDeserializeAdapter(InternalAdapter, view, ref value, events))
            {
                return true;
            }

#if UNITY_EDITOR
            if (TryDeserializeLazyLoadReference(view.AsSafe(), ref value, events))
            {
                return true;
            }
#endif

            return false;
        }

        static bool TryDeserializeAdapter<TValue>(IJsonAdapter adapter, UnsafeValueView view, ref TValue value, List<DeserializationEvent> events)
        {
            if (adapter is IJsonAdapter<TValue> typed)
            {
                try
                {
                    value = typed.Deserialize(view.AsSafe());
                }
                catch (Exception e)
                {
                    events.Add(new DeserializationEvent(EventType.Exception, e));
                }
                return true;
            }

            if (adapter is Adapters.Contravariant.IJsonAdapter<TValue> typedContravariant)
            {
                try
                {
                    // @TODO Type checking on return value.
                    value = (TValue)typedContravariant.Deserialize(view.AsSafe());
                }
                catch (Exception e)
                {
                    events.Add(new DeserializationEvent(EventType.Exception, e));
                }
                return true;
            }

            return false;
        }

#if UNITY_EDITOR
        const string k_LazyLoadReference_InstanceID = "m_InstanceID";
        static readonly string s_EmptyGlobalObjectId = new UnityEditor.GlobalObjectId().ToString();

        static bool TrySerializeLazyLoadReference<TValue>(JsonWriter writer, ref TValue value)
        {
            if (!RuntimeTypeInfoCache<TValue>.IsLazyLoadReference)
            {
                return false;
            }

            var instanceID = PropertyContainer.GetValue<TValue, int>(ref value, k_LazyLoadReference_InstanceID);
            writer.WriteValue(UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(instanceID).ToString());
            return true;
        }

        static bool TryDeserializeLazyLoadReference<TValue>(SerializedValueView view, ref TValue value, List<DeserializationEvent> events)
        {
            if (!RuntimeTypeInfoCache<TValue>.IsLazyLoadReference)
            {
                return false;
            }

            if (view.Type != TokenType.String)
            {
                return false;
            }

            var json = view.AsStringView().ToString();
            if (json == s_EmptyGlobalObjectId) // Workaround issue where GlobalObjectId.TryParse returns false for empty GlobalObjectId
            {
                return true;
            }

            if (UnityEditor.GlobalObjectId.TryParse(json, out var id))
            {
#if UNITY_2020_1_OR_NEWER
                var instanceID = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToInstanceIDSlow(id);
                PropertyContainer.SetValue(ref value, k_LazyLoadReference_InstanceID, instanceID);
#else
                var asset = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                if ((asset == null || !asset) && !id.assetGUID.Empty())
                {
                    throw new InvalidOperationException($"An error occured while deserializing asset reference GUID=[{id.assetGUID.ToString()}]. Asset is not yet loaded and will result in a null reference.");
                }

                var instanceID = asset.GetInstanceID();
                PropertyContainer.SetValue(ref value, k_LazyLoadReference_InstanceID, instanceID);
#endif
                return true;
            }

            events.Add(new DeserializationEvent(EventType.Error, $"An error occured while deserializing asset reference Value=[{json}]."));
            return false;
        }
#endif
    }
}
#endif
