using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Shamsullin.Wcf
{
    /// <summary>
    /// JSON serializer.
    /// </summary>
    public class NewtonsoftJsonDispatchFormatter : IDispatchMessageFormatter
    {
        private readonly OperationDescription _operation;

        public NewtonsoftJsonDispatchFormatter(OperationDescription operation)
        {
            _operation = operation;
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            using (var mss = new MemoryStream())
            {
                using (var messageWriter = JsonReaderWriterFactory.CreateJsonWriter(mss))
                {
                    message.WriteMessage(messageWriter);
                    messageWriter.Flush();
                    parameters[0] = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(mss.ToArray()),
                        _operation.Messages[0].Body.Parts[0].Type);
                }
            }
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

            var replyMessage = Message.CreateMessage(
                messageVersion, _operation.Messages[1].Action, new RawBodyWriter(body));
            replyMessage.Properties.Add(
                WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Raw));
            var respProp = new HttpResponseMessageProperty();
            respProp.Headers[HttpResponseHeader.ContentType] = "application/json";
            replyMessage.Properties.Add(HttpResponseMessageProperty.Name, respProp);
            return replyMessage;
        }
    }

    /// <summary>
    /// Writes JSON responses
    /// </summary>
    public class RawBodyWriter : BodyWriter
    {
        private readonly byte[] _content;

        public RawBodyWriter(byte[] content)
            : base(true)
        {
            _content = content;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("Binary");
            writer.WriteBase64(_content, 0, _content.Length);
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// REST behavior.
    /// </summary>
    public class WcfRestBehavior : WebHttpBehavior
    {
        private bool IsGetOperation(OperationDescription operation)
        {
            var wga = operation.Behaviors.Find<WebGetAttribute>();
            if (wga != null) return true;
            var wia = operation.Behaviors.Find<WebInvokeAttribute>();
            if (wia != null) return wia.Method == "GET";
            return false;
        }

        protected override IDispatchMessageFormatter GetRequestDispatchFormatter(
            OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            if (IsGetOperation(operationDescription))
            {
                return base.GetRequestDispatchFormatter(operationDescription, endpoint);
            }
            return new NewtonsoftJsonDispatchFormatter(operationDescription);
        }

        protected override IDispatchMessageFormatter GetReplyDispatchFormatter(
            OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            if (operationDescription.Messages.Count == 1 ||
                operationDescription.Messages[1].Body.ReturnValue.Type == typeof (void))
            {
                return base.GetReplyDispatchFormatter(operationDescription, endpoint);
            }
            return new NewtonsoftJsonDispatchFormatter(operationDescription);
        }
    }

    /// <summary>
    /// Extension for behavior can be used in web.config.
    /// </summary>
    public class WcfRestBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof (WcfRestBehavior);

        protected override object CreateBehavior()
        {
            return new WcfRestBehavior();
        }
    }
}