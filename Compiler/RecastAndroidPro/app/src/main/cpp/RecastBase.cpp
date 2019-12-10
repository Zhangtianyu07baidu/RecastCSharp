#include "stdafx.h"



RecastBase::RecastBase(BuildContext* ctx) :
	m_navMesh(0),
	m_filterLowHangingObstacles(true),
	m_filterLedgeSpans(true),
	m_filterWalkableLowHeightSpans(true)
{
	this->ResetSettings();
	this->m_ctx = ctx;
	this->m_navQuery = dtAllocNavMeshQuery();
	this->m_crowd = dtAllocCrowd();
	this->m_geom = new InputGeom();
	this->m_pathFinder = new PathFinder();
	this->m_convexPolygon = new ConvexPolygon(this->m_geom);
}

RecastBase::~RecastBase()
{
	dtFreeNavMeshQuery(this->m_navQuery);
	dtFreeNavMesh(this->m_navMesh);
	dtFreeCrowd(this->m_crowd);
	delete this->m_pathFinder;
	delete this->m_convexPolygon;
	delete this->m_ctx;
	delete this->m_geom;
}

void RecastBase::ResetSettings()
{
	this->m_cellSize = 0.3f;
	this->m_cellHeight = 0.2f;
	this->m_agentHeight = 2.0f;
	this->m_agentRadius = 0.6f;
	this->m_agentMaxClimb = 0.9f;
	this->m_agentMaxSlope = 45.0f;
	this->m_regionMinSize = 8;
	this->m_regionMergeSize = 20;
	this->m_edgeMaxLen = 12.0f;
	this->m_edgeMaxError = 1.3f;
	this->m_vertsPerPoly = 6.0f;
	this->m_detailSampleDist = 6.0f;
	this->m_detailSampleMaxError = 1.0f;
	this->m_partitionType = PARTITION_WATERSHED;
}

void RecastBase::SetBuildParams(float agentHeight, float agentRadius, float agentMaxClimb, float agentMaxSlope, float cellSize, float cellHeight, float regionMinSize, float regionMergeSize, float edgeMaxLen, float edgeMaxError, float vertsPerPoly, float detailSampleDist, float detailSampleMaxError, int partitionType, float tileSize)
{
	this->m_agentHeight = agentHeight;
	this->m_agentRadius = agentRadius;
	this->m_agentMaxClimb = agentMaxClimb;
	this->m_agentMaxSlope = agentMaxSlope;
	this->m_cellSize = cellSize;
	this->m_cellHeight = cellHeight;
	this->m_regionMinSize = regionMinSize;
	this->m_regionMergeSize = regionMergeSize;
	this->m_edgeMaxLen = edgeMaxLen;
	this->m_edgeMaxError = edgeMaxError;
	this->m_vertsPerPoly = vertsPerPoly;
	this->m_detailSampleDist = detailSampleDist;
	this->m_detailSampleMaxError = detailSampleMaxError;
	this->m_partitionType = partitionType;
}

bool RecastBase::LoadMeshGeometry(const char* filename)
{
	return this->m_geom->loadMesh(this->m_ctx, filename);
}

bool RecastBase::SaveMeshBin(const char* filePath)
{
	return this->SaveAll(filePath, m_navMesh);
}

bool RecastBase::SaveGeomSet(const std::string& filepath)
{
	BuildSettings settings;
	memset(&settings, 0, sizeof(settings));

	rcVcopy(settings.navMeshBMin, this->m_geom->getNavMeshBoundsMin());
	rcVcopy(settings.navMeshBMax, this->m_geom->getNavMeshBoundsMax());

	this->CollectSettings(settings);

	return this->m_geom->saveGeomSet(&settings, filepath);
}

bool RecastBase::LoadGeomSet(const std::string& filepath)
{
	return this->m_geom->loadGeomSet(this->m_ctx, filepath);
}

void RecastBase::CollectSettings(BuildSettings& settings)
{
	settings.cellSize = m_cellSize;
	settings.cellHeight = m_cellHeight;
	settings.agentHeight = m_agentHeight;
	settings.agentRadius = m_agentRadius;
	settings.agentMaxClimb = m_agentMaxClimb;
	settings.agentMaxSlope = m_agentMaxSlope;
	settings.regionMinSize = m_regionMinSize;
	settings.regionMergeSize = m_regionMergeSize;
	settings.edgeMaxLen = m_edgeMaxLen;
	settings.edgeMaxError = m_edgeMaxError;
	settings.vertsPerPoly = m_vertsPerPoly;
	settings.detailSampleDist = m_detailSampleDist;
	settings.detailSampleMaxError = m_detailSampleMaxError;
	settings.partitionType = m_partitionType;
}


bool RecastBase::LoadMeshBin(const char* filePath)
{
	dtFreeNavMesh(m_navMesh);
	m_navMesh = this->LoadAll(filePath);
	const dtStatus status = m_navQuery->init(m_navMesh, 2048);
	this->m_pathFinder->Reset();
	this->m_pathFinder->CollectNavData(this);
	return dtStatusSucceed(status);
}

static const int NAVMESHSET_MAGIC = 'M' << 24 | 'S' << 16 | 'E' << 8 | 'T'; //'MSET';
static const int NAVMESHSET_VERSION = 1;

struct NavMeshSetHeader
{
	int magic;
	int version;
	int numTiles;
	dtNavMeshParams params;
};

struct NavMeshTileHeader
{
	dtTileRef tileRef;
	int dataSize;
};

