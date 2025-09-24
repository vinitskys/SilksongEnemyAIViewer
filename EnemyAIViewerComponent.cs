using InUCS;
using InUCS.Components;
using InUCS.Addons;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text.RegularExpressions;

namespace EnemyAIViewer;

public class EnemyAIViewerComponent : LocalComponent
{
    private bool showUI = true;
    private bool isBossRoom = false;
    private BossInfo bossInfo;

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

            float boxHeight = (float)(30 * (lineCount) + 300);
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

        if (this.isBossRoom)
        {
            this.DisplayBossInfo();
        } else {
            this.DisplayNonBossInfo();
        }
    }

    private string cleanSpeciesName(string name)
    {
        name = Regex.Replace(name, @"\(\d+\)", ""); // remove (1) at the end
        name = Regex.Replace(name, @"\d+", ""); // remove 01 at the end

        name = name.Trim();
        return name;
    }

    private void extractFsmInfo(string label, PlayMakerFSM fsm)
    {
        if (fsm != null && fsm.Active)
        {
            if (!EnemyStore.nonactiveActiveStates.Contains(fsm.ActiveStateName))
            {
                this.enemyInfo.Append($"{label}: {fsm.ActiveStateName}\n");
            }
        }
    }

    private void DisplayBossInfo()
    {
        this.enemyInfoStr = this.bossInfo.GetInfo();
    }

    private void DisplayNonBossInfo()
    {
        this.enemyInfo.Append("Enemy AI Information: \n\n");
        for (int i = 0; i < this.hmCache.Count; i++)
        {
            HealthManager hm = this.hmCache[i];
            if (hm != null && hm.gameObject.activeSelf && hm.gameObject.activeInHierarchy)
            {
                GameObject enemy = hm.gameObject;
                string enemySpecies = this.cleanSpeciesName(enemy.name);
                PlayMakerFSM fsm;

                if (EnemyStore.enemiesToIgnore.Contains(enemySpecies))
                {
                    continue;
                }

                if (EnemyStore.enemiesWithMultipleFSMs.ContainsKey(enemySpecies))
                {
                    List<string> fsmNames = EnemyStore.enemiesWithMultipleFSMs[enemySpecies];
                    foreach (string fsmName in fsmNames)
                    {
                        fsm = enemy.LocateMyFSM(fsmName);
                        this.extractFsmInfo(fsmName, fsm);
                    }
                    continue;
                }

                if (EnemyStore.enemiesWithUniqueFSMs.ContainsKey(enemySpecies))
                {
                    fsm = enemy.LocateMyFSM(EnemyStore.enemiesWithUniqueFSMs[enemySpecies]);
                }
                else
                {
                    fsm = enemy.LocateMyFSM("Control");
                }

                this.extractFsmInfo(enemy.name, fsm);
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

    private void setBossSceneStatus()
    {

        foreach (HealthManager hm in this.hmCache)
        {

            if (hm == null)
            {
                continue;
            }

            if (EnemyStore.BossNameList.Contains(hm.gameObject.name))
            {
                this.isBossRoom = true;
                this.bossInfo = new BossInfo(this.manager, hm);

                break;
            }
        }
        
        
    }

    private void ActiveSceneChanged(Scene from, Scene to)
    {
        base.Logger.Info("HealthManager cache cleared");

        this.hmCache.Clear();

        this.hmCache.AddRange(
            UnityEngine.Object.FindObjectsByType<HealthManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.InstanceID
        ));

        this.setBossSceneStatus();
    }

    private void SceneLoaded(Scene from, LoadSceneMode mode)
    {
        HealthManager[] hmList = UnityEngine.Object.FindObjectsByType<HealthManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.InstanceID
        );

        foreach (HealthManager hm in hmList)
        {
            if (!this.hmCache.Contains(hm))
            {
                this.hmCache.Add(hm);
            }
        }
        
        this.setBossSceneStatus();
    }
}