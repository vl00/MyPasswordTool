using System;
using System.Windows;
using System.Windows.Markup;

namespace SilverEx.Xaml
{
    public class TypeExtension : MarkupExtension
    {
        public TypeExtension() { }

        public TypeExtension(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }
        public string TypeName { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Type == null)
            {
                if (string.IsNullOrWhiteSpace(TypeName)) 
                    throw new InvalidOperationException("No TypeName or Type specified.");
                if (serviceProvider == null) return DependencyProperty.UnsetValue;

                var resolver = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
                if (resolver == null) return DependencyProperty.UnsetValue;

                try
                {
                    Type = resolver.Resolve(TypeName); //error,by Resolve some types in mscorlib.dll in sl
                }
                catch
                {
                    Type = Type.GetType(TypeName, true);
                }
            }
            return Type;
        }
    }
}
