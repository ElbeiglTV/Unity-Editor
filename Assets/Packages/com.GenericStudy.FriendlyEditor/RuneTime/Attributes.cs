using System;
using UnityEngine;

namespace FriendlyEditor.UtilityAttributes
{
    #region HighlightAttribute
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class HighlightAttribute : PropertyAttribute
    {
        public Color color;

        public HighlightAttribute(float r, float g, float b)
        {
            this.color = new Color(r, g, b);
        }

    }
    #endregion
    #region TypePopupAttribute
    public class TypePopupAttribute : PropertyAttribute
    {
        public System.Type baseType;

        public TypePopupAttribute(System.Type baseType)
        {
            this.baseType = baseType;
        }
    }
    #endregion
    #region StringPopupAttribute
    public class StringPopupAttribute : PropertyAttribute
    {
        public string[] values;
        public string jsonPath;

        public StringPopupAttribute(params string[] values)
        {
            this.values = values;
        }
        public StringPopupAttribute(string jsonPath)
        {
            this.jsonPath = jsonPath;
        }
    }
    #endregion
    #region GetRequieredComponentAttribute
    public class GetRequieredComponentAttribute : PropertyAttribute
    {
        public System.Type requiredComponent;

        public GetRequieredComponentAttribute(System.Type requiredComponent)
        {
            this.requiredComponent = requiredComponent;
        }
    }
    #endregion
    #region TagAttribute
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class DebugTagAttribute : Attribute
    {
        public string Label { get; private set; }

        public DebugTagAttribute(string label)
        {
            this.Label = label;
        }
    }
    #endregion
    #region DynamicEnumSelectorAttribute


    public class DynamicEnumSelectorAttribute : PropertyAttribute
    {
        // Atributo vacío solo para marcar los campos
    }

    #endregion
    #region BoolButtonAttribute

    public class BoolButtonAttribute : PropertyAttribute
    {
        public string TrueLabel { get; }
        public string FalseLabel { get; }

        public BoolButtonAttribute(string trueLabel, string falseLabel)
        {
            TrueLabel = trueLabel;
            FalseLabel = falseLabel;
        }
    }
    #endregion
    #region VariableSelectorAttribute
    public class VariableSelectorAttribute : PropertyAttribute
    {
    }
    #endregion

    #region MethodButtonAttribute
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute
    {
        public string Label { get; }

        public string ControllingField { get; }
        public object[] DesiredValues { get; }

        public ButtonAttribute(string label = null, string controllingField = null, params object[] desiredValues)
        {
            Label = label;
            ControllingField = controllingField;
            DesiredValues = desiredValues;
        }
    }
    #endregion

    #region ConditionalAttribute
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class ConditionalAttribute : PropertyAttribute
    {
        public string ControllingField { get; }
        public object[] DesiredValues { get; }
        public string HeaderText { get; }
        public bool UseHeader { get; }
        public int Indent { get; }

        public ConditionalAttribute(string controllingField, params object[] desiredValues)
        {
            ControllingField = controllingField;
            DesiredValues = desiredValues;
            UseHeader = false;
            Indent = 0;
        }

        public ConditionalAttribute(string controllingField, int indent = 0, params object[] desiredValues)
        {
            ControllingField = controllingField;
            DesiredValues = desiredValues;
            UseHeader = false;
            Indent = indent;
        }

        public ConditionalAttribute(string headerText, string controllingField, params object[] desiredValues)
        {
            ControllingField = controllingField;
            DesiredValues = desiredValues;
            HeaderText = headerText;
            UseHeader = true;
        }
    }
    #endregion
    #region VisualizeAttribute
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class VisualizeAttribute : PropertyAttribute
    {
        public bool useFoldout;
        public string foldoutName;
        public bool showObjectPath;

        public VisualizeAttribute() { }

        public VisualizeAttribute(string foldoutName = "", bool useFoldout = false, bool showObjectPath = false)
        {
            this.foldoutName = foldoutName;
            this.useFoldout = useFoldout;
            this.showObjectPath = showObjectPath;
        }
    }
    #endregion
    #region ReadAttribute
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadAttribute : PropertyAttribute { }
    #endregion

