using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RainRemoverMod
{
    [BepInPlugin("com.gorillatag.rainremover", "RainRemoverMod", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly ManualLogSource Log =
            BepInEx.Logging.Logger.CreateLogSource("RainRemoverMod");

        private static readonly string[] Keywords =
            { "rain", "drizzle", "precip", "weather", "storm" };

        private readonly List<GameObject> _rainObjects = new List<GameObject>();
        private bool _rainOn;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            ScanAndDisable();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ScanAndDisable();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
                SetRain(!_rainOn);
        }

        private void SetRain(bool enabled)
        {
            _rainOn = enabled;
            foreach (var go in _rainObjects)
            {
                if (go != null)
                    go.SetActive(enabled);
            }
            Log.LogInfo($"[RainRemover] Rain {(enabled ? "on" : "off")}");
        }

        private void ScanAndDisable()
        {
            _rainObjects.Clear();

            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go == null) continue;
                string name = go.name.ToLowerInvariant();
                foreach (var kw in Keywords)
                {
                    if (name.Contains(kw))
                    {
                        _rainObjects.Add(go);
                        go.SetActive(false);
                        break;
                    }
                }
            }

            foreach (var ps in Object.FindObjectsOfType<ParticleSystem>())
            {
                if (ps == null || _rainObjects.Contains(ps.gameObject)) continue;
                string name = ps.gameObject.name.ToLowerInvariant();
                bool match = false;
                foreach (var kw in Keywords)
                    if (name.Contains(kw)) { match = true; break; }
                if (!match && ps.transform.parent != null)
                    foreach (var kw in Keywords)
                        if (ps.transform.parent.name.ToLowerInvariant().Contains(kw)) { match = true; break; }
                if (match)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.gameObject.SetActive(false);
                    _rainObjects.Add(ps.gameObject);
                }
            }

            _rainOn = false;
            Log.LogInfo($"[RainRemover] Found {_rainObjects.Count} rain object(s). Press R to toggle.");
        }
    }
}
