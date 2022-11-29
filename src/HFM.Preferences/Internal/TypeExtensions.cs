namespace HFM.Preferences.Internal;

internal static class TypeExtensions
{
    private static bool IsGenericList(this Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

    private static bool IsGenericIEnumerable(this Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);

    private static bool ImplementsGenericIEnumerable(this Type type) =>
        type.GetInterfaces().Where(x => x.IsGenericType).Select(x => x.GetGenericTypeDefinition()).Contains(typeof(IEnumerable<>));

    private static bool GenericArgumentsEquals(this Type type1, Type type2) =>
        type1.GetGenericArguments().SequenceEqual(type2.GetGenericArguments());

    internal static bool CanBeCreatedFrom(this Type type1, Type type2) =>
        type1.IsGenericList()
        && (type2.IsGenericIEnumerable() || type2.ImplementsGenericIEnumerable())
        && type1.GenericArgumentsEquals(type2);
}
