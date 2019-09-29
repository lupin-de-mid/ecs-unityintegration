// ----------------------------------------------------------------------------
// The MIT License
// Unity integration https://github.com/Leopotam/ecs-unityintegration
// for ECS framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2019 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leopotam.Ecs.UnityIntegration {
    public static class EditorHelpers {
        public static string GetCleanGenericTypeName (Type type) {
            if (!type.IsGenericType) {
                return type.Name;
            }
            var constraints = "";
            foreach (var constr in type.GetGenericArguments ()) {
                constraints += constraints.Length > 0 ? string.Format (", {0}", GetCleanGenericTypeName (constr)) : constr.Name;
            }
            return string.Format ("{0}<{1}>", type.Name.Substring (0, type.Name.LastIndexOf ("`")), constraints);
        }
    }

    public sealed class EcsEntityObserver : MonoBehaviour {
        public EcsWorld World;
        public EcsEntity Entity;
    }

    public sealed class EcsSystemsObserver : MonoBehaviour, IEcsSystemsDebugListener {
        EcsSystems _systems;

        public static GameObject Create (EcsSystems systems) {
            if (systems == null) { throw new ArgumentNullException ("systems"); }
            var go = new GameObject (systems.Name != null ? string.Format ("[ECS-SYSTEMS {0}]", systems.Name) : "[ECS-SYSTEMS]");
            DontDestroyOnLoad (go);
            go.hideFlags = HideFlags.NotEditable;
            var observer = go.AddComponent<EcsSystemsObserver> ();
            observer._systems = systems;
            systems.AddDebugListener (observer);
            return go;
        }

        public EcsSystems GetSystems () {
            return _systems;
        }

        void OnDestroy () {
            if (_systems != null) {
                _systems.RemoveDebugListener (this);
                _systems = null;
            }
        }

        void IEcsSystemsDebugListener.OnSystemsDestroyed () {
            // for immediate unregistering this MonoBehaviour from ECS.
            OnDestroy ();
            // for delayed destroying GameObject.
            Destroy (gameObject);
        }
    }

    public sealed class EcsWorldObserver : MonoBehaviour, IEcsWorldDebugListener {
        EcsWorld _world;
        readonly Dictionary<int, GameObject> _entities = new Dictionary<int, GameObject> (1024);
        static object[] _componentsCache = new object[32];

        public static GameObject Create (EcsWorld world, string name = null) {
            if (world == null) { throw new ArgumentNullException ("world"); }
            var go = new GameObject (name != null ? string.Format ("[ECS-WORLD {0}]", name) : "[ECS-WORLD]");
            DontDestroyOnLoad (go);
            go.hideFlags = HideFlags.NotEditable;
            var observer = go.AddComponent<EcsWorldObserver> ();
            observer._world = world;
            world.AddDebugListener (observer);
            return go;
        }

        public EcsWorldStats GetStats () {
            return _world.GetStats ();
        }

        void IEcsWorldDebugListener.OnEntityCreated (in EcsEntity entity) {
            Debug.LogFormat ("entity-created {0}", entity);
            GameObject go;
            if (!_entities.TryGetValue (entity.GetInternalId (), out go)) {
                go = new GameObject ();
                go.transform.SetParent (transform, false);
                go.hideFlags = HideFlags.NotEditable;
                var unityEntity = go.AddComponent<EcsEntityObserver> ();
                unityEntity.World = _world;
                unityEntity.Entity = entity;
                _entities[entity.GetInternalId ()] = go;
                UpdateEntityName (entity, false);
            } else {
                // need to update cached entity generation.
                go.GetComponent<EcsEntityObserver> ().Entity = entity;
            }
            go.SetActive (true);
        }

        void IEcsWorldDebugListener.OnEntityDestroyed (in EcsEntity entity) {
            Debug.LogFormat ("entity-destroyed {0}", entity);
            GameObject go;
            if (!_entities.TryGetValue (entity.GetInternalId (), out go)) {
                throw new Exception ("Unity visualization not exists, looks like a bug");
            }
            UpdateEntityName (entity, false);
            go.SetActive (false);
        }

        void IEcsWorldDebugListener.OnComponentAdded (in EcsEntity entity, object component) {
            Debug.LogFormat ("component-added {0} / {1}", entity, component);
            UpdateEntityName (entity, true);
        }

        void IEcsWorldDebugListener.OnComponentRemoved (in EcsEntity entity, object component) {
            Debug.LogFormat ("component-removed {0} / {1}", entity, component);
            UpdateEntityName (entity, true);
        }

        void IEcsWorldDebugListener.OnWorldDestroyed () {
            Debug.Log ("world-destroyed");
            // for immediate unregistering this MonoBehaviour from ECS.
            OnDestroy ();
            // for delayed destroying GameObject.
            Destroy (gameObject);
        }

        void UpdateEntityName (in EcsEntity entity, bool requestComponents) {
            var entityName = entity.GetInternalId ().ToString ("D8");
            if (entity.IsAlive () && requestComponents) {
                var count = entity.GetComponents (ref _componentsCache);
                for (var i = 0; i < count; i++) {
                    entityName = string.Format ("{0}:{1}", entityName, EditorHelpers.GetCleanGenericTypeName (_componentsCache[i].GetType ()));
                    _componentsCache[i] = null;
                }
            }
            _entities[entity.GetInternalId ()].name = entityName;
        }

        void OnDestroy () {
            if (_world != null) {
                _world.RemoveDebugListener (this);
                _world = null;
            }
        }
    }
}
#endif