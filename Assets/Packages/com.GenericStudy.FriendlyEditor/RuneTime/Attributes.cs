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
}
