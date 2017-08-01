using MockEngine.Interfaces;
using MockEngine.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Internal
{
    /// <summary>
    /// Note that public is required for the script engine to access methods
    /// </summary>
    public class Tables
    {
        private IEnumerable<MockTable> _mockTables;
        private DataSet _dataSet;
        private IMockContext _context;
        private ILogProvider _logProvider;
        private object _lock = new Object();
        public Tables( IMockContext context, IEnumerable<MockTable> mockTables)
        {
            _context = context;
            _logProvider = context.LogProvider;
            _mockTables = mockTables;
        }
        public DataSet DataSet
        {
            get
            {
                if ( _dataSet == null)
                {
                    _dataSet = BuildDataSet(_mockTables,_logProvider);
                }
                return _dataSet;
            }
        }
        private static Dictionary<string, Type> GetMetaData(IEnumerable<dynamic> data)
        {
            var columns = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var item in data)
            {
                foreach (var property in item)
                {
                    string name = property.Key;
                    object value = property.Value;
                    Type previousType;
                    Type type = value.GetType();
                    if (columns.TryGetValue(property.Key, out previousType))
                    {
                        // existing column
                        if (type != previousType)
                        {
                            columns[name] = typeof(object);
                        }
                    }
                    else
                    {
                        columns.Add(name, type);
                    }
                }
            }
            return columns;
        }
        private static DataSet BuildDataSet(IEnumerable<MockTable> mockTables, ILogProvider logProvider)
        {
            var dataSet = new DataSet();
            foreach (var mockTable in mockTables)
            {
                string tableName = mockTable.Name;
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    tableName = "Table_" + dataSet.Tables.Count + 1;
                }
                DataTable dataTable = new DataTable(tableName);
                dataSet.Tables.Add(dataTable);
                if (mockTable.Rows == null)
                {
                    logProvider.Warning("global table {tableName} does not declare any rows", tableName);
                }
                else
                {
                    var columns = GetMetaData(mockTable.Rows);
                    foreach (var column in columns)
                    {
                        dataTable.Columns.Add(column.Key, column.Value);
                    }
                    foreach (var item in mockTable.Rows)
                    {
                        var row = dataTable.NewRow();
                        foreach (var column in columns)
                        {
                            row[column.Key] = item[column.Key];
                        }
                        dataTable.Rows.Add(row);
                    }
                }
            }
            return dataSet;
        }
        public DataTable this[string tableName]
        {
            get
            {
                if ( tableName == null)
                {
                    _logProvider.Error("in your call to tables[tableName], tableName cannot be null");
                    throw new ArgumentNullException(nameof(tableName));
                }
                var table = this.DataSet.Tables[tableName];
                if ( table == null )
                {
                    var message = $"table \"{tableName} not found";
                    _logProvider.Error("table \"{tableName} not found", tableName);
                    throw new ArgumentOutOfRangeException(nameof(tableName), message);
                }
                return table;
            }
        }
        private IEnumerable<DynamicDataRow> ToDynamicDataRows( IEnumerable<DataRow> dataRows)
        {
            List<DynamicDataRow> rows = new List<DynamicDataRow>();
            foreach (DataRow dataRow in dataRows)
            {
                rows.Add(new DynamicDataRow(dataRow));
            }
            return rows;
        }
        public IEnumerable<DynamicDataRow> Rows(string tableName)
        {
            if (tableName == null)
            {
                _logProvider.Error("in your call to Rows(tableName), tableName cannot be null");
                throw new ArgumentNullException(nameof(tableName));
            }
            return ToDynamicDataRows( this[tableName].Rows.OfType<DataRow>() );
        }
        /// <summary>
        /// filters rows of the specified table
        /// see http://www.csharp-examples.net/dataview-rowfilter/ for more information on query syntax and sort order
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public IEnumerable<DynamicDataRow> Select(string tableName, string query, string sortOrder)
        {
            if (tableName == null)
            {
                _logProvider.Error("in your call to Select(tableName, query [, sortOrder]), tableName cannot be null");
                throw new ArgumentNullException(nameof(tableName));
            }
            if (query == null)
            {
                _logProvider.Error("in your call to Select(tableName, query [, sortOrder]), query cannot be null");
                throw new ArgumentNullException(nameof(query));
            }
            lock (_lock)
            {
                return ToDynamicDataRows(this[tableName].Select(query, sortOrder));
            }
        }
        public IEnumerable<DynamicDataRow> Select(string tableName, string query)
        {
            return Select(tableName, query, null);
        }
        public DynamicDataRow First(string tableName, string query, string sortOrder)
        {
            if (tableName == null)
            {
                _logProvider.Error("in your call to First(tableName, query), tableName cannot be null");
                throw new ArgumentNullException(nameof(tableName));
            }
            if (query == null)
            {
                _logProvider.Error("in your call to First(tableName, query), query cannot be null");
                throw new ArgumentNullException(nameof(query));
            }
            var results = this[tableName].Select(query, sortOrder);
            if (results != null && results.Length > 0)
            {
                return new DynamicDataRow(results[0]);
            }
            return null;
        }
        public DynamicDataRow First(string tableName, string query)
        {
            return First(tableName, query, null);
        }
        public DynamicDataRow FirstOrDefault(string tableName, string query, string sortOrder)
        {
            var result = First(tableName, query, sortOrder);
            if (result == null)
            {
                result = new DynamicDataRow(this[tableName].NewRow());
            }
            return result;
        }
        public DynamicDataRow FirstOrDefault(string tableName, string query)
        {
            if (tableName == null)
            {
                _logProvider.Error("in your call to FirstOrDefault(tableName, query), tableName cannot be null");
                throw new ArgumentNullException(nameof(tableName));
            }
            if (query == null)
            {
                _logProvider.Error("in your call to FirstOrDefault(tableName, query), query cannot be null");
                throw new ArgumentNullException(nameof(query));
            }
            return FirstOrDefault(tableName, query, null);
        }

        public bool Exists(string tableName, string query)
        {
            if (tableName == null)
            {
                _logProvider.Error("in your call to Exists(tableName, query), tableName cannot be null");
                throw new ArgumentNullException(nameof(tableName));
            }
            if (query == null)
            {
                _logProvider.Error("in your call to Exists(tableName, query), query cannot be null");
                throw new ArgumentNullException(nameof(query));
            }
            var result = this[tableName].Select(query);
            return result != null && result.Length > 0;
        }

    }
}
