using System;
using CallbackDefs;
using StageLib;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
#endif

public class CharacterMaterial : MonoBehaviour
{
    [SerializeField]
    private Renderer[] renderers;

    [SerializeField]
    private Texture MainTex;

    [SerializeField]
    private Texture Mask2;

    [SerializeField]
    private Texture SpecColorTex;

    [Range(0f, 10f)]
    [SerializeField]
    private float EmissionIntensity = 2f;

    [Range(0f, 1f)]
    [SerializeField]
    private float RimMin = 0.3f;

    [Range(0f, 1f)]
    [SerializeField]
    private float RimMax = 1f;

    [SerializeField]
    private Color OutlineColor = new Color(0.142f, 0.168f, 0.227f);

    [SerializeField]
    private float Outline = 0.3f;

    [ColorUsage(true, true)]
    [SerializeField]
    private Color DissolveEdge = Color.white;

    [Range(0f, 1f)]
    public int DefaultDissolveValue;

    [Range(-10f, 10f)]
    [SerializeField]
    private float DissolveModelHeight = 2.5f;

    [Range(0f, 2f)]
    [SerializeField]
    private float DissolveTime = 0.8f;

    [SerializeField]
    private Texture[] OtherTex;

    [SerializeField]
    private Texture[] OtherMask;

    private float HurtTime = 0.04f;

    private int AppearTweenUid = -1;

    private int DisappearTweenUid = -1;

    private int AppearSelectTweenUid = -1;

    private int DisappearSelectTweenUid = -1;

    private int SetEmissionFadeOutTweenUid = -1;

    private int HurtTweenUid = -1;

    [SerializeField]
    private Transform EffectOBJ;

    [Range(0f, 1f)]
    [SerializeField]
    private float RampThreshold = 0.28f;

    [Range(0.001f, 1f)]
    [SerializeField]
    private float RampSmooth = 0.001f;

    protected MaterialPropertyBlock mpb;

    [SerializeField]
    private CharacterMaterial SubCharacterMaterials;

    private bool isCustomRender;

    public bool isAllowHurtEffect = true;

    private bool needRecoverMask;

    public Color HurtColor { get; set; } = Color.white;


    public Color RimColor { get; set; } = new Color(1f, 1f, 1f, 0.15f);

    public Renderer MainRenderer => renderers[0];


    public int GetTexturesCount
    {
        get
        {
            if (OtherTex != null)
            {
                return OtherTex.Length;
            }
            return 0;
        }
    }

    public CharacterMaterial GetSubCharacterMaterials
    {
        get
        {
            return SubCharacterMaterials;
        }
    }

    public bool IsAllowInvincibleEffect { get; set; } = true;


