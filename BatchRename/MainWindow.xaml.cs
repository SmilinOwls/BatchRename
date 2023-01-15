using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Text.Json;
using System.Windows.Threading;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Core;
using Core.Format;

namespace BatchRename
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private BindingList<Model> itemTypes;
        private BindingList<IRule> rules = new BindingList<IRule>();
        private BindingList<IRule> selectedRules = new BindingList<IRule>();
       
        private ObservableCollection<FileFormat> listViewFiles = new ObservableCollection<FileFormat>();
        private ObservableCollection<FolderFormat> listViewFolders = new ObservableCollection<FolderFormat>();

        public DispatcherTimer autoSaveTimer;
        CollectionView view = null;
        ICollectionView iView = null;

        private string projectName = "Unsaved Work";
        private string projectSavingAddress = "autosave.json";
        private string lastUseSavingAddress = "lastuse.json";
        private const string auto_save = "autosave.json";
        private string currentPresetPath = "";

        // Initiate Data Lists/Collections
        List<IRule> availableRules = new List<IRule>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Config to manually set the location of main window
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            Window_Init();

            // Declare a DispatchTimer with a callback to a method to perform your save
            // Auto-save the data after 3s
            autoSaveTimer = new DispatcherTimer(); 
            autoSaveTimer.Interval = TimeSpan.FromSeconds(3.0);
            autoSaveTimer.Tick += PerformAutoSave;

            // Binding Title
            Title = projectName;

            // Set Item Source for filter ComboBox
            FilterBy.ItemsSource = typeof(Format).GetProperties().Select(p => p.Name);
            Closing += MainWindow_Closing;
        }

 
        public class Model
        {
            public string Name { get; set; }
            public ImageSource Icon { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            // declare a collection of item's types
            itemTypes = new BindingList<Model>()
            {
                new Model() {Name = "File", Icon = new BitmapImage(new Uri("icons/file.png",UriKind.Relative)) },
                new Model() {Name = "Folder", Icon = new BitmapImage(new Uri("icons/folder.png",UriKind.Relative)) },
            };

            TypeComboBox.ItemsSource = itemTypes;
            RuleComboxBox.ItemsSource = rules;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (!File.Exists(projectSavingAddress)) return;

            var loaded = MessageBox.Show(
                "Do you want to refresh your most recent project?", 
                "Autosave Performance", 
                MessageBoxButton.YesNo,
                MessageBoxImage.Question, 
                MessageBoxResult.Yes
            );

            if (loaded == MessageBoxResult.Yes)
                Project_Loaded(projectSavingAddress);
            else
            {
                // Do nothing
            }

            autoSaveTimer.Start();

        }

        private bool Window_Init()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var pluginFiles = new DirectoryInfo(path).GetFiles("Rules/*.dll");

            availableRules = (
                // From each file in the files.
                from file in pluginFiles
                    // Load the assembly.
                let asm = Assembly.LoadFile(file.FullName)
                // For every type in the assembly that is visible outside of
                // the assembly.
                from type in asm.GetExportedTypes()
                    // Where the type implements the interface.
                where typeof(IRule).IsAssignableFrom(type)
                // Create the instance.
                select (IRule)Activator.CreateInstance(type)
            // Materialize to a list.
            ).ToList();

            availableRules.ForEach(rule =>
            {
                rules.Add(rule);
                RuleFactory.Instance().Register(rule);
            });

            var lastUseProject = new LastUseProject();
            try
            {
                lastUseProject = JsonSerializer.Deserialize<LastUseProject>(File.ReadAllText(lastUseSavingAddress));
            }
            catch (JsonException JsonE)
            {
                MessageBox.Show("An error occurs when trying to parse the file", "Errors Handle");
                throw JsonE;
            }

            if (lastUseProject == null) return false;

            this.TypeComboBox.SelectedIndex = lastUseProject.Type;

            this.Top = lastUseProject.WindowPositionProperty.Top;
            this.Left = lastUseProject.WindowPositionProperty.Left;
            this.Width = lastUseProject.WindowPositionProperty.Width;
            this.Height = lastUseProject.WindowPositionProperty.Height;

           lastUseProject.Rule.ForEach(rule =>
            {
                IRule readRule = RuleFactory.Instance().RuleItem(rule);
                if (readRule != null)
                {
                    IRule cloneRule = readRule.Instance();
                    cloneRule.SetFormat((RuleFormat)rule.Clone());
                    selectedRules.Add(cloneRule);
                }
            });

            SelectedRulesListView.ItemsSource = selectedRules;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">
        // `file` can be specified with a path as an absolute path to 
        // read a file not in the debug or release folder</param>
        /// <returns></returns>
        private bool Project_Loaded(string file_path)
        {
            projectSavingAddress = file_path;
            
            if (file_path.Contains("\\")) projectName = file_path.Split('\\')[^1];

            var projectFormat = new ProjectFormat();
            try
            {
                projectFormat = JsonSerializer.Deserialize<ProjectFormat>(File.ReadAllText(file_path));
            }
            catch (JsonException JsonE)
            {
                MessageBox.Show("An error occurs when trying to parse the file", "Errors Handle");
                throw JsonE;
            }

            if (projectFormat == null) return false;

            selectedRules.Clear();
            listViewFiles.Clear();
            listViewFolders.Clear();

            projectFormat.Rule.ForEach(rule =>
            {
                IRule readRule = rules.Where(availableRule => availableRule.RuleName.Equals(rule.Type)).SingleOrDefault();
                if (readRule != null)
                {
                    IRule cloneRule = readRule.Instance();
                    cloneRule.SetFormat((RuleFormat)rule.Clone());
                    selectedRules.Add(cloneRule);
                }
            });
            
            
            bool isFileList = false;

            if(projectFormat.File.Count > 0)
            {
                foreach (var fileFormat in projectFormat.File)
                {
                    listViewFiles.Add((FileFormat)fileFormat.Clone());
                }

                isFileList = true;
            }
            else
            {
                if(projectFormat.Folder.Count > 0)
                {
                    foreach (var folderFormat in projectFormat.Folder)
                    {
                        listViewFolders.Add((FolderFormat)folderFormat.Clone());
                    }
                } else
                {
                    // Do Nothing
                }
            }
            
            ItemListView.ItemsSource = isFileList ? listViewFiles : listViewFolders;
            
            return true;
        }

        private bool SaveWorkingProject(string projectSavingAddress, string projectName, bool isAutoSaved = true)
        {
            var rules = new List<RuleFormat>();

            try
            {
                StreamWriter streamWriter = new StreamWriter(projectSavingAddress);

                foreach (var rule in selectedRules)
                {
                    rules.Add(new RuleFormat
                    {
                        Type = rule.RuleName,
                        Parameter = rule.GetFormat().Parameter,
                        Result = rule.GetFormat().Result
                    });
                }
                var files = (from file in listViewFiles select (FileFormat)file.Clone()).ToList();
                var folders = (from folder in listViewFolders select (FolderFormat)folder.Clone()).ToList();
               
                ProjectFormat projectFormat = new ProjectFormat
                {
                    Rule = rules,
                    File = files,
                    Folder = folders
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                streamWriter.Write(JsonSerializer.Serialize(projectFormat, options));
                
                if (isAutoSaved)
                {
                    StreamWriter intialWriter = new StreamWriter(lastUseSavingAddress);
                    var lastUseProject = new LastUseProject
                    {
                        Type = TypeComboBox.SelectedIndex,
                        WindowPositionProperty = new WindowPositionProperty
                        {
                            Height = this.Height,
                            Width = this.Width,
                            Top = this.Top,
                            Left = this.Left,
                        },
                        Rule = rules

                    };

                    intialWriter.Write(JsonSerializer.Serialize(lastUseProject, options));
                    intialWriter.Close();
                }

                streamWriter.Close();

                this.projectSavingAddress = projectSavingAddress;
                this.projectName = projectName;

                return true;
            }
            catch (IOException IOe)
            {
                throw IOe;
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {       
            e.Cancel = true;

            string messageTextBox = "";
            if (File.Exists(auto_save))
            {
                messageTextBox = "You have unsaved work/project. Do you want to save it before closing?";
                var result = MessageBox.Show(messageTextBox, "Batch Rename", MessageBoxButton.YesNoCancel,
                     MessageBoxImage.Question, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    string path = projectSavingAddress;
                    if (!string.IsNullOrEmpty(path))
                    {
                        var dialog = new SaveFileDialog();
                        dialog.Filter = "PROJ (*.proj)|*.proj";
                        if (dialog.ShowDialog() == true) path = dialog.FileName;
                    }
                    else
                    {
                        // Do Nothing
                    }

                    if (SaveWorkingProject(path, "",false))
                    {
                        if (File.Exists(auto_save))
                            File.Delete(auto_save);
                        Environment.Exit(0);
                    }
                }
                else if (result == MessageBoxResult.No)
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                var result = MessageBox.Show("Do you want to exit now?", "Close Application", MessageBoxButton.YesNo,
                    MessageBoxImage.Question, MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    SaveWorkingProject(projectSavingAddress, projectName, false);
                    Environment.Exit(0);
                }
            }

        }

        private void PerformAutoSave(object sender, EventArgs e)
        {
            SaveWorkingProject(projectSavingAddress, projectName);
        }

        private void AddRule(object sender, RoutedEventArgs e)
        {
            var index = RuleComboxBox.SelectedIndex;
            if (index == -1) return;
  
            selectedRules.Add(rules[index].Instance());
        }

        private void EditRule(object sender, RoutedEventArgs e)
        {
            var index = SelectedRulesListView.SelectedIndex;
            if (index == -1) return;

            var rule = selectedRules[index];

            if (rule.EditFormat())
            {
                var editWindow = rule.EditDialog();
                if (editWindow.ShowDialog() == true)
                    selectedRules[index].SetFormat(editWindow.RuleFormat);
            }
            else
            {
                MessageBox.Show("No parameter found in the rule! Please add valid parameter(s) to edit", "Error Message");
            }

            iView = CollectionViewSource.GetDefaultView(selectedRules);
            iView.Refresh();
        }
         
        private void SelectRule_DoubleClicked(object sender, MouseButtonEventArgs e)
        {
            EditRule(sender, e);
        }

        private void RemoveRule(object sender, RoutedEventArgs e)
        {
            var index = SelectedRulesListView.SelectedIndex;
            if (index == -1) return;
            
            selectedRules.RemoveAt(index);
        }
        
        private void ClearItem(object sender, RoutedEventArgs e)
        {
            string nameType = itemTypes[TypeComboBox.SelectedIndex].Name;
            if (nameType == "File") listViewFiles.RemoveAt(ItemListView.SelectedIndex);
            else if (nameType == "Folder") listViewFolders.RemoveAt(ItemListView.SelectedIndex);
            else return;
        }

        private void ResetRule(object sender, RoutedEventArgs e)
        {
            selectedRules.Clear();
        }

        private void ResetAddedItems(object sender, RoutedEventArgs e)
        {
            listViewFiles.Clear();
            listViewFolders.Clear();
        }

        private void MoveRuleToTop(object sender, RoutedEventArgs e)
        {
            var index = SelectedRulesListView.SelectedIndex;
            if (index == -1) return;

            selectedRules.Switch(0, index);
        }

        private void MoveRuleToBottom(object sender, RoutedEventArgs e)
        {
            var index = SelectedRulesListView.SelectedIndex;
            if (index == -1) return;

            selectedRules.Switch(index, selectedRules.Count - 1);
        }

        private void MoveRuleToPrev(object sender, RoutedEventArgs e)
        {
            var index = SelectedRulesListView.SelectedIndex;
            if (index == -1 || index == 0) return;

            selectedRules.Switch(index, index - 1);
        }

        private void MoveRuleToNext(object sender, RoutedEventArgs e)
        {
            var index = SelectedRulesListView.SelectedIndex;
            if (index == -1 || index == selectedRules.Count - 1) return;

            selectedRules.Switch(index, index + 1);
        }

        private void ImportNewItems_Clicked(object sender, RoutedEventArgs e)
        {
            int added_num = 0;

            if (TypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select the type of item (file/folder) to continue...", "Prerequisites");
                return;
            }

            if (TypeComboBox.SelectedItem.ToString().Equals("File"))
            {
                ItemListView.ItemsSource = listViewFiles;

                OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "All files (*.*)|*.*", Multiselect = true};

                if (openFileDialog.ShowDialog() == true)
                {
                    string[] files = openFileDialog.FileNames;
                    List<FileFormat> new_files = new List<FileFormat>();

                    foreach (string file in files)
                    {
                        bool isExisted = false;
                        string Name = Path.GetFileName(file);
                        string directoryPath = Path.GetDirectoryName(file);

                        foreach (var listViewFile in listViewFiles)
                        {
                            if (listViewFile.Name == Name && listViewFile.Path == directoryPath)
                            {
                                isExisted = true;
                                break;
                            }
                        }

                        if (!isExisted)
                        {
                            new_files.Add(new FileFormat() { Name = Name, Path = directoryPath });
                            added_num++;
                        }
                    }

                    foreach (var new_file in new_files)
                        listViewFiles.Add(new_file);
                }

                if (added_num > 0) MessageBox.Show(added_num + " files(s) added to the list view!!", "Success Message");
            }
            else if (TypeComboBox.SelectedItem.ToString().Equals("Folder"))
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                var result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    ItemListView.ItemsSource = listViewFolders;

                    string path = dialog.SelectedPath + "\\";
                    string[] folders = Directory.GetDirectories(path);
                    List<FolderFormat> new_folders = new List<FolderFormat>();

                    foreach (var folder in folders)
                    {
                        bool isExisted = false;
                        string Name = folder.Remove(0, path.Length);

                        foreach (var listViewFolder in listViewFolders)
                        {
                            if (listViewFolder.Name == Name && listViewFolder.Path == path)
                            {
                                isExisted = true;
                                break;
                            }
                        }

                        if (!isExisted)
                        {
                            new_folders.Add(new FolderFormat() { Name = Name, Path = path });
                            added_num++;
                        }
                    }

                    foreach (var new_folder in new_folders)
                        listViewFolders.Add(new_folder);

                    if (added_num > 0) MessageBox.Show(added_num + " folders(s) added to the list view!!", "Success Message");
                } 
            }

            view = (CollectionView)CollectionViewSource.GetDefaultView(ItemListView.ItemsSource);
            view.Filter = Filter();
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            bool error = false;

            if (selectedRules.Count == 0)
            {
                MessageBox.Show("No rules found to start batching! Please import some..", "Empty Collection");
                return;
            }

            if (listViewFiles.Count == 0 && listViewFolders.Count == 0)
            {
                MessageBox.Show("No file(s)/folder(s) found to start batching! Please import some..", "Empty Collection");
                return;
            }

            List<IRule> fileRename = new List<IRule>();
            List<IRule> folderRename = new List<IRule>();
            Dictionary<string, string> storeName = new Dictionary<string, string>();

            string newLocation = "";
            bool isMoved = false;

            if (RenameAndMoveToNewFolder.IsChecked == true)
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                var result = dialog.ShowDialog();

                string path = "";

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    path = dialog.SelectedPath;
                    isMoved = true;
                }

                newLocation = path;
            }

            if (listViewFiles.Count > 0)
            {
                foreach (IRule rule in selectedRules)
                {
                    fileRename.Add(rule.Instance());
                    folderRename.Add(rule.Instance());
                }

                foreach (var file in listViewFiles)
                {
                    file.ReName = file.Name;
                    file.Status = "";

                    if (!File.Exists(string.Join("/", new string[] { file.Path, file.Name })))
                    {
                        file.Status = "File Not Exist";
                        continue;
                    }

                    foreach (var resolver in fileRename)
                        file.ReName = resolver.ReName(file.ReName);

                    string location = (isMoved == true ? newLocation : file.Path) + file.ReName;

                    if (storeName.ContainsKey(location))
                    {
                        file.Status = "Duplicated File Name Found";
                        error = true;
                        continue;
                    }
                    else
                    {
                        storeName[location] = file.ReName;
                    }

                    string isValid = RuleHelper.Validate(file.ReName);

                    if (!string.IsNullOrEmpty(isValid))
                    {
                        file.Status = isValid;
                        error = true;
                    }
                }
            }
            else
            {
                foreach (var folder in listViewFolders)
                {
                    folder.ReName = folder.Name;
                    folder.Status = "";

                    if (!Directory.Exists(string.Join("/", new string[] { folder.Path, folder.Name })))
                    {
                        folder.Status = "Folder Not Exist";
                        continue;
                    }

                    foreach (var resolver in folderRename)
                        folder.ReName = resolver.ReName(folder.ReName, false);

                    string location = (isMoved == true ? newLocation : folder.Path) + folder.ReName;

                    if (storeName.ContainsKey(location))
                    {
                        folder.Status = "Duplicated Folder Name Found";
                        error = true;
                        continue;
                    }
                    else
                    {
                        storeName[location] = folder.ReName;
                    }

                    string isValid = RuleHelper.Validate(folder.ReName);
                    if (!string.IsNullOrEmpty(isValid))
                    {
                        folder.Status = isValid;
                        error = true;
                    }
                }
            }

            if(error)
            {
                MessageBox.Show("Name conflict in files/folders found! Try to change a new set of rules or remove the conficted ones", "Warning");
                return;
            }

            // If no error is found, then start batching 

            int itemCount = 0;
          
            if (listViewFiles.Count > 0)
            {
                foreach (var file in listViewFiles)
                {
                    if (isMoved)
                        File.Copy(file.Path + "/" + file.Name, newLocation + "/" + file.ReName);
                    else
                        File.Move(file.Path + "/" + file.Name, file.Path + "/" + file.ReName);

                    file.Name = file.ReName;

                    ++itemCount;
                    file.Status = "Success";
                }
            } else
            {
                foreach (var folder in listViewFolders)
                {
                    Directory.Move(folder.Path + "/" + folder.Name, folder.Path + "/" + folder.ReName);
                    folder.Name = folder.ReName;

                    ++itemCount;
                    folder.Status = "Success";
                }

            }

            MessageBox.Show($"{itemCount} items has been sucessfully changed into new names..", "Sucess Batch");
        }

        private void Preview(object sender, RoutedEventArgs e)
        {
            bool error = false;

            if (selectedRules.Count == 0)
            {
                MessageBox.Show("No rules found to preview batch! Please import some..", "Empty Collection");
                return;
            }

            if (listViewFiles.Count == 0 && listViewFolders.Count == 0)
            {
                MessageBox.Show("No file(s)/folder(s) found to preview batch! Please import some..", "Empty Collection");
                return;
            }
          
            List<IRule> fileRename = new List<IRule>();
            List<IRule> folderRename = new List<IRule>();
            Dictionary<string, string> storeName = new Dictionary<string, string>();

            if (listViewFiles.Count > 0)
            {
                foreach (IRule rule in selectedRules)
                {
                    fileRename.Add(rule.Instance());
                    folderRename.Add(rule.Instance());
                }

                foreach (var file in listViewFiles)
                {
                    file.ReName = file.Name;
                    file.Status = "";

                    if (!File.Exists(string.Join("/", new string[] { file.Path, file.Name })))
                    {
                        file.Status = "File Not Exist";
                        continue;
                    }

                    foreach (var resolver in fileRename)
                        file.ReName = resolver.ReName(file.ReName);

                    string location = file.Path + "/" + file.ReName;

                    if (storeName.ContainsKey(location))
                    {
                        file.Status = "Duplicated File Name Found";
                        error = true;
                        continue;
                    }
                    else
                    {
                        storeName[location] = file.ReName;
                    }

                    string isValid = RuleHelper.Validate(file.ReName);

                    if (!string.IsNullOrEmpty(isValid))
                    {
                        file.Status = isValid;
                        error = true;
                    }
                }
            }
            else
            {
                foreach (var folder in listViewFolders)
                {
                    folder.ReName = folder.Name;
                    folder.Status = "";

                    if (!Directory.Exists(string.Join("/", new string[] { folder.Path, folder.Name })))
                    {
                        folder.Status = "Folder Not Exist";
                        continue;
                    }

                    foreach (var resolver in folderRename)
                        folder.ReName = resolver.ReName(folder.ReName, false);

                    string location = folder.Path + folder.ReName;

                    if (storeName.ContainsKey(location))
                    {
                        folder.Status = "Duplicated Folder Name Found";
                        error = true;
                        continue;
                    }
                    else
                    {
                        storeName[location] = folder.ReName;
                    }

                    string isValid = RuleHelper.Validate(folder.ReName);
                    if (!string.IsNullOrEmpty(isValid))
                    {
                        folder.Status = isValid;
                        error = true;
                    }
                }
            }

            if (error)
            {
                MessageBox.Show("Name conflict in files/folders found! Try to change a new set of rules or remove the conficted ones", "Warning");
            }
            else
            {
                // Do Nothing
            }
        }

        private void SaveJSON(object sender, RoutedEventArgs e)
        {
            if (selectedRules.Count == 0)
            {
                MessageBox.Show("There are no selected rules to save.");
                return;
            }

            string path = currentPresetPath;
            if (string.IsNullOrEmpty(currentPresetPath))
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "JSON (*.json)|*.json";
                if (dialog.ShowDialog() == false) return;
                path = dialog.FileName;
            }
         

            List<RuleFormat> ruleJsonFormats = new List<RuleFormat>();

            foreach (var rule in selectedRules)
            {
                ruleJsonFormats.Add(new RuleFormat
                {
                    Type = rule.RuleName,
                    Parameter = rule.GetFormat().Parameter,
                    Result = rule.GetFormat().Result
                });
            }

            try
            {

                StreamWriter streamWriter = new StreamWriter(path);
                var options = new JsonSerializerOptions { WriteIndented = true };
                streamWriter.Write(JsonSerializer.Serialize(ruleJsonFormats, options));
                streamWriter.Close();

                MessageBox.Show($"Save Preset Successfully!", "Save preset");

                currentPresetPath = path;
            }
            catch (IOException IOe)
            {
                MessageBox.Show("Error to save preset..", "Error Handle");
                throw IOe;
            }
        }

        private void LoadJSON(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "JSON (*.json)|*.json";

            if (dialog.ShowDialog() == false) return;

            List<RuleFormat> ruleFormats = new List<RuleFormat>();
            string fileDialog = dialog.FileName;

            try
            {
                string preset = File.ReadAllText(fileDialog);
                ruleFormats = JsonSerializer.Deserialize<List<RuleFormat>>(preset);
            }
            catch (JsonException JsonE)
            {
                MessageBox.Show("Error to Parse the file..", "Error Handle");
                throw JsonE;
            }

            currentPresetPath = fileDialog;

            foreach (var ruleFormat in ruleFormats)
            {
                IRule rule = RuleFactory.Instance().RuleItem(ruleFormat);
                if (rule != null)
                {
                    IRule cloneRule = rule.Instance();
                    cloneRule.SetFormat(new RuleFormat
                    {
                        Type = ruleFormat.Type,
                        Parameter = ruleFormat.Parameter,
                        Result = ruleFormat.Result
                    });
                    
                    selectedRules.Add(cloneRule);
                }
            }
            
            MessageBox.Show("Load preset successfully!", "Load preset");
        }

        private void SaveProject(object sender, RoutedEventArgs e)
        {
            string path = projectSavingAddress;
            string projectName = "";
            if (string.IsNullOrEmpty(projectSavingAddress) || projectSavingAddress.Equals(auto_save))
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "PROJ (*.proj)|*.proj";

                if (dialog.ShowDialog() == false) return;
                path = dialog.FileName;

                projectName = path.Split('\\')[^1].Split('.')[0];
            }

            if (SaveWorkingProject(path, projectName, false))
            {
                Title = projectName;
            }
            else
            {
                // Do Nothing
            }
        }
        private void LoadProject(object sender, RoutedEventArgs e)
        {
         
            var dialog = new OpenFileDialog();
            dialog.Filter = "PROJ (*.proj)|*.proj";

            if (dialog.ShowDialog() == false) return;

            string path = dialog.FileName;

            if (Project_Loaded(path))
            {
                MessageBox.Show("Load Project Successfully!", "Load Project");
                if (File.Exists(auto_save)) File.Delete(auto_save);
                Title = projectName;
            }

            view = (CollectionView)CollectionViewSource.GetDefaultView(ItemListView.ItemsSource);
            view.Filter = Filter();
        }

        private void typeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (TypeComboBox.SelectedItem == null)
                return;
            else if (TypeComboBox.SelectedItem == "File")
                ItemListView.ItemsSource = listViewFiles;
            else if (TypeComboBox.SelectedItem == "Folder")
                ItemListView.ItemsSource = listViewFolders;
        }

        private void FilterBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemListView.ItemsSource == null) return;
            view = (CollectionView)CollectionViewSource.GetDefaultView(ItemListView.ItemsSource);
            view.Filter = Filter();
        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ItemListView.ItemsSource == null) return;
            CollectionViewSource.GetDefaultView(ItemListView.ItemsSource).Refresh();
           
        }

        public Predicate<object> Filter()
        {
            switch(FilterBy.SelectedItem as string)
            {
                case "Name":
                    return NameFilter;
                case "ReName":
                    return ReNameFilter;
                case "Path":
                    return PathFilter;
                case "Result":
                    return StatusFilter;
            }

            return NameFilter;
        }

        private bool NameFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return true;
            else
                return ((item as Format).Name.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool ReNameFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return true;
            else
            {
                var obj = (item as Format).ReName;
                if (obj == null) return false;
                else return obj.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0;
            }    
        }

        private bool PathFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return true;
            else
                return ((item as Format).Path.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool StatusFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return false;
            else
            {
                var obj = (item as Format).Status;
                if (obj == null) return false;
                else return obj.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        private void importNewItems_Dropped(object sender, DragEventArgs e)
        {
            int added_files_num = 0;

            if (TypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select the type of item (file/folder) to continue...", "Prerequisites");
                return;
            }
            if (TypeComboBox.SelectedItem.ToString().Equals("File"))
            {
                ItemListView.ItemsSource = listViewFiles;

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    List<FileFormat> new_files = new List<FileFormat>();
                    List<string> add_files = new List<string>();

                    foreach (string file in files)
                    {
                        FileAttributes attr = File.GetAttributes($"{file}");
                        if (attr.HasFlag(FileAttributes.Directory))
                        {
                            string[] inside_files = Directory.GetFiles($"{file}");
                            foreach (var inside_file in inside_files)
                            {
                                add_files.Add(inside_file);
                            }
                        }
                        else
                        {
                            add_files.Add(file);
                        }
                    }

                    foreach(var add_file in add_files)
                    {
                        bool isExisted = false;
                        string Name = Path.GetFileName(add_file);
                        string directoryPath = Path.GetDirectoryName(add_file);

                        foreach (var filename in listViewFiles)
                        {
                            if (filename.Name == Name && filename.Path == directoryPath)
                            {
                                isExisted = true;
                                break;
                            }
                        }

                        if (!isExisted)
                        {
                            new_files.Add(new FileFormat() { Name = Name, Path = directoryPath });
                            added_files_num++;
                        }
                    }

                    foreach (var new_file in new_files)
                        listViewFiles.Add(new_file);
                }

                if(added_files_num > 0) MessageBox.Show(added_files_num + " file(s) added to the list view!!", "Success Message");
            }
            else if (TypeComboBox.SelectedItem.ToString() == "Folder")
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    ItemListView.ItemsSource = listViewFolders;

                    string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                    List<DirectoryInfo> folders = new List<DirectoryInfo>();
                    
                    foreach(var path in paths)
                    {
                        folders.Add(new DirectoryInfo(path + @"\"));
                    }

                    List<FolderFormat> new_folders = new List<FolderFormat>();

                    foreach (var folder in folders)
                    {
                        bool isExisted = false;
                        string Name = folder.Name;

                        foreach (var listViewFolder in listViewFolders)
                        {
                            if (listViewFolder.Name == Name && listViewFolder.Path == folder.Parent.FullName + @"\")
                            {
                                isExisted = true;
                                break;
                            }
                        }

                        if (!isExisted)
                        {
                            new_folders.Add(new FolderFormat() { Name = Name, Path = folder.Parent.FullName + @"\" });
                            added_files_num++;
                        }
                    }

                    foreach (var new_folder in new_folders)
                        listViewFolders.Add(new_folder);

                    if (added_files_num > 0) MessageBox.Show(added_files_num + " folders(s) added to the list view!!", "Success Message");
                }
            }

            view = (CollectionView)CollectionViewSource.GetDefaultView(ItemListView.ItemsSource);
            view.Filter = Filter();
        }
    }
}
