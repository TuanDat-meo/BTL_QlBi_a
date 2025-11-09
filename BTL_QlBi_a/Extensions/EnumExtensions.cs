using System.ComponentModel.DataAnnotations;

namespace BTL_QlBi_a.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this System.Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var displayAttribute = fieldInfo?.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];

            return displayAttribute?.Length > 0 ? displayAttribute[0].Name : enumValue.ToString();
        }
    }
}
