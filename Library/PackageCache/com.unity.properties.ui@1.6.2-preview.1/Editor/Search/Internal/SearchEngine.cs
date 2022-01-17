using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.Internal;

namespace Unity.Properties.UI.Internal
{
    /// <summary>
    /// Stores a property path which should be used as search data.
    /// </summary>
    readonly struct SearchDataProperty
    {
        public readonly PropertyPath Path;

        public SearchDataProperty(PropertyPath path)
        {
            Path = path;
        }
    }

    /// <summary>
    /// Stores a token to property path which should be used for filtering.
    /// </summary>
    readonly struct SearchFilterProperty
    {
        public readonly string Token;
        public readonly PropertyPath Path;
        public readonly string[] SupportedOperatorTypes;
        
        public SearchFilterProperty(string token, PropertyPath path, string[] supportedOperatorTypes)
        {
            Token = token;
            Path = path;
            SupportedOperatorTypes = supportedOperatorTypes;
        }
    }
    
    /// <summary>
    /// The search engine backend type.
    /// </summary>
    enum SearchBackendType
    {
        /// <summary>
        /// A simple implementation used as a fallback.
        /// </summary>
        Properties,
            
        /// <summary>
        /// The primary search engine. This is only available if the QuickSearch package is installed.
        /// </summary>
        QuickSearch
    }

    class SearchEngine
    {
        readonly Dictionary<Type, ISearchBackend> m_SearchBackends = new Dictionary<Type, ISearchBackend>();

        List<SearchFilterProperty> SearchFilterProperties { get; } = new List<SearchFilterProperty>();
        List<SearchDataProperty> SearchDataProperties { get; } = new List<SearchDataProperty>();

        SearchBackendType m_BackendType;

        public SearchBackendType BackendType
        {
            get => m_BackendType;
            set
            {
                if (m_BackendType != value)
                    m_SearchBackends.Clear();
                
                m_BackendType = value;
            }
        }
        
        public List<string> SearchFilterTokens { get; } = new List<string>();

        public SearchEngine()
        {
            // Default to using quick search if the package is installed. Otherwise fallback to a simple implementation.
#if QUICKSEARCH_2_1_0_OR_NEWER
            m_BackendType = SearchBackendType.QuickSearch;
#else
            m_BackendType = SearchBackendType.Properties;
#endif
        }
        
        /// <summary>
        /// Clears the internal state of the <see cref="SearchEngine"/>.
        /// </summary>
        public void Clear()
        {
            SearchDataProperties.Clear();
            SearchFilterProperties.Clear();
            m_SearchBackends.Clear();
        }
        
        /// <summary>
        /// Adds a binding path to the search data. The property at the specified <paramref name="path"/> will be compared to the non-tokenized portion of the search string.
        /// </summary>
        /// <remarks>
        /// The search data should generally include things like id and/or name.
        /// </remarks>
        /// <param name="path">The property path to pull search data from.</param>
        public void AddSearchDataProperty(PropertyPath path)
        {
            SearchDataProperties.Add(new SearchDataProperty(path));
            
            foreach (var backend in m_SearchBackends.Values)
                backend.AddSearchDataProperty(path);
        }

        public void AddSearchDataCallback<TData>(Func<TData, IEnumerable<string>> getSearchDataFunc)
        {
            GetBackend<TData>().AddSearchDataCallback(getSearchDataFunc);
        }

        /// <summary>
        /// Adds a filter based on a binding path. The given token will resolve to a property at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="token">The identifier of the filter. Typically what precedes the operator in a filter.</param>
        /// <param name="path">The property this token should resolve to.</param>
        /// <param name="supportedOperatorTypes">List of supported operator tokens. Null for all operators.</param>
        public void AddSearchFilterProperty(string token, PropertyPath path, string[] supportedOperatorTypes = null)
        {
            SearchFilterTokens.Add(token);
            SearchFilterProperties.Add(new SearchFilterProperty(token, path, supportedOperatorTypes));

            foreach (var backend in m_SearchBackends.Values)
                backend.AddSearchFilterProperty(token, path, supportedOperatorTypes);
        }
        
        /// <summary>
        /// Adds a search filter based on a callback function. The given token will resolve to the result of the specified <paramref name="getSearchFilterFunc"/>.
        /// </summary>
        /// <param name="token">The identifier of the filter. Typically what precedes the operator in a filter.</param>
        /// <param name="getSearchFilterFunc">Callback used to get the object that is used in the filter. Takes an object of type TData and returns an object of type TFilter.</param>
        /// <param name="supportedOperatorTypes">List of supported operator tokens. Null for all operators.</param>
        /// <typeparam name="TData">The data type being searched.</typeparam>
        /// <typeparam name="TFilter">The return type for the filter.</typeparam>
        public void AddSearchFilterCallback<TData, TFilter>(string token, Func<TData, TFilter> getSearchFilterFunc, string[] supportedOperatorTypes = null)
        {
            SearchFilterTokens.Add(token);
            GetBackend<TData>().AddSearchFilterCallback(token, getSearchFilterFunc, supportedOperatorTypes);
        }

        /// <summary>
        /// Apply the filtering on an IEnumerable data set.
        /// </summary>
        /// <param name="text">The search string.</param>
        /// <returns>A filtered IEnumerable.</returns>
        public ISearchQuery<TData> Parse<TData>(string text)
        {
            return GetBackend<TData>().Parse(text);
        }

