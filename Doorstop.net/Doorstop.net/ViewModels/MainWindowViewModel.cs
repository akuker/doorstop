using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Doorstop.net.Models;
using System.Windows;

namespace Doorstop.net.ViewModels
{
  public class MainWindowViewModel : DoorstopBaseViewModel
  {

    Dictionary<String, Views.DocumentView> openWindows = new Dictionary<string, Views.DocumentView>();

    private string requirementsRepoPath;
    // Path of the currently selected Doorstop repository
    public string RequirementsRepoPath
    {
      get { return requirementsRepoPath; }
      set
      {
        if (requirementsRepoPath != value)
        {
          requirementsRepoPath = value;
          NotifyPropertyChanged();
          Properties.Settings.Default.RepoPath = value;
        }
      }
    }

    private ObservableCollection<RequirementsDocument> directoryStructure;

    public ObservableCollection<RequirementsDocument> DirectoryStructure
    {
      get { return directoryStructure; }
      set {
        directoryStructure = value;
        NotifyPropertyChanged();
      }
    }


    #region commands
    public ICommand OpenRepoCommand { get; set; }
    public ICommand ReloadRepoCommand { get; set; }
    public ICommand OpenDocumentCommand { get; set; }
    public ICommand CleanupCommand { get; set; }
    public ICommand LaunchExplorerCommand { get; set; }

    public ICommand DebugButtonCommand { get; set; }
    #endregion



    public MainWindowViewModel()
    {
      directoryStructure = new ObservableCollection<RequirementsDocument>();
      directoryStructure.Add(new RequirementsDocument {FullPath="Not loaded....", ShortName = "Not loaded...."});

      OpenRepoCommand = new DelegateCommand<string>(ExecuteOpenRepo, (z) => { return true; });
      ReloadRepoCommand = new DelegateCommand<string>(ExecuteReloadTree, (z) => { return true; });
      OpenDocumentCommand = new DelegateCommand<string>(ExecuteOpenDocument, (z) => { return true; });
      CleanupCommand = new DelegateCommand<string>(ExecuteCleanup, (z) => { return true; });
      LaunchExplorerCommand = new DelegateCommand<string>(ExecuteLaunchExplorer, (z) => { return true; });
      DebugButtonCommand = new DelegateCommand<string>(ExecuteDebugFunction, (z) => { return true; });

      if ((Properties.Settings.Default.RepoPath != null) && (Properties.Settings.Default.RepoPath.Length > 0))
      {
        RequirementsRepoPath = Properties.Settings.Default.RepoPath;
        ExecuteReloadTree();
      }

    }

    /// <summary>
    /// Test function that is only used for debugging. Should be deleted at some point!
    /// </summary>
    /// <param name="bla"></param>
    public void ExecuteDebugFunction(string path = null)
    {
      Document myNewDoc = Document.Load(@"C:\Users\theto\source\repos\doorstop.net\reqs\tutorial\.doorstop.yml");
      Item myItem = Item.Load(@"C:\Users\theto\source\repos\doorstop.net\reqs\tutorial\TUT004.yml", myNewDoc);
      Views.ItemEditor itemEditor = new Views.ItemEditor(myItem);
      itemEditor.ShowDialog();
    }


    private void ExecuteLaunchExplorer(string path=null)
    {
      if ((path == null) || (path.Length < 1))
        path = RequirementsRepoPath;

      if(System.IO.File.Exists(path))
      {
        path = System.IO.Path.GetDirectoryName(path);
      }

      System.Diagnostics.Process.Start(path);
    }

    private void ExecuteOpenDocument(string path)
    {
      Views.DocumentView docWindow = null;

          if (openWindows.ContainsKey(path) && (openWindows[path].DataContext != null))
          {
            docWindow = openWindows[path];
            docWindow.Activate();
          }
          else
          {
            docWindow = new Views.DocumentView(path);
            openWindows[path] = docWindow;
            docWindow.Show();
          }

        }

    public void ExecuteCleanup(string junk="")
    {
        // Close all of the other windows
      foreach (var curWindow in this.openWindows.Values)
      {
        curWindow.Close();
      }
      // Save Settings
      Properties.Settings.Default.Save();

    }

    private void ExecuteReloadTree(string Path = null)
    {
      this.DirectoryStructure.Clear();
      if (Path == null)
        Path = this.RequirementsRepoPath;
      else
        Path = System.IO.Path.GetDirectoryName(Path);
      if ((Path == null) || (Path.Length < 1))
      {
        Logger.Warning("Invalid Requirements Repo directory defined. Empty string");
        return;
      }
      string fullPath = System.IO.Path.GetFullPath(Path);
      var directoryTree = new RequirementsFolder { FullPath = fullPath};
      directoryTree.LoadChildren();
      foreach (var child in directoryTree.Children)
      {
        this.DirectoryStructure.Add(child);
      }
    }



    private void ExecuteOpenRepo(object obj)
    {
      var openDirectoryDialog = new OpenFileDialog();
      if (System.IO.Directory.Exists(RequirementsRepoPath))
      {
        openDirectoryDialog.InitialDirectory = RequirementsRepoPath;
      }
      openDirectoryDialog.Multiselect = false;
      openDirectoryDialog.DefaultExt = ".yml";
      if(openDirectoryDialog.ShowDialog() == true)
      {
        if(System.IO.File.Exists(openDirectoryDialog.FileName))
        {
          RequirementsRepoPath = System.IO.Path.GetDirectoryName(openDirectoryDialog.FileName);
          ExecuteReloadTree();
        }

      }
    }

    private void ExecuteValidate(object obj)
    {

    }
    private void ExecuteAddRequirement(object obj)
    {

    }
    private void ExecutePublishAll(object obj)
    {

    }



  }

  public class DelegateCommand<T> : System.Windows.Input.ICommand where T : class
  {
    private readonly Predicate<T> _canExecute;
    private readonly Action<T> _execute;

    public DelegateCommand(Action<T> execute)
        : this(execute, null)
    {
    }

    public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
    {
      _execute = execute;
      _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
      if (_canExecute == null)
        return true;

      return _canExecute((T)parameter);
    }

    public void Execute(object parameter)
    {
      _execute((T)parameter);
    }

    public event EventHandler CanExecuteChanged;
    public void RaiseCanExecuteChanged()
    {
      if (CanExecuteChanged != null)
        CanExecuteChanged(this, EventArgs.Empty);
    }
  }


}
