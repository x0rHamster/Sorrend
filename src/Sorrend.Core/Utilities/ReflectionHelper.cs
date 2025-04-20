namespace Sorrend.Core.Utilities
{
    public static class ReflectionHelper
    {
        public static string GetTypeName<T>()
        {
            return typeof(T).FullName
                ?? throw new TypeArgumentException($"The property \"FullName\" is empty for the type \"{typeof(T)}\".");
        }
    }
}
