using System;
using System.Globalization;

namespace Common
{
    public static class EnumUtil
    {
        public static bool IsFlagSet<TEnum>(this TEnum flags, TEnum flagToFind) where TEnum : struct
        {
            return IsFlagSet(Convert.ToInt32(flags, CultureInfo.CurrentCulture), Convert.ToInt32(flagToFind, CultureInfo.CurrentCulture));
        }

        public static bool IsFlagSet(int flags, int flagToFind)
        {
            return (flags & flagToFind) == flagToFind;
        }

        public static bool IsFlagSet(long flags, long flagToFind)
        {
            return (flags & flagToFind) == flagToFind;
        }

        public static long ClearFlag(long flags, long flagToClear)
        {
            if (IsFlagSet(flags, flagToClear)) flags &= ~flagToClear;
            return flags;
        }

        public static TEnum ClearFlag<TEnum>(this TEnum flags, TEnum flagToClear) where TEnum : struct
        {
            var i = ClearFlag(Convert.ToInt64(flags, CultureInfo.CurrentCulture), Convert.ToInt64(flagToClear, CultureInfo.CurrentCulture));
            return (TEnum)Enum.ToObject(typeof(TEnum), i);
        }

        public static long SetFlag(long flags, long flagToSet)
        {
            if (!IsFlagSet(flags, flagToSet)) flags |= flagToSet;
            return flags;
        }

        public static TEnum SetFlag<TEnum>(this TEnum flags, TEnum flagToSet) where TEnum : struct
        {
            var i = SetFlag(Convert.ToInt64(flags, CultureInfo.CurrentCulture), Convert.ToInt64(flagToSet, CultureInfo.CurrentCulture));
            return (TEnum)Enum.ToObject(typeof(TEnum), i);
        }
    }
}