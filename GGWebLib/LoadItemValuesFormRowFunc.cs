using System;
namespace GGWebLib
{
    /// <summary>
    /// 从一条数据库记录加载实体对象的方法委托
    /// </summary>
    /// <param name="row">一条数据库记录</param>
    /// <param name="item">实体对象</param>
    /// <param name="loadAllField">是否加载全部字段</param>
    /// <param name="checkSucessCount">是否要检查成功数量</param>
    /// <returns>成功加载的字段数量</returns>
    public delegate int LoadItemValuesFormRowFunc(MyDataAdapter row, object item, bool loadAllField, bool checkSucessCount);
}
