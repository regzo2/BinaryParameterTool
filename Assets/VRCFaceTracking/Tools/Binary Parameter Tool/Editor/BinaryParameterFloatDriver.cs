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
    }
}
