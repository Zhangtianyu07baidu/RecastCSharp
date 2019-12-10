# For more information about using CMake with Android Studio, read the
# documentation: https://d.android.com/studio/projects/add-native-code.html

# Sets the minimum version of CMake required to build the native library.

cmake_minimum_required(VERSION 3.4.1)

include_directories(DebugUtils/Include)
include_directories(Detour/Include)
include_directories(DetourCrowd/Include)
include_directories(DetourTileCache/Include)
include_directories(Recast/Include)
include_directories(Contrib/fastlz)
include_directories(.)

aux_source_directory(DebugUtils/Source DIR_DEBUGUTILS_SRCS)
aux_source_directory(Detour/Source DIR_DETOUR_SRCS)
aux_source_directory(DetourCrowd/Source DIR_DETOURCROWD_SRCS)
aux_source_directory(DetourTileCache/Source DIR_DETOURTILECACHE_SRCS)
aux_source_directory(Recast/Source DIR_RECAST_SRCS)
aux_source_directory(Contrib/fastlz DIR_FASTLZ_SRCS)
aux_source_directory(. DIR_CUSTOM_SRCS)

list(APPEND DIR_CUSTOM_SRCS ${DIR_FASTLZ_SRCS})

# Creates and names a library, sets it as either STATIC
# or SHARED, and provides the relative paths to its source code.
# You can define multiple libraries, and CMake builds them for you.
# Gradle automatically packages shared libraries with your APK.

add_library( # Sets the name of the library.
             recast

             # Sets the library as a shared library.
             SHARED

             # Provides a relative path to your source file(s).
             ${DIR_DEBUGUTILS_SRCS}
             ${DIR_DETOUR_SRCS}
             ${DIR_DETOURCROWD_SRCS}
             ${DIR_DETOURTILECACHE_SRCS}
             ${DIR_RECAST_SRCS}
             ${DIR_CUSTOM_SRCS}
         )

#add_library(native-lib SHARED ${DIR_LIB_SRCS})

# Searches for a specified prebuilt library and stores the path as a
# variable. Because CMake includes system libraries in the search path by
# default, you only need to specify the name of the public NDK library
# you want to add. CMake verifies that the library exists before
# completing its build.

find_library( # Sets the name of the path variable.
              log-lib

              # Specifies the name of the NDK library that
              # you want CMake to locate.
              log )

# Specifies libraries CMake should link to your target library. You
# can link multiple libraries, such as libraries you define in this
# build script, prebuilt third-party libraries, or system libraries.

target_link_libraries( # Specifies the target library.
                        recast

                       # Links the target library to the log library
                       # included in the NDK.
                       ${log-lib} )