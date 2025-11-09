using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BTL_QlBi_a.Extensions
{
    public class UnicodeEnumConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : struct, Enum
    {
        public UnicodeEnumConverter()
            : base(
                v => ConvertToString(v),
                v => ConvertToEnum(v))
        {
        }

        private static string ConvertToString(TEnum enumValue)
        {
            var memberInfo = typeof(TEnum).GetMember(enumValue.ToString()).FirstOrDefault();
            if (memberInfo != null)
            {
                var enumMemberAttr = memberInfo.GetCustomAttributes(typeof(System.Runtime.Serialization.EnumMemberAttribute), false)
                    .FirstOrDefault() as System.Runtime.Serialization.EnumMemberAttribute;

                if (enumMemberAttr != null && !string.IsNullOrEmpty(enumMemberAttr.Value))
                {
                    return enumMemberAttr.Value;
                }
            }
            return enumValue.ToString();
        }

        private static TEnum ConvertToEnum(string value)
        {
            if (string.IsNullOrEmpty(value))
                return default;

            foreach (var field in typeof(TEnum).GetFields())
            {
                if (field.IsLiteral)
                {
                    var enumMemberAttr = field.GetCustomAttributes(typeof(System.Runtime.Serialization.EnumMemberAttribute), false)
                        .FirstOrDefault() as System.Runtime.Serialization.EnumMemberAttribute;

                    if (enumMemberAttr != null && enumMemberAttr.Value == value)
                    {
                        return (TEnum)field.GetValue(null);
                    }

                    if (field.Name == value)
                    {
                        return (TEnum)field.GetValue(null);
                    }
                }
            }

            return default;
        }
    }
}
