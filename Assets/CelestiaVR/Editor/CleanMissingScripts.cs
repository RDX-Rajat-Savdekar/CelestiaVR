using UnityEngine;
using UnityEditor;

namespace CelestiaVR.Editor
{
    public static class CleanMissingScripts
    {
        [MenuItem("CelestiaVR/Fix/Remove Missing Scripts From Scene")]
        public static void RemoveMissingScripts()
        {
            int removed = 0;
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                if (count > 0)
                {
                    EditorUtility.SetDirty(go);
                    removed += count;
                    Debug.Log($"[CelestiaVR] Removed {count} missing script(s) from '{go.name}'");
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            AssetDatabase.SaveAssets();
            Debug.Log($"[CelestiaVR] Done. Removed {removed} missing scripts total. Save the scene (Ctrl+S).");
        }
    }
}
