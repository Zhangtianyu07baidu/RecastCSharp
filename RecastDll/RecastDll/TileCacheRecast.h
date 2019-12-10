#pragma once

class TileCacheRecast : public RecastBase
{
protected:
	bool m_keepInterResults;

	struct LinearAllocator* m_talloc;
	struct FastLZCompressor* m_tcomp;
	struct MeshProcess* m_tmproc;

	class dtTileCache* m_tileCache;

	float m_cacheBuildTimeMs;
	int m_cacheCompressedSize;
	int m_cacheRawSize;
	int m_cacheLayerCount;
	unsigned int m_cacheBuildMemUsage;

	int m_maxTiles;
	int m_maxPolysPerTile;
	float m_tileSize;

public:
	TileCacheRecast(BuildContext* ctx);
	virtual ~TileCacheRecast();

	void SetBuildParams(float agentHeight, float agentRadius, float agentMaxClimb, float agentMaxSlope,
		float cellSize, float cellHeight, float regionMinSize, float regionMergeSize,
		float edgeMaxLen, float edgeMaxError, float vertsPerPoly, float detailSampleDist, float detailSampleMaxError,
		int partitionType, float tileSize) override;
	bool LoadMeshGeometry(const char* filePath) override;
	bool SaveMeshBin(const char* filePath) override;
	bool LoadMeshBin(const char* filePath) override;
	void CollectSettings(BuildSettings& settings) override;
	bool Build() override;
	void Update(float dt) const;
	bool AddCylinderObstacle(const float* pos, const float radius, const float height, dtObstacleRef& result) const;
	bool AddBoxObstacle(const float* bmin, const float* bmax, dtObstacleRef& result) const;
	bool RemoveObstacle(dtObstacleRef ref) const;
	void ClearAllObstacles() const;

private:
	void saveBin(const char* path) const;
	void loadBin(const char* path);
	int rasterizeTileLayers(const int tx, const int ty, const rcConfig& cfg, struct TileCacheData* tiles, const int maxTiles);
};

