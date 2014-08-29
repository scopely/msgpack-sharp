using System;
using System.Collections.Generic;

namespace scopely.msgpacksharp
{
    public class SerializationContext
    {
        internal Dictionary<Type, MsgPackSerializer> Serializers { get; private set; }
        private SerializationMethod _serializationMethod;
        public SerializationMethod SerializationMethod
        {
            get { return _serializationMethod; }
            set
            {
                if (_serializationMethod != value)
                {
                    switch (value)
                    {
                        case SerializationMethod.Array:
                        case SerializationMethod.Map:
                            _serializationMethod = value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("value");
                    }    
                    Serializers = new Dictionary<Type, MsgPackSerializer>();
                }
            }
        }

        public SerializationContext()
        {
            Serializers = new Dictionary<Type, MsgPackSerializer>();
            _serializationMethod = SerializationMethod.Array;
        }
    }
}