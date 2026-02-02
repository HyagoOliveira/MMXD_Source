using UnityEngine;

public class BS034_ClawController : MonoBehaviour
{
	private Transform _transform;

	private Transform _TargetPos;

	private Transform[] _Chains;

	private Transform _modelTransform;

	private int _chainLength;

	public Vector3 dir = Vector3.right;

	private void Start()
	{
		_transform = base.transform;
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_TargetPos = OrangeBattleUtility.FindChildRecursive(ref target, "TargetPos");
		_modelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Root");
		_Chains = OrangeBattleUtility.FindAllChildRecursive(ref target, "ChainBone");
		_chainLength = _Chains.Length;
	}

	private void Update()
	{
		float num = Vector3.Distance(_modelTransform.position, _TargetPos.position) / (float)_chainLength;
		_modelTransform.LookAt(_TargetPos, dir);
		Transform[] chains = _Chains;
		for (int i = 0; i < chains.Length; i++)
		{
			chains[i].localPosition = Vector3.left * num;
		}
	}

	public void SetTargetLocalPosition(Vector3 pos)
	{
		_TargetPos.localPosition = pos;
	}
}
