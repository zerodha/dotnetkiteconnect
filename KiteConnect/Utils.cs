using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Web;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace KiteConnect
{
    public class Utils
    {
        /// <summary>
        /// Convert string to Date object
        /// </summary>
        /// <param name="obj">Date string.</param>
        /// <returns>Date object/</returns>
        public static DateTime? StringToDate(string DateString)
        {
            try
            {
                if(DateString.Length == 10)
                {
                    return DateTime.ParseExact(DateString, "yyyy-MM-dd", null);
                }else
                {
                    return DateTime.ParseExact(DateString, "yyyy-MM-dd HH:mm:ss", null);
                }
            }catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Serialize C# object to JSON string.
        /// </summary>
        /// <param name="obj">C# object to serialize.</param>
        /// <returns>JSON string/</returns>
        public static string JsonSerialize(object obj)
        {
            var jss = new JavaScriptSerializer();
            string json = jss.Serialize(obj);
            MatchCollection mc = Regex.Matches(json, @"\\/Date\((\d*?)\)\\/");
            foreach (Match m in mc)
            {
                UInt64 unix = UInt64.Parse(m.Groups[1].Value) / 1000;
                json = json.Replace(m.Groups[0].Value, UnixToDateTime(unix).ToString());
            }
            return json;
        }

        /// <summary>
        /// Deserialize Json string to nested string dictionary.
        /// </summary>
        /// <param name="Json">Json string to deserialize.</param>
        /// <returns>Json in the form of nested string dictionary.</returns>
        public static Dictionary<string, dynamic> JsonDeserialize(string Json)
        {
            var jss = new JavaScriptSerializer();
            Dictionary<string, dynamic> dict = jss.Deserialize<Dictionary<string, dynamic>>(Json);
            // Replace double with decimal in the map
            dict = DoubleToDecimal(dict);
            return dict;
        }

        /// <summary>
        /// Recursively traverses an object and converts double fields to decimal.
        /// This is used in Json deserialization. JavaScriptSerializer converts floats
        /// in exponential notation to double and everthing else to double. This function
        /// makes everything decimal. Currently supports only Dictionary and Array as input.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <returns>Object with decimals instead of doubles</returns>
        public static dynamic DoubleToDecimal(dynamic obj)
        {
            if (obj is double)
            {
                obj = Convert.ToDecimal(obj);
            }
            else if (obj is IDictionary)
            {
                var keys = new List<string>(obj.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    obj[keys[i]] = DoubleToDecimal(obj[keys[i]]);
                }
            }
            else if (obj is ICollection)
            {
                obj = new ArrayList(obj);
                for (int i = 0; i < obj.Count; i++)
                {
                    obj[i] = DoubleToDecimal(obj[i]);
                }
            }
            return obj;
        }

        /// <summary>
        /// Parse instruments API's CSV response.
        /// </summary>
        /// <param name="Data">Response of instruments API.</param>
        /// <returns>CSV data as array of nested string dictionary.</returns>
        public static List<Dictionary<string, dynamic>> ParseCSV(string Data)
        {
            string[] lines = Data.Split('\n');

            List<Dictionary<string, dynamic>> instruments = new List<Dictionary<string, dynamic>>();

            using (TextFieldParser parser = new TextFieldParser(StreamFromString(Data)))
            {
                // parser.CommentTokens = new string[] { "#" };
                parser.SetDelimiters(new string[] { "," });
                parser.HasFieldsEnclosedInQuotes = true;

                // Skip over header line.
                string[] headers = parser.ReadLine().Split(',');

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    Dictionary<string, dynamic> item = new Dictionary<string, dynamic>();

                    for (var i = 0; i < headers.Length; i++)
                        item.Add(headers[i], fields[i]);

                    instruments.Add(item);
                }
            }

            return instruments;
        }

        /// <summary>
        /// Wraps a string inside a stream
        /// </summary>
        /// <param name="value">string data</param>
        /// <returns>Stream that reads input string</returns>
        public static MemoryStream StreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        /// <summary>
        /// Helper function to add parameter to the request only if it is not null or empty
        /// </summary>
        /// <param name="Params">Dictionary to add the key-value pair</param>
        /// <param name="Key">Key of the parameter</param>
        /// <param name="Value">Value of the parameter</param>
        public static void AddIfNotNull(Dictionary<string, dynamic> Params, string Key, string Value)
        {
            if (!String.IsNullOrEmpty(Value))
                Params.Add(Key, Value);
        }

        /// <summary>
        /// Generates SHA256 checksum for login.
        /// </summary>
        /// <param name="Data">Input data to generate checksum for.</param>
        /// <returns>SHA256 checksum in hex format.</returns>
        public static string SHA256(string Data)
        {
            Console.WriteLine(Data);
            SHA256Managed sha256 = new SHA256Managed();
            StringBuilder hexhash = new StringBuilder();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(Data), 0, Encoding.UTF8.GetByteCount(Data));
            foreach (byte b in hash)
            {
                hexhash.Append(b.ToString("x2"));
            }
            return hexhash.ToString();
        }

        /// <summary>
        /// Creates key=value with url encoded value
        /// </summary>
        /// <param name="Key">Key</param>
        /// <param name="Value">Value</param>
        /// <returns>Combined string</returns>
        public static string BuildParam(string Key, dynamic Value)
        {
            if (Value is string)
            {
                return HttpUtility.UrlEncode(Key) + "=" + HttpUtility.UrlEncode((string)Value);
            }
            else
            {
                string[] values = (string[])Value;
                return String.Join("&", values.Select(x => HttpUtility.UrlEncode(Key) + "=" + HttpUtility.UrlEncode(x)));
            }
        }

        public static DateTime UnixToDateTime(UInt64 unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 5, 30, 0, 0, DateTimeKind.Unspecified);
            dateTime = dateTime.AddSeconds(unixTimeStamp);
            return dateTime;
        }

        public static List<decimal> ToDecimalList(ArrayList arrayList)
        {
            var res = new List<decimal>();
            foreach(var i in arrayList)
            {
                res.Add(Convert.ToDecimal(i));
            }
            return res;
        }
    }
}
