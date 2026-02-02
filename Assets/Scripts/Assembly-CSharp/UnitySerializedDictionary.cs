using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[SerializeField]
	public List<TKey> _keys = new List<TKey>();

	[SerializeField]
	public List<TValue> _values = new List<TValue>();

	public void OnBeforeSerialize()
	{
		_keys.Clear();
		_values.Clear();
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, TValue> current = enumerator.Current;
				_keys.Add(current.Key);
				_values.Add(current.Value);
			}
		}
	}

	public void OnAfterDeserialize()
	{
		Clear();
		for (int i = 0; i != Math.Min(_keys.Count, _values.Count); i++)
		{
			Add(_keys[i], _values[i]);
		}
	}
}
