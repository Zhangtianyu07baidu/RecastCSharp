// RecastDll.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"

static Envlog envlog;

static void Log(int categary, const char* msg)
{
	if (envlog)
	{
		envlog(categary, msg);
	}
}

EXPORT_API void SetEnvLog(Envlog log)
{
	envlog = log;
}

EXPORT_API SoloRecast* CreateSoloRecast()
{
	return new SoloRecast(new BuildContext(Log));
}

EXPORT_API TileRecast* CreateTileRecast()
{
	return new TileRecast(new BuildContext(Log));
}

EXPORT_API TileCacheRecast* CreateTileCacheRecast()
{
	return new TileCacheRecast(new BuildContext(Log));
}

EXPORT_API void SetBuildParams(RecastPtr recast, float agentHeight, float agentRadius, float agentMaxClimb, float agentMaxSlope,
	float cellSize, float cellHeight, float regionMinSize, float regionMergeSize,
	float edgeMaxLen, float edgeMaxError, float vertsPerPoly, float detailSampleDist, float detailSampleMaxError,
	int partitionType, float tileSize)
{
	recast->SetBuildParams(agentHeight, agentRadius, agentMaxClimb, agentMaxSlope, cellSize, cellHeight,
		regionMinSize, regionMergeSize, edgeMaxLen, edgeMaxError, vertsPerPoly, detailSampleDist,
		detailSampleMaxError, partitionType, tileSize);
}

EXPORT_API bool LoadMeshGeometry(RecastPtr recast, const char* objPath)
{
	return recast->LoadMeshGeometry(objPath);
}

EXPORT_API bool BuildBinary(RecastPtr recast, const char* binPath)
{
	if (recast->Build())
	{
		return recast->SaveMeshBin(binPath);
	}
	return false;
}

EXPORT_API bool LoadMeshBin(RecastPtr recast, const char* filePath)
{
	return recast->LoadMeshBin(filePath);
}

EXPORT_API int GetConvexCount(RecastPtr recast)
{
	InputGeom* geom = recast->GetInputGeom();
	if (geom)
	{
		return geom->getConvexVolumeCount();
	}
	return 0;
}

EXPORT_API void GetConvexArray(RecastPtr recast, ConvexVolume* value)
{
	InputGeom* geom = recast->GetInputGeom();
	if (geom)
	{
		const ConvexVolume* m_volumes = geom->getConvexVolumes();
		// Convex volumes
		for (int i = 0; i < geom->getConvexVolumeCount(); ++i)
		{
			value[i] = m_volumes[i];
		}
	}
}

EXPORT_API void GetBuildSettings(RecastPtr recast, BuildSettings* value)
{
	InputGeom* geom = recast->GetInputGeom();
	if (geom)
	{
		const BuildSettings* settings = geom->getBuildSettings();
		value->cellSize = settings->cellSize;
		value->cellHeight = settings->cellHeight;
		value->agentHeight = settings->agentHeight;
		value->agentRadius = settings->agentRadius;
		value->agentMaxClimb = settings->agentMaxClimb;
		value->agentMaxSlope = settings->agentMaxSlope;
		value->regionMinSize = settings->regionMinSize;
		value->regionMergeSize = settings->regionMergeSize;
		value->edgeMaxLen = settings->edgeMaxLen;
		value->edgeMaxError = settings->edgeMaxError;
		value->vertsPerPoly = settings->vertsPerPoly;
		value->detailSampleDist = settings->detailSampleDist;
		value->detailSampleMaxError = settings->detailSampleMaxError;
		value->partitionType = settings->partitionType;

		value->navMeshBMin[0] = settings->navMeshBMin[0];
		value->navMeshBMin[1] = settings->navMeshBMin[1];
		value->navMeshBMin[2] = settings->navMeshBMin[2];

		value->navMeshBMax[0] = settings->navMeshBMax[0];
		value->navMeshBMax[1] = settings->navMeshBMax[1];
		value->navMeshBMax[2] = settings->navMeshBMax[2];

		value->tileSize = settings->tileSize;
	}
}

EXPORT_API void SetFilterFlag(RecastPtr recast, int flag, bool isIncluded)
{
	recast->SetFilterFlag(flag, isIncluded);
}

EXPORT_API void SetAreaCost(RecastPtr recast, int area, float cost)
{
	recast->SetAreaCost(area, cost);
}

EXPORT_API int FindSmoothPath(RecastPtr recast, float* startPos, float* endPos, float* smoothPath, int* nsmooth)
{
	return recast->FindSmoothPath(startPos, endPos, smoothPath, nsmooth);
}

EXPORT_API int FindStraightPath(RecastPtr recast, float* startPos, float* endPos, float* straightPath, int* nstraight)
{
	return recast->FindStraightPath(startPos, endPos, straightPath, nstraight);
}

EXPORT_API int FindRandomPoint(RecastPtr recast, float* resultPos)
{
	return recast->FindRandomPoint(resultPos);
}

EXPORT_API int FindRandomPointAroundCircle(RecastPtr recast, float* center, float radius, float* resultPos)
{
	return recast->FindRandomPointAroundCircle(center, radius, resultPos);
}

EXPORT_API int Raycast(RecastPtr recast, float* startPos, float* endPos, float* hitPos)
{
	return recast->Raycast(startPos, endPos, hitPos);
}

EXPORT_API void Update(TileCacheRecast* tileCacheRecast, float dt)
{
	//TileCacheRecast* tileCacheRecast = dynamic_cast<TileCacheRecast*>(recast);
	tileCacheRecast->Update(dt);
}

EXPORT_API bool AddCylinderObstacle(TileCacheRecast* tileCacheRecast, float* pos, float radius, float height, dtObstacleRef& result)
{
	return tileCacheRecast->AddCylinderObstacle(pos, radius, height, result);
}

EXPORT_API bool AddBoxObstacle(TileCacheRecast* tileCacheRecast, float* bmin, float* bmax, dtObstacleRef& result)
{
	return tileCacheRecast->AddBoxObstacle(bmin, bmax, result);
}

EXPORT_API bool RemoveObstacle(TileCacheRecast* tileCacheRecast, dtObstacleRef ref)
{
	return tileCacheRecast->RemoveObstacle(ref);
}

EXPORT_API void RemoveAllObstacles(TileCacheRecast* tileCacheRecast)
{
	tileCacheRecast->ClearAllObstacles();
}

EXPORT_API void BuildTile(TileRecast* tileRecast, float* minPoint, float* maxPoint)
{
	tileRecast->BuildTile(minPoint, maxPoint);
}

EXPORT_API void RemoveTile(TileRecast* tileRecast, float* minPoint, float* maxPoint)
{
	tileRecast->RemoveTile(minPoint, maxPoint);
}

EXPORT_API void AddConvexPoint(RecastPtr recast, float* p)
{
	recast->AddConvexPoint(p);
}

EXPORT_API bool MakeConvexPolygon(RecastPtr recast, int areaType)
{
	return recast->MakeConvexPolygon(areaType);
}

EXPORT_API bool DeleteConvexPolygon(RecastPtr recast, float* p)
{
	return recast->DeleteConvexPolygon(p);
}

EXPORT_API void Release(RecastPtr recast)
{
	delete recast;
}