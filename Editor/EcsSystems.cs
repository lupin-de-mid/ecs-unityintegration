// ----------------------------------------------------------------------------
// The MIT License
// Unity integration https://github.com/Leopotam/ecs-unityintegration
// for ECS framework https://github.com/Leopotam/ecs
// Copyright (c) 2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Leopotam.Ecs.UnityIntegration.Editor {
    [CustomEditor (typeof (EcsSystemsObserver))]
    sealed class EcsSystemsObserverInspector : UnityEditor.Editor {
        static IEcsPreInitSystem[] _preInitList = new IEcsPreInitSystem[32];
        static Stack<IEcsInitSystem[]> _initList = new Stack<IEcsInitSystem[]> (8);
        static Stack<IEcsRunSystem[]> _runList = new Stack<IEcsRunSystem[]> (8);

        public override void OnInspectorGUI () {
            var savedState = GUI.enabled;
            GUI.enabled = true;
            var observer = target as EcsSystemsObserver;
            var systems = observer.GetSystems ();
            int count;

            count = systems.GetPreInitSystems (ref _preInitList);
            if (count > 0) {
                GUILayout.BeginVertical (GUI.skin.box);
                EditorGUILayout.LabelField ("PreInitialize systems", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (var i = 0; i < count; i++) {
                    EditorGUILayout.LabelField (_preInitList[i].GetType ().Name);
                    _preInitList[i] = null;
                }
                EditorGUI.indentLevel--;
                GUILayout.EndVertical ();
            }

            GUILayout.BeginVertical (GUI.skin.box);
            EditorGUILayout.LabelField ("Initialize systems", EditorStyles.boldLabel);
            OnInitSystemsGUI (systems);
            GUILayout.EndVertical ();

            GUILayout.BeginVertical (GUI.skin.box);
            EditorGUILayout.LabelField ("Run systems", EditorStyles.boldLabel);
            OnRunSystemsGUI (systems);
            GUILayout.EndVertical ();

            GUI.enabled = savedState;
        }

        void OnInitSystemsGUI (EcsSystems systems) {
            IEcsInitSystem[] initList = null;
            if (_initList.Count != 0) {
                initList = _initList.Pop ();
            }
            var count = systems.GetInitSystems (ref initList);
            if (count > 0) {
                EditorGUI.indentLevel++;
                for (var i = 0; i < count; i++) {
                    EditorGUILayout.LabelField (initList[i].GetType ().Name);
                    var asSystems = initList[i] as EcsSystems;
                    if (asSystems != null) {
                        OnInitSystemsGUI (asSystems);
                    }
                    initList[i] = null;
                }
                EditorGUI.indentLevel--;
            }
            _initList.Push (initList);
        }

        void OnRunSystemsGUI (EcsSystems systems) {
            IEcsRunSystem[] runList = null;
            if (_runList.Count != 0) {
                runList = _runList.Pop ();
            }
            var count = systems.GetRunSystems (ref runList);
            if (count > 0) {
                EditorGUI.indentLevel++;
                for (var i = 0; i < count; i++) {
                    bool enabled = true;
                    if (systems.DisabledInDebugSystems != null) {
                        systems.DisabledInDebugSystems[i] = !EditorGUILayout.Toggle (runList[i].GetType ().Name, !systems.DisabledInDebugSystems[i]);
                        enabled = !systems.DisabledInDebugSystems[i];
                    } else {
                        EditorGUILayout.LabelField (runList[i].GetType ().Name);
                    }
                    if (enabled) {
                        var asSystems = runList[i] as EcsSystems;
                        if (asSystems != null) {
                            OnRunSystemsGUI (asSystems);
                        }
                    }
                    runList[i] = null;
                }
                EditorGUI.indentLevel--;
            }
            _runList.Push (runList);
        }
    }
}