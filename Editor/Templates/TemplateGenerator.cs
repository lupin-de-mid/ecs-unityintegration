using System;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Leopotam.Ecs.UnityIntegration.Editor.Prototypes {
    sealed class TemplateGenerator : ScriptableObject {
        const string Title = "LeoECS template generator";

        const string StartupTemplate = "Startup.cs.txt";
        const string InitSystemTemplate = "InitSystem.cs.txt";
        const string RunSystemTemplate = "RunSystem.cs.txt";

        static string GetProtoContent (string proto) {
            var pathHelper = ScriptableObject.CreateInstance<TemplateGenerator> ();
            var path = Path.GetDirectoryName (AssetDatabase.GetAssetPath (MonoScript.FromScriptableObject (pathHelper)));
            UnityEngine.Object.DestroyImmediate (pathHelper);
            try {
                return File.ReadAllText (Path.Combine (path, proto));
            } catch {
                return null;
            }
        }

        [MenuItem ("Assets/Create/LeoECS/Create Startup template", false, 10)]
        static void CreateStartupProto () {
            CreateAndRenameAsset (
                string.Format ("{0}/EcsStartup.cs", GetAssetPath ()),
                GetIcon (), name => CreateProto (GetProtoContent (StartupTemplate), name));
        }

        [MenuItem ("Assets/Create/LeoECS/Create InitSystem template", false, 11)]
        static void CreateInitSystemProto () {
            CreateAndRenameAsset (
                string.Format ("{0}/EcsInitSystem.cs", GetAssetPath ()),
                GetIcon (), name => CreateProto (GetProtoContent (InitSystemTemplate), name));
        }

        [MenuItem ("Assets/Create/LeoECS/Create RunSystem template", false, 12)]
        static void CreateRunSystemProto () {
            CreateAndRenameAsset (
                string.Format ("{0}/EcsRunSystem.cs", GetAssetPath ()),
                GetIcon (), name => CreateProto (GetProtoContent (RunSystemTemplate), name));
        }

        static void CreateProto (string proto, string fileName) {
            var res = CreatePrototype (proto, fileName);
            if (res != null) {
                EditorUtility.DisplayDialog (Title, res, "Close");
            }
        }

        public static string CreatePrototype (string proto, string fileName) {
            if (string.IsNullOrEmpty (fileName)) {
                return "Invalid filename";
            }
            proto = proto.Replace ("#SCRIPTNAME#", Path.GetFileNameWithoutExtension (fileName));
            try {
                File.WriteAllText (AssetDatabase.GenerateUniqueAssetPath (fileName), proto);
            } catch (Exception ex) {
                return ex.Message;
            }
            AssetDatabase.Refresh ();
            return null;
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