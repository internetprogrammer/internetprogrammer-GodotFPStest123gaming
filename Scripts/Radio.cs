using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public partial class Radio : Node3D
{
    string[] directories;
    List<List<string>> FileList;
    const string path = "res://Radio/";
    public override void _Ready()
    {

        DirAccess directory = DirAccess.Open(path);

        directories = InitFolders(directory);
        FileList = InitFiles(directory,directories);
        GD.Print(GetFileAddress(1, 2));
        AudioHandler test = new AudioHandler(GetFileAddress(1,2));
        AddChild(test);
        //test.Play();
    }
    string[] InitFolders(DirAccess directory)
    {
        directory.ListDirBegin();
        if(directory is not null)
        {
            string[] directories = directory.GetDirectories();
            return directories;
        }
        return [];
    }
    List<List<string>> InitFiles(DirAccess directory,string[] directories)
    {
        List<List<string>> FileList = new List<List<string>>();
        foreach (string d in directories)
        {
            DirAccess dir = DirAccess.Open(path + d +"/");
            string [] files = dir.GetFiles();
            List <string> filteredFiles = [];
            foreach (string f in files)
            {
                if (f.EndsWith(".mp3"))
                {
                    filteredFiles.Add(f);
                }
            }
 
            FileList.Add(filteredFiles);

        }
        return FileList;
    }
    public int GetFolderLength()
    {
        if (directories != null)
        {
            return directories.Length;
        }
        GD.PrintErr("Directory doesnt exist!");
        return 0;
    }
    public int GetFileLength(int index)
    {
        List<string> files = FileList[index];
        if (files != null)
        {
            return files.Count;
        }
        GD.PrintErr("Index doesnt exist!");
        return 0;
    }
    public string GetFolder(int index)
    {
        if (directories[index] != "")
        {
            return directories[index];
        }
        GD.PrintErr("Index doesnt exist!");
        return "";
    }

    public string GetFile(int x,int y)
    {
        List<string> files = FileList[x];
        if (FileList[x] != null)
        {
            if (files[y] is not null) return files[y];
        }
        GD.PrintErr("Index doesnt exist!");
        return "";
    }
    public string GetFileAddress(int x, int y)
    {
        List<string> files = FileList[x];
        if (FileList[x] != null)
        {
            if (files[y] is not null) return path + directories[x] + "/" + files[y] + "/";
        }
        GD.PrintErr("Index doesnt exist!");
        return "";
    }
}
