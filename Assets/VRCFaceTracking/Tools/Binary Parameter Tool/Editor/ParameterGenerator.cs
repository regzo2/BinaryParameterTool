using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace VRCFaceTracking.EditorTools
{
    public class ParameterTools
    {
        public static AnimatorControllerParameter CheckAndCreateParameter(string paramName, AnimatorController animatorController, int type, double defaultVal = 0)
        {
            AnimatorControllerParameter param = new AnimatorControllerParameter();

            param.name = paramName;
            param.type = (AnimatorControllerParameterType)type;
            param.defaultFloat = (float)defaultVal;
            param.defaultInt = (int)defaultVal;


            AnimatorControllerParameterType _typeEnum = (AnimatorControllerParameterType)type;

            if (animatorController.parameters.Length == 0)
                animatorController.AddParameter(paramName, _typeEnum);

            for (int j = 0; j <= animatorController.parameters.Length - 1; j++)
            {

                if (animatorController.parameters[j].name == paramName)
                {
                    break;
                }

                if (animatorController.parameters.Length - 1 == j)
                    animatorController.AddParameter(param);
            }

            return param;
        }
    }
}
