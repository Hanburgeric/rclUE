# rclUE

This repository is a personal fork of [rclUE](https://github.com/rapyuta-robotics/rclUE), a ROS 2–Unreal Engine integration plugin originally developed by [Rapyuta Robotics](https://github.com/rapyuta-robotics).

The goal of this fork is to enable the use of the plugin on Windows, despite the limited support for ROS 2 support on the platform. While ROS 2 is certainly more robust on Linux, there may be some situations (e.g. a certain feature on Unreal Engine is better supported on or downright exclusive to Windows) where a Windows-compatible build of rclUE would be desirable or even necessary.

Given the lack of public solutions, I have adapted the plugin for my own development needs and am sharing the results in the hope that they help others facing similar constraints.

## System Requirements

This fork was developed on a single toolchain:
- **Operating System**: Windows 11
- **Unreal Engine**: Unreal Engine 5.5.4
- **ROS 2**: Humble Hawksbill ([Binary Installation](https://docs.ros.org/en/humble/Installation/Windows-Install-Binary.html))
- **Compiler/IDE**: Visual Studio 2022

Future updates will generally track the upstream `UE5.5_devel_humble` branch.

Consequently, if you are working with...
- a different version of Windows,
- a different version of Unreal Engine, or
- a different ROS 2 distribution or installation method,

the plugin may not compile out of the box, primarily because it relies on pre-compiled binaries located in the `ThirdParty/Win64` folder.

If that is the case, you will need to perform your own port. The guide below describes the process I followed; while it may not be a 1:1 walkthrough for every environment, it should provide a solid foundation.

If you encounter an issue that is not covered, feel free to open an issue, start a discussion thread, or submit a pull request. I will do my best to help.

## Porting for Other Environments

### Prerequisites

Before you begin, make sure the following tools are installed:
- Your desired version of Unreal Engine
- Your desired distribution of ROS 2 (binary or source)
- Git
- Python 3.x
- Visual Studio 2022 (with the `x64 Native Tools` command prompt)

#### 1. Clone the upstream branch that most closely matches your environment
```bash
git clone https://github.com/rapyuta-robotics/rclUE.git
cd rclUE
git checkout UE5.5_devel_humble
```

#### 2. Navigate to the `ThirdParty` folder
```bash
cd ThirdParty
```

#### 3. Create a directory tree for the Windows binaries
```bash
mkdir Win64\ros
cd Win64\ros
mkdir bin include lib
```

#### 4. Locate your ROS 2 installation
```bash
cd C:\dev\ros2_humble
```

#### 5. Copy core ROS 2 artifacts into the plugin
Move all relevant...
- `.dll` files from `bin`,
- include folders from `include`, and
- `.lib` files from `lib`

into the matching folders created in **Step 3**. You may choose to copy the entire `bin`, `include`, and `lib` directories for convenience, or copy only the files that exist in the Linux reference layout (`ThirdParty/ros`) for a minimal footprint approach.

Additionally, add `yaml.bin` from `bin` and `yaml.dll` from `lib`–they are not present in the upstream plugin but are required for a successful Windows build.

Note that ROS 2 Humble's headers often nest package directories twifce (e.g. `nav_msgs/nav_msgs/...` instead of `nav_msgs/...`); preserve this folder structure exactly.

#### 6. Build and add any missing libraries
Depending on your ROS 2 installation, some packages expected by the plugin may be absent. In the case of Humble, the usual suspects are:
- `pcl_msgs`
- `rclc`
- `ue_msgs`

You must build these packages from source and copy their artifacts into the matching folders created in **Step 3**.

1. Ensure Python is available; this is likely trivial if you've installed ROS 2. Otherwise, install it from the [official site](https://www.python.org/downloads/).
2. Open an `x64 Native Tools Command Prompt for VS 2022` with administrator rights.
3. Install Colcon and some common extensions.
```bash
pip install colcon-common-extensions
```
4. Create a workspace and clone the required repositories.
```bash
mkdir rclUE_ws\src
cd rclUE_ws\src

git clone https://github.com/ros-perception/perception_pcl.git
git clone https://github.com/ros2/rclc.git
git clone https://github.com/rapyuta-robotics/UE_msgs.git

# Make sure to checkout the correct branch
cd rclc
git checkout humble
cd ..
```
5. Build the workspace
```bash
# Make sure you are in the workspace directory (e.g. rclUE_ws), NOT the src directory
colcon build
```
On success, an `install` directory containing `bin`, `include`, and `lib` sub-directories for each of your repositories will be made.
(Can you create folder-like layout)?
6. Copy the build artifacts into matching folders plugin, just as you did before

#### Troubleshooting (WIP)

There is a possibility for the `colcon build` command to fail. This is more likely to be true when building the humble branch of rclc (not sure about the others).

To check the error, open the stdout_stderr.log file in the the folder of the library that failed to build. For example:
```bash
<rclUE_ws_DIRECTORY>\src\rclc\log\build_<TIMESTAMP>\rclc\stdout_stderr.log
```

There is a high likelihood that your `colcon build` command will fail, especially when building rclc.

In all likelihood, your `colcon build` command will fail. This will likely be due to rclc.

In your worskspace directory, if you check log\build_<timestamp>\rclc\stdout_stderr.log and the final messages look something like:
test_action_server.obj : error LNK2019: unresolved external symbol rclc_executor_add_action_server referenced in function "private: virtual void __cdecl Test_rclc_action_server_Test::TestBody(void)" (?TestBody@Test_rclc_action_server_Test@@EEAAXXZ) [C:\ros2\rclUE_ws\build\rclc\rclc_test.vcxproj]
test_action_client.obj : error LNK2019: unresolved external symbol rclc_executor_add_action_client referenced in function "private: virtual void __cdecl Test_rclc_action_client_Test::TestBody(void)" (?TestBody@Test_rclc_action_client_Test@@EEAAXXZ) [C:\ros2\rclUE_ws\build\rclc\rclc_test.vcxproj]
C:\ros2\rclUE_ws\build\rclc\Release\rclc_test.exe : fatal error LNK1120: 2 unresolved externals [C:\ros2\rclUE_ws\build\rclc\rclc_test.vcxproj]

Then you may apply the following fix:
1. open <ws_dir>\src\rclc\rclc\include\rclc\executor.h
2. find the function with the name `rclc_executor_add_action_client`.
3. You will find that unlike other functions, it has not been marked with the RCLC_PUBLIC macro. Add it before the function (above/before rcl_ret_t).
4. repeat 2~3 with `rclc_executor_add_action_server`.

If all is right, then your `colcon build` command should now work. In high likelihood, it will inform you that `rclc_examples` has failed to build, but this is expected (the code is littered with Linux-specific code). We do not require anything from this library, so feel free to ignore this message.

copy/paste the content in the bin/include/lib folders from pcl_msgs/rclc/ue_msgs to plugindir/Win64ThirdParty/ros.

#### 7. Update the .uplugin and rclUE.Build.cs files (WIP)

(WIP)

#### 8. Place the rclUE folder into your Unreal Engine project's Plugins folder (WIP) and update its .uproject file

(WIP)

#### 9. Perform a clean build (WIP)

(WIP)

#### 10. Build and run the project (WIP)

(WIP)

#### Troubleshooting (WIP)

build fails, check log files in saved folder. search for "missing import" as a likely source of error.

### Known Issues (WIP)

with the way the current rclUE.Build.cs file is set up, the project must first be compiled in Shipping, then run in any other configuration. this is because of how windows handles external dlls, this is WIP.