dtNavMesh* RecastBase::LoadAll(const char* path) const
{
	FILE* fp = fopen(path, "rb");
	if (!fp) return 0;

	// Read header.
	NavMeshSetHeader header;
	size_t readLen = fread(&header, sizeof(NavMeshSetHeader), 1, fp);
	if (readLen != 1)
	{
		fclose(fp);
		return 0;
	}
	if (header.magic != NAVMESHSET_MAGIC)
	{
		fclose(fp);
		return 0;
	}
	if (header.version != NAVMESHSET_VERSION)
	{
		fclose(fp);
		return 0;
	}

	dtNavMesh* mesh = dtAllocNavMesh();
	if (!mesh)
	{
		fclose(fp);
		return 0;
	}
	dtStatus status = mesh->init(&header.params);
	if (dtStatusFailed(status))
	{
		fclose(fp);
		return 0;
	}

	// Read tiles.
	for (int i = 0; i < header.numTiles; ++i)
	{
		NavMeshTileHeader tileHeader;
		readLen = fread(&tileHeader, sizeof(tileHeader), 1, fp);
		if (readLen != 1)
		{
			fclose(fp);
			return 0;
		}

		if (!tileHeader.tileRef || !tileHeader.dataSize)
			break;

		unsigned char* data = (unsigned char*)dtAlloc(tileHeader.dataSize, DT_ALLOC_PERM);
		if (!data) break;
		memset(data, 0, tileHeader.dataSize);
		readLen = fread(data, tileHeader.dataSize, 1, fp);
		if (readLen != 1)
		{
			dtFree(data);
			fclose(fp);
			return 0;
		}

		mesh->addTile(data, tileHeader.dataSize, DT_TILE_FREE_DATA, tileHeader.tileRef, 0);
	}

	fclose(fp);

	return mesh;
}

bool RecastBase::SaveAll(const char* path, const dtNavMesh* mesh) const
{
	if (!mesh)
	{
		this->m_ctx->log(RC_LOG_ERROR, "the navmesh data is null");
		return false;
	}

	FILE* fp = fopen(path, "wb");
	if (!fp)
	{
		this->m_ctx->log(RC_LOG_ERROR, "the path %s is invalid", path);
		return false;
	}

	// Store header.
	NavMeshSetHeader header;
	header.magic = NAVMESHSET_MAGIC;
	header.version = NAVMESHSET_VERSION;
	header.numTiles = 0;
	for (int i = 0; i < mesh->getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = mesh->getTile(i);
		if (!tile || !tile->header || !tile->dataSize) continue;
		header.numTiles++;
	}
	memcpy(&header.params, mesh->getParams(), sizeof(dtNavMeshParams));
	fwrite(&header, sizeof(NavMeshSetHeader), 1, fp);

	// Store tiles.
	for (int i = 0; i < mesh->getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = mesh->getTile(i);
		if (!tile || !tile->header || !tile->dataSize) continue;

		NavMeshTileHeader tileHeader;
		tileHeader.tileRef = mesh->getTileRef(tile);
		tileHeader.dataSize = tile->dataSize;
		fwrite(&tileHeader, sizeof(tileHeader), 1, fp);

		fwrite(tile->data, tile->dataSize, 1, fp);
	}

	fclose(fp);
	return true;
}

dtStatus RecastBase::FindSmoothPath(float* startPos, float* endPos, float* smoothPath, int* nsmooth) const
{
	dtStatus status = this->m_pathFinder->FindSmoothPath(startPos, endPos);
	if (dtStatusSucceed(status))
	{
		const auto path = this->m_pathFinder->getSmoothPath();
		const int count = this->m_pathFinder->getSmoothPathCount() * 3;
		for (int i = 0; i < count; i++) 
			smoothPath[i] = path[i];
		//memcpy(&smoothPath, m_pathFinder->getSmoothPath(), sizeof(float) * m_pathFinder->getSmoothPathCount() * 3);
		*nsmooth = this->m_pathFinder->getSmoothPathCount();
	}
	return status;
}

dtStatus RecastBase::FindStraightPath(float* startPos, float* endPos, float* straightPath, int* nstraight) const
{
	dtStatus status = this->m_pathFinder->FindStraightPath(startPos, endPos);

	if(dtStatusSucceed(status))
	{
		const auto path = this->m_pathFinder->getStraightPath();
		const int count = this->m_pathFinder->getStraightPathCount() * 3;
		for (int i = 0; i < count; i++)
			straightPath[i] = path[i];
		*nstraight = this->m_pathFinder->getStraightPathCount();
	}

	return status;
}

dtStatus RecastBase::FindRandomPoint(float* resultPos) const
{
	return this->m_pathFinder->FindRandomPoint(resultPos);
}

dtStatus RecastBase::FindRandomPointAroundCircle(float* center, float radius, float* resultPos) const
{
	return this->m_pathFinder->FindRandomPointAroundCircle(center, radius, resultPos);
}

dtStatus RecastBase::Raycast(float* startPos, float* endPos, float* hitPos) const
{
	dtStatus status = this->m_pathFinder->Raycast(startPos, endPos);
	if (dtStatusSucceed(status))
	{
		dtVcopy(hitPos, this->m_pathFinder->getHitPos());
	}
	return status;
}

void RecastBase::SetFilterFlag(int flag, bool isIncluded) const
{
	this->m_pathFinder->SetFilterFlag(flag, isIncluded);
}

void RecastBase::SetAreaCost(int area, float cost) const
{
	this->m_pathFinder->SetAreaCost(area, cost);
}


void RecastBase::AddConvexPoint(float* p) const
{
	this->m_convexPolygon->AddPoint(p);
}

bool RecastBase::MakeConvexPolygon(int areaType) const
{
	return this->m_convexPolygon->Make(areaType);
}

bool RecastBase::DeleteConvexPolygon(float* p) const
{
	return this->m_convexPolygon->Delete(p);
}