// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;


namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Describes a virtual button (a key from a keyboard, a mouse button, an axis of a joystick...etc.).
    /// </summary>
    public abstract partial class VirtualButton : IVirtualButton
    {
        private static readonly Dictionary<int, VirtualButton> mapIp = new Dictionary<int, VirtualButton>();
        private static readonly Dictionary<string, VirtualButton> mapName = new Dictionary<string, VirtualButton>();
        private static readonly List<VirtualButton> registered = new List<VirtualButton>();
        private static IReadOnlyCollection<VirtualButton> registeredReadOnly;
        internal const int TypeIdMask = 0x0FFFFFFF;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualButton" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="id">The id.</param>
        /// <param name="isPositiveAndNegative">if set to <c>true</c> [is positive and negative].</param>
        private VirtualButton(string name, VirtualButtonType type, int id, bool isPositiveAndNegative = false)
        {
            Id = (int)type | id;
            Name = name;
            Type = type;
            IsPositiveAndNegative = isPositiveAndNegative;
        }

        /// <summary>
        /// Unique Id for a particular button <see cref="Type"/>.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Name of this button.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Type of this button.
        /// </summary>
        public readonly VirtualButtonType Type;

        /// <summary>
        /// A boolean indicating whether this button supports positive and negative value.
        /// </summary>
        public readonly bool IsPositiveAndNegative;

        public override string ToString()
        {
            return string.Format("{0}", Name);
        }

        /// <summary>
        /// Implements the + operator to combine to <see cref="VirtualButton"/>.
        /// </summary>
        /// <param name="left">The left virtual button.</param>
        /// <param name="right">The right virtual button.</param>
        /// <returns>A set containting the two virtual buttons.</returns>
        public static IVirtualButton operator +(IVirtualButton left, VirtualButton right)
        {
            if (left == null)
            {
                return right;
            }

            return right == null ? left : new VirtualButtonGroup { left, right };
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="VirtualButton" /> to <see cref="VirtualButtonAnd" />.
        /// </summary>
        /// <param name="button">The virtual button.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator VirtualButtonGroup(VirtualButton button)
        {
            return new VirtualButtonGroup() { button };
        }

        /// <summary>
        /// Gets all registered <see cref="VirtualButton"/>.
        /// </summary>
        /// <value>The registered virtual buttons.</value>
        public static IReadOnlyCollection<VirtualButton> Registered
        {
            get
            {
                EnsureInitialize();
                return registeredReadOnly;
            }
        }

        /// <summary>
        /// Finds a virtual button by the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An instance of VirtualButton or null if no match.</returns>
        public static VirtualButton Find(string name)
        {
            VirtualButton virtualButton;
            EnsureInitialize();
            mapName.TryGetValue(name, out virtualButton);
            return virtualButton;
        }

        /// <summary>
        /// Finds a virtual button by the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>An instance of VirtualButton or null if no match.</returns>
        public static VirtualButton Find(int id)
        {
            VirtualButton virtualButton;
            EnsureInitialize();
            mapIp.TryGetValue(id, out virtualButton);
            return virtualButton;
        }

        public abstract float GetValue(InputManager manager);
 
        private static void EnsureInitialize()
        {
            lock (mapIp)
            {
                if (mapIp.Count == 0)
                {
                    RegisterFromType(typeof(Keyboard));
                    RegisterFromType(typeof(GamePad));
                    registeredReadOnly = registered;
                }
            }
        }

        internal static float ClampValue(float value)
        {
            return MathUtil.Clamp(value, -1.0f, 1.0f);
        }

        private static void RegisterFromType(Type type)
        {
            foreach (var fieldInfo in type.GetTypeInfo().DeclaredFields)
            {
                if (fieldInfo.IsStatic && fieldInfo.FieldType == typeof(VirtualButton))
                {
                    Register((VirtualButton)fieldInfo.GetValue(null));
                }
            }
        }

        private static void Register(VirtualButton virtualButton)
        {

            if (!mapIp.ContainsKey(virtualButton.Id))
            {
                mapIp.Add(virtualButton.Id, virtualButton);
                registered.Add(virtualButton);
            }

            if (!mapName.ContainsKey(virtualButton.Name))
            {
                mapName.Add(virtualButton.Name, virtualButton);
            }
        }
    }
}
