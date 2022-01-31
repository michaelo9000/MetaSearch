using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Extensions
{
    public static class StringExtensions
    {
        public static int TrimToIntAndConvert(this string s)
        {
            return Convert.ToInt32(string.Join(string.Empty, s.Where(c => char.IsDigit(c))));
        }
    }
}
