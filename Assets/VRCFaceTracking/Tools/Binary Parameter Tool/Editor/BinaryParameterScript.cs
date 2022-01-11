using UnityEngine;
using UnityEditor.Animations;
using System.Collections.Generic;
using UnityEditor;

public class BinaryParameterScript
{
    public static void CreateBinaryLayer(string baseParamName, AnimatorController animatorController, int binarySize, AnimationClip initClip, AnimationClip finalClip, float min, float max, float duration, bool nextStateInterrupt, bool writeDefaults, bool orderedInterrupt)
    {
        // Creating Parameters inside of the Animator Controller.
        CheckAndCreateBinaryParameters(baseParamName, animatorController, binarySize);

        // Create BinaryBlend parameter if it does not exist. Unity shenanaginsssss.
        CheckAndCreateParameter("BinaryBlend", animatorController, 1);
        // Creating a layer object since the default weight can not be assigned after creation.
        AnimatorControllerLayer layer = new AnimatorControllerLayer
        {
            name = baseParamName + " Binary",
            stateMachine = new AnimatorStateMachine
            {
                hideFlags = HideFlags.HideInHierarchy
            },
            defaultWeight = 1f
        };

        // Store Layer into Animator Controller, as creating a Layer object is not serialized unless we store it inside an asset.
        if (AssetDatabase.GetAssetPath(animatorController) != string.Empty)
        {
            AssetDatabase.AddObjectToAsset(layer.stateMachine, AssetDatabase.GetAssetPath(animatorController));
        }

        animatorController.AddLayer(layer);

        var rootStateMachine = layer.stateMachine;

        // Creating a Binary State Machine
        CreateBinaryStatesInMachine(baseParamName, binarySize, rootStateMachine, initClip, finalClip, writeDefaults, duration, nextStateInterrupt, orderedInterrupt, min, max);
    }

    public static void CreateCombinedBinaryLayer(string baseParamName, AnimatorController animatorController, int binarySize, AnimationClip initClip, AnimationClip finalClip, AnimationClip finalNegativeClip, float min, float max, float minNeg, float maxNeg, float duration, bool nextStateInterrupt, bool orderedInterrupt, bool writeDefaults)
    {
        // Creating Parameters inside of the Animator Controller. Will be using a list to keep track of the tracking params for later.
        CheckAndCreateBinaryParameters(baseParamName, animatorController, binarySize);

        // Create BinaryBlend parameter if it does not exist. Unity shenanaginsssss.
        CheckAndCreateParameter("BinaryBlend", animatorController, 1);

        // Create ...Negative parameter if it does not exist.
        CheckAndCreateParameter(baseParamName + "Negative", animatorController, 4);

        // Creating a layer object since the default weight can not be assigned after creation.
        AnimatorControllerLayer layer = new AnimatorControllerLayer
        {
            name = baseParamName + " Binary",
            stateMachine = new AnimatorStateMachine
            {
                hideFlags = HideFlags.HideInHierarchy
            },
            defaultWeight = 1f
        };

        // Store Layer into Animator Controller, as creating a Layer object is not serialized unless we store it inside an asset.
        if (AssetDatabase.GetAssetPath(animatorController) != string.Empty)
        {
            AssetDatabase.AddObjectToAsset(layer.stateMachine, AssetDatabase.GetAssetPath(animatorController));
        }

        animatorController.AddLayer(layer);

        var rootStateMachine = layer.stateMachine;

        // Creating a Combined Binary State Machine
        CreateCombinedBinaryStatesInMachine(baseParamName, binarySize, rootStateMachine, initClip, finalClip, finalNegativeClip, writeDefaults, duration, nextStateInterrupt, orderedInterrupt, min, max, minNeg, maxNeg);
    }

    private static void CheckAndCreateParameter(string name, AnimatorController animatorController, int type)
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

    private static void CheckAndCreateBinaryParameters(string name, AnimatorController animatorController, int binarySize)
    {
        // Creating Parameters inside of the Animator Controller. Will be using a list to keep track of the tracking params for later.
        int binarySteps = (int)Mathf.Pow(2, binarySize);
        int binarySizeTemp = 1;

        bool _exists = false;
        for (int i = 1; i < binarySteps; i++)
        {
            if (i == binarySizeTemp)
            {
                for (int j = 0; j < animatorController.parameters.Length; j++)
                {

                    if (animatorController.parameters[j].name == name + i)
                    {
                        _exists = true;
                        break;
                    }
                }

                if (_exists)
                {
                    _exists = false;
                    binarySizeTemp *= 2;
                    continue;
                }

                animatorController.AddParameter(name + i, AnimatorControllerParameterType.Bool);

                binarySizeTemp *= 2;
            }
        }
    }

