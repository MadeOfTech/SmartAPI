using System.Collections.Generic;

namespace MadeOfTech.SmartAPI.FIQL.FIQLGrammar
{
    class ConstraintTokenDefinitions
    {
        private List<TokenDefinition> _tokenDefinitions;
        private const string unreserved = "[a-zA-Z0-9-._~]";
        private const string pct_encoded = "%[a-fA-F0-9]{2}";
        private const string fiql_delim = "[!$'*+]";
        private string selector = $"({unreserved}|{pct_encoded})+";
        private string comparison = "(!|=|=lt|=gt|=le|=ge)=";
        private string arg_char = $"({unreserved}|{pct_encoded}|{fiql_delim}|=)";
        private string argument = $"({unreserved}|{pct_encoded}|{fiql_delim}|=)+";
        public ConstraintTokenDefinitions()
        {
            _tokenDefinitions = new List<TokenDefinition>();

            _tokenDefinitions.Add(new TokenDefinition("selector", selector, 2));
            _tokenDefinitions.Add(new TokenDefinition("comparison", comparison, 1));
            _tokenDefinitions.Add(new TokenDefinition("argument", argument, 1));
        }

        public List<TokenDefinition> TokenDefinitions { get => _tokenDefinitions; }
    }
}
