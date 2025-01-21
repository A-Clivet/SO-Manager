using UnityEditor;
using UnityEngine;

namespace CustomAttributes
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Récupère l'attribut personnalisé
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;

            // Récupère la propriété booléenne conditionnelle
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.ConditionField);
            if (conditionProperty is null)
            {
                Debug.LogError($"Le champ '{showIf.ConditionField}' n'existe pas dans {property.serializedObject.targetObject.GetType()}.");
                return;
            }

            // Affiche le champ si la condition est remplie
            if (conditionProperty.boolValue)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;

            SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.ConditionField);
            if (conditionProperty is not null && !conditionProperty.boolValue)
            {
                return 0;
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}