    private static void CreateCombinedBinaryStatesInMachine(string name, int binarySize, AnimatorStateMachine stateMachine, AnimationClip initClip, AnimationClip finalClip, AnimationClip finalNegativeClip, bool writeDefaults, float duration, bool nextStateInterrupt, bool orderedInterrupt, float min, float max, float minNeg, float maxNeg)
    {
        int negativeCount = 1;

        int binarySteps = (int)Mathf.Pow(2, binarySize);

        int minSteps = (int)((min + .05) * binarySteps);
        int maxSteps = (int)((max - .05) * binarySteps);
        int minNegSteps = (int)((minNeg + .05) * binarySteps);
        int maxNegSteps = (int)((maxNeg - .05) * binarySteps);

        AnimatorState[] states = new AnimatorState[binarySteps];

        stateMachine.name = name + " Binary State Machine";
        stateMachine.anyStatePosition = new Vector3(20, 0, 0);
        stateMachine.entryPosition = new Vector3
        (
            20,
            stateMachine.anyStatePosition.y - 5 - Mathf.Cos(0) * (150 + binarySteps * 4),
            0
        );

        var basisState = stateMachine.AddState(name + "0", new Vector3
        (
            stateMachine.anyStatePosition.x - 20 - Mathf.Sin(0) * (200 + binarySteps * 8),
            stateMachine.anyStatePosition.y - 5 - Mathf.Cos(0) * (100 + binarySteps * 4),
            0
        ));

        var _basisAnyStateTransition = stateMachine.AddAnyStateTransition(basisState);

        for (int j = 0; j < binarySize; j++)
        {
            _basisAnyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0, name + binarySteps);

            _basisAnyStateTransition.duration = duration;
            _basisAnyStateTransition.canTransitionToSelf = false;
            if (nextStateInterrupt)
            {
                _basisAnyStateTransition.interruptionSource = TransitionInterruptionSource.Destination;
                _basisAnyStateTransition.orderedInterruption = orderedInterrupt;
            }
        }

        basisState.motion = initClip;

        while (negativeCount != 0)
        {
            // Creating all the binary states and transitions
            for (int i = 1; i < binarySteps; i++)
            {
                states[i] = stateMachine.AddState(name + i * negativeCount, new Vector3
                (
                    stateMachine.anyStatePosition.x - 20 - Mathf.Sin(((float)i / binarySteps) * Mathf.PI * (-1) * negativeCount) * (200 + binarySteps * 8),
                    stateMachine.anyStatePosition.y - 5 - Mathf.Cos(((float)i / binarySteps) * Mathf.PI) * (100 + binarySteps * 4),
                    0
                ));

                var _anyStateTransition = stateMachine.AddAnyStateTransition(states[i]);

                for (int j = 0; j < binarySize; j++)
                {
                    int _conditionSetTrue = (i >> j) & 1;

                    if (i == binarySize)
                        _conditionSetTrue = 1;

                    if (_conditionSetTrue == 1)
                        _anyStateTransition.AddCondition(AnimatorConditionMode.If, 0, name + Mathf.Pow(2, j));
                    else _anyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0, name + Mathf.Pow(2, j));

                    _anyStateTransition.duration = duration;
                    _anyStateTransition.canTransitionToSelf = false;
                    if (nextStateInterrupt)
                    {
                        _anyStateTransition.interruptionSource = TransitionInterruptionSource.Destination;
                        _anyStateTransition.orderedInterruption = orderedInterrupt;
                    }
                }

                if (negativeCount == 1)
                    _anyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0, name + "Negative");
                else _anyStateTransition.AddCondition(AnimatorConditionMode.If, 0, name + "Negative");

                BlendTree _blendTree = new BlendTree
                {
                    blendType = BlendTreeType.Simple1D,
                    hideFlags = HideFlags.HideInHierarchy,
                    blendParameter = "BinaryBlend",
                    name = name + (i * negativeCount),
                    useAutomaticThresholds = false
                };

