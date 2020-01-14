using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GadrocsWorkshop.Helios.Util
{
    public class Tokenizer
    {
        /// <summary>
        /// called to tokenize a string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="minTokens"></param>
        /// <returns>at least minTokens tokens, with any tokens that were not present in the input set to the empty string</returns>
        public static string[] TokenizeAtLeast(string input, int minTokens, params char[] separator)
        {
            string[] raw = input.Split(separator);
            int parsed = raw.Length;
            if (parsed < minTokens)
            {
                Array.Resize(ref raw, minTokens);
            }
            for (int fill = parsed; fill < minTokens; fill++)
            {
                raw[fill] = "";
            }
            return raw;
        }
    }
}
