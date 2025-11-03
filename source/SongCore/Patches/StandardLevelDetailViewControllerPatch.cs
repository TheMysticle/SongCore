using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BGLib.Polyglot;
using HarmonyLib;

namespace SongCore.Patches
{
    /// <summary>
    /// This patch catches all exceptions and displays an error message to the user
    /// in the <see cref="StandardLevelDetailView"/> when the game is loading beatmap levels.
    /// </summary>
    [HarmonyPatch(typeof(StandardLevelDetailViewController), nameof(StandardLevelDetailViewController.ShowLoadingAndDoSomething), MethodType.Async)]
    internal static class StandardLevelDetailViewControllerPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions)
                .MatchStartForward(new CodeMatch(i => i.blocks.FirstOrDefault()?.blockType == ExceptionBlockType.BeginCatchBlock));
            codeMatcher.Instruction.blocks[0].catchType = typeof(Exception);
            return codeMatcher
                .SetOpcodeAndAdvance(OpCodes.Stloc_3)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    Transpilers.EmitDelegate<Action<StandardLevelDetailViewController, Exception>>((standardLevelDetailViewController, ex) =>
                    {
                        if (ex is OperationCanceledException)
                        {
                            return;
                        }

                        standardLevelDetailViewController.ShowContent(StandardLevelDetailViewController.ContentType.Error, Localization.Get(StandardLevelDetailViewController.kLoadingDataErrorLocalizationKey));

                        Plugin.Log.Error(ex);
                    }))
                .InstructionEnumeration();
        }
    }
}
