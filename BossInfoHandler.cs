using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using InUCS.Addons;
using System.Linq;
using System.ComponentModel.Design;
using System.Collections.Generic;

namespace EnemyAIViewer;

public class BossInfoHandler
{

    EnemyAIViewerManager manager;
    List<string> bossNames;
    public List<GameObject> bosses;
    List<HealthManager> hms;
    List<PlayMakerFSM> fsmList;

    public BossInfoHandler(EnemyAIViewerManager manager, GameObject boss)
    {
        this.manager = manager;

        this.bosses = new List<GameObject>();
        this.bossNames = new List<string>();
        this.hms = new List<HealthManager>();
        this.fsmList = new List<PlayMakerFSM>();

        this.manager.Logger.Info($"Creating Boss Info class for {boss.name}");

        BossMapping bm = BossStore.GetBossMappingIfExists(boss.name);
        if (bm != null)
        {
            // add boss object, name, and, hm 
            for (int i = 0; i < bm.bossNames.Count(); i++)
            {
                GameObject obj = GameObject.Find(bm.bossNames[i]);
                if (obj != null)
                {
                    this.bossNames.Add(bm.bossNames[i]);
                    this.bosses.Add(obj);
                    this.hms.Add(obj.GetComponent("HealthManager") as HealthManager);
                }
            }

            // add fsms
            for (int i = 0; i < bm.entityNames.Count(); i++) {
                string fsmName = bm.entityFSMs[i];
                GameObject obj = GameObject.Find(bm.entityNames[i]);
                if (obj != null)
                {
                    this.fsmList.Add(obj.LocateMyFSM(fsmName));
                }
            }
        }
        else
        {
            this.bosses.Add(boss);
            this.bossNames.Add(boss.name);
            this.hms.Add(boss.GetComponent("HealthManager") as HealthManager);
            this.fsmList.Add(boss.LocateMyFSM("Control"));
        }
    }

    public bool HasActiveBosses()
    {
        foreach (GameObject boss in this.bosses)
        {
            if (boss is not null)
            {
                if (boss.activeSelf || boss.activeInHierarchy)
                {
                    return true;
                }
            }
        }

        // none actiive!
        return false;
    }
    public string GetInfo()
    {
        MutableString info = new MutableString(500, true);

        foreach (HealthManager hm in hms)
        {
            if (hm.hp > 0)
            {
                break;
            }
            return "YOU DEFEATED"; // all are zero
        }

        info.Append(this.GetLogisticInfo()).Append("\n");
        info.Append(this.GetAttackInfo());

        return info.Finalize();
    }

    public string GetLogisticInfo()
    {
        string info = "";
        
        // multiple bosses? less detailed info
        if (this.bosses.Count() > 1)
        {
            foreach (HealthManager hm in this.hms)
            {
                info += $"{hm.gameObject.name}: {hm.hp} HP\n";
            }
            return info;
        }

        // one boss? detailed info
        GameObject boss = this.bosses[0];

        // standalone
        string dir = boss.transform.GetScaleX() == -1 ? "Left" : "Right";
       
        info += $"{this.bossNames[0]} \n\n";
        
        info += $"HP: {hms[0].hp}\n";
        info += $"x: {boss.transform.position.x:F2}, y: {boss.transform.position.y:F2}\n";
        info += $"Facing: {dir}\n";

        // relative to hornet
        GameObject hero = GameObject.Find("Hero_Hornet(Clone)");
        float xDist = boss.transform.position.x - hero.transform.position.x;
        float yDist = boss.transform.position.y - hero.transform.position.y;
        bool facingHero = xDist > 0 ? dir == "Left" : dir == "Right";

        info += $"\nDistance to Hornet: \n";
        info += $"x: {xDist:F2}, y: {yDist:F2}\n";
        info += $"Facing Hornet? {facingHero}\n";

        return info;
    }

    public FsmState TransitionViaEvent(FsmState fromState, FsmEvent trigger)
    {
        foreach (FsmTransition t in fromState.Transitions)
        {
            if (t.EventName == trigger.Name)
            {
                return t.ToFsmState;
            }
        }

        return null;
    }

