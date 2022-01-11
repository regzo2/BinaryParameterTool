using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class BinaryParameterWindow : EditorWindow
{
    private AnimationClip _initClip;
    private AnimationClip _finalClip;
    private AnimationClip _finalNegativeClip;

    private AnimatorController _animatorController;

    private bool _isCombined = false;
    private bool _nextStateInterrupt = true;
    private bool _writeDefaults = false;
    private bool _orderedInterrupt = false;

    private float _min = 0f;
    private float _max = 1f;
    private float _minNeg = 0f;
    private float _maxNeg = 1f;
    private float _duration = 0.15f;

    private int _binarySize;
    private int _binarySizeTemp;

    private string _baseParamName;

    readonly private string[] _binarySizeSelection = new string[]
        {
            "2","4","8","16","32"
        };

    [MenuItem("Tools/VRCFaceTracking/Binary Parameter Tool")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<BinaryParameterWindow>("Binary Parameter Tool");
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle
        {
            richText = true
        };

        EditorGUILayout.LabelField("<size=12><color=white><b>Avatar Properties</b></color></size>", style);

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

        if (_animatorController != null)
        {
            if (_animatorController == null)
                return;

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

            int _memoryUsage = (_binarySize + (_isCombined ? 1 : 0));

            EditorGUILayout.HelpBox("Parameter Memory Usage: " + (_memoryUsage).ToString() + " bit" + (_memoryUsage > 1 ? "s" : ""), MessageType.Info, true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=12><color=white><b>Animation Properties</b></color></size>", style);

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

            _max = EditorGUILayout.FloatField(_max);
            EditorGUILayout.EndHorizontal();

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

                _maxNeg = EditorGUILayout.FloatField(_maxNeg);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            _duration = EditorGUILayout.FloatField
            (
                new GUIContent
                (
                    "Transition Duration",
                    "How long does it take to transition to the active step? Lower values " +
                    "will be quicker but look more 'steppy', while higher values will look " +
                    "smoother but may feel 'sluggish' A good middle ground is 0.15 for 8 " +
                    "Resolution, more duration on less resolution and vice versa"
                ),
                _duration
            );

            _nextStateInterrupt = EditorGUILayout.Toggle
            (
                new GUIContent
                (
                    "Next State Interrupt",
                    "Can the destination state interrupt the current transition? Recommend " +
                    "turning this value on to make the animations connect better with the active " +
                    "parameter value."
                ),
                _nextStateInterrupt
            );

            if (_nextStateInterrupt)
                _orderedInterrupt = EditorGUILayout.Toggle
               (
                   new GUIContent
                   (
                       "Ordered Interrupt",
                       "Does Next State Interrupt need to follow a set order on the Any State transition? " +
                       "Very useful for things like blinking where it should be forced to go to a set state " +
                       "(like closed lids) if a higher priority state is activated before returning to the true " +
                       "active state."
                   ),
                   _orderedInterrupt
               );

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
                    BinaryParameterScript.CreateBinaryLayer(_baseParamName, _animatorController, _binarySize, _initClip, _finalClip, _min, _max, _duration, _nextStateInterrupt, _orderedInterrupt, _writeDefaults);
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
                BinaryParameterScript.CreateCombinedBinaryLayer(_baseParamName, _animatorController, _binarySize, _initClip, _finalClip, _finalNegativeClip, _min, _max, _minNeg, _maxNeg, _duration, _nextStateInterrupt, _orderedInterrupt, _writeDefaults);

            EditorGUILayout.HelpBox("Parameters To Add:" + GenerateParamNames(_baseParamName, _binarySize, _isCombined), MessageType.None);
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
}
