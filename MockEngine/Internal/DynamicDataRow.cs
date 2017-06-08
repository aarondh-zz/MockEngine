using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Internal
{
    /// <summary>
    /// Note that public is required for script engine access
    /// </summary>
    public class DynamicDataRow : DynamicObject
    {
        private DataRow _dataRow;

        public DynamicDataRow(DataRow dataRow)
        {
            if (dataRow == null)
                throw new ArgumentNullException("dataRow");
            this._dataRow = dataRow;
        }

        public DataRow DataRow
        {
            get { return _dataRow; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (_dataRow.Table.Columns.Contains(binder.Name))
            {
                result = _dataRow[binder.Name];
                return true;
            }
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_dataRow.Table.Columns.Contains(binder.Name))
            {
                _dataRow[binder.Name] = value;
                return true;
            }
            return false;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            string[] columnNames = new string[_dataRow.Table.Columns.Count];
            int i = 0;
            foreach( DataColumn column in _dataRow.Table.Columns)
            {
                columnNames[i++] = column.ColumnName;
            }
            return columnNames;
        }
    }
}
