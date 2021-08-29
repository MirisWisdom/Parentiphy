/**
 * Copyright (C) 2021 Emilian Roman
 * 
 * This file is part of Parentiphy.
 * 
 * Parentiphy is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or
 * (at your option) any later version.
 * 
 * Parentiphy is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Parentiphy.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.IO;
using Mono.Options;
using static System.Console;
using static System.IO.Path;
using static System.IO.SearchOption;
using static System.Threading.Tasks.Parallel;

namespace Parentiphy
{
  public class Program
  {
    private static DirectoryInfo For { get; set; } /* for a single provided directory                 */
    private static DirectoryInfo All { get; set; } /* for subdirectories within a specified directory */

    public static OptionSet OptionSet { get; set; } = new()
    {
      {"for=", "remove redundant paths for the provided specific dir", s => For = new DirectoryInfo(s)},
      {"all=", "remove redundant paths in ALL top dirs at the provided dir", s => All = new DirectoryInfo(s)}
    };

    /// <summary>
    ///   Parentiphy entry.
    /// </summary>
    /// <param name="args">
    ///   CLI args.
    /// </param>
    public static void Main(string[] args)
    {
      /**
       * Removes redundant paths, by conditionally merging the given base directory with its redundant directory.
       *
       * This is similar to:
       * 
       * 1.  WinRAR's "Remove redundant folders from extraction path." option; or
       * 2.  Ark's (through Dolphin) "Extract here, Autodetect Subfolder" procedure.
       *
       * All files and directories within the found redundant directory will be moved to the root of the base directory.
       * As a bonus, the base directory will also have its timestamp set to the discovered redundant directory.
       */

      static void Merge(DirectoryInfo baseDirectory)
      {
        var baseFiles       = baseDirectory.GetFiles("*", TopDirectoryOnly);
        var baseDirectories = baseDirectory.GetDirectories("*", TopDirectoryOnly);

        /**
         * Skip directory if no redundancies are discovered. Redundancy is assumed when the inbound directory has:
         *
         * 1.  NO top-level files; AND
         * 2.  ONE top-level directory.
         */

        if (baseFiles.Length != 0 || baseDirectories.Length != 1)
          return;

        var redundantDirectory = baseDirectories[0];
        var pendingFiles       = redundantDirectory.GetFiles("*", TopDirectoryOnly);
        var pendingDirectories = redundantDirectory.GetDirectories("*", TopDirectoryOnly);
        var realLastWrite      = redundantDirectory.LastWriteTimeUtc;

        /**
         * We move all of the files in the top-level directory to the given base directory.
         */

        foreach (var file in pendingFiles)
          file.MoveTo(Combine(baseDirectory.FullName, file.Name));

        /**
         * We carry out the same procedure for the top-level directories.
         */

        foreach (var directory in pendingDirectories)
          directory.MoveTo(Combine(baseDirectory.FullName, directory.Name));

        /**
         * If the inferred redundant directory is empty - which it should be after a successful procedure - we will
         * delete it. This marks the end of any directory/file manipulation, meaning that we can simply set the base
         * directory's timestamp to the one that the redundant directory used to have.
         */

        redundantDirectory.Refresh();

        if (redundantDirectory.GetFiles("*", AllDirectories).Length == 0)
          redundantDirectory.Delete();

        baseDirectory.LastWriteTimeUtc = realLastWrite;
      }

      OptionSet.WriteOptionDescriptions(Out);
      OptionSet.Parse(args);

      if (For != null)
        Merge(For);

      if (All == null)
        return;

      /**
       * Handle all of the subdirectories - within the given directory - in parallel.
       */

      ForEach(All.GetDirectories("*", TopDirectoryOnly), Merge);
    }
  }
}
