using System;
using System.Reflection;
using System.IO;

namespace scopely.msgpacksharp.enums
{
    internal static class MsgPackConstants
    {
        public enum Formats : byte
        {
            Float32 = 0xca,
            Float64 = 0xcb,
            Double = 0xcb,
            Str8 = 0xd9,
            String8 = 0xd9,
            Str16 = 0xda,
            String16 = 0xda,
            Str32 = 0xdb,
            String32 = 0xdb,
        }
    }
}

