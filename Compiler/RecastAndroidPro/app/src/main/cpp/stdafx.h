// stdafx.h: 标准系统包含文件的包含文件，
// 或是经常使用但不常更改的
// 项目特定的包含文件
//

#pragma once

#ifdef _WINDOWS

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // 从 Windows 头中排除极少使用的资料

// Windows 头文件:
#include <windows.h>

#define EXPORT_API extern "C" __declspec(dllexport)
//#define STD_CALL __stdcall

#else

#define EXPORT_API extern "C"
//#define STD_CALL

#endif


// 在此处引用程序需要的其他标头
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <ctype.h>
#include <string.h>
#include <cstring>
#include <string>
#include <algorithm>

#include "Contrib/fastlz/fastlz.h"
#include "Recast.h"
#include "DetourNavMesh.h"
#include "DetourCrowd.h"
#include "DetourNavMeshBuilder.h"
#include "DetourTileCacheBuilder.h"
#include "DetourTileCache.h"
#include "DetourCommon.h"
#include "RecastDump.h"

#include "PerfTimer.h"
#include "ChunkyTriMesh.h"
#include "MeshLoaderObj.h"
#include "InputGeom.h"
#include "RecastInterfaces.h"

#include "ConvexPolygon.h"
#include "PathFinder.h"
#include "RecastBase.h"
#include "SoloRecast.h"
#include "TileRecast.h"
#include "TileCacheRecast.h"

typedef RecastBase* RecastPtr;
