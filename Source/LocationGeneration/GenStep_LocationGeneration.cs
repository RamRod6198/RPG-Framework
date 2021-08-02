using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Grammar;
using static LocationGeneration.GetOrGenerateMapPatch;

namespace LocationGeneration
{
    public class GenStep_LocationGeneration : GenStep
    {
        public LocationDef locationDef;
        public override int SeedPart => 341641510;
        public override void Generate(Map map, GenStepParams parms)
        {
            var filePreset = SettlementGeneration.GetPresetFor(map.Parent, locationDef);
            if (filePreset != null)
            {
                GetOrGenerateMapPatch.locationData = new LocationData { file = filePreset, locationDef = locationDef };
            }
        }
    }
}