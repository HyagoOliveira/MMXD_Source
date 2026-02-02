using System.Collections.Generic;
using UnityEngine;

public class SlashColliderEfx : MonoBehaviour
{
	private Camera _mainCamera;

	public bool AlwaysFaceCamera = true;

	public List<Transform> _ignoreList;

	public Animator SlashAnimator;

	public ParticleSystem SlashParticle;

	private string hitEffectName;

	private int _tweenUid = -1;

	private void Awake()
	{
		_ignoreList = new List<Transform>();
		_mainCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
	}
}
