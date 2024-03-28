using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TelegramServer.Common;

public class IgnoreJsonPropertyContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        return base.CreateProperties(type, memberSerialization)
            .Select(p =>
            {
                p.PropertyName = p.UnderlyingName;
                return p;
            })
            .ToList();
    }
}