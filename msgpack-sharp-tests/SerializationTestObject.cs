using System;
using System.Collections.Generic;
using MsgPack.Serialization;
using TestEnum = scopely.msgpacksharp.tests.SerializationTests.TestEnum;

namespace scopely.msgpacksharp.tests
{
    public class SerializationTestObject
    {
        public SerializationTestObject()
        {
            strDict = new Dictionary<string, string>();
            strDict.Add("TESTKEY", "TESTVALUE");

            nullDict = new Dictionary<string, string>();
            nullDict.Add("TESTNULLKEY", null);

            intDict = new Dictionary<int, int>();
            intDict.Add(int.MinValue, int.MaxValue);

            shortDict = new Dictionary<short, short>();
            shortDict.Add(short.MinValue, short.MaxValue);

            ushortDict = new Dictionary<ushort, ushort>();
            ushortDict.Add(ushort.MinValue, ushort.MaxValue);

            longDict = new Dictionary<long, long>();
            longDict.Add(long.MinValue, long.MaxValue);

            floatDict = new Dictionary<float, float>();
            floatDict.Add(float.MinValue, float.MaxValue);

            doubleDict = new Dictionary<double, double>();
            doubleDict.Add(double.MinValue, double.MaxValue);

            byteDict = new Dictionary<byte, byte>();
            byteDict.Add(byte.MinValue, byte.MaxValue);

            charDict = new Dictionary<char, char>();
            charDict.Add(char.MinValue, char.MaxValue);

            enumDict = new Dictionary<TestEnum, TestEnum>();
            enumDict.Add(TestEnum.ENTRY_0, TestEnum.ENTRY_1);

            boolDict = new Dictionary<bool, bool>();
            boolDict.Add(true, false);

            strList = new List<string>();
            strList.Add("TESTENTRY1");
            strList.Add("TESTENTRY2");

            nullList = new List<string>();
            nullList.Add(null);
            nullList.Add(null);

            intList = new List<int>();
            nullList.Add(null);
            intList.Add(int.MinValue);
            intList.Add(int.MaxValue);

            shortList = new List<short>();
            shortList.Add(short.MinValue);
            shortList.Add(short.MaxValue);

            ushortList = new List<ushort>();
            ushortList.Add(ushort.MinValue);
            ushortList.Add(ushort.MaxValue);

            longList = new List<long>();
            longList.Add(long.MinValue);
            longList.Add(long.MaxValue);

            floatList = new List<float>();
            floatList.Add(float.MinValue);
            floatList.Add(float.MaxValue);

            doubleList = new List<double>();
            doubleList.Add(double.MinValue);
            doubleList.Add(double.MaxValue);

            byteList = new List<byte>();
            byteList.Add(byte.MinValue);
            byteList.Add(byte.MaxValue);

            charList = new List<char>();
            charList.Add(char.MinValue);
            charList.Add(char.MaxValue);

            enumList = new List<TestEnum>();
            enumList.Add(TestEnum.ENTRY_0);
            enumList.Add(TestEnum.ENTRY_1);

            boolList = new List<bool>();
            boolList.Add(true);
            boolList.Add(false);

            child = null;
        }

        [MessagePackMember(0)] Dictionary<string, string> strDict { get; set; }
        [MessagePackMember(1)] Dictionary<string, string> nullDict { get; set; }
        [MessagePackMember(2)] Dictionary<int,int> intDict { get; set; }
        //[MessagePackMember()] Dictionary<uint,uint> uintDict { get; set; }
        [MessagePackMember(3)] Dictionary<short,short> shortDict { get; set; }
        [MessagePackMember(4)] Dictionary<ushort,ushort> ushortDict { get; set; }
        [MessagePackMember(5)] Dictionary<long,long> longDict { get; set; }
        //[MessagePackMember()] Dictionary<ulong, ulong> ulongDict { get; set; }
        [MessagePackMember(6)] Dictionary<float, float> floatDict { get; set; }
        [MessagePackMember(7)] Dictionary<double, double> doubleDict { get; set; }
        [MessagePackMember(8)] Dictionary<byte, byte> byteDict { get; set; }
        //[MessagePackMember()] //Dictionary<sbyte, sbyte> sbyteDict { get; set; }
        [MessagePackMember(9)] Dictionary<char, char> charDict { get; set; }
        [MessagePackMember(10)] Dictionary<TestEnum, TestEnum> enumDict { get; set; }
        [MessagePackMember(11)] List<string> strList { get; set; }
        [MessagePackMember(12)] List<string> nullList { get; set; }
        [MessagePackMember(13)] List<int> intList { get; set; }
        //[MessagePackMember()] List<uint> uintList { get; set; }
        [MessagePackMember(14)] List<short> shortList { get; set; }
        [MessagePackMember(15)] List<ushort> ushortList { get; set; }
        [MessagePackMember(16)] List<long> longList { get; set; }
        //[MessagePackMember()] List<ulong> ulongList { get; set; }
        [MessagePackMember(17)] List<float> floatList { get; set; }
        [MessagePackMember(18)] List<double> doubleList { get; set; }
        [MessagePackMember(19)] List<byte> byteList { get; set; }
        //[MessagePackMember()] Listry<sbyte> sbyteList { get; set; }
        [MessagePackMember(20)] List<char> charList { get; set; }
        [MessagePackMember(21)] List<TestEnum> enumList { get; set; }
        [MessagePackMember(22)] SerializationTestObject child { get; set; }
        [MessagePackMember(23)] Dictionary<bool, bool> boolDict { get; set; }
        [MessagePackMember(24)] List<bool> boolList { get; set; }

