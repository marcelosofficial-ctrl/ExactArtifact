using System.IO;
using System.Windows;
using System.Windows.Media;
using ExactArtifact.Core;
using Microsoft.Win32;

namespace ExactArtifact.Gui;

public partial class MainWindow : Window
{
    private readonly FileHasher _fileHasher = new();
    private readonly ManifestService _manifestService = new();
    private string? _currentHash;
    private CancellationTokenSource? _fileCts;
    private CancellationTokenSource? _manifestCts;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void BrowseFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose a file to hash"
        };

        if (dialog.ShowDialog(this) == true)
        {
            await SelectAndHashFileAsync(dialog.FileName);
        }
    }

    private async void Window_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        var paths = e.Data.GetData(DataFormats.FileDrop) as string[];

        if (paths is null || paths.Length == 0)
        {
            return;
        }

        if (File.Exists(paths[0]))
        {
            await SelectAndHashFileAsync(paths[0]);
            return;
        }

        if (Directory.Exists(paths[0]))
        {
            FolderPathBox.Text = paths[0];
            ManifestPathBox.Text = Path.Combine(
                paths[0],
                "ExactArtifact.manifest.json");
        }
    }

    private async void Hash_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FilePathBox.Text))
        {
            SetFileStatus("SELECT A FILE", FindBrush("WarningBrush"));
            return;
        }

        await HashSelectedFileAsync();
    }

    private void CancelHash_Click(object sender, RoutedEventArgs e)
    {
        _fileCts?.Cancel();
    }

    private void CopyHash_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentHash))
        {
            return;
        }

        Clipboard.SetText(_currentHash);
        SetFileStatus("HASH COPIED", FindBrush("SuccessBrush"));
    }

    private void ExpectedHashBox_TextChanged(
        object sender,
        System.Windows.Controls.TextChangedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_currentHash))
        {
            UpdateVerificationState();
        }
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Choose the folder to describe or verify",
            Multiselect = false
        };

        if (dialog.ShowDialog(this) == true)
        {
            FolderPathBox.Text = dialog.FolderName;
            ManifestPathBox.Text = Path.Combine(
                dialog.FolderName,
                "ExactArtifact.manifest.json");
        }
    }

    private void BrowseManifest_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose an ExactArtifact manifest",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) == true)
        {
            ManifestPathBox.Text = dialog.FileName;
        }
    }

    private async void CreateManifest_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateManifestInputs())
        {
            return;
        }

        _manifestCts?.Dispose();
        _manifestCts = new CancellationTokenSource();

        SetManifestBusy(true, "CREATING");

        try
        {
            var manifest = await _manifestService.CreateAsync(
                FolderPathBox.Text,
                ManifestPathBox.Text,
                _manifestCts.Token);

            ManifestStatusLabel.Text = "CREATED";
            ManifestStatusLabel.Foreground = FindBrush("SuccessBrush");
            ManifestResultBox.Text =
                $"{manifest.Files.Count} files recorded.{Environment.NewLine}" +
                $"SHA-256 manifest saved to:{Environment.NewLine}" +
                Path.GetFullPath(ManifestPathBox.Text);
        }
        catch (OperationCanceledException)
        {
            ManifestStatusLabel.Text = "CANCELLED";
            ManifestStatusLabel.Foreground = FindBrush("WarningBrush");
            ManifestResultBox.Text = "Manifest creation was cancelled.";
        }
        catch (Exception ex)
        {
            ManifestStatusLabel.Text = "ERROR";
            ManifestStatusLabel.Foreground = FindBrush("ErrorBrush");
            ManifestResultBox.Text = ex.Message;
        }
        finally
        {
            SetManifestBusy(false, null);
            _manifestCts?.Dispose();
            _manifestCts = null;
        }
    }

    private async void VerifyManifest_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateManifestInputs())
        {
            return;
        }

        _manifestCts?.Dispose();
        _manifestCts = new CancellationTokenSource();

        SetManifestBusy(true, "VERIFYING");

        try
        {
            var result = await _manifestService.VerifyAsync(
                FolderPathBox.Text,
                ManifestPathBox.Text,
                _manifestCts.Token);

            if (result.IsExactMatch)
            {
                ManifestStatusLabel.Text = "EXACT MATCH";
                ManifestStatusLabel.Foreground = FindBrush("SuccessBrush");
                ManifestResultBox.Text =
                    $"{result.MatchedCount}/{result.ExpectedCount} expected files match exactly.{Environment.NewLine}" +
                    "0 modified | 0 missing | 0 unexpected";
                return;
            }

            ManifestStatusLabel.Text = "MISMATCH";
            ManifestStatusLabel.Foreground = FindBrush("WarningBrush");

            var lines = new List<string>
            {
                $"{result.MatchedCount}/{result.ExpectedCount} expected files match.",
                $"{result.Modified.Count} modified | {result.Missing.Count} missing | {result.Unexpected.Count} unexpected",
                ""
            };

            lines.AddRange(result.Modified.Select(path => $"MODIFIED   {path}"));
            lines.AddRange(result.Missing.Select(path => $"MISSING    {path}"));
            lines.AddRange(result.Unexpected.Select(path => $"UNEXPECTED {path}"));

            ManifestResultBox.Text = string.Join(Environment.NewLine, lines);
        }
        catch (OperationCanceledException)
        {
            ManifestStatusLabel.Text = "CANCELLED";
            ManifestStatusLabel.Foreground = FindBrush("WarningBrush");
            ManifestResultBox.Text = "Manifest verification was cancelled.";
        }
        catch (Exception ex)
        {
            ManifestStatusLabel.Text = "ERROR";
            ManifestStatusLabel.Foreground = FindBrush("ErrorBrush");
            ManifestResultBox.Text = ex.Message;
        }
        finally
        {
            SetManifestBusy(false, null);
            _manifestCts?.Dispose();
            _manifestCts = null;
        }
    }

    private void CancelManifest_Click(object sender, RoutedEventArgs e)
    {
        _manifestCts?.Cancel();
    }

    private async Task SelectAndHashFileAsync(string path)
    {
        FilePathBox.Text = path;
        FileNameLabel.Text = Path.GetFileName(path);
        await HashSelectedFileAsync();
    }

    private async Task HashSelectedFileAsync()
    {
        _fileCts?.Dispose();
        _fileCts = new CancellationTokenSource();

        _currentHash = null;
        CopyHashButton.IsEnabled = false;
        HashButton.IsEnabled = false;
        CancelHashButton.IsEnabled = true;
        FileProgress.Visibility = Visibility.Visible;
        SetFileStatus("HASHING", FindBrush("AccentBrush"));
        HashLabel.Text = "Calculating SHA-256...";
        FileMetaLabel.Text = string.Empty;

        try
        {
            var result = await _fileHasher.ComputeDetailedAsync(
                FilePathBox.Text,
                _fileCts.Token);

            _currentHash = result.Sha256;
            HashLabel.Text = result.Sha256;
            FileMetaLabel.Text =
                $"{FormatBytes(result.Length)} | Modified {result.LastWriteTimeUtc.ToLocalTime():yyyy-MM-dd HH:mm}";
            CopyHashButton.IsEnabled = true;
            UpdateVerificationState();
        }
        catch (OperationCanceledException)
        {
            HashLabel.Text = "Hash operation cancelled.";
            FileMetaLabel.Text = string.Empty;
            SetFileStatus("CANCELLED", FindBrush("WarningBrush"));
        }
        catch (Exception ex)
        {
            HashLabel.Text = ex.Message;
            FileMetaLabel.Text = string.Empty;
            SetFileStatus("ERROR", FindBrush("ErrorBrush"));
        }
        finally
        {
            HashButton.IsEnabled = true;
            CancelHashButton.IsEnabled = false;
            FileProgress.Visibility = Visibility.Collapsed;
            _fileCts?.Dispose();
            _fileCts = null;
        }
    }

    private void UpdateVerificationState()
    {
        if (string.IsNullOrWhiteSpace(_currentHash))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(ExpectedHashBox.Text))
        {
            SetFileStatus("HASH READY", FindBrush("SuccessBrush"));
            return;
        }

        try
        {
            var matches = HashVerifier.IsMatch(
                _currentHash,
                ExpectedHashBox.Text);

            SetFileStatus(
                matches ? "EXACT MATCH" : "MISMATCH",
                matches
                    ? FindBrush("SuccessBrush")
                    : FindBrush("ErrorBrush"));
        }
        catch (FormatException)
        {
            SetFileStatus(
                "EXPECTED HASH INVALID",
                FindBrush("WarningBrush"));
        }
    }

    private bool ValidateManifestInputs()
    {
        if (string.IsNullOrWhiteSpace(FolderPathBox.Text))
        {
            ManifestStatusLabel.Text = "SELECT A FOLDER";
            ManifestStatusLabel.Foreground = FindBrush("WarningBrush");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ManifestPathBox.Text))
        {
            ManifestStatusLabel.Text = "SELECT A MANIFEST";
            ManifestStatusLabel.Foreground = FindBrush("WarningBrush");
            return false;
        }

        return true;
    }

    private void SetManifestBusy(bool busy, string? status)
    {
        CreateManifestButton.IsEnabled = !busy;
        VerifyManifestButton.IsEnabled = !busy;
        CancelManifestButton.IsEnabled = busy;

        if (status is not null)
        {
            ManifestStatusLabel.Text = status;
            ManifestStatusLabel.Foreground = FindBrush("AccentBrush");
        }
    }

    private void SetFileStatus(string text, Brush brush)
    {
        FileStatusLabel.Text = text;
        FileStatusLabel.Foreground = brush;
    }

    private Brush FindBrush(string key)
    {
        return (Brush)FindResource(key);
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var value = (double)bytes;
        var unit = 0;

        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.##} {units[unit]}";
    }
}