    public string GetAttackInfo()
    {
        string info = "";

        for (int i = 0; i < this.fsmList.Count(); i++)
        {
            string entityName = this.fsmList[i].name;
            string fsmName = this.fsmList[i].FsmName;
            FsmState activeState = this.fsmList[i].Fsm.ActiveState;

            info += $"{entityName} [{fsmName}]: {activeState.Name}\n";
        }

        return info;
    }
}

        // // bool nextChoiceFound = false;
        // int statesChecked = 0;
        // FsmState candidateState = activeState;
        // while (!nextChoiceFound && (statesChecked < this.fsm.FsmStates.Count()))
        // {
        //     break;
        //     if (candidateState.Transitions.Count() == 0)
        //     {
        //         break; // dead end
        //     }
        //     else if (candidateState.Transitions.Count() == 1)
        //     {
        //         // only one next choice
        //         candidateState = candidateState.Transitions[0].ToFsmState;
        //         statesChecked++;
        //     }
        //     else if (candidateState.Transitions.Count() >= 2)
        //     {
        //         foreach (FsmStateAction a in candidateState.Actions)
        //         {
        //             if (!a.Enabled)
        //             {
        //                 continue;
        //             }

        //             if (a is CompareHP)
        //             {
        //                 // determine which path to take from here
        //                 // if none of these hit, we keep looking at this State's actions
        //                 CompareHP b = a as CompareHP;

        //                 if ((this.hm.hp == b.integer2.Value) && (b.equal is not null))
        //                 {
        //                     candidateState = this.TransitionViaEvent(candidateState, b.equal);
        //                     continue;
        //                 }
        //                 else if ((this.hm.hp < b.integer2.Value) && (b.lessThan is not null))
        //                 {
        //                     candidateState = this.TransitionViaEvent(candidateState, b.lessThan);
        //                     continue;
        //                 }
        //                 else if ((this.hm.hp > b.integer2.Value) && (b.greaterThan is not null))
        //                 {
        //                     candidateState = this.TransitionViaEvent(candidateState, b.greaterThan);
        //                     continue;
        //                 }
        //             }

        //             if (a is BoolTest)
        //             {
        //                 BoolTest b = a as BoolTest;
        //                 if ((b.boolVariable.Value) && (b.isTrue is not null))
        //                 {
        //                     candidateState = this.TransitionViaEvent(candidateState, b.isTrue);
        //                     continue;
        //                 }
        //                 else if ((!b.boolVariable.Value) && (b.isFalse is not null))
        //                 {
        //                     candidateState = this.TransitionViaEvent(candidateState, b.isFalse);
        //                     continue;
        //                 }
        //             }

        //             if (a is SendRandomEventV2)
        //             {
        //                 nextChoiceFound = true;
        //                 break;
        //             }
        //         }
        //     }
        // }

        // FsmState nextStateWithChoice = candidateState;

        // if (nextChoiceFound)
        // {
        //     info += $"Next State w/ Choice: {nextStateWithChoice.Name}\n";
        // }


// FsmStateAction a;
// FsmState s;

// MutableString info = new MutableString(500);

// for (int i = 0; i < fsm.FsmStates.Count(); i++)
// {

//     s = fsm.FsmStates[i];

//     if (s.Transitions.Count() < 2)
//     {
//         continue;
//     }

//     info.Append($"[--- {s.Name} ---]\n");
//     for (int j = 0; j < s.actions.Count(); j++)
//     {

//         a = s.actions[j];
//         if (!a.enabled)
//         {
//             continue;
//         }

//         if (a is BoolTest)
//         {
//             BoolTest b = a as BoolTest;
//             info.Append($"Is {b.boolVariable.Name} [{b.boolVariable}]?\n");
//             if (b.isTrue != null)
//             {
//                 string log_str = $" - True: {b.isTrue.Name}";

//                 foreach (FsmTransition t in s.Transitions)
//                 {
//                     if (t.fsmEvent == b.isTrue)
//                     {
//                         Log($"{log_str} --> {t.toState}");
//                     }
//                 }
//             }
//             if (b.isFalse != null)
//             {
//                 string log_str = $" - False: {b.isFalse.Name}";
//                 foreach (FsmTransition t in s.Transitions)
//                 {
//                     if (t.fsmEvent == b.isFalse)
//                     {
//                         info.Append($"{log_str} --> {t.toState}");
//                     }
//                 }
//             }
//             info.Append("\n");
//         }
//         else if (a is CompareHP)
//         {
//             CompareHP b = a as CompareHP;
//             info.Append($"Is HP < {b.integer2}? [Current: {b.hp}]\n");
//             if (b.lessThan != null)
//             {
//                 string log_str = $" - True: {b.lessThan.Name}";

//                 foreach (FsmTransition t in s.Transitions)
//                 {
//                     if (t.fsmEvent == b.lessThan)
//                     {
//                         info.Append($"{log_str} --> {t.toState}");
//                     }
//                 }
//             }
//             if (b.greaterThan != null)
//             {
//                 string log_str = $" - False: {b.greaterThan.Name}";

//                 foreach (FsmTransition t in s.Transitions)
//                 {
//                     if (t.fsmEvent == b.greaterThan)
//                     {
//                         info.Append($"{log_str} --> {t.toState}");
//                     }
//                 }
//             }
//         }
//         else if (a is SendRandomEvent)
//         {
//             info.Append("Random Choice:\n");
//             SendRandomEvent b = a as SendRandomEvent;
//             float totalWeight = 0f;
//             foreach (FsmFloat ff in b.weights)
//             {
//                 totalWeight += ff.Value;
//             }

//             for (int r = 0; r < b.events.Count(); r++)
//             {
//                 string log_str = $"{b.weights[r].Value / totalWeight * 100}%: {b.events[r].Name}";
//                 foreach (FsmTransition t in s.Transitions)
//                 {
//                     if (t.fsmEvent == b.events[r])
//                     {
//                         Log($"{log_str} --> {t.toState}");
//                     }
//                 }
//             }
//         }
//         else if (a is SendRandomEventV2)
//         {
//             info.Append("Random Choice [max X in a row]:");
//             SendRandomEventV2 b = a as SendRandomEventV2;
//             float totalWeight = 0f;
//             foreach (FsmFloat ff in b.weights)
//             {
//                 totalWeight += ff.Value;
//             }

//             for (int r = 0; r < b.events.Count(); r++)
//             {
//                 string log_str = $"{b.weights[r].Value / totalWeight * 100}%: {b.events[r].Name}";
//                 foreach (FsmTransition t in s.Transitions)
//                 {
//                     if (t.fsmEvent == b.events[r])
//                     {

//                     }
//                 }
//                 info.Append([In a row: { b.trackingInts[r].Value}/{ b.eventMax[r].Value}]");
//             }
//         }
//         else
//         {
//             Log(a);
//         }
//     }
// }
