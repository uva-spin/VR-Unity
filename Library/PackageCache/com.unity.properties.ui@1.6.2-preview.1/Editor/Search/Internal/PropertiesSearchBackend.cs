using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Properties.UI.Internal
{
    /// <summary>
    /// Lightweight implementation of the QuickSearch.QueryEngine. This is the fallback if the QuickSearch package is not installed.
    /// </summary>
    class PropertiesSearchBackend<TData> : SearchBackend<TData>
    {
        class SearchQuery : ISearchQuery<TData>
        {
            static readonly string[] k_SupportedOperators = FilterOperator.GetSupportedOperators<TData>();

            readonly Func<TData, IEnumerable<string>> m_GetSearchDataFunc;
            readonly string[] m_Tokens;

            public string SearchString { get; }

            public ICollection<string> Tokens => m_Tokens;

            public SearchQuery(string searchString, Func<TData, IEnumerable<string>> getSearchDataFunc)
            {
                SearchString = searchString;
                m_Tokens = string.IsNullOrWhiteSpace(SearchString) ? Array.Empty<string>() : SearchString.Split(' ');
                m_GetSearchDataFunc = getSearchDataFunc;
            }
            
            public IEnumerable<TData> Apply(IEnumerable<TData> data)
            {
                var tokens = FilteredSupportedTokens().ToArray();
                return tokens.Length == 0
                    ? data
                    : data.Where(d => m_GetSearchDataFunc(d).Any(s => tokens.Any(t => s.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0)));
            }

            IEnumerable<string> FilteredSupportedTokens()
            {
                foreach (var token in m_Tokens)
                {
                    var isTokenSupported = true;
                    foreach (var supportedOperator in k_SupportedOperators)
                    {
                        if (token.IndexOf(supportedOperator, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;

                        isTokenSupported = false;
                        break;
                    }

                    if (isTokenSupported)
                        yield return token;
                }
            }
        }
        
        public override ISearchQuery<TData> Parse(string text)
        {
            return new SearchQuery(text, GetSearchData);
        }

        public override void AddSearchFilterProperty(string token, PropertyPath path, string[] supportedOperatorTypes = null)
        {
            
        }

        public override void AddSearchFilterCallback<TFilter>(string token, Func<TData, TFilter> getFilterDataFunc, string[] supportedOperatorTypes = null)
        {
        }
    }

    /// <summary>
    /// Helper class to get and apply filters without using the com.unity.quicksearch package.
    /// </summary>
    static class FilterOperator
    {
        /// <summary>
        /// Gets the available operator types for the specified type.
        /// </summary>
        /// <typeparam name="T">The type to get filters for.</typeparam>
        /// <returns>An array of operator tokens.</returns>
        public static string[] GetSupportedOperators<T>()
        {
            var operators = new List<string>
            {
                ":"
            };

            var equatable = typeof(IEquatable<T>).IsAssignableFrom(typeof(T));
            var comparable = typeof(IComparable<T>).IsAssignableFrom(typeof(T));

            if (equatable)
            {
                operators.Add("!=");
                operators.Add("=");
            }

            if (comparable)
            {
                operators.Add(">");
                operators.Add(">=");
                operators.Add("<");
                operators.Add("<=");
            }

            return operators.ToArray();
        }

        public static bool ApplyOperator<T>(string token, T value, T input, StringComparison sc)
        {
            switch (token)
            {
                case ":":
                    return value?.ToString().IndexOf(input?.ToString() ?? string.Empty, sc) >= 0;
                case "=":
                    return (value as IEquatable<T>).Equals(input);
                case "!=":
                    return !(value as IEquatable<T>).Equals(input);
                case ">":
                    return (value as IComparable<T>).CompareTo(input) > 0;
                case ">=":
                    return (value as IComparable<T>).CompareTo(input) >= 0;
                case "<":
                    return (value as IComparable<T>).CompareTo(input) < 0;
                case "<=":
                    return (value as IComparable<T>).CompareTo(input) <= 0;
            }

            return false;
        }
    }
}