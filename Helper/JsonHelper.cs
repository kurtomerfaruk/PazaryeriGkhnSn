using Newtonsoft.Json;

namespace Pazaryeri.Helper
{
    public class JsonHelper
    {
        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }

        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
