using System.IO;
using UnityEngine;

[CreateAssetMenu]
public class AssetBundleScriptableObject : ScriptableObject
{
	public enum ShowcaseType
	{
		buster = 0,
		chip = 1,
		fs = 2,
		gatling = 3,
		launcher = 4,
		mgun = 5,
		ps = 6,
		saber = 7,
		skill = 8,
		spray = 9,
		sprayheavy = 10
	}

	private static AssetBundleScriptableObject m_instance;

	public static string CONFIG_NAME = "abconfig";

	public bool m_useAssetBundle = true;

	public bool m_useDebugLocalPath;

	public string m_bundlePath_Android = "AssetBundles/Android/";

	public string m_bundlePath_iOS = "AssetBundles/iOS/";

	public string m_bundlePath_StandaloneWindows = "AssetBundles/StandaloneWindows/";

	public string m_bundlePath_Switch = "AssetBundles/Switch/";

	public int m_retryCountMax = 20;

	public float m_retryTime = 1f;

	public string m_tablePath = "table/";

	public string m_uiPath = "ui/";

	public string m_audioBgmPath = "audio/bgm/";

	public string m_audioSePath = "audio/sound/";

	public string m_audioVoicePath = "audio/voice/";

	public string m_iconSkillPath = "texture/2d/icon/skill";

	public string m_iconWeapon = "texture/2d/icon/weapon";

	public string m_iconChip = "texture/2d/icon/chip";

	public string m_iconItem = "texture/2d/icon/item";

	public string m_iconEquip = "texture/2d/icon/equip";

	public string m_iconCharacter = "texture/2d/icon/character";

	public string m_iconBossHead = "texture/2d/icon/bosshead";

	public string m_iconStageBg = "texture/2d/stage/icon";

	public string m_iconShowcase = "texture/2d/icon/showcase";

	public string m_iconGallery = "texture/2d/icon/gallery";

	public string m_stageBg = "texture/2d/stage/sbg";

	public string m_prefabEnemy = "prefab/enemy/";

	public string m_iconCard = "texture/2d/icon/card/";

	public string m_icon_card_l_format = "card_patch{0}_large";

	public string m_icon_card_m_format = "card_patch{0}_mid";

	public string m_icon_card_s_format = "card_patch{0}_small";

	public string m_common_icon = "texture/prototype/common/";

	public string m_icon_rare = "texture/prototype/rare";

	public string m_iconSign = "texture/2d/icon/sign";

	public string m_iconCrusade = "texture/2d/icon/crusade";

	public string m_icon_rare_bg_format = "ui_iconsource_bg_{0}";

	public string m_icon_rare_bg_small_format = "ui_iconsource_bg_{0}_small";

	public string m_icon_rare_frame_format = "ui_iconsource_frame_{0}_l";

	public string m_icon_rare_frame_small_format = "ui_iconsource_frame_{0}";

	public string m_icon_powerup_lv_format = "ui_iconsource_powerupicon_Lv{0}";

	public string m_icon_rare_word_format = "ui_common_word_{0}";

	public string m_icon_rare2_word_format = "ui_battleend_rank_{0}";

	public string m_texture_ui_hometop = "texture/2d/ui/ui_hometop";

	public string m_texture_ui_chat_path = "texture/2d/ui/ui_chat/";

	public string m_chat_pkg_icon_format = "E_ICON_X{0:00}";

	public string m_chat_emotion_icon_format = "E_TEXTURE_X{0:00}_{1:000}";

	public string m_texture_ui_common = "texture/2d/ui/+ui_common";

	public string m_texture_ui_sub_common = "texture/2d/ui/+ui_subcommon";

	public string m_texture_ui_story = "texture/2d/ui/ui_story";

	public string m_texture_ui_event = "texture/2d/ui/ui_event";

	private string m_texture_ui_story_icon = "UI_story_icon{0}_00";

	public string m_texture_2d_stand_st = "texture/2d/stand/{0}";

	public string m_texture_icon_boss_intro = "texture/2d/icon/bossintro";

	public string m_texture_scenario = "texture/2d/scenario/{0}";

	public string m_texture_loading = "texture/2d/loading";

	public string m_texture_shop = "texture/2d/shop";

	public string m_newmodel_weapon = "newmodel/weapon/";

	public string m_dragonbones_chdb = "dragonbones/{0}_db";

	public string m_texture_ui_record = "texture/2d/ui/ui_record";

	private string m_iconSkillPathNew = "texture/2d/icon/new_skill/";

	private string m_iconSignPathNew = "texture/2d/icon/new_sign/";

	private string m_iconItemPathNew = "texture/2d/icon/new_item/";

