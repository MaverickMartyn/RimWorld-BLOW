using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BetterLoadOrderWarning
{
    public class Dialog_ModListConflict : Window
    {
        public TaggedString text;
        private readonly IEnumerable<string> loadedMods;
        private readonly IEnumerable<string> runningMods;
        public string title;

        public string buttonAText;

        public Action buttonAAction;

        public bool buttonADestructive;

        public string buttonBText;

        public Action buttonBAction;

        public string buttonCText;

        public Action buttonCAction;

        public bool buttonCClose = true;

        public float interactionDelay;

        public Action acceptAction;

        public Action cancelAction;

        public Texture2D image;

        private Vector2 scrollPosition = Vector2.zero;

        private float creationRealTime = -1f;

        private const float TitleHeight = 42f;

        protected const float ButtonHeight = 35f;

        public override Vector2 InitialSize => new Vector2(640f, 460f);

        private float TimeUntilInteractive => interactionDelay - (Time.realtimeSinceStartup - creationRealTime);

        private bool InteractionDelayExpired => TimeUntilInteractive <= 0f;

        public static Dialog_ModListConflict CreateConfirmation(TaggedString text, IEnumerable<string> loadedMods, IEnumerable<string> runningMods, Action confirmedAct, bool destructive = false, string title = null, WindowLayer layer = WindowLayer.Dialog)
        {
            return new Dialog_ModListConflict(text, loadedMods, runningMods, "Confirm".Translate(), confirmedAct, "GoBack".Translate(), null, title, destructive, confirmedAct, delegate
            {
            }, layer);
        }

        public Dialog_ModListConflict(TaggedString text, IEnumerable<string> loadedMods, IEnumerable<string> runningMods, string buttonAText = null, Action buttonAAction = null, string buttonBText = null, Action buttonBAction = null, string title = null, bool buttonADestructive = false, Action acceptAction = null, Action cancelAction = null, WindowLayer layer = WindowLayer.Dialog)
        {
            this.text = text;
            this.loadedMods = loadedMods;
            this.runningMods = runningMods;
            this.buttonAText = buttonAText;
            this.buttonAAction = buttonAAction;
            this.buttonADestructive = buttonADestructive;
            this.buttonBText = buttonBText;
            this.buttonBAction = buttonBAction;
            this.title = title;
            this.acceptAction = acceptAction;
            this.cancelAction = cancelAction;
            base.layer = layer;
            if (buttonAText.NullOrEmpty())
            {
                this.buttonAText = "OK".Translate();
            }

            forcePause = true;
            absorbInputAroundWindow = true;
            creationRealTime = RealTime.LastRealTime;
            onlyOneOfTypeAllowed = false;
            bool flag = buttonAAction == null && buttonBAction == null && buttonCAction == null;
            forceCatchAcceptAndCancelEventEvenIfUnfocused = (acceptAction != null || cancelAction != null || flag);
            closeOnAccept = flag;
            closeOnCancel = flag;
        }

        public override void DoWindowContents(Rect inRect)
        {
            float num = inRect.y;
            if (!title.NullOrEmpty())
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(0f, num, inRect.width, 42f), title);
                num += 42f;
            }

            if (image != null)
            {
                float num2 = (float)image.width / (float)image.height;
                float num3 = 270f * num2;
                GUI.DrawTexture(new Rect(inRect.x + (inRect.width - num3) / 2f, num, num3, 270f), (Texture)image);
                num += 280f;
            }

            Text.Font = GameFont.Small;
            Rect outRect = new Rect(inRect.x, num, inRect.width, inRect.height - 35f - 5f - num);
            float width = outRect.width - 16f;

            // Begin new UI Code
            var missingMods = loadedMods.Except(runningMods);
            var newMods = runningMods.Except(loadedMods);
            var textHeight = Text.CalcHeight(text, width);
            Widgets.Label(new Rect(0f, num, width, textHeight), text);
            var runningModsTitle = "BLOWRunningMods".Translate();
            var loadedModsTitle = "BLOWLoadedMods".Translate();
            var runningModsTitleHeight = Text.CalcHeight(runningModsTitle, (width / 2) - 2);
            var loadedModsTitleHeight = Text.CalcHeight(loadedModsTitle, (width / 2) - 2);
            var titlesHeight = runningModsTitleHeight > loadedModsTitleHeight ? runningModsTitleHeight : loadedModsTitleHeight;
            Text.CurFontStyle.fontStyle = FontStyle.Bold;
            Widgets.Label(new Rect(0f, textHeight + 8, (width / 2) - 2, titlesHeight), runningModsTitle);
            Widgets.Label(new Rect((width / 2) + 2, textHeight + 8, (width / 2) - 2, titlesHeight), loadedModsTitle);
            Text.CurFontStyle.fontStyle = FontStyle.Normal;

            var boxHeight = 24f;
            var loadedModsHeight = loadedMods.Count() * (boxHeight + 4);
            var runningModsHeight = runningMods.Count() * (boxHeight + 4);
            Rect viewRect = new Rect(0f, 0f, width, loadedModsHeight > runningModsHeight ? loadedModsHeight : runningModsHeight);
            var scrollViewOutRect = new Rect(0f, textHeight + titlesHeight + 16f, outRect.width, outRect.height - textHeight);
            Widgets.BeginScrollView(scrollViewOutRect, ref scrollPosition, viewRect);
            for (int i = 0; i < runningMods.Count(); i++)
            {
                var modName = runningMods.ElementAt(i);
                var isMissing = missingMods.Contains(modName);
                var isNew = newMods.Contains(modName);
                if (isNew)
                    GUI.color = Color.yellow;
                else if (isMissing)
                    GUI.color = Color.red;
                else
                    GUI.color = Color.white;

                var rect = new Rect(0f, i * (boxHeight + 4), (viewRect.width / 2) - 2, boxHeight);
                Widgets.DrawBoxSolid(rect, new Color(0.32f, 0.32f, 0.32f));
                Widgets.DrawHighlightIfMouseover(rect);
                var innerRect = new Rect(rect.x + 4f, rect.y + 2f, rect.width - 8f, rect.height - 4f);
                Widgets.Label(innerRect, modName);
                if (Mouse.IsOver(rect))
                {
                    if (isNew)
                        TooltipHandler.TipRegion(rect, new TipSignal(() => "BLOWNewMod".Translate(modName), modName.GetHashCode() * 37));
                    else if (isMissing)
                        TooltipHandler.TipRegion(rect, new TipSignal(() => "BLOWMissingMod".Translate(modName), modName.GetHashCode() * 37));
                }
            }
            for (int i = 0; i < loadedMods.Count(); i++)
            {
                var modName = loadedMods.ElementAt(i);
                var isMissing = missingMods.Contains(modName);
                var isNew = newMods.Contains(modName);
                if (isNew)
                    GUI.color = Color.yellow;
                else if (isMissing)
                    GUI.color = Color.red;
                else
                    GUI.color = Color.white;

                var rect = new Rect((viewRect.width / 2) + 2, i * (boxHeight + 4), (viewRect.width / 2) - 2, boxHeight);
                Widgets.DrawBoxSolid(rect, new Color(0.32f, 0.32f, 0.32f));
                Widgets.DrawHighlightIfMouseover(rect);
                var innerRect = new Rect(rect.x + 4f, rect.y + 2f, rect.width - 8f, rect.height - 4f);
                Widgets.Label(innerRect, modName);
                if (Mouse.IsOver(rect))
                {
                    if (isNew)
                        TooltipHandler.TipRegion(rect, new TipSignal(() => "BLOWNewMod".Translate(modName), modName.GetHashCode() * 37));
                    else if (isMissing)
                        TooltipHandler.TipRegion(rect, new TipSignal(() => "BLOWMissingMod".Translate(modName), modName.GetHashCode() * 37));
                }
            }
            // End of new UI
            Widgets.EndScrollView();
            int num4 = buttonCText.NullOrEmpty() ? 2 : 3;
            float num5 = inRect.width / (float)num4;
            float width2 = num5 - 10f;
            if (buttonADestructive)
            {
                GUI.color = new Color(1f, 0.3f, 0.35f);
            }

            string label = InteractionDelayExpired ? buttonAText : (buttonAText + "(" + Mathf.Ceil(TimeUntilInteractive).ToString("F0") + ")");
            if (Widgets.ButtonText(new Rect(num5 * (float)(num4 - 1) + 10f, inRect.height - 35f, width2, 35f), label) && InteractionDelayExpired)
            {
                if (buttonAAction != null)
                {
                    buttonAAction();
                }

                Close();
            }

            GUI.color = Color.white;
            if (buttonBText != null && Widgets.ButtonText(new Rect(0f, inRect.height - 35f, width2, 35f), buttonBText))
            {
                if (buttonBAction != null)
                {
                    buttonBAction();
                }

                Close();
            }

            if (buttonCText != null && Widgets.ButtonText(new Rect(num5, inRect.height - 35f, width2, 35f), buttonCText))
            {
                if (buttonCAction != null)
                {
                    buttonCAction();
                }

                if (buttonCClose)
                {
                    Close();
                }
            }
        }

        public override void OnCancelKeyPressed()
        {
            if (cancelAction != null)
            {
                cancelAction();
                Close();
            }
            else
            {
                base.OnCancelKeyPressed();
            }
        }

        public override void OnAcceptKeyPressed()
        {
            if (acceptAction != null)
            {
                acceptAction();
                Close();
            }
            else
            {
                base.OnAcceptKeyPressed();
            }
        }
    }
}
