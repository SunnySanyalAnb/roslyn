﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Remote.ProjectSystem;
using Roslyn.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.ProjectSystem.BrokeredService
{
    internal sealed class WorkspaceProject : IWorkspaceProject
    {
        // For the sake of the in-proc implementation here, we're going to build this atop IWorkspaceProjectContext so semantics are preserved
        // for a few edge cases. Once the project system has moved onto this directly, we can flatten the implementations out.
        private readonly IWorkspaceProjectContext _project;

        public WorkspaceProject(IWorkspaceProjectContext project)
        {
            _project = project;
        }

        public ValueTask DisposeAsync()
        {
            _project.Dispose();
            return ValueTaskFactory.CompletedTask;
        }

        public async Task AddAdditionalFilesAsync(IReadOnlyList<string> additionalFilePaths, CancellationToken cancellationToken)
        {
            await using var batch = _project.CreateBatchScope().ConfigureAwait(false);

            foreach (var additionalFilePath in additionalFilePaths)
                _project.AddAdditionalFile(additionalFilePath);
        }

        public async Task RemoveAdditionalFilesAsync(IReadOnlyList<string> additionalFilePaths, CancellationToken cancellationToken)
        {
            await using var batch = _project.CreateBatchScope().ConfigureAwait(false);

            foreach (var additionalFilePath in additionalFilePaths)
                _project.RemoveAdditionalFile(additionalFilePath);
        }

        public async Task AddAnalyzerConfigFilesAsync(IReadOnlyList<string> analyzerConfigPaths, CancellationToken cancellationToken)
        {
            await using var batch = _project.CreateBatchScope().ConfigureAwait(false);

            foreach (var analyzerConfigPath in analyzerConfigPaths)
                _project.AddAnalyzerConfigFile(analyzerConfigPath);
        }
        public async Task RemoveAnalyzerConfigFilesAsync(IReadOnlyList<string> analyzerConfigPaths, CancellationToken cancellationToken)
        {
            await using var batch = _project.CreateBatchScope().ConfigureAwait(false);

            foreach (var analyzerConfigPath in analyzerConfigPaths)
                _project.RemoveAnalyzerConfigFile(analyzerConfigPath);
        }

        public async Task AddAnalyzerReferencesAsync(IReadOnlyList<string> analyzerPaths, CancellationToken cancellationToken)
        {
            await using var batch = _project.CreateBatchScope().ConfigureAwait(false);

            foreach (var analyzerPath in analyzerPaths)
                _project.AddAnalyzerReference(analyzerPath);
        }

        public async Task RemoveAnalyzerReferencesAsync(IReadOnlyList<string> analyzerPaths, CancellationToken cancellationToken)
        {
            await using var batch = _project.CreateBatchScope().ConfigureAwait(false);

            foreach (var analyzerPath in analyzerPaths)
                _project.RemoveAnalyzerReference(analyzerPath);
        }

        public async Task AddMetadataReferencesAsync(IReadOnlyList<MetadataReferenceInfo> metadataReferences, CancellationToken cancellationToken)
        {
            await using var batch = _project.CreateBatchScope().ConfigureAwait(false);

            foreach (var metadataReference in metadataReferences)
            {
                _project.AddMetadataReference(
                    metadataReference.FilePath,
                    new MetadataReferenceProperties(MetadataImageKind.Assembly, default, metadataReference.EmbedInteropTypes));
            }
        }

        public async Task RemoveMetadataReferencesAsync(IReadOnlyList<MetadataReferenceInfo> metadataReferences, CancellationToken cancellationToken)
        {
            await using var batch = _project.CreateBatchScope().ConfigureAwait(false);

            // The existing IWorkspaceProjectContext API here is a bit odd in that it only looks at the file path, and trusts that there aren't two
            // references with the same path but different properties.
            foreach (var metadataReference in metadataReferences)
                _project.RemoveMetadataReference(metadataReference.FilePath);
        }

        public async Task AddSourceFilesAsync(IReadOnlyList<SourceFileInfo> sourceFiles, CancellationToken cancellationToken)
        {
            await using var batch = _project.CreateBatchScope().ConfigureAwait(false);

            foreach (var sourceFile in sourceFiles)
            {
                _project.AddSourceFile(
                    sourceFile.FilePath,
                    folderNames: sourceFile.FolderNames);
            }
        }
        public async Task RemoveSourceFilesAsync(IReadOnlyList<string> sourceFiles, CancellationToken cancellationToken)
        {
            await using var batch = _project.CreateBatchScope().ConfigureAwait(false);

            foreach (var sourceFile in sourceFiles)
                _project.RemoveSourceFile(sourceFile);
        }

        public async Task SetBuildSystemPropertiesAsync(IReadOnlyDictionary<string, string> properties, CancellationToken cancellationToken)
        {
            await using var batch = _project.CreateBatchScope().ConfigureAwait(false);

            foreach (var property in properties)
                _project.SetProperty(property.Key, property.Value);
        }

        public Task SetCommandLineArgumentsAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
        {
            _project.SetOptions(arguments.ToImmutableArray());
            return Task.CompletedTask;
        }

        public Task SetDisplayNameAsync(string displayName, CancellationToken cancellationToken)
        {
            _project.DisplayName = displayName;
            return Task.CompletedTask;
        }

        public Task<IAsyncDisposable> StartBatchAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_project.CreateBatchScope());
        }
    }
}
