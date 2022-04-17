using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BetterLoadOrderWarning
{
    [StaticConstructorOnStartup]
    public static class BetterLoadOrderWarning_Patches
    {
        [HarmonyPatch(typeof(ScribeMetaHeaderUtility), "TryCreateDialogsForVersionMismatchWarnings", MethodType.Normal)]
        public static class BetterLoadOrderWarning_LoadGameModWarning
        {
            static bool Prefix(Action confirmedAction, ref bool __result)
            {
                __result = ReplacementMethod(confirmedAction);

                // return false to skip the original
                return false;
            }

            private static bool VersionsMatch()
            {
                return VersionControl.BuildFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion) == VersionControl.BuildFromVersionString(VersionControl.CurrentVersionStringWithRev);
            }

            public static bool ReplacementMethod(Action confirmedAction)
            {
                // Get the private field lastMode
                ScribeMetaHeaderUtility.ScribeHeaderMode lastMode = (ScribeMetaHeaderUtility.ScribeHeaderMode)Traverse.Create(typeof(ScribeMetaHeaderUtility)).Field("lastMode").GetValue();

                string text = null;
                string text2 = null;
                if (!BackCompatibility.IsSaveCompatibleWith(ScribeMetaHeaderUtility.loadedGameVersion) && !VersionsMatch())
                {
                    text2 = "VersionMismatch".Translate();
                    string value = ScribeMetaHeaderUtility.loadedGameVersion.NullOrEmpty() ? ("(" + "UnknownLower".TranslateSimple() + ")") : ScribeMetaHeaderUtility.loadedGameVersion;
                    text = ((lastMode == ScribeMetaHeaderUtility.ScribeHeaderMode.Map) ? ((string)"SaveGameIncompatibleWarningText".Translate(value, VersionControl.CurrentVersionString)) : ((lastMode != ScribeMetaHeaderUtility.ScribeHeaderMode.World) ? ((string)"FileIncompatibleWarning".Translate(value, VersionControl.CurrentVersionString)) : ((string)"WorldFileVersionMismatch".Translate(value, VersionControl.CurrentVersionString))));
                }
                string loadedModsSummary;
                string runningModsSummary;
                bool flag = false;
                if (!ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out loadedModsSummary, out runningModsSummary))
                {
                    flag = true;
                    string text3 = "BLOWModsMismatchWarningText".Translate();
                    text = ((text != null) ? (text + "\n\n" + text3) : text3);
                    if (text2 == null)
                    {
                        text2 = "ModsMismatchWarningTitle".Translate();
                    }
                }

                if (text != null)
                {
                    var splitArr = new string[] { ", " };
                    IEnumerable<string> loadedMods = loadedModsSummary.Split(splitArr, StringSplitOptions.None);
                    IEnumerable<string> runningMods = runningModsSummary.Split(splitArr, StringSplitOptions.None);
                    Dialog_ModListConflict dialog = Dialog_ModListConflict.CreateConfirmation(text, loadedMods, runningMods, confirmedAction, destructive: false, text2);
                    dialog.buttonAText = "LoadAnyway".Translate();
                    if (flag)
                    {
                        dialog.buttonCText = "ChangeLoadedMods".Translate();
                        dialog.buttonCAction = delegate
                        {
                            if (Current.ProgramState == ProgramState.Entry)
                            {
                                ModsConfig.SetActiveToList(ScribeMetaHeaderUtility.loadedModIdsList);
                            }

                            ModsConfig.SaveFromList(ScribeMetaHeaderUtility.loadedModIdsList);
                            IEnumerable<string> enumerable = from id in Enumerable.Range(0, ScribeMetaHeaderUtility.loadedModIdsList.Count)
                                                             where ModLister.GetModWithIdentifier(ScribeMetaHeaderUtility.loadedModIdsList[id]) == null
                                                             select ScribeMetaHeaderUtility.loadedModNamesList[id];
                            if (enumerable.Any())
                            {
                                Messages.Message(string.Format("{0}: {1}", "MissingMods".Translate(), enumerable.ToCommaList()), MessageTypeDefOf.RejectInput, historical: false);
                                dialog.buttonCClose = false;
                            }

                            ModsConfig.RestartFromChangedMods();
                        };
                    }

                    Find.WindowStack.Add(dialog);
                    return true;
                }

                return false;
            }
        }
    }
}
