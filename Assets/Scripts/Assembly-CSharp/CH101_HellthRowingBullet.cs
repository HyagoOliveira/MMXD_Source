using UnityEngine;

public class CH101_HellthRowingBullet : RollingBullet
{
	[SerializeField]
	private ParticleSystem _psBody;

	[SerializeField]
	private ParticleSystem[] _psRims;

	[SerializeField]
	private ParticleSystem _psLine01;

	[SerializeField]
	private ParticleSystem _psLine02;

	[SerializeField]
	private string[] _exUseSE = new string[0];

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		if (_exUseSE.Length > 1 && !isMuteSE)
		{
			base.SoundSource.PlaySE(_exUseSE[0], _exUseSE[1]);
		}
	}

	protected override void UpdateParticleRotationDirection(bool isRight)
	{
		PlaySE("HitSE", "ht_guard03");
		if (isRight)
		{
			if ((bool)_psBody)
			{
				_psBody.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
			}
			ParticleSystem[] psRims = _psRims;
			for (int i = 0; i < psRims.Length; i++)
			{
				psRims[i].transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
			}
			if ((bool)_psLine01)
			{
				float z = Mathf.Abs(_psLine01.transform.localPosition.z) * -1f;
				float y = Mathf.Abs(_psLine01.transform.localEulerAngles.y);
				_psLine01.transform.localPosition = new Vector3(_psLine01.transform.localPosition.x, _psLine01.transform.localPosition.y, z);
				_psLine01.transform.localEulerAngles = new Vector3(_psLine01.transform.localEulerAngles.x, y, _psLine01.transform.localEulerAngles.z);
			}
			if ((bool)_psLine02)
			{
				_psLine02.transform.localScale = new Vector3(0.8f, 0.8f, -0.8f);
			}
		}
		else
		{
			if ((bool)_psBody)
			{
				_psBody.transform.localScale = new Vector3(-0.8f, 0.8f, 0.8f);
			}
			ParticleSystem[] psRims = _psRims;
			for (int i = 0; i < psRims.Length; i++)
			{
				psRims[i].transform.localScale = new Vector3(-0.8f, 0.8f, 0.8f);
			}
			if ((bool)_psLine01)
			{
				float z2 = Mathf.Abs(_psLine01.transform.localPosition.z);
				float y2 = Mathf.Abs(_psLine01.transform.localEulerAngles.y) * -1f;
				_psLine01.transform.localPosition = new Vector3(_psLine01.transform.localPosition.x, _psLine01.transform.localPosition.y, z2);
				_psLine01.transform.localEulerAngles = new Vector3(_psLine01.transform.localEulerAngles.x, y2, _psLine01.transform.localEulerAngles.z);
			}
			if ((bool)_psLine02)
			{
				_psLine02.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
			}
		}
	}
}
