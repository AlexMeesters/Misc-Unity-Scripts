// Created by Alex Meesters
// www.low-scope.com, www.alexmeesters.nl
// Licenced under the MIT Licence. https://en.wikipedia.org/wiki/MIT_License

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lowscope
{
    /// <summary>
    /// Generates an .asset file specifically for a class (T) upon access. Gets placed in "Editor Default Resources"
    /// This is purely if you want a class to act as a singleton for specific data.
    /// Useful if you require asset references from the editor, such as prefabs.
    /// 
    /// Simply create a ScriptableObject that derives from EditorDataObject<ScriptableObjectTypeName>.
    /// You can then reference assets and create static methods/fields to access them.
    /// For instance: public static string MySceneName => Instance.mySceneName;
    /// </summary>
    /// <typeparam name="T">Type of the ScriptableObject </typeparam>
    [Serializable]
    abstract public class SingleEditorDataObject<T> : ScriptableObject where T : ScriptableObject
    {
        public static T Instance => LoadInstance();

        private static T instance = null;

        private static T LoadInstance()
        {
            if (instance != null)
                return instance;

            // Get instance of the attribute.
            EditorDataObjectName getAttribute = (EditorDataObjectName)Attribute.GetCustomAttribute
                (typeof(T), typeof(EditorDataObjectName));

            string typeName = (getAttribute != null)? getAttribute.GetName() : typeof(T).ToString();
            string editorResourcesPath = $"{Application.dataPath}/Editor Default Resources";
            string classPath = $"{editorResourcesPath}/{typeName}.asset";

            // Create Editor Default Resources folder if it does not exist.
            if (!Directory.Exists(editorResourcesPath))
            {
                Directory.CreateDirectory(editorResourcesPath);
            }

            // Create scriptable object .asset file if it does not exist.
            if (!File.Exists(classPath))
            {
                var createInstance = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(createInstance, $"Assets/Editor Default Resources/{typeName}.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                instance = createInstance;
                (instance as SingleEditorDataObject<T>).OnCreated();
                return createInstance;
            }

            // Try to load the file if it does exist.
            T loadInstance = AssetDatabase.LoadAssetAtPath<T>($"Assets/Editor Default Resources/{typeName}.asset");

            if (loadInstance == null)
            {
                loadInstance = EditorGUIUtility.Load($"{typeName}") as T;
            }

            instance = (T)loadInstance;
            return instance;
        }

        protected virtual void OnCreated() { }
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class EditorDataObjectName : System.Attribute
    {
        private string name;

        public string GetName()
        {
            return name;
        }

        public EditorDataObjectName(string name)
        {
            this.name = name;
        }
    }
}
