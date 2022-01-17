#if !NET_DOTS
using System.IO;

namespace Unity.Serialization.Json.Adapters
{
    partial class JsonAdapter :
        IJsonAdapter<DirectoryInfo>,
        IJsonAdapter<FileInfo>
    {
        void IJsonAdapter<DirectoryInfo>.Serialize(JsonStringBuffer writer, DirectoryInfo value)
        {
            if (null == value) 
                writer.Write("null");
            else 
                writer.WriteEncodedJsonString(value.GetRelativePath());
        }

        DirectoryInfo IJsonAdapter<DirectoryInfo>.Deserialize(SerializedValueView view)
        {
            return view.AsStringView().Equals("null") ? null : new DirectoryInfo(view.ToString());
        }

        void IJsonAdapter<FileInfo>.Serialize(JsonStringBuffer writer, FileInfo value)
        {
            if (null == value) 
                writer.Write("null");
            else 
                writer.WriteEncodedJsonString(value.GetRelativePath());
        }

        FileInfo IJsonAdapter<FileInfo>.Deserialize(SerializedValueView view)
        {
            return view.AsStringView().Equals("null") ? null : new FileInfo(view.ToString());
        }
    }
}
#endif