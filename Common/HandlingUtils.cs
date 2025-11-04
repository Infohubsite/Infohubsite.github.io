namespace Frontend.Common
{
    public static class HandlingUtils
    {
        public static async Task<T> HandleAsync<T>(Func<Task<T>> func, Func<Exception, Task<T>> fail)
        {
            try { return await func(); }
            catch (Exception ex) { return await fail(ex); }
        }
        public static async Task<T> HandleAsync<T>(Func<Task<T>> func, Func<Exception, T> fail)
        {
            try { return await func(); }
            catch (Exception ex) { return fail(ex); }
        }
    }
}
