using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace JsonLibrary
{
    public class JsonSerializer
    {
        public string Serialize(object obj)
        {
            if (obj == null)
                return "null";

            Type objType = obj.GetType();

            if (IsPrimitiveType(objType))
                return SerializePrimitive(obj);

            if (objType == typeof(string))
                return $"\"{obj}\"";

            if (objType.IsArray)
                return SerializeArray((Array)obj);

            if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(List<>))
                return SerializeList((dynamic)obj);

            if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return SerializeDictionary((dynamic)obj);

            return SerializeObject(obj);
        }

        private bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive || type == typeof(decimal) || type == typeof(string) || type == typeof(bool);
        }

        private string EscapeString(string value)
        {
            StringBuilder builder = new StringBuilder();

            foreach (char c in value)
            {
                switch (c)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (char.IsControl(c))
                        {
                            builder.Append("\\u");
                            builder.Append(((int)c).ToString("x4"));
                        }
                        else
                        {
                            builder.Append(c);
                        }
                        break;
                }
            }

            return builder.ToString();
        }

        private string SerializePrimitive(object obj)
        {
            if (obj is string)
            {
                string stringValue = (string)obj;
                StringBuilder builder = new StringBuilder();
                builder.Append("\"");
                builder.Append(EscapeString(stringValue));
                builder.Append("\"");
                return builder.ToString();
            }
            else if (obj is bool)
            {
                return obj.ToString().ToLower();
            }

            return obj.ToString();
        }

        private string SerializeArray(Array arr)
        {
            List<string> elements = new List<string>();

            foreach (object obj in arr)
            {
                elements.Add(Serialize(obj));
            }

            return $"[{string.Join(", ", elements)}]";
        }

        private string SerializeList<T>(List<T> list)
        {
            List<string> elements = new List<string>();

            foreach (T item in list)
            {
                elements.Add(Serialize(item));
            }

            return $"[{string.Join(", ", elements)}]";
        }

        private string SerializeDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            List<string> elements = new List<string>();

            foreach (KeyValuePair<TKey, TValue> kvp in dict)
            {
                string key = SerializePrimitive(kvp.Key);
                string value = Serialize(kvp.Value);
                elements.Add($"{key}: {value}");
            }

            return $"{{{string.Join(", ", elements)}}}";
        }

        private string SerializeObject(object obj)
        {
            Type objType = obj.GetType();
            PropertyInfo[] properties = objType.GetProperties();

            List<string> elements = new List<string>();

            foreach (PropertyInfo property in properties)
            {
                string propName = property.Name;
                object propValue = property.GetValue(obj);
                string serializedValue = Serialize(propValue);
                elements.Add($"\"{propName}\": {serializedValue}");
            }

            elements.Insert(0, $"\"class\": \"{objType.Name}\"");

            return $"{{{string.Join(", ", elements)}}}";
        }

        public object Deserialize(string s)
        {
            if (s == "null")
                return null;

            if (s.StartsWith("\"") && s.EndsWith("\""))
                return s.Trim('"');

            if (s.StartsWith("[") && s.EndsWith("]"))
                return DeserializeArray(s);

            if (s.StartsWith("{") && s.EndsWith("}"))
                return DeserializeObject(s);

            return DeserializePrimitive(s);
        }

        private object DeserializePrimitive(string s)
        {
            if (bool.TryParse(s, out bool boolValue))
                return boolValue;

            if (int.TryParse(s, out int intValue))
                return intValue;

            if (long.TryParse(s, out long longValue))
                return longValue;

            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
                return Convert.ToDouble(floatValue);

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue))
                return doubleValue;

            if (decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal decimalValue))
                return decimalValue;

            throw new ArgumentException("Invalid primitive value: " + s);
        }

        private object[] DeserializeArray(string s)
        {
            s = s.Trim('[', ']');
            string[] elements = s.Split(',');

            object[] arr = new object[elements.Length];

            for (int i = 0; i < elements.Length; i++)
            {
                arr[i] = Deserialize(elements[i]);
            }

            return arr;
        }

        private object DeserializeObject(string s)
        {
            if (s.StartsWith("{") && s.EndsWith("}"))
            {
                Dictionary<string, string> keyValuePairs = ParseObjectProperties(s);

                if (keyValuePairs.ContainsKey("class"))
                    return DeserializeClassObject(keyValuePairs);

                return DeserializeDictionaryObject(keyValuePairs);
            }

            throw new ArgumentException("Invalid JSON object: " + s);
        }

        private object DeserializeClassObject(Dictionary<string, string> keyValuePairs)
        {
            string className = keyValuePairs["class"];
            keyValuePairs.Remove("class");

            Type objType = Type.GetType(className);
            if (objType == null)
                throw new ArgumentException("Invalid class type: " + className);

            object obj = Activator.CreateInstance(objType);

            foreach (var kvp in keyValuePairs)
            {
                string propName = kvp.Key;
                string propValue = kvp.Value;

                PropertyInfo property = objType.GetProperty(propName);
                if (property != null)
                {
                    object value = Deserialize(propValue);
                    property.SetValue(obj, value);
                }
            }

            return obj;
        }

        private object DeserializeDictionaryObject(Dictionary<string, string> keyValuePairs)
        {
            var dict = new Dictionary<string, object>();

            foreach (var kvp in keyValuePairs)
            {
                string key = kvp.Key;
                string value = kvp.Value;

                object deserializedValue = Deserialize(value);
                dict[key] = deserializedValue;
            }

            return dict;
        }

        private Dictionary<string, string> ParseObjectProperties(string s)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            int startIndex = s.IndexOf('{') + 1;
            int endIndex = s.LastIndexOf('}');

            string propertiesStr = s.Substring(startIndex, endIndex - startIndex);
            List<string> properties = SplitProperties(propertiesStr);

            foreach (string property in properties)
            {
                string[] keyValue = property.Split(':', 2);
                string key = keyValue[0].Trim().Trim('"');
                string value = keyValue[1].Trim().Trim('"');
                keyValuePairs[key] = value;
            }

            return keyValuePairs;
        }

        static List<string> SplitProperties(string s)
        {
            List<string> result = new List<string>();

            int start = 0;
            int end = 0;
            int curlyBracesCount = 0;
            bool withinQuotes = false;

            while (end < s.Length)
            {
                if (s[end] == '\"')
                {
                    withinQuotes = !withinQuotes;
                }

                if (s[end] == '{')
                {
                    curlyBracesCount++;
                }
                else if (s[end] == '}')
                {
                    curlyBracesCount--;
                }

                if (s[end] == ',' && curlyBracesCount == 0 && !withinQuotes)
                {
                    result.Add(s.Substring(start, end - start).Trim());
                    start = end + 1;
                }

                end++;
            }

            if (start < s.Length)
            {
                result.Add(s.Substring(start).Trim());
            }

            return result;
        }
    }
}