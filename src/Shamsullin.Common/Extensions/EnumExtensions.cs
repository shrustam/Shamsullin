using System;
using System.ComponentModel;
using System.Reflection;

namespace Shamsullin.Common.Extensions
{
	public static class EnumExtensions
	{
		public static string Description(this Enum en)
		{
			Type type = en.GetType();

			MemberInfo[] memberInfos = type.GetMember(en.ToString());

			if(memberInfos.Length > 0)
			{
				object[] attrs = memberInfos[0].GetCustomAttributes(typeof (DescriptionAttribute), false);

				if(attrs.Length > 0)
					return ((DescriptionAttribute) attrs[0]).Description;
			}

			return en.ToString();
		}

        public static int ToInt(this Enum en)
        {
            return Convert.ToInt32(en);
        }
	}
}
