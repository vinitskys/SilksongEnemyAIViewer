
using System.Collections.Generic;

namespace EnemyAIViewer;

public class EnemyStore
{
    public static List<string> nonactiveActiveStates = [
        "Fly In Ready" // HHG pre-spawn
    ];

    public static List<string> enemiesToIgnore = [
        "MossBone Cocoon"
    ];

    public static Dictionary<string, string> enemiesWithUniqueFSMs = new Dictionary<string, string> {
        {"Song Pilgrim", "Attack"},
        {"MossBone Crawler", "Noise Reaction"},
        {"MossBone Crawler Summon", "Noise Reaction"}

    };

    public static Dictionary<string, List<string>> enemiesWithMultipleFSMs = new Dictionary<string, List<string>> {
        {"Silk Boss", ["Attack Control", "Control"]}
    };

    public static List<string> BossNameList = [
        "Bone Beast",
        "Vampire Gnat",
        "Silk Boss"
    ];
}
