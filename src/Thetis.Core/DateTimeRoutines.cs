//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        27 February 2007
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using System.Web;

namespace Thetis.Core
{
    /// <summary>
    /// Miscellaneous and parsing methods for DateTime
    /// </summary>
    public static class DateTimeRoutines
    {
        #region miscellaneous methods

        /// <summary>
        /// Amount of seconds elapsed between 1970-01-01 00:00:00 and the date-time.
        /// </summary>
        /// <param name="date_time">date-time</param>
        /// <returns>seconds</returns>
        public static uint GetSecondsSinceUnixEpoch(this DateTime date_time)
        {
            TimeSpan t = date_time - new DateTime(1970, 1, 1);
            int ss = (int)t.TotalSeconds;
            if (ss < 0)
                return 0;
            return (uint)ss;
        }

        #endregion

        #region parsing definitions

        /// <summary>
        /// Defines a substring where date-time was found and result of conversion
        /// </summary>
        public class ParsedDateTime
        {
            /// <summary>
            /// Index of first char of a date substring found in the string
            /// </summary>
            readonly public int IndexOfDate = -1;
            /// <summary>
            /// Length a date substring found in the string
            /// </summary>
            readonly public int LengthOfDate = -1;
            /// <summary>
            /// Index of first char of a time substring found in the string
            /// </summary>
            readonly public int IndexOfTime = -1;
            /// <summary>
            /// Length of a time substring found in the string
            /// </summary>
            readonly public int LengthOfTime = -1;
            /// <summary>
            /// DateTime found in the string
            /// </summary>
            readonly public DateTime DateTime;
            /// <summary>
            /// True if a date was found within the string
            /// </summary>
            readonly public bool IsDateFound;
            /// <summary>
            /// True if a time was found within the string
            /// </summary>
            readonly public bool IsTimeFound;

            internal ParsedDateTime(int index_of_date, int length_of_date, int index_of_time, int length_of_time, DateTime date_time)
            {
                IndexOfDate = index_of_date;
                LengthOfDate = length_of_date;
                IndexOfTime = index_of_time;
                LengthOfTime = length_of_time;
                DateTime = date_time;
                IsDateFound = index_of_date > -1;
                IsTimeFound = index_of_time > -1;
            }
        }

        /// <summary>
        /// Date that is accepted in the following cases:
        /// - no date was parsed by TryParseDateOrTime();
        /// - no year was found by TryParseDate();
        /// It is ignored if DefaultDateIsNow = true was set after DefaultDate 
        /// </summary>
        public static DateTime DefaultDate
        {
            set
            {
                _DefaultDate = value;
                DefaultDateIsNow = false;
            }
            get
            {
                if (DefaultDateIsNow)
                    return DateTime.Now;
                else
                    return _DefaultDate;
            }
        }
        static DateTime _DefaultDate = DateTime.Now;

        /// <summary>
        /// If true then DefaultDate property is ignored and DefaultDate is always DateTime.Now
        /// </summary>
        public static bool DefaultDateIsNow = true;

        /// <summary>
        /// Defines default date-time format.
        /// </summary>
        public enum DateTimeFormat
        {
            /// <summary>
            /// month number goes before day number
            /// </summary>
            USA_DATE,
            /// <summary>
            /// day number goes before month number
            /// </summary>
            UK_DATE,
            ///// <summary>
            ///// time is specifed through AM or PM
            ///// </summary>
            //USA_TIME,
        }

        #endregion

        #region parsing derived methods for DateTime output

        /// <summary>
        /// Tries to find date and time within the passed string and return it as DateTime structure. 
        /// </summary>
        /// <param name="str">string that contains date and/or time</param>
        /// <param name="default_format">format to be used preferably in ambivalent instances</param>
        /// <param name="date_time">parsed date-time output</param>
        /// <returns>true if both date and time were found, else false</returns>
        static public bool TryParseDateTime(this string str, DateTimeFormat default_format, out DateTime date_time)
        {
            ParsedDateTime parsed_date_time;
            if (!TryParseDateTime(str, default_format, out parsed_date_time))
            {
                date_time = new DateTime(1, 1, 1);
                return false;
            }
            date_time = parsed_date_time.DateTime;
            return true;
        }