    protected void Awake()
    {
        isAllowHurtEffect = true;
        isCustomRender = !IsRenderersExist();
        if (isCustomRender)
        {
            return;
        }
        mpb = new MaterialPropertyBlock();
        UpdateProperty();
        if (EffectOBJ == null)
        {
            EffectOBJ = base.transform.Find("FX");
        }
        if ((bool)EffectOBJ && MonoBehaviourSingleton<OrangeSceneManager>.Instance.IsBattleScene)
        {
            EffectOBJ.gameObject.SetActive(DefaultDissolveValue == 0);
        }
        if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.IsBattleScene)
        {
            SetBaseRenderForBattle();
            if ((bool)base.gameObject.GetComponent<CharacterAnimatonEvent>())
            {
                Renderer[] array = renderers;
                foreach (Renderer renderer in array)
                {
                    renderer.gameObject.AddComponent<CharacterMaterialChildPass>().Init(renderer, mpb, this);
                }
            }
            MonoBehaviourSingleton<OrangeBattleUtility>.Instance.WeaponForceRotate(base.gameObject);
        }
        else
        {
            Renderer[] array = renderers;
            foreach (Renderer renderer2 in array)
            {
                renderer2.gameObject.AddComponent<CharacterMaterialChildPass>().Init(renderer2, mpb, this);
            }
        }
    }

    public void UpdateProperty(bool setProperty = true)
    {
        if (isCustomRender) return;

        var instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;

        MainRenderer.GetPropertyBlock(mpb);

        if (MainTex) mpb.SetTexture(instance.i_MainTex, MainTex);
        mpb.SetFloat(instance.i_RampThreshold, RampThreshold);
        mpb.SetFloat(instance.i_RampSmooth, RampSmooth);

        if (Mask2)
        {
            mpb.SetTexture(instance.i_Mask2, Mask2);
            mpb.SetFloat(instance.i_Intensity, EmissionIntensity);
        }
        else mpb.SetFloat(instance.i_Intensity, 0f);

        if (SpecColorTex) mpb.SetTexture(instance.i_SpecColorTex, SpecColorTex);

        mpb.SetColor(instance.i_RimColor, RimColor);
        mpb.SetFloat(instance.i_RimMin, RimMin);
        mpb.SetFloat(instance.i_RimMax, RimMax);
        mpb.SetColor(instance.i_OutlineColor, OutlineColor);
        mpb.SetFloat(instance.i_Outline, Outline);
        mpb.SetColor(instance.i_DissolveEdge, DissolveEdge);

        if (Mathf.Abs(DissolveModelHeight) < 2.5f) DissolveModelHeight = 2.5f;

        mpb.SetFloat(instance.i_DissolveModelHeight, DissolveModelHeight);
        mpb.SetFloat(instance.i_DissolveValue, DefaultDissolveValue);

        if (setProperty) UpdatePropertyBlock();
    }

    public bool IsRenderersExist()
    {
        if (renderers != null)
        {
            return renderers.Length != 0;
        }
        return false;
    }

    public int Appear(Callback callback = null, float overrideDissolveTime = -1f)
    {
        if (EffectOBJ != null)
        {
            EffectOBJ.gameObject.SetActive(true);
        }
        if (isCustomRender)
        {
            callback.CheckTargetToInvoke();
            return -1;
        }
        if (SubCharacterMaterials != null)
        {
            SubCharacterMaterials.Appear(null, overrideDissolveTime);
        }
        OrangeMaterialProperty prop = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
        ClearMaterialTween(true);
        mpb.SetFloat(prop.i_RimMin, RimMin);
        mpb.SetFloat(prop.i_RimMax, RimMax);
        mpb.SetColor(prop.i_RimColor, RimColor);
        float @float = mpb.GetFloat(prop.i_DissolveValue);
        float num = ((overrideDissolveTime == -1f) ? DissolveTime : overrideDissolveTime);
        Callback m_cb = callback;
        if (num == 0f)
        {
            mpb.SetFloat(prop.i_DissolveValue, 0f);
            AppearTweenUid = -1;
            UpdatePropertyBlock();
            m_cb.CheckTargetToInvoke();
            return -1;
        }
        return AppearTweenUid = LeanTween.value(base.gameObject, @float, 0f, num).setOnUpdate(delegate (float val)
        {
            mpb.SetFloat(prop.i_DissolveValue, val);
            UpdatePropertyBlock();
        }).setOnComplete((Action)delegate
        {
            AppearTweenUid = -1;
            UpdatePropertyBlock();
            m_cb.CheckTargetToInvoke();
        })
            .uniqueId;
    }

    public void AppearCanStopNoInterrupt(Callback callback = null, float overrideDissolveTime = -1f)
    {
        if (EffectOBJ != null)
        {
            EffectOBJ.gameObject.SetActive(true);
        }
        if (isCustomRender)
        {
            callback.CheckTargetToInvoke();
            return;
        }
        if (SubCharacterMaterials != null)
        {
            SubCharacterMaterials.AppearCanStopNoInterrupt(null, overrideDissolveTime);
        }
        OrangeMaterialProperty prop = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
        ClearMaterialTween(true);
        mpb.SetFloat(prop.i_RimMin, RimMin);
        mpb.SetFloat(prop.i_RimMax, RimMax);
        mpb.SetColor(prop.i_RimColor, RimColor);
        float @float = mpb.GetFloat(prop.i_DissolveValue);
        float fTime = ((overrideDissolveTime == -1f) ? DissolveTime : overrideDissolveTime);
        Callback m_cb = callback;
        StartCoroutine(StageResManager.TweenFloatCoroutine(@float, 0f, fTime, delegate (float val)
        {
            mpb.SetFloat(prop.i_DissolveValue, val);
            UpdatePropertyBlock();
        }, delegate
        {
            UpdatePropertyBlock();
            m_cb.CheckTargetToInvoke();
        }));
    }

    public int AppearX(Callback callback = null, float overrideDissolveTime = -1f)
    {
        if (EffectOBJ != null)
        {
            EffectOBJ.gameObject.SetActive(true);
        }
        if (isCustomRender)
        {
            callback.CheckTargetToInvoke();
            return -1;
        }
        if (SubCharacterMaterials != null)
        {
            SubCharacterMaterials.AppearX(null, overrideDissolveTime);
        }
        OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
        ClearMaterialTween(true);
        mpb.SetFloat(instance.i_RimMin, RimMin);
        mpb.SetFloat(instance.i_RimMax, RimMax);
        mpb.SetColor(instance.i_RimColor, RimColor);
        mpb.GetFloat(instance.i_DissolveValue);
        if (overrideDissolveTime == -1f)
        {
            float dissolveTime = DissolveTime;
        }
        mpb.SetFloat(instance.i_DissolveValue, 0f);
        UpdatePropertyBlock();
        callback.CheckTargetToInvoke();
        return -1;
    }

    public int Disappear(Callback callback = null, float overrideDissolveTime = -1f)
    {
        if (EffectOBJ != null)
        {
            EffectOBJ.gameObject.SetActive(false);
        }
        if (isCustomRender)
        {
            callback.CheckTargetToInvoke();
            return -1;
        }
        if (SubCharacterMaterials != null)
        {
            SubCharacterMaterials.Disappear(null, overrideDissolveTime);
        }
        OrangeMaterialProperty prop = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
        ClearMaterialTween(true);
        float @float = mpb.GetFloat(prop.i_DissolveValue);
        Callback m_cb = callback;
        float time = ((overrideDissolveTime == -1f) ? DissolveTime : overrideDissolveTime);
        return DisappearTweenUid = LeanTween.value(base.gameObject, @float, 1f, time).setOnUpdate(delegate (float val)
        {
            mpb.SetFloat(prop.i_DissolveValue, val);
            UpdatePropertyBlock();
        }).setOnComplete((Action)delegate
        {
            DisappearTweenUid = -1;
            if (m_cb != null)
            {
                m_cb();
                m_cb = null;
            }
        })
            .uniqueId;
    }

    public void DisappearX()
    {
        if (!isCustomRender)
        {
            if (SubCharacterMaterials != null)
            {
                SubCharacterMaterials.DisappearX();
            }
            OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
            mpb.SetFloat(instance.i_DissolveValue, 1f);
            UpdatePropertyBlock();
        }
    }

    public int AppearSelect(string renderName, float dissolveTime = 0.5f, Callback callback = null)
    {
        if (isCustomRender)
        {
            callback.CheckTargetToInvoke();
            return -1;
        }
        Renderer[] r = Array.FindAll(renderers, (Renderer x) => x.name.Contains(renderName));
        if (r.Length == 0)
        {
            return -1;
        }
        OrangeMaterialProperty prop = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
        ClearMaterialTween(true);
        float @float = mpb.GetFloat(prop.i_DissolveValue);
        if (dissolveTime > 0f)
        {
            Callback m_cb = callback;
            return AppearSelectTweenUid = LeanTween.value(base.gameObject, @float, 0f, dissolveTime).setOnUpdate(delegate (float val)
            {
                mpb.SetFloat(prop.i_DissolveValue, val);
                Renderer[] array2 = r;
                for (int j = 0; j < array2.Length; j++)
                {
                    array2[j].SetPropertyBlock(mpb);
                }
            }).setOnComplete((Action)delegate
            {
                mpb.SetFloat(prop.i_DissolveValue, 1f);
                m_cb.CheckTargetToInvoke();
                AppearSelectTweenUid = -1;
            })
                .uniqueId;
        }
        mpb.SetFloat(prop.i_DissolveValue, 0f);
        Renderer[] array = r;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].SetPropertyBlock(mpb);
        }
        mpb.SetFloat(prop.i_DissolveValue, 1f);
        callback.CheckTargetToInvoke();
        AppearSelectTweenUid = -1;
        return AppearSelectTweenUid;
    }

    public int DisappearSelect(string renderName, float dissolveTime = 0.5f, Callback callback = null)
    {
        if (isCustomRender)
        {
            callback.CheckTargetToInvoke();
            return -1;
        }
        Renderer[] r = Array.FindAll(renderers, (Renderer x) => x.name.Contains(renderName));
        if (r.Length == 0)
        {
            return -1;
        }
        OrangeMaterialProperty prop = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
        ClearMaterialTween(true);
        float @float = mpb.GetFloat(prop.i_DissolveValue);
        Callback m_cb = callback;
        return DisappearSelectTweenUid = LeanTween.value(base.gameObject, @float, 1f, DissolveTime).setOnUpdate(delegate (float val)
        {
            mpb.SetFloat(prop.i_DissolveValue, val);
            Renderer[] array = r;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].SetPropertyBlock(mpb);
            }
            mpb.SetFloat(prop.i_DissolveValue, 0f);
        }).setOnComplete((Action)delegate
        {
            mpb.SetFloat(prop.i_DissolveValue, 0f);
            m_cb.CheckTargetToInvoke();
            DisappearSelectTweenUid = -1;
        })
            .uniqueId;
    }

    public float GetDissolveValue()
    {
        OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
        return mpb.GetFloat(instance.i_DissolveValue);
    }

    public void SetDissolveValue(int val)
    {
        OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
        mpb.SetFloat(instance.i_DissolveValue, val);
        if (SubCharacterMaterials != null)
        {
            SubCharacterMaterials.SetDissolveValue(val);
        }
    }

    public Texture GetMainTex()
    {
        return MainTex;
    }

    public Texture GetMainMask()
    {
        return Mask2;
    }

    public void UpdateTex(int idx = -1)
    {
        if (!isCustomRender)
        {
            mpb.SetTexture(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_MainTex, (idx >= 0 && idx < OtherTex.Length) ? OtherTex[idx] : MainTex);
            UpdatePropertyBlock();
        }
    }

    public void UpdateTex(Texture texture)
    {
        if (!isCustomRender)
        {
            mpb.SetTexture(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_MainTex, texture);
            UpdatePropertyBlock();
        }
    }

    public void UpdateSpecColorTex(int idx = -1)
    {
        if (!isCustomRender)
        {
            mpb.SetTexture(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_SpecColorTex, (idx >= 0 && idx < OtherTex.Length) ? OtherTex[idx] : SpecColorTex);
            UpdatePropertyBlock();
        }
    }

    public void UpdateMask(int idx, float emissionIntensity)
    {
        if (!isCustomRender)
        {
            Texture texture = ((idx >= 0 && idx < OtherMask.Length) ? OtherMask[idx] : Mask2);
            if (null != texture)
            {
                mpb.SetTexture(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Mask2, texture);
            }
            mpb.SetFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Intensity, emissionIntensity);
            needRecoverMask = true;
            UpdatePropertyBlock();
            if (SubCharacterMaterials != null)
            {
                SubCharacterMaterials.UpdateMask(idx, emissionIntensity);
            }
        }
    }

    public void RecoverOriginalMaskSetting()
    {
        if (needRecoverMask)
        {
            needRecoverMask = false;
            if (Mask2 != null)
            {
                mpb.SetTexture(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Mask2, Mask2);
                mpb.SetFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Intensity, EmissionIntensity);
            }
            else
            {
                mpb.SetFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Intensity, 0f);
            }
            UpdatePropertyBlock();
            if (SubCharacterMaterials != null)
            {
                SubCharacterMaterials.RecoverOriginalMaskSetting();
            }
        }
    }

    public void UpdateEmission(float emissionIntensity)
    {
        EmissionIntensity = emissionIntensity;
        mpb.SetFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Intensity, emissionIntensity);
        UpdatePropertyBlock();
    }

    public void SetEmissionFadeOut()
    {
        float @float = mpb.GetFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Intensity);
        SetEmissionFadeOutTweenUid = LeanTween.value(base.gameObject, @float, 0f, 0.15f).setOnUpdate(delegate (float f)
        {
            mpb.SetFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Intensity, f);
            UpdatePropertyBlock();
        }).setOnComplete((Action)delegate
        {
            SetEmissionFadeOutTweenUid = -1;
        })
            .uniqueId;
    }

    public int Invincible(Callback callback = null)
    {
        if (HurtTweenUid != -1)
        {
            return HurtTweenUid;
        }
        if (isCustomRender)
        {
            callback.CheckTargetToInvoke();
            return -1;
        }
        if (!IsAllowInvincibleEffect)
        {
            return -1;
        }
        if (SubCharacterMaterials != null)
        {
            SubCharacterMaterials.Invincible();
        }
        OrangeMaterialProperty prop = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
        mpb.SetFloat(prop.i_RimMax, 0.5f);
        mpb.SetColor(prop.i_RimColor, prop.IColor);
        mpb.SetFloat(prop.i_DissolveValue, 0f);
        int flag = 0;
        return HurtTweenUid = LeanTween.value(base.gameObject, 0f, OrangeConst.PVE_IFRAME_DURATION, (float)OrangeConst.PVE_IFRAME_DURATION * 0.001f).setOnUpdate((Action<float>)delegate
        {
            if (flag % 4 < 2)
            {
                mpb.SetFloat(prop.i_RimMin, 0f);
            }
            else
            {
                mpb.SetFloat(prop.i_RimMin, 0.5f);
            }
            flag++;
            UpdatePropertyBlock();
        }).setOnComplete((Action)delegate
        {
            HurtTweenUid = -1;
            mpb.SetFloat(prop.i_RimMin, RimMin);
            mpb.SetFloat(prop.i_RimMax, RimMax);
            mpb.SetColor(prop.i_RimColor, RimColor);
            UpdatePropertyBlock();
            callback.CheckTargetToInvoke();
        })
            .setEaseInQuint()
            .setLoopPingPong(1)
            .uniqueId;
    }

    public int Hurt(Callback callback = null)
    {
        if (!isAllowHurtEffect)
        {
            return -1;
        }
        if (HurtTweenUid != -1)
        {
            return HurtTweenUid;
        }
        if (isCustomRender)
        {
            callback.CheckTargetToInvoke();
            return -1;
        }
        if (SubCharacterMaterials != null)
        {
            SubCharacterMaterials.Hurt();
        }
        OrangeMaterialProperty prop = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
        mpb.SetFloat(prop.i_RimMax, 0.5f);
        mpb.SetColor(prop.i_RimColor, HurtColor);
        mpb.SetFloat(prop.i_DissolveValue, 0f);
        return HurtTweenUid = LeanTween.value(base.gameObject, 0f, 0.5f, HurtTime).setOnUpdate(delegate (float val)
        {
            mpb.SetFloat(prop.i_RimMin, val);
            UpdatePropertyBlock();
        }).setOnComplete((Action)delegate
        {
            HurtTweenUid = -1;
            mpb.SetFloat(prop.i_RimMin, RimMin);
            mpb.SetFloat(prop.i_RimMax, RimMax);
            mpb.SetColor(prop.i_RimColor, RimColor);
            UpdatePropertyBlock();
            callback.CheckTargetToInvoke();
        })
            .setEaseInQuint()
            .setLoopPingPong(1)
            .uniqueId;
    }

    public void UseSimpleShadow()
    {
    }

    public Renderer[] GetRenderer()
    {
        return renderers;
    }

    public Texture[] GetOtherTextures()
    {
        return OtherTex;
    }

    public void SetRenderer(Renderer[] p_renderers)
    {
        renderers = p_renderers;
    }

    private void OnDestroy()
    {
        ClearMaterialTween(false);
        renderers = null;
    }

    private void ClearMaterialTween(bool callOnComplete)
    {
        if (AppearTweenUid != -1)
        {
            LeanTween.cancel(base.gameObject, AppearTweenUid, callOnComplete);
        }
        if (DisappearTweenUid != -1)
        {
            LeanTween.cancel(base.gameObject, DisappearTweenUid, callOnComplete);
        }
        if (AppearSelectTweenUid != -1)
        {
            LeanTween.cancel(base.gameObject, AppearSelectTweenUid, callOnComplete);
        }
        if (DisappearSelectTweenUid != -1)
        {
            LeanTween.cancel(base.gameObject, DisappearSelectTweenUid, callOnComplete);
        }
        if (SetEmissionFadeOutTweenUid != -1)
        {
            LeanTween.cancel(base.gameObject, SetEmissionFadeOutTweenUid, callOnComplete);
        }
        if (HurtTweenUid != -1)
        {
            LeanTween.cancel(base.gameObject, HurtTweenUid, callOnComplete);
        }
    }

    public void SetBaseRenderForUI()
    {
        if (!isCustomRender)
        {
            OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
            mpb.SetColor(instance.i_Color, instance.UI_RenderColor);
            mpb.SetFloat(instance.i_RampThreshold, instance.UI_RampThreshold);
            mpb.SetFloat(instance.i_Smoothness, instance.UI_Smoothness);
            mpb.SetFloat(instance.i_SpecSmooth, instance.UI_SpecSmooth);
            mpb.SetFloat(instance.i_GradientMax, instance.UI_GradientMax);
            mpb.SetFloat(instance.i_RimMin, instance.UI_RimMin);
            mpb.SetFloat(instance.i_RimMax, instance.UI_RimMax);
            mpb.SetColor(instance.i_RimColor, instance.UI_RimColor);
            mpb.SetColor(instance.i_SpecColor, instance.UI_SpecColor);
            mpb.SetFloat(instance.i_Outline, instance.UI_Outline);
            UpdatePropertyBlock();
        }
    }

    public void SetBaseRenderForBattle()
    {
        if (!isCustomRender)
        {
            OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
            if (mpb.GetColor(instance.i_Color).a == 1f)
            {
                mpb.SetColor(instance.i_Color, instance.UI_RenderColor);
            }
            if (Outline < instance.Battle_Outline)
            {
                mpb.SetFloat(instance.i_Outline, instance.Battle_Outline);
            }
            else
            {
                mpb.SetFloat(instance.i_Outline, Outline);
            }
            UpdatePropertyBlock();
        }
    }

    public void ChangeOutlineWidth(float width)
    {
        if (!isCustomRender)
        {
            Outline = width;
            mpb.SetFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Outline, Outline);
            UpdatePropertyBlock();
        }
    }

    public float GetDissolveModelHeight()
    {
        return DissolveModelHeight;
    }

    public void ChangeDissolveModelModelHeight(float height)
    {
        if (!isCustomRender)
        {
            DissolveModelHeight = height;
            mpb.SetFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_DissolveModelHeight, DissolveModelHeight);
            UpdatePropertyBlock();
        }
    }

    public void ChangeDissolveColor(Color dissolvecolor)
    {
        if (!isCustomRender)
        {
            mpb.SetColor(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_DissolveEdge, dissolvecolor);
            UpdatePropertyBlock();
        }
    }

    public void ChangeDissolveTime(float time)
    {
        DissolveTime = time;
    }

    protected virtual void UpdatePropertyBlock()
    {
        Renderer[] array = renderers;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].SetPropertyBlock(mpb);
        }
    }

    public void UpdatePropertyBlockWithSub()
    {
        Renderer[] array = renderers;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].SetPropertyBlock(mpb);
        }
        if (SubCharacterMaterials != null)
        {
            SubCharacterMaterials.UpdatePropertyBlockWithSub();
        }
    }

    public void ClearSubCharacterMaterial()
    {
        SubCharacterMaterials = null;
    }

    public void SetSubCharacterMaterial(CharacterMaterial newSub)
    {
        if (SubCharacterMaterials != null)
        {
            SubCharacterMaterials.SetSubCharacterMaterial(newSub);
        }
        else
        {
            SubCharacterMaterials = newSub;
        }
    }

    public void SetSubCharacterMaterial(GameObject target)
    {
        CharacterMaterial component = target.GetComponent<CharacterMaterial>();
        if ((bool)component)
        {
            SetSubCharacterMaterial(component);
        }
    }

    public void RebuildRendererList()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    public float GetPropertyFloat(int key)
    {
        if (isCustomRender)
        {
            return 0f;
        }
        return mpb.GetFloat(key);
    }

    public void SetPropertyFloat(int key, float val)
    {
        if (!isCustomRender)
        {
            mpb.SetFloat(key, val);
        }
    }

    public void MultiColorColor(float cr, float cg, float cb)
    {
        Color value = new Color(0.6f * cr, 0.6f * cg, 0.6f * cb);
        OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
        mpb.SetColor(instance.i_Color, value);
        UpdatePropertyBlock();
        if (SubCharacterMaterials != null)
        {
            SubCharacterMaterials.MultiColorColor(cr, cg, cb);
        }
    }

    public void ResetPrepertyBlock()
    {
        OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
        mpb.SetFloat(instance.i_RampThreshold, RampThreshold);
        mpb.SetFloat(instance.i_RampSmooth, RampSmooth);
        mpb.SetFloat(instance.i_RimMin, RimMin);
        mpb.SetFloat(instance.i_RimMax, RimMax);
        mpb.SetColor(instance.i_RimColor, RimColor);
        mpb.SetColor(instance.i_OutlineColor, OutlineColor);
        mpb.SetFloat(instance.i_Outline, Outline);
        mpb.SetColor(instance.i_DissolveEdge, DissolveEdge);
        mpb.SetFloat(instance.i_DissolveModelHeight, DissolveModelHeight);
        mpb.SetFloat(instance.i_DissolveValue, DefaultDissolveValue);
        UpdatePropertyBlock();
        if (SubCharacterMaterials != null)
        {
            SubCharacterMaterials.ResetPrepertyBlock();
        }
    }

    public float GetDissloveTime()
    {
        return DissolveTime;
    }


