using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class Option
{
	public delegate void ValueUpdatedDelegate<T>(T value);

	private static Dictionary<string, object> options = new Dictionary<string, object>();

	private static Dictionary<string, ValueUpdatedDelegate<int>> intCallbacks = new Dictionary<string, ValueUpdatedDelegate<int>>();
	private static Dictionary<string, ValueUpdatedDelegate<float>> floatCallbacks = new Dictionary<string, ValueUpdatedDelegate<float>>();
	private static Dictionary<string, ValueUpdatedDelegate<string>> stringCallbacks = new Dictionary<string, ValueUpdatedDelegate<string>>();


	static Option()
	{
		Load();
	}

	private static void Load()
	{
		if (PlayerPrefs.HasKey("keys"))
		{
			var keys = PlayerPrefs.GetString("keys");
			//Debug.Log($"Keys found {keys}");
			var stringKeys = keys.Split(';');
			foreach(string keyAndType in stringKeys)
			{
				var values = keyAndType.Split(':');
				var key = values[0];
				if (key.Length > 0)
				{
					var type = Type.GetType(values[1]);
					if (type == typeof(int))
						options.Add(key, PlayerPrefs.GetInt(key));
					if (type == typeof(float))
						options.Add(key, PlayerPrefs.GetFloat(key));
					if (type == typeof(string))
						options.Add(key, PlayerPrefs.GetString(key));
				}
			}
		}
	}

	private static void CheckType<T>()
	{
		if (typeof(T) != typeof(int) && typeof(T) != typeof(float) && typeof(T) != typeof(string))
			throw new ArgumentException($"Type {typeof(T)} not int, float or string");
	}

	private static ValueUpdatedDelegate<T> Set<T>(string key, T defaultValue, bool forced = true, ValueUpdatedDelegate<T> updateDelegate = null)
	{
		CheckType<T>();
		bool addKey = false;
		ValueUpdatedDelegate<T> del = null;

		if (!options.ContainsKey(key))
		{
			options.Add(key, defaultValue);
			addKey = true;
		}
		else
		{
			if(forced)
				options[key] = defaultValue;
		}

		Type t = null;
		if (typeof(T) == typeof(int)) {
			PlayerPrefs.SetInt(key, (int)options[key]);
			t = typeof(int);
			if (updateDelegate != null)
			{
				if (addKey)
					intCallbacks.Add(key, updateDelegate as ValueUpdatedDelegate<int>);
				else
					intCallbacks[key] = updateDelegate as ValueUpdatedDelegate<int>;
			}
			del = intCallbacks[key] as ValueUpdatedDelegate<T>;
		}
		if (typeof(T) == typeof(float)) { 
			PlayerPrefs.SetFloat(key, (float)options[key]);
			t = typeof(float);
			if (updateDelegate != null)
			{
				if (addKey)
					floatCallbacks.Add(key, updateDelegate as ValueUpdatedDelegate<float>);
				else
					floatCallbacks[key] = updateDelegate as ValueUpdatedDelegate<float>;
			}
			del = floatCallbacks[key] as ValueUpdatedDelegate<T>;
		}
		if (typeof(T) == typeof(string)) { 
			PlayerPrefs.SetString(key, (string)options[key]);
			t = typeof(string);
			if (updateDelegate != null)
			{
				if (addKey)
					stringCallbacks.Add(key, updateDelegate as ValueUpdatedDelegate<string>);
				else
					stringCallbacks[key] = updateDelegate as ValueUpdatedDelegate<string>;
			}
			del = floatCallbacks[key] as ValueUpdatedDelegate<T>;
		}

		var sb = new StringBuilder(options.Count);
		foreach (string s in options.Keys)
			sb.Append($"{s}:{t.ToString()};");
		PlayerPrefs.SetString("keys", sb.ToString());

		PlayerPrefs.Save();
		return del;
	}

	private static object Get<T>(string key)
	{
		CheckType<T>();

		if (options.ContainsKey(key))
		{
			return (T)options[key];
		}
		else
		{
			if (PlayerPrefs.HasKey(key))
			{
				object o = null;

				if (typeof(T) == typeof(int))
					o = PlayerPrefs.GetInt(key);
				if (typeof(T) == typeof(float))
					o = PlayerPrefs.GetFloat(key);
				if (typeof(T) == typeof(string))
					o = PlayerPrefs.GetString(key);

				if (o != null)
					options.Add(key, o);
				return o;
			}
			else
			{
				return null;
			}
		}
	}

	

	public static void Register<T>(string key, T defaultValue, ValueUpdatedDelegate<T> updateDelegate)
	{
		CheckType<T>();

		Set<T>(key, defaultValue, false, updateDelegate);
	}

	public static void UpdateFromGameplay<T>(string key, T value)
	{
		CheckType<T>();

		Set<T>(key, value);
	}

	public static void UpdateFromOption<T>(string key, T value)
	{
		CheckType<T>();

		var updateDelegate = Set<T>(key, value);

		updateDelegate(value);
	}

	public static bool TryGet<T>(string key, out T value)
	{
		var o = Get<T>(key);
		if (o != null)
			value = (T)o;
		else
			value = default(T);

		return o != null;
	}
}

