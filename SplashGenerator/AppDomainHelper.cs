namespace SplashGenerator
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A helper to ease dealing with <see cref="AppDomain"/> specific tasks.
    /// </summary>
    internal class AppDomainHelper
    {
        private readonly string _baseDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainHelper"/> class.
        /// </summary>
        /// <param name="baseDirectory">The base directory in which additional assemblies should be resolved.</param>
        public AppDomainHelper(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        /// <summary>
        /// Invokes the specified function in a temporary separate domain.
        /// </summary>
        /// <typeparam name="TA1">The type of the argument 1.</typeparam>
        /// <typeparam name="TA2">The type of the argument 2.</typeparam>
        /// <typeparam name="TA3">The type of the argument 3.</typeparam>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="func">The function.</param>
        /// <param name="arg1">The argument 1 of the function.</param>
        /// <param name="arg2">The argument 2 of the function.</param>
        /// <param name="arg3">The argument 3 of the function.</param>
        /// <returns>
        /// The result of the function.
        /// </returns>
        public T InvokeInSeparateDomain<TA1, TA2, TA3, T>(Func<TA1, TA2, TA3, T> func, TA1 arg1, TA2 arg2, TA3 arg3)
        {
            Contract.Requires(func != null);

            return InternalInvokeInSeparateDomain<T>(func, arg1, arg2, arg3);
        }

        /// <summary>
        /// A typed wrapper for <see cref="AppDomain.CreateInstanceAndUnwrap(string, string)"/>
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <param name="appDomain">The application domain.</param>
        /// <returns>The proxy of the unwrapped type.</returns>
        private static T CreateInstanceAndUnwrap<T>(AppDomain appDomain) where T : class
        {
            Contract.Requires(appDomain != null);
            Contract.Ensures(Contract.Result<T>() != null);

            return (T)appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }

        private T InternalInvokeInSeparateDomain<T>(Delegate func, params object[] args)
        {
            Contract.Requires(func != null);
            Contract.Requires(args != null);

            var friendlyName = "Temporary domain for " + func.Method.Name;
            var appDomain = AppDomain.CreateDomain(friendlyName, null, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), string.Empty, false);

            Contract.Assume(appDomain != null);

            try
            {
                var helper = CreateInstanceAndUnwrap<DomainHelper>(appDomain);

                var result = helper.Invoke(_baseDirectory, func.Method, func.Target, args);

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
            public object Invoke(string baseDirectory, MethodInfo method, object target, object[] args)
            {
                try
                {
                    if (baseDirectory != null)
                    {
                        var assemblies = Directory.EnumerateFiles(baseDirectory)
                            .Select(TryLoadAssembly)
                            .Where(assembly => assembly != null)
                            .ToArray();

                        AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) => assemblies.FirstOrDefault(assembly => assembly.FullName.Equals(eventArgs.Name, StringComparison.OrdinalIgnoreCase));
                    }

                    return method.Invoke(target, args);
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }

            private static Assembly TryLoadAssembly(string fileName)
            {
                try
                {
                    return Assembly.LoadFile(fileName);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
