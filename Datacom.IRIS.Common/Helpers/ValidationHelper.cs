using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Datacom.IRIS.Common
{
    public static class ValidationHelper
    {
        private const string INTEGER_ONLY_REGEX = "^[0-9]*$";
        private const string NUMERIC_ONLY_REGEX = "^[+,-]{0,1}[0-9]+(.[0-9]+){0,1}$";
        private const string POSITIVE_NUMERIC_ONLY_REGEX = "^[+]{0,1}[0-9]+(.[0-9]+){0,1}$";
        private const string ALPHA_ONLY_REGEX = "^[a-zA-Z]*$";
        private const string ALPHA_NUMERIC_ONLY_REGEX = "^[a-zA-Z0-9]*$";
        private const string ALPHA_NUMERIC_Hyphen_ONLY_REGEX = "^[a-zA-Z0-9-]*$";
        private static readonly string[] ParseDateFormats = { "d/M/yyyy", "d/M/yy" };
       
        public static ValidationResult CreateValidationResult(string message, string key, params string[] tags)
        {
            return new TranslatedValidationResult(message, key, tags);
        }

        public static void AddValidationResult(string message, string key, ValidationResults results, params string[] tags)
        {
            results.AddResult(CreateValidationResult(string.Format(message, tags), key, tags));
        }

        public static void AddValidationResultIf(bool condition, string message, string key, ValidationResults results, params string[] tags)
        {
            if (condition)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void RequiredValidator(object value, string message, string key, ValidationResults results, params string[] tag)
        {
            if (IsValueNotSet(value))
            {
                AddValidationResult(message, key, results, tag);
            }
        }

        public static void RequiredValidator(int value, string message, string key, ValidationResults results, params string[] tag)
        {
            if (value == int.MinValue)
            {
                AddValidationResult(message, key, results, tag);
            }
        }


        /**
         * There are some entities with child entities were a developer will either 
         * set the child entity object, or the child entity ID depending on how he 
         * chooses to set values in the parent entity. This validation method checks 
         * that either entity or entity ID need to be set against an entity object
         */
        public static void RequiredValidator(object value, long? id, string message, string key, ValidationResults results, params string[] tag)
        {
            if (IsValueNotSet(value) && id == 0)
            {
                AddValidationResult(message, key, results, tag);
            }
        }

        public static void RequiredIDValidator(long? id, string message, string key, ValidationResults results, params string[] tag)
        {
            if (id == 0)
            {
                AddValidationResult(message, key, results, tag);
            }
        }

        public static void RequiredIfValidator(object value, long? id, bool conditionValue, string message, string key, ValidationResults results, params string[] tag)
        {
            if (conditionValue)
            {
                RequiredValidator(value, id, message, key, results, tag);
            }
        }

        public static void RequiredValidator(DateTime date, string message, string key, ValidationResults results, params string[] tag)
        {
            if (date.Equals(DateTime.MinValue))
            {
                AddValidationResult(message, key, results, tag);
            }
        }

        public static void AddErrorIfTrue(DateTime date, bool conditionValue, string message, string key, ValidationResults results, params string[] tag)
        {
            if (conditionValue)
            {
                AddValidationResult(message, key, results, tag);
            }
        }


        private static bool IsValueNotSet(object value)
        {
            bool isObjectNotSet = value == null;
            bool isBoolNotSet = value is bool && !(bool)value;
            bool isStringNotSet = value is string && string.IsNullOrEmpty((string)value);

            return isObjectNotSet || isBoolNotSet || isStringNotSet;
        }

        /// <summary>
        ///    The validator only runs when both lowerBound and upperBound exist, otherwise the validation will pass.
        /// </summary>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tag"></param>
        public static void LengthValidator(long? lowerBound, long? upperBound, string value, string message, string key, ValidationResults results, params string[] tag)
        {
            if (lowerBound.HasValue && upperBound.HasValue)
            {
                LengthValidator(lowerBound.Value, upperBound.Value, value, message, key, results, tag);
            }
        }

        /// <summary>
        ///    The validator only runs when value is not null, otherwise the validation will pass.
        /// </summary>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tag"></param>
        public static void LengthValidator(long lowerBound, long upperBound, string value, string message, string key, ValidationResults results, params string[] tag)
        {
            if (value != null)
            {
                if (value.Length > upperBound || value.Length < lowerBound)
                {
                    AddValidationResult(message, key, results, tag);
                }
            }
        }      
        
        public static void LengthValidatorIf(long lowerBound, long upperBound, bool condition, string value, string message, string key, ValidationResults results, params string[] tag)
        {
            if (value != null && condition)
            {
                if (value.Length > upperBound || value.Length < lowerBound)
                {
                    AddValidationResult(message, key, results, tag);
                }
            }
        }

        /// <summary>
        ///    The validator only runs if value and compareToDate can parse to valid DateTime values.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="compareToDate"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tag"></param>
        public static void IsAfterValidator(string value, string compareToDate, string message, string key, ValidationResults results, params string[] tag)
        {
            DateTime dateTime = DateTime.Now, compareTo = DateTime.MinValue;
            bool validCompareToDate = !string.IsNullOrEmpty(compareToDate) && DateTime.TryParse(compareToDate, out compareTo);
            bool validValue = !string.IsNullOrEmpty(value) && DateTime.TryParse(value, out dateTime);

            if (validCompareToDate && validValue)
            {
                IsAfterValidator(dateTime, compareTo, message, key, results, tag);
            }
        }

        /// <summary>
        ///    The validator only runs when value is not null, otherwise the validation will pass.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="compareToDate"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tag"></param>
        public static void IsAfterValidator(DateTime? value, DateTime compareToDate, string message, string key, ValidationResults results, params string[] tag)
        {
            if (value.HasValue)
            {
                if (DateTime.Compare(value.Value, compareToDate) < 0 && value != DateTime.MinValue)
                {
                    AddValidationResult(message, key, results, tag);
                }
            }
        }

        /// <summary>
        ///    The validator only runs when value is not null and the condition is met, otherwise the validation will pass.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="condition"></param>
        /// <param name="compareToDate"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tag"></param>
        public static void IsAfterValidatorIf(DateTime? value, bool condition,DateTime compareToDate, string message, string key, ValidationResults results, params string[] tag)
        {
            if (condition && value.HasValue)
            {
                if (DateTime.Compare(value.Value, compareToDate) < 0 && value != DateTime.MinValue)
                {
                    AddValidationResult(message, key, results, tag);
                }
            }
        }

        /// <summary>
        ///    The validator only runs when value is not null and the condition is met, otherwise the validation will pass.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="condition"></param>
        /// <param name="compareToDate"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tag"></param>
        public static void IsBeforeValidatorIf(DateTime? value, bool condition,DateTime compareToDate, string message, string key, ValidationResults results, params string[] tag)
        {
            if (condition && value.HasValue)
            {
                if (DateTime.Compare(value.Value, compareToDate) > 0 && value != DateTime.MinValue)
                {
                    AddValidationResult(message, key, results, tag);
                }
            }
        }

        /// <summary>
        ///    The validator only runs if value and compareToDate can parse to valid DateTime values.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="compareToDate"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tag"></param>
        public static void IsBeforeValidator(string value, string compareToDate, string message, string key, ValidationResults results, params string[] tag)
        {
            DateTime dateTime = DateTime.Now, compareTo = DateTime.MaxValue;
            bool validCompareToDate = !string.IsNullOrEmpty(compareToDate) && DateTime.TryParse(compareToDate, out compareTo);
            bool validValue = !string.IsNullOrEmpty(value) && DateTime.TryParse(value, out dateTime);

            if (validCompareToDate && validValue)
            {
                IsBeforeValidator(dateTime, compareTo, message, key, results, tag);
            }
        }

        /// <summary>
        ///    The validator only runs if value is not null, otherwise it will pass the validation.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="compareToDate"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tag"></param>
        public static void IsBeforeValidator(DateTime? value, DateTime compareToDate, string message, string key, ValidationResults results, params string[] tag)
        {
            if (value.HasValue)
            {
                if (DateTime.Compare(value.Value, compareToDate) > 0 && value != DateTime.MinValue)
                {
                    AddValidationResult(message, key, results, tag);
                }
            }
        }

        public static void IsBeforeValidator(DateTime value, DateTime? compareToDate, string message, string key, ValidationResults results, params string[] tag)
        {
            if (compareToDate.HasValue)
            {
                if (DateTime.Compare(value, compareToDate.Value) > 0 && compareToDate != DateTime.MinValue)
                {
                    AddValidationResult(message, key, results, tag);
                }
            }
        }

        public static void IsBeforeValidator(DateTime value, DateTime compareToDate, string message, string key, ValidationResults results, params string[] tag)
        {
            if (value != DateTime.MinValue && compareToDate != DateTime.MinValue)
            {
                if (DateTime.Compare(value, compareToDate) > 0)
                {
                    AddValidationResult(message, key, results, tag);
                }
            }
        }

        public static void RequiredIfValidator(object value, bool conditionValue, string message, string key, ValidationResults results, params string[] tag)
        {
            if (conditionValue)
            {
                RequiredValidator(value, message, key, results, tag);
            }
        }

        public static void RequiredIfValidator(object value, string conditionString, string message, string key, ValidationResults results, params string[] tag)
        {
            RequiredIfValidator(value, !string.IsNullOrEmpty(conditionString), message, key, results, tag);
        }

        public static void RequiredIfValidator(object value, double? conditionValue, string message, string key, ValidationResults results, params string[] tag)
        {
            RequiredIfValidator(value, conditionValue.HasValue, message, key, results, tag);
        }

        public static void AtLeastOneRequiredValidator(object[] values, string message, string key, ValidationResults results, params string[] tag)
        {
            if (values.Any(v => !IsValueNotSet(v)))
                return;

            AddValidationResult(message, key, results, tag);
        }

        /// <summary>
        ///    This validator only runs when the value is not empty,
        ///    otherwise the validation will pass.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tags"></param>
        public static void RegexValidator(string pattern, string value, string message, string key, ValidationResults results, params string[] tags)
        {
            if (!string.IsNullOrEmpty(value) && !Regex.IsMatch(value, pattern))
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void RegexIfValidator(string pattern, string value, int? conditionValue, string message, string key, ValidationResults results, params string[] tags)
        {
            if (conditionValue > 0)
            {
                RegexValidator(pattern, value, message, key, results, tags);
            }
        }

        public static void IsAlphanumericsValidator(string value, string message, string key, ValidationResults results, params string[] tags)
        {
            RegexValidator(ALPHA_NUMERIC_ONLY_REGEX, value, message, key, results, tags);
        }

        public static void IsAlphanumericHyphensValidator(string value, string message, string key, ValidationResults results, params string[] tags)
        {
            RegexValidator(ALPHA_NUMERIC_Hyphen_ONLY_REGEX, value, message, key, results, tags);
        }

        public static void IsIntegerValidator(string value, string message, string key, ValidationResults results, params string[] tags)
        {
            RegexValidator(INTEGER_ONLY_REGEX, value, message, key, results, tags);
        }

        public static void IsAlphaValidator(string value, string message, string key, ValidationResults results, params string[] tags)
        {
            RegexValidator(ALPHA_ONLY_REGEX, value, message, key, results, tags);
        }

        /// <summary>
        ///    This validator only run when the value can parse to a double and threshold is not null, 
        ///    otherwise the validation will pass.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="threshold"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tags"></param>
        public static void IsLessThanEqualTo(string value, decimal? threshold, string message, string key, ValidationResults results, params string[] tags)
        {
            double doubleValue;
            if (double.TryParse(value, out doubleValue) && threshold.HasValue)
            {
                IsLessThanEqualTo(doubleValue, (double)threshold.Value, message, key, results, tags);
            }
        }

        public static void IsLessThanEqualTo(double value, double threshold, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value > threshold)
            {
                AddValidationResult(message, key, results, tags);
            }
        }
        
        public static void IsLessThanEqualTo(short? value, double threshold, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value > threshold)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsLessThanEqualTo(double? value, double? threshold, string message, string key, ValidationResults results, params string[] tags)
        {
            if (!value.HasValue) return;

            if (value > threshold)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsEqualTo(int value, int compareTo, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value != compareTo)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsEqualTo(DateTime? value, DateTime compareTo, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value != compareTo)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsEqualToIf(int value, int compareTo, bool condition, string message, string key, ValidationResults results, params string[] tags)
        {
            if (condition)
            {
                IsEqualTo(value, compareTo, message, key, results, tags);
            }
        }

        public static void IsNotEqualTo(DateTime? value, DateTime compareTo, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value == compareTo)
            {
                AddValidationResult(message, key, results, tags);
            }
        }   
        
        public static void IsNotEqualTo(TimeSpan? value, TimeSpan compareTo, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value == compareTo)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsNotEqualToIf(TimeSpan? value, TimeSpan compareTo, bool condition, string message, string key, ValidationResults results, params string[] tags)
        {
            if ((value == compareTo) && condition)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsNotEqualToIf(DateTime? value, DateTime compareTo, bool condition, string message, string key, ValidationResults results, params string[] tags)
        {
            if ((value == compareTo) && condition)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsNotEqualToIf(decimal? value, decimal compareTo, bool condition, string message, string key, ValidationResults results, params string[] tags)
        {
            if ((value == compareTo) && condition)
            {
                AddValidationResult(message, key, results, tags);
            }
        }   
        
        public static void IsNotEqualToIfDouble(double? value, double compareTo, bool condition, string message, string key, ValidationResults results, params string[] tags)
        {
            if ((value == compareTo) && condition)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsNotEqualTo(long value, long? compareTo, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value == compareTo)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsNotEqualTo(decimal? value, decimal compareTo, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value == compareTo)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsNotEqualTo(double? value, double compareTo, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value == compareTo)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsNotNullIf(TimeSpan? value, bool condition, string message, string key, ValidationResults results, params string[] tags)
        {
            if ((value == null) && condition)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsGreaterThan(decimal? value, decimal? threshold, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value.HasValue)
            {
                if (value <= threshold)
                {
                    AddValidationResult(message, key, results, tags);
                }
            }
        }

        public static void IsGreaterThan(double? value, double? threshold, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value.HasValue)
            {
                if (value <= threshold)
                {
                    AddValidationResult(message, key, results, tags);
                }
            }
        }

        public static void IsGreaterThanEqualTo(int? value, int? threshold, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value.HasValue)
            {
                if (value < threshold)
                {
                    AddValidationResult(message, key, results, tags);
                }
            }
        }

        public static void IsGreaterThanEqualTo(long? value, long? threshold, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value.HasValue)
            {
                if (value < threshold)
                {
                    AddValidationResult(message, key, results, tags);
                }
            }
        }

        public static void IsGreaterThan(int? value, int? threshold, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value.HasValue)
            {
                if (value <= threshold)
                {
                    AddValidationResult(message, key, results, tags);
                }
            }
        }

        public static void IsGreaterThanEqualToIf(decimal? value, decimal? threshold, bool condition, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value.HasValue)
            {
                if ((value < threshold) && condition)
                {
                    AddValidationResult(message, key, results, tags);
                }
            }
        }       
        
        public static void IsGreaterThanEqualToIfDouble(double? value, double? threshold, bool condition, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value.HasValue)
            {
                if ((value < threshold) && condition)
                {
                    AddValidationResult(message, key, results, tags);
                }
            }
        }

        public static void WithInRange(decimal? value, decimal lowerBound, decimal upperBound, string message, string key, ValidationResults results, params string[] tags)
        {
            if (!value.HasValue) return;
            if (value <= lowerBound || value >= upperBound)
                AddValidationResult(message, key, results, tags);
        }

        /// <summary>
        ///    This validator only run when the value can pass to a double and threshold is not null, 
        ///    otherwise the validation will pass.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="threshold"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tags"></param>
        public static void IsMoreThanEqualTo(string value, decimal? threshold, string message, string key, ValidationResults results, params string[] tags)
        {
            double doubleValue;
            if (double.TryParse(value, out doubleValue) && threshold.HasValue)
            {
                IsMoreThanEqualTo(doubleValue, (double)threshold.Value, message, key, results, tags);
            }
        }

        public static void IsMoreThanEqualTo(double? value, double? threshold, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value.HasValue)
            {
                if (value < threshold)
                {
                    AddValidationResult(message, key, results, tags);
                }
            }
        }

        public static void ObjectValidator<T>(T target, ValidationResults results, string targetRuleset)
        {
            ObjectValidator validator = new ObjectValidator(typeof(T), targetRuleset);
            validator.Validate(target, results);
        }

        public static void TryObjectValidator<T>(Type type, T target, ValidationResults results, string targetRuleset) where T : class
        {
            if (target != null && target.GetType() == type)
            {
                ObjectValidator validator = new ObjectValidator(type, targetRuleset);
                validator.Validate(target, results);
            }
        }

        public static void ObjectCollectionValidator<T>(IEnumerable<T> collection, ValidationResults results, string targetRuleset) where T : class
        {
            ObjectCollectionValidator validator = new ObjectCollectionValidator(typeof(T), targetRuleset);
            validator.Validate(collection, results);
        }

        public static void TryObjectCollectionValidator<T>(Type type, IEnumerable<T> collection, ValidationResults results, string targetRuleset) where T : class
        {
            if (typeof(T) == type)
            {
                ObjectCollectionValidator validator = new ObjectCollectionValidator(type, targetRuleset);
                validator.Validate(collection, results);
            }
        }

        public static void IsTrue(bool b, string message, string key, ValidationResults results, params string[] tags)
        {
            if (!b)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        /// <summary>
        ///    This validator only runs when the value is not empty.
        /// </summary>
        public static void IsNumericIfValidator(string value, bool conditionValue, string message, string key, ValidationResults results, params string[] tags)
        {
            if (!string.IsNullOrEmpty(value) && conditionValue)
            {
                RegexValidator(NUMERIC_ONLY_REGEX, value, message, key, results, tags);
            }
        }

        /// <summary>Must be valid numeric value</summary>
        public static void IsNumeric(string value, string message, string key, ValidationResults results, params string[] tags)
        {
            if (string.IsNullOrEmpty(value))
                AddValidationResult(message, key, results, tags);
            else
                RegexValidator(NUMERIC_ONLY_REGEX, value, message, key, results, tags);
        }


        /// <summary>Must be a positive integer i.e. 1 2 3... not 0.</summary>
        public static void IsPositiveInteger(string value, string message, string key, ValidationResults results, params string[] tags)
        {
            uint result;
            if ((string.IsNullOrEmpty(value)) || (!UInt32.TryParse(value, out result)) || result == 0)
            {
                AddValidationResult(message, key, results, tags);
            }

        }
        
        /// <summary>Must be a positive decimal i.e. 0.1, 1.0... not -1</summary>
        public static void IsPositiveDecimal(string value, string message, string key, ValidationResults results, params string[] tags)
        {
            if (!string.IsNullOrEmpty(value))
            {
                RegexValidator(POSITIVE_NUMERIC_ONLY_REGEX, value, message, key, results, tags);
            }
        }
        
        /// <summary>Must be a positive decimal i.e. 0.1, 1.0... not -1</summary>
        public static void IsPositiveDecimal(double? value, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value != null && value < 0)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        /// <summary>
        ///    This validator only runs when the value is not empty.
        /// </summary>
        public static bool IsInteger(string value, string message, string key, ValidationResults results, params string[] tags)
        {
            if (!string.IsNullOrEmpty(value))
            {
                int result;
                if (!Int32.TryParse(value, out result))
                {
                    AddValidationResult(message, key, results, tags);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///    This validator will check if the value is a valid number of decimal places.
        ///    NOTE: the validator will pass if value is empty or is NOT a number format.
        ///          the validator will also pass if the decimalPlaces is null.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimalPlaces">   The decimalPlaces must be a positive integer.    </param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tags"></param>
        public static void DecimalPlaceValidator(string value, int? decimalPlaces, string message, string key, ValidationResults results, params string[] tags)
        {
            if (decimalPlaces.HasValue && !string.IsNullOrEmpty(value) && Regex.IsMatch(value, NUMERIC_ONLY_REGEX))
            {
                int numberOfDecimal = value.IndexOf('.') < 0 ? 0 : value.Length - (value.IndexOf('.') + 1);
                if (numberOfDecimal > decimalPlaces)
                {
                    AddValidationResult(message, key, results, tags);
                }
            }
        }
        /// <summary>
        /// This function checks whether the initial value is greater then the second value
        /// It is used in secenerios such as 'Must be positive integer between 1 and 999'
        /// </summary>
        public static void IsBetweenIntegerValidator(int value, int minValue, int maxValue, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value < minValue || value > maxValue)
                AddValidationResult(message, key, results, tags);
        }

        public static void IsValidDateTime(DateTime? value, string message, string key, ValidationResults results, params string[] tags)
        {
            if (value.HasValue && value.Value == DateTime.MinValue)
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        public static void IsValidTimeIf(TimeSpan? value, bool condition, string key, ValidationResults results, params string[] tags)
        {
            if (condition)
            {
                IsValidTime(value, key, results, tags);
            }
        }

        public static void IsValidTime(TimeSpan? value, string key, ValidationResults results, params string[] tags)
        {
            if (!value.HasValue) return;
            IsValidTime(value.Value,key,results,tags);
        }

        public static void IsValidTime(TimeSpan value, string key, ValidationResults results, params string[] tags)
        {
            if (value == TimeSpan.MinValue)
            {
                AddValidationResult("EM_COM024", key, results, tags);
            }
        }

        /// <summary>
        ///    The validator only runs if the value is not empty.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tags"></param>
        public static void IsDateTimeValidator(string value, string message, string key, ValidationResults results, params string[] tags)
        {
            if (!string.IsNullOrEmpty(value) && !ValidDateTime(value))
            {
                AddValidationResult(message, key, results, tags);
            }
        }

        private static bool ValidDateTime(string value)
        {
            DateTime dateTime;
            return DateTime.TryParseExact(value, ParseDateFormats, new CultureInfo("en-NZ"), DateTimeStyles.None, out dateTime);
        }

        /// <summary>
        ///    The validator only runs if the value is not empty 
        ///    and the conditionalDate1 and conditionalDate2 are both valid DateTime format
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conditionalDate1"></param>
        /// <param name="conditionalDate2"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="results"></param>
        /// <param name="tags"></param>
        public static void IsDateTimeIfValidator(string value, string conditionalDate1, string conditionalDate2, string message, string key, ValidationResults results, params string[] tags)
        {
            bool isValidateDate1 = !string.IsNullOrEmpty(conditionalDate1) && ValidDateTime(conditionalDate1);
            bool isValidateDate2 = !string.IsNullOrEmpty(conditionalDate2) && ValidDateTime(conditionalDate2);

            if (isValidateDate1 && isValidateDate2 && !string.IsNullOrEmpty(value) && !ValidDateTime(value))
            {
                AddValidationResult(message, key, results, tags);
            }
        }


        public static void RequiredIfEntityOrIdValidator(object domainObjectBase, long? objectId, bool conditionValue, string message, string key, ValidationResults results, params string[] tags)
        {
            if (conditionValue)
            {
                RequiredEntityOrIdValidator(domainObjectBase, objectId, message, key, results, tags);
            }
        }

        public static void RequiredEntityOrIdValidator(object domainObjectBase, long? objectId, string message, string key, ValidationResults results, params string[] tags)
        {
            if (domainObjectBase == null && (objectId == 0 || objectId == null))
            {
                AddValidationResult(message, key, results, tags);
            }
        }
    }

    /// <summary>
    /// Purpose of this class class is to extend the ValidationResult object to store inside
    /// it a list of tags that will be used when transating the custom error code into a proper
    /// error message using string.Format
    /// </summary>
    [Serializable]
    public class TranslatedValidationResult : ValidationResult
    {
        [DataMember]
        public string TranslatedMessage { get; set; }

        [DataMember]
        public string[] Tags { get; set; }

        public TranslatedValidationResult(string message, string key, string[] tags)
            : base(message, null, key, "", null)
        {
            Tags = tags;
        }

        public TranslatedValidationResult(string message, object target, string key, string tag, Validator validator) : base(message, target, key, tag, validator) { }

        public TranslatedValidationResult(string message, object target, string key, string tag, Validator validator, IEnumerable<ValidationResult> nestedValidationResults) : base(message, target, key, tag, validator, nestedValidationResults) { }

        public void Translate(Dictionary<string, string> errorCache)
        {
            string temp = errorCache[Message];
            TranslatedMessage = string.Format(temp, Tags);
        }
    }
}
