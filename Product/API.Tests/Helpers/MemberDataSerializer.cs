using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace API.Tests.Helpers
{
    public class MemberDataSerializer<T> : IXunitSerializable
    {
        public T Object { get; private set; }

        public MemberDataSerializer()
        {
        }

        public MemberDataSerializer(T objectToSerialize)
        {
            Object = objectToSerialize;
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Object = JsonConvert.DeserializeObject<T>(info.GetValue<string>("objValue"));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            var json = JsonConvert.SerializeObject(Object);
            info.AddValue("objValue", json);
        }
    }
}
