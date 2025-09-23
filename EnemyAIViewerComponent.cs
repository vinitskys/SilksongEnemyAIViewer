using InUCS;
using InUCS.Components;
using HutongGames.PlayMaker;
using InUCS.Addons;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace EnemyAIViewer;
public class EnemyAIViewerComponent : LocalComponent
{
    private bool showUI = true;
    private EnemyAIViewerManager manager;
    private readonly MutableString enemyInfo = new MutableString(3000, true);
    private string enemyInfoStr = string.Empty;

    public Matrix4x4 origMatrix;

    public Matrix4x4 scaledMatrix;

    private List<HealthManager> hmCache = new List<HealthManager>(100);

    public EnemyAIViewerComponent(EnemyAIViewerManager manager, bool enabled, int priority) : base(manager, enabled, priority)
    {
        this.manager = manager;
    }

    public sealed override void OnGUI()
    {
        
        this.origMatrix = GUI.matrix;
        GUI.matrix = this.scaledMatrix;

        if (Event.current.type == EventType.Repaint && (this.showUI) && this.MainContextValid())
        {
            Color contentColor = GUI.contentColor;
            bool wordWrap = GUI.skin.box.wordWrap;
            int fontSize = GUI.skin.label.fontSize;
            FontStyle fontStyle = GUI.skin.label.fontStyle;
            TextAnchor alignment = GUI.skin.label.alignment;
            GUI.contentColor = Color.white;
            GUI.skin.label.wordWrap = false;
            GUI.skin.label.fontSize = 20;
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;

            int lineCount = this.enemyInfoStr.Count(c => c == '\n');

            float boxHeight = (float)(25 * (lineCount));
            float boxWidth = 400f;

            Texture2D boxBG = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            Rect position = new Rect(320f, 10f, boxWidth, boxHeight);
            GUI.DrawTexture(position, boxBG, ScaleMode.StretchToFill, false, 1f, new Color(0f, 0f, 0f, 0.9f), 0f, 0f);
            GUI.Label(position, this.enemyInfoStr);

            GUI.skin.label.wordWrap = wordWrap;
            GUI.skin.label.fontSize = fontSize;
            GUI.skin.label.fontStyle = fontStyle;
            GUI.skin.label.alignment = alignment;
            GUI.contentColor = contentColor;
        }
        GUI.matrix = this.origMatrix;
    }

    public sealed override void Update()
    {

        if (!this.MainContextValid())
        {
            return;
        }

        if (Input.GetKeyDown(this.manager.GlobalConfig.ToggleUIKeybind.Value))
        {
            this.showUI = !this.showUI;

            if (this.showUI)
            {
                ScreenHelper.CalculateMatrixInLine(ref this.scaledMatrix, (float)Screen.width, (float)Screen.height);
            }
        }
        
        this.SampleData();
    }

    private void SampleData()
    {
        this.enemyInfo.Append("Enemy AI Information: \n\n");
        for (int i = 0; i < this.hmCache.Count; i++)
        {
            HealthManager hm = this.hmCache[i];
            if (hm != null && hm.gameObject.activeSelf && hm.gameObject.activeInHierarchy)
            {
                GameObject enemy = hm.gameObject;

                PlayMakerFSM fsm = enemy.LocateMyFSM("Control");

                if (fsm == null){
                    fsm = enemy.LocateMyFSM("Attack"); // Song Pilgram
                }

                if (fsm != null && fsm.Active)
                {

                    // HHG State mobs have before spawning
                    if (fsm.ActiveStateName != "Fly In Ready")
                    {
                        this.enemyInfo.Append($"{enemy.name}: {fsm.ActiveStateName}\n");
                    }
                }
            }
        }

        this.enemyInfoStr = this.enemyInfo.Finalize();
    }

    public override void OnComponentEnable()
    {
        ScreenHelper.CalculateMatrixInLine(ref this.scaledMatrix, (float)Screen.width, (float)Screen.height);

        base.OnComponentEnable();
        this.manager.ActiveSceneChanged += this.ActiveSceneChanged;
        this.manager.SceneLoaded += this.SceneLoaded;

    }

    public override void OnComponentDisable()
    {
        base.OnComponentDisable();
        this.manager.ActiveSceneChanged -= this.ActiveSceneChanged;
        this.manager.SceneLoaded -= this.SceneLoaded;

    }

    public bool MainContextValid()
		{
        return Application.isPlaying &&
        this.manager.activeScene != string.Empty &&
        this.manager.activeScene != "Pre_Menu_Loader" &&
        this.manager.activeScene != "Pre_Menu_Intro" &&
        this.manager.activeScene != "Menu_Title" &&
        this.manager.activeScene != "Quit_To_Menu";
		}

    private void ActiveSceneChanged(Scene from, Scene to)
    {
        base.Logger.Info("HealthManager cache cleared");

        this.hmCache.Clear();
        this.hmCache.AddRange(
            UnityEngine.Object.FindObjectsByType<HealthManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.InstanceID
        ));
    } 

    private void SceneLoaded(Scene from, LoadSceneMode mode)
    {
        this.hmCache.AddRange(
            UnityEngine.Object.FindObjectsByType<HealthManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.InstanceID
        ));
    } 
}