﻿// Librarys
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// From Project
using SkyNotepad.Helpers;
using SkyNotepad.Models;

namespace SkyNotepad.ViewModels
{
    public class FileViewModel
    {
        // Document Model
        public DocumentModel Document { get; private set; }

        // Menu Items Commands
        public ICommand NewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand ExitCommand { get; }

        // File View Model Method
        public FileViewModel(DocumentModel _document)
        {
            Document = _document;
            NewCommand = new RelayCommand(NewFile);
            SaveCommand = new RelayCommand(SaveFile);
            SaveAsCommand = new RelayCommand(SaveFileAs);
            OpenCommand = new RelayCommand(OpenFile);
            ExitCommand = new RelayCommand(Exit);
            
            // Creates new file while first run
            CreateNewFile();
        }

        // Create new file while first run
        private void CreateNewFile()
        {
            Document.FileName = "Untitled";
            Document.FilePath = string.Empty;
            Document.Text = string.Empty;
            Document.IsSaved = false;
            Document.DateCreated = string.Empty;
            Document.AppTitle = Document.FileName + " - SkyNotepad Preview";
        }

        // New Command 
        private void NewFile()
        {
            Document.FileName = "Untitled";
            Document.FilePath = string.Empty;
            Document.Text = string.Empty;
            Document.IsSaved = false;
            Document.DateCreated = string.Empty;
            Document.AppTitle = Document.FileName + " - SkyNotepad Preview";
        }

        // Save Command
        private async void SaveFile()
        {
            if (Document.IsSaved == true)
            {
                StorageFile storageFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(Document.FileToken);
                await FileIO.WriteTextAsync(storageFile, Document.Text);
            }
            else
            {
                SaveFileAs();
            }

        }

        // Save As... Command
        private async void SaveFileAs()
        {
            try
            {
                FileSavePicker savePicker = new FileSavePicker()
                {
                    SuggestedFileName = "Untitled",
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    DefaultFileExtension = ".txt"
                };
                savePicker.FileTypeChoices.Add("Text Document", new List<string>() { ".txt" });

                StorageFile storageFile = await savePicker.PickSaveFileAsync();
                if (storageFile != null)
                {
                    CachedFileManager.DeferUpdates(storageFile);
                    await FileIO.WriteTextAsync(storageFile, Document.Text);
                    FileUpdateStatus updateStatus = await CachedFileManager.CompleteUpdatesAsync(storageFile);
                    Document.FileToken = StorageApplicationPermissions.FutureAccessList.Add(storageFile);
                    Document.FileName = storageFile.Name;
                    Document.FilePath = storageFile.Path;
                    Document.FileType = storageFile.FileType;
                    Document.DateCreated = storageFile.DateCreated.ToString();
                    Document.IsSaved = true;
                    Document.AppTitle = Document.FileName + " - SkyNotepad Preview";
                }
            }
            catch
            {
                ContentDialog FileSaveUnsuccesfullDialog = new ContentDialog()
                {
                    Title = "Error",
                    CloseButtonText = "OK",
                    PrimaryButtonText = "Try Again",
                    DefaultButton = ContentDialogButton.Close,
                    Content = "File Couldn't Saved! Try Again.",
                    PrimaryButtonCommand = SaveAsCommand
                };

                await FileSaveUnsuccesfullDialog.ShowAsync();
            }
        }

        // Open... Command
        private async void OpenFile()
        {
            try
            {
                FileOpenPicker openPicker = new FileOpenPicker()
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                openPicker.FileTypeFilter.Add(".txt");

                StorageFile storageFile = await openPicker.PickSingleFileAsync();
                if (storageFile != null)
                {
                    var stream = await storageFile.OpenAsync(FileAccessMode.Read);
                    using (StreamReader sReader = new StreamReader(stream.AsStream()))
                    {
                        Document.Text = sReader.ReadToEnd();
                        Document.FileName = storageFile.Name;
                        Document.FilePath = storageFile.Path;
                        Document.FileType = storageFile.FileType;
                        Document.DateCreated = storageFile.DateCreated.ToString();
                        Document.IsSaved = true;
                        Document.AppTitle = Document.FileName + " - SkyNotepad Preview";
                    }
                }
            }
            catch
            {
                ContentDialog FileOpenUnsuccesfullDialog = new ContentDialog()
                {
                    Title = "Error",
                    CloseButtonText = "OK",
                    PrimaryButtonText = "Try Again",
                    DefaultButton = ContentDialogButton.Close,
                    Content = "File Couldn't Opened! Try Again.",
                    PrimaryButtonCommand = OpenCommand
                };

                await FileOpenUnsuccesfullDialog.ShowAsync();
            }
        }

        // Exit Command
        private void Exit()
        {
            Application.Current.Exit();
        }
    }
}