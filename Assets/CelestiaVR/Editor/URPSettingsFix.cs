using UnityEngine;
using UnityEditor;

namespace CelestiaVR.Editor
{
    /// <summary>
    /// Fixes "UniversalRenderPipelineGlobalSettings is not at last version" build error.
    /// Deletes the outdated asset — Unity recreates it automatically on next load.
    /// </summary>
    public static class URPSettingsFix
    {
        private const string AssetPath =
            "Assets/Settings/Project Configuration/UniversalRenderPipelineGlobalSettings.asset";

        [MenuItem("CelestiaVR/Fix/Upgrade URP Settings")]
        public static void FixURPSettings()
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(AssetPath) != null)
            {
                AssetDatabase.DeleteAsset(AssetPath);
                AssetDatabase.Refresh();
                Debug.Log("[CelestiaVR] Deleted outdated URP Global Settings. Unity will recreate it on restart.");
            }
            else
            {
                Debug.Log("[CelestiaVR] URP Global Settings asset not found at expected path.");
            }

            EditorUtility.DisplayDialog(
                "URP Settings Fixed",
                "The outdated URP Global Settings asset has been deleted.\n\nUnity will recreate it automatically.\n\nPlease RESTART Unity Editor now.",
                "Restart Now");
        }
    }
}
