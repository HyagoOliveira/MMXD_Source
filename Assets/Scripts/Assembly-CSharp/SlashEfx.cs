using System;
using System.Collections.Generic;
using UnityEngine;

public class SlashEfx : MonoBehaviour
{
    private class SlashDataInfo
    {
        public float px;

        public float py;

        public float pz;

        public float rx;

        public float ry;

        public float rz;

        public float sx;

        public float sy;

        public float sz;

        public float delay;

        public float speed;
    }

    private List<SlashDataInfo> Slash_List;

    private Transform slash;

    private Transform slash2;

    private Transform wslash1;

    private Transform wslash2;

    private Transform jpslash;

    private Transform slash_coll;

    private Transform wslash1_coll;

    private Transform wslash2_coll;

    private Transform jpslash_coll;

    private ParticleSystem slash_p;

    private ParticleSystem slash2_p;

    private ParticleSystem wslash1_p;

    private ParticleSystem wslash2_p;

    private ParticleSystem jpslash_p;

    private int FXLayer;

    private int PlayerLayer;

    private float ModelScale = 1f;

    private float PositionRate = 1f;

    private bool is_male;

    private void InitMaleData()
    {
        SlashDataInfo slashDataInfo = new SlashDataInfo
        {
            px = -0.2f,
            py = 0.93f,
            pz = 0.25f,
            rx = -36.41f,
            ry = 41.27f,
            rz = 146f
        };
        slashDataInfo.sx = (slashDataInfo.sy = (slashDataInfo.sz = 1.2f * ModelScale));
        slashDataInfo.delay = 0f;
        slashDataInfo.speed = 2.25f;
        Slash_List.Add(slashDataInfo);
        slashDataInfo = new SlashDataInfo
        {
            px = 0.47f,
            py = 0.96f,
            pz = 0f,
            rx = 85.73f,
            ry = 0f,
            rz = 92.1f
        };
        slashDataInfo.sx = (slashDataInfo.sy = (slashDataInfo.sz = 1.2f * ModelScale));
        slashDataInfo.delay = 0.1f;
        slashDataInfo.speed = 1.8f;
        Slash_List.Add(slashDataInfo);
        slashDataInfo = new SlashDataInfo
        {
            px = 0.3f,
            py = 0.7f,
            pz = 0f,
            rx = -91.1f,
            ry = 29.9f,
            rz = 70.45f
        };
        slashDataInfo.sx = (slashDataInfo.sy = (slashDataInfo.sz = 1.2f * ModelScale));
        slashDataInfo.delay = 0.1f;
        slashDataInfo.speed = 1.8f;
        Slash_List.Add(slashDataInfo);
        slashDataInfo = new SlashDataInfo();
        slashDataInfo.px = 0.2f;
        slashDataInfo.py = 1f;
        slashDataInfo.pz = 0f;
        slashDataInfo.rx = 71.2f;
        slashDataInfo.ry = -57.4f;
        slashDataInfo.rz = 51.9f;
        slashDataInfo.sx = (slashDataInfo.sy = (slashDataInfo.sz = 1.2f * ModelScale));
        slashDataInfo.delay = 0.1f;
        slashDataInfo.speed = 1.8f;
        Slash_List.Add(slashDataInfo);
        slashDataInfo = new SlashDataInfo
        {
            px = 0.61f,
            py = 1.69f,
            pz = -0.5f,
            rx = -180f,
            ry = 180f,
            rz = -80f
        };
        slashDataInfo.sx = (slashDataInfo.sy = (slashDataInfo.sz = 1.2f * ModelScale));
        slashDataInfo.delay = 0.1f;
        slashDataInfo.speed = 1.8f;
        Slash_List.Add(slashDataInfo);
        slashDataInfo = new SlashDataInfo
        {
            px = 0.3f,
            py = 0.5f,
            pz = -0.5f,
            rx = 0f,
            ry = 0f,
            rz = 160f
        };
        slashDataInfo.sx = (slashDataInfo.sy = (slashDataInfo.sz = 1f * ModelScale));
        slashDataInfo.delay = 0f;
        slashDataInfo.speed = 1.2f;
        Slash_List.Add(slashDataInfo);
    }

