using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Net6_RabbitMQ.Extensions
{
    public static class IBasicPropertiesExtensions
    {
        public static int GetRetryCount(this IBasicProperties messageProperties)
        {
            var headers = messageProperties.Headers;
            var count = 0;

            if (headers?.ContainsKey(HeaderConstants.RETRY_HEADER) ?? false)
            {
                var countAsString = Convert.ToString(headers[HeaderConstants.RETRY_HEADER]);
                count = Convert.ToInt32(countAsString);
            }

            return count;
        }
        public static void CloneHeadersTo(this IBasicProperties sourceProperties, ref IBasicProperties destinationProperties)
        {
            destinationProperties.Headers = CloneHeaders(sourceProperties.Headers);

            IDictionary<string, object> CloneHeaders(IDictionary<string, object> sourceHeaders)
            {
                var clone = new Dictionary<string, object>();

                if (sourceHeaders == null) return clone;

                foreach (KeyValuePair<string, object> kvp in sourceHeaders)
                    clone.Add(kvp.Key, kvp.Value);

                return clone;
            }
        }
    }
}
