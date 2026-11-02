using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RainRemoverMod
{
    [BepInPlugin("com.gorillatag.rainremover", "RainRemoverMod", "1.0.2")]
    public class Plugin : BaseUnityPlugin
    {
        private const string ReleaseApi = "https://api.github.com/repos/kaidevelopment/RainRemoverMod/releases/latest";

        private static readonly ManualLogSource Log =
            BepInEx.Logging.Logger.CreateLogSource("RainRemoverMod");

        private static readonly string[] Keywords =
            { "rain", "drizzle", "precip", "weather", "storm" };

        private readonly List<GameObject> _rainObjects = new List<GameObject>();
        private bool _rainOn;
        private volatile string _pendingDll;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Application.quitting += OnQuit;
            ScanAndDisable();
            Task.Run(CheckForUpdate);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Application.quitting -= OnQuit;
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

        private void OnGUI()
        {
            var rainStyle = new GUIStyle(GUI.skin.label);
            rainStyle.fontSize = 22;
            rainStyle.fontStyle = FontStyle.Bold;
            rainStyle.normal.textColor = _rainOn ? Color.green : Color.red;

            string rainText = "Rain: " + (_rainOn ? "On" : "Off");
            var rainSize = rainStyle.CalcSize(new GUIContent(rainText));
            GUI.Label(new Rect((Screen.width - rainSize.x) * 0.5f, 10f, rainSize.x, rainSize.y), rainText, rainStyle);

            var creditStyle = new GUIStyle(GUI.skin.label);
            creditStyle.fontSize = 14;
            creditStyle.normal.textColor = new Color(1f, 0.71f, 0.80f);

            string creditText = "Made by AxolotlGT";
            var creditSize = creditStyle.CalcSize(new GUIContent(creditText));
            GUI.Label(new Rect(Screen.width - creditSize.x - 8f, Screen.height - creditSize.y - 8f, creditSize.x, creditSize.y), creditText, creditStyle);
        }

        private void OnQuit()
        {
            if (_pendingDll == null || !File.Exists(_pendingDll)) return;

            var dllPath = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(dllPath);
            var script = Path.Combine(dir, "apply_update.ps1");

            File.WriteAllText(script, string.Format(
                "Start-Sleep 2{0}" +
                "Copy-Item '{1}' '{2}' -Force{0}" +
                "Remove-Item '{1}'{0}" +
                "Remove-Item '{3}'{0}",
                Environment.NewLine, _pendingDll, dllPath, script));

            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = string.Format("-NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File \"{0}\"", script),
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        private void CheckForUpdate()
        {
            try
            {
                var current = Assembly.GetExecutingAssembly().GetName().Version;
                using (var wc = new WebClient())
                {
                    wc.Headers["User-Agent"] = "RainRemoverMod";
                    var json = wc.DownloadString(ReleaseApi);

                    var tagMatch = Regex.Match(json, "\"tag_name\":\\s*\"v([\\d.]+)\"");
                    if (!tagMatch.Success) return;

                    if (!Version.TryParse(tagMatch.Groups[1].Value, out var latest)) return;
                    if (latest <= current) return;

                    var urlMatch = Regex.Match(json, "\"browser_download_url\":\\s*\"([^\"]+\\.dll)\"");
                    if (!urlMatch.Success) return;

                    var dllPath = Assembly.GetExecutingAssembly().Location;
                    var pending = dllPath + ".pending";

                    wc.DownloadFile(urlMatch.Groups[1].Value, pending);
                    _pendingDll = pending;
                    Log.LogInfo(string.Format("[RainRemover] Update v{0} downloaded. Restart to apply.", latest));
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(string.Format("[RainRemover] Update check failed: {0}", ex.Message));
            }
        }

        private void SetRain(bool enabled)
        {
            _rainOn = enabled;
            foreach (var go in _rainObjects)
                if (go != null)
                    go.SetActive(enabled);
            Log.LogInfo(string.Format("[RainRemover] Rain {0}", enabled ? "on" : "off"));
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

            foreach (var ps in UnityEngine.Object.FindObjectsOfType<ParticleSystem>())
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
            Log.LogInfo(string.Format("[RainRemover] Found {0} rain object(s). Press R to toggle.", _rainObjects.Count));
        }
    }
}
