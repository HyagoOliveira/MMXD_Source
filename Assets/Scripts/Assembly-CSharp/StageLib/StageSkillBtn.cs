using UnityEngine;
using UnityEngine.UI;

namespace StageLib
{
	public class StageSkillBtn : MonoBehaviour
	{
		private Image skillimg;

		private Image RecahrgeImg;

		private Image RecahrgeBg;

		private Image OkImg;

		private Image OkBg;

		private float fNowChangeTime;

		private float fChangeTime;

		private void Start()
		{
			LoadIconCallBack loadIconCallBack = new LoadIconCallBack();
			skillimg = GetComponent<Image>();
			loadIconCallBack.TargetImage = skillimg;
			loadIconCallBack.TargetImage.color = Color.white;
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>("texture/prototype/battle", "Orange_UI_battle_Button_Skill_BG", loadIconCallBack.LoadCB);
			GameObject obj = new GameObject("skillimg");
			obj.transform.parent = base.transform;
			Image targetImage = obj.AddComponent<Image>();
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			((RectTransform)obj.transform).sizeDelta = new Vector2(120f, 120f);
			loadIconCallBack.TargetImage = targetImage;
			skillimg = targetImage;
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>("texture/prototype/battle", "Orange_UI_battle_Button_Sprint", loadIconCallBack.LoadCB);
			GameObject obj2 = new GameObject("RecahrgeImg");
			obj2.transform.parent = base.transform;
			targetImage = obj2.AddComponent<Image>();
			obj2.transform.localPosition = Vector3.zero;
			obj2.transform.localScale = Vector3.one;
			((RectTransform)obj2.transform).sizeDelta = new Vector2(140f, 140f);
			loadIconCallBack.TargetImage = targetImage;
			RecahrgeImg = targetImage;
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>("texture/prototype/battle", "Orange_UI_battle_Button_Skill_BG_LoadingBar", loadIconCallBack.LoadCB);
			GameObject obj3 = new GameObject("RecahrgeBg");
			obj3.transform.parent = base.transform;
			targetImage = obj3.AddComponent<Image>();
			obj3.transform.localPosition = Vector3.zero;
			obj3.transform.localScale = Vector3.one;
			((RectTransform)obj3.transform).sizeDelta = new Vector2(100f, 100f);
			loadIconCallBack.TargetImage = targetImage;
			RecahrgeBg = targetImage;
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>("texture/prototype/battle", "Orange_UI_battle_Button_Skill_BG_Loading", loadIconCallBack.LoadCB);
			GameObject obj4 = new GameObject("OkImg");
			obj4.transform.parent = base.transform;
			targetImage = obj4.AddComponent<Image>();
			obj4.transform.localPosition = Vector3.zero;
			obj4.transform.localScale = Vector3.one;
			((RectTransform)obj4.transform).sizeDelta = new Vector2(140f, 140f);
			loadIconCallBack.TargetImage = targetImage;
			OkImg = targetImage;
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>("texture/prototype/battle", "Orange_UI_battle_Button_Skill_BG_FinishBar", loadIconCallBack.LoadCB);
			GameObject obj5 = new GameObject("OkBg");
			obj5.transform.parent = base.transform;
			targetImage = obj5.AddComponent<Image>();
			obj5.transform.localPosition = Vector3.zero;
			obj5.transform.localScale = Vector3.one;
			((RectTransform)obj5.transform).sizeDelta = new Vector2(100f, 100f);
			loadIconCallBack.TargetImage = targetImage;
			OkBg = targetImage;
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>("texture/prototype/battle", "Orange_UI_battle_Button_Skill_BG_Finish", loadIconCallBack.LoadCB);
			RecahrgeImg.gameObject.SetActive(false);
			RecahrgeBg.gameObject.SetActive(false);
		}

		private void Update()
		{
			if (fNowChangeTime > 0f)
			{
				fNowChangeTime -= Time.deltaTime;
				OkImg.gameObject.SetActive(false);
				OkBg.gameObject.SetActive(false);
				RecahrgeImg.gameObject.SetActive(true);
				if (RecahrgeImg.type != Image.Type.Filled)
				{
					RecahrgeImg.type = Image.Type.Filled;
				}
				if (RecahrgeBg.type != Image.Type.Filled)
				{
					RecahrgeBg.type = Image.Type.Filled;
				}
				skillimg.color = new Color(1f, 1f, 1f, 0.5f);
				RecahrgeImg.fillAmount = fNowChangeTime / fChangeTime;
				RecahrgeBg.fillAmount = fNowChangeTime / fChangeTime;
			}
			else
			{
				RecahrgeImg.gameObject.SetActive(false);
				OkImg.gameObject.SetActive(true);
				OkBg.gameObject.SetActive(true);
				skillimg.color = Color.white;
				base.enabled = false;
			}
		}

		public void ReChange(float fTime)
		{
			base.enabled = true;
			fNowChangeTime = (fChangeTime = fTime);
		}
	}
}
