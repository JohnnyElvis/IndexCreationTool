// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//From https://github.com/microsoft/winget-cli/tree/v1.2.10271
//Uses WingetUtil.dll extracted from wingetcreate

namespace IndexCreationTool
{
    using System;
    using System.Diagnostics;
    using System.IO;

    class Program
    {
        public const string IndexName = @"index.db";
        public const string IndexPathInPackage = @"Public\index.db";
        public const string IndexPackageName = @"source.msix";

        static void Main(string[] args)
        {
            string rootDir = string.Empty;
            string appxManifestPath = string.Empty;
            string certPath = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-d" && ++i < args.Length)
                {
                    rootDir = args[i];
                }
                else if (args[i] == "-m" && ++i < args.Length)
                {
                    appxManifestPath = args[i];
                }
                else if (args[i] == "-c" && ++i < args.Length)
                {
                    certPath = args[i];
                }
            }

            if (string.IsNullOrEmpty(rootDir))
            {
                Console.WriteLine("Usage: IndexCreationTool.exe -d <Path to search for yaml> [-m <appxmanifest for index package> [-c <cert for signing index package>]]");
                return;
            }

            try
            {
                if (File.Exists(IndexName))
                {
                    Console.WriteLine("Verbose: Deleting old index.db");
                    File.Delete(IndexName);
                }

                Console.WriteLine("Verbose: Starting index.db creation");
                using (var indexHelper = WinGetUtilWrapper.Create(IndexName))
                {
                    foreach (string file in Directory.EnumerateFiles(rootDir, "*.yaml", SearchOption.AllDirectories))
                    {
                        indexHelper.AddManifest(file, Path.GetRelativePath(rootDir, file));
                    }
                    indexHelper.PrepareForPackaging();
                }
                Console.WriteLine("Verbose: Finished index.db creation");

                if (!string.IsNullOrEmpty(appxManifestPath))
                {
                    using (StreamWriter outputFile = new StreamWriter("MappingFile.txt"))
                    {
                        Console.WriteLine($"Verbose: Creating MappingFile.txt started");
                        outputFile.WriteLine("[Files]");
                        outputFile.WriteLine($"\"{IndexName}\" \"{IndexPathInPackage}\"");
                        //outputFile.WriteLine($"\"{appxManifestPath}\" \"AppxManifest.xml\"");
                        outputFile.WriteLine($"\"{System.IO.Path.Combine(appxManifestPath, "AppxManifest.xml")}\" \"AppxManifest.xml\"");
                        outputFile.WriteLine($"\"{System.IO.Path.Combine(appxManifestPath, "Assets\\AppPackageStoreLogo.scale-100.png")}\" \"Assets\\AppPackageStoreLogo.scale-100.png\"");
                        outputFile.WriteLine($"\"{System.IO.Path.Combine(appxManifestPath, "Assets\\AppPackageStoreLogo.scale-125.png")}\" \"Assets\\AppPackageStoreLogo.scale-125.png\"");
                        outputFile.WriteLine($"\"{System.IO.Path.Combine(appxManifestPath, "Assets\\AppPackageStoreLogo.scale-150.png")}\" \"Assets\\AppPackageStoreLogo.scale-150.png\"");
                        outputFile.WriteLine($"\"{System.IO.Path.Combine(appxManifestPath, "Assets\\AppPackageStoreLogo.scale-200.png")}\" \"Assets\\AppPackageStoreLogo.scale-200.png\"");
                        Console.WriteLine($"Verbose: Creating MappingFile.txt finished");
                    }
                    //RunCommand("C:\\Program Files (x86)\\Windows Kits\\10\\bin\\10.0.19041.0\\x64\\makeappx.exe", $"pack /f MappingFile.txt /o /nv /p {IndexPackageName}");

                    //if (!string.IsNullOrEmpty(certPath))
                    //{
                    //    RunCommand("C:\\Program Files (x86)\\Windows Kits\\10\\bin\\10.0.19041.0\\x64\\signtool.exe", $"sign /a /fd sha256 /f {certPath} {IndexPackageName}");
                    //}

                    Console.WriteLine($"Hint: Execute \"makeappx.exe pack /f MappingFile.txt /o /nv /p source.msix\" manually");
                    Console.WriteLine($"Hint: Execute \"signtool.exe sign /a /fd sha256 /f <path_to_cert_file> source.msix\" manually");

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed. Reason: " + e.Message);
            }

            Environment.Exit(0);
        }

        static void RunCommand(string command, string args)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(command, args);
            p.Start();
            p.WaitForExit();
        }
    }
}