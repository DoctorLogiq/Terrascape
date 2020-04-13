using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terrascape.Debugging;

#nullable enable

namespace Terrascape.Helpers
{
	internal static class JsonHelper
	{
		internal static List<(string, object)> ParseJsonBasic(in string p_json)
        {
            List<(string, object)> list = new List<(string, object)>();
            
            string        key           = string.Empty;
            bool          is_array      = false;
            List<object>? array_list = null;

            using JsonTextReader reader = new JsonTextReader(new StringReader(p_json));
            while (reader.Read())
            {
                JsonToken token = reader.TokenType;
                object? value = reader.Value;

                Debug.LogDebug($"Token: '{token}' ¦ Value: '{value}'");
                
                switch (token)
                {
	                case JsonToken.None:
		                break;
	                case JsonToken.StartObject:
		                break;
	                case JsonToken.EndObject:
		                break;
	                case JsonToken.StartArray:
		                is_array = true;
		                break;
	                case JsonToken.EndArray:
		                System.Diagnostics.Debug.Assert(array_list != null, nameof(array_list) + " != null");
		                list.Add((key, array_list.ToArray()));
		                is_array = false;
		                break;
	                case JsonToken.StartConstructor:
		                break;
	                case JsonToken.PropertyName:
		                key = value as string;
		                break;
	                case JsonToken.Comment:
		                break;
	                case JsonToken.Raw:
		                break;
	                case JsonToken.Integer:
		                break;
	                case JsonToken.Float:
		                break;
	                case JsonToken.String:
		                if (is_array)
		                {
			                if (array_list == null)
				                array_list = new List<object>();
			                
			                array_list.Add(value);
		                }
		                else
		                {
			                list.Add((key, value as string));
		                }
		                break;
	                case JsonToken.Boolean:
		                break;
	                case JsonToken.Null:
		                break;
	                case JsonToken.Undefined:
		                break;
	                case JsonToken.EndConstructor:
		                break;
	                case JsonToken.Date:
		                break;
	                case JsonToken.Bytes:
		                break;
	                default:
		                throw new ArgumentOutOfRangeException();
                }
            }

            return list;
        }
	}
}