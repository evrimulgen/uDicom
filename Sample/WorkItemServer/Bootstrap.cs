using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using uDicom.Common;

namespace WorkItemServer
{
    class Bootstrap
    {
        public CompositionContainer Container { get; set; }

        public void Configure()
        {
            var priorityAssemblies = SelectAssemblies().ToList();
            var priorityCatalog = new AggregateCatalog(priorityAssemblies.Select(x => new AssemblyCatalog(x)));
            var priorityProvider = new CatalogExportProvider(priorityCatalog);

            var path = Path.GetDirectoryName(
                new Uri(Assembly.GetExecutingAssembly().EscapedCodeBase).LocalPath);
            var codecCatalog = new DirectoryCatalog(path);
            var codecProvider = new CatalogExportProvider(codecCatalog);

            Container = new CompositionContainer(priorityProvider, codecProvider);
            priorityProvider.SourceProvider = Container;
            codecProvider.SourceProvider = Container;

            var batch = new CompositionBatch();

            batch.AddExportedValue(Container);

            Container.Compose(batch);

            IoC.GetInstance = GetInstance;
            IoC.GetAllInstances = GetAllInstances;
        }

        private object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = Container.GetExports<object>(contract);

            if (exports.Any())
                return exports.First().Value;

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        private IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return Container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        public IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { Assembly.GetEntryAssembly() };
        }
    }
}
