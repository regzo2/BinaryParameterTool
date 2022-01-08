using UnityEngine;
using UnityEditor.Animations;
using System.Collections.Generic;

public class BinaryParameterScript
{
    public static void CreateBinaryLayer(string baseParamName, AnimatorController animatorController, int binarySize, AnimationClip initClip, AnimationClip finalClip, float min, float max, float duration, bool nextStateInterrupt)
    {

        // Creating Parameters inside of the Animator Controller. Will be using a list to keep track of the tracking params for later.
        int binaryTemp = 1;
        List<int> binaryList = new List<int>();

        // Create BinaryBlend parameter if it does not exist. Unity shenanaginsssss.
        if (animatorController.parameters.Length == 0)
            animatorController.AddParameter("BinaryBlend", AnimatorControllerParameterType.Float);

        for (int j = 0; j <= animatorController.parameters.Length - 1; j++)
        {

            if (animatorController.parameters[j].name == "BinaryBlend")
            {
                break;
            }

            if (animatorController.parameters.Length - 1 == j)
                animatorController.AddParameter("BinaryBlend", AnimatorControllerParameterType.Float);
        }

        bool _exists = false;
        for (int i = 1; i < binarySize; i++)
        {
            if (i == binaryTemp)
            {
                binaryList.Add(i);

                for (int j = 0; j < animatorController.parameters.Length; j++)
                {

                    if (animatorController.parameters[j].name == baseParamName + i)
                    {
                        _exists = true;
                        break;
                    }
                }

                if (_exists)
                {
                    _exists = false;
                    binaryTemp *= 2;
                    continue;
                }

                animatorController.AddParameter(baseParamName + i, AnimatorControllerParameterType.Bool);

                binaryTemp *= 2;
            }
        }

        // Creating a layer object since the default weight can not be assigned after creation.
        
        animatorController.AddLayer(baseParamName + " Binary");

        var rootStateMachine = animatorController.layers[animatorController.layers.Length - 1].stateMachine;
        AnimatorState[] states = new AnimatorState[binarySize];

        // Creates a State with the user defined animations, then hooks it to the AnyState with transitions containing binary conditions, and user defined rules.
        // It just does it by hooking to the last Layer index, which should be the index our Layer was created on during this process.

        for (int i = 0; i < binarySize; i++)
        {

            rootStateMachine.name = baseParamName + " Binary State Machine";
            rootStateMachine.anyStatePosition = new Vector3(20, 0, 0);
            rootStateMachine.entryPosition = new Vector3
            (
                20,
                rootStateMachine.anyStatePosition.y - 5 - Mathf.Cos(0) * (150 + binarySize * 4),
                0
            );


            states[i] = rootStateMachine.AddState(baseParamName + i, new Vector3
            (
                rootStateMachine.anyStatePosition.x - 20 - Mathf.Sin((i / (float)binarySize) * Mathf.PI * 2f) * (200 + binarySize * 8),
                rootStateMachine.anyStatePosition.y - 5 - Mathf.Cos((i / (float)binarySize) * Mathf.PI * 2f) * (100 + binarySize * 4),
                0
            ));

            var _anyStateTransition = rootStateMachine.AddAnyStateTransition(states[i]);

            for (int j = 0; j < binaryList.Count; j++)
            {
                int _conditionSetTrue = (i >> j) & 1;

                if (i == binarySize)
                    _conditionSetTrue = 1;

                if (_conditionSetTrue == 1)
                    _anyStateTransition.AddCondition(AnimatorConditionMode.If, 0, baseParamName + binaryList[j]);
                else _anyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0, baseParamName + binaryList[j]);

                _anyStateTransition.duration = duration;
                _anyStateTransition.canTransitionToSelf = false;
                if (nextStateInterrupt)
                {
                    _anyStateTransition.interruptionSource = TransitionInterruptionSource.Destination;
                    _anyStateTransition.orderedInterruption = true;
                }
            }

            BlendTree _blendTree = new BlendTree
            {
                blendType = BlendTreeType.Simple1D
            };

            _blendTree.blendParameter = "BinaryBlend";
            _blendTree.name = baseParamName + i;
            _blendTree.useAutomaticThresholds = false;
            _blendTree.AddChild(initClip, (min * 100f) - ((float)i / (float)(binarySize - 1)) * 100f);
            _blendTree.AddChild(finalClip, (max * 100f) - ((float)i / (float)(binarySize - 1)) * 100f);


            states[i].motion = _blendTree;
        }

    }

    public static void CreateCombinedBinaryLayer(string baseParamName, AnimatorController animatorController, int binarySize, AnimationClip initClip, AnimationClip finalClip, AnimationClip finalNegativeClip, float min, float max, float minNeg, float maxNeg, float duration, bool nextStateInterrupt)
    {

        // Creating Parameters inside of the Animator Controller. Will be using a list to keep track of the tracking params for later.
        int binaryTemp = 1;
        List<int> binaryList = new List<int>();

        // Create BinaryBlend parameter if it does not exist. Unity shenanaginsssss.
        if (animatorController.parameters.Length == 0)
            animatorController.AddParameter("BinaryBlend", AnimatorControllerParameterType.Float);

        for (int j = 0; j <= animatorController.parameters.Length - 1; j++)
        {

            if (animatorController.parameters[j].name == "BinaryBlend")
            {
                break;
            }

            if (animatorController.parameters.Length - 1 == j)
                animatorController.AddParameter("BinaryBlend", AnimatorControllerParameterType.Float);
        }

        // Create ...Negative parameter if it does not exist.
        for (int j = 0; j <= animatorController.parameters.Length - 1; j++)
        {

            if (animatorController.parameters[j].name == baseParamName + "Negative")
            {
                break;
            }

            if (animatorController.parameters.Length - 1 == j)
                animatorController.AddParameter(baseParamName + "Negative", AnimatorControllerParameterType.Bool);
        }

        bool _exists = false;
        for (int i = 1; i < binarySize; i++)
        {
            if (i == binaryTemp)
            {
                binaryList.Add(i);

                for (int j = 0; j < animatorController.parameters.Length; j++)
                {

                    if (animatorController.parameters[j].name == baseParamName + i)
                    {
                        _exists = true;
                        break;
                    }
                }

                if (_exists)
                {
                    _exists = false;
                    binaryTemp *= 2;
                    continue;
                }

                animatorController.AddParameter(baseParamName + i, AnimatorControllerParameterType.Bool);

                binaryTemp *= 2;
            }
        }

        // Creating a layer object since the default weight can not be assigned after creation.
        animatorController.AddLayer(baseParamName + " Binary");

        var rootStateMachine = animatorController.layers[animatorController.layers.Length - 1].stateMachine;
        AnimatorState[] states = new AnimatorState[binarySize];

        // Creates a State with the user defined animations, then hooks it to the AnyState with transitions containing binary conditions, and user defined rules.
        // It just does it by hooking to the last Layer index, which should be the index our Layer was created on during this process.

        int negativeCount = 1;

        while (negativeCount != 0)
        {

            if (negativeCount == 1)
            {
                rootStateMachine.name = baseParamName + " Binary State Machine";
                rootStateMachine.anyStatePosition = new Vector3(20, 0, 0);
                rootStateMachine.entryPosition = new Vector3
                (
                    20,
                    rootStateMachine.anyStatePosition.y - 5 - Mathf.Cos(0) * (150 + binarySize * 4),
                    0
                );

                var basisState = rootStateMachine.AddState(baseParamName + "0", new Vector3
                (
                    rootStateMachine.anyStatePosition.x - 20 - Mathf.Sin(0) * (200 + binarySize * 8),
                    rootStateMachine.anyStatePosition.y - 5 - Mathf.Cos(0) * (100 + binarySize * 4),
                    0
                ));

                var _basisAnyStateTransition = rootStateMachine.AddAnyStateTransition(basisState);

                for (int j = 0; j < binaryList.Count; j++)
                {
                    _basisAnyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0, baseParamName + binaryList[j]);

                    _basisAnyStateTransition.duration = duration;
                    _basisAnyStateTransition.canTransitionToSelf = false;
                    if (nextStateInterrupt) 
                    {
                        _basisAnyStateTransition.interruptionSource = TransitionInterruptionSource.Destination;
                        _basisAnyStateTransition.orderedInterruption = true;
                    }
                }

                basisState.motion = initClip;
            }


            // Creating all the binary states and transitions
            for (int i = 1; i < binarySize; i++)
            {
                states[i] = rootStateMachine.AddState(baseParamName + i * negativeCount, new Vector3
                (
                    rootStateMachine.anyStatePosition.x - 20 - Mathf.Sin((i / (float)binarySize) * Mathf.PI * (-1) * negativeCount) * (200 + binarySize * 8),
                    rootStateMachine.anyStatePosition.y - 5 - Mathf.Cos((i / (float)binarySize) * Mathf.PI) * (100 + binarySize * 4),
                    0
                ));

                var _anyStateTransition = rootStateMachine.AddAnyStateTransition(states[i]);

                for (int j = 0; j < binaryList.Count; j++)
                {
                    int _conditionSetTrue = (i >> j) & 1;

                    if (i == binarySize)
                        _conditionSetTrue = 1;

                    if (_conditionSetTrue == 1)
                        _anyStateTransition.AddCondition(AnimatorConditionMode.If, 0, baseParamName + binaryList[j]);
                    else _anyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0, baseParamName + binaryList[j]);

                    _anyStateTransition.duration = duration;
                    _anyStateTransition.canTransitionToSelf = false;
                    if (nextStateInterrupt)
                    {
                        _anyStateTransition.interruptionSource = TransitionInterruptionSource.Destination;
                        _anyStateTransition.orderedInterruption = true;
                    }
                }

                if (negativeCount == 1)
                    _anyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0, baseParamName + "Negative");
                else _anyStateTransition.AddCondition(AnimatorConditionMode.If, 0, baseParamName + "Negative");

                BlendTree _blendTree = new BlendTree
                {
                    blendType = BlendTreeType.Simple1D
                };

                _blendTree.blendParameter = "BinaryBlend";
                _blendTree.name = baseParamName + (i * negativeCount);

                _blendTree.useAutomaticThresholds = false;

                if (negativeCount == 1)
                {
                    _blendTree.AddChild(initClip, (min * 100f) - ((float)i / (float)(binarySize - 1)) * 100f);
                    _blendTree.AddChild(finalClip, (max * 100f) - ((float)i / (float)(binarySize - 1)) * 100f);
                }

                if (negativeCount == -1)
                {
                    _blendTree.AddChild(initClip, (minNeg * 100f) - ((float)i / (float)(binarySize - 1)) * 100f);
                    _blendTree.AddChild(finalNegativeClip, (maxNeg * 100f) - ((float)i / (float)(binarySize - 1)) * 100f);
                }

                _blendTree.blendParameter = "BinaryBlend";

                states[i].motion = _blendTree;
            }
            if (negativeCount == -1)
                negativeCount = 0;

            if (negativeCount == 1)
                negativeCount = -1;
        }
    }
}
