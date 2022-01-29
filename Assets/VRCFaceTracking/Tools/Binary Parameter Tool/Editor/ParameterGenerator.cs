using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace VRCFaceTracking.EditorTools
{
    public class ParameterTools
    {
        public static void CheckAndCreateParameter(string name, AnimatorController animatorController, int type)
        {
            AnimatorControllerParameterType _typeEnum = (AnimatorControllerParameterType)type;

            if (animatorController.parameters.Length == 0)
                animatorController.AddParameter(name, _typeEnum);

            for (int j = 0; j <= animatorController.parameters.Length - 1; j++)
            {

                if (animatorController.parameters[j].name == name)
                {
                    break;
                }

                if (animatorController.parameters.Length - 1 == j)
                    animatorController.AddParameter(name, _typeEnum);
            }
        }
    }
}
