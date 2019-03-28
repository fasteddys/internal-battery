using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UpDiddyLib.Dto;
using System.Xml;
using System.Globalization;
using System.Reflection;

namespace UpDiddyLib.Helpers
{
    static public class Utils
    {
        /// <remarks>
        /// Shamelessly stolen from https://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class
        /// </remarks>
        /// <summary>
        /// Checks to see if a type is derived from a generic type
        /// </summary>
        /// <param name="generic"></param>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.GetTypeInfo().IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.GetTypeInfo().BaseType;
            }
            return false;
        }

        /// <summary>
        /// Convert muli-words search queries into a sql full text search query e.g. "gmail com" -> "gmail* AND com"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToSqlServerFullTextQuery(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            s = Regex.Replace(s, @"[(,)'""]", string.Empty).Trim();
            s = Regex.Replace(s, @"(\b )", @"* AND ");
            s = Regex.Replace(s, @"(\b$)", @"*");

            return s;
        }

        /// <summary>
        /// Generic formatting method to provide a placeholder when no value exists
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string FormatGenericField(string fieldValue, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldValue))
            {
                return $"No {fieldName} provided";
            }
            else
            {
                return fieldValue;
            }
        }

        /// <summary>
        /// Formats a date range depending upon the values provided for start and end date
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string FormatDateRange(DateTime? startDate, DateTime? endDate, string fieldName)
        {
            string periodDisplay = string.Empty;
            if (!startDate.HasValue)
            {
                periodDisplay = $"No {fieldName} date range specified";
            }
            else
            {
                DateTime effectiveEndDate;
                periodDisplay = startDate.Value.ToString("MMMM yyyy") + " - ";
                if (!endDate.HasValue || endDate.Value == DateTime.MinValue)
                {
                    effectiveEndDate = DateTime.UtcNow;
                    periodDisplay += "Present";
                }
                else
                {
                    effectiveEndDate = endDate.Value;
                    periodDisplay += endDate.Value.ToString("MMMM yyyy");
                }

                if (endDate > startDate)
                {
                    var period = new DateTime(effectiveEndDate.Subtract(startDate.Value).Ticks);
                    periodDisplay += " (" + period.Year + " years " + period.Month + " months)";
                }
            }
            return periodDisplay;
        }

        /// <summary>
        /// Formats an educational degree and type depending upon which fields have values
        /// </summary>
        /// <param name="degree"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string FormatEducationalDegreeAndType(string degree, string type)
        {
            string degreeDisplay = degree;
            if (!string.IsNullOrWhiteSpace(type))
            {
                degreeDisplay += $" ({type})";
            }
            if (string.IsNullOrWhiteSpace(degreeDisplay))
            {
                degreeDisplay = "No degree specified";
            }
            return degreeDisplay;
        }

        /// <summary>
        /// Formats a city, state, and postal code with commas where appropriate depending upon which fields have values
        /// </summary>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="postalCode"></param>
        /// <returns></returns>
        public static string FormatCityStateAndPostalCode(string city, string state, string postalCode)
        {
            if (string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(state) && string.IsNullOrWhiteSpace(postalCode))
            {
                return "No city, state, or postal code provided";
            }
            else if (!string.IsNullOrWhiteSpace(city))
            {
                if (!string.IsNullOrWhiteSpace(state) || !string.IsNullOrWhiteSpace(postalCode))
                {
                    return $"{city}, {state} {postalCode}".Trim();
                }
                else
                {
                    return city;
                }
            }
            else
            {
                return $"{state} {postalCode}".Trim();
            }
        }

        /// <summary>
        /// Formats a name differently depending upon which name fields have values
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        public static string FormatName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            {
                return "No name provided";
            }
            else if (string.IsNullOrWhiteSpace(firstName))
            {
                return lastName;
            }
            else if (string.IsNullOrWhiteSpace(lastName))
            {
                return firstName;
            }
            else
            {
                return $"{lastName}, {firstName}";
            }
        }

        public static string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return "No phone number provided";
            if (phoneNumber.Length < 10 || phoneNumber.Length > 15 || !Regex.Match(phoneNumber, @"^[0-9]+$").Success)
                return phoneNumber;
            int max = 15, min = 10;
            string areaCode = phoneNumber.Substring(0, 3);
            string mid = phoneNumber.Substring(3, 3);
            string lastFour = phoneNumber.Substring(6, 4);
            string extension = phoneNumber.Substring(10, phoneNumber.Length - min);
            if (phoneNumber.Length == min)
            {
                return $"({areaCode}) {mid}-{lastFour}";
            }
            else if (phoneNumber.Length > min && phoneNumber.Length <= max)
            {
                return $"({areaCode}) {mid}-{lastFour} x{extension}";
            }
            return phoneNumber;
        }

        public static string ToBase64EncodedString(IFormFile file)
        {
            string base64EncodedString = null;

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                var fileBytes = stream.ToArray();
                base64EncodedString = Convert.ToBase64String(fileBytes);
            }

            return base64EncodedString;
        }

        static public SubscriberContactInfoDto ParseContactInfoFromHrXML(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            string defaultXlms = doc.DocumentElement.NamespaceURI;
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("hrxml", defaultXlms);

            string firstName = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:PersonName/hrxml:GivenName", namespaceManager));
            string lastName = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:PersonName/hrxml:FamilyName", namespaceManager));
            string email = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:InternetEmailAddress", namespaceManager));
            string phoneNumber = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:Telephone/hrxml:FormattedNumber", namespaceManager));
            string address = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:PostalAddress/hrxml:DeliveryAddress/hrxml:AddressLine", namespaceManager));
            string state = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:PostalAddress/hrxml:Region", namespaceManager));
            string countryCode = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:PostalAddress/hrxml:Country", namespaceManager));
            string city = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:PostalAddress/hrxml:Municipality", namespaceManager));
            string postalCode = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:PostalAddress/hrxml:PostalCode", namespaceManager));

            SubscriberContactInfoDto rVal = new SubscriberContactInfoDto()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = phoneNumber,
                Address = address,
                State = state,
                CountryCode = countryCode,
                City = city,
                PostalCode = postalCode
            };

            return rVal;
        }

        static public List<string> ParseSkillsFromHrXML(string xml)
        {
            List<string> rVal = new List<String>();

            XElement theXML = XElement.Parse(xml);
            // Get list of skill found by Sovren
            var skills = theXML.Descendants()
                 .Where(e => e.Name.LocalName == "Skill")
                 .ToList();
            // Iterate over their skills 
            foreach (XElement node in skills)
                rVal.Add(node.Attribute("name").Value.Trim());

            return rVal;

        }


        static public List<SubscriberEducationHistoryDto> ParseEducationHistoryFromHrXml(string xml)
        {
            List<SubscriberEducationHistoryDto> rVal = new List<SubscriberEducationHistoryDto>();

            XElement theXML = XElement.Parse(xml);
            // Get list of skill found by Sovren
            var employmentHistory = theXML.Descendants()
                 .Where(e => e.Name.LocalName == "SchoolOrInstitution")
                 .ToList();

            // Iterate over their emplyment history  
            foreach (XElement node in employmentHistory)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(node.ToString());
                string defaultXlms = doc.DocumentElement.NamespaceURI;
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("hrxml", defaultXlms);
                bool isCurrent = false;
                // Parse attendance start date  
                DateTime startDate = ParseDateFromHrXmlDate(doc.SelectSingleNode("//hrxml:Degree/hrxml:DatesOfAttendance/hrxml:StartDate", namespaceManager), ref isCurrent);
                // Parse attendance end date 
                DateTime endDate = ParseDateFromHrXmlDate(doc.SelectSingleNode("//hrxml:Degree/hrxml:DatesOfAttendance/hrxml:EndDate", namespaceManager), ref isCurrent);
                // Parse degree date 
                DateTime degreeDate = ParseDateFromHrXmlDate(doc.SelectSingleNode("//hrxml:Degree/hrxml:DegreeDate", namespaceManager), ref isCurrent);
                string educationalInstitution = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:School/hrxml:SchoolName", namespaceManager));
                string educationalDegreeType = HrXmlNodeAttribute(doc.SelectSingleNode("//hrxml:Degree", namespaceManager), "degreeType");
                string educationalDegree = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:Degree/hrxml:DegreeName", namespaceManager)); ;
                SubscriberEducationHistoryDto educationHistory = new SubscriberEducationHistoryDto()
                {
                    CreateDate = DateTime.UtcNow,
                    ModifyDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    ModifyGuid = Guid.Empty,
                    IsDeleted = 0,
                    StartDate = startDate,
                    EndDate = endDate,
                    EducationalInstitution = educationalInstitution,
                    EducationalDegreeType = educationalDegreeType,
                    EducationalDegree = educationalDegree,
                    SubscriberEducationHistoryGuid = Guid.NewGuid()
                };
                rVal.Add(educationHistory);
            }
            return rVal;
        }


        static public List<SubscriberWorkHistoryDto> ParseWorkHistoryFromHrXml(string xml)
        {
            List<SubscriberWorkHistoryDto> rVal = new List<SubscriberWorkHistoryDto>();

            XElement theXML = XElement.Parse(xml);
            // Get list of skill found by Sovren
            var employmentHistory = theXML.Descendants()
                 .Where(e => e.Name.LocalName == "EmployerOrg")
                 .ToList();

            // Iterate over their emplyment history  
            foreach (XElement node in employmentHistory)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(node.ToString());
                string defaultXlms = doc.DocumentElement.NamespaceURI;
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("hrxml", defaultXlms);
                bool isCurrent = false;
                // Parse position start date  
                DateTime startDate = ParseDateFromHrXmlDate(doc.SelectSingleNode("//hrxml:PositionHistory/hrxml:StartDate", namespaceManager), ref isCurrent);
                // Parse position end date 
                DateTime endDate = ParseDateFromHrXmlDate(doc.SelectSingleNode("//hrxml:PositionHistory/hrxml:EndDate", namespaceManager), ref isCurrent);
                string jobTitle = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:PositionHistory/hrxml:Title", namespaceManager));
                string jobDescription = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:PositionHistory/hrxml:Description", namespaceManager));
                string company = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:EmployerOrgName", namespaceManager));
                SubscriberWorkHistoryDto workHistory = new SubscriberWorkHistoryDto()
                {
                    CreateDate = DateTime.UtcNow,
                    ModifyDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    ModifyGuid = Guid.Empty,
                    IsDeleted = 0,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsCurrent = isCurrent ? 1 : 0,
                    Company = company,
                    JobDecription = jobDescription,
                    Title = jobTitle
                };
                rVal.Add(workHistory);
            }
            return rVal;
        }


        static public string HrXmlNodeInnerText(XmlNode node)
        {
            string rVal = string.Empty;
            try
            {
                if (node != null)
                {
                    rVal = node.InnerText;
                }
                return rVal;
            }
            catch
            {
                return string.Empty;
            }
        }



        static public string HrXmlNodeAttribute(XmlNode node, string attribute)
        {
            string rVal = string.Empty;
            try
            {
                if (node != null)
                {
                    XmlElement el = node as XmlElement;
                    if (node.Attributes != null && el.HasAttribute(attribute))
                        rVal = el.Attributes[attribute].Value;
                }
                return rVal;
            }
            catch
            {
                return string.Empty;
            }
        }

        static public DateTime ParseDateFromHrXmlDate(XmlNode hrXMLDate, ref bool isCurrent)
        {
            isCurrent = false;
            string dateString = hrXMLDate.FirstChild.InnerText;

            if (hrXMLDate.FirstChild.Name == "YearMonth")
                return ParseDateFromHrXmlYearMonthTag(dateString);

            if (hrXMLDate.FirstChild.Name == "StringDate")
                ParseDateFromHrXmlStringDateTag(dateString, ref isCurrent);

            if (hrXMLDate.FirstChild.Name == "Year")
                ParseDateFromHrXmlYearTag(dateString);


            return DateTime.MinValue;
        }


        static public DateTime ParseDateFromHrXmlStringDateTag(string dateStr, ref bool isCurrent)
        {
            try
            {
                if (dateStr == "current")
                {
                    isCurrent = true;
                    return DateTime.MinValue;
                }
                // Try and parse date from StringDate tag.  This tag does not have a well defined format
                // so it probably will not parse in most cases
                return DateTime.Parse(dateStr);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }



        static public DateTime ParseDateFromHrXmlYearTag(string dateStr)
        {
            try
            {
                int year = int.Parse(dateStr);

                return new DateTime(year, 1, 1);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }




        static public DateTime ParseDateFromHrXmlYearMonthTag(string dateStr)
        {
            try
            {
                string[] dateInfo = dateStr.Split('-');
                int year = int.Parse(dateInfo[0]);
                int month = int.Parse(dateInfo[1]);

                return new DateTime(year, month, 1);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static Boolean IsValidTextFile(string Filename)
        {
            // Ensure the filename string is valid
            if (Filename == null || string.IsNullOrEmpty(Filename) || !Filename.Contains('.'))
            {
                return false;
            }

            string[] splitFileName = Filename.Split(".");
            return Constants.ValidTextFileExtensions.Contains(splitFileName[splitFileName.Length - 1]);

        }

        static public string RemoveHTML(string Str)
        {
            return Regex.Replace(Str, "<.*?>", String.Empty);
        }

        static public string RemoveQueryStringFromUrl(string url)
        {
            int idx = url.IndexOf("?");
            if (idx == -1)
                return url;
            else
                return url.Substring(0, idx);
        }


        static public T JTokenConvert<T>(JToken o, T defaultValue)
        {
            try
            {
                if (o == null)
                    return defaultValue;
                return (T)Convert.ChangeType(o.ToString(), typeof(T));
            }
            catch
            {
                return defaultValue;
            }

        }


        static public string RemoveNewlines(string Str)
        {
            return Regex.Replace(Str, "\r\n", String.Empty);
        }

        static public string RemoveRedundantSpaces(string Str)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            return regex.Replace(Str.Trim(), " ");

        }
        public static DateTime FromUnixTimeInMilliseconds(long wozTime)
        {
            return epoch.AddMilliseconds(wozTime);
        }

        public static long ToUnixTimeInMilliseconds(DateTime dateTime)
        {
            return (long)(dateTime - epoch).TotalMilliseconds;
        }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static public DateTime PriorDayOfWeek(DateTime StartTime, System.DayOfWeek DayOfTheWeek)
        {
            int DaysApart = StartTime.DayOfWeek - DayOfTheWeek;
            if (DaysApart < 0) DaysApart += 7;
            DateTime PriorDay = StartTime.AddDays(-1 * DaysApart);

            return PriorDay;
        }



        public static string ToTitleCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            return ti.ToTitleCase(value.ToLower());
        }



        public static string RemoveNonNumericCharacters(string val)
        {
            if (string.IsNullOrEmpty(val))
                return string.Empty;

            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(val, "");
        }
        [Obsolete("Remove this once we are certain we cannot make use of it", false)]
        public static string FormattedDateRange(DateTime? startDate, DateTime? endDate)
        {
            string formattedDateRange = string.Empty;
            if (!startDate.HasValue || startDate.Value == DateTime.MinValue)
            {
                return "No date range specified";
            }
            else
            {
                formattedDateRange = startDate.Value.ToString("MMMM yyyy") + " - ";
            }
            DateTime effectiveEndDate;
            if (!endDate.HasValue || endDate.Value == DateTime.MinValue)
            {
                effectiveEndDate = DateTime.UtcNow;
                formattedDateRange += "Present";
            }
            else
            {
                effectiveEndDate = endDate.Value;
                formattedDateRange += endDate.Value.ToString("MMMM yyyy");
            }
            DateTime period;
            if (effectiveEndDate > startDate.Value)
            {
                period = new DateTime(effectiveEndDate.Subtract(startDate.Value).Ticks);
            }
            else
            {
                period = DateTime.MinValue;
            }
            formattedDateRange += " (" + period.Year + " years " + period.Month + " months)";
            return formattedDateRange;
        }
        [Obsolete("Remove this once we are certain we cannot make use of it", false)]
        public static string FormattedCompensation(string compensationType, decimal compensation)
        {
            string formattedCompensation = string.Empty;
            if (compensation == 0)
            {
                return "No compensation specified";
            }
            else
            {
                formattedCompensation = $"{compensation:C}";
                if (!string.IsNullOrWhiteSpace(compensationType))
                {
                    formattedCompensation += $" ({compensationType})";
                }
            }
            return formattedCompensation;
        }

        // TODO - Remove this function and its usage (campaign landing pages) 
        //        once we're formatting the descriptions received from vendors.
        public static string FormatWozDescriptionFields(string description)
        {
            return description.Replace("Description:", "<strong>Description:</strong> ")
                .Replace("Objectives:", "<br /><br /><strong>Objectives:</strong> ");
        }
    }
}
