// Copyright 2015 Open Source Robotics Foundation, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#ifndef RCPPUTILS__VERSION_H_
#define RCPPUTILS__VERSION_H_

/// \def RCPPUTILS_VERSION_MAJOR
/// Defines RCPPUTILS major version number
#define RCPPUTILS_VERSION_MAJOR (2)

/// \def RCPPUTILS_VERSION_MINOR
/// Defines RCPPUTILS minor version number
#define RCPPUTILS_VERSION_MINOR (4)

/// \def RCPPUTILS_VERSION_PATCH
/// Defines RCPPUTILS version patch number
#define RCPPUTILS_VERSION_PATCH (4)

/// \def RCPPUTILS_VERSION_STR
/// Defines RCPPUTILS version string
#define RCPPUTILS_VERSION_STR "2.4.4"

/// \def RCPPUTILS_VERSION_GTE
/// Defines a macro to check whether the version of RCPPUTILS is greater than or equal to
/// the given version triple.
#define RCPPUTILS_VERSION_GTE(major, minor, patch) ( \
     (major < RCPPUTILS_VERSION_MAJOR) ? true \
     : (major > RCPPUTILS_VERSION_MAJOR) ? false \
     : (minor < RCPPUTILS_VERSION_MINOR) ? true \
     : (minor > RCPPUTILS_VERSION_MINOR) ? false \
     : (patch < RCPPUTILS_VERSION_PATCH) ? true \
     : (patch > RCPPUTILS_VERSION_PATCH) ? false \
     : true)

#endif  // RCPPUTILS__VERSION_H_
