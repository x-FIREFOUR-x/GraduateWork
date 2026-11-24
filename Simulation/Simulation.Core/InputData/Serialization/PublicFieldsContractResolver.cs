using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace Simulation.InputData.Serialization
{
    internal sealed class PublicFieldsContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                       .Select(field => CreateProperty(field, memberSerialization))
                       .ToList();
        }
    }
}
