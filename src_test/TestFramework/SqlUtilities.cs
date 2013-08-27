using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;

namespace AdSage.Concert.Test.Framework
{
    /// <summary>
    /// Collection of utilities for deal with Sql connections.
    /// </summary>
    public static class SqlUtilities
    {

        ///<summary>
        /// MasterDB name 
        ///</summary>
        private const string MasterDB = "master";

        /// <summary>
        /// Returns an opened SqlConnection, with proper error checking.
        /// </summary>
        public static SqlConnection GetOpenSqlConnection(this DataContext context)
        {
            if (context.Connection == null)
            {
                throw new InvalidOperationException("DataContext has no connection");
            }

            SqlConnection connection = context.Connection as SqlConnection;
            if (connection == null)
            {
                throw new InvalidOperationException("DataContext connection is of type \"" + context.GetType() + "\", not a SqlConnection.");
            }

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            

            return connection;
        }

        /// <summary>
        /// Using the given datacontext, call the action within a transaction
        /// and submit the changes when it returns.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="action"></param>
        public static void TransactionSubmit<T>(this T db, Action<T> action) where T : DataContext
        {
            using (TransactionScope scope = new TransactionScope())
            {
                action(db);
                db.SubmitChanges();
                scope.Complete();
            }
        }

        /// <summary>
        /// TODO: DO NOT USE THIS YET -- IT *WILL* BE MOVING / CHANGING
        /// </summary>
        //[Obsolete("Use a DataContext instead!")] TODO: this needs to be uncommented later after BTBIDB context generation
        public static SqlConnection CreateOpenSqlConnectionNoMap(string databaseServer, string database)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = databaseServer,
                InitialCatalog = database, //MapDatabase(database),
                IntegratedSecurity = true
            };
            SqlConnection connection = new SqlConnection(connectionStringBuilder.ConnectionString);
            connection.Open();
            return connection;
        }

        static SqlCommand CreateCommand(this SqlConnection connection, string query, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddRange(parameters);
            return cmd;
        }

#if false // currently unused...
        static void UsingSqlCommand(this SqlConnection connection, Action<SqlCommand> action, string query, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlCommand cmd = connection.CreateCommand(query, parameters))
                {
                    action(cmd);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                throw;
            }
        }
