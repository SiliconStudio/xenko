using SharpYaml.Serialization;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Core.Settings
{
    [YamlSerializerFactory]
    internal class SettingsProfileSerializer : SettingsDictionarySerializer
    {
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            var type = typeDescriptor.Type;
            return type == typeof(SettingsProfile) ? this : null;
        }

        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            var settingsProfile = (SettingsProfile)objectContext.Instance;
            var settingsDictionary = new SettingsDictionary { Profile = settingsProfile };

            if (objectContext.SerializerContext.IsSerializing)
            {
                settingsProfile.Container.EncodeSettings(settingsProfile, settingsDictionary);
            }

            objectContext.Instance = settingsDictionary;

            base.CreateOrTransformObject(ref objectContext);
        }

        protected override void TransformObjectAfterRead(ref ObjectContext objectContext)
        {
            if (!objectContext.SerializerContext.IsSerializing)
            {
                var settingsDictionary = (SettingsDictionary)objectContext.Instance;
                var settingsProfile = settingsDictionary.Profile;

                settingsProfile.Container.DecodeSettings(settingsDictionary, settingsProfile);

                objectContext.Instance = settingsProfile;
            }
        }
    }
}