        /// <summary>
        /// Tries to find date and/or time within the passed string and return it as DateTime structure. 
        /// If only date was found, time in the returned DateTime is always 0:0:0.
        /// If only time was found, date in the returned DateTime is DefaultDate.
        /// </summary>
        /// <param name="str">string that contains date and(or) time</param>
        /// <param name="default_format">format to be used preferably in ambivalent instances</param>
        /// <param name="date_time">parsed date-time output</param>
        /// <returns>true if date and/or time was found, else false</returns>
        static public bool TryParseDateOrTime(this string str, DateTimeFormat default_format, out DateTime date_time)
        {
            ParsedDateTime parsed_date_time;
            if (!TryParseDateOrTime(str, default_format, out parsed_date_time))
            {
                date_time = new DateTime(1, 1, 1);
                return false;
            }
            date_time = parsed_date_time.DateTime;
            return true;
        }

        /// <summary>
        /// Tries to find time within the passed string and return it as DateTime structure. 
        /// It recognizes only time while ignoring date, so date in the returned DateTime is always 1/1/1.
        /// </summary>
        /// <param name="str">string that contains time</param>
        /// <param name="default_format">format to be used preferably in ambivalent instances</param>
        /// <param name="time">parsed time output</param>
        /// <returns>true if time was found, else false</returns>
        public static bool TryParseTime(this string str, DateTimeFormat default_format, out DateTime time)
        {
            ParsedDateTime parsed_time;
            if (!TryParseTime(str, default_format, out parsed_time, null))
            {
                time = new DateTime(1, 1, 1);
                return false;
            }
            time = parsed_time.DateTime;
            return true;
        }

        /// <summary>
        /// Tries to find date within the passed string and return it as DateTime structure. 
        /// It recognizes only date while ignoring time, so time in the returned DateTime is always 0:0:0.
        /// If year of the date was not found then it accepts the current year. 
        /// </summary>
        /// <param name="str">string that contains date</param>
        /// <param name="default_format">format to be used preferably in ambivalent instances</param>
        /// <param name="date">parsed date output</param>
        /// <returns>true if date was found, else false</returns>
        static public bool TryParseDate(this string str, DateTimeFormat default_format, out DateTime date)
        {
            ParsedDateTime parsed_date;
            if (!TryParseDate(str, default_format, out parsed_date))
            {
                date = new DateTime(1, 1, 1);
                return false;
            }
            date = parsed_date.DateTime;
            return true;
        }

        #endregion

        #region parsing derived methods for ParsedDateTime output

        /// <summary>
        /// Tries to find date and time within the passed string and return it as ParsedDateTime object. 
        /// </summary>
        /// <param name="str">string that contains date-time</param>
        /// <param name="default_format">format to be used preferably in ambivalent instances</param>
        /// <param name="parsed_date_time">parsed date-time output</param>
        /// <returns>true if both date and time were found, else false</returns>
        static public bool TryParseDateTime(this string str, DateTimeFormat default_format, out ParsedDateTime parsed_date_time)
        {
            if (DateTimeRoutines.TryParseDateOrTime(str, default_format, out parsed_date_time)
                && parsed_date_time.IsDateFound
                && parsed_date_time.IsTimeFound
                )
                return true;

            parsed_date_time = null;
            return false;
        }

