using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Tracking
{
    public class SourceFilesCollector : AssetVisitorBase
    {
        private Dictionary<UFile, bool> sourceFiles;
        private Dictionary<MemberPath, UFile> sourceMembers;

        public Dictionary<UFile, bool> GetSourceFiles(Asset asset)
        {
            sourceFiles = new Dictionary<UFile, bool>();
            Visit(asset);
            var result = sourceFiles;
            sourceFiles = null;
            return result;
        }

        public Dictionary<MemberPath, UFile> GetSourceMembers(Asset asset)
        {
            sourceMembers = new Dictionary<MemberPath, UFile>();
            Visit(asset);
            var result = sourceMembers;
            sourceMembers = null;
            return result;
        }

        public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
        {
            // Don't visit base parts as they are visited at the top level.
            if (typeof(Asset).IsAssignableFrom(member.DeclaringType) && (member.Name == Asset.BasePartsProperty))
            {
                return;
            }

            if (sourceFiles != null)
            {
                if (member.Type == typeof(UFile) && value != null)
                {
                    var file = (UFile)value;
                    if (!string.IsNullOrWhiteSpace(file.ToString()))
                    {
                        var attribute = member.GetCustomAttributes<SourceFileMemberAttribute>(true).SingleOrDefault();
                        if (attribute != null)
                        {
                            if (!sourceFiles.ContainsKey(file))
                            {
                                sourceFiles.Add(file, attribute.UpdateAssetIfChanged);
                            }
                            else if (attribute.UpdateAssetIfChanged)
                            {
                                // If the file has already been collected, just update whether it should update the asset when changed
                                sourceFiles[file] = true;
                            }
                        }
                    }
                }
            }
            if (sourceMembers != null)
            {
                if (member.Type == typeof(UFile))
                {
                    var attribute = member.GetCustomAttributes<SourceFileMemberAttribute>(true).SingleOrDefault();
                    if (attribute != null)
                    {
                        sourceMembers[CurrentPath.Clone()] = value as UFile;
                    }
                }
            }
            base.VisitObjectMember(container, containerDescriptor, member, value);
        }
    }
}
