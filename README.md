# rclUE

This repository is a personal fork of [rclUE](https://github.com/rapyuta-robotics/rclUE), a ROS 2 ↔ Unreal Engine integration plugin originally developed by [Rapyuta Robotics](https://github.com/rapyuta-robotics).

The goal of this fork is to enable the use of the plugin on Windows. While support for ROS 2 is more robust on Linux than it is on Windows, there exist valid reasons for wanting or needing to develop on Windows ([#94](https://github.com/rapyuta-robotics/rclUE/issues/94)).

Given the lack of public solutions, I have adapted the plugin for my own development needs and am sharing the results for those facing similar constraints.

## System Requirements

This fork was developed on the following toolchain:
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

If this is the case, you will need to perform your own port. Below is a guide describing the process followed in creating this port; while it may not be a 1:1 walkthrough for every environment, it should serve as a solid foundation.

## Porting for Other Environments

### Prerequisites

Before you begin, make sure the following tools are installed on your system:
- Unreal Engine
- ROS 2 (binary or source)
- Git
- Python 3.x
- Visual Studio 2022 (with the `x64 Native Tools` command prompt)

#### 1. Clone the upstream branch that most closely matches your environment
```bash
git clone https://github.com/rapyuta-robotics/rclUE.git
cd rclUE
git checkout UE5.5_devel_humble
```

#### 2. Create a directory tree for the Windows binaries
```bash
cd ThirdParty
mkdir Win64\ros
cd Win64\ros
mkdir bin include lib
```

#### 3. Copy core ROS 2 artifacts into the directory tree
| From ROS 2 | To Plugin                    |
|------------|------------------------------|
| bin/*.dll  | ThirdParty/Win64/ros/bin     |
| include/** | ThirdParty/Win64/ros/include |
| lib/*.lib  | ThirdParty/Win64/ros/lib     |

You may choose to copy the folders in their entirety for convenience, or just the files that exist in the Linux reference layout (`ThirdParty/ros`) for a minimal footprint.

Additionally, add `yaml.dll` and `yaml.lib`–they are not present in the upstream plugin but are required for a successful Windows build.

#### 4. Build and add missing libraries
Depending on your ROS 2 installation, some packages expected by the plugin may be absent. ForROS 2 Humble, the typical suspects are:
- `pcl_msgs`
- `rclc`
- `ue_msgs`

You must build these packages from source and copy their artifacts into the directory tree from **Step 2**.

1. Open an `x64 Native Tools Command Prompt for VS 2022` with administrator rights.
2. Install Colcon and some of its common extensions.
```bash
pip install colcon-common-extensions
```
3. Create a Colcon workspace and clone the required repositories
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
4. Build the workspace
```bash
# Do this in the workspace directory (e.g. rclUE_ws), NOT the src directory
colcon build
```

On success, your workspace directory should look something like this:
```
rclUE_ws
├── build
└── install
    └── pcl_msgs
        ├── bin
        ├── include
        ├── lib
        └── share
    ├── ...
├── log
├── source
```

Copy the appropriate build artifacts to the plugin directory once more.

If all has gone well, then the plugin is now ready for use.

#### Troubleshooting

In the case of `rclc`, your error message is likely to be something like this:
```bash
test_action_server.obj : error LNK2019: unresolved external symbol rclc_executor_add_action_server referenced in function "private: virtual void __cdecl Test_rclc_action_server_Test::TestBody(void)" (?TestBody@Test_rclc_action_server_Test@@EEAAXXZ) [C:\ros2\rclUE_ws\build\rclc\rclc_test.vcxproj]
test_action_client.obj : error LNK2019: unresolved external symbol rclc_executor_add_action_client referenced in function "private: virtual void __cdecl Test_rclc_action_client_Test::TestBody(void)" (?TestBody@Test_rclc_action_client_Test@@EEAAXXZ) [C:\ros2\rclUE_ws\build\rclc\rclc_test.vcxproj]
C:\ros2\rclUE_ws\build\rclc\Release\rclc_test.exe : fatal error LNK1120: 2 unresolved externals [C:\ros2\rclUE_ws\build\rclc\rclc_test.vcxproj]
```

In this case, you may apply the following fix:
1. Open `<WORKSPACE_DIRECTORY>/src/rclc/rclc/include/rclc/executor.h`
2. Find the function with the name `rclc_executor_add_action_client`.
3. Above the function (before `rcl_ret_t`), add the following line: `RCLC_PUBLIC`.
4. Repeat steps 2 and 3 with the `rclc_executor_add_action_server` function.

Run `colcon build` again, and rclc should now build fine. You will be likely be informed that `rclc_examples` has failed to build, but you may ignore this, as the plugin only requires the core package (i.e. `rclc`).

#### 6. Update the `.uplugin` and `rclUE.Build.cs` files

In the `.uplugin` file, add `Win64` to the section marked `WhitelistPlatforms`.

In the `rclUE.Build.cs` file, add logic to detect and add the Windows libraries that you have added. If you're unsure how, feel free to reference the `rclUE.Build.cs` file from this repository, which does this.

#### 7. Place the rclUE folder into your Unreal Engine project's Plugins folder and update its `.uproject` file

To the `Plugins` section of your project's `.uproject` file, add the following:
```bash
  {
    "Name": "rclUE",
    "Enabled": true
  }
```

#### 8. Remove old build artifacts (optional)
Delete the following folders from your project directory (if they exist) to force a full rebuild:
- Binaries
- DerivedDataCache
- Intermediate
- Plugins/rclUE/Binaries
- Plugins/rclUE/Intermediate
- Saved
- <PROJECT_NAME>.sln

Your project should now be ready to build and run!

---

#### Additional troubleshooting

If the project fails to build or run, check the `*.log` files in the `Saved` folder in the project directory for guidance on what to do. If the log contains the message `missing import`, then you've likely forgotten to add a `.lib`/`.dll` from somwhere.
