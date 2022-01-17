using System;
using System.Collections.Generic;
using Unity.Properties.Internal;
using UnityEngine;

namespace Unity.Properties.UI.Internal
{
    class InspectorVisitor<T> : PropertyVisitor, IInspectorVisitor
    {
        delegate void DefaultVisitHandler<TDeclaredType>(IProperty prop, ref TDeclaredType value, PropertyPath path);
        
        public bool EnableRootCustomInspectors = true;
        
        public T Target { get; }
        public InspectorVisitorContext VisitorContext { get; }

        readonly PropertyPath m_Path = new PropertyPath();

        public InspectorVisitor(PropertyElement bindingElement, T target)
        {
            VisitorContext = new InspectorVisitorContext(bindingElement);
            Target = target;
            AddAdapter(new NullAdapter<T>(this));
            AddAdapter(new BuiltInAdapter<T>(this));
        }

        public void PushPathPart(PropertyPath.Part path)
        {
            m_Path.PushPart(path);
        }

        public void PopPathPart()
        {
            m_Path.Pop();
        }

        public void ClearPath()
        {
            m_Path.Clear();
        }

        public void RestorePath(PropertyPath path)
        {
            for (var i = 0; i < path.PartsCount; ++i)
            {
                PushPathPart(path[i]);
            }
        }

        public void AddToPath(PropertyPath path)
        {
            for (var i = 0; i < path.PartsCount; ++i)
            {
                PushPathPart(path[i]);
            }
        }

        public void RemoveFromPath(PropertyPath path)
        {
            for (var i = 0; i < path.PartsCount; ++i)
            {
                PopPathPart();
            }
        }

        public void AddToPath(IProperty property)
        {
            if (property is IListElementProperty listElementProperty)
            {
                PushPathPart(new PropertyPath.Part(listElementProperty.Index));
            }
            else if (property is IDictionaryElementProperty dictionaryElementProperty)
            {
                PushPathPart(new PropertyPath.Part((object)dictionaryElementProperty.ObjectKey));
            }
            else
            {
                PushPathPart(new PropertyPath.Part(property.Name));
            }
        }

        public void RemoveFromPath(IProperty property)
        {
            PopPathPart();
        }

        public PropertyPath GetCurrentPath()
        {
            var path = new PropertyPath();
            path.PushPath(m_Path);
            return path;
        }

        protected override bool IsExcluded<TContainer, TValue>(Property<TContainer, TValue> property,
            ref TContainer container, ref TValue value)
        {
            var shouldShow = true;
            if (null != VisitorContext.Root.m_AttributeFilter && !(property is IPropertyWrapper))
            {
                shouldShow = VisitorContext.Root.m_AttributeFilter(property.GetAttributes());
            }
            return !shouldShow || !IsFieldTypeSupported<TContainer, TValue>() || !ShouldShowField(property);
        }

        protected override void VisitProperty<TContainer, TValue>(
            Property<TContainer, TValue> property,
            ref TContainer container, 
            ref TValue value)
            => VisitWithCustomInspectorOrDefault(property, ref container, ref value, DefaultPropertyVisit);

        public void DefaultPropertyVisit<TValue>(
            IProperty property,
            ref TValue value,
            PropertyPath path)
        {
            if (path.PartsCount > 0)
            {
                if (string.IsNullOrEmpty(GuiFactory.GetDisplayName(property)))
                    property.Visit(this, ref value);
                else
                {
                    var foldout = GuiFactory.Foldout<TValue>(property, path, VisitorContext);
                    using (VisitorContext.MakeParentScope(foldout))
                        property.Visit(this, ref value);
                }
            }
            else
                property.Visit(this, ref value);
        }

        protected override void VisitList<TContainer, TList, TElement>(
            Property<TContainer, TList> property,
            ref TContainer container,
            ref TList value)
            => VisitWithCustomInspectorOrDefault(property, ref container, ref value, DefaultListVisit<TList, TElement>);
  
        void DefaultListVisit<TList, TElement>(
            IProperty property,
            ref TList value,
            PropertyPath path)
            where TList : IList<TElement>
        {
            var element = GuiFactory.Foldout<TList, TElement>(property, path, VisitorContext);
            VisitorContext.Parent.contentContainer.Add(element);
            using (VisitorContext.MakeParentScope(element))
            {
                element.Reload(property);
            }
        }

