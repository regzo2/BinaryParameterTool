using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.Collections.Generic;

namespace VRCFaceTracking.EditorTools
{
    public class BinaryParameterWindow : EditorWindow
    {
        private BinaryStateMachine _binaryStateMachine;
        private VRCAvatarDescriptor _avDescriptor;
    
        private AnimationClip _initClip;
        private AnimationClip _finalClip;
        private AnimationClip _finalNegativeClip;

        private AnimatorController _animatorController;
        private List<AnimatorController> _controllers;

        private bool _isCombined = false;
        private bool _nextStateInterrupt = true;
        private bool _writeDefaults = true;
        private bool _smooth = true;

        private float _min = 0f;
        private float _max = 1f;
        private float _minNeg = 0f;
        private float _maxNeg = 1f;
        private float _duration = 0f;
        private float _smoothness = 0.9f;

        private int _binarySize;
        private int _binarySizeTemp;
        private int _layerSelect = 4;
        private int _tab;

        private string _baseParamName;

        readonly private string[] _binarySizeSelection = new string[]
        {
            "2","4","8","16","32"
        };

        readonly private string[] _animatorSelection = new string[]
        {
            "Base","Additive","Gesture","Action","FX"
        };

        [MenuItem("Tools/VRCFaceTracking/Binary Parameter Tool")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<BinaryParameterWindow>("Binary Parameter Tool");
        }

