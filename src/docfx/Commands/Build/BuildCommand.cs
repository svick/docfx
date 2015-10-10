// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode
{
    using Microsoft.DocAsCode.EntityModel;
    using Microsoft.DocAsCode.EntityModel.Builders;
    using Newtonsoft.Json.Linq;
    using System.Linq;
    using System.IO;
    using System;
    using System.Collections.Generic;
    using Utility;
    using System.Collections.Immutable;
    using Plugins;

    class BuildCommand : ICommand
    {
        private DocumentBuilder _builder = new DocumentBuilder();
        public BuildJsonConfig Config { get; }

        public BuildCommand() : this(new BuildJsonConfig())
        {
        }

        public BuildCommand(JToken value): this(CommandFactory.ConvertJTokenTo<BuildJsonConfig>(value))
        {
        }

        public BuildCommand(BuildJsonConfig config)
        {
            Config = config;
        }

        public BuildCommand(Options options): this(options.BuildCommand) { }

        public BuildCommand(BuildCommandOptions options): this(GetConfigFromOptions(options)) { }

        public ParseResult Exec(RunningContext context)
        {
            Config.BaseDirectory = context?.BaseDirectory;
            return InternalExec(Config, context);
        }

        private ParseResult InternalExec(BuildJsonConfig config, RunningContext context)
        {
            try
            {
                var parameters = ConfigToParameter(config);
                _builder.Build(parameters);

                var documentContext = DocumentBuildContext.DeserializeFrom(parameters.OutputBaseDir);
                var assembly = typeof(Program).Assembly;
                var outputFolder = Path.Combine(config.BaseDirectory ?? string.Empty, config.Destination);
                var templateFolder = string.IsNullOrEmpty(config.TemplateFolder) ? null : Path.Combine(config.BaseDirectory ?? string.Empty, config.TemplateFolder);
                var themeFolder = string.IsNullOrEmpty(config.TemplateThemeFolder) ? null : Path.Combine(config.BaseDirectory ?? string.Empty, config.TemplateThemeFolder);
                using (var manager = new TemplateManager(assembly, "Template", templateFolder, config.Template, themeFolder, config.TemplateTheme))
                {
                    manager.ProcessTemplateAndTheme(documentContext, outputFolder, true);
                }

                // TODO: SEARCH DATA

                return ParseResult.SuccessResult;
            }
            catch (Exception e)
            {
                return new ParseResult(ResultLevel.Error, e.Message);
            }
        }

        private static DocumentBuildParameters ConfigToParameter(BuildJsonConfig config)
        {
            var parameters = new DocumentBuildParameters();
            var baseDirectory = config.BaseDirectory ?? Environment.CurrentDirectory;

            parameters.OutputBaseDir = Path.GetFullPath(Path.Combine("obj", Path.GetRandomFileName()));
            parameters.Metadata = (config.GlobalMetadata ?? new Dictionary<string, object>()).ToImmutableDictionary();
            parameters.ExternalReferencePackages = GetFilesFromFileMapping(GlobUtility.ExpandFileMapping(baseDirectory, config.ExternalReference)).ToImmutableArray();
            parameters.Files = GetFileCollectionFromFileMapping(
               Tuple.Create(DocumentType.Article, GlobUtility.ExpandFileMapping(baseDirectory, config.Content)),
               Tuple.Create(DocumentType.Override, GlobUtility.ExpandFileMapping(baseDirectory, config.Overwrite)),
               Tuple.Create(DocumentType.Resource, GlobUtility.ExpandFileMapping(baseDirectory, config.Resource)));
            return parameters;
        }

        private static IEnumerable<string> GetFilesFromFileMapping(FileMapping mapping)
        {
            foreach(var file in mapping.Items)
            {
                foreach(var item in file.Files)
                {
                    yield return Path.Combine(file.CurrentWorkingDirectory ?? Environment.CurrentDirectory, item);
                }
            }
        }

        private static FileCollection GetFileCollectionFromFileMapping(params Tuple<DocumentType, FileMapping>[] files)
        {
            var fileCollection = new FileCollection(null);
            foreach(var file in files)
            {
                if (file.Item2 != null)
                {
                    foreach (var mapping in file.Item2.Items)
                    {
                        fileCollection.Add(file.Item1, mapping.CurrentWorkingDirectory, mapping.Files);
                    }
                }
            }

            return fileCollection;
        }

        private static BuildJsonConfig GetConfigFromOptions(BuildCommandOptions options)
        {
            string jsonConfig;
            if (CommandFactory.TryGetJsonConfig(options.Content, out jsonConfig))
            {
                var command = (BuildCommand)CommandFactory.ReadConfig(jsonConfig).Commands.FirstOrDefault(s => s is BuildCommand);
                if (command == null) throw new ApplicationException($"Unable to find {SubCommandType.Build} subcommand config in file '{Constants.ConfigFileName}'.");
                return command.Config;
            }

            var config = new BuildJsonConfig();
            if (!string.IsNullOrEmpty(options.Template)) config.Template = options.Template;
            if (!string.IsNullOrEmpty(options.TemplateFolder)) config.TemplateFolder = options.TemplateFolder;

            if (!string.IsNullOrEmpty(options.TemplateTheme)) config.TemplateTheme = options.TemplateTheme;
            if (!string.IsNullOrEmpty(options.TemplateThemeFolder)) config.TemplateThemeFolder = options.TemplateThemeFolder;
            if (!string.IsNullOrEmpty(options.OutputFolder)) config.Destination = options.OutputFolder;
            config.Content = new FileMapping(new FileMappingItem() { Files = new FileItems(options.Content) });
            config.Resource = new FileMapping(new FileMappingItem() { Files = new FileItems(options.Resource) });
            config.Overwrite = new FileMapping(new FileMappingItem() { Files = new FileItems(options.Overwrite) });
            config.ExternalReference = new FileMapping(new FileMappingItem() { Files = new FileItems(options.ExternalReference) });
            return config;
        }
    }
}