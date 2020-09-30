using MadeOfTech.SmartAPI.FIQL.FIQLGrammar;
using System;
using System.Collections.Generic;
using System.Text;

namespace MadeOfTech.SmartAPI.FIQL
{
    public class TokenMatch
    {
        public string TokenType { get; set; }
        public string Value { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int Precedence { get; set; }
        public string SqlKeyword { get; set; }
        public Dictionary<string, string> Groups { get; set; }
    }
}
