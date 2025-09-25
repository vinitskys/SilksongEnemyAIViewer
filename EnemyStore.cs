
using System.Collections.Generic;

namespace EnemyAIViewer;

public class EnemyStore
{
    public static Dictionary<string, List<string>> enemiesWithMultipleFSMs = new Dictionary<string, List<string>> {
        {"Silk Boss", ["Attack Control", "Control"]}
    };

    public static List<string> BossNameList = [
        "Bone Beast",
        "Lace Boss1",
        "Vampire Gnat",
        "Spinner Boss",
        "Phantom",
        "Dancer Control", // needs work
        "Trobbio",
        "Lace Boss2 New",
        "Silk Boss" //multiple FSMs
    ];
}
