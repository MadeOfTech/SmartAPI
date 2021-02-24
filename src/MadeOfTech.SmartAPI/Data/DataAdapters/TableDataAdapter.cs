using Dapper;
using MadeOfTech.SmartAPI.Data.Models;
using MadeOfTech.SmartAPI.Db;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.DataAdapters
{
    public class TableDataAdapter : IDisposable
    {
        public enum UpsertResult
        {
            NothingChanged,
            OneRowInserted,
            OneRowUpdated
        };

        private readonly IDbConnection _dbConnection;
        private readonly Collection _collection;

        private string _injectedAttributeName;
        private object _injectedAttributeValue;

        public Collection Collection { get { return _collection; } }
        public IEnumerable<Data.Models.Attribute> Attributes { get { return _collection.attributes; } }

        public TableDataAdapter(IDbConnection dbConnection, Collection collection)
        {
            _dbConnection = dbConnection;
            _collection = collection;
        }

        /// <summary>
        /// This method allow caller to inject an attribute inside all requests
        /// that are done against the table. The effects are the following :
        /// * SelectAsync for a collection : will inject the attribute in a where clause ;
        /// * SelectAsync for a member : will inject the attribute in the where clause ;
        /// * DeleteAsync : will inject the attribute in the where clause ;
        /// * PostAsync : will inject the attribute in the insert clause ;
        /// * PutAsync : will inject the attribute in the where clause of the update 
        ///   or in the insert clause of the insert;
        /// </summary>
        /// <param name="name">name of the attribute</param>
        /// <param name="value">value of the attribute</param>
        public void injectAttribute(string name, object value)
        {
            _injectedAttributeName = name;
            _injectedAttributeValue = value;
        }

        public async Task<List<dynamic>> SelectAsync()
        {

            if (!String.IsNullOrEmpty(_injectedAttributeName))
            {
                var whereInjectedClause = ComputeInjectedWhereClause();
                var attributesSqlStatement = ComputeProjectedAttributes();
                string querySqlStatement = $"SELECT {attributesSqlStatement} FROM {_collection.tablename} WHERE {whereInjectedClause.whereSqlStatement};";
                return (await _dbConnection.QueryAsync(querySqlStatement, whereInjectedClause.sqlParameters)).ToList();
            }
            else
            {
                var attributesSqlStatement = ComputeProjectedAttributes();
                string querySqlStatement = $"SELECT {attributesSqlStatement} FROM {_collection.tablename};";
                return (await _dbConnection.QueryAsync(querySqlStatement)).ToList();
            }
        }

        public async Task<List<dynamic>> SelectAsync((string whereSqlStatement, IDictionary<string, Object> sqlParameters) whereClause)
        {
            if (!String.IsNullOrEmpty(_injectedAttributeName))
            {
                var whereInjectedClause = ComputeInjectedWhereClause();
                var attributesSqlStatement = ComputeProjectedAttributes();
                string querySqlStatement = $"SELECT {attributesSqlStatement} FROM {_collection.tablename} WHERE {whereInjectedClause.whereSqlStatement} AND {whereClause.whereSqlStatement};";
                return (await _dbConnection.QueryAsync(querySqlStatement, whereInjectedClause.sqlParameters.Concat(whereClause.sqlParameters))).ToList();
            }
            else
            {
                var attributesSqlStatement = ComputeProjectedAttributes();
                string querySqlStatement = $"SELECT {attributesSqlStatement} FROM {_collection.tablename} WHERE {whereClause.whereSqlStatement};";
                return (await _dbConnection.QueryAsync(querySqlStatement, whereClause.sqlParameters)).ToList();
            }
        }
        
        public async Task<List<dynamic>> SelectAsync(string[] keys)
        {
            if (null == keys) throw new ArgumentNullException("keys", "keys can't be null");
            var whereClause = ComputeWhereClause(keys);
            var attributesSqlStatement = ComputeProjectedAttributes();
            string querySqlStatement = $"SELECT {attributesSqlStatement} FROM {_collection.tablename} WHERE {whereClause.whereSqlStatement};";
            return (await _dbConnection.QueryAsync(querySqlStatement, whereClause.sqlParameters)).ToList();
        }
        
        public async Task<dynamic> InsertAsync(dynamic newRow)
        {
            var attributesSqlStatement = ComputeInsertedAttributes();
            var attributesValuesSqlStatement = ComputeInsertedAttributesValues();
            string querySqlStatement = $"INSERT INTO {_collection.tablename} ({attributesSqlStatement}) VALUES ({attributesValuesSqlStatement});";
            querySqlStatement += $"SELECT " + _dbConnection.LastInsertedIdClause() + " AS serial;";

            DynamicParameters parameters = new DynamicParameters();

            foreach (var attribute in _collection.attributes)
            {
                if (attribute.autovalue) continue;

                if ((null == newRow[attribute.attributename]) || ((newRow)[attribute.attributename].Type == Newtonsoft.Json.Linq.JTokenType.Null))
                {
                    parameters.Add(attribute.attributename, null);
                }
                else if (attribute.type == "string" && attribute.format == "binary")
                {
                    Console.WriteLine(newRow[attribute.attributename]);
                    if (newRow[attribute.attributename].ToString().StartsWith("0x"))
                    {
                        string value = newRow[attribute.attributename].ToString();
                        var binaryValue = value.UnHex();
                        parameters.Add(attribute.attributename, binaryValue);
                    }
                }
                else if (attribute.type == "integer" || attribute.type == "number")
                {
                    parameters.Add(attribute.attributename, (int?)newRow[attribute.attributename]);
                }
                else
                {
                    parameters.Add(attribute.attributename, newRow[attribute.attributename].ToString());
                }
            }

            if (!String.IsNullOrEmpty(_injectedAttributeName))
            {
                parameters.Add(_injectedAttributeName, _injectedAttributeValue);
            }

            return (await _dbConnection.QueryAsync(querySqlStatement, parameters)).ToList().First();
        }

        /// <summary>
        /// Upsert command is a need for PUT method.
        /// </summary>
        /// <param name="newRow"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UpsertResult> UpsertAsync(dynamic newRow, string[] keys)
        {
            DynamicParameters parameters = new DynamicParameters();

            bool upsert = true;
            foreach (var attribute in _collection.attributes)
            {
                string value = null;
                if (attribute.keyindex.HasValue) value = keys[attribute.keyindex.Value];
                else value = newRow[attribute.attributename].ToString();

                if (attribute.autovalue)
                {
                    // When PUT is called, the semantic means only these actions :
                    // - update a member which url is known ;
                    // - add a member which url is known.
                    // This last condition supposes that no auto_increment column
                    // is implied into insertion.
                    upsert = false;
                    parameters.Add(attribute.attributename, String.IsNullOrEmpty(value) ? (long?)null : long.Parse(value));
                }

                if (attribute.type == "string" && attribute.format == "binary")
                {
                    if (value.ToString().StartsWith("0x"))
                    {
                        var binaryValue = newRow[attribute.attributename].ToString().UnHex();
                        parameters.Add(attribute.attributename, binaryValue);
                    }
                }
                else if (attribute.type == "integer")
                {
                    parameters.Add(attribute.attributename, String.IsNullOrEmpty(value) ? (long?)null : long.Parse(value));
                }
                else
                {
                    parameters.Add(attribute.attributename, value);
                }
            }

            if (!String.IsNullOrEmpty(_injectedAttributeName))
            {
                parameters.Add(_injectedAttributeName, _injectedAttributeValue);
            }

            string querySqlStatement = "";
            if (upsert)
            {
                var attributesSqlStatement = ComputeInsertedAttributes();
                var insertedattributesValuesSqlStatement = ComputeInsertedAttributesValues();
                var updatedAttributesValuesSqlStatement = ComputeUpdatedAttributesValues();

                querySqlStatement = $"INSERT INTO {_collection.tablename} ({attributesSqlStatement}) VALUES ({insertedattributesValuesSqlStatement})";
                querySqlStatement += $" ON DUPLICATE KEY UPDATE {updatedAttributesValuesSqlStatement};";

                int value = await _dbConnection.ExecuteAsync(querySqlStatement, parameters);
                if (0 == value) return UpsertResult.NothingChanged;
                else if (1 == value) return UpsertResult.OneRowInserted;
                else return UpsertResult.OneRowUpdated;
            }
            else
            {
                var updatedAttributesValuesSqlStatement = ComputeUpdatedAttributesValues();
                var whereClause = ComputeWhereClause(keys);

                querySqlStatement = $"UPDATE {_collection.tablename} SET {updatedAttributesValuesSqlStatement} WHERE {whereClause.whereSqlStatement}";

                int value = await _dbConnection.ExecuteAsync(querySqlStatement, parameters);
                if (0 == value) return UpsertResult.NothingChanged;
                else return UpsertResult.OneRowUpdated;
            }
        }

        public async Task DeleteASync(string[] keys)
        {
            if (null == keys) throw new ArgumentNullException("keys", "keys can't be null");
            var whereClause = ComputeWhereClause(keys);
            string querySqlStatement = $"DELETE FROM {_collection.tablename} WHERE {whereClause.whereSqlStatement};";
            await _dbConnection.ExecuteAsync(querySqlStatement, whereClause.sqlParameters);
        }

        #region Inner methods
        private string ComputeProjectedAttributes()
        {
            if (null == _collection.attributes) return "*";
            if (0 == _collection.attributes.Count()) return "*";

            var attributesSqlStatement = "";
            foreach (var attribute in _collection.attributes)
            {
                if (!string.IsNullOrEmpty(attributesSqlStatement)) attributesSqlStatement += ",";
                attributesSqlStatement += $"{attribute.columnname} AS {attribute.attributename}";
            }
            return attributesSqlStatement;
        }

        private string ComputeInsertedAttributes()
        {
            if (null == _collection.attributes) return null;
            if (0 == _collection.attributes.Count()) return null;

            var attributesSqlStatement = "";
            foreach (var attribute in _collection.attributes)
            {
                if (attribute.autovalue) continue;
                if (!string.IsNullOrEmpty(attributesSqlStatement)) attributesSqlStatement += ",";
                attributesSqlStatement += $"{attribute.columnname}";
            }
            if (!String.IsNullOrEmpty(_injectedAttributeName))
            {
                if (!string.IsNullOrEmpty(attributesSqlStatement)) attributesSqlStatement += ",";
                attributesSqlStatement += $"{_injectedAttributeName}";
            }
            return attributesSqlStatement;
        }

        private string ComputeInsertedAttributesValues()
        {
            if (null == _collection.attributes) return null;
            if (0 == _collection.attributes.Count()) return null;

            var attributesSqlStatement = "";
            foreach (var attribute in _collection.attributes)
            {
                if (attribute.autovalue) continue;
                if (!string.IsNullOrEmpty(attributesSqlStatement)) attributesSqlStatement += ",";
                attributesSqlStatement += $"@{attribute.attributename}";
            }
            if (!String.IsNullOrEmpty(_injectedAttributeName))
            {
                if (!string.IsNullOrEmpty(attributesSqlStatement)) attributesSqlStatement += ",";
                attributesSqlStatement += $"@{_injectedAttributeName}";
            }
            return attributesSqlStatement;
        }
        private string ComputeUpdatedAttributesValues()
        {
            if (null == _collection.attributes) return null;
            if (0 == _collection.attributes.Count()) return null;

            var attributesSqlStatement = "";
            foreach (var attribute in _collection.attributes)
            {
                if (attribute.keyindex.HasValue) continue;
                if (!string.IsNullOrEmpty(attributesSqlStatement)) attributesSqlStatement += ",";
                attributesSqlStatement += $"{attribute.attributename}=@{attribute.attributename}";
            }
            return attributesSqlStatement;
        }

        /// <summary>
        /// This method compute the where clause that allow to locate precisely
        /// a member into a collection. The key of this member could be composite,
        /// meaning that many keys could be needed to locate the member.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>
        /// It returns the where SQL Statement and an array containing the typedValues.
        /// </returns>
        private (string whereSqlStatement, IDictionary<string, Object> sqlParameters) ComputeWhereClause(string[] keys)
        {
            var keyAttributes = GetKeyAttributes();
            if (0 == keyAttributes.Count()) return (null, null);
            else if (keys.Length != keyAttributes.Count()) return (null, null);

            string whereSqlStatement = "(";
            var sqlParameters = new ExpandoObject() as IDictionary<string, Object>;
            foreach (var keyAttribute in keyAttributes)
            {
                object typedValue = null;
                switch (keyAttribute.type)
                {
                    case "integer":
                    case "number":
                    case "serial":
                        typedValue = long.Parse(keys[keyAttribute.keyindex.Value]);
                        break;
                    case "string":
                        typedValue = keys[keyAttribute.keyindex.Value];
                        break;
                    case "datetime":
                        typedValue = keys[keyAttribute.keyindex.Value].UnISO8601();
                        break;
                    default:
                        return (null, null);
                }

                if (whereSqlStatement.Length > 1) whereSqlStatement += " AND ";
                whereSqlStatement += $"{keyAttribute.columnname}=@{keyAttribute.attributename}";
                sqlParameters.Add(keyAttribute.attributename, typedValue);
            }

            whereSqlStatement += ")";

            if (!String.IsNullOrEmpty(_injectedAttributeName))
            {
                var whereInjectedClause = ComputeInjectedWhereClause();
                whereSqlStatement += " AND " + whereInjectedClause.whereSqlStatement;
                sqlParameters.Add(whereInjectedClause.sqlParameters.First());
            }

            return (whereSqlStatement, sqlParameters);
        }

        private (string whereSqlStatement, IDictionary<string, Object> sqlParameters) ComputeInjectedWhereClause()
        {
            if (String.IsNullOrEmpty(_injectedAttributeName)) return (null, null);

            string whereSqlStatement = "(";
            var sqlParameters = new ExpandoObject() as IDictionary<string, Object>;

            whereSqlStatement += $"{_injectedAttributeName}=@{_injectedAttributeName}";
            sqlParameters.Add(_injectedAttributeName, _injectedAttributeValue);

            whereSqlStatement += ")";
            return (whereSqlStatement, sqlParameters);
        }

        List<Data.Models.Attribute> GetKeyAttributes()
        {
            var attributes = new List<Data.Models.Attribute>();
            if (null == _collection.attributes) return attributes;

            foreach (var attribute in _collection.attributes)
            {
                if (attribute.keyindex.HasValue) attributes.Add(attribute);
            }
            return attributes;
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // Pour détecter les appels redondants

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: libérer les ressources managées.
                }

                // TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
                // TODO: définir les champs de grande taille avec la valeur Null.

                disposedValue = true;
            }
        }


        // Ce code est ajouté pour implémenter correctement le modèle supprimable.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
