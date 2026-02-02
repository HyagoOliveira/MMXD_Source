using CallbackDefs;
using UnityEngine;
using enums;

public class CharacterAnimatorStandBy : MonoBehaviour
{
	private class AnimatorHash
	{
		public static int HASH_01 = Animator.StringToHash("1");

		public static int HASH_02 = Animator.StringToHash("2");

		public static int HASH_03 = Animator.StringToHash("3");
	}

	private readonly string PART_HAND = "HandMesh_L";

	private readonly string PART_HAND_OLD = "L_HandMesh";

	private string animatorName = string.Empty;

	private string modelName = string.Empty;

	private RuntimeAnimatorController runtimeAnimatorController;

	private Animator animator;

	private CharacterMaterial[] characterMaterials;

	private GameObject weapon;

	private int nowIdx = -1;

	private float[] extraSize;

	private bool handEnable = true;

	private string[] clips = new string[3];

	private string nowClipName = string.Empty;

	private int loopCount;

	public bool IsSpecialPos { get; set; }

	private void Awake()
	{
		animator = GetComponent<Animator>();
		characterMaterials = GetComponentsInChildren<CharacterMaterial>();
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UI_CHARACTER_POS, ChangeCharacterPos);
	}

	public void ChangeCharacterPos()
	{
		if (!IsSpecialPos)
		{
			nowIdx++;
			if (nowIdx > 3)
			{
				nowIdx = 1;
			}
			if (loopCount < 3 && nowClipName == clips[nowIdx - 1])
			{
				loopCount++;
				ChangeCharacterPos();
			}
			else
			{
				animator.Play(GetAniHash(nowIdx), -1, 0f);
				nowClipName = clips[nowIdx - 1];
				loopCount = 0;
			}
		}
	}

	public void Init(string p_modelName, string p_animatorName, RuntimeAnimatorController p_runtimeAnimatorController, Animator p_animator, int p_id = 1, float[] p_extraSize = null)
	{
		modelName = p_modelName;
		animatorName = p_animatorName;
		runtimeAnimatorController = p_runtimeAnimatorController;
		animator = p_animator;
		extraSize = p_extraSize;
		nowIdx = p_id;
	}

	public void SetWeapon(GameObject p_weapon, WEAPON_TABLE p_weaponData)
	{
		if ((bool)weapon)
		{
			CharacterMaterial cmOld = weapon.GetComponent<CharacterMaterial>();
			cmOld.ChangeDissolveTime(0.3f);
			cmOld.Disappear(delegate
			{
				Object.DestroyImmediate(cmOld.gameObject);
			});
		}
		weapon = Object.Instantiate(p_weapon);
		if (IsSpecialPos)
		{
			OrangeBattleUtility.SetWeaponParentSp(weapon.transform, p_weaponData.n_TYPE, base.gameObject.transform);
		}
		else
		{
			Transform[] target = base.gameObject.GetComponentsInChildren<Transform>(true);
			OrangeBattleUtility.SetWeaponParent(weapon.transform, p_weaponData.n_TYPE, ref target);
			weapon.transform.localScale = new Vector3(extraSize[0], extraSize[0], extraSize[0]);
			SetHandMesh(OrangeBattleUtility.GetHandMeshEnable(p_weaponData.n_TYPE));
		}
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.scrEffect == 2)
		{
			Transform transform = OrangeBattleUtility.FindChildRecursive(weapon.transform, "efx", true);
			if (transform != null)
			{
				transform.gameObject.SetActive(true);
				if (extraSize.Length != 0 && extraSize[0] == 0.55f)
				{
					ParticleSystem[] componentsInChildren = transform.GetComponentsInChildren<ParticleSystem>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						ParticleSystem.MainModule main = componentsInChildren[i].main;
						if (main.simulationSpace == ParticleSystemSimulationSpace.World)
						{
							main.simulationSpace = ParticleSystemSimulationSpace.Local;
						}
					}
				}
			}
		}
		CharacterMaterial component = weapon.GetComponent<CharacterMaterial>();
		component.SetBaseRenderForUI();
		component.ChangeDissolveTime(0.3f);
		component.Appear();
		MonoBehaviourSingleton<OrangeBattleUtility>.Instance.WeaponForceRotate(weapon);
	}

	private int GetAniHash(int id)
	{
		switch (id)
		{
		default:
			return AnimatorHash.HASH_01;
		case 2:
			return AnimatorHash.HASH_02;
		case 3:
			return AnimatorHash.HASH_03;
		}
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<int>(EventManager.ID.UPDATE_RENDER_WEAPON, UpdateWeapon);
		if (nowIdx > 0)
		{
			Init(modelName, animatorName, runtimeAnimatorController, animator, nowIdx, extraSize);
		}
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<int>(EventManager.ID.UPDATE_RENDER_WEAPON, UpdateWeapon);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UI_CHARACTER_POS, ChangeCharacterPos);
	}

	private void UpdateWeapon(int weaponId)
	{
		UpdateWeapon(weaponId, false, null);
	}

	public void UpdateWeapon(int weaponId, bool updatePos, Callback p_cb)
	{
		WEAPON_TABLE equipWeapon = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[weaponId];
		if (IsSpecialPos)
		{
			string[] bnudles2 = new string[1] { AssetBundleScriptableObject.Instance.m_newmodel_weapon + equipWeapon.s_MODEL };
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(bnudles2, delegate
			{
				SetWeapon(MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<GameObject>(bnudles2[0], equipWeapon.s_MODEL + "_U.prefab"), equipWeapon);
				p_cb.CheckTargetToInvoke();
			}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
			return;
		}
		WeaponType weaponType = (WeaponType)equipWeapon.n_TYPE;
		string[] bnudles = new string[3]
		{
			string.Empty,
			AssetBundleScriptableObject.Instance.m_newmodel_weapon + equipWeapon.s_MODEL,
			string.Empty
		};
		clips = OrangeAnimatonHelper.GetStandByClips(modelName, animatorName, weaponType, out bnudles[0]);
		OverrideStandClips(weaponType, ref bnudles[0], ref clips);
		string[] clipsEye = OrangeAnimatonHelper.GetEyesClips(animatorName, out bnudles[2]);
		Vector3 tempPosCache = base.transform.localPosition;
		if (updatePos)
		{
			base.transform.localPosition = new Vector3(10000f, 10000f, 0f);
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(bnudles, delegate
		{
			if (bnudles[0] != string.Empty)
			{
				AnimatorOverrideController overrideController = OrangeAnimatonHelper.OverrideRuntimeAnimClip(ref runtimeAnimatorController, ref bnudles[0], ref clips);
				OrangeAnimatonHelper.OverrideRuntimeAnimClip("face_", ref overrideController, ref bnudles[2], ref clipsEye);
				animator.runtimeAnimatorController = overrideController;
			}
			SetWeapon(MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<GameObject>(bnudles[1], equipWeapon.s_MODEL + "_U.prefab"), equipWeapon);
			animator.Play(GetAniHash(nowIdx), -1, 0f);
			base.transform.localPosition = tempPosCache;
			p_cb.CheckTargetToInvoke();
		}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
	}

	private void SetHandMesh(bool active, float time = 0.5f)
	{
		if (active == handEnable)
		{
			return;
		}
		handEnable = active;
		if (active)
		{
			CharacterMaterial[] array = characterMaterials;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AppearSelect(PART_HAND_OLD, 0f);
			}
			array = characterMaterials;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AppearSelect(PART_HAND, 0f);
			}
		}
		else
		{
			CharacterMaterial[] array = characterMaterials;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DisappearSelect(PART_HAND_OLD, time);
			}
			array = characterMaterials;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DisappearSelect(PART_HAND, time);
			}
		}
	}

	protected virtual void OverrideStandClips(WeaponType weaponType, ref string originalBundleName, ref string[] originalClips)
	{
	}
}
