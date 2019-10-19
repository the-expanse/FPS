using UnityEditor;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor.SceneManagement;
using ScriptLoader;
using UnityEngine.SceneManagement;
using Mono.Cecil;

enum ScreenType {
    StartScreen,
    ScriptScreen,
    BuildScreen,
    AfterBuildScreen,
    TestScreen,
    None
}

public class SideQuest : EditorWindow {

    string inputDirectory = "Assets/SideQuest/Temp";
    static string outputDirectory = "Assets/SideQuest/Output";
    private const float POSITION_UPDATE_INTERVAL = 0.5f;
    private float _nextPosUpdate = 0;
    bool isInit = false;
    bool isWarningsInit = false;
    bool isInspector = false;

    bool isDryRun = true;

    string saveScene = null;
    string bundleFilePath = null;

    TextEditor te = new TextEditor();
    Vector2 scrollPosition = new Vector2();
    List<bool> showFlags = new List<bool>();

    List<string> bundles = new List<string>();
    JObject scripts = new JObject();
    JObject assets = new JObject();
    JArray warnings = new JArray();

    GUIStyle headerStyle;
    GUIStyle bodyStyle;
    GUIStyle inputStyle;
    GUIStyle buttonStyle;
    GUIStyle wideButtonStyle;
    GUIStyle toggleStyle;
    GUIStyle codeStyle;
    GUIStyle headerStyleLeft;
    GUIStyle centerbuttonStyle;

    public Texture tex;

    List<string> hiddenAssets = new List<string>();

    ScreenType currentScreen = ScreenType.StartScreen;

    [MenuItem("Tools/SideQuest")]
    public static void Init() {
        GetWindow(typeof(SideQuest));
    }
    void OnEnable() {
        EditorApplication.playModeStateChanged += StateChange;
    }

    public System.Threading.Timer SetTimeout(Action callback, int interval) {
        System.Threading.Timer timer = null;
        timer = new System.Threading.Timer((obj) => {
            SDK.RunOnMainThread.Enqueue(callback);
            timer.Dispose();
        }, null, interval, System.Threading.Timeout.Infinite);
        return timer;
    }

    void StateChange(PlayModeStateChange stateChange) {
        if(stateChange == PlayModeStateChange.EnteredEditMode) {
            //ResetAssets();
            if (saveScene != null) {
                EditorSceneManager.OpenScene(saveScene);
                saveScene = null;
            }
            EditorApplication.UnlockReloadAssemblies();
        } else if(stateChange == PlayModeStateChange.EnteredPlayMode) {
            if(bundleFilePath != null) {
                RunBundleRuntime(bundleFilePath);
                bundleFilePath = null;
            }
        }
    }

    void HideAssets() {
        foreach (string assetPath in AssetDatabase.GetAllAssetPaths()) {
            if (assetPath.StartsWith("Assets/") &&
                !assetPath.StartsWith("Assets/SideQuest")) {
                var ext = Path.GetExtension(assetPath);
                var isAsset = new string[] {
                    ".mat",".shader",
                    ".jpg",".jpeg",".exr",".png",
                    ".wav",".mp3",".ogg",".cs"
                }.Contains(ext);
                if (isAsset) {
                    File.Delete(assetPath + ".meta");
                    File.Move(assetPath, assetPath+".~~");
                    hiddenAssets.Add(assetPath + ".~~");
                }
            }
        }
        AssetDatabase.Refresh();
    }

    void ResetAssets() {
        foreach (string assetPath in Directory.GetFiles("Assets/", "*.*", SearchOption.AllDirectories)) {
            if (!assetPath.StartsWith("Assets/.git") && assetPath.EndsWith(".~~")) {
                string newFileName = Path.GetDirectoryName(assetPath) + "/" + Path.GetFileNameWithoutExtension(assetPath);
                File.Move(assetPath, newFileName);
            }
        }
        AssetDatabase.Refresh();
    }

