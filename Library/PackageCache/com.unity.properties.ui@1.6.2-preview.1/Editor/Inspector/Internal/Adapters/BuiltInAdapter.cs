using System;
using Unity.Properties.Adapters;
using Unity.Properties.Internal;
using Object = UnityEngine.Object;

namespace Unity.Properties.UI.Internal
{
    sealed class BuiltInAdapter<T> : InspectorAdapter<T>
        , IVisit
        , IVisitPrimitives
        , IVisit<string>
        , Unity.Properties.Adapters.Contravariant.IVisit<UnityEngine.Object>
    {
        bool NoReentrace = false;

        bool AllowInspector => !NoReentrace && Visitor.EnableRootCustomInspectors; 

            delegate TElement DrawHandler<TContainer, TValue, out TElement>(
            IProperty<TContainer> property,
            ref TValue value,
            PropertyPath path,
            InspectorVisitorContext visitorContext);
        
        public BuiltInAdapter(InspectorVisitor<T> visitor) : base(visitor)
        {
        }

        VisitStatus IVisit<sbyte>.Visit<TContainer>(Property<TContainer, sbyte> property, ref TContainer container, ref sbyte value)
            => VisitPrimitive(property, ref value, GuiFactory.SByteField);

        VisitStatus IVisit<short>.Visit<TContainer>(Property<TContainer, short> property, ref TContainer container, ref short value)
            => VisitPrimitive(property, ref value, GuiFactory.ShortField);

        VisitStatus IVisit<int>.Visit<TContainer>(Property<TContainer, int> property, ref TContainer container, ref int value)
            => VisitPrimitive(property, ref value, GuiFactory.IntField);

        VisitStatus IVisit<long>.Visit<TContainer>(Property<TContainer, long> property, ref TContainer container, ref long value)
            => VisitPrimitive(property, ref value, GuiFactory.LongField);

        VisitStatus IVisit<byte>.Visit<TContainer>(Property<TContainer, byte> property, ref TContainer container, ref byte value)
            => VisitPrimitive(property, ref value, GuiFactory.ByteField);

        VisitStatus IVisit<ushort>.Visit<TContainer>(Property<TContainer, ushort> property, ref TContainer container, ref ushort value)
            => VisitPrimitive(property, ref value, GuiFactory.UShortField);

        VisitStatus IVisit<uint>.Visit<TContainer>(Property<TContainer, uint> property, ref TContainer container, ref uint value)
            => VisitPrimitive(property, ref value, GuiFactory.UIntField);

        VisitStatus IVisit<ulong>.Visit<TContainer>(Property<TContainer, ulong> property, ref TContainer container, ref ulong value)
            => VisitPrimitive(property, ref value, GuiFactory.ULongField);

        VisitStatus IVisit<float>.Visit<TContainer>(Property<TContainer, float> property, ref TContainer container, ref float value)
            => VisitPrimitive(property, ref value, GuiFactory.FloatField);

        VisitStatus IVisit<double>.Visit<TContainer>(Property<TContainer, double> property, ref TContainer container, ref double value)
            => VisitPrimitive(property, ref value, GuiFactory.DoubleField);

        VisitStatus IVisit<bool>.Visit<TContainer>(Property<TContainer, bool> property, ref TContainer container, ref bool value)
            => VisitPrimitive(property, ref value, GuiFactory.Toggle);

        VisitStatus IVisit<char>.Visit<TContainer>(Property<TContainer, char> property, ref TContainer container, ref char value)
            => VisitPrimitive(property, ref value, GuiFactory.CharField);
        
        VisitStatus IVisit<string>.Visit<TContainer>(Property<TContainer, string> property, ref TContainer container, ref string value)
            => VisitPrimitive(property, ref value, GuiFactory.TextField);
        
        VisitStatus VisitPrimitive<TContainer, TValue, TElement>(
            IProperty<TContainer> property,
            ref TValue value,
            DrawHandler<TContainer, TValue, TElement> handler
        )
        {
            Visitor.AddToPath(property);
            try
            {
                var path = Visitor.GetCurrentPath();
                
                var inspector = AllowInspector ? GetPropertyDrawer<TValue>(property, Visitor.VisitorContext.Root, path) : null;
                NoReentrace = true;
                if (null == inspector)
                {
                    handler(property, ref value, path, VisitorContext);
                }
                else
                {
                    var customInspector = new CustomInspectorElement(path, inspector, Visitor.VisitorContext.Root);
                    Visitor.VisitorContext.Parent.contentContainer.Add(customInspector);
                }
            }
            finally
            {
                Visitor.RemoveFromPath(property);
                NoReentrace = false;
            }
            return VisitStatus.Stop;
        }

        static IInspector<TValue> GetPropertyDrawer<TValue>(IProperty property, PropertyElement root, PropertyPath propertyPath)
        {
            var drawer = CustomInspectorDatabase.GetPropertyDrawer<TValue>(property.GetAttributes());
            if (null != drawer)
            {
                drawer.Context = new InspectorContext<TValue>(
                    root,
                    propertyPath,
                    property,
                    property.GetAttributes()
                );
            }
            return drawer;
        }
        
        static IInspector<TValue> GetInspector<TValue>(IProperty property, PropertyElement root, PropertyPath propertyPath)
        {
            var inspector = CustomInspectorDatabase.GetBestInspectorType<TValue>(property);
            if (null != inspector)
            {
                inspector.Context = new InspectorContext<TValue>(
                    root,
                    propertyPath,
                    property,
                    property.GetAttributes()
                );
            }
            return inspector;
        }

        public VisitStatus Visit<TContainer>(IProperty<TContainer> property, ref TContainer container,
            Object value)
             => VisitPrimitive(property, ref value, GuiFactory.ObjectField);

        public VisitStatus Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container,
            ref TValue value)
        {
            if (RuntimeTypeInfoCache<TValue>.IsEnum)
            {
                Visitor.AddToPath(property);
                try
                {
                    var path = Visitor.GetCurrentPath();
                    var inspector = AllowInspector ? GetInspector<TValue>(property, VisitorContext.Root, path) : null;
                    NoReentrace = true;
                    if (null != inspector)
                    {
                        var customInspector = new CustomInspectorElement(path, inspector, Visitor.VisitorContext.Root);
                        Visitor.VisitorContext.Parent.contentContainer.Add(customInspector);
                        return VisitStatus.Stop;
                    }
                }
                finally
                {
                    NoReentrace = false;
                    Visitor.RemoveFromPath(property);
                }

                return RuntimeTypeInfoCache<TValue>.IsEnumFlags
                    ? VisitPrimitive(property, ref value, GuiFactory.FlagsField)
                    : VisitPrimitive(property, ref value, GuiFactory.EnumField);
            }
            return VisitStatus.Unhandled;
        }
    }
}
