// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.EntityModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    using Jint;

    using Microsoft.DocAsCode.Common;
    using Microsoft.DocAsCode.Utility;

    public class Template
    {
        private static readonly Regex IsRegexPatternRegex = new Regex(@"^\s*/(.*)/\s*$", RegexOptions.Compiled);
        private readonly object _locker = new object();
        private readonly ResourcePoolManager<ITemplateRenderer> _rendererPool = null;

        private readonly ResourcePoolManager<Engine> _enginePool = null;
        private readonly string _script;

        public string Name { get; }
        public string ScriptName { get; }
        public string Extension { get; }
        public string Type { get; }
        public bool IsPrimary { get; }
        public IEnumerable<TemplateResourceInfo> Resources { get; }

        public Template(string template, string templateName, string script, ResourceCollection resourceCollection)
        {
            if (string.IsNullOrEmpty(templateName)) throw new ArgumentNullException(nameof(templateName));
            if (string.IsNullOrEmpty(template)) throw new ArgumentNullException(nameof(template));
            Name = templateName;
            var typeAndExtension = GetTemplateTypeAndExtension(templateName);
            Extension = typeAndExtension.Item2;
            Type = typeAndExtension.Item1;
            IsPrimary = typeAndExtension.Item3;
            _script = script;
            if (script != null)
            {
                ScriptName = templateName + ".js";
                _enginePool = ResourcePool.Create(() => CreateEngine(script), Constants.DefaultParallelism);
            }

            if (resourceCollection != null)
            {
                _rendererPool = ResourcePool.Create(() => CreateRenderer(resourceCollection, templateName, template), Constants.DefaultParallelism);
            }

            Resources = ExtractDependentResources();
        }

        /// <summary>
        /// Transform from raw model to view model
        /// TODO: refactor to merge model and attrs into one input model
        /// </summary>
        /// <param name="model">The raw model</param>
        /// <param name="attrs">The system generated attributes</param>
        /// <returns>The view model</returns>
        public object TransformModel(object model, object attrs, object global)
        {
            if (_enginePool == null) return model;
            return ProcessWithJint(model, attrs, global);
        }

        /// <summary>
        /// Transform from view model to the final result using template
        /// Supported template languages are mustache and liquid
        /// </summary>
        /// <param name="model">The input view model</param>
        /// <returns>The output after applying template</returns>
        public string Transform(object model)
        {
            if (_rendererPool == null || model == null) return null;
            using (var lease = _rendererPool.Rent())
            {
                return lease.Resource.Render(model);
            }
        }

        private object ProcessWithJint(object model, object attrs, object global)
        {
            var argument1 = JintProcessorHelper.ConvertStrongTypeToJsValue(model);
            var argument2 = JintProcessorHelper.ConvertStrongTypeToJsValue(attrs);
            var argument3 = JintProcessorHelper.ConvertStrongTypeToJsValue(global);
            using (var lease = _enginePool.Rent())
            {
                return lease.Resource.Invoke("transform", argument1, argument2, argument3).ToObject();
            }
        }

        private string GetRelativeResourceKey(string relativePath)
        {
            // Make sure resource keys are combined using '/'
            return Path.GetDirectoryName(this.Name).ToNormalizedPath().ForwardSlashCombine(relativePath);
        }

        private static Tuple<string, string, bool> GetTemplateTypeAndExtension(string templateName)
        {
            // Remove folder and .tmpl
            templateName = Path.GetFileNameWithoutExtension(templateName);
            var splitterIndex = templateName.IndexOf('.');
            if (splitterIndex < 0) return Tuple.Create(templateName, string.Empty, false);
            var type = templateName.Substring(0, splitterIndex);
            var extension = templateName.Substring(splitterIndex);
            var isPrimary = false;
            if (extension.EndsWith(".primary"))
            {
                isPrimary = true;
                extension = extension.Substring(0, extension.Length - 8);
            }
            return Tuple.Create(type, extension, isPrimary);
        }


        /// <summary>
        /// Dependent files are defined in following syntax in Mustache template leveraging Mustache Comments
        /// {{! include('file') }}
        /// file path can be wrapped by quote ' or double quote " or none
        /// </summary>
        /// <param name="template"></param>
        private IEnumerable<TemplateResourceInfo> ExtractDependentResources()
        {
            if (_rendererPool == null) yield break;
            using (var lease = _rendererPool.Rent())
            {
                var _renderer = lease.Resource;
                if (_renderer.Dependencies == null) yield break;
                foreach (var dependency in _renderer.Dependencies)
                {
                    string filePath = dependency;
                    if (string.IsNullOrWhiteSpace(filePath)) continue;
                    if (filePath.StartsWith("./")) filePath = filePath.Substring(2);
                    var regexPatternMatch = IsRegexPatternRegex.Match(filePath);
                    if (regexPatternMatch.Groups.Count > 1)
                    {
                        filePath = regexPatternMatch.Groups[1].Value;
                        yield return new TemplateResourceInfo(GetRelativeResourceKey(filePath), filePath, true);
                    }
                    else
                    {
                        yield return new TemplateResourceInfo(GetRelativeResourceKey(filePath), filePath, false);
                    }
                }
            }
        }

        private static Engine CreateEngine(string script)
        {
            if (string.IsNullOrEmpty(script)) throw new ArgumentNullException(nameof(script));
            var engine = new Engine();

            engine.SetValue("console", new
            {
                log = new Action<object>(Logger.Log)
            });

            // throw exception when execution fails
            engine.Execute(script);
            return engine;
        }

        private static ITemplateRenderer CreateRenderer(ResourceCollection resourceCollection, string templateName, string template)
        {
            if (resourceCollection == null) throw new ArgumentNullException(nameof(resourceCollection));
            if (Path.GetExtension(templateName).Equals(".liquid", StringComparison.OrdinalIgnoreCase))
            {
                return LiquidTemplateRenderer.Create(resourceCollection, template);
            }
            else
            {
                return new MustacheTemplateRenderer(resourceCollection, template);
            }
        }
    }
}
