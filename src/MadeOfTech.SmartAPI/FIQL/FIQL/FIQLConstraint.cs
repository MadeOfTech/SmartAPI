using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace MadeOfTech.SmartAPI.FIQL
{
    class FIQLConstraint : IFLQLElement
    {
        private string _selector;
        private string _comparison;
        private string _argument;

        public void ParseToken(DslToken token)
        {
            if (token.TokenType != "constraint")
                throw new FIQLException("Error while parsing a constraint : token " + token.ToString() + " is not an constraint.");

            _selector = token.Groups["selector"];
            if (token.Groups.ContainsKey("comparison"))
                _comparison = token.Groups["comparison"];
            if (token.Groups.ContainsKey("argument"))
                _argument = token.Groups["argument"];
        }

        public (string whereSqlStatement, IDictionary<string, Object> sqlParameters) ComputeWhereClause(IEnumerable<Data.Models.Attribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                if (attribute.attributename == _selector)
                {
                    if (null == _comparison && null == _argument && attribute.type == "boolean")
                    {
                        // In this case, the constraint is only the field itself.
                        // It supposes that its type is boolean.
                        return (SQLGetColumnName(attribute), new Dictionary<string, Object>());
                    }
                    else
                    {
                        string columnname = SQLGetColumnName(attribute);
                        var operatorAndOperand = SQLGetOperatorAndOperand(attribute);
                        string clause = columnname + " " + operatorAndOperand.Item1 + " @" + columnname + this.GetHashCode().ToString();
                        var dictionary = new Dictionary<string, Object>();
                        dictionary.Add(columnname + this.GetHashCode().ToString(), operatorAndOperand.Item2);
                        return (clause, dictionary);
                    }
                }
            }

            throw new FIQLException("Unknown attribute : " + _selector + ".");
        }

        private string SQLGetColumnName(Data.Models.Attribute attribute)
        {
            return attribute.columnname;
        }

        private (string, object) SQLGetOperatorAndOperand(Data.Models.Attribute attribute)
        {
            if (attribute.type == "string" && string.IsNullOrEmpty(attribute.format))
            {
                // Two comparison operators are applicable to simple text comparisons;
                // o  "==" yields True if the string value(as per XPath) of any
                //   selected node matches the argument; otherwise False.
                // o  "!=" yields True if the string value of every selected node does
                //   not match the argument; otherwise False.
                if (_comparison == "==")
                {
                    if (_argument.StartsWith("*") || _argument.EndsWith("*"))
                    {
                        return ("LIKE", SQLGetStringValue());
                    }
                    else
                    {
                        return ("=", SQLGetStringValue());
                    }
                }
                else if (_comparison == "!=")
                {
                    if (_argument.StartsWith("*") || _argument.EndsWith("*"))
                    {
                        return ("NOT LIKE", SQLGetStringValue());
                    }
                    else
                    {
                        return ("<>", SQLGetStringValue());
                    }
                }
            }
            else if (attribute.type == "string" && attribute.format == "0xbyte")
            {
                try
                {
                    var value = _argument.UnHex();
                    return (SQLGetGenericOperator(), value);
                }
                catch (FIQLException)
                {
                    throw;
                }
                catch
                {
                    throw new FIQLException("Value for attribute " + _selector + " is not a 0xbyte : " + _argument + ".");
                }
            }
            else if (attribute.type == "string" && attribute.format == "byte")
            {
                try
                {
                    var value = Convert.FromBase64String(_argument);
                    return (SQLGetGenericOperator(), value);
                }
                catch (FIQLException)
                {
                    throw;
                }
                catch
                {
                    throw new FIQLException("Value for attribute " + _selector + " is not a byte : " + _argument + ".");
                }
            }
            else if (attribute.type == "string" && attribute.format == "date-time")
            {
                try
                {
                    var value = _argument.UnISO8601();
                    return (SQLGetGenericOperator(), value);
                }
                catch (FIQLException)
                {
                    throw;
                }
                catch
                {
                    throw new FIQLException("Value for attribute " + _selector + " is not a date-time : " + _argument + ".");
                }
            }
            else if (attribute.type == "string" && attribute.format == "date")
            {
                try
                {
                    var value = _argument.UnISO8601();
                    return (SQLGetGenericOperator(), value);
                }
                catch (FIQLException)
                {
                    throw;
                }
                catch
                {
                    throw new FIQLException("Value for attribute " + _selector + " is not a date-time : " + _argument + ".");
                }
            }
            else if (attribute.type == "integer")
            {
                try
                {
                    var value = long.Parse(_argument);
                    return (SQLGetGenericOperator(), value);
                }
                catch (FIQLException)
                {
                    throw;
                }
                catch
                {
                    throw new FIQLException("Value for attribute " + _selector + " is not an integer : " + _argument + ".");
                }
            }
            else if (attribute.type == "number")
            {
                try
                {
                    var value = System.Decimal.Parse(_argument);
                    return (SQLGetGenericOperator(), value);
                }
                catch (FIQLException)
                {
                    throw;
                }
                catch
                {
                    throw new FIQLException("Value for attribute " + _selector + " is not a number : " + _argument + ".");
                }
            }
            else if (attribute.type == "boolean")
            {
                if (_argument.ToLower().Equals("true"))
                    return (SQLGetGenericOperator(), "1");
                else if (_argument.ToLower().Equals("false"))
                    return (SQLGetGenericOperator(), "0");
                else
                    throw new FIQLException("Value for attribute " + _selector + " is not a boolean : " + _argument + ".");
            }

            throw new FIQLException("Attribute which type is " + attribute.type + " can't be handled.");
        }

        private string SQLGetGenericOperator()
        {
            switch (_comparison)
            {
                case "==": return "=";
                case "!=": return "<>";
                case "=lt=": return "<";
                case "=le=": return "<=";
                case "=gt=": return ">";
                case "=ge=": return ">=";
                default: throw new FIQLException("Unknown operator : " + _comparison + ".");
            }
        }

        private string SQLGetStringValue()
        {
            string value = _argument;
            bool startWildcard = false;
            bool endWildcard = false;
            if (value.StartsWith("*"))
            {
                value = value.Remove(0, 1);
                startWildcard = true;
            }
            if (value.EndsWith("*"))
            {
                value = value.Remove(value.Length - 1, 1);
                endWildcard = true;
            }
            value = (startWildcard ? "%" : "") + HttpUtility.UrlDecode(value, Encoding.UTF8) + (endWildcard ? "%" : "");
            return value;
        }


        public override string ToString()
        {
            string value = " " + _selector + " " + _comparison + " " + _argument;
            return value;
        }
    }
}
