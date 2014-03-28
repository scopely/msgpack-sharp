msgpack-sharp
=============

A lightweight, high performance msgpack framework for the CLR that works in constrained environments like AOT under Unity and Xamarin

Usage
=====
This framework is purpose-built for moving DTO's around in a compact and fast way. Therefore, it requires that you specify the order of serialized properties so that we can avoid packing things like property names into payloads. The easiest way to do this framework is with annotations.

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

Scope and Limitations
=============

