using System;
using System.Collections.Generic;

namespace WebApplication1.RSocket
{
    internal class Stubs
    {
        public static readonly Func<IList<string>, string> All;

        static Stubs()
        {
            All = protocols =>
            {
                return null;
            };
        }
    }
}