        /// <summary>
        /// Tries to find time within the passed string and return it as ParsedDateTime object. 
        /// It recognizes only time while ignoring date, so date in the returned ParsedDateTime is always 1/1/1
        /// </summary>
        /// <param name="str">string that contains date-time</param>
        /// <param name="default_format">format to be used preferably in ambivalent instances</param>
        /// <param name="parsed_time">parsed date-time output</param>
        /// <returns>true if time was found, else false</returns>
        static public bool TryParseTime(this string str, DateTimeFormat default_format, out ParsedDateTime parsed_time)
        {
            return TryParseTime(str, default_format, out parsed_time, null);
        }

        /// <summary>
        /// Tries to find date and/or time within the passed string and return it as ParsedDateTime object. 
        /// If only date was found, time in the returned ParsedDateTime is always 0:0:0.
        /// If only time was found, date in the returned ParsedDateTime is DefaultDate.
        /// </summary>
        /// <param name="str">string that contains date-time</param>
        /// <param name="default_format">format to be used preferably in ambivalent instances</param>
        /// <param name="parsed_date_time">parsed date-time output</param>
        /// <returns>true if date or time was found, else false</returns>
        static public bool TryParseDateOrTime(this string str, DateTimeFormat default_format, out ParsedDateTime parsed_date_time)
        {
            parsed_date_time = null;

            ParsedDateTime parsed_date;
            ParsedDateTime parsed_time;
            if (!TryParseDate(str, default_format, out parsed_date))
            {
                if (!TryParseTime(str, default_format, out parsed_time, null))
                    return false;

                DateTime date_time = new DateTime(DefaultDate.Year, DefaultDate.Month, DefaultDate.Day, parsed_time.DateTime.Hour, parsed_time.DateTime.Minute, parsed_time.DateTime.Second);
                parsed_date_time = new ParsedDateTime(-1, -1, parsed_time.IndexOfTime, parsed_time.LengthOfTime, date_time);
            }
            else
            {
                if (!TryParseTime(str, default_format, out parsed_time, parsed_date))
                {
                    DateTime date_time = new DateTime(parsed_date.DateTime.Year, parsed_date.DateTime.Month, parsed_date.DateTime.Day, 0, 0, 0);
                    parsed_date_time = new ParsedDateTime(parsed_date.IndexOfDate, parsed_date.LengthOfDate, -1, -1, date_time);
                }
                else
                {
                    DateTime date_time = new DateTime(parsed_date.DateTime.Year, parsed_date.DateTime.Month, parsed_date.DateTime.Day, parsed_time.DateTime.Hour, parsed_time.DateTime.Minute, parsed_time.DateTime.Second);
                    parsed_date_time = new ParsedDateTime(parsed_date.IndexOfDate, parsed_date.LengthOfDate, parsed_time.IndexOfTime, parsed_time.LengthOfTime, date_time);
                }
            }

            return true;
        }

        #endregion

        #region parsing base methods

