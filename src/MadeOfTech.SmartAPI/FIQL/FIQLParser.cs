using MadeOfTech.SmartAPI.FIQL.FIQLGrammar;

namespace MadeOfTech.SmartAPI.FIQL
{
    public class FIQLParser
    {
        public FIQLParser()
        {
        }

        public FIQLExpression Parse(string fiqlQuery)
        {
            var expressionTokenDefinitions = new ExpressionTokenDefinitions();
            var tokenizer = new Tokenizer(expressionTokenDefinitions.TokenDefinitions);
            var tokens = tokenizer.Tokenize(fiqlQuery);

            FIQLExpression rootExpression = new FIQLExpression();
            foreach (var token in tokens)
            {
                rootExpression.ParseToken(token);
            }
            return rootExpression;
        }
    }
}
