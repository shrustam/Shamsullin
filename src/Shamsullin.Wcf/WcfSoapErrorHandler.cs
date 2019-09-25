using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Shamsullin.Wcf
{
    /// <summary>
    /// Exceptions handler for SOAP.
    /// </summary>
    public class WcfSoapErrorHandler : IErrorHandler
    {
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            var faultException = error as FaultException;
            if (faultException == null)
            {
                faultException = new FaultException<ErrorModel>(new ErrorModel
                {
                    Message = error.Message,
                    Success = false,
                    Type = error.GetType().Name
                }, "Bad Request");
            }

            fault = Message.CreateMessage(version, faultException.CreateMessageFault(), faultException.Action);
            fault.Properties[HttpResponseMessageProperty.Name] = new HttpResponseMessageProperty
            {
                StatusCode = HttpStatusCode.OK
            };
        }

        public bool HandleError(Exception error)
        {
            Trace.WriteLine($"{error.TargetSite.Name}: {error}");
            return false;
        }
    }

    /// <summary>
    /// Exception handler for SOAP service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class WcfSoapErrorHandlerAttribute : Attribute, IServiceBehavior
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
            IErrorHandler errorHandler = new WcfSoapErrorHandler();
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                channelDispatcher.ErrorHandlers.Add(errorHandler);
            }
        }
    }
}