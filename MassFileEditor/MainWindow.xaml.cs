using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MassFileEditor
{
    /// <summary>
    /// Logic behind the mass file renaming tool
    /// </summary>
    public partial class MainWindow : Window
    {
        string directory = "";//Current directory 
       //List of currently found files & file extensions to check for, respectively
        List<string> currentFileList = new List<string>();
        //Set default to video extensions
        List<string> extensionsToCheck = new List<string>{".avi",".mov",".mp4",".mkv",
                                                         ".flv",".mpg",".wmv"};       
        public MainWindow()
        {
            InitializeComponent();
        }

        //Method to rename files to their associated inputs, if needed
        private void renameFiles()
        {
            string currentFilePath;
            string newFilePath;
            string extension;
            foreach (Grid mediaFileRow in textBoxList.Items)
            {
                Label oldFileName = mediaFileRow.Children[0] as Label;
                TextBox renameBox = mediaFileRow.Children[1] as TextBox;
                if (File.Exists(currentDirectory.Content + "\\" + oldFileName.Content))
                {
                    //only rename a file if a new name has been defined
                    if (renameBox.Text != "")
                    {
                        currentFilePath = currentDirectory.Content + "\\" + oldFileName.Content;
                        extension = oldFileName.Content.ToString().Substring(oldFileName.Content.ToString().LastIndexOf('.'));
                        newFilePath = currentDirectory.Content + "\\" + prefixBox.Text + renameBox.Text + extension;
                        File.Move(currentFilePath, newFilePath);

                        //update the file list, for property refactoring
                        currentFileList.Remove(currentFilePath);
                        currentFileList.Add(newFilePath);
                    }
                }
            }
        }

        //Method to search for all desired files
        //
        //Returns a list of filepaths
        public List<string> search(string path)
        {
            //get all files that have a common extension, found in 'extensionsToCheck'
            SearchOption depth;
            //Assume only the top level folders for renames, as lower might have different prefixes
            if (topLevelOnlyBox.IsChecked.Value)
            {
                Debug.WriteLine("Showing only toplvl");
                depth = SearchOption.TopDirectoryOnly;
            }
            else
            {
                Debug.WriteLine("Show all");
                depth = SearchOption.AllDirectories;
            }
            //List any files with the extensions chosen
            List<string> files = (Directory.EnumerateFiles(path, "*.*", depth)
                .Where(s => extensionsToCheck.Any(s.EndsWith))).ToList();
            return files;
        }

        //Method to initiate file metadata property edits
        private void refactorFileProperties(List<string> files)
        {
            TagLib.File toFix;
            foreach (string f in files)
            {
                try
                {
                    toFix = TagLib.File.Create(f);

                    //Below refers to the experimental method at the bottom of this file
                    //which should be run here if desired
                    //editSelectedAttribute(toFix);

                    toFix.Tag.Title = null;
                    toFix.Tag.Title = getFileName(toFix.Name, false);
                    toFix.Save();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }
        }

        //Method to populate the file name listbox
        private void populateFileList()
        {
            List<Grid> fileList = new List<Grid>();
            foreach (string file in currentFileList)
            {
                Grid newRow = new Grid();
                ColumnDefinition currentNameCol = new ColumnDefinition();
                currentNameCol.Width = new GridLength(300);
                ColumnDefinition newNameCol = new ColumnDefinition();

                newRow.ColumnDefinitions.Add(currentNameCol);
                newRow.ColumnDefinitions.Add(newNameCol);
                Label fileLabel = new Label();
                fileLabel.Content = getFileName(file, true);

                TextBox fileNameTextbox = new TextBox();
                fileNameTextbox.Width = 300;
                Grid.SetColumn(fileLabel, 0);
                Grid.SetColumn(fileNameTextbox, 1);

                newRow.Children.Add(fileLabel);
                newRow.Children.Add(fileNameTextbox);
                fileList.Add(newRow);

            }
            textBoxList.ItemsSource = fileList;
        }

        //Method to parse file paths down to just the name
        private static string getFileName(string f, bool includeExtension)
        {
            string[] pathSplit = f.Split('\\');
            string tempName = pathSplit.Last();

            if (!includeExtension)
            {
                int extStart = tempName.LastIndexOf('.');
                tempName = tempName.Substring(0, extStart);
            }
            return tempName;
        }

        //Method to set the searchable extension list
        //triggered by the 'ExtensionPopup' window
        private void setExtensions(List<string> newExtensionList)
        {
            extensionsToCheck = newExtensionList;
            initiateFileList();
        }

        #region Event Handlers

        //Event handler for clicking the 'select directory' button
        private void directorySelect_Click_1(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog findChooser = new System.Windows.Forms.FolderBrowserDialog();

            if (findChooser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                directory = findChooser.SelectedPath;
                currentDirectory.Content = directory;
                initiateFileList();
            }
        }

        //Event handler for when the 'start' button is clicked
        private void start_Click_1(object sender, RoutedEventArgs e)
        {
            if (directory != "")
            {
                renameFiles();

                if (setTitle.IsChecked.Value)
                {
                    refactorFileProperties(currentFileList);
                }

                initiateFileList();
            }
            else
            {
                System.Windows.MessageBox.Show("Error: Please select a filepath before starting the utility");
            }

        }

        //Method to prime and start the file list population
        private void initiateFileList()
        {
            if (currentDirectory.Content.ToString() == "None")
            {
                return;
            }
            currentFileList = search(currentDirectory.Content.ToString());
            if (currentFileList.Count > 0)
            {
                populateFileList();
            }
        }

        //Event handler for clickening the 'Set Extensions" button
        private void extensions_Click_1(object sender, RoutedEventArgs e)
        {
            ExtensionPopup extPopup = new ExtensionPopup(extensionsToCheck, setExtensions);
            extPopup.Show();
        }

        #endregion


        #region Expanded Functionality Methods
        /*
         * Note: These should work alright, but are generally untested.
         *      Use them to expand functionality to other types of media files.
         *      Associated GUI elements are commented out in 'MainWindow.xaml'
         */

        /*
      //This Method determines which metadata tag to edit
      private void editSelectedAttribute(TagLib.File toFix)
      {
          //use 'selected value' for more descriptive switch
          switch (attributeBox.SelectedValue.ToString())
          {
              case "None":
                  break;
              case "Title":
                  toFix.Tag.Title = null;
                  toFix.Tag.Title = getFileName(toFix.Name, false);
                  break;
              case "Artist":
                  toFix.Tag.AlbumArtists = null;
                  toFix.Tag.AlbumArtists = metaDataValues.Text.Split(',');
                  break;
              case "Genre":
                  toFix.Tag.Genres = null;
                  toFix.Tag.Genres = metaDataValues.Text.Split(',');
                  break;
              case "Track":
                  uint track;
                  if (UInt32.TryParse(metaDataValues.Text.ToString(), out track))
                  {
                      toFix.Tag.Track = 0;
                      toFix.Tag.Track = track;
                  }
                  else
                  {
                      MessageBox.Show("Error: Track must be a number. Ignoring attribute changes.");
                  }
                  break;
              case "Year":
                  uint year;
                  if (UInt32.TryParse(metaDataValues.Text.ToString(), out year))
                  {
                      toFix.Tag.Year = 0;
                      toFix.Tag.Year = year;
                  }
                  else
                  {
                      MessageBox.Show("Error: Year must be a number. Ignoring attribute changes.");
                  }
                  break;
              case "Album":
                  toFix.Tag.Album = null;
                  toFix.Tag.Album = metaDataValues.Text;
                  break;
              default:
                  break;
          }
      }
       * */
        #endregion
    }
}
