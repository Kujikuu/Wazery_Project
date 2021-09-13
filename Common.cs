using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LightConquer_Project
{
	public static class Common
	{
		static Common()
		{
			_clock = Stopwatch.StartNew();
			Random = new ThreadSafeRandom();
		}
		public const int MS_PER_SECOND = 1000;
		public const int MS_PER_MINUTE = 60000;
		public const int MS_PER_HOUR = 3600000;
		public static ThreadSafeRandom Random;
		private static readonly Stopwatch _clock;
		public static bool PercentSuccess(double _chance)
		{
			return Random.NextDouble() * 100 < _chance;
		}
		public static bool PercentSuccess(int _chance)
		{
			return Random.NextDouble() * 100 < _chance;
		}
		public static long Clock
		{
			get
			{
				lock (_clock)
					return _clock.ElapsedMilliseconds;
			}
		}
		public static uint SecondsServerOnline
		{
			get { return (uint)(_clock.ElapsedMilliseconds / MS_PER_SECOND); }
		}
		public static uint MinutesServerOnline
		{
			get { return (uint)(_clock.ElapsedMilliseconds / MS_PER_MINUTE); }
		}
		public static uint HoursServerOnline
		{
			get { return (uint)(_clock.ElapsedMilliseconds / MS_PER_HOUR); }
		}
	}
}