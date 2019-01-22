using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UpDiddyLib.Dto;
using System.Xml.XPath;
using System.Xml;
using UpDiddy.Helpers;

namespace UpDiddyLib.Helpers
{
    static public class Utils
    {
        public static string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 10 || phoneNumber.Length > 15)
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

            SubscriberContactInfoDto rVal = new SubscriberContactInfoDto()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = phoneNumber,
                Address = address,
                State = state,
                CountryCode = countryCode,
                City = city
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

            value = value.ToLower();
            StringBuilder NonAlphaSymbolsString = new StringBuilder();

            //Create a string of only non-alpha characters
            foreach (char val in value.ToCharArray())
            {
                if (!IsEnglishLetter(val) && !IsNumeric(val))
                {
                    NonAlphaSymbolsString.Append(val);
                }
            }

            //Transfer non-alpha string to array
            char[] NonAlphaSymbols = NonAlphaSymbolsString.ToString().ToCharArray();

            if(NonAlphaSymbols.Length > 0)
            {
                string[] IndividualNameStrings = Regex.Split(value, "[- '.()]");
                StringBuilder FinalString = new StringBuilder();
                int NonAlphaCount = 0;
                if (!IsEnglishLetter(value.ToCharArray()[0]) && !IsNumeric(value.ToCharArray()[0]))
                {
                    FinalString.Append(NonAlphaSymbols[NonAlphaCount]);
                    NonAlphaCount++;
                }
                foreach(string var in IndividualNameStrings)
                {
                    FinalString.Append(IndividualStringToUpper(var));
                    if(NonAlphaCount < NonAlphaSymbols.Length)
                    {
                        FinalString.Append(NonAlphaSymbols[NonAlphaCount]);
                        NonAlphaCount++;
                    }
                    
                }
                return FinalString.ToString();
            }
            else
            {
                return IndividualStringToUpper(value);
            }


            
        }

        public static string IndividualStringToUpper(string value)
        {
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.  
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            return new string(array);
        }

        public static bool IsEnglishLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        public static bool IsNumeric(char c)
        {
            return (c >= '0' && c <= '9');
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
            if(!startDate.HasValue || startDate.Value == DateTime.MinValue)
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
        [Obsolete("Remove this once we are certain we cannot make use of it",false)]
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
                if(!string.IsNullOrWhiteSpace(compensationType))
                {
                    formattedCompensation += $" ({compensationType})";
                }
            }
            return formattedCompensation;
        }
    }
}
