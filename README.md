# rclUE

This repository is a personal fork of [rclUE](https://github.com/rapyuta-robotics/rclUE), a ROS 2—Unreal Engine integration plugin originally developed by [Rapyuta Robotics](https://github.com/rapyuta-robotics).

The goal of this fork is to enable usage of the plugin on Windows, despite the current limitations of ROS 2 support on said platform. While ROS 2 is best supported on Linux, there are valid scenarios—such as Unreal Engine features that are exclusive to or better supported on Windows (as noted [here](https://github.com/rapyuta-robotics/rclUE/issues/94))—where a Windows-compatible build of rclUE would be desirable.

As someone personally facing these constraints and unable to find an existing public port, I've resorted to adapting the plugin myself. This repository shares the result, in the hopes that it will benefit others with similar needs.

## Development Environment

As mentioned, this fork was created with personal use in mind; as such, I've not really taken the time to consider various development environments.

However, the very end of this document ??? the process of creating your own port of rclUE, so resort to that if the plugin refuses to compile.

## Known problems


### Missing libraries

In a fresh binary installation of ROS 2 Humble on Windows, three libraries will be missing:
- `pcl_msgs`
- `rclc`
- `ue_msgs`

As such, it is up to the user to build these libraries and place these libraries in their appropriate folders (i.e. bin, include, and lib).

This step can be quite frustrating, so a step-by-step guide has been prepared below.

1. First, ensure that Python has been installed on your system. If you've followed the official ROS 2 Humble installation guide, then this will already ahve been done for you. If not, download a release from [here](https://www.python.org/downloads/). The pre-compiled libraries have been built with Python 3.8 (the version listed on the official installation guide).
2. pip install colcon-common-extensions
3. mkdir <ws_directory>/src
4. cd <ws_directory>/src
5. git clone pcl_msgs
6. git clone rclc (checkout humble)
8. git clone ue_msgs

#### Troubleshooting

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

# Basic information

## Online documentation

https://rclUE.readthedocs.io/en/devel/

## Supported versions

Main support

- Ubuntu 20.04
- Unreal Engine 5.10
- ROS2 Foxy
- Clang: 13.0.1

Maintenance/experimental

- Ubuntu 22.04 and ROS2 Humble(`UE5_devel_humble` branch)
- Ubuntu 24.04 and ROS2 Humble(`UE5_devel_jazzy` branch)

Please download UE5.10 for Linux by following [Unreal Engine for Linux](https://www.unrealengine.com/en-US/linux)

## Branches

- `devel`: This build of the plugin is based on ROS2 Foxy and has been tested on Ubuntu 20 and UE5.10.
- `UE5_devel_foxy`: Same as above.
- `UE5_devel_humble_20.04`(experimental): This build of the plugin is based on ROS 2 humble and has been tested on Ubuntu 20.04 and UE5.1.
- `UE5_devel_humble`(experimental): This build of the plugin is based on ROS 2 humble, Ubuntu 22.04 and UE5.1.
- `UE5_devel_jazzy`(experimental): This build of the plugin is based on ROS 2 jazzy, Ubuntu 24.04 and UE5.1.


## TroubleShooting

### Missing library
Due to pre-compiled libraries, ThirdParty/ros/lib/librcl.so dynamically links libyaml.so and libspdlog.so.1, which needs to be provided by/installed on the host system. If not, Unreal fails to load the plugin or package the project without further details.

On some operating systems, even with libyaml and libspdlog installed, the version appendix may not exist. You can try creating them using:

```
cd /lib64
ln -s </path/to/libyaml.so.X.Y.Z> libyaml.so
ln -s </path/to/libspdlog.so.X.Y.Z> libspdlog.so.1
```

# rclUE and ROS2

## Description

- We use ros2 'foxy' lightweighted (not all binaries are included). Source/ThirdParty/ros folder is fully autogenerated by [UE_tools](https://github.com/rapyuta-robotics/UE_tools)
- ros includes [UE_msgs](https://github.com/rapyuta-robotics/UE_msgs)
- UE uses centimeters but ROS uses meters. Please convert manually or use [URRConversionUtils](https://rapyutasimulationplugins.readthedocs.io/en/devel/doxygen_generated/html/d4/dc1/class_u_r_r_conversion_utils.html) in [RapyutaSimulationPlugins](https://rapyutasimulationplugins.readthedocs.io/en/devel/index.html)
- within the Unreal Editor: Edit->Plugins, search and enable for `rclc`

## Windows is currently unsupported

# Getting Started

The plugin folder contains a video "Example_BP_PubSub.mp4" demonstrating how to setup a PubSub example in Blueprint.

An example setup using this plugin can be found at [turtlebot3-UE](https://github.com/rapyuta-robotics/turtlebot3-UE)

# Notes on working with ROS 2 and UE

- rcl and void\* types cannot be managed by UE (no UPROPERTY) and therefore can't be used directly in Blueprint. Whenever access to these variables is needed, the user should write a class to wrap it and all of their handling must be done in C++.
- some basic numerical types are not natively supported in Blueprint (e.g. double, unsigned int). In order to use these, a workaround is needed (a plugin implementing those types for BP, a modified UE or a custom implementation).
- In autogenerated messages, the method MsgToString() should be implemented by the user as its current purpose is to help debugging.

# How to update ROS inside RclUE

Currently there is a scripts in [UE_tools](https://github.com/rapyuta-robotics/UE_tools) to automatically build and update ROS2 libraries. Please follow [steps](https://github.com/rapyuta-robotics/UE_tools#general-usage)

# Add CustomMsg in rclUE or other Plugins
Please check [CustomMsgExample](https://github.com/yuokamoto/rclUE-Examples/blob/custom_msg_example/Plugins/CustomMsgExample/README.md) as a example of custom msg in different plugin then rclUE.


# Install pre-commit

Please install pre-commit before commiting your changes.
Follow this instruction https://pre-commit.com/

then run

```bash
pre-commit install
```

# Documentation

## Tools

documentation is built with three tools

- [doxygen](http://www.doxygen.org)
- [sphinx](http://www.sphinx-doc.org)
- [breathe](https://breathe.readthedocs.io)

## Locally build

1. install tools in #tools section.
2. build
   ```
   cd docs
   make --always-make html
   ```
3. Open following in your browser.
   - Sphinx at `file:///<path to cloned repo>/docs/source/_readthedocs/html/index.html`
   - Original doxygen output at `file:///<path to cloned repo>/docs/source/_readthedocs/html/doxygen_generated/html/index.html`

# Maintainer

yu.okamoto@rapyuta-robotics.com