#endif

        static T UsingSqlCommand<T>(this SqlConnection connection, Func<SqlCommand, T> func, string query, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlCommand cmd = connection.CreateCommand(query, parameters))
                {
                    return func(cmd);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Executes a non-query command.
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="query">The query</param>
        /// <param name="parameters">Parameters to the query</param>
        public static void ExecuteNonQuery(this SqlConnection connection, string query, params SqlParameter[] parameters)
        {
            connection.UsingSqlCommand(cmd => cmd.ExecuteNonQuery(), query, parameters);
        }

        /// <summary>
        /// Gets a set from a database query that enumerates multiple rows.
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="query">The query for the set</param>
        /// <param name="parameters">Parameters to the query</param>
        /// <returns>The values from the query; hard casted to T</returns>
        public static object[][] ExecuteArray(this SqlConnection connection, string query, params SqlParameter[] parameters)
        {
            return connection.ExecuteArray(
                reader =>
                {
                    object[] row = new object[reader.FieldCount];
                    for (int col = 0; col < row.Length; col++)
                    {
                        row[col] = reader[col];
                    }
                    return row;
                },
                query,
                parameters);
        }

        /// <summary>
        /// Gets a set from a database query that returns multiple rows of one column each.
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="query">The query for the set</param>
        /// <param name="parameters">Parameters to the query</param>
        /// <returns>The values from the query; hard casted to T</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T[] ExecuteArray<T>(this SqlConnection connection, string query, params SqlParameter[] parameters)
        {
            return connection.ExecuteArray(reader => (T)(reader[0] is DBNull ? null : reader[0]), query, parameters);
        }

        /// <summary>
        /// Gets a set from a database query that returns multiple rows, transformed by func.
        /// Why are you not using Linq?
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="func">Function to convert a row to an instance of T</param>
        /// <param name="query">The query for the set</param>
        /// <param name="parameters">Parameters to the query</param>
        /// <returns>The values from the query; hard casted to T</returns>
        public static T[] ExecuteArray<T>(this SqlConnection connection, Func<SqlDataReader, T> func, string query, params SqlParameter[] parameters)
        {
            return connection.UsingSqlCommand(
                cmd =>
                {
                    List<T> list = new List<T>();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(func(reader));
                        }
                    }

                    return list.ToArray();
                },
                query,
                parameters);
        }

        /// <summary>
        /// Gets a single value from a sql query (this will only return the first row of the first column of the query output; 
        /// it will ignore the rest of the output: use for single data item queries.
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="query">The query for the single value</param>
        /// <param name="parameters">Parameters to the query</param>
        /// <returns>an object value of the query; cast as needed</returns> // originally an object value...
        public static object ExecuteScalar(this SqlConnection connection, string query, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddRange(parameters);
                object retVal = cmd.ExecuteScalar();
                return retVal;
            }
        }

        /// <summary>
        /// Gets a single value from a sql query (this will only return the first row of the first column of the query output; 
        /// it will ignore the rest of the output: use for single data item queries.
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="query">The query for the single value</param>
        /// <param name="parameters">Parameters to the query</param>
        /// <returns>The value of the query; hard casted to T</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T ExecuteScalar<T>(this SqlConnection connection, string query, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddRange(parameters);
                object val = cmd.ExecuteScalar();
                T retVal = (T)(val is DBNull ? null : val);
                return retVal;
            }
        }

        /// <summary>
        /// Gets a single row of data from a sql query, asserting if there is not exactly one row.
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="query">The query for the single value</param>
        /// <param name="parameters">Parameters to the query</param>
        /// <returns>row as dictionary</returns>
        public static Dictionary<string, object> ExecuteRow(this SqlConnection connection, string query, params SqlParameter[] parameters)
        {
            using (var rows = connection.ExecuteRows(query, parameters).GetEnumerator())
            {
                Assert.IsTrue(rows.MoveNext(), "There were no rows of data returned.");
                var row = rows.Current;
                Assert.IsFalse(rows.MoveNext(), "There was more than one row of data returned.");
                return row;
            }
        }

        /// <summary>
        /// Gets rows of data from a sql query.  Note that this is not terribly effecient;
        /// don't use it if performance matters (however, it yields, rather than loading
        /// all into memory, so it will work in a streaming scenario).
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="query">The query for the single value</param>
        /// <param name="parameters">Parameters to the query</param>
        /// <returns>rows as dictionaries</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IEnumerable<Dictionary<string, object>> ExecuteRows(this SqlConnection connection, string query, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddRange(parameters);
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();
                        for (int i = 0; i < sdr.FieldCount; i++)
                        {
                            row.Add(sdr.GetName(i), sdr[i]);
                        }
                        yield return row;
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the specified table exists.
        /// </summary>
        /// <param name="connection">Connection to the database</param>
        /// <param name="tableName">Table to check for</param>
        /// <returns></returns>
        public static bool DoesTableExist(this SqlConnection connection, string tableName)
        {
            return connection.ExecuteArray(
                reader => (string)reader["TABLE_NAME"],
                "sp_tables",
                new SqlParameter("@table_name", tableName)).Contains(tableName);
        }

        ///<summary>
        /// Gets MS SQL connection string
        ///</summary>
        ///<param name="databaseServer">Database server name</param>
        ///<param name="database">Database name</param>
        ///<returns>MS SQL connection string</returns>
        private static string GetConnectionString(string databaseServer, string database)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = databaseServer,
                InitialCatalog = database,
                IntegratedSecurity = true
            };

            return builder.ConnectionString;
        }
    }
}