        /// <summary>
        /// Tries to find time within the passed string (relatively to the passed parsed_date if any) and return it as ParsedDateTime object.
        /// It recognizes only time while ignoring date, so date in the returned ParsedDateTime is always 1/1/1
        /// </summary>
        /// <param name="str">string that contains date</param>
        /// <param name="default_format">format to be used preferably in ambivalent instances</param>
        /// <param name="parsed_time">parsed date-time output</param>
        /// <param name="parsed_date">ParsedDateTime object if the date was found within this string, else NULL</param>
        /// <returns>true if time was found, else false</returns>
        public static bool TryParseTime(this string str, DateTimeFormat default_format, out ParsedDateTime parsed_time, ParsedDateTime parsed_date)
        {
            parsed_time = null;

            Match m;
            if (parsed_date != null && parsed_date.IndexOfDate > -1)
            {//look around the found date
                //look for <date> [h]h:mm[:ss] [PM/AM]
                m = Regex.Match(str.Substring(parsed_date.IndexOfDate + parsed_date.LengthOfDate), @"(?<=^\s*,?\s+|^\s*at\s*|^\s*[T\-]\s*)(?'hour'\d{1,2})\s*:\s*(?'minute'\d{2})\s*(?::\s*(?'second'\d{2}))?(?:\s*(?'ampm'AM|am|PM|pm))?(?=$|[^\d\w])", RegexOptions.Compiled);
                if (!m.Success)
                    //look for [h]h:mm:ss <date>
                    m = Regex.Match(str.Substring(0, parsed_date.IndexOfDate), @"(?<=^|[^\d])(?'hour'\d{1,2})\s*:\s*(?'minute'\d{2})\s*(?::\s*(?'second'\d{2}))?(?:\s*(?'ampm'AM|am|PM|pm))?(?=$|[\s,]+)", RegexOptions.Compiled);
            }
            else//look anywere within string
                //look for [h]h:mm[:ss] [PM/AM]
                m = Regex.Match(str, @"(?<=^|\s+|\s*T\s*)(?'hour'\d{1,2})\s*:\s*(?'minute'\d{2})\s*(?::\s*(?'second'\d{2}))?(?:\s*(?'ampm'AM|am|PM|pm))?(?=$|[^\d\w])", RegexOptions.Compiled);

            if (m.Success)
            {
                try
                {
                    int hour = int.Parse(m.Groups["hour"].Value);
                    if (hour < 0 || hour > 23)
                        return false;

                    int minute = int.Parse(m.Groups["minute"].Value);
                    if (minute < 0 || minute > 59)
                        return false;

                    int second = 0;
                    if (!string.IsNullOrEmpty(m.Groups["second"].Value))
                    {
                        second = int.Parse(m.Groups["second"].Value);
                        if (second < 0 || second > 59)
                            return false;
                    }

                    if (string.Compare(m.Groups["ampm"].Value, "PM", true) == 0 && hour < 12)
                        hour += 12;
                    else if (string.Compare(m.Groups["ampm"].Value, "AM", true) == 0 && hour == 12)
                        hour -= 12;

                    DateTime date_time = new DateTime(1, 1, 1, hour, minute, second);
                    parsed_time = new ParsedDateTime(-1, -1, m.Index, m.Length, date_time);
                }
                catch
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to find date within the passed string and return it as ParsedDateTime object. 
        /// It recognizes only date while ignoring time, so time in the returned ParsedDateTime is always 0:0:0.
        /// If year of the date was not found then it accepts the current year. 
        /// </summary>
        /// <param name="str">string that contains date</param>
        /// <param name="default_format">format to be used preferably in ambivalent instances</param>
        /// <param name="parsed_date">parsed date output</param>
        /// <returns>true if date was found, else false</returns>
        static public bool TryParseDate(this string str, DateTimeFormat default_format, out ParsedDateTime parsed_date)
        {
            parsed_date = null;

            if (string.IsNullOrEmpty(str))
                return false;

            //look for dd/mm/yy
            Match m = Regex.Match(str, @"(?<=^|[^\d])(?'day'\d{1,2})\s*(?'separator'[\\/\.])+\s*(?'month'\d{1,2})\s*\'separator'+\s*(?'year'\d{2}|\d{4})(?=$|[^\d])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (m.Success)
            {
                DateTime date;
                if ((default_format ^ DateTimeFormat.USA_DATE) == DateTimeFormat.USA_DATE)
                {
                    if (!convert_to_date(int.Parse(m.Groups["year"].Value), int.Parse(m.Groups["day"].Value), int.Parse(m.Groups["month"].Value), out date))
                        return false;
                }
                else
                {
                    if (!convert_to_date(int.Parse(m.Groups["year"].Value), int.Parse(m.Groups["month"].Value), int.Parse(m.Groups["day"].Value), out date))
                        return false;
                }
                parsed_date = new ParsedDateTime(m.Index, m.Length, -1, -1, date);
                return true;
            }

            //look for [yy]yy-mm-dd
            m = Regex.Match(str, @"(?<=^|[^\d])(?'year'\d{2}|\d{4})\s*(?'separator'[\-])\s*(?'month'\d{1,2})\s*\'separator'+\s*(?'day'\d{1,2})(?=$|[^\d])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (m.Success)
            {
                DateTime date;
                if (!convert_to_date(int.Parse(m.Groups["year"].Value), int.Parse(m.Groups["month"].Value), int.Parse(m.Groups["day"].Value), out date))
                    return false;
                parsed_date = new ParsedDateTime(m.Index, m.Length, -1, -1, date);
                return true;
            }

            //look for month dd yyyy
            m = Regex.Match(str, @"(?:^|[^\d\w])(?'month'Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[uarychilestmbro]*\s+(?'day'\d{1,2})(?:-?st|-?th|-?rd|-?nd)?\s*,?\s*(?'year'\d{4})(?=$|[^\d\w])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (!m.Success)
                //look for dd month [yy]yy
                m = Regex.Match(str, @"(?:^|[^\d\w:])(?'day'\d{1,2})(?:-?st\s+|-?th\s+|-?rd\s+|-?nd\s+|-|\s+)(?'month'Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[uarychilestmbro]*(?:\s*,?\s*|-)'?(?'year'\d{2}|\d{4})(?=$|[^\d\w])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (!m.Success)
                //look for yyyy month dd
                m = Regex.Match(str, @"(?:^|[^\d\w])(?'year'\d{4})\s+(?'month'Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[uarychilestmbro]*\s+(?'day'\d{1,2})(?:-?st|-?th|-?rd|-?nd)?(?=$|[^\d\w])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (!m.Success)
                //look for  month dd [yyyy]
                m = Regex.Match(str, @"(?:^|[^\d\w])(?'month'Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[uarychilestmbro]*\s+(?'day'\d{1,2})(?:-?st|-?th|-?rd|-?nd)?(?:\s*,?\s*(?'year'\d{4}))?(?=$|[^\d\w])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (m.Success)
            {
                int month = -1;
                int index_of_date = m.Index;
                int length_of_date = m.Length;

                switch (m.Groups["month"].Value)
                {
                    case "Jan":
                    case "JAN":
                        month = 1;
                        break;
                    case "Feb":
                    case "FEB":
                        month = 2;
                        break;
                    case "Mar":
                    case "MAR":
                        month = 3;
                        break;
                    case "Apr":
                    case "APR":
                        month = 4;
                        break;
                    case "May":
                    case "MAY":
                        month = 5;
                        break;
                    case "Jun":
                    case "JUN":
                        month = 6;
                        break;
                    case "Jul":
                        month = 7;
                        break;
                    case "Aug":
                    case "AUG":
                        month = 8;
                        break;
                    case "Sep":
                    case "SEP":
                        month = 9;
                        break;
                    case "Oct":
                    case "OCT":
                        month = 10;
                        break;
                    case "Nov":
                    case "NOV":
                        month = 11;
                        break;
                    case "Dec":
                    case "DEC":
                        month = 12;
                        break;
                }

                int year;
                if (!string.IsNullOrEmpty(m.Groups["year"].Value))
                    year = int.Parse(m.Groups["year"].Value);
                else
                    year = DefaultDate.Year;

                DateTime date;
                if (!convert_to_date(year, month, int.Parse(m.Groups["day"].Value), out date))
                    return false;
                parsed_date = new ParsedDateTime(index_of_date, length_of_date, -1, -1, date);
                return true;
            }

            return false;
        }

        static bool convert_to_date(int year, int month, int day, out DateTime date)
        {
            if (year >= 100)
            {
                if (year < 1000)
                {
                    date = new DateTime(1, 1, 1);
                    return false;
                }
            }
            else
                if (year > 30)
                    year += 1900;
                else
                    year += 2000;

            try
            {
                date = new DateTime(year, month, day);
            }
            catch
            {
                date = new DateTime(1, 1, 1);
                return false;
            }
            return true;
        }

        #endregion
    }
}