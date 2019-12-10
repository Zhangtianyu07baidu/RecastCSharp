#pragma once
class RecastBase;

class PathFinder
{
	RecastBase* m_recast;

	dtNavMesh* m_navMesh;
	dtNavMeshQuery* m_navQuery;

	dtQueryFilter m_filter;

	int m_straightPathOptions;

	static const int MAX_POLYS = 256;
	static const int MAX_SMOOTH = 2048;

	dtPolyRef m_startRef;
	dtPolyRef m_endRef;
	dtPolyRef m_polys[MAX_POLYS];
	dtPolyRef m_parent[MAX_POLYS];
	int m_npolys;
	float m_straightPath[MAX_POLYS * 3];
	unsigned char m_straightPathFlags[MAX_POLYS];
	dtPolyRef m_straightPathPolys[MAX_POLYS];
	int m_nstraightPath;
	float m_polyPickExt[3];
	float m_smoothPath[MAX_SMOOTH * 3];
	int m_nsmoothPath;
	float m_queryPoly[4 * 3];

	static const int MAX_RAND_POINTS = 64;
	float m_randPoints[MAX_RAND_POINTS * 3];
	int m_nrandPoints;
	bool m_randPointsInCircle;

	float m_spos[3];
	float m_epos[3];
	float m_hitPos[3];
	float m_hitNormal[3];
	bool m_hitResult;
	float m_distanceToWall;
	float m_neighbourhoodRadius;
	float m_randomRadius;

	dtPolyRef m_pathIterPolys[MAX_POLYS];
	int m_pathIterPolyCount;
	float m_prevIterPos[3], m_iterPos[3], m_steerPos[3], m_targetPos[3];

	static const int MAX_STEER_POINTS = 10;
	float m_steerPoints[MAX_STEER_POINTS * 3];
	int m_steerPointCount;

public:
	PathFinder();
	virtual ~PathFinder();

	void CollectNavData(RecastBase* recast);
	void Reset();

	void SetFilterFlag(int flag, bool isIncluded);
	void SetAreaCost(int area, float cost);
	dtStatus FindSmoothPath(float* startPos, float* endPos);
	dtStatus FindStraightPath(float* startPos, float* endPos);
	dtStatus FindRandomPoint(float* resultPos);
	dtStatus FindRandomPointAroundCircle(float* center, float radius, float* resultPos);
	dtStatus Raycast(float* startPos, float* endPos);

	float* getStraightPath() { return m_straightPath; }
	int getStraightPathCount() const { return m_nstraightPath; }
	float* getSmoothPath() { return m_smoothPath; }
	int getSmoothPathCount() const { return m_nsmoothPath; }
	float* getStartPos() { return this->m_spos; }
	float* getEndPos() { return this->m_epos; }
	float* getHitPos() { return this->m_hitPos; }
};

