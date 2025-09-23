using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

namespace EnemyAIViewer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
// [BepInDependency("com.bepin.SSDebugkz")]
public class EnemyAIViewer : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static GameObject CurrentObject { get; set; }

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        if (EnemyAIViewer.CurrentObject == null)
        {
            EnemyAIViewer.CurrentObject = new GameObject("EnemyAIViewerPlugin", new Type[] {
                typeof(EnemyAIViewerManager)
            })
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            UnityEngine.Object.DontDestroyOnLoad(EnemyAIViewer.CurrentObject);

            base.Logger.LogInfo("Successfully created.");
        }
    }
}

public delegate void ActiveSceneChanged(Scene from, Scene to);
public delegate void SceneLoaded(Scene scene, LoadSceneMode mode);