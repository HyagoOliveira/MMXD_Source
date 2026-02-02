using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CallbackDefs;
using Kino;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class RenderTextureObj : MonoBehaviour
{
	private enum RenderType
	{
		NONE = 0,
		CHARACTER = 1,
		WEAPON = 2,
		Enemy = 3
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass41_0
	{
		public RenderTextureObj _003C_003E4__this;

		public WEAPON_TABLE equipWeapon;

		public CHARACTER_TABLE character;

		public string debutModolName;

		public EmptyBlockUI emptyBlockUI;

		public string newModelName;

		public string animatorName;

		internal void _003CUpdateCharacter_003Eb__0(UnityEngine.Object obj)
		{
			_003C_003Ec__DisplayClass41_1 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass41_1
			{
				CS_0024_003C_003E8__locals1 = this,
				obj = obj
			};
			_003C_003E4__this.nLoadCount--;
			CS_0024_003C_003E8__locals0.go = null;
			CS_0024_003C_003E8__locals0.animator = null;
			CS_0024_003C_003E8__locals0.tRuntimeAnimatorController = null;
			if (!_003C_003E4__this.OnlyDebut && equipWeapon != null && character.n_SPECIAL_SHOWPOSE > 0)
			{
				_003C_003E4__this.nLoadCount++;
				_003C_003E4__this.CharacterDebutForceLoop = true;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("model/animator/empty", "empty2", delegate(RuntimeAnimatorController obj2)
				{
					_003C_003Ec__DisplayClass41_2 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass41_2
					{
						CS_0024_003C_003E8__locals2 = CS_0024_003C_003E8__locals0
					};
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount - 1;
					CS_0024_003C_003E8__locals0.tRuntimeAnimatorController = obj2;
					if (!int.TryParse(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.modelName.Substring(2, 3), out CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.characterId))
					{
						CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.characterId = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.character.n_ID;
					}
					CS_0024_003C_003E8__locals2.bnudles = new string[2]
					{
						string.Empty,
						AssetBundleScriptableObject.Instance.m_newmodel_weapon + CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.equipWeapon.s_MODEL
					};
					CS_0024_003C_003E8__locals2.clips = OrangeAnimatonHelper.GetUniqueDebutName(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.debutModolName, out CS_0024_003C_003E8__locals2.bnudles[0]);
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.UpdateBonusClip(ref CS_0024_003C_003E8__locals2.clips);
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount + 1;
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(CS_0024_003C_003E8__locals2.bnudles, delegate
					{
						CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount - 1;
						for (int num2 = CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.childCount - 1; num2 >= 0; num2--)
						{
							CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.GetChild(num2).gameObject.SetActive(false);
							UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.GetChild(num2).gameObject);
						}
						AnimatorOverrideController runtimeAnimatorController = OrangeAnimatonHelper.OverrideRuntimeAnimClip(ref CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.tRuntimeAnimatorController, ref CS_0024_003C_003E8__locals2.bnudles[0], ref CS_0024_003C_003E8__locals2.clips);
						if (CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.obj == null)
						{
							CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.go = new GameObject();
							CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.go.transform.SetParent(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition);
							CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.SetLoaded(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.emptyBlockUI);
						}
						else
						{
							CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.go = UnityEngine.Object.Instantiate(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.obj, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition, false) as GameObject;
							CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.go.transform.localScale = new Vector3(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.f_MODELSIZE, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.f_MODELSIZE, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.f_MODELSIZE);
							CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.animator = CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.go.GetComponent<Animator>();
							float[] characterTableExtraSize2 = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTableExtraSize(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.s_ModelExtraSize);
							if (characterTableExtraSize2.Length > 3)
							{
								CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.go.transform.localPosition = new Vector3(0f, characterTableExtraSize2[3], 0f);
							}
							else
							{
								CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.go.transform.localPosition = new Vector3(0f, 0f, 0f);
							}
							CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.animator.runtimeAnimatorController = runtimeAnimatorController;
							CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
							if (CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.CharacterDebutForceLoop)
							{
								CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.animator.Play("2");
							}
							CharacterAnimatorStandBy characterAnimatorStandBy2 = CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.go.AddOrGetComponent<CharacterAnimatorStandBy>();
							characterAnimatorStandBy2.IsSpecialPos = true;
							characterAnimatorStandBy2.Init(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.newModelName, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.animatorName, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.tRuntimeAnimatorController, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.animator, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.n_WEAPON_MOTION, characterTableExtraSize2);
							characterAnimatorStandBy2.SetWeapon(MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<GameObject>(CS_0024_003C_003E8__locals2.bnudles[1], CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.equipWeapon.s_MODEL + "_U.prefab"), CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.equipWeapon);
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_RANKING_CHARACTER_CHANGE);
							OrangeBattleUtility.ChangeLayersRecursively<Transform>(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.go.transform, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderLayer);
							for (int i = 0; i < CS_0024_003C_003E8__locals2.bnudles.Length; i++)
							{
								MonoBehaviourSingleton<AssetsBundleManager>.Instance.UnloadSingleBundleCache(CS_0024_003C_003E8__locals2.bnudles[i]);
							}
							CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.SetLoaded(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.emptyBlockUI);
						}
					}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
				});
				return;
			}
			if (!_003C_003E4__this.OnlyDebut && equipWeapon != null)
			{
				_003C_003E4__this.nLoadCount++;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("model/animator/empty", "empty", delegate(UnityEngine.Object obj2)
				{
					CS_0024_003C_003E8__locals0.tRuntimeAnimatorController = (RuntimeAnimatorController)obj2;
					for (int num = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.childCount - 1; num >= 0; num--)
					{
						UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.GetChild(num).gameObject);
					}
					CS_0024_003C_003E8__locals0.go = UnityEngine.Object.Instantiate(CS_0024_003C_003E8__locals0.obj, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition, false) as GameObject;
					CS_0024_003C_003E8__locals0.animator = CS_0024_003C_003E8__locals0.go.GetComponent<Animator>();
					float[] characterTableExtraSize = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTableExtraSize(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.character.s_ModelExtraSize);
					CS_0024_003C_003E8__locals0.go.AddOrGetComponent<CharacterAnimatonRandWink>().Setup(CS_0024_003C_003E8__locals0.animator);
					CS_0024_003C_003E8__locals0.go.transform.localScale = new Vector3(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.character.f_MODELSIZE, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.character.f_MODELSIZE, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.character.f_MODELSIZE);
					if (characterTableExtraSize.Length > 3)
					{
						CS_0024_003C_003E8__locals0.go.transform.localPosition = new Vector3(0f, characterTableExtraSize[3], 0f);
					}
					else
					{
						CS_0024_003C_003E8__locals0.go.transform.localPosition = new Vector3(0f, 0f, 0f);
					}
					CharacterAnimatorStandBy characterAnimatorStandBy = CS_0024_003C_003E8__locals0.go.AddOrGetComponent<CharacterAnimatorStandBy>();
					characterAnimatorStandBy.Init(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.newModelName, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.animatorName, CS_0024_003C_003E8__locals0.tRuntimeAnimatorController, CS_0024_003C_003E8__locals0.animator, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.character.n_WEAPON_MOTION, characterTableExtraSize);
					characterAnimatorStandBy.UpdateWeapon(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.equipWeapon.n_ID, true, delegate
					{
						CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount - 1;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_RANKING_CHARACTER_CHANGE);
						OrangeBattleUtility.ChangeLayersRecursively<Transform>(CS_0024_003C_003E8__locals0.go.transform, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderLayer);
						CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.SetLoaded(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.emptyBlockUI);
					});
				});
				return;
			}
			_003C_003E4__this.nLoadCount++;
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("model/animator/empty", "empty2", delegate(RuntimeAnimatorController obj2)
			{
				_003C_003Ec__DisplayClass41_3 CS_0024_003C_003E8__locals1 = new _003C_003Ec__DisplayClass41_3
				{
					CS_0024_003C_003E8__locals3 = CS_0024_003C_003E8__locals0
				};
				CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount - 1;
				CS_0024_003C_003E8__locals0.tRuntimeAnimatorController = obj2;
				if (!int.TryParse(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.modelName.Substring(2, 3), out CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.characterId))
				{
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.characterId = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.character.n_ID;
				}
				CS_0024_003C_003E8__locals1.bnudles = new string[1] { string.Empty };
				CS_0024_003C_003E8__locals1.clips = OrangeAnimatonHelper.GetUniqueDebutName(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.debutModolName, out CS_0024_003C_003E8__locals1.bnudles[0]);
				CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.UpdateBonusClip(ref CS_0024_003C_003E8__locals1.clips);
				CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount + 1;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(CS_0024_003C_003E8__locals1.bnudles, delegate
				{
					CS_0024_003C_003E8__locals1._003CUpdateCharacter_003Eg__LoadAssetsComplete_007C7();
				}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
			});
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass41_1
	{
		public RuntimeAnimatorController tRuntimeAnimatorController;

		public UnityEngine.Object obj;

		public GameObject go;

		public Animator animator;

		public _003C_003Ec__DisplayClass41_0 CS_0024_003C_003E8__locals1;

		public Callback _003C_003E9__5;

		internal void _003CUpdateCharacter_003Eb__1(RuntimeAnimatorController obj2)
		{
			_003C_003Ec__DisplayClass41_2 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass41_2
			{
				CS_0024_003C_003E8__locals2 = this
			};
			CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount - 1;
			tRuntimeAnimatorController = obj2;
			if (!int.TryParse(CS_0024_003C_003E8__locals1._003C_003E4__this.modelName.Substring(2, 3), out CS_0024_003C_003E8__locals1._003C_003E4__this.characterId))
			{
				CS_0024_003C_003E8__locals1._003C_003E4__this.characterId = CS_0024_003C_003E8__locals1.character.n_ID;
			}
			CS_0024_003C_003E8__locals0.bnudles = new string[2]
			{
				string.Empty,
				AssetBundleScriptableObject.Instance.m_newmodel_weapon + CS_0024_003C_003E8__locals1.equipWeapon.s_MODEL
			};
			CS_0024_003C_003E8__locals0.clips = OrangeAnimatonHelper.GetUniqueDebutName(CS_0024_003C_003E8__locals1.debutModolName, out CS_0024_003C_003E8__locals0.bnudles[0]);
			CS_0024_003C_003E8__locals1._003C_003E4__this.UpdateBonusClip(ref CS_0024_003C_003E8__locals0.clips);
			CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount + 1;
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(CS_0024_003C_003E8__locals0.bnudles, delegate
			{
				CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount - 1;
				for (int num = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.childCount - 1; num >= 0; num--)
				{
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.GetChild(num).gameObject.SetActive(false);
					UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.GetChild(num).gameObject);
				}
				AnimatorOverrideController runtimeAnimatorController = OrangeAnimatonHelper.OverrideRuntimeAnimClip(ref CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.tRuntimeAnimatorController, ref CS_0024_003C_003E8__locals0.bnudles[0], ref CS_0024_003C_003E8__locals0.clips);
				if (CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.obj == null)
				{
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.go = new GameObject();
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.go.transform.SetParent(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition);
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.SetLoaded(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.emptyBlockUI);
				}
				else
				{
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.go = UnityEngine.Object.Instantiate(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.obj, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition, false) as GameObject;
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.go.transform.localScale = new Vector3(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.f_MODELSIZE, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.f_MODELSIZE, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.f_MODELSIZE);
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.animator = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.go.GetComponent<Animator>();
					float[] characterTableExtraSize = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTableExtraSize(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.s_ModelExtraSize);
					if (characterTableExtraSize.Length > 3)
					{
						CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.go.transform.localPosition = new Vector3(0f, characterTableExtraSize[3], 0f);
					}
					else
					{
						CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.go.transform.localPosition = new Vector3(0f, 0f, 0f);
					}
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.animator.runtimeAnimatorController = runtimeAnimatorController;
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
					if (CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.CharacterDebutForceLoop)
					{
						CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.animator.Play("2");
					}
					CharacterAnimatorStandBy characterAnimatorStandBy = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.go.AddOrGetComponent<CharacterAnimatorStandBy>();
					characterAnimatorStandBy.IsSpecialPos = true;
					characterAnimatorStandBy.Init(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.newModelName, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.animatorName, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.tRuntimeAnimatorController, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.animator, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.n_WEAPON_MOTION, characterTableExtraSize);
					characterAnimatorStandBy.SetWeapon(MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<GameObject>(CS_0024_003C_003E8__locals0.bnudles[1], CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.equipWeapon.s_MODEL + "_U.prefab"), CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.equipWeapon);
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_RANKING_CHARACTER_CHANGE);
					OrangeBattleUtility.ChangeLayersRecursively<Transform>(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.go.transform, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderLayer);
					for (int i = 0; i < CS_0024_003C_003E8__locals0.bnudles.Length; i++)
					{
						MonoBehaviourSingleton<AssetsBundleManager>.Instance.UnloadSingleBundleCache(CS_0024_003C_003E8__locals0.bnudles[i]);
					}
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.SetLoaded(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.emptyBlockUI);
				}
			}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
		}

		internal void _003CUpdateCharacter_003Eb__2(UnityEngine.Object obj2)
		{
			tRuntimeAnimatorController = (RuntimeAnimatorController)obj2;
			for (int num = CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.childCount - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.GetChild(num).gameObject);
			}
			go = UnityEngine.Object.Instantiate(obj, CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition, false) as GameObject;
			animator = go.GetComponent<Animator>();
			float[] characterTableExtraSize = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTableExtraSize(CS_0024_003C_003E8__locals1.character.s_ModelExtraSize);
			go.AddOrGetComponent<CharacterAnimatonRandWink>().Setup(animator);
			go.transform.localScale = new Vector3(CS_0024_003C_003E8__locals1.character.f_MODELSIZE, CS_0024_003C_003E8__locals1.character.f_MODELSIZE, CS_0024_003C_003E8__locals1.character.f_MODELSIZE);
			if (characterTableExtraSize.Length > 3)
			{
				go.transform.localPosition = new Vector3(0f, characterTableExtraSize[3], 0f);
			}
			else
			{
				go.transform.localPosition = new Vector3(0f, 0f, 0f);
			}
			CharacterAnimatorStandBy characterAnimatorStandBy = go.AddOrGetComponent<CharacterAnimatorStandBy>();
			characterAnimatorStandBy.Init(CS_0024_003C_003E8__locals1.newModelName, CS_0024_003C_003E8__locals1.animatorName, tRuntimeAnimatorController, animator, CS_0024_003C_003E8__locals1.character.n_WEAPON_MOTION, characterTableExtraSize);
			characterAnimatorStandBy.UpdateWeapon(CS_0024_003C_003E8__locals1.equipWeapon.n_ID, true, delegate
			{
				CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount - 1;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_RANKING_CHARACTER_CHANGE);
				OrangeBattleUtility.ChangeLayersRecursively<Transform>(go.transform, CS_0024_003C_003E8__locals1._003C_003E4__this.RenderLayer);
				CS_0024_003C_003E8__locals1._003C_003E4__this.SetLoaded(CS_0024_003C_003E8__locals1.emptyBlockUI);
			});
		}

		internal void _003CUpdateCharacter_003Eb__5()
		{
			CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount - 1;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_RANKING_CHARACTER_CHANGE);
			OrangeBattleUtility.ChangeLayersRecursively<Transform>(go.transform, CS_0024_003C_003E8__locals1._003C_003E4__this.RenderLayer);
			CS_0024_003C_003E8__locals1._003C_003E4__this.SetLoaded(CS_0024_003C_003E8__locals1.emptyBlockUI);
		}

		internal void _003CUpdateCharacter_003Eb__3(RuntimeAnimatorController obj2)
		{
			_003C_003Ec__DisplayClass41_3 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass41_3
			{
				CS_0024_003C_003E8__locals3 = this
			};
			CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount - 1;
			tRuntimeAnimatorController = obj2;
			if (!int.TryParse(CS_0024_003C_003E8__locals1._003C_003E4__this.modelName.Substring(2, 3), out CS_0024_003C_003E8__locals1._003C_003E4__this.characterId))
			{
				CS_0024_003C_003E8__locals1._003C_003E4__this.characterId = CS_0024_003C_003E8__locals1.character.n_ID;
			}
			CS_0024_003C_003E8__locals0.bnudles = new string[1] { string.Empty };
			CS_0024_003C_003E8__locals0.clips = OrangeAnimatonHelper.GetUniqueDebutName(CS_0024_003C_003E8__locals1.debutModolName, out CS_0024_003C_003E8__locals0.bnudles[0]);
			CS_0024_003C_003E8__locals1._003C_003E4__this.UpdateBonusClip(ref CS_0024_003C_003E8__locals0.clips);
			CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount + 1;
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(CS_0024_003C_003E8__locals0.bnudles, delegate
			{
				CS_0024_003C_003E8__locals0._003CUpdateCharacter_003Eg__LoadAssetsComplete_007C7();
			}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass41_2
	{
		public string[] bnudles;

		public string[] clips;

		public _003C_003Ec__DisplayClass41_1 CS_0024_003C_003E8__locals2;

		internal void _003CUpdateCharacter_003Eb__4()
		{
			CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount - 1;
			for (int num = CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.childCount - 1; num >= 0; num--)
			{
				CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.GetChild(num).gameObject.SetActive(false);
				UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.GetChild(num).gameObject);
			}
			AnimatorOverrideController runtimeAnimatorController = OrangeAnimatonHelper.OverrideRuntimeAnimClip(ref CS_0024_003C_003E8__locals2.tRuntimeAnimatorController, ref bnudles[0], ref clips);
			if (CS_0024_003C_003E8__locals2.obj == null)
			{
				CS_0024_003C_003E8__locals2.go = new GameObject();
				CS_0024_003C_003E8__locals2.go.transform.SetParent(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition);
				CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.SetLoaded(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.emptyBlockUI);
				return;
			}
			CS_0024_003C_003E8__locals2.go = UnityEngine.Object.Instantiate(CS_0024_003C_003E8__locals2.obj, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition, false) as GameObject;
			CS_0024_003C_003E8__locals2.go.transform.localScale = new Vector3(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.f_MODELSIZE, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.f_MODELSIZE, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.f_MODELSIZE);
			CS_0024_003C_003E8__locals2.animator = CS_0024_003C_003E8__locals2.go.GetComponent<Animator>();
			float[] characterTableExtraSize = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTableExtraSize(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.s_ModelExtraSize);
			if (characterTableExtraSize.Length > 3)
			{
				CS_0024_003C_003E8__locals2.go.transform.localPosition = new Vector3(0f, characterTableExtraSize[3], 0f);
			}
			else
			{
				CS_0024_003C_003E8__locals2.go.transform.localPosition = new Vector3(0f, 0f, 0f);
			}
			CS_0024_003C_003E8__locals2.animator.runtimeAnimatorController = runtimeAnimatorController;
			CS_0024_003C_003E8__locals2.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			if (CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.CharacterDebutForceLoop)
			{
				CS_0024_003C_003E8__locals2.animator.Play("2");
			}
			CharacterAnimatorStandBy characterAnimatorStandBy = CS_0024_003C_003E8__locals2.go.AddOrGetComponent<CharacterAnimatorStandBy>();
			characterAnimatorStandBy.IsSpecialPos = true;
			characterAnimatorStandBy.Init(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.newModelName, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.animatorName, CS_0024_003C_003E8__locals2.tRuntimeAnimatorController, CS_0024_003C_003E8__locals2.animator, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.character.n_WEAPON_MOTION, characterTableExtraSize);
			characterAnimatorStandBy.SetWeapon(MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<GameObject>(bnudles[1], CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.equipWeapon.s_MODEL + "_U.prefab"), CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.equipWeapon);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_RANKING_CHARACTER_CHANGE);
			OrangeBattleUtility.ChangeLayersRecursively<Transform>(CS_0024_003C_003E8__locals2.go.transform, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderLayer);
			for (int i = 0; i < bnudles.Length; i++)
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.UnloadSingleBundleCache(bnudles[i]);
			}
			CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.SetLoaded(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.emptyBlockUI);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass41_3
	{
		public string[] bnudles;

		public string[] clips;

		public _003C_003Ec__DisplayClass41_1 CS_0024_003C_003E8__locals3;

		internal void _003CUpdateCharacter_003Eb__6()
		{
			_003CUpdateCharacter_003Eg__LoadAssetsComplete_007C7();
		}

		internal void _003CUpdateCharacter_003Eg__LoadAssetsComplete_007C7()
		{
			_003C_003Ec__DisplayClass41_4 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass41_4
			{
				CS_0024_003C_003E8__locals4 = this
			};
			CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount = CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1._003C_003E4__this.nLoadCount - 1;
			for (int num = CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.childCount - 1; num >= 0; num--)
			{
				CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.GetChild(num).gameObject.SetActive(false);
				UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition.GetChild(num).gameObject);
			}
			CS_0024_003C_003E8__locals0.overrideController = OrangeAnimatonHelper.OverrideRuntimeAnimClip(ref CS_0024_003C_003E8__locals3.tRuntimeAnimatorController, ref bnudles[0], ref clips);
			if (CS_0024_003C_003E8__locals3.obj == null)
			{
				CS_0024_003C_003E8__locals3.go = new GameObject();
				CS_0024_003C_003E8__locals3.go.transform.SetParent(CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1._003C_003E4__this.RenderPosition);
				CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1._003C_003E4__this.SetLoaded(CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1.emptyBlockUI);
				return;
			}
			AnimationClip[] animationClips = CS_0024_003C_003E8__locals0.overrideController.animationClips;
			List<string> list = new List<string>();
			List<int> list2 = new List<int>();
			AnimationClip[] array = animationClips;
			for (int i = 0; i < array.Length; i++)
			{
				AnimationEvent[] events = array[i].events;
				foreach (AnimationEvent animationEvent in events)
				{
					if (!(animationEvent.functionName == CharacterAnimatonEvent.FUNCTION_NAME_PLAYSE))
					{
						continue;
					}
					string[] array2 = animationEvent.stringParameter.Split(',');
					if (array2.Length > 1 && !MonoBehaviourSingleton<AudioManager>.Instance.IsAtomSourceExist(array2[0]))
					{
						int item = (array2[0].StartsWith("VOICE") ? 3 : 2);
						if (!list.Contains(array2[0]))
						{
							list.Add(array2[0]);
							list2.Add(item);
						}
					}
				}
			}
			if (list.Count > 0)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(list.ToArray(), list2.ToArray(), delegate
				{
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals4.CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1._003C_003E4__this.InstantiateDebutCharacter(CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals4.CS_0024_003C_003E8__locals3.obj, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals4.CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1.character.f_MODELSIZE, CS_0024_003C_003E8__locals0.overrideController, CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals4.CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1.emptyBlockUI);
				});
			}
			else
			{
				CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1._003C_003E4__this.InstantiateDebutCharacter(CS_0024_003C_003E8__locals3.obj, CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1.character.f_MODELSIZE, CS_0024_003C_003E8__locals0.overrideController, CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1.emptyBlockUI);
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass41_4
	{
		public AnimatorOverrideController overrideController;

		public _003C_003Ec__DisplayClass41_3 CS_0024_003C_003E8__locals4;

		internal void _003CUpdateCharacter_003Eb__8()
		{
			CS_0024_003C_003E8__locals4.CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1._003C_003E4__this.InstantiateDebutCharacter(CS_0024_003C_003E8__locals4.CS_0024_003C_003E8__locals3.obj, CS_0024_003C_003E8__locals4.CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1.character.f_MODELSIZE, overrideController, CS_0024_003C_003E8__locals4.CS_0024_003C_003E8__locals3.CS_0024_003C_003E8__locals1.emptyBlockUI);
		}
	}

	private bool DEBUT_LOAD_FROM_FBX;

	private readonly float fixedFov = 31.5f;

	private RenderType renderType;

	private RenderTexture tRenderTexture;

	private Material refMaterial;

	public Camera renderCamera;

	public Transform RenderPosition;

	public Light renderLight;

	public string[] EXclips = new string[2] { "null", "null" };

	private Dictionary<int, int> dictEXclipsCount = new Dictionary<int, int>();

	public bool CanCount;

	private int renderLayer;

	private GameObject bloomEffect;

	private bool bModelLoaded;

	private int nLoadCount;

	private Color cameraColorDefault = Color.clear;

	private Color cameraColorUnlit = new Color(0.51f, 0.51f, 0.51f, 0f);

	private RenderTextureDescriptor tRenderTextureDescriptor;

	private string modelName = string.Empty;

	private readonly string[] modelEndWith = new string[3] { "_U.prefab", "_U_S.prefab", "_U_S_S.prefab" };

	private BloomOnImage bloomOnImage;

	private readonly int InvisibleLayer = 2;

	private bool isCanvasActive = true;

	private int characterId;

	private string[] unlitModels = new string[6] { "ch073_000", "ch021_000", "ch084_000", "ch107_000", "Saber_007", "Saber_000" };

	private string[] alphaClipModel = new string[1] { "ch078_000" };

	public bool CharacterDebutForceLoop { get; set; }

	public bool OnlyDebut { get; set; }

	public int RenderLayer
	{
		get
		{
			if (!isCanvasActive)
			{
				return InvisibleLayer;
			}
			return renderLayer;
		}
	}

	private void Awake()
	{
		dictEXclipsCount.Clear();
		CharacterDebutForceLoop = false;
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<CHARACTER_TABLE, WEAPON_TABLE, SKIN_TABLE>(EventManager.ID.UPDATE_RENDER_CHARACTER, UpdateCharacter);
		Singleton<GenericEventManager>.Instance.AttachEvent<WEAPON_TABLE>(EventManager.ID.RT_UPDATE_WEAPON, UpdateWeapon);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BACK_TO_HOMETOP, BackToHometop);
		Singleton<GenericEventManager>.Instance.AttachEvent<int, float>(EventManager.ID.CHARACTER_RT_SUNSHINE, UpdateSunshine);
		Singleton<GenericEventManager>.Instance.AttachEvent<int, float>(EventManager.ID.RT_UPDATE_CAMERA_FOV, UpdateFov);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<CHARACTER_TABLE, WEAPON_TABLE, SKIN_TABLE>(EventManager.ID.UPDATE_RENDER_CHARACTER, UpdateCharacter);
		Singleton<GenericEventManager>.Instance.DetachEvent<WEAPON_TABLE>(EventManager.ID.RT_UPDATE_WEAPON, UpdateWeapon);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.BACK_TO_HOMETOP, BackToHometop);
		Singleton<GenericEventManager>.Instance.DetachEvent<int, float>(EventManager.ID.CHARACTER_RT_SUNSHINE, UpdateSunshine);
		Singleton<GenericEventManager>.Instance.DetachEvent<int, float>(EventManager.ID.RT_UPDATE_CAMERA_FOV, UpdateFov);
	}

	private void BackToHometop()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		ReleaseMemory();
		if (bloomEffect != null)
		{
			UnityEngine.Object.DestroyImmediate(bloomEffect);
		}
	}

	private void UpdateSunshine(int light, float time)
	{
		GameObject gameObject = renderLight.gameObject;
		LeanTween.cancel(gameObject);
		if (light == 0)
		{
			LeanTween.rotateLocal(gameObject, new Vector3(110f, 0f, 0f), time);
		}
		else
		{
			LeanTween.rotateLocal(gameObject, new Vector3(0f, 0f, 0f), time);
		}
	}

	private void UpdateFov(int fovTo, float time)
	{
		if (OnlyDebut)
		{
			GameObject obj = renderCamera.gameObject;
			LeanTween.cancel(obj);
			float fieldOfView = renderCamera.fieldOfView;
			LeanTween.value(obj, fieldOfView, fovTo, time).setOnUpdate(delegate(float f)
			{
				renderCamera.fieldOfView = f;
			}).setIgnoreTimeScale(true);
		}
	}

	private void ReleaseMemory()
	{
		if (tRenderTexture != null)
		{
			renderCamera.targetTexture = null;
			tRenderTexture.Release();
			RenderTexture.ReleaseTemporary(tRenderTexture);
			tRenderTexture = null;
		}
		if (refMaterial != null)
		{
			refMaterial.hideFlags = HideFlags.None;
			UnityEngine.Object.DestroyImmediate(refMaterial);
		}
		GC.Collect();
	}

	public void AssignNewRender(CHARACTER_TABLE character, WEAPON_TABLE equipWeapon, SKIN_TABLE skin, Vector3 localPos, RawImage renderTarget, int renderLayer = 0)
	{
		this.renderLayer = renderLayer;
		if (this.renderLayer == 0)
		{
			this.renderLayer = ManagedSingleton<OrangeLayerManager>.Instance.RenderTextureLayer;
		}
		renderType = RenderType.CHARACTER;
		RenderPosition.localPosition = localPos;
		RenderPosition.localRotation = Quaternion.Euler(5f, 180f, 0f);
		Create(1920, 1080, renderTarget);
		UpdateCharacter(character, equipWeapon, skin);
		nLoadCount++;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("shader/uieffect", GetMaterialName(character.s_MODEL), delegate(Material obj)
		{
			nLoadCount--;
			refMaterial = UnityEngine.Object.Instantiate(obj);
			renderTarget.material = refMaterial;
			renderTarget.color = Color.white;
		});
		bloomOnImage = ApplyCharacterBloom(renderTarget, character.n_ID);
	}

	public void AssignNewEnemyRender(string enemyname, Vector3 localPos, RawImage renderTarget, int renderLayer = 0)
	{
		this.renderLayer = renderLayer;
		if (this.renderLayer == 0)
		{
			this.renderLayer = ManagedSingleton<OrangeLayerManager>.Instance.RenderTextureLayer;
		}
		renderType = RenderType.Enemy;
		RenderPosition.localPosition = localPos;
		RenderPosition.localRotation = Quaternion.Euler(0f, 0f, 0f);
		Create(1920, 1080, renderTarget);
		UpdateModelName(enemyname);
		nLoadCount++;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("shader/uieffect", GetMaterialName(enemyname), delegate(Material obj)
		{
			nLoadCount--;
			refMaterial = UnityEngine.Object.Instantiate(obj);
			renderTarget.material = refMaterial;
			renderTarget.color = Color.white;
		});
	}

	public void AssignNewWeaponRender(WEAPON_TABLE weapon, Vector3 localPos, RawImage renderTarget, int renderLayer = 1)
	{
		this.renderLayer = renderLayer;
		if (this.renderLayer == 0)
		{
			this.renderLayer = ManagedSingleton<OrangeLayerManager>.Instance.RenderTextureLayer;
		}
		renderType = RenderType.WEAPON;
		RenderPosition.localPosition = localPos;
		RenderPosition.localRotation = Quaternion.Euler(0f, 0f, 0f);
		Create(1920, 1080, renderTarget);
		UpdateWeapon(weapon);
		nLoadCount++;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("shader/uieffect", GetMaterialName(weapon.s_MODEL), delegate(Material obj)
		{
			nLoadCount--;
			refMaterial = UnityEngine.Object.Instantiate(obj);
			renderTarget.material = refMaterial;
			renderTarget.color = Color.white;
		});
		ApplyBloom(renderTarget);
	}

	private void Create(int p_width, int p_height, RawImage renderTarget)
	{
		int width = Mathf.CeilToInt((float)p_width * MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenRate);
		int height = Mathf.CeilToInt((float)p_height * MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenRate);
		tRenderTextureDescriptor = new RenderTextureDescriptor(width, height, MonoBehaviourSingleton<OrangeGameManager>.Instance.GetHDRTextureFormat(), 24);
		tRenderTextureDescriptor.dimension = TextureDimension.Tex2D;
		tRenderTextureDescriptor.useMipMap = false;
		tRenderTextureDescriptor.sRGB = false;
		ReleaseMemory();
		tRenderTexture = RenderTexture.GetTemporary(tRenderTextureDescriptor);
		renderTarget.rectTransform.sizeDelta = new Vector2(p_width, p_height);
		renderTarget.texture = tRenderTexture;
		renderCamera.targetTexture = tRenderTexture;
	}

	private void UpdateCharacter(CHARACTER_TABLE character, WEAPON_TABLE equipWeapon, SKIN_TABLE skinTable)
	{
		if (renderType != RenderType.CHARACTER)
		{
			return;
		}
		string newModelName = character.s_MODEL;
		string debutModolName = character.s_MODEL;
		string animatorName = character.s_ANIMATOR;
		if (skinTable != null)
		{
			newModelName = skinTable.s_MODEL;
			if (skinTable.n_SUBTYPE == 0)
			{
				animatorName = skinTable.s_ANIMATOR;
				debutModolName = skinTable.s_MODEL;
			}
			else if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(skinTable.s_MOTION))
			{
				debutModolName = skinTable.s_MOTION;
			}
		}
		if (modelName == newModelName)
		{
			return;
		}
		modelName = newModelName;
		if (isCanvasActive)
		{
			SetCameraActive(true);
		}
		EmptyBlockUI emptyBlockUI = MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUISync<EmptyBlockUI>("UI_EmptyBlock", true);
		emptyBlockUI.SetBlock(true);
		bModelLoaded = false;
		int num = 0;
		if (equipWeapon == null)
		{
			num = 1;
		}
		else if (OnlyDebut || character.n_SPECIAL_SHOWPOSE > 0)
		{
			num = 1;
		}
		if (bloomOnImage != null)
		{
			bloomOnImage.radius = MonoBehaviourSingleton<OrangeBattleUtility>.Instance.GetBloomRadius(character.n_ID);
		}
		LeanTween.cancel(renderCamera.gameObject);
		renderCamera.fieldOfView = fixedFov;
		nLoadCount++;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("model/character/" + modelName, modelName + modelEndWith[num], delegate(UnityEngine.Object obj)
		{
			nLoadCount--;
			GameObject go = null;
			Animator animator = null;
			RuntimeAnimatorController tRuntimeAnimatorController = null;
			if (!OnlyDebut && equipWeapon != null && character.n_SPECIAL_SHOWPOSE > 0)
			{
				nLoadCount++;
				CharacterDebutForceLoop = true;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("model/animator/empty", "empty2", delegate(RuntimeAnimatorController obj2)
				{
					nLoadCount--;
					tRuntimeAnimatorController = obj2;
					if (!int.TryParse(modelName.Substring(2, 3), out characterId))
					{
						characterId = character.n_ID;
					}
					string[] bnudles = new string[2]
					{
						string.Empty,
						AssetBundleScriptableObject.Instance.m_newmodel_weapon + equipWeapon.s_MODEL
					};
					string[] clips = OrangeAnimatonHelper.GetUniqueDebutName(debutModolName, out bnudles[0]);
					UpdateBonusClip(ref clips);
					nLoadCount++;
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(bnudles, delegate
					{
						nLoadCount--;
						for (int num3 = RenderPosition.childCount - 1; num3 >= 0; num3--)
						{
							RenderPosition.GetChild(num3).gameObject.SetActive(false);
							UnityEngine.Object.Destroy(RenderPosition.GetChild(num3).gameObject);
						}
						AnimatorOverrideController runtimeAnimatorController = OrangeAnimatonHelper.OverrideRuntimeAnimClip(ref tRuntimeAnimatorController, ref bnudles[0], ref clips);
						if (obj == null)
						{
							go = new GameObject();
							go.transform.SetParent(RenderPosition);
							SetLoaded(emptyBlockUI);
						}
						else
						{
							go = UnityEngine.Object.Instantiate(obj, RenderPosition, false) as GameObject;
							go.transform.localScale = new Vector3(character.f_MODELSIZE, character.f_MODELSIZE, character.f_MODELSIZE);
							animator = go.GetComponent<Animator>();
							float[] characterTableExtraSize2 = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTableExtraSize(character.s_ModelExtraSize);
							if (characterTableExtraSize2.Length > 3)
							{
								go.transform.localPosition = new Vector3(0f, characterTableExtraSize2[3], 0f);
							}
							else
							{
								go.transform.localPosition = new Vector3(0f, 0f, 0f);
							}
							animator.runtimeAnimatorController = runtimeAnimatorController;
							animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
							if (CharacterDebutForceLoop)
							{
								animator.Play("2");
							}
							CharacterAnimatorStandBy characterAnimatorStandBy2 = go.AddOrGetComponent<CharacterAnimatorStandBy>();
							characterAnimatorStandBy2.IsSpecialPos = true;
							characterAnimatorStandBy2.Init(newModelName, animatorName, tRuntimeAnimatorController, animator, character.n_WEAPON_MOTION, characterTableExtraSize2);
							characterAnimatorStandBy2.SetWeapon(MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<GameObject>(bnudles[1], equipWeapon.s_MODEL + "_U.prefab"), equipWeapon);
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_RANKING_CHARACTER_CHANGE);
							OrangeBattleUtility.ChangeLayersRecursively<Transform>(go.transform, RenderLayer);
							for (int i = 0; i < bnudles.Length; i++)
							{
								MonoBehaviourSingleton<AssetsBundleManager>.Instance.UnloadSingleBundleCache(bnudles[i]);
							}
							SetLoaded(emptyBlockUI);
						}
					}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
				});
			}
			else if (!OnlyDebut && equipWeapon != null)
			{
				nLoadCount++;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("model/animator/empty", "empty", delegate(UnityEngine.Object obj2)
				{
					tRuntimeAnimatorController = (RuntimeAnimatorController)obj2;
					for (int num2 = RenderPosition.childCount - 1; num2 >= 0; num2--)
					{
						UnityEngine.Object.Destroy(RenderPosition.GetChild(num2).gameObject);
					}
					go = UnityEngine.Object.Instantiate(obj, RenderPosition, false) as GameObject;
					animator = go.GetComponent<Animator>();
					float[] characterTableExtraSize = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTableExtraSize(character.s_ModelExtraSize);
					go.AddOrGetComponent<CharacterAnimatonRandWink>().Setup(animator);
					go.transform.localScale = new Vector3(character.f_MODELSIZE, character.f_MODELSIZE, character.f_MODELSIZE);
					if (characterTableExtraSize.Length > 3)
					{
						go.transform.localPosition = new Vector3(0f, characterTableExtraSize[3], 0f);
					}
					else
					{
						go.transform.localPosition = new Vector3(0f, 0f, 0f);
					}
					CharacterAnimatorStandBy characterAnimatorStandBy = go.AddOrGetComponent<CharacterAnimatorStandBy>();
					characterAnimatorStandBy.Init(newModelName, animatorName, tRuntimeAnimatorController, animator, character.n_WEAPON_MOTION, characterTableExtraSize);
					characterAnimatorStandBy.UpdateWeapon(equipWeapon.n_ID, true, delegate
					{
						nLoadCount--;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_RANKING_CHARACTER_CHANGE);
						OrangeBattleUtility.ChangeLayersRecursively<Transform>(go.transform, RenderLayer);
						SetLoaded(emptyBlockUI);
					});
				});
			}
			else
			{
				nLoadCount++;
				_003C_003Ec__DisplayClass41_1 CS_0024_003C_003E8__locals1 = null;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("model/animator/empty", "empty2", delegate(RuntimeAnimatorController obj2)
				{
					_003C_003Ec__DisplayClass41_3 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass41_3();
					CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals3 = CS_0024_003C_003E8__locals1;
					nLoadCount--;
					tRuntimeAnimatorController = obj2;
					if (!int.TryParse(modelName.Substring(2, 3), out characterId))
					{
						characterId = character.n_ID;
					}
					CS_0024_003C_003E8__locals2.bnudles = new string[1] { string.Empty };
					CS_0024_003C_003E8__locals2.clips = OrangeAnimatonHelper.GetUniqueDebutName(debutModolName, out CS_0024_003C_003E8__locals2.bnudles[0]);
					UpdateBonusClip(ref CS_0024_003C_003E8__locals2.clips);
					nLoadCount++;
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(CS_0024_003C_003E8__locals2.bnudles, delegate
					{
						CS_0024_003C_003E8__locals2._003CUpdateCharacter_003Eg__LoadAssetsComplete_007C7();
					}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
				});
			}
		});
	}

	private bool IsEggPose(AnimatorOverrideController animator)
	{
		if (animator == null)
		{
			return false;
		}
		if (!OnlyDebut || CharacterDebutForceLoop)
		{
			return false;
		}
		AnimationClip[] animationClips = animator.animationClips;
		for (int i = 0; i < animationClips.Length; i++)
		{
			if (animationClips[i].name.Equals("3"))
			{
				return false;
			}
		}
		return true;
	}

	private void InstantiateDebutCharacter<T>(T obj, float f_MODELSIZE, AnimatorOverrideController overrideController, EmptyBlockUI emptyBlockUI) where T : UnityEngine.Object
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(obj, RenderPosition, false) as GameObject;
		Animator component = gameObject.GetComponent<Animator>();
		gameObject.transform.localScale = new Vector3(f_MODELSIZE, f_MODELSIZE, f_MODELSIZE);
		component.runtimeAnimatorController = overrideController;
		component.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		if (CharacterDebutForceLoop)
		{
			component.Play("2");
		}
		if (IsEggPose(overrideController) && (bool)gameObject.GetComponent<Demo_EasterEggs>())
		{
			gameObject.GetComponent<Demo_EasterEggs>().isStart = true;
		}
		OrangeBattleUtility.ChangeLayersRecursively<Transform>(gameObject.transform, RenderLayer);
		SetLoaded(emptyBlockUI);
	}

	private void SetLoaded<T>(T blockUI) where T : OrangeUIBase
	{
		bModelLoaded = true;
		blockUI.OnClickCloseBtn();
		if (isCanvasActive)
		{
			SetCameraActive(true);
		}
	}

	public bool IsModelLoaded()
	{
		return bModelLoaded;
	}

	public bool IsCanDelete()
	{
		return nLoadCount == 0;
	}

	public void UpdateModelName(params object[] p_param)
	{
		if (renderType != RenderType.Enemy)
		{
			return;
		}
		string text = (string)p_param[0];
		if (modelName == text)
		{
			return;
		}
		modelName = text;
		if (RenderPosition.childCount != 0)
		{
			UnityEngine.Object.Destroy(RenderPosition.GetChild(0).gameObject);
		}
		nLoadCount++;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_prefabEnemy + modelName, modelName, delegate(GameObject obj)
		{
			nLoadCount--;
			GameObject gameObject = UnityEngine.Object.Instantiate(obj, RenderPosition, false);
			OrangeBattleUtility.ChangeLayersRecursively<Transform>(gameObject.transform, RenderLayer);
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			gameObject.GetComponentInParent<EnemyControllerBase>().SetChipInfoAnim();
			string[] array = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.Where((KeyValuePair<int, DISC_TABLE> x) => x.Value.s_MODEL == modelName).ToDictionary((KeyValuePair<int, DISC_TABLE> x) => x.Value.s_MODEL.ToString(), (KeyValuePair<int, DISC_TABLE> x) => x.Value)[modelName].s_PIVOT.Split(',');
			float result5;
			float result4;
			float result3;
			float result2;
			float result;
			float result6 = (result5 = (result4 = (result3 = (result2 = (result = 0f)))));
			float result7 = 1f;
			if (array.Length != 0)
			{
				float.TryParse(array[0], out result6);
			}
			if (array.Length > 1)
			{
				float.TryParse(array[1], out result5);
			}
			if (array.Length > 2)
			{
				float.TryParse(array[2], out result4);
			}
			if (array.Length > 3)
			{
				float.TryParse(array[3], out result3);
			}
			if (array.Length > 4)
			{
				float.TryParse(array[4], out result2);
			}
			if (array.Length > 5)
			{
				float.TryParse(array[5], out result);
			}
			if (array.Length > 6)
			{
				float.TryParse(array[6], out result7);
			}
			gameObject.transform.localPosition = new Vector3(result6, result5, result4);
			gameObject.transform.localRotation = Quaternion.Euler(result3, result2, result);
			gameObject.transform.localScale = new Vector3(result7, result7, result7);
			CharacterMaterial component = gameObject.GetComponent<CharacterMaterial>();
			if (component != null)
			{
				component.SetBaseRenderForUI();
				component.AppearX();
			}
			bModelLoaded = true;
			if (isCanvasActive)
			{
				SetCameraActive(true);
			}
		});
	}

	private void UpdateWeapon(WEAPON_TABLE weapon)
	{
		if (renderType != RenderType.WEAPON || modelName == weapon.s_MODEL)
		{
			return;
		}
		EmptyBlockUI emptyBlockUI = MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUISync<EmptyBlockUI>("UI_EmptyBlock", true);
		emptyBlockUI.SetBlock(true);
		modelName = weapon.s_MODEL;
		if (RenderPosition.childCount != 0)
		{
			UnityEngine.Object.Destroy(RenderPosition.GetChild(0).gameObject);
		}
		nLoadCount++;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_newmodel_weapon + modelName, modelName + "_U.prefab", delegate(GameObject obj)
		{
			nLoadCount--;
			GameObject gameObject = new GameObject("RotateObj");
			gameObject.transform.SetParent(RenderPosition, false);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(obj, Vector3.zero, Quaternion.identity, gameObject.transform);
			OrangeBattleUtility.ChangeLayersRecursively<Transform>(gameObject2.transform, RenderLayer);
			if ((short)weapon.n_TYPE == 8)
			{
				gameObject2.transform.localRotation = Quaternion.Euler(18f, 90f, 0f);
				RotateAroundPivot rotateAroundPivot = gameObject2.AddComponent<RotateAroundPivot>();
				rotateAroundPivot.Pivot = new Vector3(0f, 0f, 0.58f);
				rotateAroundPivot.RotateZ = true;
				if (weapon.s_WEAPON_PIVOT != "null")
				{
					float x = 0f;
					float y = 0f;
					float z = 0f;
					float fscale = 0f;
					float num = 0f;
					ParserPivot(weapon.s_WEAPON_PIVOT, out x, out y, out z, out fscale, out num);
					gameObject.transform.localPosition = new Vector3(0f, 0f, fscale);
					gameObject2.transform.localPosition = new Vector3(x, y, z);
					gameObject.AddComponent<RotateSelf>();
				}
				else
				{
					gameObject2.transform.localPosition = new Vector3(-0.5f, -0.05f, 1f);
				}
			}
			else if (weapon.s_WEAPON_PIVOT != "null")
			{
				float x2 = 0f;
				float y2 = 0f;
				float z2 = 0f;
				float fscale2 = 0f;
				float num2 = 0f;
				ParserPivot(weapon.s_WEAPON_PIVOT, out x2, out y2, out z2, out fscale2, out num2);
				gameObject.transform.localPosition = new Vector3(0f, 0f, fscale2);
				gameObject2.transform.localPosition = new Vector3(x2, y2, z2);
				gameObject.AddComponent<RotateSelf>();
			}
			else
			{
				Renderer[] componentsInChildren = gameObject2.transform.GetComponentsInChildren<Renderer>();
				Vector3 vector = Vector3.zero;
				Vector3 vector2 = Vector3.zero;
				if (componentsInChildren.Length != 0)
				{
					vector = componentsInChildren[0].bounds.min;
					vector2 = componentsInChildren[0].bounds.max;
					for (int i = 1; i < componentsInChildren.Length; i++)
					{
						Vector3 min = componentsInChildren[i].bounds.min;
						Vector3 max = componentsInChildren[i].bounds.max;
						if (vector.x > min.x)
						{
							vector.x = min.x;
						}
						if (vector.y > min.y)
						{
							vector.y = min.y;
						}
						if (vector.z > min.z)
						{
							vector.z = min.z;
						}
						if (vector2.x < max.x)
						{
							vector2.x = max.x;
						}
						if (vector2.y < max.y)
						{
							vector2.y = max.y;
						}
						if (vector2.z < max.z)
						{
							vector2.z = max.z;
						}
					}
				}
				Vector3 pivot = (vector + vector2) / 2f;
				gameObject2.transform.localPosition = Vector3.zero;
				RotateAroundPivot rotateAroundPivot2 = gameObject2.AddComponent<RotateAroundPivot>();
				rotateAroundPivot2.Pivot = pivot;
				rotateAroundPivot2.RotateY = true;
			}
			Transform transform = OrangeBattleUtility.FindChildRecursive(gameObject2.transform, "efx", true);
			if (transform != null)
			{
				transform.gameObject.SetActive(true);
			}
			CharacterMaterial component = gameObject2.GetComponent<CharacterMaterial>();
			if (component != null)
			{
				component.SetBaseRenderForUI();
				component.Appear();
			}
			SetLoaded(emptyBlockUI);
		});
	}

	private void ParserPivot(string s_WEAPON_PIVOT, out float x, out float y, out float z, out float fscale, out float rotation)
	{
		string[] array = s_WEAPON_PIVOT.Split(',');
		x = 0f;
		y = 0f;
		z = 0f;
		fscale = 0f;
		rotation = 0f;
		if (array.Length != 0)
		{
			float.TryParse(array[0], out x);
		}
		if (array.Length > 1)
		{
			float.TryParse(array[1], out y);
		}
		if (array.Length > 2)
		{
			float.TryParse(array[2], out z);
		}
		if (array.Length > 3)
		{
			float.TryParse(array[3], out fscale);
		}
		if (array.Length > 4)
		{
			float.TryParse(array[4], out rotation);
		}
	}

	private void ApplyBloom(RawImage renderTarget)
	{
		if (bloomEffect != null)
		{
			bloomEffect.hideFlags = HideFlags.None;
			UnityEngine.Object.DestroyImmediate(bloomEffect);
		}
		bloomEffect = new GameObject("bloomEft");
		OrangeBattleUtility.AddBloomOnImage(bloomEffect, renderTarget.gameObject);
		bloomEffect.transform.SetParent(renderTarget.transform, false);
		bloomEffect.transform.localScale = Vector3.one;
	}

	private BloomOnImage ApplyCharacterBloom(RawImage renderTarget, int characterId)
	{
		if (bloomEffect != null)
		{
			bloomEffect.hideFlags = HideFlags.None;
			UnityEngine.Object.DestroyImmediate(bloomEffect);
		}
		bloomEffect = new GameObject("bloomEft");
		BloomOnImage result = OrangeBattleUtility.AddBloomOnImage(bloomEffect, renderTarget.gameObject, characterId);
		bloomEffect.transform.SetParent(renderTarget.transform, false);
		bloomEffect.transform.localScale = Vector3.one;
		return result;
	}

	public void SetCameraActive(bool p_active)
	{
		isCanvasActive = p_active;
		if (isCanvasActive)
		{
			renderCamera.cullingMask = 1 << renderLayer;
			renderLight.cullingMask = 1 << renderLayer;
		}
		else
		{
			renderCamera.cullingMask = 0;
			renderLight.cullingMask = 0;
		}
		OrangeBattleUtility.ChangeLayersRecursively<Transform>(RenderPosition, RenderLayer);
	}

	public void BonusCount(int id)
	{
		if (characterId != 0 && CanCount && characterId == id)
		{
			if (dictEXclipsCount.ContainsKey(characterId))
			{
				dictEXclipsCount[characterId]++;
			}
			else
			{
				dictEXclipsCount.Add(characterId, 1);
			}
		}
	}

	private void UpdateBonusClip(ref string[] clips)
	{
		int value;
		if (!dictEXclipsCount.TryGetValue(characterId, out value))
		{
			return;
		}
		switch (characterId)
		{
		case 23:
			if (value >= 5)
			{
				clips = new string[3] { "ch023_ui_debut_egg_start", "ch023_ui_debut_loop", "ch023_ui_debut_loop" };
			}
			break;
		case 43:
			if (value >= 3)
			{
				clips = new string[3] { "ch043_ui_debut_egg_start", "ch043_ui_debut_loop", "ch023_ui_debut_loop" };
			}
			break;
		}
	}

	private string GetMaterialName(string modelName)
	{
		if (OnlyDebut && alphaClipModel.Contains(modelName))
		{
			renderCamera.backgroundColor = cameraColorDefault;
			return "TransparentRT_AlphaClip";
		}
		if (unlitModels.Contains(modelName))
		{
			renderCamera.backgroundColor = cameraColorUnlit;
			return "TransparentUnlit";
		}
		renderCamera.backgroundColor = cameraColorDefault;
		return "TransparentRT";
	}
}
