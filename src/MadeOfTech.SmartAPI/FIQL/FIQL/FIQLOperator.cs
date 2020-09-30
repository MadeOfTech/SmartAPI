using System;
using System.Collections.Generic;

namespace MadeOfTech.SmartAPI.FIQL
{
    class FIQLOperator : IFLQLElement
    {
        char _operator;

        public void ParseToken(DslToken token)
        {
            if (token.TokenType != "operator")
                throw new FIQLException("Error while parsing a constraint : token " + token.ToString() + " is not an operator.");

            _operator = token.Value[0];
        }

        public (string whereSqlStatement, IDictionary<string, Object> sqlParameters) ComputeWhereClause(IEnumerable<Data.Models.Attribute> attributes)
        {
            return ((_operator == ';') ? " AND " : " OR ", new Dictionary<string, Object>());
        }

        public override string ToString()
        {
            return (_operator == ';') ? " AND " : " OR ";
        }
    }
}