    private void InitFemaleData()
    {
        SlashDataInfo slashDataInfo = new SlashDataInfo
        {
            px = -0.2f,
            py = 0.93f,
            pz = 0.25f,
            rx = -36.41f,
            ry = 41.27f,
            rz = 146f
        };
        slashDataInfo.sx = (slashDataInfo.sy = (slashDataInfo.sz = 1.2f * ModelScale));
        slashDataInfo.delay = 0f;
        slashDataInfo.speed = 2.25f;
        Slash_List.Add(slashDataInfo);
        slashDataInfo = new SlashDataInfo
        {
            px = 0.47f,
            py = 0.96f,
            pz = 0f,
            rx = 85.73f,
            ry = 0f,
            rz = 92.1f
        };
        slashDataInfo.sx = (slashDataInfo.sy = (slashDataInfo.sz = 1.2f * ModelScale));
        slashDataInfo.delay = 0.1f;
        slashDataInfo.speed = 1.8f;
        Slash_List.Add(slashDataInfo);
        slashDataInfo = new SlashDataInfo
        {
            px = 0.2f,
            py = 1f,
            pz = -0.5f,
            rx = -40f,
            ry = 0f,
            rz = 170f
        };
        slashDataInfo.sx = (slashDataInfo.sy = (slashDataInfo.sz = 1f * ModelScale));
        slashDataInfo.delay = 0f;
        slashDataInfo.speed = 1.8f;
        Slash_List.Add(slashDataInfo);
        slashDataInfo = new SlashDataInfo
        {
            px = 0.47f,
            py = 1.29f,
            pz = 0f,
            rx = 0f,
            ry = 180f,
            rz = -75.38f
        };
        slashDataInfo.sx = (slashDataInfo.sy = (slashDataInfo.sz = 1f * ModelScale));
        slashDataInfo.delay = 0f;
        slashDataInfo.speed = 1.5f;
        Slash_List.Add(slashDataInfo);
        slashDataInfo = new SlashDataInfo
        {
            px = 0.21f,
            py = 1.8f,
            pz = -1f,
            rx = 0f,
            ry = 0f,
            rz = 58.28f
        };
        slashDataInfo.sx = (slashDataInfo.sy = (slashDataInfo.sz = 1f * ModelScale));
        slashDataInfo.delay = 0.1f;
        slashDataInfo.speed = 1.5f;
        Slash_List.Add(slashDataInfo);
        slashDataInfo = new SlashDataInfo
        {
            px = 0.3f,
            py = 0.5f,
            pz = -0.5f,
            rx = 0f,
            ry = 0f,
            rz = 160f
        };
        slashDataInfo.sx = (slashDataInfo.sy = (slashDataInfo.sz = 1f * ModelScale));
        slashDataInfo.delay = 0f;
        slashDataInfo.speed = 1.2f;
        Slash_List.Add(slashDataInfo);
    }

