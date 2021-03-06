// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.EntityModel.Swagger.Internal
{
    using System;
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class SwaggerObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(SwaggerObjectBase).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var swaggerBase = (SwaggerObjectBase)value;
            switch (swaggerBase.ObjectType)
            {
                case SwaggerObjectType.ReferenceObject:
                    {
                        var swagger = (SwaggerReferenceObject)swaggerBase;
                        var jObject = JObject.FromObject(swagger.Reference, serializer);

                        // Preserve value inside swagger.Token
                        foreach (var k in swagger.Token)
                        {
                            JToken existing;
                            if (jObject.TryGetValue(k.Key, out existing))
                            {
                                throw new JsonException($"{k.Key} is already defined in referenced object \"{swagger.DeferredReference}\".");
                            }

                            jObject.Add(k.Key, k.Value);
                        }

                        jObject.WriteTo(writer);
                    }
                    break;
                case SwaggerObjectType.Object:
                    {
                        var swagger = (SwaggerObject)swaggerBase;
                        var jObject = new JObject();
                        foreach (var i in swagger.Dictionary)
                        {
                            jObject.Add(i.Key, JToken.FromObject(i.Value, serializer));
                        }
                        jObject.WriteTo(writer);
                    }
                    break;
                case SwaggerObjectType.Array:
                    {
                        var swagger = (SwaggerArray)swaggerBase;
                        var jArray = JArray.FromObject(swagger.Array.Select(s => JToken.FromObject(s, serializer)));
                        jArray.WriteTo(writer);
                    }
                    break;
                case SwaggerObjectType.ValueType:
                    {
                        var swagger = (SwaggerValue)swaggerBase;
                        swagger.Token.WriteTo(writer);
                    }
                    break;
                default:
                    throw new NotSupportedException(swaggerBase.ObjectType.ToString());
            }
        }
    }
}
