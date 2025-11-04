using Shared.Interface;

namespace Frontend.Extenstions
{
    public static class ConverterExtensions
    {
        public static TTo ConvertFrom<TTo, TFrom>(this TFrom from) where TTo : IConverterFrom<TTo, TFrom> => TTo.Convert(from);
        public static TTo ConvertTo<TTo, TFrom>(this TFrom from) where TFrom : IConverterTo<TFrom, TTo> => TFrom.Convert(from);
    }
}
