using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Authentication;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Shamsullin.Common;

namespace Shamsullin.Wcf
{
    /// <summary>
    /// Exceptions handler for REST/JSON.
    /// </summary>
    public class WcfRestErrorHandler : IErrorHandler
    {
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            var errorModel = new ErrorModel {Message = error.Message, Type = error.GetType().Name, Success = false};
            fault = Message.CreateMessage(version, null, errorModel, new DataContractJsonSerializer(typeof (ErrorModel)));
            fault.Properties.Add(WebBodyFormatMessageProperty.Name,
                new WebBodyFormatMessageProperty(WebContentFormat.Json));

            if (IsAuthenticationException(error))
            {
                var rmp = new HttpResponseMessageProperty {StatusCode = HttpStatusCode.Unauthorized};
                rmp.StatusDescription = rmp.StatusCode.ToString();
                rmp.Headers[HttpResponseHeader.ContentType] = "application/json";
                fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
            }
            else if (error.GetType() == typeof (InvalidOperationException) ||
                     error.GetType() == typeof (ArgumentException))
            {
                var rmp = new HttpResponseMessageProperty {StatusCode = HttpStatusCode.BadRequest};
                rmp.StatusDescription = rmp.StatusCode.ToString();
                rmp.Headers[HttpResponseHeader.ContentType] = "application/json";
                fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
            }
            else
            {
                errorModel.Message = "Unable to perform the operation";
                var rmp = new HttpResponseMessageProperty {StatusCode = HttpStatusCode.InternalServerError};
                rmp.StatusDescription = rmp.StatusCode.ToString();
                rmp.Headers[HttpResponseHeader.ContentType] = "application/json";
                fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
            }
        }

        public bool HandleError(Exception error)
        {
            Log.Instance.Info(error.TargetSite.Name, error);
            return false;
        }

        public bool IsAuthenticationException(Exception error)
        {
            var current = error;
            while (current != null)
            {
                if (current.GetType() == typeof (AuthenticationException)) return true;
                current = current.InnerException;
            }
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