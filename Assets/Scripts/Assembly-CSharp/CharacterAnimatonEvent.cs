#define RELEASE
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimatonEvent : MonoBehaviour
{
    public static readonly string FUNCTION_NAME_PLAYSE = "PlaySE";

    private readonly string PART_HAND_OLD = "L_HandMesh";

    private readonly string PART_HAND = "HandMesh_L";

    private Transform[] transformChilds;

    private CharacterMaterial[] characterMaterials;

    private bool isVisible = true;

    public bool IgnoreAnimEvents { get; set; }

    private void Awake()
    {
        IgnoreAnimEvents = false;
        transformChilds = GetComponentsInChildren<Transform>(true);
        characterMaterials = GetComponents<CharacterMaterial>();
    }

    private void OnEnable()
    {
        Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.CHARACTER_RT_VISIBLE, SetAudioMute);
    }

    private void OnDisable()
    {
        Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.CHARACTER_RT_VISIBLE, SetAudioMute);
    }

    public void SetAudioMute(bool bVisible)
    {
        isVisible = bVisible;
    }

    public void ShowStartMask()
    {
        if (!IgnoreAnimEvents)
        {
            CharacterMaterial[] array = characterMaterials;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].UpdateMask(0, 4f);
            }
        }
    }

    public void StopStartMask()
    {
        if (!IgnoreAnimEvents)
        {
            CharacterMaterial[] array = characterMaterials;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].SetEmissionFadeOut();
            }
        }
    }

    public void SetGameObjectAppear(AnimationEvent e)
    {
        if (IgnoreAnimEvents)
        {
            return;
        }
        string[] array = e.stringParameter.Split(',');
        foreach (string key in array)
        {
            Transform transform = OrangeBattleUtility.FindChildRecursive(ref transformChilds, key, true);
            if (transform == null)
            {
                continue;
            }
            CharacterMaterial cm = transform.GetComponent<CharacterMaterial>();
            if ((bool)cm)
            {
                transform.gameObject.SetActive(true);
                cm.gameObject.AddOrGetComponent<CharacterAnimatonEventEx>().AddDisableCB(delegate
                {
                    cm.UpdateProperty();
                });
                cm.ChangeDissolveTime(e.floatParameter);
                cm.SetBaseRenderForUI();
                if (e.floatParameter > 0f)
                {
                    cm.Appear(delegate
                    {
                        cm.SetDissolveValue(cm.DefaultDissolveValue);
                    });
                }
                else
                {
                    cm.SetDissolveValue(0);
                    cm.UpdatePropertyBlockWithSub();
                }
            }
            else
            {
                CharacterMaterialChildPass component = transform.GetComponent<CharacterMaterialChildPass>();
                if ((bool)component)
                {
                    component.Appear(e.floatParameter);
                }
                else
                {
                    transform.gameObject.SetActive(true);
                }
            }
        }
    }

    public void SetGameObjectDisappear(AnimationEvent e)
    {
        if (IgnoreAnimEvents)
        {
            return;
        }
        string[] array = e.stringParameter.Split(',');
        foreach (string key in array)
        {
            Transform transform = OrangeBattleUtility.FindChildRecursive(ref transformChilds, key, true);
            if ((bool)transform)
            {
                CharacterMaterialChildPass component = transform.GetComponent<CharacterMaterialChildPass>();
                if ((bool)component)
                {
                    component.Disappear(e.floatParameter);
                }
                else
                {
                    transform.gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetHandAppear(AnimationEvent e)
    {
        if (!IgnoreAnimEvents)
        {
            CharacterMaterial[] array = characterMaterials;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].AppearSelect(PART_HAND_OLD, e.floatParameter);
            }
            array = characterMaterials;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].AppearSelect(PART_HAND, e.floatParameter);
            }
        }
    }

    public void SetHandDisapper(AnimationEvent e)
    {
        if (!IgnoreAnimEvents)
        {
            CharacterMaterial[] array = characterMaterials;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].DisappearSelect(PART_HAND_OLD, e.floatParameter);
            }
            array = characterMaterials;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].DisappearSelect(PART_HAND, e.floatParameter);
            }
        }
    }

    public void PlayFx(AnimationEvent e)
    {
        if (IgnoreAnimEvents)
        {
            return;
        }
        GameObject gameObject = (GameObject)e.objectReferenceParameter;
        if (gameObject == null)
        {
            Debug.LogWarning("[CharacterAnimatonEvent] PlayFx : GameObject is NULL!");
            return;
        }
        Transform parent = ((e.stringParameter != string.Empty) ? OrangeBattleUtility.FindChildRecursive(ref transformChilds, e.stringParameter, true) : base.transform);
        GameObject gameObject2 = Object.Instantiate(gameObject);
        gameObject2.transform.SetParent(parent, false);
        if (e.intParameter == 1)
        {
            gameObject2.transform.SetParent(base.transform, true);
        }
        OrangeBattleUtility.ChangeLayersRecursively<Transform>(gameObject2.transform, base.gameObject.layer);
        gameObject2.SetActive(true);
        gameObject2.GetComponentInChildren<ParticleSystem>().Play();
    }

    public void PlayFxScale(AnimationEvent e)
    {
        if (!IgnoreAnimEvents)
        {
            GameObject original = (GameObject)e.objectReferenceParameter;
            Transform parent = ((e.stringParameter != string.Empty) ? OrangeBattleUtility.FindChildRecursive(ref transformChilds, e.stringParameter, true) : base.transform);
            GameObject gameObject = Object.Instantiate(original);
            gameObject.transform.SetParent(parent, false);
            if (e.intParameter == 1)
            {
                gameObject.transform.SetParent(base.transform, true);
            }
            OrangeBattleUtility.ChangeLayersRecursively<Transform>(gameObject.transform, base.gameObject.layer);
            gameObject.SetActive(true);
            float floatParameter = e.floatParameter;
            if (floatParameter != 0f)
            {
                gameObject.transform.localScale = new Vector3(floatParameter, floatParameter, floatParameter);
            }
            gameObject.GetComponent<ParticleSystem>().Play();
        }
    }

    public void PlaySE(AnimationEvent e)
    {
        if (IgnoreAnimEvents || !isVisible)
        {
            return;
        }
        string[] split = e.stringParameter.Split(',');
        if (split.Length > 1)
        {
            int p_channel = (split[0].StartsWith("VOICE") ? 3 : 2);
            MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(split[0], p_channel, delegate
            {
                MonoBehaviourSingleton<AudioManager>.Instance.Play(split[0], split[1]);
            });
        }
    }

    public void CameraShake(AnimationEvent e)
    {
        if (!IgnoreAnimEvents)
        {
            MonoBehaviourSingleton<UIManager>.Instance.Eft_ShakeUI();
        }
    }

    public void CameraShakeX(AnimationEvent e)
    {
        if (!IgnoreAnimEvents)
        {
            string[] array = e.stringParameter.Split(',');
            if (array.Length == 3)
            {
                float result = 0f;
                float result2 = 0f;
                float result3 = 0f;
                float.TryParse(array[0], out result);
                float.TryParse(array[1], out result2);
                float.TryParse(array[2], out result3);
                MonoBehaviourSingleton<UIManager>.Instance.Eft_ShakeX(result, result2, result3);
            }
        }
    }

    public void CameraShakeY(AnimationEvent e)
    {
        if (!IgnoreAnimEvents)
        {
            string[] array = e.stringParameter.Split(',');
            if (array.Length == 3)
            {
                float result = 0f;
                float result2 = 0f;
                float result3 = 0f;
                float.TryParse(array[0], out result);
                float.TryParse(array[1], out result2);
                float.TryParse(array[2], out result3);
                MonoBehaviourSingleton<UIManager>.Instance.Eft_ShakeY(result, result2, result3);
            }
        }
    }

    public void UICharaBonusCount(AnimationEvent e)
    {
        int p_param = e.intParameter;
        if (e.intParameter == 0)
        {
            p_param = 23;
        }
        Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_CHARACTERINFO_BONUS_COUNT, p_param);
    }

    public void Sunshine(AnimationEvent e)
    {
        Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CHARACTER_RT_SUNSHINE, e.intParameter, e.floatParameter);
    }

    public void UpdateTexture(AnimationEvent e)
    {
        if (!IgnoreAnimEvents && e.intParameter < characterMaterials.Length)
        {
            Texture texture = (Texture)e.objectReferenceParameter;
            if ((bool)texture)
            {
                characterMaterials[e.intParameter].UpdateTex(texture);
            }
        }
    }

    public void Dialog(AnimationEvent e)
    {
        if (!IgnoreAnimEvents)
        {
            Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CHARACTER_RT_DIALOG, e.intParameter == 0, e.floatParameter);
        }
    }

    public void CameraFOV(AnimationEvent e)
    {
        if (!IgnoreAnimEvents)
        {
            Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.RT_UPDATE_CAMERA_FOV, e.intParameter, e.floatParameter);
        }
    }

    public void Emission(AnimationEvent e)
    {
        if (!IgnoreAnimEvents && e.intParameter < characterMaterials.Length)
        {
            characterMaterials[e.intParameter].UpdateEmission(e.floatParameter);
        }
    }
}
