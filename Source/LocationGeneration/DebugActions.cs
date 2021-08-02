using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace LocationGeneration
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
            var curModName = LoadedModManager.RunningMods.Where(x => x.assemblies.loadedAssemblies.Contains(Assembly.GetExecutingAssembly())).FirstOrDefault().Name;
            Log.Message("curModName: " + curModName);
            ModMetaData modMetaData = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData x) => x != null && x.Name != null && x.Active && x.Name == curModName);
            string path = Path.GetFullPath(modMetaData.RootDir.ToString() + "/Presets/");
            Log.Message("path: " + path);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            using (IEnumerator<FileInfo> enumerator = directoryInfo.GetFiles().AsEnumerable().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string name = enumerator.Current.Name;
                    list.Add(new DebugMenuOption(name, 0, delegate ()
                    {
                        path = path + name;
                        Map map = Find.CurrentMap;
                        SettlementGeneration.DoSettlementGeneration(map, path, null, Faction.OfPlayer, false);
                    }));
                }
            }
            if (list.Any())
            {
                Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
            }
        }
    }
}