    void CreateDirectories() {
        if (!Directory.Exists(inputDirectory)) {
            Directory.CreateDirectory(inputDirectory);
        }
        if (!Directory.Exists(outputDirectory)) {
            Directory.CreateDirectory(outputDirectory);
        }
    }

    void SetupStyles() {
        minSize = new Vector2(350, 288);
        GUILayout.MinHeight(600f);

        headerStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        headerStyle.alignment = TextAnchor.UpperCenter;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 14;

        headerStyleLeft = new GUIStyle(GUI.skin.GetStyle("Label"));
        headerStyleLeft.alignment = TextAnchor.UpperLeft;
        headerStyleLeft.fontStyle = FontStyle.Bold;
        headerStyleLeft.fontSize = 14;

        bodyStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        bodyStyle.wordWrap = true;
        bodyStyle.fontSize = 12;
        bodyStyle.fontStyle = FontStyle.Normal;
        bodyStyle.margin.left = 20;
        bodyStyle.margin.right = 20;

        codeStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        codeStyle.wordWrap = true;
        codeStyle.alignment = TextAnchor.UpperLeft;
        codeStyle.fontSize = 9;
        codeStyle.fontStyle = FontStyle.Normal;

        inputStyle = GUI.skin.GetStyle("TextField");
        inputStyle.margin.left = 20;
        inputStyle.margin.right = 20;

        buttonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
        buttonStyle.fontSize = 14;
        buttonStyle.padding.top = 8;
        buttonStyle.padding.bottom = 8;
        buttonStyle.margin.bottom = 20;
        buttonStyle.margin.top = 10;
        buttonStyle.margin.right = 20;
        buttonStyle.margin.left = 20;
        
        centerbuttonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
        centerbuttonStyle.fontSize = 14;
        centerbuttonStyle.padding.top = 8;
        centerbuttonStyle.padding.bottom = 8;
        centerbuttonStyle.margin.bottom = 20;
        centerbuttonStyle.margin.top = 10;
        centerbuttonStyle.margin.right = 40;
        centerbuttonStyle.margin.left = 40;

        wideButtonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
        wideButtonStyle.fontSize = 12;
        wideButtonStyle.padding.top = 4;
        wideButtonStyle.padding.bottom = 4;
        wideButtonStyle.margin.left = 20;
        wideButtonStyle.margin.right = 20;
    }

    void OnGUI() {
        if (!isInit) {
            CreateDirectories();
            isInit = true;
        }
        SetupStyles();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.Label(tex, headerStyle);
        if (isInspector) {
            ShowTestScreen();
        } else {
            switch (currentScreen) {
                case ScreenType.StartScreen:
                    ShowStartScreen();
                    break;
                case ScreenType.ScriptScreen:
                    ShowScriptScreen();
                    break;
                case ScreenType.BuildScreen:
                    ShowBuildScreen();
                    break;
                case ScreenType.AfterBuildScreen:
                    ShowAfterBuildScreen();
                    break;
                case ScreenType.None:
                    ShowNoneScreen();
                    break;
            }
        }
        GUILayout.EndScrollView();
    }

    void Reset() {
        isInit = false;
        isWarningsInit = false;
    }

    void ShowNoneScreen() {
        if (GUILayout.Button("Back to Start", centerbuttonStyle)) {
            Reset();
            currentScreen = ScreenType.StartScreen;
        }
    }

