using UnityEngine;
using UnityEditor;

namespace CelestiaVR.Editor
{
    public static class CelestialObjectGenerator
    {
        private const string OutputPath = "Assets/CelestiaVR/Data/CelestialObjects";

        [MenuItem("CelestiaVR/Generate Celestial Object Assets")]
        public static void GenerateAll()
        {
            // Create folders using AssetDatabase so Unity tracks them properly
            if (!AssetDatabase.IsValidFolder("Assets/CelestiaVR"))
                AssetDatabase.CreateFolder("Assets", "CelestiaVR");
            if (!AssetDatabase.IsValidFolder("Assets/CelestiaVR/Data"))
                AssetDatabase.CreateFolder("Assets/CelestiaVR", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/CelestiaVR/Data/CelestialObjects"))
                AssetDatabase.CreateFolder("Assets/CelestiaVR/Data", "CelestialObjects");

            CreateAsset("Jupiter",
                CelestialObjectType.Planet,
                raHours: 5.30f, decDegrees: 22.5f,
                description: "The largest planet in our solar system, a gas giant with iconic cloud bands and the Great Red Spot.",
                distance: "~600 million km from Earth",
                fact: "Jupiter's Great Red Spot is a storm larger than Earth that has raged for over 350 years.",
                visualScale: 4f);

            CreateAsset("Saturn",
                CelestialObjectType.Planet,
                raHours: 22.85f, decDegrees: -13.5f,
                description: "The ringed jewel of the solar system. Saturn's rings are made of ice and rock fragments.",
                distance: "~1.4 billion km from Earth",
                fact: "Saturn's rings extend up to 282,000 km from the planet but are less than 1 km thick.",
                visualScale: 3.5f);

            CreateAsset("Mars",
                CelestialObjectType.Planet,
                raHours: 6.90f, decDegrees: 26.0f,
                description: "The Red Planet — Mars gets its color from iron oxide (rust) covering its surface.",
                distance: "~100 million km from Earth",
                fact: "Mars has the tallest volcano in the solar system: Olympus Mons, nearly 3x the height of Everest.",
                visualScale: 1.5f);

            CreateAsset("Venus",
                CelestialObjectType.Planet,
                raHours: 20.50f, decDegrees: -24.0f,
                description: "The brightest natural object in the night sky after the Moon. A world shrouded in toxic clouds.",
                distance: "~180 million km from Earth",
                fact: "A day on Venus is longer than its year — it rotates so slowly that the sun rises in the west.",
                visualScale: 1.8f);

            CreateAsset("Mercury",
                CelestialObjectType.Planet,
                raHours: 19.80f, decDegrees: -22.0f,
                description: "The smallest planet and closest to the Sun. Rarely visible, hugging the twilight horizon.",
                distance: "~180 million km from Earth",
                fact: "Despite being closest to the Sun, Mercury is not the hottest planet — Venus is, due to its thick atmosphere.",
                visualScale: 0.8f);

            CreateAsset("Sirius",
                CelestialObjectType.Star,
                raHours: 6.7525f, decDegrees: -16.7161f,
                description: "Alpha Canis Majoris — the brightest star in the night sky, part of the constellation Canis Major.",
                distance: "8.6 light-years",
                fact: "Sirius is actually a binary system. Its companion, Sirius B, is a white dwarf about the size of Earth.",
                visualScale: 1.2f);

            CreateAsset("Betelgeuse",
                CelestialObjectType.Star,
                raHours: 5.9194f, decDegrees: 7.4070f,
                description: "Alpha Orionis — a red supergiant marking Orion's right shoulder. One of the largest known stars.",
                distance: "~700 light-years",
                fact: "If Betelgeuse replaced our Sun, it would extend past the orbit of Jupiter.",
                visualScale: 2.0f);

            CreateAsset("Orion Nebula",
                CelestialObjectType.Nebula,
                raHours: 5.5833f, decDegrees: -5.3911f,
                description: "Messier 42 — a stellar nursery 1,344 light-years away, visible to the naked eye below Orion's belt.",
                distance: "1,344 light-years",
                fact: "The Orion Nebula contains over 700 stars in various stages of formation.",
                visualScale: 5.0f);

            CreateAsset("Andromeda Galaxy",
                CelestialObjectType.Galaxy,
                raHours: 0.7122f, decDegrees: 41.2689f,
                description: "Messier 31 — our nearest large galactic neighbor. Visible to the naked eye as a faint smudge.",
                distance: "2.537 million light-years",
                fact: "Andromeda is on a collision course with the Milky Way — they will merge in about 4.5 billion years.",
                visualScale: 8.0f);

            CreateAsset("Pleiades",
                CelestialObjectType.StarCluster,
                raHours: 3.7908f, decDegrees: 24.1050f,
                description: "Messier 45 — the Seven Sisters, an open star cluster in Taurus, one of the closest to Earth.",
                distance: "444 light-years",
                fact: "The Pleiades appear in the mythologies of cultures worldwide — from Greek legend to Aboriginal Australian stories.",
                visualScale: 4.0f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CelestiaVR] Generated 10 CelestialObjectData assets in " + OutputPath);

            // Auto-wire after generation
            WirePlacer();
        }

        [MenuItem("CelestiaVR/Wire Celestial Object Placer")]
        public static void WirePlacer()
        {
            var placer = Object.FindFirstObjectByType<CelestialObjectPlacer>();
            if (placer == null)
            {
                Debug.LogError("[CelestiaVR] No CelestialObjectPlacer found in scene. Add it first.");
                return;
            }

            // Load all CelestialObjectData assets from the output folder
            var guids = AssetDatabase.FindAssets("t:CelestialObjectData", new[] { OutputPath });
            if (guids.Length == 0)
            {
                Debug.LogError("[CelestiaVR] No CelestialObjectData assets found. Run Generate first.");
                return;
            }

            var entries = new System.Collections.Generic.List<CelestialObjectPlacer.CelestialEntry>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<CelestialObjectData>(path);
                if (data != null)
                    entries.Add(new CelestialObjectPlacer.CelestialEntry { data = data });
            }

            var so = new SerializedObject(placer);
            var listProp = so.FindProperty("celestialObjects");
            listProp.arraySize = entries.Count;
            for (int i = 0; i < entries.Count; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("data").objectReferenceValue = entries[i].data;
                element.FindPropertyRelative("prefabOverride").objectReferenceValue = null;
            }
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(placer);
            Debug.Log($"[CelestiaVR] Wired {entries.Count} celestial objects into CelestialObjectPlacer.");
        }

        private static void CreateAsset(
            string objectName,
            CelestialObjectType type,
            float raHours,
            float decDegrees,
            string description,
            string distance,
            string fact,
            float visualScale)
        {
            string assetPath = $"{OutputPath}/{objectName}.asset";

            // Don't overwrite if already exists
            var existing = AssetDatabase.LoadAssetAtPath<CelestialObjectData>(assetPath);
            if (existing != null)
            {
                Debug.Log($"[CelestiaVR] Skipping {objectName} — asset already exists.");
                return;
            }

            var data = ScriptableObject.CreateInstance<CelestialObjectData>();
            data.objectName = objectName;
            data.objectType = type;
            data.rightAscensionHours = raHours;
            data.declinationDegrees = decDegrees;
            data.description = description;
            data.distanceFromEarth = distance;
            data.objectFact = fact;
            data.visualScale = visualScale;

            AssetDatabase.CreateAsset(data, assetPath);
        }
    }
}
