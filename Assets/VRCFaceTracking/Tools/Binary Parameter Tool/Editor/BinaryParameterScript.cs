using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;
using System;

namespace VRCFaceTracking.EditorTools
{
    public class BinaryStateMachine
    {
        public AnimatorController animatorController;

        public AnimationClip initClip;
        public AnimationClip finalClip;
        public AnimationClip finalNegativeClip;

        public string baseParamName;

        public int binarySize;

        public float min;
        public float max;
        public float minNeg;
        public float maxNeg;
        public float duration;

        public bool nextStateInterrupt;
        public bool isCombined;
        public bool writeDefaults;

        public BinaryStateMachine()
        {
            animatorController = new AnimatorController();

            initClip = new AnimationClip();
            finalClip = new AnimationClip();
            finalNegativeClip = new AnimationClip();

            baseParamName = "BaseParam";

            binarySize = 4;

            min = 0f;
            max = 1f;
            minNeg = 0f;
            maxNeg = 1f;
            duration = 0.1f;

            nextStateInterrupt = true;
            writeDefaults = true;
        }

        public void CreateBinaryLayer()
        {
            
            // Creating Parameters inside of the Animator Controller.
            CheckAndCreateBinaryParameters(baseParamName, animatorController, binarySize);

            // Create BinaryBlend parameter if it does not exist. Unity shenanaginsssss.
            ParameterTools.CheckAndCreateParameter("BinaryBlend", animatorController, 1);

            // Clear out existing ...Binary layers to make-way for an updated one
            for (int i = 0; i < animatorController.layers.Length; i++)
                if (animatorController.layers[i].name == baseParamName + " Binary")
                {
                    animatorController.RemoveLayer(i);
                }

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

            /* IMPLEMENT IN NEXT UPDATE PLS LOL
            // Creating branching Binary State Machine
            CreateBranchingBinaryStatesInMachine(baseParamName, binarySize, rootStateMachine, initClip, finalClip, writeDefaults, duration, nextStateInterrupt, min, max);
            */

            // Creating branching Binary State Machine
            CreateUniformBinaryStatesInMachine(baseParamName, binarySize, rootStateMachine, initClip, finalClip, writeDefaults, duration, nextStateInterrupt, min, max);
        }

        public void CreateCombinedBinaryLayer()
        {
            // Creating Parameters inside of the Animator Controller. Will be using a list to keep track of the tracking params for later.
            CheckAndCreateBinaryParameters(baseParamName, animatorController, binarySize);

            // Create BinaryBlend parameter if it does not exist. Unity shenanaginsssss.
            ParameterTools.CheckAndCreateParameter("BinaryBlend", animatorController, 1);

            // Create ...Negative parameter if it does not exist.
            ParameterTools.CheckAndCreateParameter(baseParamName + "Negative", animatorController, 4);

            // Clear out existing ...Binary layers to make-way for an updated one
            for (int i = 0; i < animatorController.layers.Length; i++)
                if (animatorController.layers[i].name == baseParamName + " Binary")
                {
                    animatorController.RemoveLayer(i);
                }

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
            CreateCombinedUniformBinaryStatesInMachine(baseParamName, binarySize, rootStateMachine, initClip, finalClip, finalNegativeClip, writeDefaults, duration, nextStateInterrupt, min, max, minNeg, maxNeg);
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

        private static void CreateCombinedUniformBinaryStatesInMachine(string name, int binarySize, AnimatorStateMachine stateMachine, AnimationClip initClip, AnimationClip finalClip, AnimationClip finalNegativeClip, bool writeDefaults, float duration, bool nextStateInterrupt, float min, float max, float minNeg, float maxNeg)
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
                _basisAnyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0, name + Mathf.Pow(2, j));

                _basisAnyStateTransition.duration = duration;
                _basisAnyStateTransition.canTransitionToSelf = false;
                if (nextStateInterrupt)
                {
                    _basisAnyStateTransition.interruptionSource = TransitionInterruptionSource.Destination;
                    _basisAnyStateTransition.orderedInterruption = true;
                }


            }

