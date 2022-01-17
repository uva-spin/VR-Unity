using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Build.Editor
{
    class BuildConfigurationDependencyList : IList<BuildConfiguration>, IList
    {
        BuildConfiguration m_Source;
        BuildConfiguration m_Target;
        List<BuildConfiguration> m_List;

        public Action OnChanged = delegate { };

        public BuildConfigurationDependencyList(BuildConfiguration source, BuildConfiguration target)
        {
            m_Source = source;
            m_Target = target;
            m_List = FilterDependencies(target.Dependencies.Select(dependency => dependency.asset)).ToList();
        }

        public int IndexOf(BuildConfiguration item) => m_List.IndexOf(item);

        public void Insert(int index, BuildConfiguration item)
        {
            m_List.Insert(index, item);
            Update();
        }

        public void RemoveAt(int index)
        {
            m_List.RemoveAt(index);
            Update();
        }

        public BuildConfiguration this[int index]
        {
            get => m_List[index];
            set
            {
                m_List[index] = value;
                Update();
            }
        }

        public void Add(BuildConfiguration item)
        {
            m_List.Add(item);
            Update();
        }

        public void Clear()
        {
            m_List.Clear();
            Update();
        }

        public bool Contains(BuildConfiguration item) => m_List.Contains(item);

        public void CopyTo(BuildConfiguration[] array, int arrayIndex) => m_List.CopyTo(array, arrayIndex);

        public bool Remove(BuildConfiguration item)
        {
            if (m_List.Remove(item))
            {
                Update();
                return true;
            }
            return false;
        }

        public int Count => m_List.Count;

        public bool IsReadOnly => ((IList<BuildConfiguration>)m_List).IsReadOnly;

        public IEnumerator<BuildConfiguration> GetEnumerator() => ((IEnumerable<BuildConfiguration>)m_List).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_List).GetEnumerator();

        public int Add(object value)
        {
            var result = ((IList)m_List).Add(value);
            Update();
            return result;
        }

        public bool Contains(object value) => ((IList)m_List).Contains(value);

        public int IndexOf(object value) => ((IList)m_List).IndexOf(value);

        public void Insert(int index, object value)
        {
            ((IList)m_List).Insert(index, value);
            Update();
        }

        public void Remove(object value)
        {
            ((IList)m_List).Remove(value);
            Update();
        }

        public bool IsFixedSize => ((IList)m_List).IsFixedSize;

        object IList.this[int index]
        {
            get => ((IList)m_List)[index];
            set => ((IList)m_List)[index] = value;
        }

        public void CopyTo(Array array, int index) => ((ICollection)m_List).CopyTo(array, index);

        public bool IsSynchronized => ((ICollection)m_List).IsSynchronized;

        public object SyncRoot => ((ICollection)m_List).SyncRoot;

        void Update()
        {
            m_Target.Dependencies.Clear();
            m_Target.Dependencies.AddRange(FilterDependencies(m_List).Select(dependency => new LazyLoadReference<BuildConfiguration>(dependency)));
            OnChanged.Invoke();
        }

        IEnumerable<BuildConfiguration> FilterDependencies(IEnumerable<BuildConfiguration> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                if (dependency == null || !dependency || dependency == m_Source || dependency.HasDependency(m_Source))
                {
                    yield return null;
                }
                else
                {
                    yield return dependency;
                }
            }
        }
    }
}
