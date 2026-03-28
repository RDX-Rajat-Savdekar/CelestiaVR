using UnityEngine;
using UnityEditor;

namespace CelestiaVR.Editor
{
    /// <summary>
    /// Generates ConstellationData ScriptableObject assets with catalog-accurate J2000 coordinates.
    /// Source: HYG Database v3.8 (astronexus/HYG-Database, Hipparcos-derived)
    ///         Stellarium modern_st Sky & Telescope constellation line figures
    /// RA in decimal hours, Dec in decimal degrees.
    /// </summary>
    public static class ConstellationGenerator
    {
        private const string OutPath    = "Assets/CelestiaVR/Data/Constellations";
        private const string ObjectPath = "Assets/CelestiaVR/Data/CelestialObjects";

        [MenuItem("CelestiaVR/Generate Constellation Assets")]
        public static void GenerateAll()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CelestiaVR/Data"))
                AssetDatabase.CreateFolder("Assets/CelestiaVR", "Data");
            if (!AssetDatabase.IsValidFolder(OutPath))
                AssetDatabase.CreateFolder("Assets/CelestiaVR/Data", "Constellations");

            CreateOrion();
            CreateCanisMajor();
            CreateTaurus();
            CreateGemini();
            CreateAndromeda();
            CreateAquarius();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CelestiaVR] Constellation assets regenerated with HYG v3.8 catalog data.");
        }

        // ─── ORION (9 stars) ──────────────────────────────────────────────────────
        // Source: HYG v3.8 / Stellarium modern_st SnT figure
        // Shoulders: Betelgeuse, Bellatrix
        // Head:      Meissa (Lambda Ori)
        // Belt:      Mintaka, Alnilam, Alnitak
        // Feet:      Rigel, Saiph
        // Leg joint: Eta Ori (bends right leg naturally)
        static void CreateOrion()
        {
            var d = MakeOrGet("Orion");
            d.constellationName = "Orion";
            d.mythology = "The great hunter of Greek mythology, placed in the sky by Zeus. " +
                          "Orion is one of the most recognizable constellations, visible worldwide.";
            d.stars = new[]
            {
                S("Betelgeuse", 5.919529f,   7.407063f),  // [0] alf Ori  HIP 27989
                S("Bellatrix",  5.418851f,   6.349702f),  // [1] gam Ori  HIP 25336
                S("Meissa",     5.585633f,   9.934158f),  // [2] lam Ori  HIP 26207  (head)
                S("Mintaka",    5.533445f,  -0.299092f),  // [3] del Ori  HIP 25930  (belt W)
                S("Alnilam",    5.603559f,  -1.201920f),  // [4] eps Ori  HIP 26311  (belt C)
                S("Alnitak",    5.679313f,  -1.942572f),  // [5] zet Ori  HIP 26727  (belt E)
                S("Saiph",      5.795941f,  -9.669605f),  // [6] kap Ori  HIP 27366  (right foot)
                S("Rigel",      5.242298f,  -8.201640f),  // [7] bet Ori  HIP 24436  (left foot)
                S("Eta Ori",    5.407950f,  -2.397100f),  // [8] eta Ori  HIP 25281  (leg joint)
            };
            d.connectionIndices = new[]
            {
                2, 0,  // Meissa     – Betelgeuse  (head→shoulder)
                2, 1,  // Meissa     – Bellatrix   (head→shoulder)
                0, 1,  // Betelgeuse – Bellatrix   (shoulders)
                0, 5,  // Betelgeuse – Alnitak     (left arm to belt)
                3, 4,  // Mintaka    – Alnilam     (belt)
                4, 5,  // Alnilam    – Alnitak     (belt)
                5, 6,  // Alnitak    – Saiph       (right leg)
                3, 8,  // Mintaka    – Eta Ori     (left leg joint)
                8, 7,  // Eta Ori    – Rigel       (left leg foot)
                1, 3,  // Bellatrix  – Mintaka     (shoulder to belt)
            };
            d.lineColor      = new Color(0.55f, 0.75f, 1.00f, 0.55f);
            d.highlightColor = new Color(0.70f, 0.90f, 1.00f, 1.00f);
            d.lineWidth      = 0.12f;
            d.highlightWidth = 0.22f;
            d.linkedObjects  = LoadObjects("Betelgeuse", "Orion Nebula");
            Save(d, "Orion");
        }

        // ─── CANIS MAJOR (10 stars) ──────────────────────────────────────────────
        // Source: HYG v3.8 / Stellarium modern_st SnT figure
        static void CreateCanisMajor()
        {
            var d = MakeOrGet("CanisMajor");
            d.constellationName = "Canis Major";
            d.mythology = "Orion's faithful hunting dog, home to Sirius — the brightest star in the night sky.";
            d.stars = new[]
            {
                S("Sirius",    6.752481f, -16.716116f),  // [0] alf CMa  HIP 32349
                S("Mirzam",    6.378329f, -17.955918f),  // [1] bet CMa  HIP 30324
                S("Muliphein", 7.062637f, -15.633286f),  // [2] gam CMa  HIP 34045
                S("Wezen",     7.139857f, -26.393200f),  // [3] del CMa  HIP 34444
                S("Adhara",    6.977097f, -28.972084f),  // [4] eps CMa  HIP 33579
                S("Aludra",    7.401584f, -29.303104f),  // [5] eta CMa  HIP 35904
                S("Nu2 CMa",   6.611400f, -19.255900f),  // [6] nu2 CMa  HIP 31592
                S("Omi1 CMa",  6.902210f, -24.184200f),  // [7] omi1 CMa HIP 33152
                S("Iota CMa",  6.935620f, -17.054200f),  // [8] iot CMa  HIP 33347
                S("Theta CMa", 6.903170f, -12.038600f),  // [9] tht CMa  HIP 33160
            };
            d.connectionIndices = new[]
            {
                1, 0,   // Mirzam     – Sirius
                0, 3,   // Sirius     – Wezen
                3, 4,   // Wezen      – Adhara
                3, 5,   // Wezen      – Aludra
                1, 6,   // Mirzam     – Nu2 CMa
                6, 7,   // Nu2 CMa    – Omi1 CMa
                7, 4,   // Omi1 CMa   – Adhara
                0, 8,   // Sirius     – Iota CMa
                8, 2,   // Iota CMa   – Muliphein
                2, 9,   // Muliphein  – Theta CMa
            };
            d.lineColor      = new Color(0.55f, 0.75f, 1.00f, 0.55f);
            d.highlightColor = new Color(0.70f, 0.90f, 1.00f, 1.00f);
            d.lineWidth      = 0.12f;
            d.highlightWidth = 0.22f;
            d.linkedObjects  = LoadObjects("Sirius");
            Save(d, "CanisMajor");
        }

        // ─── TAURUS (11 stars — includes the Hyades V) ───────────────────────────
        static void CreateTaurus()
        {
            var d = MakeOrGet("Taurus");
            d.constellationName = "Taurus";
            d.mythology = "The bull, one of the oldest constellations — visible from Griffith in December. " +
                          "Contains the Pleiades star cluster and the Hyades V-shape.";
            d.stars = new[]
            {
                S("Aldebaran",   4.598677f,  16.509301f),  // [0]  alf Tau  HIP 21421
                S("Elnath",      5.438198f,  28.607450f),  // [1]  bet Tau  HIP 25428
                S("Zeta Tau",    5.627413f,  21.142549f),  // [2]  zet Tau  HIP 26451
                S("Theta1 Tau",  4.476248f,  15.962181f),  // [3]  the1 Tau HIP 20885  (Hyades)
                S("Gamma Tau",   4.329890f,  15.627600f),  // [4]  gam Tau  HIP 20205  (Hyades)
                S("Delta1 Tau",  4.382250f,  17.542500f),  // [5]  del1 Tau HIP 20455  (Hyades)
                S("Ain",         4.476943f,  19.180431f),  // [6]  eps Tau  HIP 20889  (Hyades)
                S("Lambda Tau",  4.011338f,  12.490347f),  // [7]  lam Tau  HIP 18724
                S("Xi Tau",      3.452820f,   9.732680f),  // [8]  xi  Tau  HIP 16083
                S("Omicron Tau", 3.413554f,   9.028870f),  // [9]  omi Tau  HIP 15900
                S("Nu Tau",      4.052610f,   5.989200f),  // [10] nu  Tau  HIP 18907
            };
            d.connectionIndices = new[]
            {
                2, 0,   // Zeta Tau   – Aldebaran   (horn tip to eye)
                0, 3,   // Aldebaran  – Theta1 Tau  (Hyades)
                3, 4,   // Theta1 Tau – Gamma Tau
                4, 5,   // Gamma Tau  – Delta1 Tau
                5, 6,   // Delta1 Tau – Ain
                6, 1,   // Ain        – Elnath       (horn)
                4, 7,   // Gamma Tau  – Lambda Tau
                7, 8,   // Lambda Tau – Xi Tau
                8, 9,   // Xi Tau     – Omicron Tau
                9, 10,  // Omicron    – Nu Tau
            };
            d.lineColor      = new Color(0.55f, 0.75f, 1.00f, 0.55f);
            d.highlightColor = new Color(0.70f, 0.90f, 1.00f, 1.00f);
            d.lineWidth      = 0.12f;
            d.highlightWidth = 0.22f;
            d.linkedObjects  = LoadObjects("Pleiades");
            Save(d, "Taurus");
        }

        // ─── GEMINI (16 stars) ───────────────────────────────────────────────────
        static void CreateGemini()
        {
            var d = MakeOrGet("Gemini");
            d.constellationName = "Gemini";
            d.mythology = "The twin brothers Castor and Pollux of Greek mythology. " +
                          "Their heads are marked by the two brightest stars, always rising and setting together.";
            d.stars = new[]
            {
                S("Castor",      7.576634f,  31.888276f),  // [0]  alf Gem  HIP 36850
                S("Pollux",      7.755277f,  28.026199f),  // [1]  bet Gem  HIP 37826
                S("Alhena",      6.628528f,  16.399252f),  // [2]  gam Gem  HIP 31681
                S("Wasat",       7.335383f,  21.982320f),  // [3]  del Gem  HIP 35550
                S("Mebsuda",     6.732202f,  25.131124f),  // [4]  eps Gem  HIP 32246
                S("Mekbuda",     7.068481f,  20.570297f),  // [5]  zet Gem  HIP 34088
                S("Tejat",       6.382673f,  22.513586f),  // [6]  mu  Gem  HIP 30343
                S("Propus",      6.247961f,  22.506799f),  // [7]  eta Gem  HIP 29655
                S("Xi Gem",      6.754820f,  12.895600f),  // [8]  xi  Gem  HIP 32362
                S("Lambda Gem",  7.301550f,  16.540400f),  // [9]  lam Gem  HIP 35350
                S("Upsilon Gem", 7.598710f,  26.895700f),  // [10] ups Gem  HIP 36962
                S("Kappa Gem",   7.740790f,  24.398000f),  // [11] kap Gem  HIP 37740
                S("Iota Gem",    7.428780f,  27.798100f),  // [12] iot Gem  HIP 36046
                S("Tau Gem",     7.185660f,  30.245200f),  // [13] tau Gem  HIP 34693
                S("Theta Gem",   6.879820f,  33.961300f),  // [14] tht Gem  HIP 33018
                S("Nu Gem",      6.482720f,  20.212100f),  // [15] nu  Gem  HIP 30883
            };
            d.connectionIndices = new[]
            {
                 8,  9,  // Xi Gem     – Lambda Gem
                 9,  3,  // Lambda     – Wasat
                 3,  5,  // Wasat      – Mekbuda
                 5,  2,  // Mekbuda    – Alhena
                 5,  4,  // Mekbuda    – Mebsuda
                 4, 15,  // Mebsuda    – Nu Gem
                 4,  6,  // Mebsuda    – Tejat
                 6,  7,  // Tejat      – Propus
                 3, 10,  // Wasat      – Upsilon Gem
                10,  1,  // Upsilon    – Pollux
                10, 11,  // Upsilon    – Kappa Gem
                10, 12,  // Upsilon    – Iota Gem
                12, 13,  // Iota       – Tau Gem
                13,  0,  // Tau Gem    – Castor
                13, 14,  // Tau Gem    – Theta Gem
            };
            d.lineColor      = new Color(0.55f, 0.75f, 1.00f, 0.55f);
            d.highlightColor = new Color(0.70f, 0.90f, 1.00f, 1.00f);
            d.lineWidth      = 0.12f;
            d.highlightWidth = 0.22f;
            d.linkedObjects  = LoadObjects("Mars");
            Save(d, "Gemini");
        }

        // ─── ANDROMEDA (8 stars) ─────────────────────────────────────────────────
        static void CreateAndromeda()
        {
            var d = MakeOrGet("Andromeda");
            d.constellationName = "Andromeda";
            d.mythology = "Princess Andromeda chained to a rock, rescued by Perseus. " +
                          "Her constellation contains the Andromeda Galaxy — 2.5 million light-years away.";
            d.stars = new[]
            {
                S("Alpheratz", 0.139791f,  29.090432f),  // [0] alf And  HIP 677
                S("Delta And", 0.655462f,  30.861024f),  // [1] del And  HIP 3092
                S("Mirach",    1.162194f,  35.620558f),  // [2] bet And  HIP 5447
                S("Almach",    2.064984f,  42.329725f),  // [3] gam And  HIP 9640
                S("Pi And",    0.614680f,  33.719344f),  // [4] pi  And  HIP 2912
                S("Mu And",    0.945885f,  38.499345f),  // [5] mu  And  HIP 4436
                S("Nu And",    0.830230f,  41.078900f),  // [6] nu  And  HIP 3881
                S("Sigma And", 0.305463f,  36.785224f),  // [7] sig And  HIP 1473
            };
            d.connectionIndices = new[]
            {
                0, 1,  // Alpheratz  – Delta And
                1, 2,  // Delta And  – Mirach
                2, 3,  // Mirach     – Almach
                1, 4,  // Delta And  – Pi And
                4, 2,  // Pi And     – Mirach
                2, 5,  // Mirach     – Mu And
                5, 6,  // Mu And     – Nu And
                0, 7,  // Alpheratz  – Sigma And
                7, 2,  // Sigma And  – Mirach
            };
            d.lineColor      = new Color(0.55f, 0.75f, 1.00f, 0.55f);
            d.highlightColor = new Color(0.70f, 0.90f, 1.00f, 1.00f);
            d.lineWidth      = 0.12f;
            d.highlightWidth = 0.22f;
            d.linkedObjects  = LoadObjects("Andromeda Galaxy");
            Save(d, "Andromeda");
        }

        // ─── AQUARIUS (12 stars) ─────────────────────────────────────────────────
        static void CreateAquarius()
        {
            var d = MakeOrGet("Aquarius");
            d.constellationName = "Aquarius";
            d.mythology = "The water-bearer, one of the oldest constellations. " +
                          "Saturn currently resides in Aquarius, visible from Griffith in December.";
            d.stars = new[]
            {
                S("Sadalsuud",  21.525982f,  -5.571172f),  // [0]  bet Aqr  HIP 106278
                S("Sadalmelik", 22.096399f,  -0.319851f),  // [1]  alf Aqr  HIP 109074
                S("Sadachbia",  22.360938f,  -1.387331f),  // [2]  gam Aqr  HIP 110395
                S("Albali",     20.794598f,  -9.495776f),  // [3]  eps Aqr  HIP 102618
                S("Ancha",      22.280565f,  -7.783290f),  // [4]  tht Aqr  HIP 110003
                S("Skat",       22.910837f, -15.820820f),  // [5]  del Aqr  HIP 113136
                S("Zeta1 Aqr",  22.480531f,  -0.019972f),  // [6]  zet1 Aqr HIP 110960 (water jar)
                S("Eta Aqr",    22.589270f,  -0.117500f),  // [7]  eta Aqr  HIP 111497 (water jar)
                S("Pi Aqr",     22.421280f,   1.377400f),  // [8]  pi  Aqr  HIP 110672 (water jar)
                S("Iota Aqr",   22.107290f, -13.869700f),  // [9]  iot Aqr  HIP 109139
                S("Lambda Aqr", 22.876910f,  -7.579600f),  // [10] lam Aqr  HIP 112961
                S("Tau Aqr",    22.826530f, -13.592500f),  // [11] tau Aqr  HIP 114542
            };
            d.connectionIndices = new[]
            {
                 3,  0,  // Albali      – Sadalsuud
                 0,  1,  // Sadalsuud   – Sadalmelik
                 1,  2,  // Sadalmelik  – Sadachbia
                 2,  6,  // Sadachbia   – Zeta1 Aqr
                 6,  7,  // Zeta1 Aqr   – Eta Aqr    (water jar)
                 6,  8,  // Zeta1 Aqr   – Pi Aqr     (water jar)
                 1,  9,  // Sadalmelik  – Iota Aqr
                 1,  4,  // Sadalmelik  – Ancha
                 4, 10,  // Ancha       – Lambda Aqr
                10,  5,  // Lambda Aqr  – Skat
                 5, 11,  // Skat        – Tau Aqr
                11, 10,  // Tau Aqr     – Lambda Aqr
            };
            d.lineColor      = new Color(0.55f, 0.75f, 1.00f, 0.55f);
            d.highlightColor = new Color(0.70f, 0.90f, 1.00f, 1.00f);
            d.lineWidth      = 0.12f;
            d.highlightWidth = 0.22f;
            d.linkedObjects  = LoadObjects("Saturn");
            Save(d, "Aquarius");
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        static ConstellationData MakeOrGet(string assetName)
        {
            string path = $"{OutPath}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ConstellationData>(path);
            return existing != null ? existing : ScriptableObject.CreateInstance<ConstellationData>();
        }

        static void Save(ConstellationData d, string assetName)
        {
            string path = $"{OutPath}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<ConstellationData>(path) == null)
                AssetDatabase.CreateAsset(d, path);
            else
                EditorUtility.SetDirty(d);
        }

        static ConstellationData.ConstellationStar S(string name, float ra, float dec) =>
            new ConstellationData.ConstellationStar { starName = name, raHours = ra, decDegrees = dec };

        static CelestialObjectData[] LoadObjects(params string[] names)
        {
            var list = new System.Collections.Generic.List<CelestialObjectData>();
            foreach (var name in names)
            {
                var obj = AssetDatabase.LoadAssetAtPath<CelestialObjectData>($"{ObjectPath}/{name}.asset");
                if (obj != null) list.Add(obj);
                else Debug.LogWarning($"[ConstellationGenerator] Not found: {ObjectPath}/{name}.asset");
            }
            return list.ToArray();
        }
    }
}