        public SerializationTestObject AddChild()
        {
            child = new SerializationTestObject();
            return this;
        }

        public override int GetHashCode()
        {
            return strDict.GetHashCode() + nullDict.GetHashCode() +
            intDict.GetHashCode() + shortDict.GetHashCode() + ushortDict.GetHashCode() +
            longDict.GetHashCode() + floatDict.GetHashCode() + doubleDict.GetHashCode() +
            byteDict.GetHashCode() + charDict.GetHashCode() + enumDict.GetHashCode() +
            strList.GetHashCode() + nullList.GetHashCode() + intList.GetHashCode() +
            shortList.GetHashCode() + ushortList.GetHashCode() + longList.GetHashCode() +
            floatList.GetHashCode() + doubleList.GetHashCode() + byteList.GetHashCode() +
            charList.GetHashCode() + enumList.GetHashCode() + boolDict.GetHashCode() + 
            boolList.GetHashCode();
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            SerializationTestObject sto = obj as SerializationTestObject;
            if ((System.Object)sto == null)
            {
                return false;
            }

            // THIS CHECK FAILS! When AddChild() is used!
            bool childProof = false;
            if (child == null && sto.child == null)
                childProof = true;
            else if(child != null)
                childProof = child.Equals(sto.child);

            // Return true if the fields match:
            return childProof && AreDictsEqual<string, string>(strDict, sto.strDict) &&
            AreDictsEqual<string, string>(nullDict, sto.nullDict) &&
            AreDictsEqual<int, int>(intDict, sto.intDict) &&
            AreDictsEqual<short, short>(shortDict, sto.shortDict) &&
            AreDictsEqual<ushort, ushort>(ushortDict, sto.ushortDict) &&
            AreDictsEqual<long, long>(longDict, sto.longDict) &&
            AreDictsEqual<float, float>(floatDict, sto.floatDict) &&
            AreDictsEqual<double, double>(doubleDict, sto.doubleDict) &&
            AreDictsEqual<byte, byte>(byteDict, sto.byteDict) &&
            AreDictsEqual<char, char>(charDict, sto.charDict) &&
            AreDictsEqual<TestEnum, TestEnum>(enumDict, sto.enumDict) &&
            AreListsEqual<string>(strList, sto.strList) &&
            AreListsEqual<string>(nullList, sto.nullList) &&
            AreListsEqual<int>(intList, sto.intList) &&
            AreListsEqual<short>(shortList, sto.shortList) &&
            AreListsEqual<ushort>(ushortList, sto.ushortList) &&
            AreListsEqual<long>(longList, sto.longList) &&
            AreListsEqual<float>(floatList, sto.floatList) &&
            AreListsEqual<double>(doubleList, sto.doubleList) &&
            AreListsEqual<byte>(byteList, sto.byteList) &&
            AreListsEqual<char>(charList, sto.charList) &&
            AreListsEqual<TestEnum>(enumList, sto.enumList) &&
            AreDictsEqual<bool, bool>(boolDict, sto.boolDict) &&
            AreListsEqual<bool>(boolList, sto.boolList);
        }

        private static bool AreDictsEqual<Key, Value> (Dictionary<Key, Value> dict1,
            Dictionary<Key, Value> dict2)
        {
            bool same = true;

            if (dict1.Count != dict2.Count)
                return false;

            foreach(Key k in dict1.Keys)
            {
                if(dict1[k] == null)
                {
                    if (dict2[k] != null)
                        same = false;
                }
                else if (!dict1[k].Equals(dict2[k]))
                    same = false;
            }

            return same;
        }

        private static bool AreListsEqual<T> (List<T> list1, List<T> list2)
        {
            bool same = true;

            if (list1.Count != list2.Count)
                return false;

            for(int i = 0; i < list1.Count; ++i)
            {
                if(list1[i] == null)
                {
                    if (list2[i] != null)
                        same = false;
                }
                else if (!list1[i].Equals(list2[i]))
                    same = false;
            }

            return same;
        }
    }
}