        private void OnGUI()
        {
            _binaryStateMachine = new BinaryStateMachine();

            GUIStyle style = new GUIStyle
            {
                richText = true
            };

            EditorGUILayout.LabelField("<size=12><color=white><b>Avatar Properties</b></color></size>", style);
            /*
            _animatorController = (AnimatorController)EditorGUILayout.ObjectField
            (
                new GUIContent
                (
                    "Animator Controller",
                    "Animator Controller that will have the parameters, animations, transitions, states, and layer added to."
                ),
                _animatorController,
                typeof(AnimatorController),
                true
            );
            */

            _avDescriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField
            (
                new GUIContent
                (
                    "Avatar",
                    "The VRC Avatar that will have the Binary Parameters set up on. " +
                    "The Avatar must have a VRCAvatarDescriptor to show up in this field."
                ),
                _avDescriptor,
                typeof(VRCAvatarDescriptor),
                true
            );

            if (_avDescriptor != null)
            {
                _layerSelect = EditorGUILayout.Popup
                (
                    new GUIContent
                    (
                        "Layer",
                        "This selects what VRChat Playable Layer you would like to set up " +
                        "the following Binary Animation Layer into. A layer must be populated " +
                        "in order for the tool to properly set up an Animation Layer."
                    ),
                    _layerSelect,
                    _animatorSelection
                );

                _animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(_avDescriptor.baseAnimationLayers[_layerSelect].animatorController));

                _binaryStateMachine.animatorController = _animatorController;

                if (_binaryStateMachine.animatorController == null)
                {
                    EditorGUILayout.HelpBox("This Playable Layer must have an Animator Controller in order for the Binary Parameter Tool to work with it.", MessageType.Warning, true);
                    return;
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("<size=12><color=white><b>Parameter Properties</b></color></size>", style);

                _baseParamName = EditorGUILayout.TextField
                (
                    new GUIContent
                    (
                        "Parameter",
                        "The name of the parameter to be converted into a Binary parameter. " +
                        "These can be found on the VRCFaceTracking GitHub page. Make sure that " +
                        "when using the same parameter to use the same Binary Resolution, otherwise " +
                        "animations may have unintended behavior."
                    ),
                    _baseParamName);

                _binaryStateMachine.baseParamName = _baseParamName;

                _binarySizeTemp = EditorGUILayout.Popup
                (
                    new GUIContent
                    (
                        "Binary Resolution",
                        "How many steps a Binary Parameter can make. Higher values are more accurate, " +
                        "while lower values are more economic for parameter space. Recommended to use a " +
                        "Resolution of 16 or less for more space savings."
                    ),
                    _binarySizeTemp,
                    _binarySizeSelection
                );

                _binarySize  = _binarySizeTemp + 1;

                _binaryStateMachine.binarySize = _binarySize;

                _isCombined = EditorGUILayout.Toggle
                (
                    new GUIContent
                    (
                        "Combined Parameter",
                        "Does this parameter go from positive to negative? " +
                        "This option will add an extra bool to keep track of the " +
                        "positive/negative of the parameter."
                    ),
                    _isCombined
                );

                _binaryStateMachine.isCombined = _isCombined;

                int _memoryUsage = (_binarySize + (_isCombined ? 1 : 0));

                EditorGUILayout.HelpBox("Parameter Memory Usage: " + (_memoryUsage).ToString() + " bit" + (_memoryUsage > 1 ? "s" : ""), MessageType.Info, true);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("<size=12><color=white><b>Animation Properties</b></color></size>", style);

                _tab = GUILayout.Toolbar(_tab,
                    new GUIContent[]
                    {
                        new GUIContent
                        (
                            "Float Parameter",
                            "This Binary State Machine will drive the Float " +
                            "parameter listed in the 'Parameter' field above " +
                            "using Binary parameters. Useful for converting " +
                            "existing animations that are using floats."
                        ),
                        new GUIContent
                        (
                            "Direct Animation",
                            "This mode creates a Binary State Machine that directly drives " +
                            "any animations added to the Animation fields below " +
                            "using Binary parameters."
                        )
                    });

                if (_tab == 1)
                {
                    _initClip = (AnimationClip)EditorGUILayout.ObjectField
                    (
                        new GUIContent
                        (
                            "Initial Clip",
                            "Animation Clip that represents when the parameter is at 0."
                        ),
                        _initClip,
                        typeof(AnimationClip),
                        true
                    );

                    _binaryStateMachine.initClip = _initClip;

                    _finalClip = (AnimationClip)EditorGUILayout.ObjectField
                    (
                        new GUIContent
                        (
                            "Final Clip",
                            "Animation Clip that represents when the parameter is at 1."
                        ),
                        _finalClip,
                        typeof(AnimationClip),
                        true
                    );

                    _binaryStateMachine.finalClip = _finalClip;

                    // Min/Max threshold to use for adjusting animations to better fit faces (may become depricated if params get normalized properly on the face tracker)
                    EditorGUILayout.BeginHorizontal();
                    _min = EditorGUILayout.FloatField
                    (
                        new GUIContent
                        (
                            "Min/Max Anim Thresholds",
                            "When should the animation start/end? Values will be adjusted to " +
                            "fit within the size of the Binary Parameter."
                        ),
                        _min
                    );

                    _binaryStateMachine.min = _min;

                    _max = EditorGUILayout.FloatField(_max);
                    EditorGUILayout.EndHorizontal();

                    _binaryStateMachine.max = _max;

                    if (_isCombined)
                    {
                        EditorGUILayout.Space();

                        _finalNegativeClip = (AnimationClip)EditorGUILayout.ObjectField
                        (
                            new GUIContent
                            (
                                "Final Negative Clip",
                                "Animation Clip that represents when the parameter is at -1. Make sure that it " +
                                "animates the same properties as the Initial Clip!"
                            ),
                            _finalNegativeClip,
                            typeof(AnimationClip),
                            true
                        );

                        _binaryStateMachine.finalNegativeClip = _finalNegativeClip;

                        EditorGUILayout.BeginHorizontal();
                        _minNeg = EditorGUILayout.FloatField
                        (
                            new GUIContent
                            (
                                "Min/Max Anim Thresholds",
                                "When should the animation start/end? Values other than (0,1) " +
                                "might not work as well or be as accurate on lower Binary Bit Resolutions"
                            ),
                            _minNeg
                        );

                        _binaryStateMachine.minNeg = _minNeg;

                        _maxNeg = EditorGUILayout.FloatField(_maxNeg);
                        EditorGUILayout.EndHorizontal();

                        _binaryStateMachine.maxNeg = _maxNeg;
                    }
                }

                if (_tab == 0)
                {
                    _binaryStateMachine.min = 0f;

                    _binaryStateMachine.max = 1f;

                    if (_isCombined)
                    {

                        _binaryStateMachine.finalNegativeClip = _finalNegativeClip;

                        _binaryStateMachine.minNeg = 0f;

                        _binaryStateMachine.maxNeg = 1f;
                    }
                }

                EditorGUILayout.Space();

                if (_tab == 0)
                    _smooth = EditorGUILayout.Toggle
                    (
                        new GUIContent
                        (
                            "Smoother Float Layer",
                            "Creates an animation layer that smooths out " +
                            "the driven float using a feedback loop."
                        ),
                        _smooth
                    );

                if (_tab == 0 && _smooth)
                    _smoothness = EditorGUILayout.FloatField
                    (
                        new GUIContent
                        (
                            "Smoothness",
                            "What percentage of the actual value gets multiplied with the " +
                            "driven Float parameter. The higher the number the smoother the " +
                            "parameter gets blended. Recommended 0.9"
                        ),
                        _smoothness
                    );

                if (!_smooth)
                {
                    _duration = EditorGUILayout.FloatField
                    (
                        new GUIContent
                        (
                            "Transition Duration",
                            "How long does it take to transition to the active step? Lower values " +
                            "will be quicker but look more 'steppy', while higher values will look " +
                            "smoother but may feel 'sluggish' A good middle ground is 0.1 for 8 " +
                            "Resolution, more duration on less resolution and vice versa"
                        ),
                        _duration
                    );

                    _binaryStateMachine.duration = _duration;

                    _nextStateInterrupt = EditorGUILayout.Toggle
                   (
                       new GUIContent
                       (
                           "Next State Interrupt",
                           "Cant the next state interrupt the current transition? Very " +
                           "useful in making the animation states better connect with " +
                           "the current parameter value (less delay)."
                       ),
                       _nextStateInterrupt
                   );

                    _binaryStateMachine.nextStateInterrupt = _nextStateInterrupt;
                }

                else
                {
                    _binaryStateMachine.duration = 0;
                    _binaryStateMachine.nextStateInterrupt = false;
                }


                _writeDefaults = EditorGUILayout.Toggle
                (
                    new GUIContent
                    (
                        "Write Defaults",
                        "Can the Animations on this layer set unbound Animation Properties to their default " +
                        "values? Recommended to keep this off to avoid blendshape animation conflicts. WARNING: " +
                        "You will run into issues if you have Write Defaults enabled and disabled on different " +
                        "animations, use a tool like AV3Manager to set Write Defaults to a universal enable/disable."

                    ),
                    _writeDefaults
                );

                _binaryStateMachine.writeDefaults = _writeDefaults;

                EditorGUILayout.Space();
                if (!_isCombined)
                {
                    if (GUILayout.Button
                    (
                        new GUIContent
                        (
                            "Create Binary Parameter Layer",
                            "Creates a new Layer in the selected Animator Controller as well as a set of states with " +
                            "set animations, transitions, and parameters that handle the specified Binary Parameter."
                        )))
                    {
                        if (_tab == 0 && !_smooth)
                        {
                            ParameterTools.CheckAndCreateParameter(_baseParamName, _animatorController, 1);

                            _binaryStateMachine.initClip = BinaryParameterFloatDriver.CreateFloatDriverAnimation(_baseParamName, 0f);
                            _binaryStateMachine.finalClip = BinaryParameterFloatDriver.CreateFloatDriverAnimation(_baseParamName, 1f);
                        }
                        else if (_tab == 0)
                        {
                            ParameterTools.CheckAndCreateParameter(_baseParamName, _animatorController, 1);

                            _binaryStateMachine.CreateSmoothingLayer(_smoothness);

                            _binaryStateMachine.initClip = BinaryParameterFloatDriver.CreateFloatDriverAnimation(_baseParamName + "Proxy", 0f);
                            _binaryStateMachine.finalClip = BinaryParameterFloatDriver.CreateFloatDriverAnimation(_baseParamName + "Proxy", 1f);
                        }
                        if (ParameterTools.AddVRCParameter(_avDescriptor, GenerateBinaryParams(_baseParamName, _binarySize, _isCombined)))
                        {
                            ParameterTools.RemoveVRCParameter(_avDescriptor, new VRCExpressionParameters.Parameter
                            {
                                name = _baseParamName,
                                valueType = VRCExpressionParameters.ValueType.Float
                            });
                            _binaryStateMachine.CreateBinaryLayer();
                        }
                        else
                            EditorGUILayout.HelpBox("Parameters can not fit, or Expressions Parameters do not exist.", MessageType.Warning);
                    }
                }
                else if (GUILayout.Button
                (
                    new GUIContent
                    (
                        "Create Combined Binary Parameter Layer",
                        "Creates a new Layer in the selected Animator Controller as well as a set of states with " +
                        "set animations, transitions, and parameters that handle the specified Combined Binary Parameter."
                    )))
                {
                    if (ParameterTools.AddVRCParameter(_avDescriptor, GenerateBinaryParams(_baseParamName, _binarySize, _isCombined)))
                    {
                        if (_tab == 0 && !_smooth)
                        {
                            ParameterTools.RemoveVRCParameter(_avDescriptor, new VRCExpressionParameters.Parameter
                            {
                                name = _baseParamName,
                                valueType = VRCExpressionParameters.ValueType.Float
                            });
                            ParameterTools.CheckAndCreateParameter(_baseParamName, _animatorController, 1);

                            _binaryStateMachine.initClip = BinaryParameterFloatDriver.CreateFloatDriverAnimation(_baseParamName, 0f);
                            _binaryStateMachine.finalClip = BinaryParameterFloatDriver.CreateFloatDriverAnimation(_baseParamName, 1f);
                            _binaryStateMachine.finalNegativeClip = BinaryParameterFloatDriver.CreateFloatDriverAnimation(_baseParamName, -1f);
                        }
                        else if (_tab == 0)
                        {
                            ParameterTools.RemoveVRCParameter(_avDescriptor, new VRCExpressionParameters.Parameter
                            {
                                name = _baseParamName,
                                valueType = VRCExpressionParameters.ValueType.Float
                            });
                            ParameterTools.CheckAndCreateParameter(_baseParamName, _animatorController, 1);

                            _binaryStateMachine.CreateSmoothingLayer(_smoothness);
                            _binaryStateMachine.initClip = BinaryParameterFloatDriver.CreateFloatDriverAnimation(_baseParamName + "Proxy", 0f);
                            _binaryStateMachine.finalClip = BinaryParameterFloatDriver.CreateFloatDriverAnimation(_baseParamName + "Proxy", 1f);
                            _binaryStateMachine.finalNegativeClip = BinaryParameterFloatDriver.CreateFloatDriverAnimation(_baseParamName + "Proxy", -1f);
                        }
                        _binaryStateMachine.CreateCombinedBinaryLayer();
                    }
                    else
                        EditorGUILayout.HelpBox("Parameters can not fit, or Expressions Parameters do not exist.", MessageType.Warning);
                }
                EditorGUILayout.HelpBox("Parameters (" + _avDescriptor.expressionParameters.CalcTotalCost() + "/128):" + GenerateParamNames(_baseParamName, _binarySize, _isCombined), MessageType.None);

            }
        }

        private string GenerateParamNames(string name, int binarySize, bool isCombined)
        {
            string paramString = "";

            for (int i = 0; i < binarySize; i++)
            {
                paramString += "\n" + name + Mathf.Pow(2, i);
            }

            paramString += isCombined ? "\n" + name + "Negative" : "";

            return paramString;
        }

        private List<VRCExpressionParameters.Parameter> GenerateBinaryParams(string name, int binarySize, bool isCombined)
        {
            List<VRCExpressionParameters.Parameter> param = new List<VRCExpressionParameters.Parameter>();

            for (int i = 0; i < binarySize; i++)
            {
                string paramString = name + Mathf.Pow(2, i);

                param.Add
                (
                    new VRCExpressionParameters.Parameter
                    {
                        name = paramString,
                        valueType = VRCExpressionParameters.ValueType.Bool,
                        saved = false
                    }
                );;
            }

            if (isCombined)
            param.Add
                (
                    new VRCExpressionParameters.Parameter
                    {
                        name = _baseParamName + "Negative",
                        valueType = VRCExpressionParameters.ValueType.Bool,
                        saved = false
                    }
                );

            return param;
        }
    }
}
