using System;
using TiltBrush;
using UnityEngine;
using System.Collections.Generic;
using Environment = TiltBrush.Environment;

namespace EternityEngine
{
    public class EnvironmentCatalog : SingletonUpdateWhileEnabled<EnvironmentCatalog>
    {
        public event Action EnvironmentsChanged;
        public Material m_SkyboxMaterial;
        public TiltBrush.Environment m_DefaultEnvironment;
        bool m_IsLoading;
        Dictionary<Guid, Environment> m_GuidToEnvironment;

        public IEnumerable<Environment> AllEnvironments
        {
            get { return m_GuidToEnvironment.Values; }
        }
        public Environment DefaultEnvironment
        {
            get { return m_DefaultEnvironment; }
        }

        public override void Awake ()
        {
            base.Awake ();
            m_GuidToEnvironment = new Dictionary<Guid, Environment>();
        }

        public bool IsLoading { get { return m_IsLoading; } }

        public void BeginReload()
        {
            var newEnvironments = new List<Environment>();
            LoadEnvironmentsInManifest(newEnvironments);
            newEnvironments.Add(DefaultEnvironment);

            m_GuidToEnvironment.Clear();
            foreach (var env in newEnvironments)
            {
                Environment tmp;
                if (m_GuidToEnvironment.TryGetValue(env.m_Guid, out tmp) && tmp != env)
                {
                    Debug.LogErrorFormat("Guid collision: {0}, {1}", tmp, env);
                    continue;
                }
                m_GuidToEnvironment[env.m_Guid] = env;
            }

            Resources.UnloadUnusedAssets();
            m_IsLoading = true;
        }

        public Environment GetEnvironment(Guid guid)
        {
            try
            {
                return m_GuidToEnvironment[guid];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public override void DoUpdate ()
        {
            if (m_IsLoading)
            {
                m_IsLoading = false;
                Resources.UnloadUnusedAssets();
                if (EnvironmentsChanged != null)
                {
                    EnvironmentsChanged();
                }
            }
        }

        static void LoadEnvironmentsInManifest(List<Environment> output)
        {
            var manifest = App.Instance.m_Manifest;
            foreach (var asset in manifest.Environments)
            {
                if (asset != null)
                {
                    output.Add(asset);
                }
            }
        }
    }
} // namespace TiltBrush
