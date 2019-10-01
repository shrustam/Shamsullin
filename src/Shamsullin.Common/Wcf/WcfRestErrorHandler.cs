using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using log4net;

namespace Shamsullin.Common.Wcf
{
    /// <summary>
    /// Exceptions handler for REST/JSON.
    /// </summary>
    public class WcfRestErrorHandler : IErrorHandler
    {
        public ILog Log = LogManager.GetLogger(typeof(WcfRestErrorHandler));

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            var errorModel = new ErrorModel { Message = error.Message, Type = error.GetType().Name, Success = false };
            fault = Message.CreateMessage(version, null, errorModel, new DataContractJsonSerializer(typeof(ErrorModel)));
            fault.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Json));

            var rmp = new HttpResponseMessageProperty();
            rmp.Headers[HttpResponseHeader.ContentType] = "application/json";
            fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
        }

        public bool HandleError(Exception error)
        {
            Log?.Warn(error);
            return false;
        }
    }

    /// <summary>
    /// Attribute for for REST/JSON exceptions handling.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class WcfRestErrorHandlerAttribute : Attribute, IServiceBehavior
    {
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            IErrorHandler errorHandler = new WcfRestErrorHandler();
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                channelDispatcher.ErrorHandlers.Add(errorHandler);
            }
        }
    }
}