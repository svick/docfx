// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.EntityModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Newtonsoft.Json;
    using YamlDotNet.Serialization;

    using Microsoft.DocAsCode.Utility.EntityMergers;
    using Microsoft.DocAsCode.YamlSerialization;

    [Serializable]
    public class ItemViewModel : IOverwriteDocumentViewModel
    {
        [YamlMember(Alias = Constants.PropertyName.Uid)]
        [JsonProperty(Constants.PropertyName.Uid)]
        [MergeOption(MergeOption.MergeKey)]
        public string Uid { get; set; }

        [YamlMember(Alias = Constants.PropertyName.Id)]
        [JsonProperty(Constants.PropertyName.Id)]
        public string Id { get; set; }

        [YamlMember(Alias = "parent")]
        [JsonProperty("parent")]
        public string Parent { get; set; }

        [YamlMember(Alias = "children")]
        [MergeOption(MergeOption.Ignore)] // todo : merge more children
        [JsonProperty("children")]
        public List<string> Children { get; set; }

        [YamlMember(Alias = Constants.PropertyName.Href)]
        [JsonProperty(Constants.PropertyName.Href)]
        public string Href { get; set; }

        [YamlMember(Alias = "langs")]
        [JsonProperty("langs")]
        public string[] SupportedLanguages { get; set; } = new string[] { "csharp", "vb" };

        [YamlMember(Alias = "name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [ExtensibleMember("name.")]
        [JsonIgnore]
        public SortedList<string, string> Names { get; set; } = new SortedList<string, string>();

        [YamlIgnore]
        [JsonIgnore]
        public string NameForCSharp
        {
            get
            {
                string result;
                Names.TryGetValue("csharp", out result);
                return result;
            }
            set
            {
                if (value == null)
                {
                    Names.Remove("csharp");
                }
                else
                {
                    Names["csharp"] = value;
                }
            }
        }

        [YamlIgnore]
        [JsonIgnore]
        public string NameForVB
        {
            get
            {
                string result;
                Names.TryGetValue("vb", out result);
                return result;
            }
            set
            {
                if (value == null)
                {
                    Names.Remove("vb");
                }
                else
                {
                    Names["vb"] = value;
                }
            }
        }

        [YamlMember(Alias = "fullName")]
        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [ExtensibleMember("fullName.")]
        [JsonIgnore]
        public SortedList<string, string> FullNames { get; set; } = new SortedList<string, string>();

        [YamlIgnore]
        [JsonIgnore]
        public string FullNameForCSharp
        {
            get
            {
                string result;
                FullNames.TryGetValue("csharp", out result);
                return result;
            }
            set
            {
                if (value == null)
                {
                    FullNames.Remove("csharp");
                }
                else
                {
                    FullNames["csharp"] = value;
                }
            }
        }

        [YamlIgnore]
        [JsonIgnore]
        public string FullNameForVB
        {
            get
            {
                string result;
                FullNames.TryGetValue("vb", out result);
                return result;
            }
            set
            {
                if (value == null)
                {
                    FullNames.Remove("vb");
                }
                else
                {
                    FullNames["vb"] = value;
                }
            }
        }

        [YamlMember(Alias = Constants.PropertyName.Type)]
        [JsonProperty(Constants.PropertyName.Type)]
        public MemberType? Type { get; set; }

        [YamlMember(Alias = Constants.PropertyName.Source)]
        [JsonProperty(Constants.PropertyName.Source)]
        public SourceDetail Source { get; set; }

        [YamlMember(Alias = Constants.PropertyName.Documentation)]
        [JsonProperty(Constants.PropertyName.Documentation)]
        public SourceDetail Documentation { get; set; }

        [YamlMember(Alias = "assemblies")]
        [MergeOption(MergeOption.Ignore)] // todo : merge more children
        [JsonProperty("assemblies")]
        public List<string> AssemblyNameList { get; set; }

        [YamlMember(Alias = "namespace")]
        [JsonProperty("namespace")]
        public string NamespaceName { get; set; }

        [YamlMember(Alias = "summary")]
        [JsonProperty("summary")]
        public string Summary { get; set; }

        [YamlMember(Alias = "remarks")]
        [JsonProperty("remarks")]
        public string Remarks { get; set; }

        [YamlMember(Alias = "example")]
        [JsonProperty("example")]
        public List<string> Examples { get; set; }

        [YamlMember(Alias = "syntax")]
        [JsonProperty("syntax")]
        public SyntaxDetailViewModel Syntax { get; set; }

        [YamlMember(Alias = "overridden")]
        [JsonProperty("overridden")]
        public string Overridden { get; set; }

        [YamlMember(Alias = "exceptions")]
        [JsonProperty("exceptions")]
        public List<CrefInfo> Exceptions { get; set; }

        [YamlMember(Alias = "seealso")]
        [JsonProperty("seealso")]
        public List<CrefInfo> SeeAlsos { get; set; }

        [YamlMember(Alias = "see")]
        [JsonProperty("see")]
        public List<CrefInfo> Sees { get; set; }

        [YamlMember(Alias = "inheritance")]
        [MergeOption(MergeOption.Ignore)]
        [JsonProperty("inheritance")]
        public List<string> Inheritance { get; set; }

        [YamlMember(Alias = "implements")]
        [MergeOption(MergeOption.Ignore)] // todo : merge more children
        [JsonProperty("implements")]
        public List<string> Implements { get; set; }

        [YamlMember(Alias = "inheritedMembers")]
        [MergeOption(MergeOption.Ignore)] // todo : merge more children
        [JsonProperty("inheritedMembers")]
        public List<string> InheritedMembers { get; set; }

        [ExtensibleMember("modifiers.")]
        [MergeOption(MergeOption.Ignore)] // todo : merge more children
        [JsonIgnore]
        public SortedList<string, List<string>> Modifiers { get; set; } = new SortedList<string, List<string>>();

        [YamlMember(Alias = Constants.PropertyName.Conceptual)]
        [JsonProperty(Constants.PropertyName.Conceptual)]
        public string Conceptual { get; set; }

        [YamlMember(Alias = "platform")]
        [JsonProperty("platform")]
        public List<string> Platform { get; set; }

        [ExtensibleMember]
        [JsonIgnore]
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [YamlIgnore]
        [JsonExtensionData(WriteData = true, ReadData = false)]
        public Dictionary<string, object> ExtensionData
        {
            get
            {
                var result = new Dictionary<string, object>();
                foreach (var item in Names)
                {
                    result["name." + item.Key] = item.Value;
                }
                foreach (var item in FullNames)
                {
                    result["fullName." + item.Key] = item.Value;
                }
                foreach (var item in Modifiers)
                {
                    result["modifier." + item.Key] = item.Value;
                }
                foreach (var item in Metadata)
                {
                    result[item.Key] = item.Value;
                }
                return result;
            }
        }

        public static ItemViewModel FromModel(MetadataItem model)
        {
            if (model == null)
            {
                return null;
            }
            var result = new ItemViewModel
            {
                Uid = model.Name,
                Parent = model.Parent?.Name,
                Children = model.Items?.Select(x => x.Name).OrderBy(s => s).ToList(),
                Type = model.Type,
                Source = model.Source,
                Documentation = model.Documentation,
                AssemblyNameList = model.AssemblyNameList,
                NamespaceName = model.NamespaceName,
                Summary = model.Summary,
                Remarks = model.Remarks,
                Examples = model.Examples,
                Syntax = SyntaxDetailViewModel.FromModel(model.Syntax),
                Overridden = model.Overridden,
                Exceptions = model.Exceptions,
                Sees = model.Sees,
                SeeAlsos = model.SeeAlsos,
                Inheritance = model.Inheritance,
                Implements = model.Implements,
                InheritedMembers = model.InheritedMembers,
            };

            result.Id = model.Name.Substring((model.Parent?.Name?.Length ?? -1) + 1);

            result.Name = model.DisplayNames.GetLanguageProperty(SyntaxLanguage.Default);
            var nameForCSharp = model.DisplayNames.GetLanguageProperty(SyntaxLanguage.CSharp);
            if (result.Name != nameForCSharp)
            {
                result.NameForCSharp = nameForCSharp;
            }
            var nameForVB = model.DisplayNames.GetLanguageProperty(SyntaxLanguage.VB);
            if (result.Name != nameForVB)
            {
                result.NameForVB = nameForVB;
            }

            result.FullName = model.DisplayQualifiedNames.GetLanguageProperty(SyntaxLanguage.Default);
            var fullnameForCSharp = model.DisplayQualifiedNames.GetLanguageProperty(SyntaxLanguage.CSharp);
            if (result.FullName != fullnameForCSharp)
            {
                result.FullNameForCSharp = fullnameForCSharp;
            }
            var fullnameForVB = model.DisplayQualifiedNames.GetLanguageProperty(SyntaxLanguage.VB);
            if (result.FullName != fullnameForVB)
            {
                result.FullNameForVB = fullnameForVB;
            }

            var modifierCSharp = model.Modifiers.GetLanguageProperty(SyntaxLanguage.CSharp);
            if (modifierCSharp?.Count > 0)
            {
                result.Modifiers["csharp"] = modifierCSharp;
            }
            var modifierForVB = model.Modifiers.GetLanguageProperty(SyntaxLanguage.VB);
            if (modifierForVB?.Count > 0)
            {
                result.Modifiers["vb"] = modifierForVB;
            }

            return result;
        }
    }
}
