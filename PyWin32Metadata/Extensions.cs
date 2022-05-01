using System;
using System.Linq;
using System.Reflection.Metadata;

namespace PyWin32Metadata
{
    public static class Extensions
    {
        public static (string, string) GetFullName(this MetadataReader reader, InterfaceImplementationHandle implementation) => GetFullName(reader, reader.GetTypeReference((TypeReferenceHandle)reader.GetInterfaceImplementation(implementation).Interface));
        public static (string, string) GetFullName(this MetadataReader reader, InterfaceImplementation implementation) => GetFullName(reader, reader.GetTypeReference((TypeReferenceHandle)implementation.Interface));
        public static (string, string) GetFullName(this MetadataReader reader, TypeReference type)
        {
            var ns = reader.GetString(type.Namespace);
            var name = reader.GetString(type.Name);
            return (ns, name);
        }

        public static (string, string) GetFullName(this MetadataReader reader, TypeDefinition type)
        {
            var ns = reader.GetString(type.Namespace);
            var name = reader.GetString(type.Name);
            return (ns, name);
        }

        public static MemberReference GetMember(this MetadataReader reader, CustomAttribute attribute) => reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
        public static TypeReference GetType(this MetadataReader reader, CustomAttribute attribute) => reader.GetTypeReference((TypeReferenceHandle)GetMember(reader, attribute).Parent);
        public static (string, string) GetFullName(this MetadataReader reader, CustomAttributeHandle attribute) => GetFullName(reader, GetType(reader, reader.GetCustomAttribute(attribute)));
        public static (string, string) GetFullName(this MetadataReader reader, CustomAttribute attribute) => GetFullName(reader, GetType(reader, attribute));
        public static CustomAttributeValue<object?> GetValue(this MetadataReader reader, CustomAttribute attribute) => attribute.DecodeValue(CustomAttributeTypeProvider.Instance);

        public static Guid? GetInteropGuid(this MetadataReader reader, TypeDefinition type)
        {
            var guid = type.GetCustomAttributes().Where(h => reader.GetFullName(reader.GetCustomAttribute(h)) == ("Windows.Win32.Interop", "GuidAttribute")).FirstOrDefault();
            if (guid.IsNil)
                return null;

            return GetInteropGuid(reader, reader.GetCustomAttribute(guid));
        }

        public static Guid? GetInteropGuid(this MetadataReader reader, CustomAttribute attribute)
        {
            var value = GetValue(reader, attribute);
            if (value.FixedArguments.Length != 11)
                return null;

#pragma warning disable CS8605 // Unboxing a possibly null value.
            return new Guid(
                (uint)value.FixedArguments[0].Value,
                (ushort)value.FixedArguments[1].Value,
                (ushort)value.FixedArguments[2].Value,
                (byte)value.FixedArguments[3].Value,
                (byte)value.FixedArguments[4].Value,
                (byte)value.FixedArguments[5].Value,
                (byte)value.FixedArguments[6].Value,
                (byte)value.FixedArguments[7].Value,
                (byte)value.FixedArguments[8].Value,
                (byte)value.FixedArguments[9].Value,
                (byte)value.FixedArguments[10].Value);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }
    }
}
