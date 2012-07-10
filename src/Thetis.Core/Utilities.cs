using System;
using System.Text;

namespace Thetis.Core
{
	/// <summary>
	/// A static utilities class for Thetis.Core
	/// </summary>
	static public class Utilities
	{
		
		/// <summary>
		/// Formats a timespan in a human readable fashion.
		/// </summary>
		/// <returns>
		/// A formatted string.
		/// </returns>
		/// <param name='ts'>
		/// The timespan to be formatted.
		/// </param>
		public static String FormatTimespan(TimeSpan ts)
		{
			
            StringBuilder sb = new StringBuilder();
			int years = (ts.Days - ts.Days % 365) / 365;
			if (years > 0) sb.Append(String.Format("{0} year", years));
			if (years > 1) sb.Append(String.Format("s"));
            if (ts.Days > 0) 
			{
				if (sb.Length > 0) sb.Append(", ");
				sb.Append(String.Format("{0} day", ts.Days % 365));
				if (ts.Days > 1) sb.Append("s");
			}
            if (ts.Hours > 0)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(String.Format("{0} hour", ts.Hours));
				if (ts.Hours > 1) sb.Append("s");

            }
            if (ts.Minutes > 0)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(String.Format("{0} minute", ts.Minutes));
				if (ts.Minutes > 1) sb.Append("s");

            }
            if (ts.Seconds > 0)
            {
                if (sb.Length > 0) sb.Append(" and ");
                sb.Append(String.Format("{0} second", ts.Seconds));
				if (ts.Seconds > 1) sb.Append("s");

            }
            
            return sb.ToString();
		}
		
	}
}

