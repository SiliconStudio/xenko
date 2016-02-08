// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// Original code from http://nuget.codeplex.com  class SemanticVersion
// Copyright 2010-2014 Outercurve Foundation
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
using System;
using System.Text.RegularExpressions;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A hybrid implementation of SemVer that supports semantic versioning as described at http://semver.org while not strictly enforcing it to 
    /// allow older 4-digit versioning schemes to continue working.
    /// </summary>
    [DataContract("PackageVersion")]
    [NonIdentifiable]
    [DataSerializer(typeof(PackageVersionDataSerializer))]
    public sealed class PackageVersion : IComparable, IComparable<PackageVersion>, IEquatable<PackageVersion>
    {
        private const RegexOptions Flags = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
        private static readonly Regex SemanticVersionRegex = new Regex(@"^(?<Version>\d+(\s*\.\s*\d+){0,3})(?<Release>-[a-z][0-9a-z-]*)?$", Flags);
        private static readonly Regex StrictSemanticVersionRegex = new Regex(@"^(?<Version>\d+(\.\d+){2})(?<Release>-[a-z][0-9a-z-]*)?$", Flags);
        private readonly string originalString;

        /// <summary>
        /// Defines version 0.
        /// </summary>
        public static readonly PackageVersion Zero = Parse("0");

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageVersion"/> class.
        /// </summary>
        /// <param name="version">The version.</param>
        public PackageVersion(string version)
            : this(Parse(version))
        {
            // The constructor normalizes the version string so that it we do not need to normalize it every time we need to operate on it. 
            // The original string represents the original form in which the version is represented to be used when printing.
            originalString = version;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageVersion"/> class.
        /// </summary>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        /// <param name="build">The build.</param>
        /// <param name="revision">The revision.</param>
        public PackageVersion(int major, int minor, int build, int revision)
            : this(new Version(major, minor, build, revision))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageVersion"/> class.
        /// </summary>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        /// <param name="build">The build.</param>
        /// <param name="specialVersion">The special version.</param>
        public PackageVersion(int major, int minor, int build, string specialVersion)
            : this(new Version(major, minor, build), specialVersion)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageVersion"/> class.
        /// </summary>
        /// <param name="version">The version.</param>
        public PackageVersion(Version version)
            : this(version, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageVersion"/> class.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="specialVersion">The special version.</param>
        public PackageVersion(Version version, string specialVersion)
            : this(version, specialVersion, null)
        {
        }

        private PackageVersion(Version version, string specialVersion, string originalString)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            Version = NormalizeVersionValue(version);
            SpecialVersion = specialVersion ?? String.Empty;
            this.originalString = String.IsNullOrEmpty(originalString) ? version.ToString() + (!String.IsNullOrEmpty(specialVersion) ? '-' + specialVersion : null) : originalString;
        }

        internal PackageVersion(PackageVersion semVer)
        {
            originalString = semVer.ToString();
            Version = semVer.Version;
            SpecialVersion = semVer.SpecialVersion;
        }

        /// <summary>
        /// Gets the normalized version portion.
        /// </summary>
        public Version Version { get; private set; }

        /// <summary>
        /// Gets the optional special version.
        /// </summary>
        public string SpecialVersion { get; private set; }

        public string[] GetOriginalVersionComponents()
        {
            if (!String.IsNullOrEmpty(originalString))
            {
                string original;

                // search the start of the SpecialVersion part, if any
                int dashIndex = originalString.IndexOf('-');
                if (dashIndex != -1)
                {
                    // remove the SpecialVersion part
                    original = originalString.Substring(0, dashIndex);
                }
                else
                {
                    original = originalString;
                }

                return SplitAndPadVersionString(original);
            }
            else
            {
                return SplitAndPadVersionString(Version.ToString());
            }
        }

        private static string[] SplitAndPadVersionString(string version)
        {
            string[] a = version.Split('.');
            if (a.Length == 4)
            {
                return a;
            }
            else
            {
                // if 'a' has less than 4 elements, we pad the '0' at the end 
                // to make it 4.
                var b = new string[4] { "0", "0", "0", "0" };
                Array.Copy(a, 0, b, 0, a.Length);
                return b;
            }
        }

        /// <summary>
        /// Parses a version string using loose semantic versioning rules that allows 2-4 version components followed by an optional special version.
        /// </summary>
        public static PackageVersion Parse(string version)
        {
            if (String.IsNullOrEmpty(version))
            {
                throw new ArgumentNullException("version", "cannot be null or empty");
            }

            PackageVersion semVer;
            if (!TryParse(version, out semVer))
            {
                throw new ArgumentException(String.Format("Invalid version format [{0}]", version), "version");
            }
            return semVer;
        }

        /// <summary>
        /// Parses a version string using loose semantic versioning rules that allows 2-4 version components followed by an optional special version.
        /// </summary>
        public static bool TryParse(string version, out PackageVersion value)
        {
            return TryParseInternal(version, SemanticVersionRegex, out value);
        }

        /// <summary>
        /// Parses a version string using strict semantic versioning rules that allows exactly 3 components and an optional special version.
        /// </summary>
        public static bool TryParseStrict(string version, out PackageVersion value)
        {
            return TryParseInternal(version, StrictSemanticVersionRegex, out value);
        }

        private static bool TryParseInternal(string version, Regex regex, out PackageVersion semVer)
        {
            semVer = null;
            if (String.IsNullOrEmpty(version))
            {
                return false;
            }

            var match = regex.Match(version.Trim());
            Version versionValue;
            if (!match.Success || !Version.TryParse(match.Groups["Version"].Value, out versionValue))
            {
                // Support integer version numbers (i.e. 1 -> 1.0)
                int versionNumber;
                if (Int32.TryParse(version, out versionNumber))
                {
                    semVer = new PackageVersion(new Version(versionNumber, 0));
                    return true;
                }

                return false;
            }

            semVer = new PackageVersion(NormalizeVersionValue(versionValue), match.Groups["Release"].Value.TrimStart('-'), version.Replace(" ", ""));
            return true;
        }

        /// <summary>
        /// Attempts to parse the version token as a SemanticVersion.
        /// </summary>
        /// <returns>An instance of SemanticVersion if it parses correctly, null otherwise.</returns>
        public static PackageVersion ParseOptionalVersion(string version)
        {
            PackageVersion semVer;
            TryParse(version, out semVer);
            return semVer;
        }

        private static Version NormalizeVersionValue(Version version)
        {
            return new Version(version.Major,
                version.Minor,
                Math.Max(version.Build, 0),
                Math.Max(version.Revision, 0));
        }

        public int CompareTo(object obj)
        {
            if (Object.ReferenceEquals(obj, null))
            {
                return 1;
            }
            PackageVersion other = obj as PackageVersion;
            if (other == null)
            {
                throw new ArgumentException("Object must be a SemanticVersion", "obj");
            }
            return CompareTo(other);
        }

        public int CompareTo(PackageVersion other)
        {
            if (Object.ReferenceEquals(other, null))
            {
                return 1;
            }

            int result = Version.CompareTo(other.Version);

            if (result != 0)
            {
                return result;
            }

            bool empty = String.IsNullOrEmpty(SpecialVersion);
            bool otherEmpty = String.IsNullOrEmpty(other.SpecialVersion);
            if (empty && otherEmpty)
            {
                return 0;
            }
            else if (empty)
            {
                return 1;
            }
            else if (otherEmpty)
            {
                return -1;
            }
            return StringComparer.OrdinalIgnoreCase.Compare(SpecialVersion, other.SpecialVersion);
        }

        public static bool operator ==(PackageVersion version1, PackageVersion version2)
        {
            if (Object.ReferenceEquals(version1, null))
            {
                return Object.ReferenceEquals(version2, null);
            }
            return version1.Equals(version2);
        }

        public static bool operator !=(PackageVersion version1, PackageVersion version2)
        {
            return !(version1 == version2);
        }

        public static bool operator <(PackageVersion version1, PackageVersion version2)
        {
            if (version1 == null)
            {
                throw new ArgumentNullException("version1");
            }
            return version1.CompareTo(version2) < 0;
        }

        public static bool operator <=(PackageVersion version1, PackageVersion version2)
        {
            return (version1 == version2) || (version1 < version2);
        }

        public static bool operator >(PackageVersion version1, PackageVersion version2)
        {
            if (version1 == null)
            {
                throw new ArgumentNullException("version1");
            }
            return version2 < version1;
        }

        public static bool operator >=(PackageVersion version1, PackageVersion version2)
        {
            return (version1 == version2) || (version1 > version2);
        }

        public override string ToString()
        {
            return originalString;
        }

        public bool Equals(PackageVersion other)
        {
            return !Object.ReferenceEquals(null, other) &&
                   Version.Equals(other.Version) &&
                   SpecialVersion.Equals(other.SpecialVersion, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            PackageVersion semVer = obj as PackageVersion;
            return !Object.ReferenceEquals(null, semVer) && Equals(semVer);
        }

        public override int GetHashCode()
        {
            int hashCode = Version.GetHashCode();
            if (SpecialVersion != null)
            {
                hashCode = hashCode*4567 + SpecialVersion.GetHashCode();
            }

            return hashCode;
        }

        internal class PackageVersionDataSerializer : DataSerializer<PackageVersion>
        {
            /// <inheritdoc/>
            public override bool IsBlittable { get { return true; } }

            /// <inheritdoc/>
            public override void Serialize(ref PackageVersion obj, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Deserialize)
                {
                    string version = null;
                    stream.Serialize(ref version);
                    obj = PackageVersion.Parse(version);
                }
                else
                {
                    string version = obj.ToString();
                    stream.Serialize(ref version);
                }
            }
        }

        internal static PackageVersion FromSemanticVersion(NuGet.SemanticVersion semanticVersion)
        {
            if (semanticVersion == null)
            {
                return null;
            }
            return new PackageVersion(semanticVersion.ToString());
        }
    }
}
