using InUCS;
using BepInEx.Configuration;
using UnityEngine.SceneManagement;

namespace EnemyAIViewer;
public class EnemyAIViewerManager : ComponentManager
{
    public event ActiveSceneChanged ActiveSceneChanged;
    public event SceneLoaded SceneLoaded;

    public GlobalConfig GlobalConfig { get; private set; }
    public ConfigFile BepinConf
    {
        get
        {
            return EnemyAIViewer.PersistentConfig;
        }
    }

    public string activeScene;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        base.Logger.Message($"AI: Scene loaded: {scene.name}");
        if (this.SceneLoaded != null)
        {
            this.SceneLoaded(scene, mode);
        }
    }

    private void OnActiveSceneChanged(Scene from, Scene to)
    {
        base.Logger.Message("AI: Scene change: " + from.name + " -> " + to.name);

        this.activeScene = to.name;

        if (this.ActiveSceneChanged != null)
        {
            this.ActiveSceneChanged(from, to);
        }
    }

    private new void OnEnable()
    {
        base.Logger.Message("Manager enabled");
        SceneManager.activeSceneChanged += this.OnActiveSceneChanged;
        SceneManager.sceneLoaded += this.OnSceneLoaded;
        base.OnEnable();
    }

    private new void OnDisable()
    {
        base.Logger.Message("Manager disabled");
        SceneManager.activeSceneChanged -= this.OnActiveSceneChanged;
        SceneManager.sceneLoaded -= this.OnSceneLoaded;
        base.OnDisable();
    }

    private new void OnDestroy()
    {
        base.Logger.Message("Manager destroyed");
        base.OnDestroy();
    }

    protected sealed override void SpawnComponents()
    {
        this.AddComponent(new EnemyAIViewerComponent(this, true, 0));
    }

    private new void Awake()
    {
        this.GlobalConfig = new GlobalConfig(this.BepinConf);
        this.SpawnComponents();
        base.Awake();
    }
}