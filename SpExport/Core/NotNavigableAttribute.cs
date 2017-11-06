using System;

namespace SpExport.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotNavigableAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class IdentityAttribute : Attribute
    {
    }
}