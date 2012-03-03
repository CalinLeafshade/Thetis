using System;
using System.Text;

namespace Thetis.Core
{
	static public class Utilities
	{
		
		public static String FormatTimespan(TimeSpan ts)
		{
			
            StringBuilder sb = new StringBuilder();
			
            if (ts.Days > 0) sb.Append(String.Format("{0} day", ts.Days));
			if (ts.Days > 1) sb.Append("s");
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

