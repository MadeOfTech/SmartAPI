using MadeOfTech.SmartAPI.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace MadeOfTech.SmartAPI.FIQL
{
    public class FIQLExpression : IFLQLElement
    {
        List<IFLQLElement> _fiqlElements = new List<IFLQLElement>();
        FIQLExpression _currentChildExpression = null;
        int _expressionDepth = 0;

        public FIQLExpression() { }

        public (string whereSqlStatement, IDictionary<string, object> sqlParameters) ComputeWhereClause(IEnumerable<Attribute> attributes)
        {
            string whereSqlStatement = " ( ";
            var sqlParameters = new Dictionary<string, object>();
            foreach (var element in _fiqlElements)
            {
                var innerClause = element.ComputeWhereClause(attributes);
                whereSqlStatement += innerClause.whereSqlStatement;
                sqlParameters = sqlParameters.Concat(innerClause.sqlParameters).ToDictionary(x=>x.Key, x => x.Value);
            }
            whereSqlStatement += " ) ";
            return (whereSqlStatement, sqlParameters);
        }

        public void ParseToken(DslToken token)
        {
            if (_expressionDepth > 0)
            {
                switch (token.TokenType)
                {
                    case "openParenthesis":
                        _expressionDepth++;
                        break;
                    case "closeParenthesis":
                        _expressionDepth--;
                        if (_expressionDepth == 0)
                        {
                            _fiqlElements.Add(_currentChildExpression);
                            _currentChildExpression = null;
                        }
                        break;
                    case "sequenceTerminator":
                        throw new FIQLException("FIQL Clause is not correct. There are too much open parenthesis.");
                    default:
                        _currentChildExpression.ParseToken(token);
                        break;
                }
            }
            else
            { 
                switch (token.TokenType)
                {
                    case "openParenthesis":
                        _expressionDepth++;
                        _currentChildExpression = new FIQLExpression();
                        break;
                    case "closeParenthesis":
                        throw new FIQLException("FIQL Clause is not correct. We encountered a problem with a close parenthesis.");
                    case "constraint":
                        // TODO : modify with the column name
                        var fiqlConstraint = new FIQLConstraint();
                        fiqlConstraint.ParseToken(token);
                        _fiqlElements.Add(fiqlConstraint);
                        break;
                    case "operator":
                        var fiqlOperator = new FIQLOperator();
                        fiqlOperator.ParseToken(token);
                        _fiqlElements.Add(fiqlOperator);
                        break;
                    case "sequenceTerminator":
                        return;
                }
            }
        }

        public override string ToString()
        {
            string value = " ( ";
            foreach (var element in _fiqlElements)
            {
                value += element.ToString();
            }
            value += " ) ";
            return value;
        }
    }
}
