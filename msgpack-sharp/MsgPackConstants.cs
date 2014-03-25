using System;
using System.Reflection;
using System.IO;

namespace scopely.msgpacksharp
{
    internal static class MsgPackConstants
    {
        public const int MAX_PROPERTY_COUNT = 15;
		public const byte POSITIVE_FIXINT_MAX = 0x7f;
		public const int NEGATIVE_FIXINT_MIN = -31;
        
        public static class Formats 
        {
            public const byte FLOAT_32 = 0xca;
            public const byte FLOAT_64 = 0xcb;
            public const byte DOUBLE = 0xcb;
            public const byte STR_8 = 0xd9;
            public const byte STRING_8 = 0xd9;
            public const byte STR_16 = 0xda;
            public const byte STRING_16 = 0xda;
            public const byte STR_32 = 0xdb;
            public const byte STRING_32 = 0xdb;
			public const byte NIL = 0xc0;
        }
        
        public static class FixedString
        {
            public const byte MIN = 0xa0;
            public const byte MAX = 0xbf;
            public const int MAX_LENGTH = 31;
        }
        
        public static class FixedMap
        {
            public const byte MIN = 0x80;
            public const byte MAX = 0x8f;
        }
    }
}

