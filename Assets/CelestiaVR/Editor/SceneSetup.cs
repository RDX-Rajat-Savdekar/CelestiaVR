using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;

namespace CelestiaVR.Editor
{
    /// <summary>
    /// One-click scene setup for CelestiaVR.
    /// Run each step from the CelestiaVR menu in order.
    /// </summary>
    public static class SceneSetup
    {
        // ─────────────────────────────────────────────────────────────────────
        // 1. Layer
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/1 - Create CelestialObject Layer")]
        public static void CreateCelestialLayer()
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset"));
            var layers = tagManager.FindProperty("layers");

            // Check if already exists
            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == "CelestialObject")
                {
                    Debug.Log("[CelestiaVR] Layer 'CelestialObject' already exists.");
                    return;
                }
            }

            // Find first empty user layer (8–31)
            for (int i = 8; i < layers.arraySize; i++)
            {
                var el = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(el.stringValue))
                {
                    el.stringValue = "CelestialObject";
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"[CelestiaVR] Created layer 'CelestialObject' at index {i}.");
                    return;
                }
            }
            Debug.LogError("[CelestiaVR] No free layer slot found.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2. Telescope eyepiece
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/2 - Setup Telescope Eyepiece")]
        public static void SetupTelescopeEyepiece()
        {
            // Find EyepieceCamera GO
            var eyepieceCamGO = GameObject.Find("EyepieceCamera");
            if (eyepieceCamGO == null) { Debug.LogError("[CelestiaVR] EyepieceCamera not found in scene."); return; }

            // Add / configure Camera
            var cam = eyepieceCamGO.GetComponent<Camera>();
            if (cam == null) cam = eyepieceCamGO.AddComponent<Camera>();
            cam.fieldOfView  = 60f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane  = 2000f;
            cam.clearFlags   = CameraClearFlags.Skybox;
            cam.enabled      = false;

            // Find or create EyepieceRT
            var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(
                "Assets/CelestiaVR/Prefabs/telescope/source/EyepieceRT.asset");
            if (rt == null)
            {
                rt = new RenderTexture(512, 512, 16);
                rt.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
                rt.name = "EyepieceRT";
                AssetDatabase.CreateAsset(rt, "Assets/CelestiaVR/Prefabs/telescope/source/EyepieceRT.asset");
                AssetDatabase.SaveAssets();
            }
            cam.targetTexture = rt;

            // Create EyepieceDisplay quad as child of EyepieceCamera
            var existing = eyepieceCamGO.transform.Find("EyepieceDisplay");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "EyepieceDisplay";
            quad.transform.SetParent(eyepieceCamGO.transform, false);
            quad.transform.localPosition = new Vector3(0f, 0f, 0.08f);
            quad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            quad.transform.localScale    = new Vector3(0.05f, 0.05f, 1f);
            Object.DestroyImmediate(quad.GetComponent<MeshCollider>());

            // Create unlit material using EyepieceRT
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.mainTexture = rt;
            mat.name = "EyepieceMaterial";
            AssetDatabase.CreateAsset(mat, "Assets/CelestiaVR/Prefabs/telescope/source/EyepieceMaterial.mat");
            quad.GetComponent<MeshRenderer>().sharedMaterial = mat;

            EditorUtility.SetDirty(eyepieceCamGO);
            Debug.Log("[CelestiaVR] Eyepiece camera + display quad set up.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3. Wire TelescopeController
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/3 - Wire Telescope Controller")]
        public static void WireTelescopeController()
        {
            var telescopeGO = GameObject.Find("Telescope");
            if (telescopeGO == null) { Debug.LogError("[CelestiaVR] Telescope not found."); return; }

            var controller = telescopeGO.GetComponent<TelescopeController>();
            if (controller == null) { Debug.LogError("[CelestiaVR] TelescopeController not on Telescope."); return; }

            var so = new SerializedObject(controller);

            // EyepieceCamera
            var eyepieceCamGO = FindInChildren(telescopeGO.transform, "EyepieceCamera");
            if (eyepieceCamGO != null)
            {
                var cam = eyepieceCamGO.GetComponent<Camera>();
                so.FindProperty("eyepieceCamera").objectReferenceValue = cam;

                var rt = cam?.targetTexture;
                so.FindProperty("eyepieceRenderTexture").objectReferenceValue = rt;
            }

            // BarrelPivot
            var barrelPivot = FindInChildren(telescopeGO.transform, "BarrelPivot");
            if (barrelPivot != null)
                so.FindProperty("barrelPivot").objectReferenceValue = barrelPivot;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            Debug.Log("[CelestiaVR] TelescopeController wired.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 4. Assign CelestialObject layer to CelestialObjects GO + Dwell
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/4 - Assign CelestialObject Layer")]
        public static void AssignCelestialLayer()
        {
            int layer = LayerMask.NameToLayer("CelestialObject");
            if (layer == -1) { Debug.LogError("[CelestiaVR] Run step 1 first to create the layer."); return; }

            var celestialObjectsGO = GameObject.Find("CelestialObjects");
            if (celestialObjectsGO != null)
            {
                celestialObjectsGO.layer = layer;
                foreach (Transform child in celestialObjectsGO.transform)
                    child.gameObject.layer = layer;
                EditorUtility.SetDirty(celestialObjectsGO);
                Debug.Log("[CelestiaVR] CelestialObjects layer assigned.");
            }

            // Wire layer mask into TelescopeDwellController
            var telescope = GameObject.Find("Telescope");
            if (telescope != null)
            {
                var dwell = telescope.GetComponent<TelescopeDwellController>();
                if (dwell != null)
                {
                    var so = new SerializedObject(dwell);
                    so.FindProperty("celestialObjectLayer").intValue = 1 << layer;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(dwell);
                    Debug.Log("[CelestiaVR] TelescopeDwellController layer mask set.");
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // 5. Wire ConstellationManager
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/5 - Wire Constellation Manager")]
        public static void WireConstellationManager()
        {
            var cmGO = GameObject.Find("ConstellationManager");
            if (cmGO == null) { Debug.LogError("[CelestiaVR] ConstellationManager not in scene."); return; }

            var cm = cmGO.GetComponent<ConstellationManager>();
            if (cm == null) { Debug.LogError("[CelestiaVR] ConstellationManager component missing."); return; }

            var so = new SerializedObject(cm);

            // SkyDome reference
            var skyDome = GameObject.Find("SkyDome");
            if (skyDome != null)
                so.FindProperty("skyDome").objectReferenceValue = skyDome.transform;

            so.FindProperty("skyDomeRadius").floatValue = 50f;

            // ConstellationLine material
            var lineMat = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/CelestiaVR/Prefabs/ConstellationLine.mat");
            if (lineMat == null)
            {
                lineMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                lineMat.color = new Color(0.4f, 0.7f, 1f, 0.6f);
                lineMat.name = "ConstellationLine";
                AssetDatabase.CreateAsset(lineMat, "Assets/CelestiaVR/Prefabs/ConstellationLine.mat");
                AssetDatabase.SaveAssets();
            }
            so.FindProperty("constellationLineMaterial").objectReferenceValue = lineMat;

            // Load all constellation assets
            var guids = AssetDatabase.FindAssets("t:ConstellationData",
                new[] { "Assets/CelestiaVR/Data/Constellations" });
            var constellationsProp = so.FindProperty("constellations");
            constellationsProp.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var data = AssetDatabase.LoadAssetAtPath<ConstellationData>(path);
                constellationsProp.GetArrayElementAtIndex(i).objectReferenceValue = data;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(cm);
            Debug.Log($"[CelestiaVR] ConstellationManager wired with {guids.Length} constellations.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 6. Build InspectionPanel hierarchy
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/6 - Build Inspection Panel")]
        public static void BuildInspectionPanel()
        {
            // Remove old
            var old = GameObject.Find("InspectionPanel");
            if (old != null) Object.DestroyImmediate(old);

            // Root
            var root = new GameObject("InspectionPanel");
            root.AddComponent<InspectionPanel>();
            root.AddComponent<InspectionTrigger>();

            // PanelRoot
            var panelRoot = new GameObject("PanelRoot");
            panelRoot.transform.SetParent(root.transform, false);
            var cg = panelRoot.AddComponent<CanvasGroup>();

            // ModelStage (left side)
            var modelStage = new GameObject("ModelStage");
            modelStage.transform.SetParent(panelRoot.transform, false);
            modelStage.transform.localPosition = new Vector3(-0.25f, 0f, 0f);

            // PlanetView
            var planetViewGO = new GameObject("PlanetView");
            planetViewGO.transform.SetParent(modelStage.transform, false);
            var planetView = planetViewGO.AddComponent<PlanetInspectionView>();

            // ConstellationView
            var constViewGO = new GameObject("ConstellationView");
            constViewGO.transform.SetParent(modelStage.transform, false);
            var constView = constViewGO.AddComponent<ConstellationInspectionView>();
            constViewGO.SetActive(false);

            // InfoCanvas (right side)
            var infoCanvasGO = new GameObject("InfoCanvas");
            infoCanvasGO.transform.SetParent(panelRoot.transform, false);
            infoCanvasGO.transform.localPosition = new Vector3(0.15f, 0f, 0f);
            var canvas = infoCanvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            infoCanvasGO.AddComponent<CanvasScaler>();
            infoCanvasGO.AddComponent<GraphicRaycaster>();
            var rt2 = infoCanvasGO.GetComponent<RectTransform>();
            rt2.sizeDelta = new Vector2(400f, 500f);
            infoCanvasGO.transform.localScale = Vector3.one * 0.001f;

            // TMP text fields
            var nameText    = MakeTMPLabel(infoCanvasGO.transform, "NameText",    new Vector2(0, 200), 28, FontStyles.Bold);
            var typeText    = MakeTMPLabel(infoCanvasGO.transform, "TypeText",    new Vector2(0, 160), 18, FontStyles.Normal);
            var descText    = MakeTMPLabel(infoCanvasGO.transform, "DescText",    new Vector2(0, 80),  16, FontStyles.Normal);
            var distText    = MakeTMPLabel(infoCanvasGO.transform, "DistText",    new Vector2(0, -60), 16, FontStyles.Normal);
            var factText    = MakeTMPLabel(infoCanvasGO.transform, "FactText",    new Vector2(0, -140),15, FontStyles.Italic);

            // Wire InspectionPanel
            var ip = root.GetComponent<InspectionPanel>();
            var ipSO = new SerializedObject(ip);
            ipSO.FindProperty("panelRoot").objectReferenceValue          = panelRoot.transform;
            ipSO.FindProperty("canvasGroup").objectReferenceValue        = cg;
            ipSO.FindProperty("modelStage").objectReferenceValue         = modelStage.transform;
            ipSO.FindProperty("planetView").objectReferenceValue         = planetView;
            ipSO.FindProperty("constellationView").objectReferenceValue  = constView;
            ipSO.FindProperty("nameText").objectReferenceValue           = nameText;
            ipSO.FindProperty("typeText").objectReferenceValue           = typeText;
            ipSO.FindProperty("descriptionText").objectReferenceValue    = descText;
            ipSO.FindProperty("distanceText").objectReferenceValue       = distText;
            ipSO.FindProperty("factText").objectReferenceValue           = factText;
            ipSO.ApplyModifiedProperties();

            root.SetActive(true);
            panelRoot.SetActive(false); // hidden until triggered

            EditorUtility.SetDirty(root);
            Debug.Log("[CelestiaVR] InspectionPanel built.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 7. Build Onboarding Panel
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/7 - Build Onboarding Panel")]
        public static void BuildOnboardingPanel()
        {
            var old = GameObject.Find("OnboardingPanel");
            if (old != null) Object.DestroyImmediate(old);

            var go = new GameObject("OnboardingPanel");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            var cg = go.AddComponent<CanvasGroup>();
            var op = go.AddComponent<OnboardingPanel>();

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(800, 500);
            go.transform.localScale = Vector3.one * 0.001f;
            go.transform.position = new Vector3(0, 1.6f, 1.5f);

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var img = bg.AddComponent<Image>();
            img.color = new Color(0.05f, 0.05f, 0.15f, 0.9f);
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

            // Title
            MakeTMPLabel(go.transform, "Title", new Vector2(0, 190), 32, FontStyles.Bold, "Welcome to CelestiaVR");

            // Instructions
            MakeTMPLabel(go.transform, "Instructions", new Vector2(0, 20), 18, FontStyles.Normal,
                "Left Joystick → Pan telescope\nRight Joystick → Zoom\nX Button → Discovery Mode\nB Button → Settings\nRight Trigger → Inspect object");

            // Skip button
            var skipGO = new GameObject("SkipButton");
            skipGO.transform.SetParent(go.transform, false);
            var btn = skipGO.AddComponent<Button>();
            var btnImg = skipGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.4f, 0.8f, 1f);
            var btnRT = skipGO.GetComponent<RectTransform>();
            btnRT.anchoredPosition = new Vector2(0, -190);
            btnRT.sizeDelta = new Vector2(200, 50);
            var skipLabel = MakeTMPLabel(skipGO.transform, "SkipLabel", Vector2.zero, 20, FontStyles.Bold, "Skip");
            btn.onClick.AddListener(op.Skip);

            // Wire canvasGroup
            var opSO = new SerializedObject(op);
            opSO.FindProperty("canvasGroup").objectReferenceValue = cg;
            opSO.ApplyModifiedProperties();

            EditorUtility.SetDirty(go);
            Debug.Log("[CelestiaVR] OnboardingPanel built.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 8. Build Settings Panel
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/8 - Build Settings Panel")]
        public static void BuildSettingsPanel()
        {
            var old = GameObject.Find("SettingsPanel");
            if (old != null) Object.DestroyImmediate(old);

            var go = new GameObject("SettingsPanel");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            var sp = go.AddComponent<SettingsPanel>();
            go.transform.localScale = Vector3.one * 0.001f;
            go.transform.position   = new Vector3(2f, 1.4f, 0f); // on observatory wall

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 600);

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var img = bg.AddComponent<Image>();
            img.color = new Color(0.05f, 0.05f, 0.15f, 0.92f);
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

            MakeTMPLabel(go.transform, "Title", new Vector2(0, 250), 28, FontStyles.Bold, "Settings");
            MakeTMPLabel(go.transform, "AudioLabel", new Vector2(-120, 150), 20, FontStyles.Normal, "Volume");

            // Audio slider
            var sliderGO = new GameObject("AudioSlider");
            sliderGO.transform.SetParent(go.transform, false);
            var slider = sliderGO.AddComponent<Slider>();
            sliderGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(60, 150);
            sliderGO.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 30);
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;
            slider.onValueChanged.AddListener(sp.SetAudioVolume);

            // Constellation toggle
            MakeTMPLabel(go.transform, "ConstLabel", new Vector2(-130, 60), 20, FontStyles.Normal, "Constellations");
            var constTogGO = new GameObject("ConstellationToggle");
            constTogGO.transform.SetParent(go.transform, false);
            var constTog = constTogGO.AddComponent<Toggle>(); // Toggle adds RectTransform
            constTogGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(120, 60);
            constTog.isOn = true;
            constTog.onValueChanged.AddListener(sp.ToggleConstellations);

            // Exit button
            var exitGO = new GameObject("ExitButton");
            exitGO.transform.SetParent(go.transform, false);
            var exitBtn = exitGO.AddComponent<Button>();
            var exitImg = exitGO.AddComponent<Image>();
            exitImg.color = new Color(0.7f, 0.15f, 0.15f, 1f);
            exitGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -220);
            exitGO.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 55);
            MakeTMPLabel(exitGO.transform, "ExitLabel", Vector2.zero, 22, FontStyles.Bold, "Exit");
            exitBtn.onClick.AddListener(sp.ExitExperience);

            go.SetActive(false); // hidden until B button

            // Wire settings panel into TelescopeInputHandler
            var telescope = GameObject.Find("Telescope");
            if (telescope != null)
            {
                var handler = telescope.GetComponent<TelescopeInputHandler>();
                if (handler != null)
                {
                    var hSO = new SerializedObject(handler);
                    hSO.FindProperty("settingsPanel").objectReferenceValue = go;
                    hSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(handler);
                }
            }

            EditorUtility.SetDirty(go);
            Debug.Log("[CelestiaVR] SettingsPanel built and wired to telescope.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 9. Setup Ambient Audio
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/9 - Setup Ambient Audio")]
        public static void SetupAmbientAudio()
        {
            var old = GameObject.Find("AmbientAudio");
            if (old != null) Object.DestroyImmediate(old);

            var root = new GameObject("AmbientAudio");
            var controller = root.AddComponent<AmbientAudioController>();

            var windSource = new GameObject("WindLoop").AddComponent<AudioSource>();
            windSource.transform.SetParent(root.transform, false);
            windSource.loop = true; windSource.spatialBlend = 0f; windSource.playOnAwake = false;

            var citySource = new GameObject("CityHum").AddComponent<AudioSource>();
            citySource.transform.SetParent(root.transform, false);
            citySource.loop = true; citySource.spatialBlend = 0f; citySource.playOnAwake = false;

            var oneShotSource = new GameObject("OneShot").AddComponent<AudioSource>();
            oneShotSource.transform.SetParent(root.transform, false);
            oneShotSource.spatialBlend = 0f; oneShotSource.playOnAwake = false;

            var so = new SerializedObject(controller);
            so.FindProperty("windLoopSource").objectReferenceValue  = windSource;
            so.FindProperty("cityHumSource").objectReferenceValue   = citySource;
            so.FindProperty("oneShotSource").objectReferenceValue   = oneShotSource;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(root);
            Debug.Log("[CelestiaVR] AmbientAudio set up. Assign audio clips in Inspector.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 10. Build InfoPanel
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/10 - Build Info Panel")]
        public static void BuildInfoPanel()
        {
            var old = GameObject.Find("InfoPanel");
            if (old != null) Object.DestroyImmediate(old);

            var go = new GameObject("InfoPanel");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            var cg = go.AddComponent<CanvasGroup>();
            var ip = go.AddComponent<InfoPanel>();

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(600, 350);
            go.transform.localScale = Vector3.one * 0.001f;
            go.transform.position   = new Vector3(0f, 1.6f, 2f);

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.04f, 0.12f, 0.88f);
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

            var nameText = MakeTMPLabel(go.transform, "ObjectName",  new Vector2(0,  140), 30, FontStyles.Bold);
            var typeText = MakeTMPLabel(go.transform, "ObjectType",  new Vector2(0,  100), 18, FontStyles.Normal);
            var descText = MakeTMPLabel(go.transform, "Description", new Vector2(0,   30), 15, FontStyles.Normal);
            var distText = MakeTMPLabel(go.transform, "Distance",    new Vector2(0,  -70), 15, FontStyles.Normal);
            var factText = MakeTMPLabel(go.transform, "Fact",        new Vector2(0, -130), 14, FontStyles.Italic);

            var ipSO = new SerializedObject(ip);
            ipSO.FindProperty("objectNameText").objectReferenceValue  = nameText;
            ipSO.FindProperty("objectTypeText").objectReferenceValue  = typeText;
            ipSO.FindProperty("descriptionText").objectReferenceValue = descText;
            ipSO.FindProperty("distanceText").objectReferenceValue    = distText;
            ipSO.FindProperty("factText").objectReferenceValue        = factText;
            ipSO.FindProperty("canvasGroup").objectReferenceValue     = cg;
            ipSO.ApplyModifiedProperties();

            go.SetActive(false);
            EditorUtility.SetDirty(go);
            Debug.Log("[CelestiaVR] InfoPanel built.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 11. Setup SceneTransitionController (fade quad on XR camera)
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/11 - Setup Scene Transition")]
        public static void SetupSceneTransition()
        {
            // Find main camera (XR Origin > Camera Offset > Main Camera)
            var mainCamGO = GameObject.FindWithTag("MainCamera");
            if (mainCamGO == null)
            {
                Debug.LogError("[CelestiaVR] Main Camera not found. Ensure a camera is tagged MainCamera.");
                return;
            }

            // Remove old
            var oldCtrl = GameObject.Find("SceneTransitionController");
            if (oldCtrl != null) Object.DestroyImmediate(oldCtrl);

            // Create controller GO at scene root
            var ctrlGO = new GameObject("SceneTransitionController");
            var stc = ctrlGO.AddComponent<SceneTransitionController>();

            // Create full-screen black quad parented to the camera
            var oldQuad = mainCamGO.transform.Find("FadeOverlayQuad");
            if (oldQuad != null) Object.DestroyImmediate(oldQuad.gameObject);

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "FadeOverlayQuad";
            quad.transform.SetParent(mainCamGO.transform, false);
            quad.transform.localPosition = new Vector3(0f, 0f, 0.31f); // just beyond near clip
            quad.transform.localRotation = Quaternion.identity;
            quad.transform.localScale    = new Vector3(0.6f, 0.6f, 1f); // covers FOV
            Object.DestroyImmediate(quad.GetComponent<MeshCollider>());

            // Black unlit material
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = Color.black;
            mat.name  = "FadeOverlayMat";
            // Enable transparency
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0);   // Alpha
            mat.renderQueue = 3000;
            AssetDatabase.CreateAsset(mat, "Assets/CelestiaVR/Prefabs/FadeOverlayMat.mat");
            AssetDatabase.SaveAssets();
            quad.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Screen Space - Camera canvas renders in stereo on Quest (ScreenSpaceOverlay does not)
            var overlayCanvasGO = new GameObject("FadeCanvas");
            overlayCanvasGO.transform.SetParent(mainCamGO.transform, false);
            var overlayCanvas = overlayCanvasGO.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            overlayCanvas.worldCamera = mainCamGO.GetComponent<Camera>();
            overlayCanvas.planeDistance = 0.32f;  // just beyond near clip
            overlayCanvas.sortingOrder = 999;
            overlayCanvasGO.AddComponent<CanvasScaler>();

            var imgGO = new GameObject("FadeImage");
            imgGO.transform.SetParent(overlayCanvasGO.transform, false);
            var fadeImg = imgGO.AddComponent<Image>();
            fadeImg.color = Color.black;
            var imgRT = imgGO.GetComponent<RectTransform>();
            imgRT.anchorMin = Vector2.zero; imgRT.anchorMax = Vector2.one;
            imgRT.offsetMin = imgRT.offsetMax = Vector2.zero;

            // Destroy the quad — canvas image is better for VR fades
            Object.DestroyImmediate(quad);

            var stcSO = new SerializedObject(stc);
            stcSO.FindProperty("fadeOverlay").objectReferenceValue = fadeImg;
            stcSO.ApplyModifiedProperties();

            EditorUtility.SetDirty(ctrlGO);
            Debug.Log("[CelestiaVR] SceneTransitionController set up with fade canvas.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 12. Setup Telescope Proximity Indicator
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/12 - Setup Proximity Indicator")]
        public static void SetupProximityIndicator()
        {
            var telescope = GameObject.Find("Telescope");
            if (telescope == null) { Debug.LogError("[CelestiaVR] Telescope not found."); return; }

            // Add component if missing
            var indicator = telescope.GetComponent<TelescopeProximityIndicator>();
            if (indicator == null)
                indicator = telescope.AddComponent<TelescopeProximityIndicator>();

            // Create GlowRing quad as child
            var oldRing = telescope.transform.Find("GlowRing");
            if (oldRing != null) Object.DestroyImmediate(oldRing.gameObject);

            var ring = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ring.name = "GlowRing";
            ring.transform.SetParent(telescope.transform, false);
            ring.transform.localPosition = new Vector3(0f, -0.05f, 0f); // just above floor
            ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // flat on floor
            ring.transform.localScale    = new Vector3(1.5f, 1.5f, 1f);
            Object.DestroyImmediate(ring.GetComponent<MeshCollider>());

            // Unlit emissive blue material
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = new Color(0.3f, 0.6f, 1f, 0.6f);
            mat.name  = "GlowRingMat";
            mat.SetFloat("_Surface", 1); // Transparent
            mat.renderQueue = 3000;
            AssetDatabase.CreateAsset(mat, "Assets/CelestiaVR/Prefabs/GlowRingMat.mat");
            AssetDatabase.SaveAssets();
            ring.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Wire indicator
            var so = new SerializedObject(indicator);
            so.FindProperty("glowRing").objectReferenceValue = ring;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(telescope);
            Debug.Log("[CelestiaVR] Telescope proximity indicator + glow ring set up.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 13. Assign Ambient Audio Clips to AmbientAudioController
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/13 - Assign Ambient Audio Clips")]
        public static void AssignAmbientAudioClips()
        {
            var audioGO = GameObject.Find("AmbientAudio");
            if (audioGO == null) { Debug.LogError("[CelestiaVR] AmbientAudio not in scene. Run step 9 first."); return; }

            var controller = audioGO.GetComponent<AmbientAudioController>();
            if (controller == null) { Debug.LogError("[CelestiaVR] AmbientAudioController component missing."); return; }

            const string audioPath = "Assets/CelestiaVR/Audio/Ambient/";
            var so = new SerializedObject(controller);

            TryAssignClip(so, "windLoop",       audioPath + "WindLoop.mp3");
            TryAssignClip(so, "windGust",       audioPath + "WindGust.mp3");
            TryAssignClip(so, "cityHum",        audioPath + "CityHum.mp3");
            TryAssignClip(so, "cricketOneShot", audioPath + "Cricket.mp3");
            TryAssignClip(so, "coyoteRare",     audioPath + "Coyote.mp3");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            Debug.Log("[CelestiaVR] Ambient audio clips assigned to AmbientAudioController.");
        }

        private static void TryAssignClip(SerializedObject so, string fieldName, string assetPath)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (clip == null) { Debug.LogWarning($"[CelestiaVR] Clip not found at {assetPath} — re-run after Unity imports audio."); return; }
            so.FindProperty(fieldName).objectReferenceValue = clip;
        }

        // ─────────────────────────────────────────────────────────────────────
        // RUN ALL
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/→ RUN ALL SETUP STEPS")]
        public static void RunAll()
        {
            CreateCelestialLayer();
            SetupTelescopeEyepiece();
            WireTelescopeController();
            AssignCelestialLayer();
            WireConstellationManager();
            BuildInspectionPanel();
            BuildOnboardingPanel();
            BuildSettingsPanel();
            SetupAmbientAudio();
            BuildInfoPanel();
            SetupSceneTransition();
            SetupProximityIndicator();
            AssignAmbientAudioClips();
            AssignAudioClips();
            SetupBalconyPlaceholder();

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[CelestiaVR] ✓ All setup steps complete. Save the scene (Ctrl+S).");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────
        private static Transform FindInChildren(Transform parent, string name)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>())
                if (child.name == name) return child;
            return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 14. Balcony Placeholder
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Creates a simple stand-in balcony: a large floor plane + three low railing
        /// cubes on the North/East/West edges.  Stone-grey URP Lit material is applied.
        /// The player spawn point is centred on the floor at y = 0.
        ///
        /// Dimensions (metres):  floor 10 × 8, railing height 1.1 m, width 0.15 m.
        /// Run again to rebuild (old root is destroyed first).
        /// </summary>
        [MenuItem("CelestiaVR/Setup/14 - Setup Balcony Placeholder")]
        public static void SetupBalconyPlaceholder()
        {
            // ── Destroy old ──────────────────────────────────────────────────
            var old = GameObject.Find("BalconyPlaceholder");
            if (old != null) Object.DestroyImmediate(old);

            // ── Root ─────────────────────────────────────────────────────────
            var root = new GameObject("BalconyPlaceholder");

            // ── Stone material (URP Lit, no texture — just a grey colour) ────
            const string matPath = "Assets/CelestiaVR/Prefabs/BalconyStone.mat";
            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (stoneMat == null)
            {
                stoneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                // Warm stone grey — adjust the colour tint to taste
                stoneMat.color = new Color(0.55f, 0.52f, 0.48f);
                stoneMat.name  = "BalconyStone";
                // Slightly rough, non-metallic
                stoneMat.SetFloat("_Metallic",   0f);
                stoneMat.SetFloat("_Smoothness", 0.25f);
                AssetDatabase.CreateAsset(stoneMat, matPath);
                AssetDatabase.SaveAssets();
            }

            // ── Floor ────────────────────────────────────────────────────────
            // Unity's default Plane is 10 × 10 units at scale 1.
            // We want 10 m (X) × 8 m (Z) → scale (1, 1, 0.8).
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "BalconyFloor";
            floor.transform.SetParent(root.transform, false);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale    = new Vector3(1f, 1f, 0.8f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = stoneMat;
            // Keep MeshCollider so the player stands on it.

            // ── Helper: make a railing cube ───────────────────────────────────
            // pos is centre-world position relative to root
            void MakeRailing(string railName, Vector3 pos, Vector3 scale)
            {
                var r = GameObject.CreatePrimitive(PrimitiveType.Cube);
                r.name = railName;
                r.transform.SetParent(root.transform, false);
                r.transform.localPosition = pos;
                r.transform.localScale    = scale;
                r.GetComponent<MeshRenderer>().sharedMaterial = stoneMat;
            }

            // Railing thickness / height
            const float railH = 1.1f;   // height above floor (cube half-height = 0.55)
            const float railT = 0.15f;  // thickness
            const float railY = railH * 0.5f;  // centre y so bottom sits at y=0

            // Floor extents: X ±5 m, Z ±4 m
            // North railing (far end, +Z)
            MakeRailing("Railing_North", new Vector3(0f,        railY,  4f),
                                         new Vector3(10f + railT, railH, railT));

            // East railing (+X side)
            MakeRailing("Railing_East",  new Vector3( 5f, railY, 0f),
                                         new Vector3(railT, railH, 8f));

            // West railing (−X side)
            MakeRailing("Railing_West",  new Vector3(-5f, railY, 0f),
                                         new Vector3(railT, railH, 8f));

            // South side left open so the player can "enter" from an interior.

            // ── Telescope table (small cube to rest telescope on) ────────────
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "TelescopeTable";
            table.transform.SetParent(root.transform, false);
            table.transform.localPosition = new Vector3(0f, 0.45f, 1.5f); // centred, ~knee height
            table.transform.localScale    = new Vector3(0.6f, 0.9f, 0.6f);
            table.GetComponent<MeshRenderer>().sharedMaterial = stoneMat;

            // ── Teleport area marker (TeleportationArea on the floor) ────────
            // Requires XR Interaction Toolkit — add the component if available.
            var areaType = System.Type.GetType(
                "UnityEngine.XR.Interaction.Toolkit.AR.TeleportationArea, " +
                "Unity.XR.Interaction.Toolkit");
            // Try the non-AR namespace first (XRIT 3.x)
            if (areaType == null)
                areaType = System.Type.GetType(
                    "UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea, " +
                    "Unity.XR.Interaction.Toolkit");
            if (areaType == null)
                areaType = System.Type.GetType(
                    "UnityEngine.XR.Interaction.Toolkit.TeleportationArea, " +
                    "Unity.XR.Interaction.Toolkit");

            if (areaType != null)
            {
                floor.AddComponent(areaType);
                Debug.Log("[CelestiaVR] TeleportationArea added to BalconyFloor.");
            }
            else
            {
                Debug.LogWarning("[CelestiaVR] TeleportationArea type not found — add it manually to BalconyFloor.");
            }

            // ── Mark dirty & finish ───────────────────────────────────────────
            EditorUtility.SetDirty(root);
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[CelestiaVR] Balcony placeholder created. Move XR Origin so Camera Offset is at y = 0.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Audio Clip Auto-Assignment
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("CelestiaVR/Setup/Assign Audio Clips")]
        public static void AssignAudioClips()
        {
            const string teleDir   = "Assets/CelestiaVR/Audio/Telescope";
            const string discoDir  = "Assets/CelestiaVR/Audio/Discovery";

            AudioClip Load(string folder, string name) =>
                AssetDatabase.LoadAssetAtPath<AudioClip>($"{folder}/{name}.mp3");

            // ── TelescopeController ──────────────────────────────────────────
            var tc = Object.FindFirstObjectByType<TelescopeController>();
            if (tc != null)
            {
                var so = new SerializedObject(tc);
                SetClip(so, "grabSound",     Load(teleDir, "TelescopeGrab"));
                SetClip(so, "frictionSound", Load(teleDir, "TelescopeFriction"));
                SetClip(so, "zoomSound",     Load(teleDir, "TelescopeZoom"));
                SetClip(so, "endStopSound",  Load(teleDir, "TelescopeEndStop"));
                so.ApplyModifiedProperties();
                EnsureAudioSources(tc.gameObject, so, "oneShotSource", "frictionSource");
                EditorUtility.SetDirty(tc);
                Debug.Log("[CelestiaVR] Audio clips assigned to TelescopeController.");
            }
            else Debug.LogWarning("[CelestiaVR] TelescopeController not found in scene.");

            // ── TelescopeDwellController ─────────────────────────────────────
            var td = Object.FindFirstObjectByType<TelescopeDwellController>();
            if (td != null)
            {
                var so = new SerializedObject(td);
                SetClip(so, "dwellTone", Load(discoDir, "DwellTone"));
                so.ApplyModifiedProperties();
                EnsureAudioSource(td.gameObject, so, "dwellAudio", 0.25f);
                EditorUtility.SetDirty(td);
            }

            // ── TelescopeInputHandler ────────────────────────────────────────
            var th = Object.FindFirstObjectByType<TelescopeInputHandler>();
            if (th != null)
            {
                var so = new SerializedObject(th);
                SetClip(so, "discoveryWhoosh", Load(discoDir, "DiscoveryWhoosh"));
                so.ApplyModifiedProperties();
                EnsureAudioSource(th.gameObject, so, "audioSource", 0.8f);
                EditorUtility.SetDirty(th);
            }

            // ── DiscoveryManager ─────────────────────────────────────────────
            var dm = Object.FindFirstObjectByType<DiscoveryManager>();
            if (dm != null)
            {
                var so = new SerializedObject(dm);
                SetClip(so, "discoveryChime", Load(discoDir, "SingingBowl"));
                so.ApplyModifiedProperties();
                EnsureAudioSource(dm.gameObject, so, "audioSource", 1f);
                EditorUtility.SetDirty(dm);
            }

            // ── InspectionPanel ──────────────────────────────────────────────
            var ip = Object.FindFirstObjectByType<InspectionPanel>();
            if (ip != null)
            {
                var so = new SerializedObject(ip);
                SetClip(so, "panelAppearSound", Load(discoDir, "InfoPanelAppear"));
                so.ApplyModifiedProperties();
                EnsureAudioSource(ip.gameObject, so, "audioSource", 0.6f);
                EditorUtility.SetDirty(ip);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[CelestiaVR] Audio assignment complete.");
        }

        private static void SetClip(SerializedObject so, string field, AudioClip clip)
        {
            if (clip == null) { Debug.LogWarning($"[CelestiaVR] Clip not found for field: {field}"); return; }
            var prop = so.FindProperty(field);
            if (prop != null) prop.objectReferenceValue = clip;
        }

        private static void EnsureAudioSource(GameObject go, SerializedObject so, string field, float volume)
        {
            var prop = so.FindProperty(field);
            if (prop == null) return;
            if (prop.objectReferenceValue == null)
            {
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.volume = volume;
                prop.objectReferenceValue = src;
                so.ApplyModifiedProperties();
            }
        }

        private static void EnsureAudioSources(GameObject go, SerializedObject so,
            string oneShotField, string loopField)
        {
            // oneShot source
            var p1 = so.FindProperty(oneShotField);
            if (p1 != null && p1.objectReferenceValue == null)
            {
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake  = false;
                src.spatialBlend = 0f;
                src.volume       = 0.8f;
                p1.objectReferenceValue = src;
                so.ApplyModifiedProperties();
            }
            // loop source (friction)
            var p2 = so.FindProperty(loopField);
            if (p2 != null && p2.objectReferenceValue == null)
            {
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake  = false;
                src.spatialBlend = 0f;
                src.volume       = 0.5f;
                src.loop         = true;
                p2.objectReferenceValue = src;
                so.ApplyModifiedProperties();
            }
        }

        private static TextMeshProUGUI MakeTMPLabel(Transform parent, string goName,
            Vector2 anchoredPos, float fontSize, FontStyles style, string text = "")
        {
            var go = new GameObject(goName);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.fontStyle = style;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(380, 80);
            return tmp;
        }
    }
}
