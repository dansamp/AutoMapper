using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AutoMapper
{
    public class TypeInfo
    {
        private readonly MemberInfo[] _publicGetters;
        private readonly MemberInfo[] _publicAccessors;
        private readonly MethodInfo[] _publicGetMethods;
        private readonly ConstructorInfo[] _constructors;

        public Type Type { get; private set; }

        public TypeInfo(Type type)
        {
            Type = type;
        	var publicReadableMembers = GetAllPublicReadableMembers();
            var publicWritableMembers = GetAllPublicWritableMembers();
			_publicGetters = BuildPublicReadAccessors(publicReadableMembers);
            _publicAccessors = BuildPublicAccessors(publicWritableMembers);
            _publicGetMethods = BuildPublicNoArgMethods();
            _constructors = type.GetTypeInfo().DeclaredConstructors.Where(ci => !ci.IsStatic).ToArray();
        }

        public IEnumerable<ConstructorInfo> GetConstructors()
        {
            return _constructors;
        }

        public IEnumerable<MemberInfo> GetPublicReadAccessors()
        {
            return _publicGetters;
        }

		public IEnumerable<MemberInfo> GetPublicWriteAccessors()
        {
            return _publicAccessors;
        }

        public IEnumerable<MethodInfo> GetPublicNoArgMethods()
        {
            return _publicGetMethods;
        }

		public IEnumerable<MethodInfo> GetPublicNoArgExtensionMethods(Assembly[] sourceExtensionMethodSearch)
		{
			if (sourceExtensionMethodSearch == null)
			{
				return new MethodInfo[] { };
			}

			//http://stackoverflow.com/questions/299515/c-sharp-reflection-to-identify-extension-methods
			return sourceExtensionMethodSearch
				.SelectMany(assembly => assembly.DefinedTypes)
				.Where(type => type.IsSealed && !type.IsGenericType && !type.IsNested)
                .SelectMany(type => type.DeclaredMethods)
                .Where(method => method.IsStatic)
				.Where(method => method.IsDefined(typeof(ExtensionAttribute), false))
				.Where(method => method.GetParameters()[0].ParameterType == Type);
		}

		private MemberInfo[] BuildPublicReadAccessors(IEnumerable<MemberInfo> allMembers)
        {
			// Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
		    var memberInfos = allMembers as MemberInfo[] ?? allMembers.ToArray();

		    var filteredMembers = memberInfos
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x => x.First())
                .Concat(memberInfos.OfType<FieldInfo>().Cast<MemberInfo>());  // add FieldInfo objects back

            return filteredMembers.ToArray();
        }

        private MemberInfo[] BuildPublicAccessors(IEnumerable<MemberInfo> allMembers)
        {
        	// Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            var memberInfos = allMembers as MemberInfo[] ?? allMembers.ToArray();

            var filteredMembers = memberInfos
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x =>
                    x.Any(y => y.CanWrite && y.CanRead) ? // favor the first property that can both read & write - otherwise pick the first one
						x.First(y => y.CanWrite && y.CanRead) :
                        x.First())
				.Where(pi => pi.CanWrite || pi.PropertyType.IsListOrDictionaryType())
                .Concat(memberInfos.Where(x => x is FieldInfo));  // add FieldInfo objects back

            return filteredMembers.ToArray();
        }

    	private IEnumerable<MemberInfo> GetAllPublicReadableMembers()
    	{
            return GetAllPublicMembers(propertyInfo => propertyInfo.CanRead && propertyInfo.GetMethod.IsPublic);
    	}

        private IEnumerable<MemberInfo> GetAllPublicWritableMembers()
        {
            return GetAllPublicMembers(propertyInfo =>
            {
                bool propertyIsEnumerable = (typeof(string) != propertyInfo.PropertyType)
                                            && propertyInfo.PropertyType.GetTypeInfo().IsSubclassOf(typeof(IEnumerable));

                return (propertyInfo.CanWrite) || (propertyIsEnumerable);
            });
        }

        private IEnumerable<MemberInfo> GetAllPublicMembers(Func<PropertyInfo, bool> propertyAvailableFor)
        {
            Func<Type, IEnumerable<MemberInfo>> memberListBuilder = type =>
            {
                var publicProps = type
                    .GetRuntimeProperties()
                    .Where(p => !p.GetIndexParameters().Any())
                    .Where(propertyAvailableFor);

                var publicFields = type
                    .GetRuntimeFields()
                    .Where(fi => fi.IsPublic);

                return publicFields
                    .Cast<MemberInfo>()
                    .Concat(publicProps);
            };

            return GetAllAssociatedTypes(Type)
                .SelectMany(memberListBuilder);
        }

        private IEnumerable<Type> GetAllAssociatedTypes(Type toScan)
        {
            for (var t = Type; t != null; t = t.BaseType)
                yield return t;

            foreach (var type in toScan.GetInterfaces())
            {
                yield return type;
            }
        }

        private MethodInfo[] BuildPublicNoArgMethods()
        {
            return Type
                .GetRuntimeMethods()
                .Where(m => !m.IsStatic)
                .Where(m => m.ReturnType != typeof (void))
                .Where(m => m.GetParameters().Length == 0)
                .Where(m => m.IsPublic)
                //.Where(m => m.MemberType == MemberTypes.Method)
                .ToArray();
        }
    }
}