using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Quests
{
    public class MapComponentGeneration : MapComponent
    {
        public MapComponentGeneration(Map map) : base(map)
        {

        }
        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            if (this.DoGeneration && path.Length > 0)
            {
                SettlementGeneration.DoSettlementGeneration(this.map, this.path, this.map.ParentFaction, false);
                this.DoGeneration = false;
            }
            if (this.ReFog)
            {
                Log.Message("Refog" + this.map);
                FloodFillerFog.DebugRefogMap(this.map);
                this.ReFog = false;
            }
        }

        //public override void MapComponentTick()
        //{
        //    base.MapComponentTick();
        //    foreach (var locationDef in DefDatabase<LocationDef>.AllDefs)
        //    {
        //        Log.Message(Path.GetFullPath(locationDef.modContentPack.RootDir + "//" + locationDef.filePreset));
        //    }
        //}

        public void DoForcedGeneration(bool disableFog)
        {
            SettlementGeneration.DoSettlementGeneration(this.map, this.path, this.map.ParentFaction, disableFog);
            this.DoGeneration = false;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.DoGeneration, "DoGeneration", false);
            Scribe_Values.Look<string>(ref this.path, "path", "");
        }

        public bool DoGeneration = false;

        public bool ReFog = false;
        public string path = "";

    }
}

