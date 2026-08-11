using System.IO;
using UnityEngine;
using Immersal.XR;

namespace DarsiNavigasi.Config
{
    /// <summary>
    /// Loads the Immersal VPS developer token from an uncommitted local config file
    /// (Assets/Resources/appconfig.json or StreamingAssets/appconfig.json)
    /// to prevent security token leakage in git repository and scene files (KI-01).
    /// </summary>
    public class ImmersalTokenLoader : MonoBehaviour
    {
        [System.Serializable]
        private class AppConfigData
        {
            public string immersalDeveloperToken = "";
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeTokenBeforeSceneLoad()
        {
            ApplyToken();
        }

        private void Awake()
        {
            ApplyToken();
        }

        public static bool ApplyToken()
        {
            string token = LoadTokenFromConfig();
            if (!string.IsNullOrEmpty(token))
            {
                if (ImmersalSDK.Instance != null)
                {
                    ImmersalSDK.Instance.developerToken = token;
                    Debug.Log("[ImmersalTokenLoader] Token developer ImmersalSDK berhasil dimuat dari konfigurasi lokal.");
                    return true;
                }
                else
                {
                    ImmersalSDK sdk = Object.FindObjectOfType<ImmersalSDK>();
                    if (sdk != null)
                    {
                        sdk.developerToken = token;
                        Debug.Log("[ImmersalTokenLoader] Token developer ImmersalSDK berhasil dipasang pada GameObject Scene.");
                        return true;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[ImmersalTokenLoader] PERINGATAN: Token Immersal tidak ditemukan di Resources/appconfig.json. Buat file dari appconfig.example.json.");
            }
            return false;
        }

        public static string LoadTokenFromConfig()
        {
            // 1. Coba muat dari Resources/appconfig.json
            TextAsset configAsset = Resources.Load<TextAsset>("appconfig");
            if (configAsset != null && !string.IsNullOrEmpty(configAsset.text))
            {
                AppConfigData data = JsonUtility.FromJson<AppConfigData>(configAsset.text);
                if (data != null && !string.IsNullOrEmpty(data.immersalDeveloperToken))
                {
                    return data.immersalDeveloperToken;
                }
            }

            // 2. Fallback: StreamingAssets/appconfig.json
            string streamingPath = Path.Combine(Application.streamingAssetsPath, "appconfig.json");
            if (File.Exists(streamingPath))
            {
                string json = File.ReadAllText(streamingPath);
                AppConfigData data = JsonUtility.FromJson<AppConfigData>(json);
                if (data != null && !string.IsNullOrEmpty(data.immersalDeveloperToken))
                {
                    return data.immersalDeveloperToken;
                }
            }

            return null;
        }
    }
}
