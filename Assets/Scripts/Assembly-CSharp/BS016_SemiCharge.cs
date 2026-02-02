using UnityEngine;

public class BS016_SemiCharge : MonoBehaviour
{
	private Vector3 rotate = new Vector3(635.3f, 0f, 0f);

	private Transform _modelTransform;

	private void Start()
	{
		_modelTransform = OrangeBattleUtility.FindChildRecursive(base.transform, "model", true);
	}

	public void Update()
	{
		_modelTransform.Rotate(rotate * Time.deltaTime);
	}
}
