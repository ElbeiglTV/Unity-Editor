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
}
