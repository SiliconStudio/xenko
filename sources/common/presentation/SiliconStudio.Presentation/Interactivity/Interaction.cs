// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows;

namespace SiliconStudio.Presentation.Interactivity
{
    /// <summary>
    /// A container for an attached dependency property that contains a collection of behavior.
    /// The purpose of this class is to be used in place of System.Windows.Interactivity.Interaction.
    /// This class allows to set behaviors in styles because <see cref="SiliconStudio.Presentation.Interactivity.BehaviorCollection"/>
    /// has a public parameterless constructor and the Behaviors attached property has a public setter.
    /// When the collection is modified or set, it automatically synchronize the attached property
    /// System.Windows.Interactivity.Interaction.Behaviors.
    /// </summary>
    public static class Interaction
    {
        public static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached("Behaviors", typeof(BehaviorCollection), typeof(Interaction), new PropertyMetadata(new BehaviorCollection(), OnBehaviorCollectionChanged));

        public static BehaviorCollection GetBehaviors(DependencyObject obj)
        {
            return (BehaviorCollection)obj.GetValue(BehaviorsProperty);
        }

        public static void SetBehaviors(DependencyObject obj, BehaviorCollection value)
        {
            obj.SetValue(BehaviorsProperty, value);
        }

        private static void OnBehaviorCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (BehaviorCollection)e.OldValue;
            oldValue?.Detach();

            var newValue = (BehaviorCollection)e.NewValue;
            if (newValue != null)
            {
                if (newValue.AssociatedObject != null)
                    newValue = newValue.Clone();

                newValue.Attach(d);
            }
        }
    }
}
