using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterLoadOrderWarning
{
    [StaticConstructorOnStartup]
    public static class BetterLoadOrderWarning
    {
        static BetterLoadOrderWarning() //our constructor
        {
            Log.Message($"[BLOW] Hello from {nameof(BetterLoadOrderWarning)}!");
            var harmony = new Harmony("MM_BetterLoadOrderWarning");
            harmony.PatchAll();
        }
    }
}