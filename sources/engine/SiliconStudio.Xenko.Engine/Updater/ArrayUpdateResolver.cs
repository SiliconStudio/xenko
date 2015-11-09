using System;
using System.Globalization;

namespace SiliconStudio.Xenko.Updater
{
    public class ArrayUpdateResolver<T> : UpdateMemberResolver
    {
        public override Type SupportedType => typeof(T[]);

        public override UpdatableMember ResolveIndexer(string indexerName)
        {
            // Transform index into integer
            int indexerValue;
            if (!int.TryParse(indexerName, NumberStyles.Any, CultureInfo.InvariantCulture, out indexerValue))
                throw new InvalidOperationException(string.Format("Property path parse error: could not parse indexer value '{0}'", indexerName));

            var updatableField = new UpdatableField<T>(0);

            var offset = UpdateEngineHelper.ArrayFirstElementOffset + indexerValue * updatableField.Size;
            updatableField.Offset = offset;

            return updatableField;
        }
    }
}