using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
namespace SpExport.Util.Extention
{
    public static class ValidType
    {
        public static bool IsValidTypeToConvert(this string fieldType)
        {
            switch (fieldType.ToLower())
            {
                //case "ContentTypeId":
                //    break;
                case "text":
                    return true;
                case "note":
                    return true;
                case "number":
                    return true;
                case "calculated":
                    return true;
                case "currency":
                    return true;
                case "datetime":
                    return true;
                case "lookup":
                    return true;
                case "boolean":
                    return true;
                //case "User ":
                //    break;
                case "url":
                    return true;
                case "outcomechoice":
                    return true;
                case "counter":
                    return true;
            }

            return false;
        }

        public static string ToSqlDataType(this string fieldType)
        {
            //https://msdn.microsoft.com/en-us/library/dd588079(v=office.11).aspx
            switch (fieldType.ToLower())
            {
                case "counter":
                    return "int";
                case "text":
                    return "nvarchar(255)";
                case "lookup":
                case "calculated":
                case "multichoice":
                case "usermulti":
                    return "nvarchar(450)";
                case "long text":
                    return "nvarchar(450)";
                case "note":
                    return "ntext";

                case "integer":
                    return "int";
                case "number":
                case "currency":
                    return "float";
                case "datetime":
                    return "datetime";
                case "url":
                    return "nvarchar(4000)";
                case "boolean":
                    return "bit";
                case "outcomechoice":
                case "choice":
                    return "nvarchar(255)";
                //case "choice (integer)":
                //    return "int";


            }
            return string.Empty;
        }

        public static string IsNull(this bool isTrue)
        {
            return isTrue ? " NOT NULL " : " NULL ";
        }
        public static object GetSharepointFieldValue(this object obj, string fieldType)
        {
            ////if (obj is  int)
            ////{
            ////    return null;
            ////}else if (obj is string)
            ////{

            ////}
            ////else if (obj is float)
            ////{

            ////}
            switch (fieldType.ToLower())
            {
                case "boolean":
                    if (obj ==null)
                    {
                        return "null";
                    }
                    return string.Format("'{0}'", obj);
                case "calculated":
                    string cv = string.Empty;
                    if (obj == null)
                    {
                        cv = "";
                    }
                    else if (obj is FieldCalculatedErrorValue)
                    {
                        cv = ((FieldCalculatedErrorValue)obj).ErrorMessage;
                    }

                    else
                    {
                        cv = obj.ToString();
                    }

                    //Microsoft.SharePoint.Client.FieldCalculated c;
                    //Microsoft.SharePoint.Client.FieldCalculatedErrorValue cc;
                    //cc.
                    return string.Format("N'{0}'", cv);
                case "lookup":
                    string tt = string.Empty;
                    var value = obj as FieldLookupValue;
                    if (value != null)
                    {
                        tt = value.LookupValue;
                    }
                    else if (obj is string)
                    {
                        tt = obj.ToString();
                    }

                    return string.Format("N'{0}'", tt);

                case "usermulti":
                    if (obj == null)
                    {
                        obj = new FieldUserValue();
                    }
                    var users = (FieldUserValue[])obj;
                    string[] vls = users.Select(x => x.LookupValue).ToArray();
                    var userstr = string.Join("$#$", vls);
                    return string.Format("N'{0}'", userstr);
                case "multichoice":
                    if (obj == null)
                    {
                        obj = new string[1];
                    }
                    string str = string.Join("$#$", (string[])obj);
                    return string.Format("N'{0}'", str);
                case "url":
                    string url = string.Empty;
                    FieldUrlValue urlValue = obj as FieldUrlValue;
                    if (urlValue != null)
                    {
                       url= urlValue.Url;
                    }
                    return string.Format("N'{0}'", url);
                case "number":
                case "currency":
                case "integer":
                case "counter":
                    if (obj == null)
                    {
                        return "null";
                    }
                    return obj;
                
                case "text":
                case "long text":
                case "note":
                    if (obj == null)
                    {
                        return  "null"  ;
                    }
                    return string.Format("N'{0}'", obj);
                case "outcomechoice":
                case "choice":
                    if (obj == null)
                    {
                        obj = "Choice-1";
                    }
                    return string.Format("N'{0}'", obj);
                case "datetime":
                    if (obj == null)
                    {
                        obj = "1999-09-09";
                    }
                    return string.Format("'{0}'", obj);

            }
            return obj;
        }
    }
}
