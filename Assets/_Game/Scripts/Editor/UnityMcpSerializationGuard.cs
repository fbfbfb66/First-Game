using System;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

/// <summary>Protects MCP component inspection from recursive Unity value properties.</summary>
[InitializeOnLoad]
internal static class UnityMcpSerializationGuard
{
    static UnityMcpSerializationGuard()
    {
        // Install before editor update callbacks can replay pending MCP requests.
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType("Unity.AI.MCP.Editor.Helpers.GameObjectSerializer");
            if (type == null) continue;

            try
            {
                var field = type.GetField("_outputSerializer", BindingFlags.Static | BindingFlags.NonPublic);
                if (field?.GetValue(null) is not JsonSerializer serializer)
                {
                    Debug.LogWarning("Unity MCP serialization guard could not find the component output serializer. Review the workaround before inspecting RectTransform components.");
                    return;
                }

                serializer.Converters.Insert(0, new MatrixValueConverter());
                serializer.Converters.Insert(0, new TransformHandleConverter());
                Debug.Log("[UnityMcpSerializationGuard] Installed bounded Matrix4x4 and opaque TransformHandle serialization for MCP component inspection.");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Unity MCP serialization guard could not install: {exception.Message}. Avoid RectTransform component inspection until corrected.");
            }

            return;
        }
    }

    // Engine handles expose derived handles and matrices. Reflecting through
    // them as ordinary value objects can recurse without any reference loop.
    private sealed class TransformHandleConverter : JsonConverter
    {
        public override bool CanRead => false;
        public override bool CanConvert(Type objectType) => objectType.FullName == "UnityEngine.TransformHandle";

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue("UnityEngine.TransformHandle (opaque engine handle)");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("This converter only writes MCP inspection results.");
        }
    }

    private sealed class MatrixValueConverter : JsonConverter<Matrix4x4>
    {
        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    writer.WritePropertyName($"m{row}{column}");
                    writer.WriteValue(value[row, column]);
                }
            }
            writer.WriteEndObject();
        }

        public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("This converter only writes MCP inspection results.");
        }
    }
}
