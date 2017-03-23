using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using uDicom.Common;

namespace uDicom.WorkItemService.ShredHost
{
    internal sealed class WcfHelper
    {
        private enum HostBindingType
        {
            BasicHttp,
            WSHttp,
            WSDualHttp,
            NetTcp,
            NamedPipes
        }

        public static ServiceEndpointDescription StartBasicHttpHost<TServiceType, TServiceInterfaceType>(string name,
            string description, int port, string serviceAddressBase)
        {
            return StartHost<TServiceType, TServiceInterfaceType>(name, description, HostBindingType.BasicHttp, port, 0,
                serviceAddressBase);
        }

        public static ServiceEndpointDescription StartHttpHost<TServiceType, TServiceInterfaceType>(string name,
            string description, int port, string serviceAddressBase)
        {
            return StartHost<TServiceType, TServiceInterfaceType>(name, description, HostBindingType.WSHttp, port, 0,
                serviceAddressBase);
        }

        public static ServiceEndpointDescription StartHttpDualHost<TServiceType, TServiceInterfaceType>(string name,
            string description, int port, string serviceAddressBase)
        {
            return StartHost<TServiceType, TServiceInterfaceType>(name, description, HostBindingType.WSDualHttp, port, 0,
                serviceAddressBase);
        }

        public static ServiceEndpointDescription StartNetTcpHost<TServiceType, TServiceInterfaceType>(string name,
            string description, int port, int metaDataHttpPort, string serviceAddressBase)
        {
            return StartHost<TServiceType, TServiceInterfaceType>(name, description, HostBindingType.NetTcp,
                metaDataHttpPort, port, serviceAddressBase);
        }

        public static ServiceEndpointDescription StartNetPipeHost<TServiceType, TServiceInterfaceType>(string name,
            string description, int metaDataHttpPort, string serviceAddressBase)
        {
            return StartHost<TServiceType, TServiceInterfaceType>(name, description, HostBindingType.NamedPipes,
                metaDataHttpPort, 0, serviceAddressBase);
        }

        private static ServiceEndpointDescription StartHost<TServiceType, TServiceInterfaceType>
            (
            string name,
            string description,
            HostBindingType bindingType,
            int httpPort,
            int tcpPort,
            string serviceAddressBase)
        {
            var sed = new ServiceEndpointDescription(name, description);

            sed.Binding = GetBinding<TServiceInterfaceType>(bindingType);
            sed.ServiceHost = new ServiceHost(typeof(TServiceType));
            var endpointAddress = GetEndpointAddress(name, bindingType, tcpPort, httpPort, serviceAddressBase);

            var metadataBehavior = sed.ServiceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (null == metadataBehavior)
            {
                if (bindingType == HostBindingType.BasicHttp ||
                    bindingType == HostBindingType.WSHttp ||
                    bindingType == HostBindingType.WSDualHttp)
                {
                    metadataBehavior = new ServiceMetadataBehavior();
                    metadataBehavior.HttpGetEnabled = true;
                    metadataBehavior.HttpGetUrl = endpointAddress;
                    sed.ServiceHost.Description.Behaviors.Add(metadataBehavior);
                }
            }

            var debugBehavior = sed.ServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (null == debugBehavior)
            {
                debugBehavior = new ServiceDebugBehavior
                {
                    IncludeExceptionDetailInFaults = true
                };

                sed.ServiceHost.Description.Behaviors.Add(debugBehavior);
            }

            sed.ServiceHost.AddServiceEndpoint(typeof(TServiceInterfaceType), sed.Binding, endpointAddress);
            sed.ServiceHost.Open();

            return sed;
        }

        private static Binding GetBinding<TServiceInterfaceType>(HostBindingType bindingType)
        {
            var contractObjects = typeof(TServiceInterfaceType).GetCustomAttributes(typeof(ServiceContractAttribute),
                false);
            string serviceConfigurationName = null;
            if (contractObjects.Length > 0)
                serviceConfigurationName = ((ServiceContractAttribute) contractObjects[0]).ConfigurationName;

            if (string.IsNullOrEmpty(serviceConfigurationName))
                serviceConfigurationName = typeof(TServiceInterfaceType).Name;

            Binding binding;

            if (bindingType == HostBindingType.NetTcp)
            {
                var configurationName = string.Format("{0}_{1}", typeof(NetTcpBinding).Name, serviceConfigurationName);
                try
                {
                    binding = new NetTcpBinding(configurationName);
                }
                catch
                {
                    Platform.Log(LogLevel.Info,
                        string.Format("unable to load binding configuration {0}; using default binding configuration",
                            configurationName));
                    binding = new NetTcpBinding();
                }

                ((NetTcpBinding) binding).PortSharingEnabled = true;
            }
            else if (bindingType == HostBindingType.NamedPipes)
            {
                var configurationName = string.Format("{0}_{1}", typeof(NetNamedPipeBinding).Name,
                    serviceConfigurationName);
                try
                {
                    binding = new NetNamedPipeBinding(configurationName);
                }
                catch
                {
                    Platform.Log(LogLevel.Info,
                        "unable to load binding configuration {0}; using default binding configuration",
                        configurationName);
                    binding = new NetNamedPipeBinding();
                }
            }
            else if (bindingType == HostBindingType.WSDualHttp)
            {
                var configurationName = string.Format("{0}_{1}", typeof(WSDualHttpBinding).Name,
                    serviceConfigurationName);
                try
                {
                    binding = new WSDualHttpBinding(configurationName);
                }
                catch
                {
                    Platform.Log(LogLevel.Info,
                        "unable to load binding configuration {0}; using default binding configuration",
                        configurationName);
                    binding = new WSDualHttpBinding();
                }
            }
            else if (bindingType == HostBindingType.WSHttp)
            {
                var configurationName = string.Format("{0}_{1}", typeof(WSHttpBinding).Name, serviceConfigurationName);
                try
                {
                    binding = new WSHttpBinding(configurationName);
                }
                catch
                {
                    Platform.Log(LogLevel.Info,
                        "unable to load binding configuration {0}; using default binding configuration",
                        configurationName);
                    binding = new WSHttpBinding();
                }
            }
            else
            {
                var configurationName = string.Format("{0}_{1}", typeof(BasicHttpBinding).Name, serviceConfigurationName);
                try
                {
                    binding = new BasicHttpBinding(configurationName);
                }
                catch
                {
                    Platform.Log(LogLevel.Info,
                        "unable to load binding configuration {0}; using default binding configuration",
                        configurationName);
                    binding = new BasicHttpBinding();
                }
            }

            return binding;
        }

        public static void StopHost(ServiceEndpointDescription sed)
        {
            sed.ServiceHost.Close();
        }

        private static Uri GetEndpointAddress(string endpointName, HostBindingType bindingType, int tcpPort,
            int httpPort, string serviceAddressBase)
        {
            var serviceBase = string.IsNullOrEmpty(serviceAddressBase) ? endpointName : serviceAddressBase;
            var serviceAdress = string.Format("{0}/{1}", serviceBase, endpointName);

            if (bindingType == HostBindingType.NetTcp)
                return new UriBuilder(string.Format("net.tcp://localhost:{0}/{1}", tcpPort, serviceAdress)).Uri;
            if (bindingType == HostBindingType.NamedPipes)
                return new UriBuilder(string.Format("net.pipe://localhost/{0}", serviceAdress)).Uri;

            return new UriBuilder(string.Format("http://localhost:{0}/{1}", httpPort, serviceAdress)).Uri;
        }
    }
}
