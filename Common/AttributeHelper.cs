using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Common
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class FieldIgnoreAttribute : Attribute
    {
    }
}
