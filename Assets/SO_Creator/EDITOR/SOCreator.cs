using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class SOCreator : EditorWindow
{
    private Type selectedType; // Type de ScriptableObject sélectionné
    private ScriptableObject instance; // Instance du ScriptableObject créé
    private List<Type> availableTypes; // Liste des types disponibles
    private Vector2 scrollPos;

    [MenuItem("Tools/SO Creator")]
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
            if (selectedType != null && (instance == null || instance.GetType() != selectedType))
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
        if (instance != null)
        {
            EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            SerializedObject serializedObject = new SerializedObject(instance);
            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true); // Passer au premier champ

            while (property.NextVisible(false))
            {
                EditorGUILayout.PropertyField(property, true);
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
        // string path = EditorUtility.SaveFilePanelInProject(
        //     "Save ScriptableObject",
        //     so.name,
        //     "asset",
        //     "Select a location to save the ScriptableObject."
        // );

        AssetDatabase.CreateAsset(so, "Assets/SO_Creator/ScriptableObjects/Unit.cs");
        AssetDatabase.SaveAssets();
        
    }
}
