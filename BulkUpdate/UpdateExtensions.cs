using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EntityExtensions
{
    public static class UpdateExtensions
    {
        //TODO Connection string should not be hardcoded
        private const string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=UniversityConsumer;Integrated Security=SSPI;";

        #region SQL Queries
        private const string CREATE_TEMPORARY_TABLE = "CREATE TABLE #temp (Id INT primary key,{0} varchar(50))";
        private const string INSERT_VALUES_QUERY = "INSERT INTO #temp ( Id, {0} ) VALUES";
        private const string UPDATE_VALUES_QUERY = @"UPDATE {0}
                                                SET {0}.{1} = (SELECT {1}
                                                FROM #temp
                                                WHERE #temp.Id = {0}.Id) ";
        private const string ADDITIONAL_WHERE_CLAUSE = "where {0}.Id in (";

        private const string DROP_TEMP_TABLE = @"IF OBJECT_ID('tempdb.dbo.#temp', 'U') IS NOT NULL
												DROP TABLE #temp; ";
        #endregion
       
        public static void ExecuteScalarBulk<T,P>(this T entity,Expression<Func<T, P>> selector, Dictionary<string,string> entityValues)
           // where P: struct
            where T : class
        {
            // get column for update with hidden reflection from the expression(property name)
            //I believe they use reflection to get property name
            var body = (MemberExpression)selector.Body;

            var columnName= body.Member.Name;
            var tableName=body.Member.DeclaringType.Name;

            //establish connection
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                using(var transaction= connection.BeginTransaction())
                    try
                    {
                        using (var command = new SqlCommand())
                        {
                            command.Connection = connection;
                            command.Transaction = transaction;
                            
                            //TODO: check if same table exists/ dynamically decide its columns
                            command.CommandText = string.Format(CREATE_TEMPORARY_TABLE, columnName);

                            var sb = new StringBuilder();
                            sb.Append(string.Format(INSERT_VALUES_QUERY, columnName));
                            var additionalWhere = ADDITIONAL_WHERE_CLAUSE;

                            //Maximum number of rows is 1000, TODO: paging
                            foreach (var member in entityValues)
                            {
                                sb.Append(string.Format(" ({0}, '{1}'),", member.Key, member.Value));
                                additionalWhere += (member.Key + ",");
                            }
                            var insertStatement = sb.ToString();
                            command.CommandText += insertStatement.Substring(0, insertStatement.Length - 1);

                            var updateQuery = UPDATE_VALUES_QUERY + additionalWhere.Substring(0,additionalWhere.Length-1)+");";
                            command.CommandText += string.Format(updateQuery, tableName, columnName);

                            command.CommandText += DROP_TEMP_TABLE;


                            var result = command.ExecuteScalar();
                            transaction.Commit();
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();

                    }
                    finally
                    {
                        connection.Close();
                    }
            }
        }
    }
}
