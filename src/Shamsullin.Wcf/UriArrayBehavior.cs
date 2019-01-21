using System;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace Monitoring.Agent
{
	public class UriArrayBehavior : WebHttpBehavior
	{
		private WebMessageFormat _defaultOutgoingResponseFormat;

		public UriArrayBehavior()
		{
			_defaultOutgoingResponseFormat = WebMessageFormat.Json;
		}

		public override WebMessageFormat DefaultOutgoingResponseFormat
		{
			get { return _defaultOutgoingResponseFormat; }
			set { _defaultOutgoingResponseFormat = value; }
		}

		protected override QueryStringConverter GetQueryStringConverter(OperationDescription operationDescription)
		{
			return new UriArrayConverter();
		}
	}

    public class UriArrayConverter : QueryStringConverter
    {
        public override bool CanConvert(Type type)
        {
            return base.CanConvert(type.IsArray ? type.GetElementType() : type);
        }

        public override object ConvertStringToValue(string parameter, Type parameterType)
        {
            if (!parameterType.IsArray) return base.ConvertStringToValue(parameter, parameterType);
            var elementType = parameterType.GetElementType();
            var parameterList = parameter.Split(',');
            var result = Array.CreateInstance(elementType, parameterList.Length);
            for (var i = 0; i < parameterList.Length; i++)
            {
                result.SetValue(base.ConvertStringToValue(parameterList[i], elementType), i);
            }

            return result;
        }
    }

    public class UriArrayBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(UriArrayBehavior);

        protected override object CreateBehavior()
        {
            return new UriArrayBehavior();
        }
    }
}