using System.Collections;
using UnityEngine;

public class StageOnceCoroutine : MonoBehaviour
{
	public class StageOnceParam
	{
		public object param1;

		public object param2;

		public object param3;
	}

	public delegate void callfunc(StageOnceParam TestSOP);

	private static StageOnceCoroutine _Instance;

	public callfunc tcallfunc;

	public StageOnceParam tStageOnceParam;

	public IEnumerator waitfun;

	public static StageOnceCoroutine Instance
	{
		get
		{
			if (_Instance == null)
			{
				GameObject obj = new GameObject();
				_Instance = obj.AddComponent<StageOnceCoroutine>();
				Object.DontDestroyOnLoad(obj);
			}
			return _Instance;
		}
	}

	public void StartOnceCoroutine()
	{
		StartCoroutine("CallAndDelete");
	}

	public IEnumerator CallAndDelete()
	{
		yield return waitfun;
		if (tcallfunc != null)
		{
			tcallfunc(tStageOnceParam);
		}
		Object.Destroy(base.gameObject);
	}
}
