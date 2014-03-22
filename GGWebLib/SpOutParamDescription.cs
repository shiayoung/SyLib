using System;
using System.Collections.Generic;
namespace GGWebLib
{
    /// <summary>
    /// 用于描述一个存储过程命令的输出参数的情况
    /// </summary>
    internal sealed class SpOutParamDescription
    {
        public object DataItem;
        private List<string> m_NamesList;
        public List<string> NamesList
        {
            get
            {
                return this.m_NamesList;
            }
        }
        public void AddParamName(string name)
        {
            if (this.m_NamesList == null)
            {
                this.m_NamesList = new List<string>(10);
            }
            this.m_NamesList.Add(name);
        }
    }
}
