using System;
using System.Reflection;
using UnityEngine;

namespace UnityChan
{
    public class SpringManager : MonoBehaviour
    {
        public float dynamicRatio = 1f;

        public float stiffnessForce;

        public AnimationCurve stiffnessCurve;

        public float dragForce;

        public AnimationCurve dragCurve;

        public SpringBone[] springBones;

        [SerializeField]
        private float stiffnessPingPongVal;

        [SerializeField]
        private float stiffnessTimeVal;

        private int tweenUid = -1;

        private void Start()
        {
            UpdateParameters();
        }

        private void OnEnable()
        {
            if (stiffnessPingPongVal != 0f && stiffnessTimeVal != 0f)
            {
                tweenUid = LeanTween.value(stiffnessForce + stiffnessPingPongVal, stiffnessForce - stiffnessPingPongVal, stiffnessTimeVal).setOnUpdate(delegate (float val)
                {
                    stiffnessForce = val;
                    UpdateStiffnessForce(stiffnessForce);
                }).setOnComplete((Action)delegate
                {
                    tweenUid = -1;
                })
                    .setLoopPingPong(-1)
                    .uniqueId;
            }
        }

        private void OnDisable()
        {
            LeanTween.cancel(ref tweenUid);
        }

        private void LateUpdate()
        {
            if (dynamicRatio == 0f)
            {
                return;
            }
            for (int i = 0; i < springBones.Length; i++)
            {
                if (dynamicRatio > springBones[i].threshold)
                {
                    springBones[i].UpdateSpring();
                }
            }
        }

        private void UpdateParameters()
        {
            if (stiffnessTimeVal == 0f)
            {
                UpdateParameter("stiffnessForce", stiffnessForce, stiffnessCurve);
            }
            UpdateParameter("dragForce", dragForce, dragCurve);
        }

        private void UpdateParameter(string fieldName, float baseValue, AnimationCurve curve)
        {
            float time = curve.keys[0].time;
            float time2 = curve.keys[curve.length - 1].time;
            FieldInfo field = springBones[0].GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < springBones.Length; i++)
            {
                if (!springBones[i].isUseEachBoneForceSettings)
                {
                    float num = curve.Evaluate(time + (time2 - time) * (float)i / (float)(springBones.Length - 1));
                    field.SetValue(springBones[i], baseValue * num);
                }
            }
        }

        private void UpdateStiffnessForce(float val)
        {
            for (int i = 0; i < springBones.Length; i++)
            {
                if (!springBones[i].isUseEachBoneForceSettings)
                {
                    springBones[i].stiffnessForce = val;
                }
            }
        }
    }
}
