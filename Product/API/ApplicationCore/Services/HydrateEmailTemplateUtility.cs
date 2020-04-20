using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;

namespace UpDiddyApi.ApplicationCore.Services
{
    public static class HydrateEmailTemplateUtility
    {
        private const string OpenDelimiter = "{{";
        private const string CloseDelimiter = "}}";

        public static List<HydratedEmailTemplate> HydratedEmailTemplates(string template, Recruiter recruiter, List<Profile> profiles)
        {
            var parsedTokens = ParseTemplate(template);

            var hydrated = profiles
                .Select(profile => GetHydratedEmailTemplate(parsedTokens.ToList(), recruiter, profile))
                .ToList();

            return hydrated;
        }

        private static HydratedEmailTemplate GetHydratedEmailTemplate(List<Token> tokens, Recruiter recruiter, Profile profile)
        {
            Hydrate(tokens, recruiter, profile);

            return new HydratedEmailTemplate
            {
                Profile = profile,
                Recruiter = recruiter,
                Value = Flatten(tokens)
            };
        }

        #region Parse Template

        private static List<Token> ParseTemplate(string template)
        {
            if (string.IsNullOrEmpty(template)) { return new List<Token>(); }

            List<Token> recursive(string value, List<Token> tokens)
            {
                var index = value.IndexOf(OpenDelimiter);
                if (index == -1)
                {
                    if (value.Length > 0)
                    {
                        tokens.Add(new PlainTextToken { Value = value });
                    }
                    return tokens;
                }

                if (index > 0)
                {
                    tokens.Add(new PlainTextToken { Value = value.Substring(0, index) });
                }

                var index2 = value.IndexOf(CloseDelimiter, index);
                if (index2 != -1 && index2 > index)
                {
                    tokens.Add(ParseToken(value.Substring(index + OpenDelimiter.Length, index2 - index - CloseDelimiter.Length)));
                }

                return recursive(value.Substring(index + CloseDelimiter.Length), tokens);
            }

            return recursive(template, new List<Token>());
        }

        private static Token ParseToken(string token)
        {
            var index = token.IndexOf(',');

            string fallbackValue = null;

            if (index > -1)
            {
                fallbackValue = token.Substring(index + 1);
                token = token.Substring(0, index);
            }

            return new ParsedToken
            {
                Property = token,
                FallbackValue = fallbackValue
            };
        }

        #endregion Parse Template

        #region Hydrate Tokens

        private static void Hydrate(List<Token> tokens, Recruiter recruiter, Profile profile)
        {
            foreach (var token in tokens)
            {
                if (!(token is ParsedToken parsedToken)) { continue; }

                var objectToTest =
                    parsedToken.Property.StartsWith("recruiter.", StringComparison.CurrentCultureIgnoreCase) ? recruiter
                    : parsedToken.Property.StartsWith("profile.", StringComparison.CurrentCultureIgnoreCase) ? profile
                    : null as object;

                if (objectToTest == null) { continue; }

                var val = GetPropertyValue(objectToTest, parsedToken.Property.Remove(0, parsedToken.Property.IndexOf('.') + 1));
                if (val != null)
                {
                    parsedToken.Value = val.ToString();
                }
            }
        }

        private static object GetPropertyValue(object src, string propName)
        {
            if (src == null && string.IsNullOrEmpty(propName)) { return null; }

            if (propName.Contains('.'))
            {
                // complex nested type.  Get the child value...
                var parts = propName.Split(new char[] { '.' }, 2);
                var prop = src.GetType().GetProperty(parts[0]);
                if (prop == null) { return null; }
                var subObject = prop.GetValue(src, null);
                if (subObject == null) { return null; }

                return GetPropertyValue(subObject, parts[1]);
            }
            else
            {
                var prop = src.GetType().GetProperty(propName);
                return prop?.GetValue(src, null);
            }
        }

        #endregion Hydrate Tokens

        #region Tokens

        private abstract class Token { }

        private class PlainTextToken : Token
        {
            public string Value { get; set; }
        }

        private class ParsedToken : Token
        {
            public string Value { get; set; }
            public string Property { get; set; }
            public string FallbackValue { get; set; }

            public string GetValue()
            {
                if (!string.IsNullOrEmpty(Value)) { return Value; }
                if (!string.IsNullOrEmpty(FallbackValue)) { return FallbackValue; }
                return Property;
            }
        }

        #endregion Tokens

        private static string Flatten(List<Token> tokens)
        {
            var stringBuilder = new StringBuilder();

            foreach (var token in tokens)
            {
                switch (token)
                {
                    case PlainTextToken plainTextToken:
                        stringBuilder.Append(plainTextToken.Value);
                        break;
                    case ParsedToken parsedToken:
                        stringBuilder.Append(parsedToken.GetValue());
                        break;
                    default:
                        break;
                }
            }

            return stringBuilder.ToString();
        }
    }
}
