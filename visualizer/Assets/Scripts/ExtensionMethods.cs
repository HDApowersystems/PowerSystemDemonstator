using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class ExtensionMethods
{

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
    public static float RemapClamp(this float value, float from1, float to1, float from2, float to2)
    {
        return Mathf.Clamp(Remap(value, from1, to1, from2, to2), from2, to2);
    }

    public static T CloneObject<T>(this T toClone)
    {
        var serializer = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            serializer.Serialize(ms, toClone);
            ms.Position = 0;
            return (T)serializer.Deserialize(ms);
        }
    }
}
