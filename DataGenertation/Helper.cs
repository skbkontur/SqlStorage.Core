﻿using System;
using System.Reflection;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.DataGenertation
{
    internal static class Helper
    {
        public static void FillWithRandomTrash(object obj, Random random, int fillRate, int stringsLength, int arraysSize)
        {
            var type = obj.GetType();
            var properties = typePropertiesCache.Get(type);
            var isNull = new bool[properties.Length];
            for (var i = 0; i < isNull.Length; ++i)
                isNull[i] = CanBeNull(properties[i].PropertyType) && random.Next(101) > fillRate;
            for (var index = 0; index < properties.Length; index++)
            {
                if (isNull[index]) continue;
                var property = properties[index];
                var propertyType = property.PropertyType;
                var setter = property.GetSetMethod();
                if (!propertyType.IsArray)
                {
                    if (IsALeaf(propertyType))
                        setter.Invoke(obj, new[] {GetRandomValue(propertyType, random, stringsLength)});
                    else
                    {
                        var constructorInfo = typeConstructorCache.Get(propertyType);
                        var child = constructorInfo.Invoke(new object[0]);
                        setter.Invoke(obj, new[] {child});
                        FillWithRandomTrash(child, random, fillRate, stringsLength, arraysSize);
                    }
                }
                else
                {
                    var elementType = propertyType.GetElementType();
                    var length = random.Next(arraysSize, arraysSize * 2);
                    var array = Array.CreateInstance(elementType, length);
                    setter.Invoke(obj, new[] {array});
                    if (IsALeaf(elementType))
                    {
                        for (var i = 0; i < length; ++i)
                            array.SetValue(GetRandomValue(elementType, random, stringsLength), i);
                    }
                    else
                    {
                        var constructorInfo = typeConstructorCache.Get(elementType);
                        for (var i = 0; i < length; ++i)
                            array.SetValue(constructorInfo.Invoke(new object[0]), i);
                        for (var i = 0; i < length; ++i)
                            FillWithRandomTrash(array.GetValue(i), random, fillRate, stringsLength, arraysSize);
                    }
                }
            }
        }

        private static string RandomString(Random random, int length, char first, char last)
        {
            var arr = new char[length];
            for (var i = 0; i < length; ++i)
                arr[i] = (char)random.Next(first, last + 1);
            return new string(arr);
        }

        private static bool CanBeNull(Type type)
        {
            return type.IsClass || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        private static object GetRandomValue(Type type, Random random, int stringsLength)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return GetRandomValue(type.GetGenericArguments()[0], random, stringsLength);
            if (type == typeof(Guid))
                return Guid.NewGuid();
            switch (Type.GetTypeCode(type))
            {
            case TypeCode.Boolean:
                return random.Next(2) == 0;
            case TypeCode.Byte:
                return (byte)(random.Next(256));
            case TypeCode.SByte:
                return (sbyte)(random.Next(256) - 128);
            case TypeCode.UInt16:
                return (ushort)(random.Next(65536));
            case TypeCode.Int16:
                return (short)(random.Next(65536) - 32768);
            case TypeCode.Char:
                return (char)random.Next('a', 'z' + 1);
            case TypeCode.UInt32:
                return (uint)(random.Next(65536)) | (((uint)(random.Next(65536))) << 16);
            case TypeCode.Int32:
                return (int)(((uint)(random.Next(65536)) | (((uint)(random.Next(65536))) << 16)) - (1U << 31));
            case TypeCode.UInt64:
                return RandomUInt64(random);
            case TypeCode.Int64:
                return (long)(RandomUInt64(random) - (1UL << 63));
            case TypeCode.DateTime:
                return new DateTime(DateTime.MinValue.Ticks + (long)(RandomUInt64(random) % ((ulong)(DateTime.MaxValue.Ticks - DateTime.MinValue.Ticks + 1))));
            case TypeCode.Single:
                return (float)random.NextDouble();
            case TypeCode.Double:
                return random.NextDouble();
            case TypeCode.String:
                return RandomString(random, random.Next(stringsLength, stringsLength * 3), 'a', 'z');
            default:
                throw new NotSupportedException();
            }
        }

        private static ulong RandomUInt64(Random random)
        {
            var word1 = (ulong)random.Next(65536);
            var word2 = ((ulong)random.Next(65536)) << 16;
            var word3 = ((ulong)random.Next(65536)) << 32;
            var word4 = ((ulong)random.Next(65536)) << 48;
            return word1 | word2 | word3 | word4;
        }

        private static bool IsALeaf(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsALeaf(type.GetGenericArguments()[0]));
        }

        private static readonly ConcurrentCache<Type, PropertyInfo[]> typePropertiesCache = new ConcurrentCache<Type, PropertyInfo[]>(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public));
        private static readonly ConcurrentCache<Type, ConstructorInfo> typeConstructorCache = new ConcurrentCache<Type, ConstructorInfo>(type => type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null));
    }
}