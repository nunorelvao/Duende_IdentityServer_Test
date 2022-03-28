using System.Text.Json;

namespace WeatherMVC.Utils
{
    public static class JsonTextSerializerExtensions
    {
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
      

        //////// VERY IMPORTANT NOTE -- FOR USE WITH System.Text.Json *********
        /// Use this code for deserialize becasue by default uses case sensitive properties mapping

        public static T? DeserializeFromJson<T>(this string value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(value, options);
        }

        public static string SerializeToJson<T>(this T value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Serialize(value, options);
        }
    }
}




