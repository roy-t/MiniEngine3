﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content
{
    [Service]
    public sealed partial class ContentManager : IDisposable
    {
        private readonly ILogger Logger;
        private readonly Device Device;
        private readonly IVirtualFileSystem FileSystem;
        private readonly Stack<List<IContent>> ContentStack;

        public ContentManager(ILogger logger, Device device, IVirtualFileSystem fileSystem)
        {
            this.Logger = logger.ForContext<ContentManager>();
            this.ContentStack = new Stack<List<IContent>>();
            this.ContentStack.Push(new List<IContent>());
            this.Device = device;
            this.FileSystem = fileSystem;
        }

        public void Push()
        {
            this.ContentStack.Push(new List<IContent>());
        }

        public void Pop()
        {
            Dispose(this.ContentStack.Pop());
        }

        public void Dispose()
        {
            while (this.ContentStack.Count > 0)
            {
                Dispose(this.ContentStack.Pop());
            }
        }

        [Conditional("DEBUG")]
        public void ReloadChangedContent()
        {
            foreach (var file in this.FileSystem.GetChangedFiles())
            {
                try
                {
                    this.ReloadContentReferencingFile(file);
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, "Failed to reload {@file}", file);
                }
            }

            this.FileSystem.ClearChangedFiles();
        }

        private void Add(IContent content)
        {
            this.ContentStack.Peek().Add(content);
            this.Watch(content);
        }

        [Conditional("DEBUG")]
        private void Watch(IContent content)
        {
            this.FileSystem.WatchFile(content.FileName);
            this.Logger.Information("Watching file {@file}", content.FileName);
        }

        private void ReloadContentReferencingFile(string path)
        {
            foreach (var list in this.ContentStack)
            {
                foreach (var content in list)
                {
                    if (content.FileName.Equals(path, StringComparison.OrdinalIgnoreCase))
                    {
                        this.Logger.Information("Reloading {@content} because it references {@file}", content.GetType().Name, path);
                        content.Reload();
                    }
                }
            }
        }

        private static void Dispose(List<IContent> list)
        {
            foreach (var content in list)
            {
                content.Dispose();
            }
        }
    }
}