// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.EntityModel
{
    public interface IResolverPipeline
    {
        void Run(MetadataModel yaml, ResolverContext context);
    }
}
