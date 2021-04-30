using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Quests
{
	public class LocationDef : Def
	{
		public FactionDef factionBase;

		public string folderWithPresets;

		public string filePreset;

        public bool disableCenterCellOffset;

        public FloatRange? percentOfDamagedWalls;

        public FloatRange? percentOfDestroyedWalls;

        public FloatRange? percentOfDamagedFurnitures;
        public override void PostLoad()
        {
            base.PostLoad();
        }
    }
}