    #region RelatedValuesAttribute
    [Serializable]
    public struct RelatedValues<W, T, F>
    {
        public W Left;
        public T Centre;
        public F Right;
    }
    #endregion
    #region DrawTogetherAttribute
    public enum DrawType { Variable, Struct }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DrawTogetherAttribute : PropertyAttribute
    {
        public string DisplayName;
        public int Order;
        public DrawType Type;
        public bool DisplayGroupName;

        public DrawTogetherAttribute(string displayName)
        {
            DisplayName = displayName;
            Type = DrawType.Variable;
            Order = 0;
            DisplayGroupName = false;
        }

        public DrawTogetherAttribute(int order)
        {
            DisplayName = "";
            Type = DrawType.Variable;
            Order = order;
            DisplayGroupName = false;
        }

        public DrawTogetherAttribute(string displayName, int order, DrawType type = DrawType.Variable, bool displayGroupName = false)
        {
            DisplayName = displayName;
            Order = order;
            Type = type;
            DisplayGroupName = displayGroupName;
        }

        public DrawTogetherAttribute(string displayName, DrawType type = DrawType.Struct, bool displayGroupName = false)
        {
            DisplayName = displayName;
            Order = 0;
            Type = type;
            DisplayGroupName = displayGroupName;
        }

        public DrawTogetherAttribute(DrawType type)
        {
            DisplayName = "";
            Type = type;
            Order = 0;
            DisplayGroupName = false;
        }
    }
    #endregion
    #region FlexibleSliderAttribute
    public enum SliderType
    {
        Normal,   // single float/int slider
        MinMax,   // Vector2/Vector2Int min-max slider
        Vector    // Vector2/3/4 slider (each component with its own slider)
    }

    public enum ClampType
    {
        Clamped,
        Unclamped
    }

    public class FlexibleSliderAttribute : PropertyAttribute
    {
        public readonly SliderType Type;
        public readonly ClampType Clamp;

        public readonly float Min;
        public readonly float Max;

        public readonly string MinProperty;
        public readonly string MaxProperty;

        public readonly string RangeVectorProperty;

        public readonly float Step;
        public readonly string StepProperty;

        /*
        // --- Numeric Range Constructor ---
        public FlexibleSliderAttribute(SliderType type, float min, float max,
                                       ClampType clamp = ClampType.Clamped, float step = 0f, string stepProperty = null)
        {
            Type = type;
            Min = min;
            Max = max;
            Clamp = clamp;
            Step = step;
            StepProperty = stepProperty;
        }

        // --- Min/Max Property Names (optional step or stepProperty) ---
        public FlexibleSliderAttribute(SliderType type, string minProperty, string maxProperty,
                                       ClampType clamp = ClampType.Clamped, float step = 0f, string stepProperty = null)
        {
            Type = type;
            MinProperty = minProperty;
            MaxProperty = maxProperty;
            Clamp = clamp;
            Step = step;
            StepProperty = stepProperty;
        }
        */

        // --- Vector2 Range Property (optional step) ---
        public FlexibleSliderAttribute(SliderType type, string rangeVectorProperty,
                                       ClampType clamp = ClampType.Clamped, float step = 0f, string stepProperty = null)
        {
            Type = type;
            RangeVectorProperty = rangeVectorProperty;
            Clamp = clamp;
            Step = step;
            StepProperty = stepProperty;
        }

        public FlexibleSliderAttribute(SliderType type, float min = 0, float max = 1, string minProperty = null, string maxProperty = null,
                                       ClampType clamp = ClampType.Clamped, float step = 0f, string stepProperty = null)
        {
            Type = type;
            Min = min;
            Max = max;
            MinProperty = minProperty;
            MaxProperty = maxProperty;
            Clamp = clamp;
            Step = step;
            StepProperty = stepProperty;
        }
    }
    #endregion
    #region DefaultStringAttribute 
    public class DefaultStringAttribute : PropertyAttribute
    {
        public readonly string defaultValue;

        public DefaultStringAttribute(string defaultValue)
        {
            this.defaultValue = defaultValue;
        }
    }
    #endregion
    #region TagFieldAttribute
    public class TagsAttribute : PropertyAttribute
    {

    }
    #endregion

}
