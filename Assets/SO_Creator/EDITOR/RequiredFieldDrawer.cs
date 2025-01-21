using UnityEditor;
using UnityEngine;

namespace CustomAttributes
{
    [CustomPropertyDrawer(typeof(RequiredFieldAttribute))]
    public class RequiredFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isValid = true;

            // Vérifie si la propriété est null ou vide
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                isValid = property.objectReferenceValue is not null;
            }
            else if (property.propertyType == SerializedPropertyType.String)
            {
                isValid = !string.IsNullOrEmpty(property.stringValue);
            }

            // Affiche un champ coloré si invalide
            if (!isValid)
            {
                GUI.color = Color.red;
            }

            EditorGUI.PropertyField(position, property, label);

            // Réinitialise la couleur
            GUI.color = Color.white;

            // Affiche un message d'erreur dans l'inspecteur
            if (!isValid)
            {
                Rect helpBoxRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight * 1.5f);
                EditorGUI.HelpBox(helpBoxRect, $"{label.text} is required!", MessageType.Error);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool isValid = true;

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                isValid = property.objectReferenceValue is not null;
            }
            else if (property.propertyType == SerializedPropertyType.String)
            {
                isValid = !string.IsNullOrEmpty(property.stringValue);
            }

            // Augmente la hauteur si invalide pour afficher le message d'erreur
            if (!isValid)
            {
                return EditorGUIUtility.singleLineHeight * 2.5f;
            }

            return EditorGUIUtility.singleLineHeight;
        }
    }
}
