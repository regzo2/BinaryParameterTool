using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VRCFaceTracking.EditorTools
{
    public class ParameterTools
    {
        public static bool AddVRCParameter(VRCAvatarDescriptor avatarDescriptor, List<VRCExpressionParameters.Parameter> parameters)
        {
            // Make sure Parameters aren't null
            if (avatarDescriptor.expressionParameters == null)
            {
                Debug.LogWarning("ExpressionsParameters not found!");
                return false;
            }
            // Instantiate and Save to Database
            VRCExpressionParameters newParameters = avatarDescriptor.expressionParameters;
            string assetPath = AssetDatabase.GetAssetPath(avatarDescriptor.expressionParameters);
            if (assetPath != String.Empty)
            {
                AssetDatabase.RemoveObjectFromAsset(avatarDescriptor.expressionParameters);
                AssetDatabase.CreateAsset(newParameters, assetPath);
                avatarDescriptor.expressionParameters = newParameters;
            }

            foreach (VRCExpressionParameters.Parameter parameter in parameters)
            {
                // Make sure the parameter doesn't already exist
                VRCExpressionParameters.Parameter foundParameter = newParameters.FindParameter(parameter.name);
                if (foundParameter == null || foundParameter.valueType != parameter.valueType)
                {
                    // Add the parameter
                    List<VRCExpressionParameters.Parameter> betterParametersBecauseItsAListInstead =
                        newParameters.parameters.ToList();
                    betterParametersBecauseItsAListInstead.Add(parameter);
                    newParameters.parameters = betterParametersBecauseItsAListInstead.ToArray();
                }
            }
            return true;
        }
      
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