	public static AssetBundleScriptableObject Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = Resources.Load<AssetBundleScriptableObject>("AssetBundleScriptableObject");
			}
			return m_instance;
		}
	}

	public string GetEmotionPkgBundle(int pkgID)
	{
		return m_texture_ui_chat_path + string.Format(m_chat_pkg_icon_format, pkgID);
	}

	public string GetEmotionTextureName(int pkgID, int TextureID)
	{
		return string.Format(m_chat_emotion_icon_format, pkgID, TextureID);
	}

	public string GetDebugLocalPath()
	{
		return new DirectoryInfo(Application.dataPath).Parent.FullName + "/";
	}

	public string GetIconRareBg(int p_rare)
	{
		return string.Format(m_icon_rare_bg_format, RareIntToStr(p_rare));
	}

	public string GetIconRareBgSmall(int p_rare)
	{
		return string.Format(m_icon_rare_bg_small_format, RareIntToStr(p_rare));
	}

	public string GetIconRareFrame(int p_rare)
	{
		return string.Format(m_icon_rare_frame_format, RareIntToStr(p_rare));
	}

	public string GetIconRareFrameSmall(int p_rare)
	{
		return string.Format(m_icon_rare_frame_small_format, RareIntToStr(p_rare));
	}

	public string GetPowerUpLvIcon(int p_level)
	{
		return string.Format(m_icon_powerup_lv_format, p_level);
	}

	public string GetIconRareWord(int p_rare)
	{
		return string.Format(m_icon_rare_word_format, RareIntToStr(p_rare));
	}

	public string GetShowcase(string s_SHOWCASE)
	{
		string[] array = s_SHOWCASE.Split('_');
		if (array.Length >= 3)
		{
			if (array[1] == ShowcaseType.skill.ToString() || array[1] == ShowcaseType.ps.ToString())
			{
				return m_iconShowcase + "/" + array[1] + "/" + array[2].Substring(0, 2);
			}
			return m_iconShowcase + "/" + array[1];
		}
		return m_iconShowcase;
	}

	public string GetIconCharacter(string s_ICON)
	{
		string[] array = s_ICON.Split('_');
		if (array.Length >= 3)
		{
			return m_iconCharacter + "/" + array[1] + "/" + array[2].Substring(0, 2);
		}
		return m_iconCharacter;
	}

	public string GetIconCharacter2(string s_ICON)
	{
		string[] array = s_ICON.Split('_');
		if (array.Length >= 3)
		{
			return m_iconCharacter + "2/" + array[1] + "/" + array[2].Substring(0, 2);
		}
		return m_iconCharacter;
	}

	public string GetIconSkill(string s_ICON)
	{
		string[] array = s_ICON.Split('_');
		if (array.Length >= 3)
		{
			return m_iconSkillPathNew + array[1] + "/" + array[2].Substring(0, 2);
		}
		return m_iconSkillPath;
	}

	public string GetIconSign(string s_ICON)
	{
		string[] array = s_ICON.Split('_');
		if (array.Length >= 4)
		{
			return m_iconSignPathNew + array[2].Substring(1, 2) + "/" + array[3].Substring(0, 2);
		}
		return m_iconSkillPath;
	}

	public string GetIconItem(string s_ICON)
	{
		string[] array = s_ICON.Split('_');
		if (array.Length >= 3)
		{
			return m_iconItemPathNew + array[1];
		}
		return m_iconSkillPath;
	}

	public string GetIconRare2Word(int p_rare)
	{
		return string.Format(m_icon_rare2_word_format, RareIntToStr(p_rare));
	}

	public string RareIntToStr(int p_rare)
	{
		switch (p_rare)
		{
		case 2:
			return "C";
		case 3:
			return "B";
		case 4:
			return "A";
		case 5:
			return "S";
		case 6:
			return "SS";
		default:
			return "D";
		}
	}

	public string GetStEnemyBundleName(int idx, out string assetName)
	{
		switch (idx)
		{
		case 0:
			assetName = "st_enemy_cf0";
			return string.Format(m_texture_2d_stand_st, "st_enemy_cf0");
		case 1:
			assetName = "st_enemy_eagle";
			return string.Format(m_texture_2d_stand_st, "st_enemy_eagle");
		case 2:
			assetName = "st_enemy_penguin";
			return string.Format(m_texture_2d_stand_st, "st_enemy_penguin");
		default:
			assetName = string.Empty;
			return string.Empty;
		}
	}

	public string GetStoryIconFrame(int n_DIFFICULTY)
	{
		return string.Format(m_texture_ui_story_icon, n_DIFFICULTY.ToString("00"));
	}
}