        SearchBackend<TData> GetBackend<TData>()
        {
            if (m_SearchBackends.TryGetValue(typeof(TData), out var value))
            {
                return value as SearchBackend<TData>;
            }

#if QUICKSEARCH_2_1_0_OR_NEWER
            var backend = BackendType == SearchBackendType.QuickSearch
                ? (SearchBackend<TData>) new QuickSearchBackend<TData>()
                : (SearchBackend<TData>) new PropertiesSearchBackend<TData>();
#else
            var backend = new PropertiesSearchBackend<TData>();
#endif

            m_SearchBackends[typeof(TData)] = backend;

            // Register any property based search data.
            foreach (var searchData in SearchDataProperties)
                backend.AddSearchDataProperty(searchData.Path);
            
            // Register any property based filters.
            foreach (var filter in SearchFilterProperties)
                backend.AddSearchFilterProperty(filter.Token, filter.Path, filter.SupportedOperatorTypes);
            
            return backend;
        }
    }

    interface ISearchBackend
    {
        void AddSearchDataProperty(PropertyPath path);
        void AddSearchFilterProperty(string token, PropertyPath path, string[] supportedOperatorTypes = null);
    }

    /// <summary>
    /// Common interface to abstract the query engine backend.
    /// </summary>
    /// <typeparam name="TData">The strongly typed data this engine can filter.</typeparam>
    abstract class SearchBackend<TData> : ISearchBackend
    {
        class SearchDataVisitor : PathVisitor
        {
            class CollectionSearchDataVisitor : ICollectionPropertyBagVisitor
            {
                public List<string> SearchData;
                
                void ICollectionPropertyBagVisitor.Visit<TCollection, TElement>(ICollectionPropertyBag<TCollection, TElement> properties, ref TCollection container)
                {
                    if (null == container) 
                        return;
                    
                    foreach (var element in container)
                        SearchData.Add(element?.ToString());
                }
            }

            readonly CollectionSearchDataVisitor m_CollectionSearchDataVisitor = new CollectionSearchDataVisitor();
            
            public readonly List<string> SearchData = new List<string>();

            protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
            {
                if (PropertyBagStore.GetPropertyBag<TValue>() is ICollectionPropertyBagAccept<TValue> collectionPropertyBagAccept)
                {
                    m_CollectionSearchDataVisitor.SearchData = SearchData;
                    collectionPropertyBagAccept.Accept(m_CollectionSearchDataVisitor, ref value);
                }
                else
                {
                    SearchData.Add(value?.ToString());
                }
            }

            public override void Reset()
            {
                SearchData.Clear();
                base.Reset();
            }
        }

        readonly List<PropertyPath> m_SearchDataProperties = new List<PropertyPath>();
        readonly List<Func<TData, IEnumerable<string>>> m_SearchDataFunc = new List<Func<TData, IEnumerable<string>>>();
        readonly SearchDataVisitor m_SearchDataVisitor = new SearchDataVisitor();

        /// <summary>
        /// Returns all search data strings for the given <typeparamref name="TData"/> instance.
        /// </summary>
        /// <remarks>
        /// The search data strings are extracted based on the <see cref="SearchDataProperty"/> elements registered.
        /// </remarks>
        /// <param name="data">The instance to gather data from.</param>
        /// <typeparam name="TData">The instance type.</typeparam>
        /// <returns>An <see cref="IEnumerator{T}"/> over the search data strings for the specified data.</returns>
        protected IEnumerable<string> GetSearchData(TData data)
        {
            if (RuntimeTypeInfoCache<TData>.CanBeNull)
                if (null == data) yield break;

            foreach (var searchData in m_SearchDataFunc.SelectMany(func => func(data)))
                yield return searchData;

            foreach (var searchDataPath in m_SearchDataProperties)
            {
                m_SearchDataVisitor.Reset();
                m_SearchDataVisitor.Path = searchDataPath;

                PropertyContainer.Visit(ref data, m_SearchDataVisitor, out _);

                if (m_SearchDataVisitor.ErrorCode != VisitErrorCode.Ok) 
                    continue;

                foreach (var element in m_SearchDataVisitor.SearchData.Where(element => null != element))
                    yield return element;
            }
        }

        public void AddSearchDataProperty(PropertyPath path)
        {
            m_SearchDataProperties.Add(path);
        }
        
        public void AddSearchDataCallback(Func<TData, IEnumerable<string>> getSearchDataFunc)
        {
            if (null == getSearchDataFunc)
                throw new ArgumentNullException(nameof(getSearchDataFunc));
            
            m_SearchDataFunc.Add(getSearchDataFunc);
        }

        public abstract void AddSearchFilterProperty(string token, PropertyPath path, string[] supportedOperatorTypes = null);
        public abstract void AddSearchFilterCallback<TFilter>(string token, Func<TData, TFilter> getFilterDataFunc, string[] supportedOperatorTypes = null);

        /// <summary>
        /// Applies the given search text to the specified data set.
        /// </summary>
        /// <param name="text">The search string.</param>
        /// <returns>A <see cref="ISearchQuery{TData}"/> which can be applied to data to generate a filtered set.</returns>
        public abstract ISearchQuery<TData> Parse(string text);
    }
}