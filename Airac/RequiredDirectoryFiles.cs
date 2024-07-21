using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RequiredDirectoryFiles
{
    public RequiredDirectoryFiles(List<string> _RequiredFiles, Dictionary<string, RequiredDirectoryFiles> _RequiredFilesInFolders)
    {
        RequiredFiles = _RequiredFiles;
        RequiredFilesInFolders = _RequiredFilesInFolders;
    }

    public IReadOnlyList<string> RequiredFiles { get; private set; } = new List<string>();
    public Dictionary<string, RequiredDirectoryFiles> RequiredFilesInFolders { get; private set; } = new Dictionary<string, RequiredDirectoryFiles>();

    public bool CheckForRequiredFiles(DirectoryInfo Info)
    {
        FileInfo[] Files = Info.GetFiles();
        DirectoryInfo[] Directories = Info.GetDirectories();
        List<string> FileNames = Files.Select(X => X.Name).ToList();
        List<string> DirectoriesNames = Directories.Select(X => X.Name).ToList();

        if (Files.Length < RequiredFiles.Count || Directories.Length < RequiredFilesInFolders.Count)
            return false;

        foreach (string FileName in RequiredFiles)
        {
            if (!FileNames.Contains(FileName))
                return false;
        }

        foreach (string Directory in RequiredFilesInFolders.Keys)
        {
            if (!DirectoriesNames.Contains(Directory))
                return false;
        }

        return true;
    }
}
