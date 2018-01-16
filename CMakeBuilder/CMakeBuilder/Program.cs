using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CMakeBuilder
{
  class Program
  {
    static void DoDirectory(string aDirectory)
    {
      var sourceTypes = new SortedSet<string>();
      sourceTypes.Add(".cpp");
      sourceTypes.Add(".cc");
      sourceTypes.Add(".c");

      var headerTypes = new SortedSet<string>();
      headerTypes.Add(".hpp");
      headerTypes.Add(".hh");
      headerTypes.Add(".h");
      headerTypes.Add(".inl");

      var directories = Directory.GetDirectories(aDirectory);

      string workingDirectory = Path.GetFullPath(aDirectory).TrimEnd(Path.DirectorySeparatorChar);
      string workingDirectoryName = workingDirectory.Split(Path.DirectorySeparatorChar).Last();

      foreach (var directory in directories)
      {
        var privateSources = new List<string>();
        var publicSources = new List<string>();
        string directoryName = Path.GetDirectoryName(directory);

        StringBuilder system = new StringBuilder();

        system.Append("target_sources(");
        system.Append(workingDirectoryName);
        system.Append("\n");

        string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
          var relativeFile = Path.GetRelativePath(directory, file).Replace('\\', '/');
          string fileName = Path.GetFileName(file);

          var extension = Path.GetExtension(fileName);

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
          System.IO.File.WriteAllText(Path.Combine(directory, "CMakeLists.txt"), "\n");
        }
        else
        {
          System.IO.File.WriteAllText(Path.Combine(directory, "CMakeLists.txt"), system.ToString());
        }
      }

      //System.IO.File.WriteAllText("CMakeLists.txt", "");
    }

    static void Main(string[] args)
    {
      DoDirectory("C:/Users/playm/Documents/GitKraken/LambPlanet/Dependencies/YTE/Dependencies/assimp");
      DoDirectory("C:/Users/playm/Documents/GitKraken/LambPlanet/Dependencies/YTE/Dependencies/Bullet");
      //DoDirectory("C:/Users/playm/Documents/GitKraken/LambPlanet/Dependencies/YTE/Dependencies/LuaJIT");
    }
  }
}