#if UNITY_EDITOR
    [ContextMenu("Update Materials")]
    private void UpdateMaterials_Menu()
    {
        var path = GetPrefabPathFromSelection();
        var hasInvalidPath = string.IsNullOrEmpty(path);
        if (hasInvalidPath) return;

        var folder = GetFolderPath(path);
        var materialsFolder = GetOrAddMaterialFolder(folder);
        var prefab = PrefabUtility.LoadPrefabContents(path);

        foreach (var character in prefab.GetComponents<CharacterMaterial>())
        {
            character.UpdateMaterials(materialsFolder);
        }

        // Save contents back to Prefab Asset and unload contents.
        PrefabUtility.SaveAsPrefabAsset(prefab, path);
        PrefabUtility.UnloadPrefabContents(prefab);
    }

    private static string GetPrefabPathFromSelection()
    {
        // Selection from Scene Hierarchy
        var selection = Selection.activeGameObject;
        var isPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(selection);
        if (isPrefab) return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selection);

        // Selection from Prefab Mode
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        var isPrefabMode = stage != null;
        if (isPrefabMode) return stage.assetPath;

        // Selection from Project Inspector
        selection = Selection.activeObject as GameObject;
        isPrefab = selection && PrefabUtility.IsPartOfPrefabAsset(selection);
        if (isPrefab) return AssetDatabase.GetAssetPath(selection);

        Debug.LogError("You should select a Prefab.");
        return string.Empty;
    }

    private void UpdateMaterials(string materialsFolder)
    {
        var material = CopyMaterial(MainRenderer.sharedMaterial, materialsFolder, gameObject.name);
        if (material == null) return;

        foreach (var renderer in renderers)
        {
            renderer.material = material;
        }

        UpdateProperty_New();
    }

    // Based on UpdateProperty(bool setProperty = true)
    private void UpdateProperty_New()
    {
        var i_MainTex = Shader.PropertyToID("_MainTex");
        var i_RampThreshold = Shader.PropertyToID("_RampThreshold");
        var i_RampSmooth = Shader.PropertyToID("_RampSmooth");
        var i_Mask2 = Shader.PropertyToID("_Mask2");
        var i_Intensity = Shader.PropertyToID("_Intensity");
        var i_SpecColorTex = Shader.PropertyToID("_SpecColorTex");
        var i_RimColor = Shader.PropertyToID("_RimColor");
        var i_RimMin = Shader.PropertyToID("_RimMin");
        var i_RimMax = Shader.PropertyToID("_RimMax");
        var i_OutlineColor = Shader.PropertyToID("_OutlineColor");
        var i_Outline = Shader.PropertyToID("_Outline");
        var i_DissolveValue = Shader.PropertyToID("_DissolveValue");
        var i_DissolveEdge = Shader.PropertyToID("_DissolveEdge");
        var i_DissolveModelHeight = Shader.PropertyToID("_DissolveModelHeight");

        var material = MainRenderer.sharedMaterial;

        if (MainTex) material.SetTexture(i_MainTex, MainTex);
        material.SetFloat(i_RampThreshold, RampThreshold);
        material.SetFloat(i_RampSmooth, RampSmooth);

        if (Mask2)
        {
            material.SetTexture(i_Mask2, Mask2);
            material.SetFloat(i_Intensity, EmissionIntensity);
        }
        else material.SetFloat(i_Intensity, 0f);

        if (SpecColorTex) material.SetTexture(i_SpecColorTex, SpecColorTex);

        material.SetColor(i_RimColor, RimColor);
        material.SetFloat(i_RimMin, RimMin);
        material.SetFloat(i_RimMax, RimMax);
        material.SetColor(i_OutlineColor, OutlineColor);
        material.SetFloat(i_Outline, Outline);
        material.SetColor(i_DissolveEdge, DissolveEdge);

        if (Mathf.Abs(DissolveModelHeight) < 2.5f) DissolveModelHeight = 2.5f;

        material.SetFloat(i_DissolveModelHeight, DissolveModelHeight);
        material.SetFloat(i_DissolveValue, DefaultDissolveValue);
    }

    private static Material CopyMaterial(Material originalMaterial, string materialsFolder, string name)
    {
        var originalMaterialPath = AssetDatabase.GetAssetPath(originalMaterial);
        var materialName = $"{name}_{originalMaterial.name}";
        var materialPath = $"{materialsFolder}/{materialName}.mat";
        var wasCopied = AssetDatabase.CopyAsset(originalMaterialPath, materialPath);
        return wasCopied ? AssetDatabase.LoadAssetAtPath<Material>(materialPath) : null;
    }

    private static string GetFolderPath(string path) => System.IO.Path.GetDirectoryName(path);

    private static string GetOrAddMaterialFolder(string parent)
    {
        const string folderName = "Materials";
        var path = System.IO.Path.Combine(parent, folderName);
        var hasFolder = AssetDatabase.IsValidFolder(path);

        return hasFolder ? path : AssetDatabase.GUIDToAssetPath(
            AssetDatabase.CreateFolder(parent, folderName)
        );
    }
#endif
}
