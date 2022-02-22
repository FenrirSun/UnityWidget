using UnityEngine;
using System.Collections;
using System;

public static class ExtranalHelper
{
    public static T EnsureComponent<T>(this GameObject go) where T : Component
    {
        T result = go.GetComponent<T>();

        if (result == null)
            result = go.AddComponent<T>();

        return result;
    }

    public static string LastSubstringStartsOfAny(this string source, params string[] value)
    {
        return StartsOfAny(source, System.StringComparison.CurrentCulture, false, value);
    }
    public static string LastSubstringStartsOfAny(this string source, params char[] value)
    {
        return StartsOfAny(source, System.StringComparison.CurrentCulture, false, new string(value));
    }
    public static string LastSubstringStartsOfAny(this string source, System.StringComparison comparison, params string[] value)
    {
        return StartsOfAny(source, comparison, false, value);
    }
    public static string LastSubstringEndsOfAny(this string source, params char[] value)
    {
        return EndsWithAny(source, System.StringComparison.CurrentCulture, false, value);
    }
    public static string LastSubstringEndsOfAny(this string source, System.StringComparison comparison, params char[] value)
    {
        return EndsWithAny(source, comparison, false, value);
    }
    private static string EndsWithAny(this string source, System.StringComparison comparison, bool first, params char[] value)
    {
        if (source == null)
            return null;
        int index = -1;
        if (comparison == StringComparison.CurrentCultureIgnoreCase ||
comparison == StringComparison.InvariantCultureIgnoreCase ||
comparison == StringComparison.OrdinalIgnoreCase)
        {
            source = source.ToLower();
            for (int i = 0; i < value.Length; i++)
                value[i] = (value[i] + "").ToLower()[0];
        }

        for (int i = 0, n; i < value.Length; i++)
        {
            if (first)
                n = source.IndexOf(value[i]);
            else
                n = source.LastIndexOf(value[i]);
            if (n < 0)
                continue;
            if (n > index)
                index = n;
        }
        if (index < 0)
            return source;

        return source.Substring(0, index);

    }
    private static string StartsOfAny(this string source, System.StringComparison comparison, bool first, params string[] value)
    {
        if (source == null)
            return null;
        int index = -1;
        int strIndex = 0;
        for (int i = 0, n; i < value.Length; i++)
        {
            if (first)
                n = source.IndexOf(value[i], comparison);
            else
                n = source.LastIndexOf(value[i], comparison);
            if (n < 0)
                continue;
            if (n > index)
            {
                index = n;
                strIndex = i;
            }
        }
        if (index < 0)
            return source;

        return source.Substring(index + value[strIndex].Length);
    }

    public static T GetResources<T>(string path) where T : UnityEngine.Object, new()
    {
        T t = new T();

        t = Resources.Load(path) as T;

        return t;
    }
}
