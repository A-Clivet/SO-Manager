using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/*
 * Exo Tools
 * Axel Clivet
 *
 *
 * Editor Window
 * Creation SO
 * Attribute custom field (serialized field est un attribute (attribute = ce qui est entre []), donc il faut en faire un perso qui a un effet)
 * utiliser une liste de string
 */

using UnityEditor;
using UnityEngine;

using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

public class SOCreator : EditorWindow
{
    private Type selectedType;
    private ScriptableObject instance;
    private Vector2 scrollPos;

    private List<Type> cachedScriptableObjectTypes; // Cache des types disponibles

    [MenuItem("Tools/SO Creator")]
    public static void OpenWindow()
    {
        GetWindow<SOCreator>("SO Creator");
    }

    private void OnEnable()
    {
        // Charge les types au démarrage
        RefreshScriptableObjectTypes();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("ScriptableObject Creator", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh ScriptableObject Types", GUILayout.Height(25)))
        {
            RefreshScriptableObjectTypes();
        }

        if (cachedScriptableObjectTypes != null && cachedScriptableObjectTypes.Count > 0)
        {
            var typeNames = cachedScriptableObjectTypes.Select(t => t.Name).ToArray();
            int selectedIndex = cachedScriptableObjectTypes.IndexOf(selectedType);
            selectedIndex = EditorGUILayout.Popup("Select Type", selectedIndex, typeNames);
            selectedType = selectedIndex >= 0 ? cachedScriptableObjectTypes[selectedIndex] : null;

            if (selectedType is not null)
            {
                if (instance is null || instance.GetType() != selectedType)
                {
                    instance = CreateInstance(selectedType);
                }

                // Affichage des champs et validation
                DrawFields(instance);
                bool isValid = IsInstanceValid(instance);

                if (!isValid)
                {
                    EditorGUILayout.HelpBox("Some fields are invalid or missing. Please correct them before proceeding.", MessageType.Error);
                }

                // Bouton pour créer l'asset
                using (new EditorGUI.DisabledScope(!isValid))
                {
                    if (GUILayout.Button("Create Asset", GUILayout.Height(30)))
                    {
                        CreateAsset(instance);
                    }
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No ScriptableObject types found. Please refresh.", MessageType.Warning);
        }
    }


    private void RefreshScriptableObjectTypes()
    {
        cachedScriptableObjectTypes = GetAllScriptableObjectTypes().ToList();
    }

    private Type[] GetAllScriptableObjectTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(ScriptableObject)) &&
                           !type.IsAbstract &&
                           type.GetCustomAttributes(typeof(IncludeInSOCreatorAttribute), true).Length > 0)
            .ToArray();
    }

    private bool IsInAssetsFolder(Type type)
    {
        string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");
        return guids.Any(guid =>

        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return path.StartsWith("Assets/");
        });
    }

    private void DrawFields(ScriptableObject so)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        var fields = so.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.IsNotSerialized || (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null))
                continue;

            var requiredAttribute = field.GetCustomAttribute<RequiredFieldAttribute>();
            object value = field.GetValue(so);

            // Couleur rouge si champ requis invalide
            if (requiredAttribute != null && (value == null || (value is string s && string.IsNullOrEmpty(s))))
            {
                GUI.color = Color.red;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(field.Name, GUILayout.Width(150));

            if (field.FieldType == typeof(int))
            {
                int intValue = EditorGUILayout.IntField((int)value);
                field.SetValue(so, intValue);
            }
            else if (field.FieldType == typeof(float))
            {
                float floatValue = EditorGUILayout.FloatField((float)value);
                field.SetValue(so, floatValue);
            }
            else if (field.FieldType == typeof(string))
            {
                string stringValue = EditorGUILayout.TextField((string)value);
                field.SetValue(so, stringValue);
            }
            else if (field.FieldType == typeof(Sprite))
            {
                Sprite spriteValue = (Sprite)EditorGUILayout.ObjectField((Sprite)value, typeof(Sprite), false);
                field.SetValue(so, spriteValue);
            }

            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white; // Réinitialise la couleur
        }

        EditorGUILayout.EndScrollView();
    }

    
    private bool IsInstanceValid(ScriptableObject so)
    {
        var fields = so.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var requiredAttribute = field.GetCustomAttribute<RequiredFieldAttribute>();
            if (requiredAttribute != null)
            {
                var value = field.GetValue(so);
                if (value == null || (value is string s && string.IsNullOrEmpty(s)))
                {
                    return false;
                }
            }
        }
        return true;
    }


    private void CreateAsset(ScriptableObject so)
    {
        string path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", so.name, "asset", "Choose a location to save the asset.");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"ScriptableObject of type {so.GetType().Name} saved to {path}");
        }
    }
}