                // Need this function to serialize the BlendTrees, otherwise they go byebye
                if (AssetDatabase.GetAssetPath(stateMachine) != string.Empty)
                {
                    AssetDatabase.AddObjectToAsset(_blendTree, AssetDatabase.GetAssetPath(stateMachine));
                }

                if (negativeCount == 1)
                {
                    _blendTree.AddChild(initClip, minSteps + (int)((-1) * (i * (binarySteps) / (binarySteps - 1))));
                    _blendTree.AddChild(finalClip, maxSteps - i * (binarySteps) / (binarySteps - 1));
                }

                if (negativeCount == -1)
                {
                    _blendTree.AddChild(initClip, minNegSteps + (int)((-1) * (i * (binarySteps) / (binarySteps - 1))));
                    _blendTree.AddChild(finalNegativeClip, maxNegSteps - i * (binarySteps) / (binarySteps - 1));
                }

                states[i].motion = _blendTree;
                states[i].writeDefaultValues = writeDefaults;
            }
            if (negativeCount == -1)
                negativeCount = 0;

            if (negativeCount == 1)
                negativeCount = -1;
        }
    }

    private static void CreateBinaryStatesInMachine(string name, int binarySize, AnimatorStateMachine stateMachine, AnimationClip initClip, AnimationClip finalClip, bool writeDefaults, float duration, bool nextStateInterrupt, bool orderedInterrupt, float min, float max)
    {
        int binarySteps = (int)Mathf.Pow(2, binarySize);

        int minSteps = (int)((min + .05) * binarySteps);
        int maxSteps = (int)((max - .05) * binarySteps);

        AnimatorState[] states = new AnimatorState[binarySteps];

        for (int i = 0; i < binarySteps; i++)
        {

            stateMachine.name = name + " Binary State Machine";
            stateMachine.anyStatePosition = new Vector3(20, 0, 0);
            stateMachine.entryPosition = new Vector3
            (
                20,
                stateMachine.anyStatePosition.y - 5 - Mathf.Cos(0) * (150 + binarySteps * 4),
                0
            );


            states[i] = stateMachine.AddState(name + i, new Vector3
            (
                stateMachine.anyStatePosition.x - 20 - Mathf.Sin((i / (float)binarySteps) * Mathf.PI * 2f) * (200 + binarySteps * 8),
                stateMachine.anyStatePosition.y - 5 - Mathf.Cos((i / (float)binarySteps) * Mathf.PI * 2f) * (100 + binarySteps * 4),
                0
            ));

            var _anyStateTransition = stateMachine.AddAnyStateTransition(states[i]);

            for (int j = 0; j < binarySize; j++)
            {
                int _conditionSetTrue = (i >> j) & 1;

                if (i == binarySize)
                    _conditionSetTrue = 1;

                if (_conditionSetTrue == 1)
                    _anyStateTransition.AddCondition(AnimatorConditionMode.If, 0, name + Mathf.Pow(2, j));
                else _anyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0, name + Mathf.Pow(2, j));

                _anyStateTransition.duration = duration;
                _anyStateTransition.canTransitionToSelf = false;
                if (nextStateInterrupt)
                {
                    _anyStateTransition.interruptionSource = TransitionInterruptionSource.Destination;
                    _anyStateTransition.orderedInterruption = orderedInterrupt;
                }
            }

            BlendTree _blendTree = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                hideFlags = HideFlags.HideInHierarchy,
                blendParameter = "BinaryBlend",
                name = name + i,
                useAutomaticThresholds = false,
            };

            // Need this function to serialize the BlendTrees, otherwise they go byebye
            if (AssetDatabase.GetAssetPath(stateMachine) != string.Empty)
            {
                AssetDatabase.AddObjectToAsset(_blendTree, AssetDatabase.GetAssetPath(stateMachine));
            }

            _blendTree.AddChild(initClip, minSteps + (int)((-1) * (i * (binarySteps) / (binarySteps))));
            _blendTree.AddChild(finalClip, maxSteps - i * (binarySteps) / (binarySteps));

            states[i].motion = _blendTree;
            states[i].writeDefaultValues = writeDefaults;
        }
    }
}
