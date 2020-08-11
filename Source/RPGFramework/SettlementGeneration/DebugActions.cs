using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Quests
{
    [StaticConstructorOnStartup]
    public static class Actions
    {
        [DebugAction("General", "Make everything belong to player faction")]
        public static void MakeEverythingPlayerFaction()
        {
            Map map = Find.CurrentMap;
            foreach (var thing in map.listerThings.AllThings)
            {
                if (thing.Faction != null)
                {
                    thing.SetFaction(Faction.OfPlayer);
                }
            }
        }

        [DebugAction("General", "Make blueprint (with pawns)")]
        public static void CreateBlueprint()
        {
            string name = "";
            var dialog = new Dialog_NameBlueprint(name, true);
            Find.WindowStack.Add(dialog);
        }

        [DebugAction("General", "Save everything in the map")]
        public static void SaveEverything()
        {
            string name = "";
            var dialog = new Dialog_SaveEverything(name);
            Find.WindowStack.Add(dialog);
        }

        [DebugAction("General", "Make blueprint (without pawns)")]
        public static void CreateBlueprintWithoutPawns()
        {
            string name = "";
            var dialog = new Dialog_NameBlueprint(name, false);
            Find.WindowStack.Add(dialog);
        }

        [DebugAction("General", "Load blueprint")]
        public static void LoadBlueprint()
        {
            ModMetaData modMetaData = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData x) =>
                x != null && x.Name != null && x.Active && x.Name.StartsWith("RPG Framework"));
            string path = Path.GetFullPath(modMetaData.RootDir.ToString() + "/Presets/");
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            Map map = Find.CurrentMap;

            List<DebugMenuOption> list = new List<DebugMenuOption>();
            using (IEnumerator<FileInfo> enumerator = directoryInfo.GetFiles().AsEnumerable().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string name = enumerator.Current.Name;
                    list.Add(new DebugMenuOption(name, 0, delegate ()
                    {
                        path = path + name;
                        SettlementGeneration.DoSettlementGeneration(map, path, Faction.OfPlayer, false);
                    }));
                }
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }

        [DebugAction("General", "Add quest giver", actionType = DebugActionType.ToolMapForPawns)]
        public static void AddQuestGiver(Pawn p)
        {
            var questComp = Current.Game.GetComponent<QuestTracker>();
            questComp.CreateQuestGiver(p);
        }

        [DebugAction("General", "Add quest giver (with specific quest)", actionType = DebugActionType.ToolMapForPawns)]
        public static void AddQuestGiverSpecifi(Pawn p)
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            using (IEnumerator<QuestScriptDef> enumerator = DefDatabase<QuestScriptDef>.AllDefs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string name = enumerator.Current.defName;
                    list.Add(new DebugMenuOption(name, 0, delegate ()
                    {
                        var specificQuests = new List<QuestScriptDef>();
                        specificQuests.Add(enumerator.Current);
                        var questComp = Current.Game.GetComponent<QuestTracker>();
                        questComp.CreateQuestGiver(p, specificQuests);
                    }));
                }
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }
    }
}