            basisState.motion = initClip;
            basisState.writeDefaultValues = writeDefaults;

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

                        if (_conditionSetTrue == 1)
                            _anyStateTransition.AddCondition(AnimatorConditionMode.If, 0, name + Mathf.Pow(2, j));
                        else _anyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0, name + Mathf.Pow(2, j));

                        _anyStateTransition.duration = duration;
                        _anyStateTransition.canTransitionToSelf = false;
                        if (nextStateInterrupt)
                        {
                            _anyStateTransition.interruptionSource = TransitionInterruptionSource.Destination;
                            _anyStateTransition.orderedInterruption = true;
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

        private static void CreateUniformBinaryStatesInMachine(string name, int binarySize, AnimatorStateMachine stateMachine, AnimationClip initClip, AnimationClip finalClip, bool writeDefaults, float duration, bool nextStateInterrupt, float min, float max)
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

                    if (_conditionSetTrue == 1)
                        _anyStateTransition.AddCondition(AnimatorConditionMode.If, 0, name + Mathf.Pow(2, j));
                    else _anyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0, name + Mathf.Pow(2, j));

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

        private static void CreateBranchingBinaryStatesInMachine(string name, int binarySize, AnimatorStateMachine stateMachine, AnimationClip initClip, AnimationClip finalClip, bool writeDefaults, float duration, bool nextStateInterrupt, float min, float max, AnimationClip finalNegativeClip = null, float minNeg = 0, float maxNeg = 0)
        {
            // Skips creating the negative & positive branch
            bool skipNegative = false;
            if (finalNegativeClip == null)
                skipNegative = true;

            // How many steps in-between the minimum and maximum value.
            int binarySteps = (int)Mathf.Pow(2, binarySize);

            int minSteps = (int)((min + .05) * binarySteps);
            int maxSteps = (int)((max - .05) * binarySteps);

            stateMachine.name = name + " Binary State Machine";
            stateMachine.entryPosition = new Vector3(0, 20, 0);
            stateMachine.anyStatePosition = new Vector3
            (
                20,
                stateMachine.entryPosition.x - 10 - Mathf.Cos(0) * (150 + binarySteps * 4),
                0
            );

            AnimatorState[] finalStates = new AnimatorState[binarySteps];

            for (int i = 0; i < finalStates.Length; i++)
            {
                finalStates[i] = stateMachine.AddState("test");
            }

            for (int i = 0; i < binarySteps; i++)
            {
                // Exit state conditions
                for (int j = 0; j < binarySize; j++)
                {
                    AnimatorStateTransition _exitStateTransition = finalStates[i].AddExitTransition();

                    int _conditionSetTrue = (i >> j) & 1;

                    if (_conditionSetTrue == 1)
                        _exitStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0, name + Mathf.Pow(2, j));
                    else _exitStateTransition.AddCondition(AnimatorConditionMode.If, 0, name + Mathf.Pow(2, j));

                    _exitStateTransition.duration = 0;
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

                finalStates[i].motion = _blendTree;
                finalStates[i].writeDefaultValues = writeDefaults;
            }

            ConvergeBinaryEndStates(finalStates, stateMachine, name);
        }

        private static void ConvergeBinaryEndStates(AnimatorState[] states, AnimatorStateMachine stateMachine, string name, int binaryIter = 1)
        {
            // Breaks out of recursion once there are 2 states left.
            if (states.Length == 2)
            {
                AnimatorState basisState = stateMachine.AddState("test");

                AnimatorStateTransition[] basistransitions = new AnimatorStateTransition[2];

                basistransitions[0] = basisState.AddTransition(states[0]);

                for (int i = 0; i < basistransitions.Length; i++)
                {
                    basistransitions[i] = basisState.AddTransition(states[i]);
                    basistransitions[i].duration = 0;
                    basistransitions[i].exitTime = 0;
                }

                basistransitions[0].AddCondition(AnimatorConditionMode.IfNot, 0, name + binaryIter);
                basistransitions[1].AddCondition(AnimatorConditionMode.If, 0, name + binaryIter);

                return;
            }

            // Node states that are to be recursively sent until binary size is 1.
            AnimatorState[] nodeStates = new AnimatorState[states.Length / 2];

            // Assigning the new nodes to the State Machine
            for (int i = 0; i < nodeStates.Length; i++)
            {
                nodeStates[i] = stateMachine.AddState("test");
            }

            // Pair even/odd states and hook them to a node state. The node states will be half the number of binary states
            for (int i = 0; i < states.Length; i++)
            {
                int nodeIter = (int)(Math.Ceiling((i + .5) / 2) - 1);

                AnimatorStateTransition _transition = nodeStates[nodeIter].AddTransition(states[i]);

                if (i % 2 == 1)
                    _transition.AddCondition(AnimatorConditionMode.If, 0, name + binaryIter);
                else
                {
                    _transition.AddCondition(AnimatorConditionMode.IfNot, 0, name + binaryIter);
                }

                _transition.duration = 0;
                _transition.exitTime = 0;
            }

            ConvergeBinaryEndStates(nodeStates, stateMachine, name, binaryIter * 2);
        }

        public void CreateSmoothingLayer(float smoothness)
        {
            AnimatorControllerParameter smootherParam = ParameterTools.CheckAndCreateParameter(baseParamName + "Smoother", animatorController, 1, smoothness);
            ParameterTools.CheckAndCreateParameter(baseParamName + "Proxy", animatorController, 1);
            ParameterTools.CheckAndCreateParameter(baseParamName, animatorController, 1);

            // Clear out existing ...Binary layers to make-way for an updated one
            for (int i = 0; i < animatorController.layers.Length; i++)
                if (animatorController.layers[i].name == baseParamName + " Float Smoother")
                {
                    animatorController.RemoveLayer(i);
                }

            // Creates an animation layer
            AnimatorControllerLayer layer = new AnimatorControllerLayer
            {
                name = baseParamName + " Float Smoother",
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

            //Adds the Layer asset to the animator controller
            animatorController.AddLayer(layer);

            // Creating 3 blend trees to create the feedback loop
            BlendTree rootTree = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                hideFlags = HideFlags.HideInHierarchy,
                blendParameter = baseParamName + "Smoother",
                name = "Root",
                useAutomaticThresholds = false
            };
            BlendTree falseTree = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                hideFlags = HideFlags.HideInHierarchy,
                blendParameter = baseParamName + "Proxy",
                name = "ProxyBlend",
                useAutomaticThresholds = false
            }; ; 
            BlendTree trueTree = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                hideFlags = HideFlags.HideInHierarchy,
                blendParameter = baseParamName,
                name = "TrueBlend",
                useAutomaticThresholds = false
            }; ;

            // Create smoothing anims
            AnimationClip[] driverAnims = BinaryParameterFloatDriver.CreateFloatSmootherAnimation(baseParamName, -1f);

            rootTree.AddChild(falseTree, 0);
            rootTree.AddChild(trueTree, 1);

            falseTree.AddChild(driverAnims[0], -1);
            falseTree.AddChild(driverAnims[1], 1);

            trueTree.AddChild(driverAnims[0], -1);
            trueTree.AddChild(driverAnims[1], 1);

            AssetDatabase.AddObjectToAsset(rootTree, AssetDatabase.GetAssetPath(layer.stateMachine));
            AssetDatabase.AddObjectToAsset(falseTree, AssetDatabase.GetAssetPath(layer.stateMachine));
            AssetDatabase.AddObjectToAsset(trueTree, AssetDatabase.GetAssetPath(layer.stateMachine));

            AnimatorState state = layer.stateMachine.AddState("Smoother");

            state.motion = rootTree;
        }

    }
}
