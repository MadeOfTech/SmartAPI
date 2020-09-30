using System.Collections.Generic;
using System.Linq;

namespace MadeOfTech.SmartAPI.FIQL
{
    class Tokenizer
    {
        IEnumerable<TokenDefinition> _tokenDefinitions;
        public Tokenizer(IEnumerable<TokenDefinition> tokenDefinitions)
        {
            _tokenDefinitions = tokenDefinitions;
        }

        public IEnumerable<DslToken> Tokenize(string message)
        {
            var tokenMatches = FindTokenMatches(message);

            var groupedByIndex = tokenMatches.GroupBy(x => x.StartIndex)
                .OrderBy(x => x.Key)
                .ToList();

            TokenMatch lastMatch = null;
            for (int i = 0; i < groupedByIndex.Count; i++)
            {
                var bestMatch = groupedByIndex[i].OrderBy(x => x.Precedence).First();
                if (lastMatch != null && bestMatch.StartIndex < lastMatch.EndIndex)
                    continue;

                yield return new DslToken(bestMatch.TokenType,
                                          bestMatch.Value,
                                          bestMatch.SqlKeyword,
                                          bestMatch.Groups);

                lastMatch = bestMatch;
            }

            yield return new DslToken("sequenceTerminator");
        }

        private List<TokenMatch> FindTokenMatches(string message)
        {
            var tokenMatches = new List<TokenMatch>();

            foreach (var tokenDefinition in _tokenDefinitions)
                tokenMatches.AddRange(tokenDefinition.FindMatches(message).ToList());

            return tokenMatches;
        }
    }
}
