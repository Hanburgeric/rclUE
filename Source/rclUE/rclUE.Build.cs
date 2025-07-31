// Copyright 2020-2023 Rapyuta Robotics Co., Ltd.

using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using UnrealBuildTool;

public class rclUE : ModuleRules
{
	private string RosPath
	{
		get { return Path.Combine(ModuleDirectory, "../../", "ThirdParty", "ros"); }
	}

	private string Win64RosPath
	{
		get { return Path.Combine(ModuleDirectory, "..", "..", "ThirdParty", "Win64", "ros"); }
	}

	private void AddModule(string InModulePath, bool bInCopySharedLibsToOutputDir = false)
	{
        string includePath = Path.Combine(InModulePath, "include");

		// since ros2 humble has include path like ue_msgs/ue_msgs/msgs instead of ue_msgs/msgs
        if(Directory.Exists(includePath))
        {
            PublicIncludePaths.Add(includePath);
			Console.WriteLine("== Add [rclUE] include:");
			var includes = Directory.EnumerateDirectories(includePath);
            foreach (var include in includes)
            {
                Console.WriteLine(include);
	            PublicIncludePaths.Add(include);
            }

        }

        string libPath = Path.Combine(InModulePath, "lib");

        if(Directory.Exists(libPath))
        {
            //PublicLibraryPaths.Add(libPath);
            PublicRuntimeLibraryPaths.Add(libPath);
            PrivateRuntimeLibraryPaths.Add(libPath);
            var libs = Directory.EnumerateFiles(libPath, "*.so", SearchOption.TopDirectoryOnly); //.Union(Directory.EnumerateFiles(libPath, "*.so.*", searchOption));

            Console.WriteLine("== Add [rclUE] libs:");
            foreach (var lib in libs)
            {
                Console.WriteLine(lib);
                PublicAdditionalLibraries.Add(lib);
                if (bInCopySharedLibsToOutputDir)
                {
                	// NOTE: Don't use $(BinaryOutputDir), which makes lib copied to this module's Binaries folder in Editor build, not the executor folder (UE4Editor/UnrealEditor or packaged app)
                    RuntimeDependencies.Add(Path.Combine("$(TargetOutputDir)", Path.GetFileName(lib)), lib, StagedFileType.NonUFS);
                }
                else
                {
                    RuntimeDependencies.Add(lib, StagedFileType.NonUFS);
                }
            }
        }
	}

	private void AddWin64Module(string InModulePath, bool bInCopySharedLibsToOutputDir = false)
	{
        // Includes
        string includePath = Path.Combine(InModulePath, "include");
        if (Directory.Exists(includePath))
        {
            PublicIncludePaths.Add(includePath);
            Console.WriteLine("== Add [rclUE] include:");

            var includes = Directory.EnumerateDirectories(includePath);
            foreach (var include in includes)
            {
                Console.WriteLine(include);
                PublicIncludePaths.Add(include);
            }
        }

        // Import libraries; no need to add public/private runtime library paths here,
        // as this is irrelevant on Windows (i.e. Windows executables do not use an rpath analogue)
        string libPath = Path.Combine(InModulePath, "lib");
        if (Directory.Exists(libPath))
        {
            Console.WriteLine("== Add [rclUE] libs:");

            var libs = Directory.EnumerateFiles(libPath, "*.lib", SearchOption.TopDirectoryOnly);
            foreach (var lib in libs)
            {
                Console.WriteLine(lib);
                PublicAdditionalLibraries.Add(lib);
            }
        }

        // Dynamic link libraries
        string binPath = Path.Combine(InModulePath, "bin");
        if (Directory.Exists(binPath))
        {
            Console.WriteLine("== Add [rclUE] dlls:");

            var dlls = Directory.EnumerateFiles(binPath, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var dll in dlls)
            {
                // Delay-load such that symbols are resolved only on first use,
                // akin to how dlopen behaves on Linux
                PublicDelayLoadDLLs.Add(Path.GetFileName(dll));
                Console.WriteLine(dll);

                if (bInCopySharedLibsToOutputDir)
                {
                    RuntimeDependencies.Add(Path.Combine("$(TargetOutputDir)", Path.GetFileName(dll)), dll, StagedFileType.NonUFS);
                }
                else
                {
                    RuntimeDependencies.Add(dll, StagedFileType.NonUFS);
                }

                // Again, due to the lack of an rpath analogue on Windows executables,
                // it is necessary to move the DLLs manually to a directory where
                // they will be detected by the OS (e.g. next to the executable);
                // this allows Editor builds to run without having to first be built
                // as a game (i.e. DebugGame, Development, Shipping)
                if (Target.Type == TargetType.Editor)
                {
                    RuntimeDependencies.Add(Path.Combine("$(ProjectDir)", "Binaries", "Win64", Path.GetFileName(dll)), dll, StagedFileType.NonUFS);
                }
            }
        }
    }

    public rclUE(ReadOnlyTargetRules Target) : base(Target)
	{
		var envVars = Environment.GetEnvironmentVariables();
		string ldLibraryPathKey = "LD_LIBRARY_PATH";

		if (envVars.Contains(ldLibraryPathKey))
		{
			Console.WriteLine(string.Format("[rclUE] LD_LIBRARY_PATH: {0}", envVars[ldLibraryPathKey]));
		}

        PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
		CppStandard = CppStandardVersion.Latest;
		if (Target.Platform == UnrealTargetPlatform.Linux)
		{
			// In order to ROS shared libs to be linkable by client UE app & for sake of portability, they need to all co-locate in the same folder
			// => Need to copy them TargetOutputDir
			AddModule(RosPath, true);
		}
		else if (Target.Platform == UnrealTargetPlatform.Win64)
		{
            AddWin64Module(Win64RosPath, true);
		}

		PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "Public"));
		PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "Private"));

		PublicDependencyModuleNames.AddRange(
			new string[]
			{
				"Core",
				"CoreUObject",
				"Engine",
				"Projects"
			}
		);
	}
}
