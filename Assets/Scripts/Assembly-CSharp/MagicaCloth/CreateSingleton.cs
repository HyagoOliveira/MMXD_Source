using UnityEngine;

namespace MagicaCloth
{
	public abstract class CreateSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T instance;

		private static T initInstance;

		private static bool isDestroy;

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					instance = Object.FindObjectOfType<T>();
					if (instance == null && Application.isPlaying)
					{
						instance = new GameObject(typeof(T).Name).AddComponent<T>();
					}
				}
				InitInstance();
				return instance;
			}
		}

		private static void InitInstance()
		{
			if (initInstance == null && instance != null && Application.isPlaying)
			{
				Object.DontDestroyOnLoad(instance.gameObject);
				(instance as CreateSingleton<T>).InitSingleton();
				initInstance = instance;
			}
		}

		public static bool IsInstance()
		{
			if (instance != null)
			{
				return !isDestroy;
			}
			return false;
		}

		protected virtual void Awake()
		{
			if (instance == null)
			{
				instance = this as T;
				InitInstance();
			}
			else
			{
				(instance as CreateSingleton<T>).DuplicateDetection(this as T);
				Object.Destroy(base.gameObject);
			}
		}

		protected virtual void OnDestroy()
		{
			if (instance == this)
			{
				isDestroy = true;
			}
		}

		protected virtual void DuplicateDetection(T duplicate)
		{
		}

		protected abstract void InitSingleton();
	}
}
