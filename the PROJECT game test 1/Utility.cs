using static System.Enum;
using static System.IO.File;
using static System.Convert;
namespace Top_Down_Game
{
    public static class Utility
    {
        public static T ToEnum<T>(this string value) // Checks the string and compares it to the existing enum types
        {
            return (T)Parse(typeof(T), value); // Returns the enum type that matches the string name
        }
        public static int[] ToIntArray(this string[] array)
        {
            int[] ints = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                ints[i] = ToInt32(array[i]);
            }
            return ints;
        }
    }
}