    void ShowBuildScreen() {
        if (Selection.activeGameObject == null) {
            Reset();
            currentScreen = ScreenType.StartScreen;
            return;
        }
        GUILayout.Label("SideQuest Build", headerStyle);
        GUILayout.Label("Select an output directory for the app.", bodyStyle);
        GUILayout.Space(20);
        outputDirectory = EditorGUILayout.TextField("Output Directory", outputDirectory, inputStyle);
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        isDryRun = GUILayout.Toggle(isDryRun, "Dry run ( skip mobile build )");
        if (GUILayout.Button("Build SideQuest App", buttonStyle, GUILayout.MaxWidth(200))) {
            BuildAllAssetBundles();
            RefreshAssetBundles();
            currentScreen = ScreenType.AfterBuildScreen;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Back", buttonStyle, GUILayout.MaxWidth(100))) {
            RefreshScripts();
            currentScreen = ScreenType.ScriptScreen;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    void ShowAfterBuildScreen() {
        if (Selection.activeGameObject == null) {
            Reset();
            currentScreen = ScreenType.StartScreen;
            return;
        }
        GUILayout.Label("App Build Completed!", headerStyle);
        GUILayout.Label("Select an app from the list to test it.", bodyStyle);
        GUILayout.Space(20);
        if (bundles.Count > 0) {
            foreach (string fileName in bundles) {
                if (GUILayout.Button("Run " + Path.GetFileName(fileName), wideButtonStyle)) {
                    RunBundle(fileName);
                }
            }
            GUILayout.Space(20);
        } else {
            GUILayout.Label("No apps in the output folder: " + outputDirectory, bodyStyle);
        }
        if (GUILayout.Button("Back", centerbuttonStyle)) {
            Reset();
            currentScreen = ScreenType.StartScreen;
        }
    }

    void ShowTestScreen() {
        GUILayout.Label("SideQuest Inspector", headerStyle);
        GUILayout.Label("Select an app from the list to start the test.", bodyStyle);
        GUILayout.Space(20);
        if (GUILayout.Button("Select SideQuest App", centerbuttonStyle)) {
            Reset();
            string path = EditorUtility.OpenFilePanel("Select SideQuest App", "", "sqa");
            if (path.Length != 0) {
                bundles.Add(path);
                RunBundle(path);
            }
        }
        if (bundles.Count > 0) {
            GUILayout.Space(20);
            foreach (string fileName in bundles) {
                if (GUILayout.Button("Run " + Path.GetFileName(fileName), wideButtonStyle)) {
                    RunBundle(fileName);
                }
            }
            GUILayout.Space(20);
        }
    }

    void ShowScriptScreen() {
        if (Selection.activeGameObject == null) {
            Reset();
            currentScreen = ScreenType.StartScreen;
            return;
        }
        GUILayout.Label(Selection.activeGameObject.name+" Scripts & Assets", headerStyle);
        GUILayout.Space(20);
        GUILayout.Label("Pick the scripts to include with your app.", bodyStyle);
        if (scripts.Count > 0) {
            GUILayout.BeginVertical("box");
            foreach (var x in scripts) {
                string name = x.Key;
                bool value = (bool)x.Value;
                if(GUILayout.Toggle(value, name)) {
                    scripts[name] = new JValue(true);
                } else {
                    scripts[name] = new JValue(false);
                }
            }
            GUILayout.EndVertical();
            GUILayout.Space(20);
        } else {
            GUILayout.Label("No scripts in the current project.", bodyStyle);
        }
        GUILayout.Space(20);
        GUILayout.Label("Pick the assets to include with your app.", bodyStyle);
        if (assets.Count > 0) {
            GUILayout.BeginVertical("box");
            foreach (var x in assets) {
                string name = x.Key;
                bool value = (bool)x.Value;
                if (GUILayout.Toggle(value, name)) {
                    assets[name] = new JValue(true);
                } else {
                    assets[name] = new JValue(false);
                }
            }
            GUILayout.EndVertical();
            GUILayout.Space(20);
        } else {
            GUILayout.Label("No assets in the current project.", bodyStyle);
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Back", buttonStyle, GUILayout.MaxWidth(100))) {
            Reset();
            currentScreen = ScreenType.StartScreen;
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Next", buttonStyle, GUILayout.MaxWidth(100))) {
            currentScreen = ScreenType.BuildScreen;
        }
        GUILayout.EndHorizontal();
    }

    void ShowStartScreen() {
        GUILayout.Label("SideQuest Legends SDK", headerStyle);
        GUILayout.Label("Select an object in the hierarchy and click Build to make a SideQuest Legends App. \n\nWARNING:\n Items in the output folder may be overwritten!", bodyStyle);
        GUILayout.Space(20);
        GUI.enabled = Selection.activeGameObject != null;
        if (!GUI.enabled) {
            GUILayout.Label("Select a GameObject in the hierarchy to start...", bodyStyle);
            isWarningsInit = false;
        }
        if (GUI.enabled) {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical("box");
            GUILayout.Label("Selected: "+Selection.activeGameObject.name, bodyStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        if (GUI.enabled) {
            if (!isWarningsInit) {
                CheckWarnings();
                isWarningsInit = true;
            }
            GUILayout.Space(20);
            string allCode = "";
            for (var i = 0; i < warnings.Count; i++) {
                JObject x = (JObject)warnings[i];
                if (i > showFlags.Count - 1) {
                    showFlags.Add(false);
                }
                showFlags[i] = EditorGUILayout.Foldout(showFlags[i], "WARN: " + x["Type"] + " script should be in the Start method of an AppBehaviour! ");
                string path = (string)x["path"];
                string rootPath = BuildReferences.GetGameObjectPath(Selection.activeGameObject.transform);
                if (path.StartsWith(rootPath) && path != rootPath) {
                    path = path.Substring(rootPath.Length + 1);
                }
                string code = path == rootPath? "gameObject.AddComponent<" + x["Type"] + ">();" : "transform.Find(\"" + path + "\").gameObject.AddComponent<" + x["Type"] + ">();";
                allCode += code + "\n";
                if (showFlags[i]) {
                    GUILayout.BeginVertical("box");
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    GUILayout.Label(x["Type"] + " - " + x["path"], bodyStyle);
                    GUILayout.Space(10);
                    GUILayout.Label("Add this code to the Start method of your AppBehaviour to setup for runtime.", codeStyle);
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Copy Code", buttonStyle, GUILayout.MaxWidth(100))) {
                        te.text = code;
                        te.SelectAll();
                        te.Copy();
                        showFlags[i] = false;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical("box");
                    GUILayout.Label(code, codeStyle);
                    GUILayout.EndVertical();

                    GUILayout.Label("You should then remove this script from the object. ", codeStyle);
                    GUILayout.EndVertical();
                }
            }
            if (warnings.Count > 0) {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Copy All", buttonStyle, GUILayout.MaxWidth(100))) {
                    te.text = allCode;
                    te.SelectAll();
                    te.Copy();
                    for (var i = 0; i < showFlags.Count; i++) {
                        showFlags[i] = false;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Next", buttonStyle, GUILayout.MaxWidth(100))) {
            RefreshScripts();
            currentScreen = ScreenType.ScriptScreen;
        }
        GUILayout.EndHorizontal();
        GUI.enabled = true;
    }

    void CheckWarnings() {
        warnings = BuildReferences.Scan(Selection.activeGameObject);
    }

    void Throttle(Action callback) {
        if (Time.realtimeSinceStartup > _nextPosUpdate) {
            _nextPosUpdate = Time.realtimeSinceStartup + POSITION_UPDATE_INTERVAL;
            callback();
        }
    }

    void RefreshAssetBundles() {
        //bundles = new List<string>();
        //foreach (string fileName in Directory.GetFiles(outputDirectory)) {
        //    if (fileName.EndsWith(".sqa")) {
        //        bundles.Add(fileName);
        //    }
        //}
    }

    void RefreshScripts() {
        scripts = new JObject();
        AppBehaviour mainApp = Selection.activeGameObject.GetComponent<AppBehaviour>();
        List<string> active_scripts = new List<string>();
        if (mainApp != null) {
            Loader loader = new Loader();
            string appBehaviourPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(mainApp));
            loader.CompileSources(
                new string[] { File.ReadAllText(appBehaviourPath) },
                loader.assemblyReferences
            );
            AssemblyDefinition defenition = null;
            using (MemoryStream stream = new MemoryStream(loader.assemblyData)) {
                defenition = AssemblyDefinition.ReadAssembly(stream);
            }
            active_scripts.Add(appBehaviourPath);
            foreach (ModuleDefinition module in defenition.Modules) {
                IEnumerable<TypeReference> references = module.GetTypeReferences();
                foreach (TypeReference reference in references) {
                    if (!reference.Namespace.StartsWith("Unity") && !reference.Namespace.StartsWith("System")) {
                        var so = CreateInstance(Type.GetType(reference.FullName + ", " + mainApp.GetType().Assembly.FullName));
                        var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(so));
                        active_scripts.Add(path);
                    }
                }
            }
        }
        foreach (string assetPath in AssetDatabase.GetAllAssetPaths()) {
            if (assetPath.StartsWith("Assets/") &&
                !assetPath.StartsWith("Assets/SideQuest")) {
                var ext = Path.GetExtension(assetPath);
                var isAsset = new string[] {
                    ".mat",".shader",
                    ".jpg",".jpeg",".exr",".png",
                    ".wav",".mp3",".ogg"
                }.Contains(ext);
                if (ext == ".cs" && mainApp != null) {
                    scripts[assetPath] = new JValue(active_scripts.Contains(assetPath));
                } else if (isAsset) {
                    assets[assetPath] = new JValue(false);
                }
            }
        }
    }

    void BuildAllAssetBundles() {
        string[] paths = new string[Selection.activeGameObject.transform.childCount];
        string localPath = inputDirectory + "/" + Selection.activeGameObject.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        for (int i = 0; i < Selection.activeGameObject.transform.childCount; i++) {
            GameObject child = Selection.activeGameObject.transform.GetChild(i).gameObject;
            paths[i] = inputDirectory + "/" + child.name + ".prefab";
            paths[i] = AssetDatabase.GenerateUniqueAssetPath(paths[i]);
            PrefabUtility.CreatePrefab(paths[i], child);
        }
        foreach (var x in scripts) {
            if ((bool)x.Value) {
                File.Copy(x.Key, x.Key + ".txt");
            }
        }
        if (scripts.Count > 0) {
            AssetDatabase.Refresh();
        }
        BuildBundle(BuildTarget.StandaloneWindows, paths);
        if (!isDryRun) {
            BuildBundle(BuildTarget.Android, paths);
        }
        for (int i = 0; i < paths.Length; i++) {
            File.Delete(paths[i]);
            File.Delete(paths[i] + ".meta");
        }
        foreach (var x in scripts) {
            if ((bool)x.Value) {
                File.Delete(x.Key + ".txt.meta");
                File.Delete(x.Key + ".txt");
            }
        }
        if (scripts.Count > 0) {
            AssetDatabase.Refresh();
        }
        string[] newFileEntries = Directory.GetFiles(outputDirectory);
        foreach (string fileName in newFileEntries) {
            if (fileName.Substring(fileName.Length - 9) == ".manifest" ||
                Path.GetFileName(fileName) == "Output" ||
                Path.GetFileName(fileName) == "Output.meta" ||
                fileName.Substring(fileName.Length - 14) == ".manifest.meta") {
                File.Delete(fileName);
            }
        }
        AssetDatabase.Refresh();
    }

    void MarkAsset(string fileName, string bundlePath) {
        AssetImporter asset = AssetImporter.GetAtPath(fileName);
        if (asset != null) {
            asset.SetAssetBundleNameAndVariant(bundlePath, "");
        }
    }

    void BuildBundle(BuildTarget target, string[] prefabPaths) {
        string bundlePath = Selection.activeGameObject.name + ".sqa" + (target == BuildTarget.StandaloneWindows ? "" : "_android");
        bundlePath = AssetDatabase.GenerateUniqueAssetPath(bundlePath);
        foreach(string path in prefabPaths) {
            MarkAsset(path, bundlePath);
        }
        foreach (var x in scripts) {
            if ((bool)x.Value) {
                MarkAsset(x.Key + ".txt", bundlePath);
            }
        }
        foreach (var x in assets) {
            if ((bool)x.Value) {
                MarkAsset(x.Key, bundlePath);
            }
        }
        if (!Directory.Exists(outputDirectory)) {
            Directory.CreateDirectory(outputDirectory);
        }
        BuildPipeline.BuildAssetBundles(outputDirectory, BuildAssetBundleOptions.None, target);
    }

    void RunBundle(string filePath) {
        //HideAssets();
        bundleFilePath = filePath;
        saveScene = SceneManager.GetActiveScene().path;
        EditorSceneManager.OpenScene("Assets/SideQuest/Scenes/TestScene.unity",OpenSceneMode.Single);
        EditorApplication.ExecuteMenuItem("Edit/Play");
        if (!EditorApplication.isPlaying) {
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }
    }

    void RunBundleRuntime(string filePath) {
        UnityEngine.Object Cockpit = AssetDatabase.LoadAssetAtPath("Assets/SideQuest/Prefabs/Cockpit.prefab", typeof(GameObject));
        GameObject cockpit = (GameObject)Instantiate(Cockpit);
        cockpit.name = "Cockpit";
        cockpit.AddComponent<SDK>();
        SetTimeout(() => {
            UnityEngine.Object AvatarContainer = AssetDatabase.LoadAssetAtPath("Assets/SideQuest/Prefabs/AvatarContainer.prefab", typeof(GameObject));
            ((GameObject)Instantiate(AvatarContainer)).name = "AvatarContainer";
            UnityEngine.Object SceneParent = AssetDatabase.LoadAssetAtPath("Assets/SideQuest/Prefabs/SceneParent.prefab", typeof(GameObject));
            GameObject sceneParent = (GameObject)Instantiate(SceneParent);
            sceneParent.name = "SceneParent";
            AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
            GameObject container = new GameObject(Path.GetFileNameWithoutExtension(filePath));
            container.transform.SetParent(sceneParent.transform);
            if (bundle != null) {
                string[] assetName = bundle.GetAllAssetNames();
                List<string> scripts = new List<string>();
                List<GameObject> prefab = new List<GameObject>();
                for (int i = 0; i < assetName.Length; i++) {
                    switch (Path.GetExtension(assetName[i])) {
                        case ".txt":
                            AssetBundleRequest script = bundle.LoadAssetAsync(assetName[i]);
                            if (script.asset) {
                                TextAsset _scriptText = (TextAsset)script.asset;
                                scripts.Add(_scriptText.text);
                            }
                            break;
                        case ".fbx":
                            AssetBundleRequest FBXObject = bundle.LoadAssetWithSubAssetsAsync(assetName[i]);
                            prefab.Add((GameObject)FBXObject.asset);
                            break;
                        case ".prefab":
                            AssetBundleRequest Object = bundle.LoadAssetAsync(assetName[i]);
                            prefab.Add((GameObject)Object.asset);
                            break;
                    }
                }
                if (scripts.Count > 0) {
                    Loader loader = new Loader();
                    loader.Load(scripts);
                    loader.AddScript<AppBehaviour>(container);
                }
                for (int i = 0; i < prefab.Count; i++) {
                    var child = GameObject.Instantiate(prefab[i]);
                    child.name = prefab[i].name;
                    child.transform.SetParent(container.transform, false);
                }
            }

            SetTimeout(() => EditorApplication.LockReloadAssemblies(), 500);
        }, 1000);
    }

}