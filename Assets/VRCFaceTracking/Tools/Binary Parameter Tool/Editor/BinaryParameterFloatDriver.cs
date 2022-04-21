using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;

namespace VRCFaceTracking.EditorTools
{
    public class BinaryParameterFloatDriver
    {
        public static AnimationClip CreateFloatDriverAnimation(string baseParamName, float parameterValue)
        {
            AnimationClip _animationClip = new AnimationClip();

            AnimationCurve _curve = new AnimationCurve(new Keyframe(0.0f, parameterValue));

            _animationClip.SetCurve("", typeof(Animator), baseParamName, _curve);

            if (!Directory.Exists("Assets/VRCFaceTracking/Generated/Anims/"))
            {
                Directory.CreateDirectory("Assets/VRCFaceTracking/Generated/Anims/");
            }

            string[] guid = (AssetDatabase.FindAssets(baseParamName + parameterValue + "Float"));

            if (guid.Length == 0)
            {
                AssetDatabase.CreateAsset(_animationClip, "Assets/VRCFaceTracking/Generated/Anims/" + baseParamName + parameterValue + "Float.anim");
                AssetDatabase.SaveAssets();
            }

            else
            {
                _animationClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid[0]), typeof(AnimationClip));
            }

            return _animationClip;
        }

        public static AnimationClip CreateFloatDriverAnimation(string baseParamName, string animName, float parameterValue)
        {
            AnimationClip _animationClip = new AnimationClip();

            AnimationCurve _curve = new AnimationCurve(new Keyframe(0.0f, parameterValue));

            _animationClip.SetCurve("", typeof(Animator), baseParamName, _curve);

            if (!Directory.Exists("Assets/VRCFaceTracking/Generated/Anims/"))
            {
                Directory.CreateDirectory("Assets/VRCFaceTracking/Generated/Anims/");
            }

            string[] guid = (AssetDatabase.FindAssets(animName));

            if (guid.Length == 0)
            {
                AssetDatabase.CreateAsset(_animationClip, "Assets/VRCFaceTracking/Generated/Anims/" + animName + ".anim");
                AssetDatabase.SaveAssets();
            }

            else
            {
                _animationClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid[0]), typeof(AnimationClip));
            }

            return _animationClip;
        }

        public static AnimationClip[] CreateFloatSmootherAnimation(string baseParamName, float initThreshold = 0, float finalThreshold = 1)
        {
            AnimationClip _animationClip1 = new AnimationClip();
            AnimationClip _animationClip2 = new AnimationClip();

            AnimationCurve _curve1 = new AnimationCurve(new Keyframe(0.0f, initThreshold));
            AnimationCurve _curve2 = new AnimationCurve(new Keyframe(0.0f, finalThreshold));

            _animationClip1.SetCurve("", typeof(Animator), baseParamName, _curve1);
            _animationClip2.SetCurve("", typeof(Animator), baseParamName, _curve2);

            if (!Directory.Exists("Assets/VRCFaceTracking/Generated/Anims/"))
            {
                Directory.CreateDirectory("Assets/VRCFaceTracking/Generated/Anims/");
            }

            string[] guid = (AssetDatabase.FindAssets(baseParamName + initThreshold + "Smoother"));

            if (guid.Length == 0)
            {
                AssetDatabase.CreateAsset(_animationClip1, "Assets/VRCFaceTracking/Generated/Anims/"+ baseParamName + initThreshold + " Smoother");
                AssetDatabase.SaveAssets();
            }

            else
            {
                _animationClip1 = (AnimationClip)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid[0]), typeof(AnimationClip));
            }

            guid = (AssetDatabase.FindAssets(baseParamName + finalThreshold + "Smoother"));

            if (guid.Length == 0)
            {
                AssetDatabase.CreateAsset(_animationClip2, "Assets/VRCFaceTracking/Generated/Anims/" + baseParamName + finalThreshold + " Smoother");
                AssetDatabase.SaveAssets();
            }

            else
            {
                _animationClip2 = (AnimationClip)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid[0]), typeof(AnimationClip));
            }

            return new AnimationClip[] { _animationClip1, _animationClip2 };
        }
    }
}
