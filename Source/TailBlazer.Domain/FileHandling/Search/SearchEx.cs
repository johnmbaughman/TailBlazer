using System;
using System.Text.RegularExpressions;

namespace TailBlazer.Domain.FileHandling.Search
{
    public static class SearchEx
    {
        public static Func<string, bool> BuildPredicate(this SearchMetadata source)
        {
            const RegexOptions caseInsensitiveOptions = RegexOptions.IgnorePatternWhitespace
                                                        | RegexOptions.Compiled
                                                        | RegexOptions.IgnoreCase;

            const RegexOptions caseSensitiveOptions = RegexOptions.IgnorePatternWhitespace
                                                      | RegexOptions.Compiled;

            Func<string, bool> predicate;
            if (!source.UseRegex)
            {
                var stringComparison = source.IgnoreCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                predicate = s => s.Contains(source.SearchText, stringComparison);
            }
            else
            {
                var options = source.IgnoreCase ? caseInsensitiveOptions : caseSensitiveOptions;
                var regex = new Regex(source.SearchText, options);
                predicate = s => regex.IsMatch(s);
            }
            return predicate;
        }
    }
}

}