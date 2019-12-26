using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;

namespace API.Tests
{
    /*
    public class UnitTest1
    {
        [Theory]
        [MemberData("TestData", MemberType = typeof(TestDataProvider))]
        public void Test1(string testName, MemberDataSerializer<TestData> testCase)
        {
            Assert.Equal(1, testCase.Object.IntProp);
        }
    }

    public class TestDataProvider
    {
        public static IEnumerable<object[]> TestData()
        {
            yield return new object[] { "test1", new MemberDataSerializer<TestData>(new TestData { IntProp = 1, StringProp = "hello" }) };
            yield return new object[] { "test2", new MemberDataSerializer<TestData>(new TestData { IntProp = 2, StringProp = "Myro" }) };
        }
    }

    public class TestData
    {
        public int IntProp { get; set; }
        public string StringProp { get; set; }
    }
    
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
    }*/
}
