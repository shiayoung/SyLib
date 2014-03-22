namespace GGWebLib
{
    using System;

    internal static class SR
    {
        public static readonly string AjaxAssemblyNameIsNull = "AjaxMethodV1Handler.AjaxAssemblyName 没有设置。";
        public static readonly string CanNotShareConnection = "不能共享连接，因为属性DbContext为空。";
        public static readonly string ClassNameSearchPatternIsError = "ClassNameSearchPattern [{0}] 格式无效。";
        public static readonly string ClassNameSearchPatternIsNull = "AjaxClassSearchHelper.ClassNameSearchPattern 没有设置。";
        public static readonly string CommandTextIsNotSP = "命令不是存储过程，或没有设置存储过程。";
        public static readonly string ConfigItemNotFound = "配置项: [{0}] 不存在。";
        public static readonly string ConfigItemNotSet = "配置项 [{0}] 没有被设置。";
        public static readonly string ConfigItemRegistered = "配置项 [{0}] 已经注册过了。";
        public static readonly string DbContextNotNull = "DbContext成员已经设置过了。";
        public static readonly string DbFileNotIncludeTables = "数据文件中不包含任何数据表";
        public static readonly string HexLenIsWrong = "十六进制的字节数组的长度不正确。";
        public static readonly string IE_DateItemIsNull = "Internal Error, SpOutParamDescription.DataItem is null.";
        public static readonly string InvalidRequest = "无效的请求，不能被解析为类/方法的调用。";
        public static readonly string KeyNotFoundInRequest = "当前请求中没有找到名为 [{0}] 的参数项。";
        public static readonly string LoadUCFailed = "加载用户控件 [{0}] 失败.";
        public static readonly string MemberNotFound = "属性或字段 [{0}] 没有找到。";
        public static readonly string MethodCalled = "方法已经被调用过了。";
        public static readonly string MethodNotFound = "根据指定的方法名 [{0}] 找不到相应的实现方法。";
        public static readonly string ModelTypeIsIncorrect = "方法返回的结果类型并不是期望的实体类型。";
        public static readonly string MySqlClientFactoryNotFound = "类型 MySqlClientFactory 没有找到。";
        public static readonly string NeedInitGGDbContext = "GGDbContext没有找到需要的默认设置，请先调用GGDbContext.Init().";
        public static readonly string NotFoundMethod = "没有找到指定的方法。";
        public static readonly string NotReturnModel = "方法没有返回所期望的实体对象。";
        public static readonly string PropertyNotFound = "属性 [{0}] 没有找到。";
        public static readonly string SetSpOutError_CanNotSet = "设置存储过程输出参数值失败，因为写实体成员 [{0}] 失败。";
        public static readonly string SetSpOutError_CmdParamNotFound = "设置存储过程输出参数值失败，因为命令参数 [{0}] 不存在。";
        public static readonly string SetSpOutError_MemberNotFound = "设置存储过程输出参数值失败，因为实体成员 [{0}] 不存在。";
        public static readonly string StringFormatInvalid = "要拆分的字符串的格式无效。";
        public static readonly string TypeIsNotEnum = "指定的类型不是枚举类型。";
        public static readonly string TypeNotFound = "类型 [{0}] 没有找到。";
        public static readonly string TypeNotGuidprimaryKeyField = "类型 [{0}] 没有定义 Guid 主关键字。";
        public static readonly string TypeNotIdprimaryKeyField = "类型 [{0}] 没有定义 ID 主关键字。";
        public static readonly string UrlParameterNotSet = "URL参数项 [{0}] 没有设置。";
        public static readonly string ViewDataTypeIsWrong = "参数viewData的类型与用户控件的参数类型不一致。";
    }
}

