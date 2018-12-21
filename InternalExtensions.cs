using System;
using System.Reflection;

namespace Kahia.Web.VirtualPathProvider
{
    internal static class InternalExtensions
    {
        public static object GetFieldValueNonPublic(this object param, String FieldName)
        {
            return param.GetType().GetField(FieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(param);
        }
        public static Boolean IsNotNullAndEmptyString(this object value)
        {
            return value != null && !String.IsNullOrWhiteSpace(value.ToString());
        }

        public static String Append(this String value, String parameter)
        {
            return value.ToStringByDefaultValue() + parameter;
        }

        public static Boolean IsNullOrEmptyString(this object value)
        {
            return value == null || String.IsNullOrWhiteSpace(value.ToString());
        }
        public static String FormatString(this String value, params object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return value;
            return String.Format(value, parameters);
        }
        public static String ReplaceFromBeginning(this String value, String prefixToBeReplaced, String replaceValue, StringComparison stringComparison)
        {
            prefixToBeReplaced = prefixToBeReplaced.ToStringByDefaultValue();
            var result = value.StartsWith(prefixToBeReplaced, stringComparison) ? replaceValue.Append(value.Substring(prefixToBeReplaced.Length)) : value;
            return result;
        }

        public static String ToStringByDefaultValue(this object value, String defaultValue = "")
        {
            return value == null || String.IsNullOrWhiteSpace(value.ToString()) ? defaultValue : value.ToString();
        }
        
        public static String ReplaceFromBeginning(this String value, String prefixToBeReplaced, String replaceValue)
        {
            return value.StartsWith(prefixToBeReplaced, StringComparison.InvariantCultureIgnoreCase)
                ? replaceValue.Append(value.Substring(prefixToBeReplaced.Length))
                : value;
        }

        public static String RemoveFromBeginning(this String value, String prefixToBeReplaced)
        {
            return ReplaceFromBeginning(value, prefixToBeReplaced, String.Empty);
        }

        /// <summary>
        /// Belirtilen string verilen parametreyle başlıyorsa, o parametre belirtilen String'in başlangıcından çıkartılır.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="prefixToBeReplaced"></param>
        /// <param name="stringComparison"></param>
        /// <returns></returns>
        public static String RemoveFromBeginning(this String value, String prefixToBeReplaced, StringComparison stringComparison)
        {
            return ReplaceFromBeginning(value, prefixToBeReplaced, String.Empty, stringComparison);
        }

    }
}
