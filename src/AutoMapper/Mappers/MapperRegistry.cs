using System;
using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    public static class MapperRegistry
    {
        public static Func<IEnumerable<IObjectMapper>> AllMappers = () => new IObjectMapper[]
        {
#if !(SILVERLIGHT || NETFX_CORE)
            new DataReaderMapper(),
#endif
            new TypeMapMapper(TypeMapObjectMapperRegistry.AllMappers()),
            new StringMapper(),
            new FlagsEnumMapper(),
            new EnumMapper(),
            new ArrayMapper(),
			new EnumerableToDictionaryMapper(),
#if !(SILVERLIGHT || NETFX_CORE)
            new NameValueCollectionMapper(), 
#endif
            new DictionaryMapper(),
#if !(SILVERLIGHT || NETFX_CORE)
            new ListSourceMapper(),
#endif
            new ReadOnlyCollectionMapper(), 
            new CollectionMapper(),
            new EnumerableMapper(),
            new AssignableMapper(),
#if !(SILVERLIGHT || NETFX_CORE)
            new TypeConverterMapper(),
#endif
            new NullableSourceMapper(),
            new NullableMapper(),
            new ImplicitConversionOperatorMapper(),
            new ExplicitConversionOperatorMapper(),
        };
    }
}