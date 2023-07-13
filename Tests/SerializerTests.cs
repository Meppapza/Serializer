using JsonLibrary;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

[TestClass]
public class JsonSerializerTests
{
    private JsonSerializer serializer;

    [TestInitialize]
    public void Setup()
    {
        serializer = new JsonSerializer();
    }

    [TestMethod]
    public void Serialize_PrimitiveTypes_ReturnsValidJson()
    {
        // Arrange
        int intValue = 42;
        double doubleValue = 3.14;
        string stringValue = "Hello, World!";
        bool boolValue = true;

        // Act
        string intJson = serializer.Serialize(intValue);
        string doubleJson = serializer.Serialize(doubleValue);
        string stringJson = serializer.Serialize(stringValue);
        string boolJson = serializer.Serialize(boolValue);

        // Assert
        Assert.AreEqual("42", intJson);
        Assert.AreEqual("3.14", doubleJson.Replace(',', '.'));
        Assert.AreEqual("\"Hello, World!\"", stringJson);
        Assert.AreEqual("true", boolJson);
    }

    [TestMethod]
    public void Serialize_ComplexObject_ReturnsValidJson()
    {
        // Arrange
        Person person = new Person { Name = "Artemiy", Age = 19 };
        string expectedJson = "{\"class\": \"Person\", \"Name\": \"Artemiy\", \"Age\": 19, \"Address\": null}";

        // Act
        string json = serializer.Serialize(person);

        // Assert
        Assert.AreEqual(expectedJson, json);
    }

    [TestMethod]
    public void Serialize_List_ReturnsValidJson()
    {
        // Arrange
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
        string expectedJson = "[1, 2, 3, 4, 5]";

        // Act
        string json = serializer.Serialize(numbers);

        // Assert
        Assert.AreEqual(expectedJson, json);
    }

    [TestMethod]
    public void Serialize_Dictionary_ReturnsValidJson()
    {
        // Arrange
        Dictionary<string, int> dictionary = new Dictionary<string, int>
        {
            { "A", 1 },
            { "B", 2 },
            { "C", 3 }
        };
        string expectedJson = "{\"A\": 1, \"B\": 2, \"C\": 3}";

        // Act
        string json = serializer.Serialize(dictionary);

        // Assert
        Assert.AreEqual(expectedJson, json);
    }

    [TestMethod]
    public void Deserialize_Null_ReturnsDefaultValue()
    {
        // Arrange
        string json = "null";

        // Act
        var result = serializer.Deserialize(json);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Deserialize_String_ReturnsValidValue()
    {
        // Arrange
        string json = "\"Hello, World!\"";

        // Act
        var result = serializer.Deserialize(json);

        // Assert
        Assert.AreEqual("Hello, World!", result);
    }

    [TestMethod]
    public void Deserialize_Array_ReturnsValidArray()
    {
        // Arrange
        string json = "[1, 2, 3]";

        // Act
        var result = serializer.Deserialize(json) as object[];

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Length);
        Assert.AreEqual(1, result[0]);
        Assert.AreEqual(2, result[1]);
        Assert.AreEqual(3, result[2]);
    }

    [TestMethod]
    public void Deserialize_Dictionary_ReturnsValidDictionary()
    {
        // Arrange
        string json = "{\"A\": 1, \"B\": 2, \"C\": 3}";

        // Act
        var result = serializer.Deserialize(json) as Dictionary<string, object>;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(1, result["A"]);
        Assert.AreEqual(2, result["B"]);
        Assert.AreEqual(3, result["C"]);
    }

    [TestMethod]
    public void Deserialize_PrimitiveTypes_ReturnsValidValue()
    {
        // Arrange
        string intJson = "42";
        string doubleJson = "3.14";
        string boolJson = "true";

        // Act
        int intValue = (int)serializer.Deserialize(intJson);
        double doubleValue = (double)serializer.Deserialize(doubleJson);
        bool boolValue = (bool)serializer.Deserialize(boolJson);

        // Assert
        Assert.AreEqual(42, intValue);
        Assert.AreEqual(3.14, doubleValue, 0.001);
        Assert.AreEqual(true, boolValue);
    }

    [TestMethod]
    public void Serialize_NestedObjects_ReturnsValidJson()
    {
        // Arrange
        Address address = new Address { City = "Vladivostok", Country = "Russia" };
        Person person = new Person { Name = "Sonya", Age = 20, Address = address };
        string expectedJson = "{\"class\": \"Person\", \"Name\": \"Sonya\", \"Age\": 20, \"Address\": {\"class\": \"Address\", \"City\": \"Vladivostok\", \"Country\": \"Russia\"}}";

        // Act
        string json = serializer.Serialize(person);

        // Assert
        Assert.AreEqual(expectedJson, json);
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
    }
    public class Address
    {
        public string City { get; set; }
        public string Country { get; set; }
    }
}