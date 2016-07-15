namespace SplashGenerator
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// A helper to ease dealing with <see cref="AppDomain"/> specific tasks.
    /// </summary>
    public static class AppDomainHelper
    {
        /// <summary>
        /// Invokes the specified function in a temporary separate domain.
        /// </summary>
        /// <typeparam name="TA1">The type of the arguments.</typeparam>
        /// <typeparam name="TA2">The type of the arguments.</typeparam>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="func">The function.</param>
        /// <param name="arg1">The arguments of the function.</param>
        /// <param name="arg2">The arguments of the function.</param>
        /// <returns>
        /// The result of the function.
        /// </returns>
        public static T InvokeInSeparateDomain<TA1, TA2, T>(this Func<TA1, TA2, T> func, TA1 arg1, TA2 arg2)
        {
            Contract.Requires(func != null);

            return InternalInvokeInSeparateDomain<T>(func, arg1, arg2);
        }


        /// <summary>
        /// Invokes the specified function in a temporary separate domain.
        /// </summary>
        /// <typeparam name="TA1">The type of the arguments.</typeparam>
        /// <typeparam name="TA2">The type of the arguments.</typeparam>
        /// <typeparam name="TA3">The type of the arguments.</typeparam>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="func">The function.</param>
        /// <param name="arg1">The arguments of the function.</param>
        /// <param name="arg2">The arguments of the function.</param>
        /// <param name="arg3">The arguments of the function.</param>
        /// <returns>
        /// The result of the function.
        /// </returns>
        public static T InvokeInSeparateDomain<TA1, TA2, TA3, T>(this Func<TA1, TA2, TA3, T> func, TA1 arg1, TA2 arg2, TA3 arg3)
        {
            Contract.Requires(func != null);

            return InternalInvokeInSeparateDomain<T>(func, arg1, arg2, arg3);
        }

        /// <summary>
        /// A wrapper for <see cref="AppDomain.CreateInstanceAndUnwrap(string, string)"/>
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <param name="appDomain">The application domain.</param>
        /// <returns>The proxy of the unwrapped type.</returns>
        public static T CreateInstanceAndUnwrap<T>(this AppDomain appDomain) where T : class
        {
            Contract.Requires(appDomain != null);
            Contract.Ensures(Contract.Result<T>() != null);

            return (T)appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }

        private static T InternalInvokeInSeparateDomain<T>(Delegate func, params object[] args)
        {
            Contract.Requires(func != null);
            Contract.Requires(args != null);

            var friendlyName = "Temporary domain for " + func.Method.Name;
            var currentDomain = AppDomain.CurrentDomain;
            var appDomain = AppDomain.CreateDomain(friendlyName, currentDomain.Evidence, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), string.Empty, false);

            Contract.Assume(appDomain != null);

            try
            {
                var helper = appDomain.CreateInstanceAndUnwrap<DomainHelper>();

                var result = helper.Invoke(func.Method, func.Target, args);

                HandleException(result as Exception);

                return result == null ? default(T) : (T)result;

            }
            finally
            {
                AppDomain.Unload(appDomain);
            }
        }

        private static void HandleException(Exception exception)
        {
            if (exception == null)
                return;

            while (exception is TargetInvocationException)
                exception = exception.InnerException;

            throw exception;
        }


        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Created in another AppDomain.")]
        private class DomainHelper : MarshalByRefObject
        {
            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            public object Invoke(MethodInfo method, object target, object[] args)
            {
                try
                {
                    return method.Invoke(target, args);
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
        }
    }
}
