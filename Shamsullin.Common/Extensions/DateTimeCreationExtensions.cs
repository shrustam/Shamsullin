using System;

namespace Shamsullin.Common.Extensions
{
	public static class DateTimeCreationExtensions
	{
		private const int JanuaryNumber = 1;
		private const int FebruaryNumber = 2;
		private const int MarchNumber = 3;
		private const int AprilNumber = 4;
		private const int MayNumber = 5;
		private const int JuneNumber = 6;
		private const int JulyNumber = 7;
		private const int AugustNumber = 8;
		private const int SeptemberNumber = 9;
		private const int OctoberNumber = 10;
		private const int NovemberNumber = 11;
		private const int DecemberNumber = 12;

		public static DateTime January(this int day, int year)
		{
			return new DateTime(year, JanuaryNumber, day);
		}

		public static DateTime February(this int day, int year)
		{
			return new DateTime(year, FebruaryNumber, day);
		}

		public static DateTime March(this int day, int year)
		{
			return new DateTime(year, MarchNumber, day);
		}

		public static DateTime April(this int day, int year)
		{
			return new DateTime(year, AprilNumber, day);
		}

		public static DateTime May(this int day, int year)
		{
			return new DateTime(year, MayNumber, day);
		}

		public static DateTime June(this int day, int year)
		{
			return new DateTime(year, JuneNumber, day);
		}

		public static DateTime July(this int day, int year)
		{
			return new DateTime(year, JulyNumber, day);
		}

		public static DateTime August(this int day, int year)
		{
			return new DateTime(year, AugustNumber, day);
		}

		public static DateTime September(this int day, int year)
		{
			return new DateTime(year, SeptemberNumber, day);
		}

		public static DateTime October(this int day, int year)
		{
			return new DateTime(year, OctoberNumber, day);
		}

		public static DateTime November(this int day, int year)
		{
			return new DateTime(year, NovemberNumber, day);
		}

		public static DateTime December(this int day, int year)
		{
			return new DateTime(year, DecemberNumber, day);
		}
	}
}
