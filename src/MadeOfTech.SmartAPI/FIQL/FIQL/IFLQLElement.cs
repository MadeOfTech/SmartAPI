using System;
using System.Collections.Generic;

namespace MadeOfTech.SmartAPI.FIQL
{
    interface IFLQLElement
    {
        /// <summary>
        /// This method permit the element to parse a new token.
        /// </summary>
        /// <param name="token">Token to be parsed</param>
        void ParseToken(DslToken token);

        /// <summary>
        /// This method generates SQL using attributes that are given in parameter.
        /// 
        /// </summary>
        /// <param name="attributes">
        /// List of attributes that are allowed to be used to build query.
        /// </param>
        /// <returns>Plain SQL string</returns>
        (string whereSqlStatement, IDictionary<string, Object> sqlParameters) ComputeWhereClause(IEnumerable<Data.Models.Attribute> attributes);
    }
}
