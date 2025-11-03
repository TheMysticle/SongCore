using HarmonyLib;
using IPA.Utilities;

namespace SongCore.Utilities
{
    public static class CodeMatcherExtensions
    {
        private static readonly FieldAccessor<CodeMatcher, string>.Accessor LastErrorAccessor =
            FieldAccessor<CodeMatcher, string>.GetAccessor("lastError");

        /// <summary>Prints the list of instructions of this code matcher instance.</summary>
        /// <param name="codeMatcher">The code matcher instance.</param>
        /// <returns>The code matcher instance.</returns>
        public static CodeMatcher PrintInstructions(this CodeMatcher codeMatcher)
        {
            var instructions = codeMatcher.Instructions();
            for (var i = 0; i < instructions.Count; i++)
            {
                Plugin.Log.Info($"\t {i} {instructions[i]}");
            }

            return codeMatcher;
        }
    }
}
