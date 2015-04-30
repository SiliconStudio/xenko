// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Design;

namespace SiliconStudio.Paradox.Assets.Serializers
{
    /// <summary>
    /// Error resistant Script loading. It should work even if there is missing properties, members or types.
    /// If main script type is missing (usually due to broken assemblies), it will keep the Yaml representation so that it can be properly saved alter.
    /// </summary>
    [YamlSerializerFactory]
    internal class ScriptCollectionSerializer : CollectionSerializer
    {
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            var type = typeDescriptor.Type;
            return type == typeof(ScriptCollection) ? this : null;
        }

        /// <inheritdoc/>
        protected override object ReadCollectionItem(ref ObjectContext objectContext, Type itemType)
        {
            // Save the Yaml stream, in case loading fails we can keep this representation
            var parsingEvents = new List<ParsingEvent>();
            var reader = objectContext.Reader;
            var startDepth = reader.CurrentDepth;
            do
            {
                parsingEvents.Add(reader.Expect<ParsingEvent>());
            } while (reader.CurrentDepth > startDepth);

            // Save states
            var previousReader = objectContext.SerializerContext.Reader;
            var previousAllowErrors = objectContext.SerializerContext.AllowErrors;

            objectContext.SerializerContext.Reader = new EventReader(new MemoryParser(parsingEvents));
            objectContext.SerializerContext.AllowErrors = true;

            try
            {
                return objectContext.ObjectSerializerBackend.ReadCollectionItem(ref objectContext, itemType);
            }
            catch (YamlException)
            {
                // There was a failure, let's keep this object so that it can be serialized back later
                return new UnloadableScript(parsingEvents);
            }
            finally
            {
                // Restore states
                objectContext.SerializerContext.Reader = previousReader;
                objectContext.SerializerContext.AllowErrors = previousAllowErrors;
            }
        }

        /// <inheritdoc/>
        protected override void WriteCollectionItem(ref ObjectContext objectContext, object item, Type itemType)
        {
            // Check if we have a Yaml representation (in case loading failed)
            var unloadableScript = item as UnloadableScript;
            if (unloadableScript != null)
            {
                var writer = objectContext.Writer;
                foreach (var parsingEvent in unloadableScript.ParsingEvents)
                {
                    writer.Emit(parsingEvent);
                }
                return;
            }

            base.WriteCollectionItem(ref objectContext, item, itemType);
        }
    }
}