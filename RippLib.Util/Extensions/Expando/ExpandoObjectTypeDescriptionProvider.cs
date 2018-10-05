using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace RippLib.Util.Extensions
{
    public class ExpandoObjectTypeDescriptor : ICustomTypeDescriptor
    {
        private readonly IDictionary<string, object> _instance;

        public ExpandoObjectTypeDescriptor(dynamic instance)
        {
            _instance = instance as IDictionary<string, object>;
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _instance;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(
                _instance.Keys
                          .Select(x => new ExpandoObjectPropertyDescriptor(_instance, x))
                          .ToArray<PropertyDescriptor>());
        }

        class ExpandoObjectPropertyDescriptor : PropertyDescriptor
        {
            private readonly IDictionary<string, object> _instance;
            private readonly string _name;

            public ExpandoObjectPropertyDescriptor(IDictionary<string, object> instance, string name)
                : base(name, null)
            {
                _instance = instance;
                _name = name;
            }

            public override Type PropertyType => _instance[_name].GetType();

            public override void SetValue(object component, object value)
            {
                _instance[_name] = value;
            }

            public override object GetValue(object component)
            {
                return _instance[_name];
            }

            public override bool IsReadOnly => false;

            public override Type ComponentType => null;
            

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override void ResetValue(object component)
            {
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override string Category => string.Empty;

            public override string Description => string.Empty;
        }
    }
    public class ExpandoObjectTypeDescriptionProvider : TypeDescriptionProvider
    {
        private static readonly TypeDescriptionProvider _default = TypeDescriptor.GetProvider(typeof(ExpandoObject));

        public ExpandoObjectTypeDescriptionProvider()
            : base(_default)
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            var defaultDescriptor = base.GetTypeDescriptor(objectType, instance);

            return instance == null ? defaultDescriptor :
                new ExpandoObjectTypeDescriptor(instance);
        }
    }
}
