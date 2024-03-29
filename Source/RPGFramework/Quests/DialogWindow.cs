﻿using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Quests
{
	public class DialogWindow : Window
	{
		public override Vector2 InitialSize
		{
			get
			{
				int num = 480;
				if (this.curNode.options.Count > 5)
				{
					Text.Font = GameFont.Small;
					num += (this.curNode.options.Count - 5) * (int)(Text.LineHeight + 7f);
				}
				return new Vector2(620f, (float)Mathf.Min(num, UI.screenHeight));
			}
		}

		private bool InteractiveNow
		{
			get
			{
				return Time.realtimeSinceStartup >= this.makeInteractiveAtTime;
			}
		}

		public DialogWindow(DiaNode nodeRoot, bool delayInteractivity = false, 
			bool radioMode = false, string title = null)
		{
			this.title = title;
			this.GotoNode(nodeRoot);
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.closeOnAccept = false;
			this.closeOnCancel = false;
			if (delayInteractivity)
			{
				this.makeInteractiveAtTime = RealTime.LastRealTime + 0.5f;
			}
			this.soundAppear = SoundDefOf.CommsWindow_Open;
			this.soundClose = SoundDefOf.CommsWindow_Close;
			if (radioMode)
			{
				this.soundAmbient = SoundDefOf.RadioComms_Ambience;
			}
		}

		public void StartDialog()
		{
			foreach (var option in curDialog.options)
			{
				var diaOption = new DiaOption(option.text);
				diaOption.action = delegate ()
				{
					foreach (var action in option.actions)
					{
						action.window = this;
						action.DoAction();
					}
				};
				curNode.options.Add(diaOption);
			}
			Find.WindowStack.Add(this);
		}

		public override void PreClose()
		{
			base.PreClose();
			this.curNode.PreClose();
		}

		public override void PostClose()
		{
			base.PostClose();
			if (this.closeAction != null)
			{
				this.closeAction();
			}
		}

		public override void WindowOnGUI()
		{
			if (this.screenFillColor != Color.clear)
			{
				GUI.color = this.screenFillColor;
				GUI.DrawTexture(new Rect(0f, 0f, (float)UI.screenWidth, (float)UI.screenHeight), BaseContent.WhiteTex);
				GUI.color = Color.white;
			}
			base.WindowOnGUI();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = inRect.AtZero();
			if (this.title != null)
			{
				Text.Font = GameFont.Small;
				Rect rect2 = rect;
				rect2.height = 36f;
				rect.yMin += 53f;
				Widgets.DrawTitleBG(rect2);
				rect2.xMin += 9f;
				rect2.yMin += 5f;
				Widgets.Label(rect2, this.title);
			}
			this.DrawNode(rect);
		}

		protected void DrawNode(Rect rect)
		{
			GUI.BeginGroup(rect);
			Text.Font = GameFont.Small;
			float num = Mathf.Min(this.optTotalHeight, rect.height - 100f - this.Margin * 2f);
			Rect outRect = new Rect(0f, 0f, rect.width, rect.height - Mathf.Max(num, this.minOptionsAreaHeight));
			float width = rect.width - 16f;
			Rect rect2 = new Rect(0f, 0f, width, Text.CalcHeight(this.curNode.text, width));
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect2, true);
			Widgets.Label(rect2, this.curNode.text.Resolve());
			Widgets.EndScrollView();
			Widgets.BeginScrollView(new Rect(0f, rect.height - num, rect.width, num), ref this.optsScrollPosition, new Rect(0f, 0f, rect.width - 16f, this.optTotalHeight), true);
			float num2 = 0f;
			float num3 = 0f;
			for (int i = 0; i < this.curNode.options.Count; i++)
			{
				Rect rect3 = new Rect(15f, num2, rect.width - 30f, 999f);
				float num4 = this.curNode.options[i].OptOnGUI(rect3, this.InteractiveNow);
				num2 += num4 + 7f;
				num3 += num4 + 7f;
			}
			if (Event.current.type == EventType.Layout)
			{
				this.optTotalHeight = num3;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
		}

		public void GotoNode(DiaNode node)
		{
			foreach (DiaOption diaOption in node.options)
			{
				diaOption.dialog = this;
			}
			this.curNode = node;
		}

		public Pawn initiator;

		public Pawn talker; 

		public DialogDef curDialog;

		private Vector2 scrollPosition;

		private Vector2 optsScrollPosition;

		protected string title;

		protected DiaNode curNode;

		public Action closeAction;

		private float makeInteractiveAtTime;

		public Color screenFillColor = Color.clear;

		protected float minOptionsAreaHeight;

		private const float InteractivityDelay = 0.5f;

		private const float TitleHeight = 36f;

		protected const float OptHorMargin = 15f;

		protected const float OptVerticalSpace = 7f;

		private const int ResizeIfMoreOptionsThan = 5;

		private const float MinSpaceLeftForTextAfterOptionsResizing = 100f;

		private float optTotalHeight;
	}
}

