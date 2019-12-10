#pragma once

//
// Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
//
// This software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
//


/// These are just sample areas to use consistent values across the samples.
/// The use should specify these base on his needs.
enum PolyAreas
{
	POLYAREA_GROUND = 0,
	POLYAREA_WATER = 1,
	POLYAREA_ROAD = 2,
	POLYAREA_DOOR = 3,
	POLYAREA_GRASS = 4,
	POLYAREA_JUMP = 5,
};

enum PolyFlags
{
	POLYFLAGS_WALK = 0x01,		// Ability to walk (ground, grass, road)
	POLYFLAGS_SWIM = 0x02,		// Ability to swim (water).
	POLYFLAGS_DOOR = 0x04,		// Ability to move through doors.
	POLYFLAGS_JUMP = 0x08,		// Ability to jump.
	POLYFLAGS_DISABLED = 0x10,		// Disabled polygon
	POLYFLAGS_ALL = 0xffff	// All abilities.
};

enum PartitionType
{
	PARTITION_WATERSHED = 0,
	PARTITION_MONOTONE = 1,
	PARTITION_LAYERS = 2,
};

class RecastBase
{
protected:
	InputGeom* m_geom;
	dtNavMesh* m_navMesh;
	dtNavMeshQuery* m_navQuery;
	dtCrowd* m_crowd;

	float m_cellSize;
	float m_cellHeight;
	float m_agentHeight;
	float m_agentRadius;
	float m_agentMaxClimb;
	float m_agentMaxSlope;
	float m_regionMinSize;
	float m_regionMergeSize;
	float m_edgeMaxLen;
	float m_edgeMaxError;
	float m_vertsPerPoly;
	float m_detailSampleDist;
	float m_detailSampleMaxError;
	int m_partitionType;

	bool m_filterLowHangingObstacles;
	bool m_filterLedgeSpans;
	bool m_filterWalkableLowHeightSpans;

	PathFinder* m_pathFinder;
	ConvexPolygon* m_convexPolygon;
	BuildContext* m_ctx;

	dtNavMesh* LoadAll(const char* path) const;
	bool SaveAll(const char* path, const dtNavMesh* mesh) const;

	void ResetSettings();
public:
	RecastBase(BuildContext* ctx);
	virtual ~RecastBase();

	void SetContext(BuildContext* ctx) { this->m_ctx = ctx; }
	BuildContext* GetContext() const { return this->m_ctx; }

	virtual void SetBuildParams(float agentHeight, float agentRadius, float agentMaxClimb, float agentMaxSlope,
		float cellSize, float cellHeight, float regionMinSize, float regionMergeSize,
		float edgeMaxLen, float edgeMaxError, float vertsPerPoly, float detailSampleDist, float detailSampleMaxError,
		int partitionType, float tileSize);

	virtual bool LoadMeshGeometry(const char* filePath);
	virtual bool Build() = 0;
	virtual bool SaveMeshBin(const char* filePath);
	virtual bool LoadMeshBin(const char* filePath);

	virtual bool SaveGeomSet(const std::string& filepath);
	virtual bool LoadGeomSet(const std::string& filepath);

	virtual void CollectSettings(BuildSettings& settings);

	dtStatus FindSmoothPath(float* startPos, float* endPos, float* smoothPath, int* nsmooth) const;
	dtStatus FindStraightPath(float* startPos, float* endPos, float* straightPath, int* nstraight) const;
	dtStatus FindRandomPoint(float* resultPos) const;
	dtStatus FindRandomPointAroundCircle(float* center, float radius, float* resultPos) const;
	dtStatus Raycast(float* startPos, float* endPos, float* hitPos) const;

	void SetFilterFlag(int flag, bool isIncluded) const;
	void SetAreaCost(int area, float cost) const;

	void AddConvexPoint(float* p) const;
	bool MakeConvexPolygon(int areaType) const;
	bool DeleteConvexPolygon(float* p) const;

	InputGeom* GetInputGeom() const { return this->m_geom; }
	dtNavMesh* GetNavMesh() const { return this->m_navMesh; }
	dtNavMeshQuery* GetNavMeshQuery() const { return this->m_navQuery; }
	dtCrowd* GetCrowd() const { return this->m_crowd; }
	float GetAgentRadius() const { return this->m_agentRadius; }
	float GetAgentHeight() const { return this->m_agentHeight; }
	float GetAgentClimb() const { return this->m_agentMaxClimb; }

private:
	// Explicitly disabled copy constructor and copy assignment operator.
	RecastBase(const RecastBase&);
	RecastBase& operator=(const RecastBase&);
};
