using MadeOfTech.SmartAPI.FIQL.FIQLGrammar;
using System;
using System.Collections.Generic;
using System.Text;

namespace MadeOfTech.SmartAPI.FIQL
{
    public class DslToken
    {
        public DslToken(string tokenType)
        {
            TokenType = tokenType;
            Value = string.Empty;
            SqlKeyword = null;
        }

        public DslToken(string tokenType, string value, string sqlKeyword, Dictionary<string, string> groups)
        {
            TokenType = tokenType;
            Value = value;
            SqlKeyword = sqlKeyword;
            Groups = groups;
        }

        public string TokenType { get; set; }
        public string Value { get; set; }
        public string SqlKeyword { get; set; }
        public Dictionary<string , string> Groups { get; set; }

        public DslToken Clone()
        {
            return new DslToken(TokenType, Value, SqlKeyword, Groups);
        }

        public override string ToString()
        {
            return TokenType + "(" + Value + ")";
        }
    }
}
