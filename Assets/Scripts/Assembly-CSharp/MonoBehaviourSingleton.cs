using UnityEngine;

public abstract class MonoBehaviourSingleton<T> : MonoBehaviour, IReflector where T : MonoBehaviour
{
	protected static T _instance;

	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = (T)Object.FindObjectOfType(typeof(T));
				if (_instance == null)
				{
					_instance = new GameObject(typeof(T).Name).AddComponent<T>();
				}
				Object.DontDestroyOnLoad(_instance);
			}
			return _instance;
		}
	}

	protected virtual void OnDestroy()
	{
		_instance = null;
	}
}
