using System;
using System.Data;
using System.Data.Common;
namespace GGWebLib
{
    /// <summary>
    /// <para>对DataRow和DbDataReader的统一包装类。</para>
    /// <para>因为这二个类虽然都提供了“索引器”，但它们没有共同的基类或实现接口，所以通常只能写“二套代码”。</para>
    /// <para>使用这个MyDataAdapter，可以提供一致的“索引器”。</para>
    /// </summary>
    public class MyDataAdapter
    {
        private interface IMyBaseDataRow
        {
            object GetValue(string fieldName);
            void SetNewRow(object row);
            string[] GetColumnNames();
        }
        private sealed class MyDataRow : MyDataAdapter.IMyBaseDataRow
        {
            private DataRow m_dataRow;
            public MyDataRow(DataRow row)
            {
                if (row == null)
                {
                    throw new ArgumentNullException("row");
                }
                this.m_dataRow = row;
            }
            public object GetValue(string fieldName)
            {
                return this.m_dataRow[fieldName];
            }
            public void SetNewRow(object row)
            {
                this.m_dataRow = (DataRow)row;
            }
            public string[] GetColumnNames()
            {
                string[] array = new string[this.m_dataRow.Table.Columns.Count];
                for (int i = 0; i < this.m_dataRow.Table.Columns.Count; i++)
                {
                    array[i] = this.m_dataRow.Table.Columns[i].ColumnName;
                }
                return array;
            }
        }
        private sealed class MyDataReader : MyDataAdapter.IMyBaseDataRow
        {
            private DbDataReader m_reader;
            public MyDataReader(DbDataReader reader)
            {
                if (reader == null)
                {
                    throw new ArgumentNullException("reader");
                }
                this.m_reader = reader;
            }
            public object GetValue(string fieldName)
            {
                return this.m_reader[fieldName];
            }
            public void SetNewRow(object reader)
            {
            }
            public string[] GetColumnNames()
            {
                int fieldCount = this.m_reader.FieldCount;
                string[] array = new string[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                {
                    array[i] = this.m_reader.GetName(i);
                }
                return array;
            }
        }
        private MyDataAdapter.IMyBaseDataRow m_rowOrReader;
        /// <summary>
        /// 用于保存额外的用户上下文数据
        /// </summary>
        public object UserData;
        /// <summary>
        /// 用于从DataRow或DataReader中，根据字段名获取相应字段的值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public object this[string fieldName]
        {
            get
            {
                return this.m_rowOrReader.GetValue(fieldName);
            }
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="row"></param>
        public MyDataAdapter(DataRow row)
        {
            this.m_rowOrReader = new MyDataAdapter.MyDataRow(row);
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="reader"></param>
        public MyDataAdapter(DbDataReader reader)
        {
            this.m_rowOrReader = new MyDataAdapter.MyDataReader(reader);
        }
        /// <summary>
        /// <para>对于DataTable执行foreach操作时，没有必要为每个DataRow创建一个MyDataAdapter对象，可以调用这个方法设置“当前行”</para>
        /// <para>如果是对于同一个DbDataReader，执行这个调用就没有意义了。</para>
        /// <para>注意：新行的【字段列表】一定要和“前一行”是一样的。（此处代码不检查，但可能会引发异常）</para>
        /// </summary>
        /// <param name="row">参数的类型只能是：DataRow 或 DbDataReader，否则会抛出异常</param>
        public void SetCurrentRow(object row)
        {
            this.m_rowOrReader.SetNewRow(row);
        }
        /// <summary>
        /// 获取当前数据行所属表的列名称
        /// </summary>
        /// <param name="toUpper">是否转成大写</param>
        /// <returns>数据行所属表的所有列名称</returns>
        public string[] GetColumnNames(bool toUpper)
        {
            string[] columnNames = this.m_rowOrReader.GetColumnNames();
            if (columnNames == null || columnNames.Length == 0 || !toUpper)
            {
                return columnNames;
            }
            for (int i = 0; i < columnNames.Length; i++)
            {
                columnNames[i] = columnNames[i].ToUpper();
            }
            return columnNames;
        }
    }
}
