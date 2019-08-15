﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Microsoft.VisualStudio.ProjectSystem.Properties;

using Xunit;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Input.Commands
{
    public abstract class AbstractOpenProjectDesignerCommandTests
    {
        [Fact]
        public void GetCommandStatusAsync_NullAsNodes_ThrowsArgumentNull()
        {
            var command = CreateInstance();

            Assert.Throws<ArgumentNullException>("nodes", () =>
            {
                command.GetCommandStatusAsync(null!, GetCommandId(), true, "commandText", CommandStatus.Enabled);
            });
        }

        [Fact]
        public async Task GetCommandStatusAsync_UnrecognizedCommandIdAsCommandId_ReturnsUnhandled()
        {
            var command = CreateInstance();

            var tree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    Properties (flags: {Folder AppDesignerFolder})
");

            var nodes = ImmutableHashSet.Create(tree.Children[0]);

            var result = await command.GetCommandStatusAsync(nodes, 1, true, "commandText", CommandStatus.Enabled);

            Assert.False(result.Handled);
        }

        [Fact]
        public async Task TryHandleCommandAsync_UnrecognizedCommandIdAsCommandId_ReturnsFalse()
        {
            var command = CreateInstance();

            var tree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    Properties (flags: {Folder AppDesignerFolder})
");

            var nodes = ImmutableHashSet.Create(tree.Children[0]);

            var result = await command.TryHandleCommandAsync(nodes, 1, true, 0, IntPtr.Zero, IntPtr.Zero);

            Assert.False(result);
        }

        [Fact]
        public async Task GetCommandStatusAsync_MoreThanOneNodeAsNodes_ReturnsUnhandled()
        {
            var command = CreateInstance();

            var tree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    Properties (flags: {Folder AppDesignerFolder})
");

            var nodes = ImmutableHashSet.Create(tree, tree.Children[0]);

            var result = await command.GetCommandStatusAsync(nodes, GetCommandId(), true, "commandText", (CommandStatus)0);

            Assert.False(result.Handled);
        }


        [Fact]
        public async Task TryHandleCommandAsync_MoreThanOneNodeAsNodes_ReturnsFalse()
        {
            var command = CreateInstance();

            var tree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    Properties (flags: {Folder AppDesignerFolder})
");

            var nodes = ImmutableHashSet.Create(tree, tree.Children[0]);

            var result = await command.TryHandleCommandAsync(nodes, GetCommandId(), true, 0, IntPtr.Zero, IntPtr.Zero);

            Assert.False(result);
        }

        [Fact]
        public async Task GetCommandStatusAsync_NonAppDesignerFolderAsNodes_ReturnsUnhandled()
        {
            var command = CreateInstance();

            var tree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    Properties (flags: {Folder})
");

            var nodes = ImmutableHashSet.Create(tree.Children[0]);

            var result = await command.GetCommandStatusAsync(nodes, GetCommandId(), true, "commandText", (CommandStatus)0);

            Assert.False(result.Handled);
        }

        [Fact]
        public async Task TryHandleCommandAsync_NonAppDesignerFolderAsNodes_ReturnsFalse()
        {
            var command = CreateInstance();

            var tree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    Properties (flags: {Folder})
");

            var nodes = ImmutableHashSet.Create(tree.Children[0]);

            var result = await command.TryHandleCommandAsync(nodes, GetCommandId(), true, 0, IntPtr.Zero, IntPtr.Zero);

            Assert.False(result);
        }

        [Fact]
        public async Task GetCommandStatusAsync_AppDesignerFolderAsNodes_ReturnsHandled()
        {
            var command = CreateInstance();

            var tree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    Properties (flags: {Folder AppDesignerFolder})
");

            var nodes = ImmutableHashSet.Create(tree.Children[0]);

            var result = await command.GetCommandStatusAsync(nodes, GetCommandId(), true, "commandText", (CommandStatus)0);

            Assert.True(result.Handled);
            Assert.Equal("commandText", result.CommandText);
            Assert.Equal(CommandStatus.Enabled | CommandStatus.Supported, result.Status);
        }

        [Fact]
        public async Task TryHandleCommandAsync_AppDesignerFolderAsNodes_ReturnsTrue()
        {
            var command = CreateInstance();

            var tree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    Properties (flags: {Folder AppDesignerFolder})
");

            var nodes = ImmutableHashSet.Create(tree.Children[0]);

            var result = await command.TryHandleCommandAsync(nodes, GetCommandId(), true, 0, IntPtr.Zero, IntPtr.Zero);

            Assert.True(result);
        }

        [Fact]
        public async Task TryHandleCommandAsync_AppDesignerFolderAsNodes_CallsShowProjectDesignerAsync()
        {
            int callCount = 0;
            var designerService = IProjectDesignerServiceFactory.ImplementShowProjectDesignerAsync(() => { callCount++; });

            var command = CreateInstance(designerService);

            var tree = ProjectTreeParser.Parse(@"
Root (flags: {ProjectRoot})
    Properties (flags: {Folder AppDesignerFolder})
");

            var nodes = ImmutableHashSet.Create(tree.Children[0]);

            await command.TryHandleCommandAsync(nodes, GetCommandId(), true, 0, IntPtr.Zero, IntPtr.Zero);

            Assert.Equal(1, callCount);
        }

        internal abstract long GetCommandId();

        internal abstract AbstractOpenProjectDesignerCommand CreateInstance(IProjectDesignerService? designerService = null);
    }
}
