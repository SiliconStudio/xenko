// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows.Markup;
using System.Xaml;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    [ContentProperty("Name")]
    [Obsolete]
    public class Reference : MarkupExtension
    {
        public Reference()
        {
        }

        public Reference(string name)
        {
            Name = name;
        }

        [ConstructorArgument("name")]
        public string Name { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            // the bellow method do not work when no Application has been created
            /*
            object[] references = (from window in Application.Current.Windows.Cast<Window>()
                                             let name = window.FindName(Name)
                                             where name != null
                                             select name)
                                             .ToArray();

            if (references.Length == 0)
                throw new InvalidOperationException(string.Format("Impossible to find reference for name '{0}'", Name));

            if (references.Length > 1)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine(string.Format("There are {0} references for name '{1}'.", references.Length, Name));
                foreach (object reference in references)
                    message.AppendLine(string.Format("  reference: '{0}' ({1})", reference, reference.GetType().FullName));
                throw new InvalidOperationException(message.ToString());
            }

            return references[0];
            */

            IXamlNameResolver service = serviceProvider.GetService(typeof(IXamlNameResolver)) as IXamlNameResolver;
            if (service == null)
                return null; // happens in design mode

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Reference markup extension must be provided a name.");

            object fixupToken = service.Resolve(Name);

            if (fixupToken == null)
                fixupToken = service.GetFixupToken(new [] { Name }, true);

            return fixupToken;
        }
    }
}
