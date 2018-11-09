using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UpDiddyLib.Dto;

namespace UpDiddyLib.Helpers
{
    static public class Utils
    {
        static public string RemoveHTML(string Str)
        {
            return Regex.Replace(Str, "<.*?>", String.Empty);
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
        public static DateTime FromWozTime(long wozTime)
        {
            return epoch.AddMilliseconds(wozTime);
        }

        public static long ToWozTime(DateTime dateTime)
        {
            return (long)(dateTime - epoch).TotalMilliseconds;
        }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static public long CurrentTimeInUnixMilliseconds()
        {
            long rval = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return rval;
        }

        static public long CurrentTimeInUnixSeconds()
        {
            long rval = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return rval;
        }


        static public DateTime PriorDayOfWeek(DateTime StartTime, System.DayOfWeek DayOfTheWeek)
        {           
            int DaysApart = StartTime.DayOfWeek - DayOfTheWeek;
            if (DaysApart < 0) DaysApart += 7;
            DateTime PriorDay = StartTime.AddDays(-1 * DaysApart);

            return PriorDay;
        }

        static public Dictionary<CountryDto, List<StateDto>> InitializeCountryStateMapping(IList<CountryStateDto> CountryStateList)
        {
            CountryStateDto countryStateDto = CountryStateList[0];

            // May need this to contain the other info about the country
            CountryDto previousCountry = new CountryDto
            {
                DisplayName = countryStateDto.DisplayName,
                //Code2 = countryStateDto.Code2,
                Code3 = countryStateDto.Code3
            };
            List<StateDto> states = new List<StateDto>();
            Dictionary<CountryDto, List<StateDto>> CountryStateMapping = new Dictionary<CountryDto, List<StateDto>>();
            foreach (CountryStateDto csdto in CountryStateList)
            {
                if (!(previousCountry.DisplayName).Equals(csdto.DisplayName))
                {
                    CountryStateMapping.Add(previousCountry, states);
                    states = new List<StateDto>();
                    previousCountry = new CountryDto
                    {
                        DisplayName = csdto.DisplayName,
                        //Code2 = csdto.Code2,
                        Code3 = csdto.Code3
                    };
                    states.Add(new StateDto
                    {
                        Name = csdto.Name,
                        StateGuid = null
                    });
                }
                else
                {
                    states.Add(new StateDto
                    {
                        Name = csdto.Name,
                        StateGuid = null
                    });
                }
            }
            return CountryStateMapping;
        }

    }
}
