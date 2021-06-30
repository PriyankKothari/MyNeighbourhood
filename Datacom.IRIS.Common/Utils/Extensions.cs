using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Datacom.IRIS.Common.Utils
{
    public static class Extensions
    {

        #region Traversing Extensions

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        public static int ForEachWithIndex<T>(this IEnumerable<T> list, Action<int, T> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            var index = 0;

            foreach (var elem in list)
                action(index++, elem);

            return index;
        }

        public static void ReverseForEach<T>(this IList<T> list, Action<int, T> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            for (int i = list.Count - 1; i >= 0; i--)
            {
                action(i, list[i]);
            }
        }

        public static void ReverseForEach<T>(this IList list, Action<int, T> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            for (int i = list.Count - 1; i >= 0; i--)
            {
                action(i, (T)list[i]);
            }
        }

        #endregion

        #region Collections/Lists Extensions

        public delegate void WhenKeyExistsAction(string id);

        public static void WhenKeyExists(this NameValueCollection collection, string key, WhenKeyExistsAction action)
        {
            string value = collection[key];
            if (value != null)
            {
                action.Invoke(value);
            }
        }

        public static string ListToString<T>(this IList<T> list)
        {
            string result = list.Count == 0 ? "(empty)" : "";

            for (int i = 0; i < list.Count; i++)
            {
                result += Environment.NewLine + list[i];
                if (i + 1 < list.Count) result += "\t\t--------";
            }

            return result;
        }

        public static void DeleteWhere<T>(this IList<T> list, Predicate<T> pred)
        {
            if (pred == null) throw new ArgumentNullException("pred");

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (pred(list[i]))
                    list.RemoveAt(i);
            }
        }

        public static TKey GetKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value)
        {
            return dict.SingleOrDefault(x => x.Value.Equals(value)).Key;
        }

        public static List<T> SortFluent<T>(this List<T> list, Comparison<T> comparison)
        {
            list.Sort(comparison);
            return list;
        }

        #endregion

        #region HTMLTextWriter Extensions

        public static void AddAttributeIf(this HtmlTextWriter writer, HtmlTextWriterAttribute attribute, string value, bool condition)
        {
            if (condition)
            {
                writer.AddAttribute(attribute, value);
            }
        }

        public static void AddAttributeIfNotEmpty(this HtmlTextWriter writer, HtmlTextWriterAttribute attribute, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                writer.AddAttribute(attribute, value);
            }
        }

        #endregion

        #region Date Extensions

        private static readonly string[] ParseDateFormats = { "d/M/yyyy", "d/M/yy" };
        private const string DateFormat = "dd/MM/yyyy";
        private const string DateTimeFormat = "dd/MM/yyyy hh:mm tt";
        private const string DateTimeFormat24 = "dd/MM/yyyy HH\\:mm";
        private const string PercentageFormat = "F";

        public static string ToDateString(this DateTime dateTime)
        {
            return ToDateString((DateTime?)dateTime);
        }

        public static string ToDateString(this DateTime? dateTime)
        {
            return dateTime != null && dateTime != DateTime.MinValue ? dateTime.Value.ToString(DateFormat) : string.Empty;
        }

        public static string ToDateString(this DateTime? date, string format = DateFormat)
        {
            return (date != null && date.HasValue) ? date.Value.ToString(format) : string.Empty;
        }

        public static string ToPercentageString(this Decimal? value)
        {
            return value.HasValue ? ToPercentageString(value.Value) : string.Empty;
        }    
        
        public static string ToPercentageString(this Double? value)
        {
            return value.HasValue ? ToPercentageString(value.Value) : string.Empty;
        }

        public static string ToPercentageString(this Decimal value)
        {
            return value.ToString(PercentageFormat);
        }   
        
        public static string ToPercentageString(this Double value)
        {
            return value.ToString(PercentageFormat);
        }

        /// <summary>
        ///   Converts a string to a DateTime object. If string is null or empty, method returns a null. This
        ///   is useful to unset the source value. The string will always be parsed as 'dd/MM/yyyy'. If it 
        ///   fails parsing we return a DateTime.min. This should allow a system to distinguish between no
        ///   datetime being typed in, and one that was typed in but failed parsing.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime? ToDateTime(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return ToDateTimeNonNullable(value);
        }

        public static DateTime ToDateTimeNonNullable(this string value)
        {
            try
            {
                return DateTime.ParseExact(value, ParseDateFormats, new CultureInfo("en-NZ"), DateTimeStyles.None);
            }
            catch { }

            return DateTime.MinValue;
        }

        public static string ToDateTimeString(this DateTime dateTime)
        {
            return ToDateTimeString((DateTime?)dateTime);
        }

        public static string ToDateTimeString(this DateTime? dateTime)
        {
            return dateTime != null ? dateTime.Value.ToString(DateTimeFormat) : string.Empty;
        }

        public static string ToDateTimeString24(this DateTime? dateTime)
        {
            return dateTime != null ? dateTime.Value.ToString(DateTimeFormat24) : string.Empty;
        }

        public static string ToDateTimeString24(this DateTime dateTime)
        {
            return ToDateTimeString24((DateTime?)dateTime);
        }

        // NB: Safer than using raw DateTime.IsDaylightSaving as local system may have an incorrect time zone.
        public static bool IsNzDayLightSavingTime(this DateTime dateTime)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time").IsDaylightSavingTime(dateTime);
        }

        public static DateTime ToNzStandardTime(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, "New Zealand Standard Time", "UTC").Add(TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time").BaseUtcOffset);
        }

        #endregion

        #region Time Extensions

        private const string TimeFormat = "hh\\:mm";

        public static string ToTimeString(this TimeSpan time)
        {
            return time.ToString(TimeFormat);
        }

        public static string ToTimeString(this TimeSpan? time)
        {
            return time.HasValue ? ToTimeString(time.Value) : string.Empty;
        }

        public static TimeSpan? ToNullableTimeSpan(this string value, string format = "hhmm")
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            TimeSpan parsedTime;
            var valueToParse = value.Trim().Replace(":", "");
            if (TimeSpan.TryParseExact(valueToParse, format, CultureInfo.InvariantCulture, out parsedTime))
            {
                if (parsedTime.Ticks >= 0) return (TimeSpan?)parsedTime;
            }

            return TimeSpan.MinValue;
        }

        public static TimeSpan? ToNullableTimeSpanTryParse(this string value)
        {

            var timeRegex = new Regex("^(([0-9])|([0-1][0-9])|([2][0-3])):(([0-9])|([0-5][0-9]))$");

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (!timeRegex.IsMatch(value))
            {
                return TimeSpan.MinValue;
            }

            TimeSpan parsedTime;

            if (TimeSpan.TryParse(value, out parsedTime))
            {
                if (parsedTime.Ticks > 0) return parsedTime;
            }

            return TimeSpan.MinValue;
        }

        #endregion

        #region WebControls Extensions

        public static T FindInMasterPages<T>(this Page page, string placeholder) where T : class
        {
            MasterPage masterPage = page.Master;
            while (masterPage != null)
            {
                Control control = masterPage.FindControl(placeholder);
                if (control != null)
                {
                    return control as T;
                }

                masterPage = masterPage.Master;
            }

            return null;
        }

        public delegate void ControlAction<T>(T control);

        /// <summary>
        ///    Recursively loops through all controls nested within the passed in Control object and 
        ///    executes the supplied action on the first instance found of them
        /// </summary>
        public static void RecursiveFindFirstControl<T>(this Control control, ControlAction<T> controlAction) where T : class
        {
            foreach (Control childControl in control.Controls)
            {
                if (childControl is T)
                {
                    controlAction.Invoke(childControl as T);
                }
                else if (childControl.HasControls())
                {
                    childControl.RecursiveFindFirstControl(controlAction);
                }
            }
        }

        /// <summary>
        ///    Recursively loops through all controls nested within the passed in Control object and 
        ///    executes the supplied action on every instance found of them. Quite an expensive method.
        ///    Use only when required.
        /// </summary>
        public static void RecursiveFindControl<T>(this Control control, ControlAction<T> controlAction) where T : class
        {
            foreach (Control childControl in control.Controls)
            {
                if (childControl is T)
                {
                    controlAction.Invoke(childControl as T);
                }
                
                if (childControl.HasControls())
                {
                    childControl.RecursiveFindControl(controlAction);
                }
            }
        }

        /// <summary>
        ///    Recursively loops through all controls nested within the passed in Control object and returns the control that
        ///    contains the supplied ID.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control"></param>
        /// <param name="id"></param>
        /// <param name="controlAction"></param>
        public static void RecursiveFindFirstControl<T>(this Control control, string id, ControlAction<T> controlAction) where T : class
        {
            foreach (Control childControl in control.Controls)
            {
                if (childControl is T && childControl.ID == id)
                {
                    controlAction.Invoke(childControl as T);
                    break;
                }

                if (childControl.HasControls())
                {
                    childControl.RecursiveFindFirstControl(id, controlAction);
                }
            }
        }

        public static void RecursiveFindFirstControl<T>(this Control control, bool condition, string id, ControlAction<T> controlAction) where T : class
        {
            if (condition)
            {
                control.RecursiveFindFirstControl(id, controlAction);
            }
        }

        public static ContentPlaceHolder FindContentPlaceHolder(this Page page, string rootPlaceHolder, params string[] childPlaceHolders)
        {
            ContentPlaceHolder placeHolderDestination = page.FindInMasterPages<ContentPlaceHolder>(rootPlaceHolder);

            foreach (var placeHolder in childPlaceHolders)
            {
                try
                {
                    ContentPlaceHolder findControl = (ContentPlaceHolder)placeHolderDestination.FindControl(placeHolder);
                    placeHolderDestination = findControl;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return placeHolderDestination;
        }

        public static Control FindFiredControl(this Page page)
        {
            Control control = page;
            Boolean asyncPost = (page.Request.Form["__ASYNCPOST"] == "true");
            String eventTarget = page.Request.Form["__EVENTTARGET"];

            if (page.IsPostBack == true)
            {
                //check if the postback control is not a button
                if (String.IsNullOrEmpty(eventTarget) == false)
                {
                    //all naming containers
                    String[] parts = eventTarget.Split('$');

                    foreach (String part in parts)
                    {
                        //find control on its naming container
                        control = control != null ? control.FindControl(part) : null;
                    }
                }
                else
                {
                    //search all submitted form keys
                    foreach (String key in page.Request.Form.AllKeys.Where(k => Char.IsLetter(k[0])).ToArray())
                    {
                        //all naming containers
                        String[] parts = key.Split('$');

                        //initialize control to page on each iteration
                        control = page;

                        foreach (String part in parts)
                        {
                            //find control on its naming container
                            if ((part.EndsWith(".x")) || (part.EndsWith(".y")))
                            {
                                //ImageButton
                                control = control != null ? control.FindControl(part.Substring(0, part.Length - 2)) : null;
                            }
                            else
                            {
                                //other button type
                                control = control != null ? control.FindControl(part) : null;
                            }
                        }

                        if (control is IPostBackEventHandler)
                        {
                            if (((control is ScriptManager) || (control is ScriptManagerProxy)) && (asyncPost == true))
                            {
                                //ScriptManager/ScriptManagerProxy themselves never fire postback events
                                continue;
                            }
                            //found firing event
                            break;
                        }

                        //clear control for next iteration
                        control = null;
                    }
                }
            }

            return (control);
        }


        /// <summary>
        /// Return the the root master page
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="page"></param>
        /// <returns></returns>
        public static MasterPage RootMasterPage(this Page page)
        {
            MasterPage masterPage = page.Master;
            while (masterPage.Master != null)
            {
                masterPage = masterPage.Master;
            }

            return masterPage;
        }

        #endregion

        #region String Extensions

        public static string NullIfEmpty(this string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }

        public static string EmptyIfNull(this string value)
        {
            return value == null? string.Empty: value;
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static Nullable<T> ToNullable<T>(this string s) where T : struct
        {
            Nullable<T> result = new Nullable<T>();
            try
            {
                if (!string.IsNullOrEmpty(s) && s.Trim().Length > 0)
                {
                    TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
                    result = (T)conv.ConvertFrom(s);
                }
            }
            catch { }
            return result;
        }

        /// <summary>
        /// In order to keep the information about whether the source string (in this case the string is "value") is numeric in a int (or long),
        /// we define -1 as not numeric only. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long? ParseAsNullableLong(this string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return ParseAsLong(value);
        }

        public static bool ParseAsBoolean(this string value)
        {
            bool boolValue;
            Boolean.TryParse(value, out boolValue);
            return boolValue;
        }

        public static bool ParseComplexAsBoolean(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            value = value.Trim().ToLower();

            string[] validTrue = { "true", "t", "1", "yes", "y" };

            return validTrue.Any(x => x.Contains(value));
        }

        public static bool? ParseAsNullableBoolean(this string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return ParseAsBoolean(value);
        }
       
        public static short? ParseAsNullableShort(this string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return ParseAsShort(value);
        }

        public static long ParseAsLong(this string value)
        {
            long result;
            Int64.TryParse(value, out result);
            return result;
        }

        public static short ParseAsShort(this string value)
        {
            short result;
            Int16.TryParse(value, out result);
            return result;
        }

        public static int? ParseAsNullableInt(this string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return ParseAsInt(value);
        }

        public static string ToSentenceCase(this string str)
        {
            return Regex.Replace(str, "[a-z][A-Z]", m => string.Format("{0} {1}", m.Value[0], char.ToLower(m.Value[1])));
            // Build server does not support C#6
            //return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
        }
        /// <summary>
        /// Adds space between the words that has upper case characters ie "AuthorisationType" returns "Authorisation Type"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>

        public static string ToWordCase(this string str)
        {
            return Regex.Replace(str, "[a-z][A-Z]", m => string.Format("{0} {1}", m.Value[0], m.Value[1]));
            // Build server does not support C#6
            //return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {m.Value[1]}");
        }

        public static string SafeSubstring(this string text, int start, int length)
        {
            return string.IsNullOrEmpty(text) ? string.Empty
                : text.Length <= start ? string.Empty
                : text.Length - start <= length ? text.Substring(start)
                : text.Substring(start, length);
        }

        public static string ParseNullableDecimalAsCurrency(this decimal? value)
        {
            if (value == null) return string.Empty;
            return string.Format("{0:C2}", value).Remove(0, 1);
        }  
        
        public static string ParseDecimalAsCurrency(this decimal value)
        {
            return string.Format("{0:C2}", value).Remove(0, 1);
        }

        public static string ParseDecimalAsCurrencyNoDecimalPlaces(this decimal value)
        {
            return string.Format("{0:C0}", value).Remove(0, 1);
        }

        public static string ParseCurrencyAsNullableDecimalNoDecimalPlaces(this decimal? value)
        {
            if (value.HasValue) 
                return ParseDecimalAsCurrencyNoDecimalPlaces(value.Value);
            else
                return "";
        }

        public static decimal ParseCurrencyAsDecimal(this string value)
        {
            decimal result = 0;
            const int maxCount = 100;
            if (String.IsNullOrEmpty(value))
                return decimal.MinValue;

            const string decimalNumberPattern = @"^\-?[0-9]{{1,{4}}}(\{0}[0-9]{{{2}}})*(\{0}[0-9]{{{3}}})*(\{1}[0-9]+)*$";

            NumberFormatInfo format = CultureInfo.CurrentCulture.NumberFormat;

            int secondaryGroupSize = format.CurrencyGroupSizes.Length > 1
                    ? format.CurrencyGroupSizes[1]
                    : format.CurrencyGroupSizes[0];

            var r = new Regex(String.Format(decimalNumberPattern
                                           , format.CurrencyGroupSeparator == " " ? "s" : format.CurrencyGroupSeparator
                                           , format.CurrencyDecimalSeparator
                                           , secondaryGroupSize
                                           , format.CurrencyGroupSizes[0]
                                           , maxCount), RegexOptions.Compiled | RegexOptions.CultureInvariant);
            if (!r.IsMatch(value.Trim()) )
                return decimal.MinValue;
            if (!Decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
                return decimal.MinValue;
            if (result < 0)
                return decimal.MinValue;
            return result;
        }

        public static decimal? ParseCurrencyAsNullableDecimal(this string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return ParseCurrencyAsDecimal(value);
        }

        public static int ParseAsInt(this string value)
        {
            int result;
            bool tryParse = Int32.TryParse(value, out result);
            return tryParse ? result : int.MinValue;
        }

        public static decimal? ParseAsNullableDecimal(this string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return ParseAsDecimal(value);
        }

        public static double? ParseAsNullableDouble(this string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return ParseAsDouble(value);
        } 
        
        public static decimal ParseAsDecimal(this string value)
        {
            decimal result;
            bool tryParse = Decimal.TryParse(value, out result);
            return tryParse ? result : decimal.MinValue;
        }   
        
        public static double ParseAsDouble(this string value)
        {
            double result;
            bool tryParse = Double.TryParse(value, out result);
            return tryParse ? result : double.MinValue;
        }

        public static string NewlineToParagraph(this object text)
        {
            string result = "<p>" + text.ToString()
                .Replace(Environment.NewLine + Environment.NewLine, "</p><p>")
                .Replace(Environment.NewLine, "<br />")
                .Replace("</p><p>", "</p>" + Environment.NewLine + "<p>") + "</p>";

            return result;
        }


        public static TEnum? ParseAsNullableEnum<TEnum>(this string value) where TEnum : struct
        {
            return EnumHelper.TryParse<TEnum>(value);
        }

        /// <summary>
        /// Returns the first x characters from a string
        /// </summary>
        /// <param name="maxNbCharacters"></param>
        /// <returns></returns>
        public static string MaxLength(this string s, int maxNbCharacters)
        {
            return s != null && s.Length > maxNbCharacters ? s.Substring(0, maxNbCharacters) : s;
        }

        public static string ToAmountString(this double? value, int precison = 10)
        {
            return !value.HasValue ? "" : ToAmountString(value.Value);
        }

        public static string ToAmountString(this double value, int precison = 10)
        {
            //"{0:.###########}"
            var format = "{0:." + new string('#', precison) + "}";
            return String.Format(format, value);
        }

        public static string ReplaceCaseInsensitive(this string input, string search, string replacement)
        {
            string result = Regex.Replace(input, Regex.Escape(search), replacement.Replace("$", "$$"), RegexOptions.IgnoreCase);
            return result;
        }

        #endregion

        #region LINQ Extensions

        public static T RandomSingle<T>(this IEnumerable<T> target)
        {
            Random r = new Random(DateTime.Now.Millisecond);
            int position = r.Next(target.Count());
            return target.ElementAt(position);
        }

        public static TResult Eval<T, TResult>(this T obj, Func<T, TResult> func)
                    where T : class
        {
            return obj.Eval(func, default(TResult));
        }

        public static TResult Eval<T, TResult>(this T obj, Func<T, TResult> func, TResult defaultValue)
            where T : class
        {
            return obj == null ? defaultValue : func(obj);
        }

        public static void Execute<T>(this T obj, Action<T> action) where T : class
        {
            if (obj != null)
                action(obj);
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        #endregion

        #region IEnumerable Extensions

        //Fileters an IEnumberable (distinct) based on a specified property from the object in the IEnumerable. 
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();

            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        //TODO - Currently this is not being used after WorkflowReopsitory.GetTaskGroups and GetTaskUsers is removed.
        //Please revisit this logic before this is used somewhere else - especialy for the case of isEmptySource || isEmptySecondSource, shouldn't
        //the intersection of empty source return a empty list?
        //Finds the intersection of IEnumerables based on a specified property from the generic type.
        public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> source, IEnumerable<TSource> secondSource, Func<TSource, TKey> keySelector)
        {
            bool isEmptySource = source == null || source.Count() == 0;
            bool isEmptySecondSource = secondSource == null || secondSource.Count() == 0;

            if (isEmptySource)  
            {
                foreach (TSource element in secondSource)
                {
                    yield return element;
                }
            }
            if (isEmptySecondSource)
            {
                foreach (TSource element in source)
                {
                    yield return element;
                }
            }

            if (!isEmptySource && !isEmptySecondSource)
            {
                HashSet<TKey> seenKeys = new HashSet<TKey>();
                foreach (TSource element in source)
                {
                    seenKeys.Add(keySelector(element));
                }

                foreach (TSource element in secondSource)
                {
                    if (!seenKeys.Add(keySelector(element)))
                    {
                        yield return element;
                    }
                }
            }
        }


        #endregion

        #region 'Other' Extensions

        public static string ToJSON(this object obj)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(obj);
        }

        /// <summary>
        ///  Parse a JSON string into an object T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T ParseJSONTo<T>(this string str)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(str);
        }

        public static T CastAs<T>(this object item)
        {
            return (T)item;
        }


        public static string ToStringOrEmptyIfNull<T>(this T? obj) where T: struct
        {
            return obj == null ? string.Empty : obj.ToString();
        }

        /// <summary>Pluralise or Singularise a string</summary>
        /// <param name="item">item to work on</param>
        /// <param name="pluralise">true to pluralise, false to singularise</param>
        /// <returns>Plural or Singular based on pluralise flag</returns>
        public static string ToPluralOrSingular(this string item, bool pluralise)
        {
            var itemIsPluralised = item.Substring(item.Length - 1, 1).ToLower() == "s";

            if (pluralise && !itemIsPluralised)
            {
                //Pluralise
                return item + "s";
            }

            if (!pluralise && itemIsPluralised)
            {
                //Make singular
                return item.Remove(item.Length - 1, 1);
            }

            return item;
        }

        #endregion
    }
}
