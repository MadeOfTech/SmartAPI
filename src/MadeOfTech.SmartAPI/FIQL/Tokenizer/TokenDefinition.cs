using MadeOfTech.SmartAPI.FIQL.FIQLGrammar;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MadeOfTech.SmartAPI.FIQL
{
    public class TokenDefinition
    {
        private Regex _regex;
        private readonly string _returnsToken;
        private readonly int _precedence;

        public TokenDefinition(string returnsToken, string regexPattern, int precedence)
        {
            _regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _returnsToken = returnsToken;
            _precedence = precedence;
        }

        public IEnumerable<TokenMatch> FindMatches(string inputString)
        {
            var matches = _regex.Matches(inputString);
            for (int i = 0; i < matches.Count; i++)
            {
                var groupNames = _regex.GetGroupNames();
                Dictionary<string, string> groups = null;
                if (groupNames.Length > 0)
                {
                    groups = new Dictionary<string, string>();
                    foreach (var groupName in groupNames)
                    {
                        groups.Add(groupName, matches[i].Groups[groupName].Value);
                    }
                }
                yield return new TokenMatch()
                {
                    StartIndex = matches[i].Index,
                    EndIndex = matches[i].Index + matches[i].Length,
                    TokenType = _returnsToken,
                    Value = matches[i].Value,
                    Precedence = _precedence,
                    Groups = groups
                };
            }
        }
    }
}
