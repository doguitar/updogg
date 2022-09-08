using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Updogg.Extensions.EntityFramework
{  //
   // Summary:
   //     Provides CLR methods that get translated to database functions when used in LINQ
   //     to Entities queries. The methods on this class are accessed via Microsoft.EntityFrameworkCore.EF.Functions.
    public static class DbFunctionsExtensions
    {
        //
        // Summary:
        //     An implementation of the SQL LIKE operation. On relational databases this is
        //     usually directly translated to SQL.
        //     Note that if this function is translated into SQL, then the semantics of the
        //     comparison will depend on the database configuration. In particular, it may be
        //     either case-sensitive or case-insensitive. If this function is evaluated on the
        //     client, then it will always use a case-insensitive comparison.
        //
        // Parameters:
        //   _:
        //     The DbFunctions instance.
        //
        //   matchExpression:
        //     The string that is to be matched.
        //
        //   pattern:
        //     The pattern which may involve wildcards %,_,[,],^.
        //
        // Returns:
        //     true if there is a match.
        public static bool RLike(this DbFunctions _, string matchExpression, string pattern)
        {
            return RLikeCore(matchExpression, pattern, null);
        }

        //
        // Summary:
        //     An implementation of the SQL LIKE operation. On relational databases this is
        //     usually directly translated to SQL.
        //     Note that if this function is translated into SQL, then the semantics of the
        //     comparison will depend on the database configuration. In particular, it may be
        //     either case-sensitive or case-insensitive. If this function is evaluated on the
        //     client, then it will always use a case-insensitive comparison.
        //
        // Parameters:
        //   _:
        //     The DbFunctions instance.
        //
        //   matchExpression:
        //     The string that is to be matched.
        //
        //   pattern:
        //     The pattern which may involve wildcards %,_,[,],^.
        //
        //   escapeCharacter:
        //     The escape character (as a single character string) to use in front of %,_,[,],^
        //     if they are not used as wildcards.
        //
        // Returns:
        //     true if there is a match.
        public static bool RLike(this DbFunctions _, string matchExpression, string pattern, string escapeCharacter)
        {
            return RLikeCore(matchExpression, pattern, escapeCharacter);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        private static bool RLikeCore(string matchExpression, string pattern, string? escapeCharacter)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            throw new InvalidOperationException(CoreStrings.FunctionOnClient("RLike"));
        }
    }
}
