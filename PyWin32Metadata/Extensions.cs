using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace PyWin32Metadata
{
    public static class Extensions
    {
        public static (string, string) GetFullName(this MetadataReader reader, EntityHandle handle)
        {
            if (handle.IsNil)
                return ("", "");

            switch (handle.Kind)
            {
                case HandleKind.TypeDefinition:
                    return GetFullName(reader, (TypeDefinitionHandle)handle);

                case HandleKind.TypeReference:
                    return GetFullName(reader, (TypeReferenceHandle)handle);

                default:
                    throw new NotSupportedException();
            }
        }

        public static (string, string) GetFullName(this MetadataReader reader, InterfaceImplementationHandle implementation) => GetFullName(reader, reader.GetTypeReference((TypeReferenceHandle)reader.GetInterfaceImplementation(implementation).Interface));
        public static (string, string) GetFullName(this MetadataReader reader, InterfaceImplementation implementation) => GetFullName(reader, reader.GetTypeReference((TypeReferenceHandle)implementation.Interface));
        public static (string, string) GetFullName(this MetadataReader reader, TypeReferenceHandle type) => GetFullName(reader, reader.GetTypeReference(type));
        public static (string, string) GetFullName(this MetadataReader reader, TypeReference type)
        {
            var ns = reader.GetString(type.Namespace);
            var name = reader.GetString(type.Name);
            return (ns, name);
        }

        public static (string, string) GetFullName(this MetadataReader reader, TypeDefinitionHandle type) => GetFullName(reader, reader.GetTypeDefinition(type));
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
        public static CustomAttributeValue<object?> GetValue(CustomAttribute attribute) => attribute.DecodeValue(CustomAttributeTypeProvider.Instance);

        public static bool IsNativeTypedef(this MetadataReader reader, TypeDefinition type) => type.GetCustomAttributes().Any(h => reader.GetFullName(reader.GetCustomAttribute(h)) == ("Windows.Win32.Interop", "NativeTypedefAttribute"));
        public static bool IsUnmanagedFunctionPointer(this MetadataReader reader, TypeDefinition type) => type.GetCustomAttributes().Any(h => reader.GetFullName(reader.GetCustomAttribute(h)) == ("System.Runtime.InteropServices", "UnmanagedFunctionPointerAttribute"));
        public static bool IsHandle(this MetadataReader reader, TypeDefinition type)
        {
            if (!IsNativeTypedef(reader, type))
                return false;

            var fields = type.GetFields();
            if (fields.Count != 1)
                return false;

            var field = reader.GetFieldDefinition(fields.First());
            if (!field.Attributes.HasFlag(FieldAttributes.Public) || field.Attributes.HasFlag(FieldAttributes.Static))
                return false;

            var sig = field.DecodeSignature(SignatureTypeProvider.Instance, null);
            return sig.FullName == ("System", "IntPtr");
        }

        public static bool IsFunction(this MetadataReader reader, TypeDefinition type)
        {
            var bfn = reader.GetFullName(type.BaseType);
            if (bfn != ("System", "MulticastDelegate"))
                return false;

            return IsUnmanagedFunctionPointer(reader, type);
        }

        public static bool IsStructure(this MetadataReader reader, TypeDefinition type)
        {
            if (!type.Attributes.HasFlag(TypeAttributes.Public | TypeAttributes.SequentialLayout))
                return false;

            var bfn = reader.GetFullName(type.BaseType);
            if (bfn != ("System", "ValueType"))
                return false;

            // if structure has only one "Value" field, it's not really a structure (WPARAM, etc.)
            if (IsNativeTypedef(reader, type) && type.GetFields().Count == 1)
            {
                var field = reader.GetFieldDefinition(type.GetFields().First());
                if (string.Compare(reader.GetString(field.Name), "value", StringComparison.OrdinalIgnoreCase) == 0)
                    return false;
            }
            return true;
        }

        public static Guid? GetInteropGuid(this MetadataReader reader, TypeDefinition type)
        {
            var guid = type.GetCustomAttributes().Where(h => reader.GetFullName(reader.GetCustomAttribute(h)) == ("Windows.Win32.Interop", "GuidAttribute")).FirstOrDefault();
            if (guid.IsNil)
                return null;

            return GetInteropGuid(reader.GetCustomAttribute(guid));
        }

        public static Guid? GetInteropGuid(CustomAttribute attribute)
        {
            var value = GetValue(attribute);
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
