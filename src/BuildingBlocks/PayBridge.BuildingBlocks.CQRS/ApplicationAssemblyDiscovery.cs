using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.CQRS
{
    internal class ApplicationAssemblyDiscovery
    {
        private const string ApplicationAssemblyPrefix = "Paybridge.Modules.";
        private const string ApplicationAssemblySuffix = ".Application";

        public static Assembly[] DiscoverApplicationAssemblies()
        {
            var dependencyContext = DependencyContext.Default;
            if (dependencyContext is null)
            {
                throw new InvalidOperationException("DependencyContext couldn't be loaded. Application assemblies can't be discovered.");
            }

            var assemblyNames = dependencyContext.CompileLibraries
                .Where(lib => lib.Name.StartsWith(ApplicationAssemblyPrefix, StringComparison.OrdinalIgnoreCase) &&
                              lib.Name.EndsWith(ApplicationAssemblySuffix, StringComparison.OrdinalIgnoreCase))
                .Select(lib => lib.Name)
                .Distinct()
                .OrderBy(name=>name)
                .ToArray();

            if (assemblyNames.Length == 0)
            {
                throw new InvalidOperationException(" No module application were found." +
                    "Expected at least one assembly with name starting with 'Paybridge.Modules.' and ending with '.Application'.");
            }

            return assemblyNames.Select(Assembly.Load).ToArray();
        }
    }
}