    public void InitSlashData(string effname, bool isMale, Transform attachPoint, Vector3 modelScale)
    {
        ModelScale = modelScale.x;
        MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/slash/" + effname + "_efx_sla", effname + "_efx_sla", delegate (GameObject obj)
        {
            if (!(obj == null))
            {
                CreateFXObj(obj, out slash, out slash_p, out slash_coll, effname);
                CreateFXObj(obj, out slash2, out slash2_p, effname);
            }
        });
        MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/slash/" + effname + "_efx_jpsla", effname + "_efx_jpsla", delegate (GameObject obj)
        {
            if (!(obj == null))
            {
                CreateFXObj(obj, out jpslash, out jpslash_p, out jpslash_coll, effname);
            }
        });
        MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/slash/" + effname + "_efx_wsla1", effname + "_efx_wsla1", delegate (GameObject obj)
        {
            if (!(obj == null))
            {
                CreateFXObj(obj, out wslash1, out wslash1_p, out wslash1_coll, effname);
            }
        });
        MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/slash/" + effname + "_efx_wsla2", effname + "_efx_wsla2", delegate (GameObject obj)
        {
            if (!(obj == null))
            {
                CreateFXObj(obj, out wslash2, out wslash2_p, out wslash2_coll, effname);
            }
        });
        if (isMale)
        {
            InitMaleData();
        }
        else
        {
            InitFemaleData();
        }
        PositionRate = 1f;
        base.transform.SetParent(attachPoint, false);
        is_male = isMale;
    }

    private void SetDataInfo(Transform trm, int index)
    {
        SlashDataInfo slashDataInfo = Slash_List[index];
        trm.localPosition = new Vector3(slashDataInfo.px, slashDataInfo.py, slashDataInfo.pz);
        trm.localRotation = Quaternion.Euler(slashDataInfo.rx, slashDataInfo.ry, slashDataInfo.rz);
        trm.localScale = new Vector3(slashDataInfo.sx, slashDataInfo.sy, slashDataInfo.sz);
        trm.localPosition *= PositionRate;
        ParticleSystem.MainModule main = trm.GetComponent<ParticleSystem>().main;
        main.startDelay = slashDataInfo.delay;
        main.simulationSpeed = slashDataInfo.speed;
    }

    public void ActiveColliderEffect(ParticleSystem Ptr, Transform collide_ent, string ActionName, float speed, float starttime)
    {
    }

    public void ActivateMeleeEffect(bool activate, SlashType slashType, Quaternion rot, bool isLeft)
    {
        if (!wslash1_p || !wslash2_p || !jpslash_p || !slash_p || !slash2_p)
        {
            return;
        }
        if (wslash1_p.isPlaying)
        {
            wslash1_p.Stop(true);
        }
        if (wslash2_p.isPlaying)
        {
            wslash2_p.Stop(true);
        }
        if (jpslash_p.isPlaying)
        {
            jpslash_p.Stop(true);
        }
        if (slash_p.isPlaying)
        {
            slash_p.Stop(true);
        }
        if (slash2_p.isPlaying)
        {
            slash2_p.Stop(true);
        }
        if ((bool)jpslash_coll)
        {
            jpslash_coll.GetComponent<CircleCollider2D>().gameObject.SetActive(false);
        }
        if ((bool)slash_coll)
        {
            BoxCollider2D component = slash_coll.GetComponent<BoxCollider2D>();
            component.gameObject.SetActive(false);
            component.isTrigger = false;
        }
        if (!activate)
        {
            return;
        }
        base.transform.localRotation = rot;
        switch (slashType)
        {
            case SlashType.StandSlash1:
                {
                    Transform transform4;
                    ParticleSystem particleSystem5;
                    if (slash_p.isPlaying)
                    {
                        transform4 = slash2;
                        particleSystem5 = slash2_p;
                    }
                    else
                    {
                        transform4 = slash;
                        particleSystem5 = slash_p;
                    }
                    SetDataInfo(transform4, 0);
                    transform4.gameObject.layer = FXLayer;
                    ParticleSystem.MainModule main7 = particleSystem5.main;
                    main7.startRotationY = 0f;
                    particleSystem5.Play();
                    ActiveColliderEffect(particleSystem5, slash_coll, "slashcollider", 1f, 0.03f);
                    break;
                }
            case SlashType.StandSlash2:
                {
                    Transform transform5;
                    ParticleSystem particleSystem7;
                    if (slash_p.isPlaying)
                    {
                        transform5 = slash2;
                        particleSystem7 = slash2_p;
                    }
                    else
                    {
                        transform5 = slash;
                        particleSystem7 = slash_p;
                    }
                    SetDataInfo(transform5, 1);
                    ParticleSystem.MainModule main10 = particleSystem7.main;
                    if (is_male)
                    {
                        if (isLeft)
                        {
                            transform5.transform.localPosition = new Vector3(0.47f, 0.83f, 0f);
                            main10.startRotationY = (float)Math.PI * 77f / 90f;
                        }
                        else
                        {
                            transform5.transform.localPosition = new Vector3(0.47f, 0.96f, 0f);
                            main10.startRotationY = 0f;
                        }
                        transform5.gameObject.layer = PlayerLayer;
                    }
                    else
                    {
                        if (isLeft)
                        {
                            transform5.transform.localPosition = new Vector3(0.47f, 0.83f, 0f);
                            main10.startRotationY = (float)Math.PI * 77f / 90f;
                        }
                        else
                        {
                            transform5.transform.localPosition = new Vector3(0.47f, 0.96f, 0f);
                            main10.startRotationY = 0f;
                        }
                        transform5.gameObject.layer = FXLayer;
                    }
                    transform5.transform.localPosition *= PositionRate;
                    particleSystem7.Play();
                    ActiveColliderEffect(particleSystem7, slash_coll, "slashcollider", 1f, 0f);
                    break;
                }
            case SlashType.StandSlash3:
                {
                    Transform transform3;
                    ParticleSystem particleSystem4;
                    if (slash_p.isPlaying)
                    {
                        transform3 = slash2;
                        particleSystem4 = slash2_p;
                    }
                    else
                    {
                        transform3 = slash;
                        particleSystem4 = slash_p;
                    }
                    SetDataInfo(transform3, 2);
                    ParticleSystem.MainModule main6 = particleSystem4.main;
                    main6.startRotationY = 0f;
                    if (is_male)
                    {
                        if (isLeft)
                        {
                            transform3.transform.localPosition = new Vector3(0.3f, 0.77f, 0f);
                            main6.startRotationY = 2.7925267f;
                        }
                        else
                        {
                            transform3.transform.localPosition = new Vector3(0.3f, 0.7f, 0f);
                            main6.startRotationY = 0f;
                        }
                        transform3.transform.localPosition *= PositionRate;
                        transform3.gameObject.layer = PlayerLayer;
                    }
                    else
                    {
                        transform3.gameObject.layer = FXLayer;
                    }
                    particleSystem4.Play();
                    ActiveColliderEffect(particleSystem4, slash_coll, "slashcollider", 1f, 0f);
                    break;
                }
            case SlashType.StandSlash4:
                {
                    Transform transform;
                    ParticleSystem particleSystem;
                    if (slash_p.isPlaying)
                    {
                        transform = slash2;
                        particleSystem = slash2_p;
                    }
                    else
                    {
                        transform = slash;
                        particleSystem = slash_p;
                    }
                    SetDataInfo(transform, 3);
                    ParticleSystem.MainModule main2 = particleSystem.main;
                    main2.startRotationY = 0f;
                    if (is_male)
                    {
                        if (isLeft)
                        {
                            transform.transform.localPosition = new Vector3(0.2f, 0.8f, 0f);
                            main2.startRotationY = (float)Math.PI * 3f / 4f;
                        }
                        else
                        {
                            transform.transform.localPosition = new Vector3(0.2f, 1f, 0f);
                            main2.startRotationY = 0f;
                        }
                        transform.transform.localPosition *= PositionRate;
                        transform.gameObject.layer = PlayerLayer;
                    }
                    else
                    {
                        transform.gameObject.layer = FXLayer;
                    }
                    particleSystem.Play();
                    ActiveColliderEffect(particleSystem, slash_coll, "slashcollider", 1f, 0f);
                    break;
                }
            case SlashType.StandSlash5:
                {
                    Transform transform2;
                    ParticleSystem particleSystem3;
                    if (slash_p.isPlaying)
                    {
                        transform2 = slash2;
                        particleSystem3 = slash2_p;
                    }
                    else
                    {
                        transform2 = slash;
                        particleSystem3 = slash_p;
                    }
                    SetDataInfo(transform2, 4);
                    ParticleSystem.MainModule main5 = particleSystem3.main;
                    main5.startRotationY = 0f;
                    if (!is_male)
                    {
                        if (isLeft)
                        {
                            transform2.transform.localPosition = new Vector3(0.21f, 1.8f, 1f);
                        }
                        else
                        {
                            transform2.transform.localPosition = new Vector3(0.21f, 1.8f, -1f);
                        }
                        transform2.transform.localPosition *= PositionRate;
                    }
                    transform2.gameObject.layer = FXLayer;
                    particleSystem3.Play();
                    ActiveColliderEffect(particleSystem3, slash_coll, "slashcollider", 1f, 0.2f);
                    break;
                }
            case SlashType.WalkSlash1:
                {
                    ParticleSystem particleSystem6 = wslash1_p;
                    ParticleSystem component3 = wslash1.GetComponent<SlashEffect>().EffectList[0].GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule main8 = particleSystem6.main;
                    ParticleSystem.MainModule main9 = component3.main;
                    wslash1.gameObject.layer = PlayerLayer;
                    if (isLeft)
                    {
                        wslash1.transform.localPosition = new Vector3(0.3f, 0.96f, 0f);
                        main8.startRotationY = 2.7925267f;
                        wslash1.GetComponent<SlashEffect>().EffectList[0].transform.localPosition = new Vector3(-0.0264f, 0.1397f, -0.05f);
                        main9.startRotationY = (float)Math.PI * 77f / 90f;
                    }
                    else
                    {
                        wslash1.transform.localPosition = new Vector3(0.3f, 0.89f, 0f);
                        main8.startRotationY = 0f;
                        wslash1.GetComponent<SlashEffect>().EffectList[0].transform.localPosition = new Vector3(-0.0264f, -0.1397f, 0.05f);
                        main9.startRotationY = 0f;
                    }
                    wslash1.transform.localPosition *= PositionRate;
                    particleSystem6.gameObject.SetActive(true);
                    particleSystem6.Play();
                    ActiveColliderEffect(wslash1_p, wslash1_coll, "wslashcollider1", 2f, 0f);
                    break;
                }
            case SlashType.WalkSlash2:
                {
                    ParticleSystem particleSystem2 = wslash2_p;
                    ParticleSystem component2 = wslash2.GetComponent<SlashEffect>().EffectList[0].GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule main3 = particleSystem2.main;
                    ParticleSystem.MainModule main4 = component2.main;
                    wslash2_p.gameObject.layer = PlayerLayer;
                    if (isLeft)
                    {
                        wslash2.transform.localPosition = new Vector3(0.3f, 0.96f, 0f);
                        main3.startRotationY = 2.7925267f;
                        wslash2.GetComponent<SlashEffect>().EffectList[0].transform.localPosition = new Vector3(-0.0264f, 0.1397f, -0.05f);
                        main4.startRotationY = (float)Math.PI * 77f / 90f;
                    }
                    else
                    {
                        wslash2.transform.localPosition = new Vector3(0.3f, 0.89f, 0f);
                        main3.startRotationY = 0f;
                        wslash2.GetComponent<SlashEffect>().EffectList[0].transform.localPosition = new Vector3(-0.0264f, -0.1397f, 0.05f);
                        main4.startRotationY = 0f;
                    }
                    wslash2.transform.localPosition *= PositionRate;
                    particleSystem2.gameObject.SetActive(true);
                    particleSystem2.Play();
                    ActiveColliderEffect(wslash2_p, wslash2_coll, "wslashcollider2", 2f, 0f);
                    break;
                }
            case SlashType.DashSlash1:
            case SlashType.JumpSlash1:
                jpslash_p.transform.localPosition *= PositionRate;
                jpslash_p.Play();
                if ((bool)jpslash_coll)
                {
                    jpslash_coll.GetComponent<CircleCollider2D>().gameObject.SetActive(true);
                }
                break;
            case SlashType.CrouchSlash1:
                {
                    ParticleSystem.MainModule main = slash_p.main;
                    main.startRotationY = 0f;
                    SetDataInfo(slash, 5);
                    slash_p.gameObject.layer = FXLayer;
                    slash_p.Play();
                    ActiveColliderEffect(slash_p, slash_coll, "slashcollider", 1f, 0f);
                    break;
                }
            case SlashType.Reserved3:
                break;
        }
    }

    public void DeActivateMeleeEffect()
    {
        if ((bool)wslash1_p && (bool)wslash2_p && (bool)jpslash_p && (bool)slash_p && (bool)slash2_p)
        {
            wslash1_p.Stop(true);
            wslash2_p.Stop(true);
            jpslash_p.Stop(true);
            slash_p.Stop(true);
            slash2_p.Stop(true);
            wslash1_p.gameObject.SetActive(false);
            wslash2_p.gameObject.SetActive(false);
            if ((bool)jpslash_coll)
            {
                jpslash_coll.GetComponent<CircleCollider2D>().gameObject.SetActive(false);
            }
            if ((bool)slash_coll)
            {
                BoxCollider2D component = slash_coll.GetComponent<BoxCollider2D>();
                component.gameObject.SetActive(false);
                component.isTrigger = false;
            }
        }
    }

    private void Awake()
    {
        Slash_List = new List<SlashDataInfo>();
        FXLayer = ManagedSingleton<OrangeLayerManager>.Instance.FxLayer;
        PlayerLayer = ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer;
    }

    private void CreateFXObj(GameObject slashObj, out Transform slashT, out ParticleSystem slashP, out Transform slashC, string effName)
    {
        GameObject gameObject = UnityEngine.Object.Instantiate(slashObj);
        gameObject.transform.SetParent(base.transform, false);
        slashT = gameObject.transform;
        slashT.localScale = new Vector3(slashT.localScale.x * ModelScale, slashT.localScale.y * ModelScale, slashT.localScale.z * ModelScale);
        slashP = slashT.GetComponent<ParticleSystem>();
        slashC = OrangeBattleUtility.FindChildRecursive(slashT, "slashcollider");
    }

    private void CreateFXObj(GameObject slashObj, out Transform slashT, out ParticleSystem slashP, string effName)
    {
        GameObject gameObject = UnityEngine.Object.Instantiate(slashObj);
        gameObject.transform.SetParent(base.transform, false);
        slashT = gameObject.transform;
        slashT.localScale = new Vector3(slashT.localScale.x * ModelScale, slashT.localScale.y * ModelScale, slashT.localScale.z * ModelScale);
        slashP = slashT.GetComponent<ParticleSystem>();
    }

    public void SetPositionRate(float value)
    {
        PositionRate = value;
    }
}
