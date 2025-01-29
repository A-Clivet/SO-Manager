using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using SOCreatorPackage;

namespace SOCreatorPackage
{
    public class SOCreator : EditorWindow
    {
        private Type selectedType; // Type de ScriptableObject sélectionné
        private ScriptableObject instance; // Instance du ScriptableObject créé
        private List<Type> availableTypes; // Liste des types disponibles
        private Vector2 scrollPos;

        [MenuItem("Rendu/SO_Creator_AC")]
        public static void OpenWindow()
        {
            GetWindow<SOCreator>("SO Creator");
        }

        private void OnEnable()
        {
            RefreshAvailableTypes(); // Met à jour la liste des types disponibles
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("ScriptableObject Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Afficher un popup pour sélectionner le type
            if (availableTypes != null && availableTypes.Count > 0)
            {
                string[] typeNames = availableTypes.Select(t => t.Name).ToArray();
                int selectedIndex = availableTypes.IndexOf(selectedType);
                selectedIndex = EditorGUILayout.Popup("Select Type", selectedIndex, typeNames);

                // Met à jour le type sélectionné
                selectedType = selectedIndex >= 0 ? availableTypes[selectedIndex] : null;

                // Crée une nouvelle instance si le type change
                if (selectedType != null && (instance is null || instance.GetType() != selectedType))
                {
                    instance = CreateInstance(selectedType);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No ScriptableObject types found. Please refresh.", MessageType.Warning);
            }

            EditorGUILayout.Space();

            // Dessine les champs si une instance est créée
            if (instance is not null)
            {
                EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                SerializedObject serializedObject = new SerializedObject(instance);
                SerializedProperty property = serializedObject.GetIterator();
                property.NextVisible(true); // Passer au premier champ


                // Calcul de la largeur maximale pour les libellés
                float maxLabelWidth = 0f;

                var fields = instance.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() is null)
                        continue;

                    string fieldName = $"{field.Name} ({(field.IsPublic ? "public" : "private")})";
                    float labelWidth = GUI.skin.label.CalcSize(new GUIContent(fieldName)).x;
                    maxLabelWidth = Mathf.Max(maxLabelWidth, labelWidth);
                }

                // Boucle pour afficher les champs
                while (property.NextVisible(false))
                {
                    FieldInfo fieldInfo = instance.GetType().GetField(property.name,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    // Vérifie si le champ est lié à un ShowIfAttribute
                    ShowIfAttribute showIfAttribute = fieldInfo?.GetCustomAttribute<ShowIfAttribute>();
                    if (showIfAttribute is not null)
                    {
                        // Récupère le champ conditionnel
                        FieldInfo conditionField = instance.GetType().GetField(showIfAttribute.ConditionField,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (conditionField is not null && conditionField.FieldType == typeof(bool))
                        {
                            // Vérifie la condition
                            bool conditionValue = (bool)conditionField.GetValue(instance);
                            if (!conditionValue)
                            {
                                continue; // Skip le champ si la condition est fausse
                            }
                        }
                        else
                        {
                            Debug.LogError(
                                $"Condition field '{showIfAttribute.ConditionField}' is missing or not a boolean.");
                        }
                    }

                    string accessModifier = fieldInfo is not null && fieldInfo.IsPublic ? "public" : "private";

                    EditorGUILayout.BeginHorizontal();

                    // Largeur dynamique basée sur la taille du libellé le plus large
                    GUILayout.Label($"({accessModifier}) {property.displayName}", GUILayout.Width(maxLabelWidth + 10));

                    // Champ d'édition
                    EditorGUILayout.PropertyField(property, GUIContent.none, true, GUILayout.ExpandWidth(true));

                    EditorGUILayout.EndHorizontal();
                }


                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();

                // Bouton pour créer l'asset
                if (GUILayout.Button("Create Asset"))
                {
                    CreateAsset(instance);
                }
            }

            // Bouton de rafraîchissement
            if (GUILayout.Button("Refresh Available Types"))
            {
                RefreshAvailableTypes();
            }
        }

        private void RefreshAvailableTypes()
        {
            // Récupère tous les types de ScriptableObject marqués par [IncludeInSOCreator]
            availableTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t =>
                    t.GetCustomAttributes(typeof(IncludeInSOCreatorAttribute), true).Length > 0 &&
                    t.IsSubclassOf(typeof(ScriptableObject)) &&
                    !t.IsAbstract)
                .ToList();
        }

        private void CreateAsset(ScriptableObject so)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save ScriptableObject",
                so.name,
                "asset",
                "Select a location to save the ScriptableObject."
            );

            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.SaveAssets();
        }
    }
}