        protected override void VisitDictionary<TContainer, TDictionary, TKey, TValue>(
            Property<TContainer, TDictionary> property,
            ref TContainer container,
            ref TDictionary value)
            => VisitWithCustomInspectorOrDefault(property, ref container, ref value,
                DefaultDictionaryVisit<TDictionary, TKey, TValue>);

        void DefaultDictionaryVisit<TDictionary, TKey, TValue>(
            IProperty property,
            ref TDictionary value,
            PropertyPath path)
            where TDictionary : IDictionary<TKey, TValue>
        {
            var element = GuiFactory.Foldout<TDictionary, TKey, TValue>(property, path, VisitorContext);
            VisitorContext.Parent.contentContainer.Add(element);
            using (VisitorContext.MakeParentScope(element))
            {
                element.Reload(property);
            }
        }

        protected override void VisitSet<TContainer, TSet, TValue>(
            Property<TContainer, TSet> property,
            ref TContainer container,
            ref TSet value)
            => VisitWithCustomInspectorOrDefault(property, ref container, ref value, DefaultSetVisit<TSet, TValue>);

        void DefaultSetVisit<TSet, TValue>(IProperty property,
            ref TSet value,
            PropertyPath path)
            where TSet : ISet<TValue>
        {
            var element = GuiFactory.SetFoldout<TSet, TValue>(property, path, VisitorContext);
            VisitorContext.Parent.contentContainer.Add(element);
            using (VisitorContext.MakeParentScope(element))
            {
                element.Reload(property);
            }
        }

        void VisitWithCustomInspectorOrDefault<TContainer, TDeclaredType>(IProperty property, ref TContainer container, ref TDeclaredType value, DefaultVisitHandler<TDeclaredType> defaultVisit)
        {
            if (!RuntimeTypeInfoCache.IsContainerType(value.GetType()))
            {
                return;
            }
            
            var isWrapper = typeof(PropertyWrapper<TDeclaredType>).IsInstanceOfType(container);
            if (!isWrapper)
                AddToPath(property);

            try
            {
                var path = GetCurrentPath();
                using (var references = VisitorContext.MakeVisitedReferencesScope(this, ref value, path))
                {
                    if (references.VisitedOnCurrentBranch)
                    {
                        VisitorContext.Parent.Add(new CircularReferenceElement<TDeclaredType>(VisitorContext.Root, property, value, path, references.GetReferencePath()));
                        return;
                    }

                    if (TryVisitWithCustomInspector(ref value, path, property))
                        return;

                    defaultVisit(property, ref value, path);
                }
            }
            finally
            {
                if (!isWrapper)
                    RemoveFromPath(property);
            }
        }
        
        bool TryVisitWithCustomInspector<TDeclaredValue>(ref TDeclaredValue value, PropertyPath path, IProperty property)
        {
            var inspector = GetCustomInspector(ref value, path, property);
            var old = EnableRootCustomInspectors;
            EnableRootCustomInspectors = true;
            if (null != inspector)
            {
                var customInspector = new CustomInspectorElement(path, inspector, VisitorContext.Root);
                VisitorContext.Parent.contentContainer.Add(customInspector);
                return true;
            }
            EnableRootCustomInspectors = old;
            return false;
        }

        IInspector GetCustomInspector<TDeclaredValue>(ref TDeclaredValue value, PropertyPath path, IProperty property)
        {
            if (!EnableRootCustomInspectors) 
                return null;
            
            var visitor = CustomInspectorVisitor<TDeclaredValue>.Pool.Get();
            visitor.PropertyPath = path;
            visitor.Root = VisitorContext.Root;
            visitor.Property = property;
            try
            {
                PropertyContainer.Visit(ref value, visitor);
                return visitor.Inspector;
            }
            finally
            {
                CustomInspectorVisitor<TDeclaredValue>.Pool.Release(visitor);
            }
        }

        static bool IsFieldTypeSupported<TContainer, TValue>()
        {
            if (typeof(TValue) == typeof(object) && typeof(TContainer) != typeof(PropertyWrapper<TValue>))
                return false;

            if (Nullable.GetUnderlyingType(typeof(TValue)) != null)
                return false;

            return true;
        }

        static bool ShouldShowField<TContainer, TValue>(Property<TContainer, TValue> property)
        {
            return !property.HasAttribute<HideInInspector>();
        }
    }
}