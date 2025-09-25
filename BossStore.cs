
using System.Collections.Generic;
using System.Linq;

namespace EnemyAIViewer;

public class BossMapping
{
    public string[] bossNames;
    public string[] entityNames;
    public string[] entityFSMs;

    public BossMapping(string[] bossNames, string[] entityNames, string[] entityFSMs)
    {
        this.bossNames = bossNames;
        this.entityNames = entityNames;
        this.entityFSMs = entityFSMs;
    }
}

public class BossStore
{

    public static BossMapping GetBossMappingIfExists(string bossName)
    {
        foreach (BossMapping bm in BossStore.BossesWithMultipleFSMs)
        {
            if (bm.bossNames.Contains(bossName))
            {
                return bm;
            }
        }
        return null;
    }

    public static List<BossMapping> BossesWithMultipleFSMs = new List<BossMapping> {
        new BossMapping(
            ["Silk Boss"],
            ["Silk Boss",      "Silk Boss", "Hand L",       "Hand R"],
            ["Attack Control", "Control",   "Hand Control", "Hand Control"]
        ),

        new BossMapping(
            ["Dancer A", "Dancer B"],
            ["Dancer Control", "Dancer A", "Dancer B"],
            ["Control", "Control", "Control"]
        )
        };

    // anything that's not "Boss Scene" or "Boss Control"
    public static List<string> BossScenes = [
        "Boss Scene",
        "Boss Control",
        "Dancer Control"
    ];

    // anything that's not "Boss Scene" or "Boss Control"
    public static List<string> BossesWithUnusualSceneNames = [
        "Mossbone Mother",
        "Vampire Gnat"
    ];


}
