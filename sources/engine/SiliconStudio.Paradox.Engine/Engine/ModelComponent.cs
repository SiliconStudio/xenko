// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Add a <see cref="Model"/> to an <see cref="Entity"/>, that will be used during rendering.
    /// </summary>
    [DataConverter(AutoGenerate = true)]
    [DataContract("ModelComponent")]
    public sealed class ModelComponent : EntityComponent, IModelInstance
    {
        public static PropertyKey<ModelComponent> Key = new PropertyKey<ModelComponent>("Key", typeof(ModelComponent));

        private Model model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelComponent"/> class.
        /// </summary>
        public ModelComponent() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelComponent"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public ModelComponent(Model model)
        {
            Enabled = true;
            Parameters = new ParameterCollection();
            Model = model;
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        [DataMemberConvert]
        [DataMemberCustomSerializer]
        public Model Model
        {
            get
            {
                return model;
            }
            set
            {
                model = value;
                ModelUpdated();
            }
        }

        [DataMemberIgnore]
        public ModelViewHierarchyUpdater ModelViewHierarchy
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether rendering is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if rendering is enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMemberConvert]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the draw order (from lowest to highest).
        /// </summary>
        /// <value>
        /// The draw order.
        /// </value>
        [DataMemberConvert]
        public int DrawOrder { get; set; }

        /// <summary>
        /// Gets the parameters used to render this mesh.
        /// </summary>
        /// <value>The parameters.</value>
        [DataMemberConvert]
        public ParameterCollection Parameters { get; private set; }

        private void ModelUpdated()
        {
            if (model != null)
            {
                if (ModelViewHierarchy != null)
                {
                    // Reuse previous ModelViewHierarchy
                    ModelViewHierarchy.Initialize(model);
                }
                else
                {
                    ModelViewHierarchy = new ModelViewHierarchyUpdater(model);
                }
            }
        }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}