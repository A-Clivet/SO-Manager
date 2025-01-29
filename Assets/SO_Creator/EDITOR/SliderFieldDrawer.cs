using UnityEditor;
using UnityEngine;

namespace SOCreatorPackage
{
    [CustomPropertyDrawer(typeof(SliderFieldAttribute))]
    public class SliderFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SliderFieldAttribute slider = (SliderFieldAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = EditorGUI.IntSlider(position, label, property.intValue, (int)slider.Min, (int)slider.Max);
            }
            else if (property.propertyType == SerializedPropertyType.Float)
            {
                property.floatValue = EditorGUI.Slider(position, label, property.floatValue, slider.Min, slider.Max);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use [SliderField] with int or float.");
            }
        }
    }
}