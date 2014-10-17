using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SiliconStudio.Quantum;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Quantum.Legacy;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.BuildEngine.Editor.View
{
    public class PropertyGridItemTemplateSelector : DataTemplateSelector
    {
        private static DataTemplate errorTemplate;

        private static DataTemplate viewModelTemplate;
        private static DataTemplate textTemplate;
        private static DataTemplate vectorTemplate;

        private static readonly DataTemplate EmptyTemplate;

        static PropertyGridItemTemplateSelector()
        {
            EmptyTemplate = new DataTemplate();
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DataTemplate template = null;

            var element = container as FrameworkElement;
            if (element == null)
                throw new Exception("Container must be of type FrameworkElement");


            if (item is ViewModelReference)
            {
                return (DataTemplate)element.FindResource("ViewModelItemReferenceLink");
            }

            var content = item as IContent;
            if (content != null)
            {
                if (content.OwnerNode != null && content.OwnerNode.Parent != null)
                {
                    if (content.OwnerNode.Content.Type.IsGenericType && content.OwnerNode.Content.Type.GetGenericTypeDefinition() == typeof(IList<>))
                        return (DataTemplate)element.FindResource("ListItem");
                }
            }

            var node = item as IViewModelNode;
            if (node != null)
            {
                var viewModel = node;

                if (node.Content != null && (node.Content.Flags & ViewModelContentFlags.HiddenContent) != 0)
                {
                    template = EmptyTemplate;
                }
                else if (viewModel.Content.Type == typeof(Image))
                {
                    template = (DataTemplate)element.FindResource("TextureParameter");
                }
                else if (viewModel.Content.Type == typeof(Vector2)
                    || viewModel.Content.Type == typeof(Vector3)
                    || viewModel.Content.Type == typeof(Vector4)
                    || viewModel.Content.Type == typeof(Matrix))
                {
                    return vectorTemplate ?? (vectorTemplate = (DataTemplate)element.FindResource("VectorParameter"));
                }
                else if (viewModel.Content.Type == typeof(Color3))
                {
                    template = (DataTemplate)element.FindResource("Color3View");
                }
                else if (viewModel.Content.Type.IsEnum)
                {
                    template = (DataTemplate)element.FindResource("EnumView");
                }
                else if (Nullable.GetUnderlyingType(viewModel.Content.Type) != null && Nullable.GetUnderlyingType(viewModel.Content.Type).IsEnum)
                {
                    template = (DataTemplate)element.FindResource("NullableEnumView");
                }
                //else if (viewModel.Type == typeof(IList<ViewModelReference>))
                //{
                //    template = (DataTemplate)element.FindResource("ListViewModelReference");
                //}
                else if (viewModel.Content.Type == typeof(IList<IContent>))
                {
                    template = (DataTemplate)element.FindResource("ListViewModel");
                }
                else if (viewModel.Children.Count > 0 && !viewModel.Children.Any(x => x.Content.Value is ICommand))
                {
                    if (viewModelTemplate == null)
                        viewModelTemplate = (DataTemplate)element.FindResource("IViewModelNode");
                    template = viewModelTemplate;
                }
                else if (viewModel.Content.Type == typeof(ViewModelReference))
                {
                    template = (DataTemplate)element.FindResource("ViewModelReferenceLink");
                }
                else if (viewModel.Content.Type == typeof(bool))
                {
                    template = (DataTemplate)element.FindResource("BooleanView");
                }
                else if (viewModel.Content.Type == typeof(string) && !viewModel.Content.IsReadOnly)
                {
                    template = (DataTemplate)element.FindResource("DroppableTextBox");
                }
                else if (viewModel.Content.IsReadOnly)
                {
                    template = (DataTemplate)element.FindResource("ReadOnlyTextBox");
                }
                else
                {
                    if (textTemplate == null)
                        textTemplate = (DataTemplate)element.FindResource("TextBox");
                    template = textTemplate;
                }
            }

            if (template == null)
            {
                if (errorTemplate == null)
                    errorTemplate = (DataTemplate)element.FindResource("Error");
                template = errorTemplate;
            }

            return template;
        }
    }
}