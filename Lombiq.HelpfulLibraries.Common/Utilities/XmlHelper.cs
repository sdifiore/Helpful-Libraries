﻿using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Lombiq.HelpfulLibraries.Common.Utilities;

/// <summary>
/// Utility class to streamline XML serialization.
/// </summary>
public static class XmlHelper
{
    /// <summary>
    /// Deserializes the provided <paramref name="xml"/> to the expected <paramref name="outputType"/>. If <paramref
    /// name="xml"/> is null or empty it just shortcuts by returning <see langword="default"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">An error occurred during deserialization.</exception>
    public static object Deserialize(string xml, Type outputType, params Type[] extraTypes)
    {
        if (string.IsNullOrEmpty(xml)) return default;

        var serializer = new XmlSerializer(outputType, extraTypes);

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader);

        return serializer.Deserialize(xmlReader);
    }

    /// <summary>
    /// Deserializes the provided <paramref name="xml"/> to the expected type using <see cref="Deserialize"/>. If the
    /// value to be returned is not <typeparamref name="T"/>, it returns <see langword="default"/> instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">An error occurred during deserialization.</exception>
    public static T Deserialize<T>(string xml) => Deserialize(xml, typeof(T)) is T result ? result : default;

    /// <summary>
    /// Tries to <see cref="Deserialize"/> the provided <paramref name="xml"/> into <paramref name="result"/>.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the deserialization was successful, <see langword="false"/> if the resulting value is
    /// not <typeparamref name="T"/> or if an exception was raised.
    /// </returns>
    public static bool TryDeserialize<T>(string xml, out T result)
    {
        result = default;

        try
        {
            if (Deserialize(xml, typeof(T)) is not T deserialized) return false;

            result = deserialized;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes <paramref name="data"/>.
    /// </summary>
    public static string Serialize<T>(T data, params Type[] extraTypes)
    {
        var serializer = new XmlSerializer(typeof(T), extraTypes);
        var stringBuilder = new StringBuilder();

        using var writer = new StringWriter(stringBuilder);
        serializer.Serialize(writer, data);

        return stringBuilder.ToString();
    }
}
