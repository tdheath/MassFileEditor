using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MassFileEditor
{
    /// <summary>
    /// Popup window to allow the user to select extensions to search for.
    /// </summary>
    public partial class ExtensionPopup : Window
    {
        //Action which is used to return values to the parent window
        private Action<List<string>> returnExtensions;
        List<string> extensionsToUse, availableExtensions;
        bool changeMade;

        //Constructor, which initializes with the current extension list and a given action 
        //to return the edited value via a method in the main window
        public ExtensionPopup(List<string> currentExts, Action<List<string>> action)
        {
            InitializeComponent();

            Closed += sendExtensions;
            returnExtensions = action;

            //set the available exensions to use
            availableExtensions = new List<string>{".avi",".mov",".mp4",".mkv",
                                                   ".flv",".mpg",".wmv",".gif",
                                                   ".jpg",".png",".wma",".wav",                                                   
                                                   ".mid",".mp3",".xls",".xlsx",
                                                   ".doc",".docx",".pdf",".log",
                                                   ".txt",".ppt",".pptx",".xml",
                                                   ".rar",".7z",".zip",".torrent"};

            //set default extensions to video files
            extensionsToUse = currentExts;//new List<string>{".avi",".mov",".mp4",".mkv",
                                                   //".flv",".mpg",".wmv"};

            changeMade = false;

            buildChoiceGrid();
        }

        //Method to build the grid of checkboxes for the user to select extensions with
        private void buildChoiceGrid()
        {
            List<Grid> contentList = new List<Grid>();
            foreach (string extension in availableExtensions)
            {
                Grid newRow = new Grid();
                ColumnDefinition currentNameCol = new ColumnDefinition();
                currentNameCol.Width = new GridLength(15);
                ColumnDefinition newNameCol = new ColumnDefinition();

                newRow.ColumnDefinitions.Add(currentNameCol);
                newRow.ColumnDefinitions.Add(newNameCol);
                Label extLabel = new Label();
                extLabel.Content = extension;

                CheckBox extCheckbox = new CheckBox();
                extCheckbox.Width = 15;
                if (extensionsToUse.Contains(extension))
                {
                    extCheckbox.IsChecked = true;
                }
                extCheckbox.Click += boxChecked;

                Grid.SetColumn(extCheckbox, 0);
                Grid.SetColumn(extLabel, 1);

                newRow.Children.Add(extCheckbox);
                newRow.Children.Add(extLabel);
                contentList.Add(newRow);

            }
            Grid extGrid = new Grid();
            extGrid.Margin = new Thickness(20, 0, 20, 0);
            extGrid.ColumnDefinitions.Add(new ColumnDefinition());
            extGrid.ColumnDefinitions.Add(new ColumnDefinition());
            extGrid.ColumnDefinitions.Add(new ColumnDefinition());
            extGrid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int i = 0; i < contentList.Count; i++)
            {
                if (i % 4 == 0)
                {
                    extGrid.RowDefinitions.Add(new RowDefinition());
                }
                Grid.SetColumn(contentList[i], i % 4);
                Grid.SetRow(contentList[i], i / 4);

                extGrid.Children.Add(contentList[i]);
            }
            Grid.SetRow(extGrid, 1);
            mainGrid.Children.Add(extGrid);
        }

        //Method to build the extension list based on the user's choices
        private void reconstructExtensionList()
        {
            extensionsToUse.Clear();
            //access the last child, which is the grid added within the method above
            Grid extGrid = mainGrid.Children[mainGrid.Children.Count - 1] as Grid;
            foreach (Grid row in extGrid.Children)
            {
                //each grid entry is [checkbox | label], so access accordingly
                CheckBox useExt = row.Children[0] as CheckBox;
                if (useExt.IsChecked == true)
                {
                    Label extLabel = row.Children[1] as Label;
                    extensionsToUse.Add(extLabel.Content.ToString());
                }
            }
        }

        #region Event Handlers

        //Event handler for when the 'Ok' button is clicked
        private void acceptChange_Click_1(object sender, RoutedEventArgs e)
        {
            if (changeMade)
            {
                reconstructExtensionList();
                changeMade = false;
            }
            this.Close();
        }

        //Event handler for when the user checks a box
        private void boxChecked(object sender, RoutedEventArgs e)
        {
            changeMade = true;
        }

        //Event handler to return the new extension list to the parent window
        private void sendExtensions(object sender, EventArgs e)
        {
            returnExtensions(extensionsToUse);
        }
        #endregion

    }
}
