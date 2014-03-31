Overview
=============
A lightweight, high performance [MessagePack](http://msgpack.org/) framework for the CLR that works in constrained environments like AOT under Unity and Xamarin.

Usage
=====
This framework is purpose-built for moving DTO's around in a compact and fast way. Therefore, it requires that you specify the order of serialized properties so that we can avoid packing things like property names into payloads. The easiest way to do this is with attributes.

```c#
// Declare your DTO/message class
public class MyMessage
{
    // Use the [MsgPack] annotation to specify the sequence order of (de)serialized properties
    [MsgPack(Sequence = 10)]
    public string MyString { get; set; }
    // Sequence values specify an order, but they need not be consecutive (here we count by 10 to make it easier to add new props later without changing the others)
    [MsgPack(Sequence = 20)]
    public int MyNumber { get; set; }
    [MsgPack(Sequence = 30)]
    public List<MyMessage> MyChildren { get; set; }
    [MsgPack(Sequence = 30)]
    public Dictionary<string,string> MyMetadata { get; set; }
}

// Serialize an instances of your DTO's
var msg = new MyMessage() { MyString = "Hello!" };
byte[] payload = msg.ToMsgPack();

// Deserialize an instance of your DTO
var restoredMsg = MsgPackSerializer.Deserialize<MyMessage>(payload);
```

You can also directly (de)serialize primitives instead of a complex type as the top of the hierarchy:

```c#
var myStuff = new Dictionary<string,string>();
myStuff["Foo"] = "bar";
myStuff["Bar"] = "baz";
byte[] payload = myStuff.ToMsgPack();

float myNumber = 12.0f;
byte[] payload = myNumber.ToMsgPack();
```

The framework is safe to use with Unity and Xamarin, even under AOT (e.g. for iOS).

It uses reflection to discover the memory layout of classes for reading and writing with the MessagePack format, but it caches these reflections for fast access when repeatedly reusing the same Types. The small amount of reflection that is used is safe for Unity's subset of .NET.

Scope and Limitations
=============
This framework is built to be fast and useful, not exhaustive. It supports any referential hierarchy of complex types (your own classes). But the only collections that it supports are `List<T>` and `Dictionary<T,U>`. It does not support raw arrays. It supports the following primitives:
* Strings of any length
* float and double
* sbyte, byte, ushort, short, uint, int, ulong, long
