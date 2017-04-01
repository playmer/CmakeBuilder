using System;
using System.IO;
using System.Linq;
using System.Text;

namespace CMakeBuilder
{
  class Program
  {
    static void Main(string[] args)
    {
      var directories = Directory.GetDirectories(Directory.GetCurrentDirectory());

      string workingDirectory = Path.GetFullPath(Directory.GetCurrentDirectory()).TrimEnd(Path.DirectorySeparatorChar);
      string workingDirectoryName = workingDirectory.Split(Path.DirectorySeparatorChar).Last();

      string projectSource = string.Concat(workingDirectoryName, "_Source");
      string oldProjectSource = string.Concat("Previous_", workingDirectoryName, "_Source");

      foreach (var directory in directories)
      {
        string directoryName = Path.GetDirectoryName(directory);

        StringBuilder system = new StringBuilder();

        system.Append("Set(");
        system.Append(oldProjectSource);
        system.Append(" ${");
        system.Append(projectSource);
        system.Append("})\n\n");

        string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
          string fileName = Path.GetFileName(file);

          system.Append("AddFile(");
          system.Append(projectSource);
          system.Append(" ");
          system.Append(oldProjectSource);
          system.Append(" ");
          system.Append(fileName);
          system.Append(")\n");
        }

        system.Append("\nSet(");
        system.Append(projectSource);
        system.Append(" ${");
        system.Append(oldProjectSource);
        system.Append("} PARENT_SCOPE)\n");

        Console.WriteLine(system.ToString());
        //System.IO.File.WriteAllText("CMakeLists.txt", system.ToString());
      }

      //System.IO.File.WriteAllText("CMakeLists.txt", "");
    }
  }
}