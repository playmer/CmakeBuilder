using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CMakeBuilder
{
  class CMakeListWriter
  {
    public List<string> Blacklist = new List<string>();
    // the bool is to keep track of if the directory is going to ignore the blacklist or not
    public List<Tuple<string, bool>> DirectoryList = new List<Tuple<string, bool>>();

    public bool ContainsDirectoryOnBlacklist(string aDirectory)
    {
      if (this.Blacklist.Count == 0)
      {
        return false;
      }

      int indexOfFileName = aDirectory.LastIndexOf(Path.DirectorySeparatorChar);

      String trimmedDirectory;

      if (aDirectory.Contains("."))
      {
        trimmedDirectory = aDirectory.Remove(indexOfFileName);
      }
      else
      {
        trimmedDirectory = aDirectory;
      }

      foreach (var blacklistedFolder in this.Blacklist)
      {

        if (trimmedDirectory.Contains(blacklistedFolder))
        {
          return true;
        }
      }

      return false;
    }

    public void CreateCMakeListFileIfNoneExists(string aDirectory)
    {
      var files = Directory.GetFiles(aDirectory);

      foreach (var file in files)
      {
        if (file.Contains("CMakeLists.txt"))
        {
          return;
        }
      }

      // create the file that will be for user
      StringBuilder system = new StringBuilder();

      system.Append("################################################################################\n");
      system.Append("# Add any files that need to be manually added in this file.\n");
      system.Append("# Link: https://github.com/playmer/CmakeBuilder \n");
      system.Append("################################################################################\n");
      system.Append("#Adds generated list, do not remove line unless you are sure you want to ignore the generated file.\n");
      system.Append("include(${CMAKE_CURRENT_LIST_DIR}/Generated.cmake)\n");

      // save out the file
      System.IO.File.WriteAllText(Path.Combine(aDirectory, "CMakeLists.txt"), system.ToString());
    }

    public void AddDirectory(string aDirectory, bool ignoreBlacklist = false)
    {
      this.DirectoryList.Add(new Tuple<String,bool>(aDirectory, ignoreBlacklist));
    }

    public void AddToBlacklist(string aDirOrFile)
    {
      this.Blacklist.Add(aDirOrFile);
    }

    public void DoDirectories()
    {
      foreach (var directory in this.DirectoryList)
      {
        DoDirectory(directory.Item1, directory.Item2);
      }
    }

    public void DoDirectory(string aDirectory, bool ignoreBlacklist)
    {
      var sourceTypes = new SortedSet<string>();
      sourceTypes.Add(".cpp");
      sourceTypes.Add(".cxx");
      sourceTypes.Add(".cc");
      sourceTypes.Add(".c");

      var headerTypes = new SortedSet<string>();
      headerTypes.Add(".hpp");
      headerTypes.Add(".hxx");
      headerTypes.Add(".hh");
      headerTypes.Add(".h");
      headerTypes.Add(".inl");

      var directories = Directory.GetDirectories(aDirectory);

      string workingDirectory = Path.GetFullPath(aDirectory).TrimEnd(Path.DirectorySeparatorChar);
      string workingDirectoryName = workingDirectory.Split(Path.DirectorySeparatorChar).Last();

      foreach (var directory in directories)
      {
        // skip any directories that contain the blacklisted directories
        if (!ignoreBlacklist && ContainsDirectoryOnBlacklist(directory))
        {
          continue;
        }

        CreateCMakeListFileIfNoneExists(directory);

        var privateSources = new List<string>();
        var publicSources = new List<string>();

        string directoryName = Path.GetDirectoryName(directory);

        string currentDirectoryName = directory.Split(Path.DirectorySeparatorChar).Last();


        StringBuilder system = new StringBuilder();

        system.Append("################################################################################\n");
        system.Append("# Generated using Joshua T. Fisher's 'CMake Builder'.\n");
        system.Append("# Link: https://github.com/playmer/CmakeBuilder \n");
        system.Append("################################################################################\n");

        system.Append("target_sources(");
        system.Append(currentDirectoryName);
        system.Append("\n");

        string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
          var relativeFile = Path.GetRelativePath(directory, file).Replace('\\', '/');

          // skip any directories that contain the blacklisted directories
          if (ContainsDirectoryOnBlacklist(file))
          {
            continue;
          }

          string fileName = Path.GetFileName(file);

          var extension = Path.GetExtension(fileName);

          if (extension == ".cxx")
            Console.WriteLine("found it.");

          if (sourceTypes.Contains(extension))
          {
            privateSources.Add(relativeFile);
          }
          else if (headerTypes.Contains(extension))
          {
            publicSources.Add(relativeFile);
          }
        }

        system.Append("  PRIVATE\n");

        foreach (var file in privateSources)
        {
          system.Append("    ${CMAKE_CURRENT_LIST_DIR}/");
          system.Append(file);
          system.Append("\n");
        }

        system.Append("#  PUBLIC\n");

        foreach (var file in publicSources)
        {
          system.Append("    ${CMAKE_CURRENT_LIST_DIR}/");
          system.Append(file);
          system.Append("\n");
        }

        system.Append(")\n");

        Console.WriteLine(system.ToString());

        if (0 == publicSources.Count && 0 == privateSources.Count)
        {
          System.IO.File.WriteAllText(Path.Combine(directory, "Generated.cmake"), "\n");
        }
        else
        {
          System.IO.File.WriteAllText(Path.Combine(directory, "Generated.cmake"), system.ToString());
        }
      }
    }
  }
}
