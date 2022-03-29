using System.Text.Json;

namespace WeatherMVC.Utils
{
    public static class JsonTextSerializerExtensions
    {
        //////// VERY IMPORTANT NOTE -- FOR USE WITH NewtsoftJson for retro compatibility and aproach the Generics serializers *********
        //        /// <summary>
        //        /// Serialize object to json
        //        /// </summary>
        //        /// <typeparam name="T">Object type</typeparam>
        //        /// <param name="item">Object instance to serialize</param>
        //        /// <returns>Serialized json value</returns>
        //        public static string SerializeToJson<T>(this T item) => JsonConvert.SerializeObject(item);
        //        /// <summary>
        //        /// Deserialize json value to an object instance
        //        /// </summary>
        //        /// <typeparam name="T">Object type</typeparam>
        //        /// <param name="value">Serialized json value</param>
        //        /// <returns>Object instance</returns>
        //        public static T DeserializeFromJson<T>(this string value) => JsonConvert.DeserializeObject<T>(value);


        //////// VERY IMPORTANT NOTE: as default Json Serializer of Microsoft is case sensitive -- FOR USE WITH new .net core System.Text.Json *********
        /// <summary>
        /// Deserializes from json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="usePropertyNameCaseInsensitive">if set to <c>true</c> [use property name case insensitive].</param>
        /// <returns></returns>
        /// Use this code for deserialize becasue by default uses case sensitive properties mapping

        public static T? DeserializeFromJson<T>(this string value, bool usePropertyNameCaseInsensitive)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = usePropertyNameCaseInsensitive
            };

            return JsonSerializer.Deserialize<T>(value, options);
        }

        /// <summary>
        /// Serializes to json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="usePropertyNameCaseInsensitive">if set to <c>true</c> [use property name case insensitive].</param>
        /// <returns></returns>
        public static string SerializeToJson<T>(this T value, bool usePropertyNameCaseInsensitive)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = usePropertyNameCaseInsensitive
            };

            return JsonSerializer.Serialize(value, options);
        }

    }
}




