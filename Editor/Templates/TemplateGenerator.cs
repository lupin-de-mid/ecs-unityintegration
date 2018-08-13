﻿using System;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Leopotam.Ecs.UnityIntegration.Editor.Prototypes {
    /// <summary>
    /// Generates templates of ecs user classes.
    /// </summary>
    sealed class TemplateGenerator : ScriptableObject {
        const string Title = "LeoECS template generator";

        const string StartupTemplate = "Startup.cs.txt";
        const string InitSystemTemplate = "InitSystem.cs.txt";
        const string RunSystemTemplate = "RunSystem.cs.txt";

        [MenuItem ("Assets/Create/LeoECS/Create Startup template", false, 10)]
        static void CreateStartupTpl () {
            CreateAndRenameAsset (
                string.Format ("{0}/EcsStartup.cs", GetAssetPath ()),
                GetIcon (), name => CreateTemplateInternal (GetTemplateContent (StartupTemplate), name));
        }

        [MenuItem ("Assets/Create/LeoECS/Create InitSystem template", false, 11)]
        static void CreateInitSystemTpl () {
            CreateAndRenameAsset (
                string.Format ("{0}/EcsInitSystem.cs", GetAssetPath ()),
                GetIcon (), name => CreateTemplateInternal (GetTemplateContent (InitSystemTemplate), name));
        }

        [MenuItem ("Assets/Create/LeoECS/Create RunSystem template", false, 12)]
        static void CreateRunSystemTpl () {
            CreateAndRenameAsset (
                string.Format ("{0}/EcsRunSystem.cs", GetAssetPath ()),
                GetIcon (), name => CreateTemplateInternal (GetTemplateContent (RunSystemTemplate), name));
        }

        public static string CreateTemplate (string proto, string fileName) {
            if (string.IsNullOrEmpty (fileName)) {
                return "Invalid filename";
            }
            var ns = EditorSettings.projectGenerationRootNamespace;
            if (string.IsNullOrWhiteSpace (EditorSettings.projectGenerationRootNamespace)) {
                ns = "Client";
            }
            proto = proto.Replace ("#NS#", ns);
            proto = proto.Replace ("#SCRIPTNAME#", Path.GetFileNameWithoutExtension (fileName));
            try {
                File.WriteAllText (AssetDatabase.GenerateUniqueAssetPath (fileName), proto);
            } catch (Exception ex) {
                return ex.Message;
            }
            AssetDatabase.Refresh ();
            return null;
        }

        static void CreateTemplateInternal (string proto, string fileName) {
            var res = CreateTemplate (proto, fileName);
            if (res != null) {
                EditorUtility.DisplayDialog (Title, res, "Close");
            }
        }

        static string GetTemplateContent (string proto) {
            // hack: its only one way to get current editor script path. :(
            var pathHelper = ScriptableObject.CreateInstance<TemplateGenerator> ();
            var path = Path.GetDirectoryName (AssetDatabase.GetAssetPath (MonoScript.FromScriptableObject (pathHelper)));
            UnityEngine.Object.DestroyImmediate (pathHelper);
            try {
                return File.ReadAllText (Path.Combine (path, proto));
            } catch {
                return null;
            }
        }

        static string GetAssetPath () {
            var path = AssetDatabase.GetAssetPath (Selection.activeObject);
            if (!string.IsNullOrEmpty (path) && AssetDatabase.Contains (Selection.activeObject)) {
                if (!AssetDatabase.IsValidFolder (path)) {
                    path = Path.GetDirectoryName (path);
                }
            } else {
                path = "Assets";
            }
            return path;
        }

        static Texture2D GetIcon () {
            return EditorGUIUtility.IconContent ("cs Script Icon").image as Texture2D;
        }

        static void CreateAndRenameAsset (string fileName, Texture2D icon, Action<string> onSuccess) {
            var action = ScriptableObject.CreateInstance<CustomEndNameAction> ();
            action.Callback = onSuccess;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists (0, action, fileName, icon, null);
        }

        sealed class CustomEndNameAction : EndNameEditAction {
            public Action<string> Callback;

            public override void Action (int instanceId, string pathName, string resourceFile) {
                if (Callback != null) {
                    Callback (pathName);
                }
            }
        }
    }
}