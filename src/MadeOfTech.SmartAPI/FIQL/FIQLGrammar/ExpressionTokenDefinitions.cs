using System.Collections.Generic;

namespace MadeOfTech.SmartAPI.FIQL.FIQLGrammar
{
    class ExpressionTokenDefinitions
    {
        private List<TokenDefinition> _tokenDefinitions;
        private const string unreserved = "[a-zA-Z0-9-._~]";
        private const string pct_encoded = "%[a-fA-F0-9]{2}";
        private const string fiql_delim = "[!$'*+]";
        private string selector = $"(?<selector>({unreserved}|{pct_encoded})+)";
        private string comparison = "(?<comparison>(!|=|=lt|=gt|=le|=ge)=)";
        private string arg_char = $"({unreserved}|{pct_encoded}|{fiql_delim}|=)";
        private string argument = $"(?<argument>({unreserved}|{pct_encoded}|{fiql_delim}|=)+)";
        public ExpressionTokenDefinitions()
        {
            _tokenDefinitions = new List<TokenDefinition>();

            _tokenDefinitions.Add(new TokenDefinition("operator", "[;,]", 1));
            _tokenDefinitions.Add(new TokenDefinition("openParenthesis", "\\(", 1));
            _tokenDefinitions.Add(new TokenDefinition("closeParenthesis", "\\)", 1));
            _tokenDefinitions.Add(new TokenDefinition("constraint", "(" + selector + comparison + argument + "|" + selector + ")", 1));
        }

        public List<TokenDefinition> TokenDefinitions { get => _tokenDefinitions; }
    }
}
