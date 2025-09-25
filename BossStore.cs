
using System.Collections.Generic;

namespace EnemyAIViewer;

public class BossMapping
{
    public string bossName;
    public string[] entityNames;
    public string[] entityFSMs;

    public BossMapping(string bossName, string[] entityNames, string[] entityFSMs)
    {
        this.bossName = bossName;
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
            if (bm.bossName == bossName)
            {
                return bm;
            }
        }
        return null;
    }

    public static List<BossMapping> BossesWithMultipleFSMs = new List<BossMapping> {
        new BossMapping(
            "Silk Boss",
            ["Silk Boss",      "Silk Boss", "Hand L", "Hand R"],
            ["Attack Control", "Control", "Hand Control", "Hand Control"]
        )
        };

    // anything that's not "Boss Scene"
    public static List<string> BossesWithUnusualSceneNames = [
        "Mossbone Mother",
        "Vampire Gnat"
    ];
}
