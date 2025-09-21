namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class MinMaxRangeAttribute : PropertyAttribute
    {
        internal readonly float min;
        internal readonly float max;

        internal MinMaxRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}

namespace UnityEditor
{
    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    internal sealed class MinMaxRange : PropertyDrawer
    {
        private const float kPrefixPaddingRight = 2;
        private const float kSpacing = 5;

        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            var range = attribute as MinMaxRangeAttribute;
            float minValue = property.vector2Value.x;
            float maxValue = property.vector2Value.y;

            {
                Rect labelPosition = new(
                    position.x,
                    position.y,
                    EditorGUIUtility.labelWidth,
                    position.height
                );

                EditorGUI.LabelField(labelPosition, label);
            }

            {
                Rect sliderPosition = new(
                    position.x + EditorGUIUtility.labelWidth + kPrefixPaddingRight + EditorGUIUtility.fieldWidth + kSpacing,
                    position.y,
                    position.width - EditorGUIUtility.labelWidth - 2 * (EditorGUIUtility.fieldWidth + kSpacing) - kPrefixPaddingRight,
                    position.height
                );

                EditorGUI.MinMaxSlider(sliderPosition, ref minValue, ref maxValue, range.min, range.max);
            }

            {
                Rect minPosition = new(
                    position.x + EditorGUIUtility.labelWidth + kPrefixPaddingRight,
                    position.y,
                    EditorGUIUtility.fieldWidth,
                    position.height
                );

                minValue = EditorGUI.FloatField(minPosition, minValue);
            }

            {
                Rect maxPosition = new(
                    position.xMax - EditorGUIUtility.fieldWidth,
                    position.y,
                    EditorGUIUtility.fieldWidth,
                    position.height
                );

                maxValue = EditorGUI.FloatField(maxPosition, maxValue);
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.vector2Value = new Vector2(minValue, maxValue);
            }

            EditorGUI.EndProperty();
        }
    }
}
