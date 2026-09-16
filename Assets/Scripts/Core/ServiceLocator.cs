using System.Collections.Generic;
using UnityEngine;


    public class ServiceLocator
    {
        private readonly Dictionary<string, IService> _services = new Dictionary<string, IService>();
        private static ServiceLocator _instance;

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceLocator();
                }

                return _instance;
            }
        }

        public bool TryGet<T>(out T service) where T : IService
        {
            string key = typeof(T).Name;
            if (_services.ContainsKey(key))
            {
                service = (T)_services[key];
                return true;
            }
            service = default;
            return false;
        }

        public bool Exist<T>() where T : IService
        {
            return _services.ContainsKey(typeof(T).Name);
        }

        public T Get<T>() where T : IService
        {
            string key = typeof(T).Name;

            if (!_services.ContainsKey(key))
            {
#if UNITY_EDITOR
                Debug.LogError($"{key} not registered with {GetType().Name}");
#endif
                throw new System.InvalidOperationException();
            }

            return (T)_services[key];
        }

        public void Register<T>(T service, bool replaceExistingService = false) where T : IService
        {
            string key = typeof(T).Name;

            if (_services.ContainsKey(key))
            {
                if (replaceExistingService)
                {
                    _services[key] = service;
                }
                else
                {
#if UNITY_EDITOR
                    Debug.Log($"Attempted to register service of type {key} which is already registered with the {GetType().Name}.");
#endif
                }

                return;
            }

            _services.Add(key, service);
        }

        public void Unregister<T>(T service) where T : IService
        {
            string key = typeof(T).Name;

            if (!_services.ContainsKey(key))
            {
#if UNITY_EDITOR
                Debug.Log($"Attempted to unregister service of type {key} which is not registered with the {GetType().Name}.");
#endif
                return;
            }

            _services.Remove(key);
        }
    }
