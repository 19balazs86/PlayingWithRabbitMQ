﻿using System;
using System.IO;
using System.Reactive.Linq;

namespace PlayingWithRabbitMQ.Queue.FileSystem
{
  public class Consumer<T> : IConsumer<T> where T : class
  {
    private const string _searchPattern = "*.json";

    private readonly FileSystemWatcher _fileSystemWatcher;

    public IObservable<IMessage<T>> MessageSource { get; private set; }

    public Consumer(string messageFolderPath, string failedMessageFolderPath)
    {
      _fileSystemWatcher = new FileSystemWatcher(messageFolderPath, _searchPattern);

      // --> Func to create Message object.
      Func<string, IMessage<T>> createMessageFunc = msgPath => new Message<T>(msgPath, failedMessageFolderPath);

      // --> Create Observable for the existing files in the folder.
      IObservable<IMessage<T>> existingFilesObservable = Directory
        .GetFiles(messageFolderPath, _searchPattern)
        .ToObservable()
        .Select(createMessageFunc);

      // --> Create Observable for the FileSystemWatcher.
      IObservable<IMessage<T>> fsWatcherObservable = Observable.FromEvent<FileSystemEventHandler, string>(
        // Conversion.
        action => (sender, fsEventArgs) => action(fsEventArgs.FullPath),
        // Add handler.
        handler =>
        {
          _fileSystemWatcher.Created += handler;

          _fileSystemWatcher.EnableRaisingEvents = true;
        },
        // Remove handler.
        handler =>
        {
          _fileSystemWatcher.EnableRaisingEvents = false;

          _fileSystemWatcher.Created -= handler;
        })
        .Select(createMessageFunc);

      // --> Create Observable to cancat these 2 source.
      MessageSource = existingFilesObservable.Concat(fsWatcherObservable);
    }

    public void Dispose() => _fileSystemWatcher.Dispose();
  }
}