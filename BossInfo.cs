using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using InUCS.Addons;
using System.Linq;
using System.ComponentModel.Design;

namespace EnemyAIViewer;

public class BossInfo
{

    EnemyAIViewerManager manager;
    string bossName;
    GameObject boss;
    HealthManager hm;
    PlayMakerFSM fsm;
    Logger logger = new Logger();

    int maxHp;

    public BossInfo(EnemyAIViewerManager manager, HealthManager hm)
    {
        this.manager = manager;
        this.hm = hm;

        this.bossName = hm.gameObject.name;
        this.boss = hm.gameObject;
        this.fsm = this.boss.LocateMyFSM("Control");

        this.maxHp = this.hm.hp;
    }

    public string GetInfo()
    { 
        MutableString info = new MutableString(500, true);

        {
        if (this.hm.hp == 0)
            return "ENEMY SLAIN";
        }

        info.Append($"{this.boss.name}: \n\n");

        info.Append(this.GetLogisticInfo()).Append("\n\n");
        info.Append(this.GetAttackInfo());

        return info.Finalize();
    }

    public string GetLogisticInfo()
    {
        string info = "";

        string dir = this.boss.transform.GetScaleX() == -1 ? "Left" : "Right";

        info += $"HP: {this.hm.hp} / {this.maxHp}\n";
        info += $"x: {this.boss.transform.position.x:F2}, y: {this.boss.transform.position.y:F2}\n";
        info += $"Facing: {dir}";

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

        FsmState activeState = this.fsm.Fsm.ActiveState;

        info += $"AI State: {activeState.Name}\n";

        bool nextChoiceFound = false;
        int statesChecked = 0;
        FsmState candidateState = activeState;
        while (!nextChoiceFound && (statesChecked < this.fsm.FsmStates.Count()))
        {
            break;
            if (candidateState.Transitions.Count() == 0)
            {
                break; // dead end
            }
            else if (candidateState.Transitions.Count() == 1)
            {
                // only one next choice
                candidateState = candidateState.Transitions[0].ToFsmState;
                statesChecked++;
            }
            else if (candidateState.Transitions.Count() >= 2)
            {
                foreach (FsmStateAction a in candidateState.Actions)
                {
                    if (!a.Enabled)
                    {
                        continue;
                    }

                    if (a is CompareHP)
                    {
                        // determine which path to take from here
                        // if none of these hit, we keep looking at this State's actions
                        CompareHP b = a as CompareHP;

                        if ((this.hm.hp == b.integer2.Value) && (b.equal is not null))
                        {
                            candidateState = this.TransitionViaEvent(candidateState, b.equal);
                            continue;
                        }
                        else if ((this.hm.hp < b.integer2.Value) && (b.lessThan is not null))
                        {
                            candidateState = this.TransitionViaEvent(candidateState, b.lessThan);
                            continue;
                        }
                        else if ((this.hm.hp > b.integer2.Value) && (b.greaterThan is not null))
                        {
                            candidateState = this.TransitionViaEvent(candidateState, b.greaterThan);
                            continue;
                        }
                    }

                    if (a is BoolTest)
                    {
                        BoolTest b = a as BoolTest;
                        if ((b.boolVariable.Value) && (b.isTrue is not null))
                        {
                            candidateState = this.TransitionViaEvent(candidateState, b.isTrue);
                            continue;
                        }
                        else if ((!b.boolVariable.Value) && (b.isFalse is not null))
                        {
                            candidateState = this.TransitionViaEvent(candidateState, b.isFalse);
                            continue;
                        }
                    }

                    if (a is SendRandomEventV2)
                    {
                        nextChoiceFound = true;
                        break;
                    }
                }
            }
        }

        FsmState nextStateWithChoice = candidateState;

        if (nextChoiceFound)
        {
            info += $"Next State w/ Choice: {nextStateWithChoice.Name}\n";
        }


        return info;
    }
}

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
