#pragma once

class TileRecast : public RecastBase
{
protected:
	bool m_keepInterResults;
	bool m_buildAll;
	float m_totalBuildTimeMs;

	unsigned char* m_triareas;
	rcHeightfield* m_solid;
	rcCompactHeightfield* m_chf;
	rcContourSet* m_cset;
	rcPolyMesh* m_pmesh;
	rcPolyMeshDetail* m_dmesh;
	rcConfig m_cfg;

	int m_maxTiles;
	int m_maxPolysPerTile;
	float m_tileSize;

	unsigned int m_tileCol;
	float m_lastBuiltTileBmin[3];
	float m_lastBuiltTileBmax[3];
	float m_tileBuildTime;
	float m_tileMemUsage;
	int m_tileTriCount;

	unsigned char* BuildTileMesh(const int tx, const int ty, const float* bmin, const float* bmax, int& dataSize);

	void Cleanup();

public:
	TileRecast(BuildContext* ctx);
	virtual ~TileRecast();

	void SetBuildParams(float agentHeight, float agentRadius, float agentMaxClimb, float agentMaxSlope,
		float cellSize, float cellHeight, float regionMinSize, float regionMergeSize,
		float edgeMaxLen, float edgeMaxError, float vertsPerPoly, float detailSampleDist, float detailSampleMaxError,
		int partitionType, float tileSize) override;
	bool LoadMeshGeometry(const char* filePath) override;
	bool Build() override;
	bool SaveMeshBin(const char* filePath) override;
	bool LoadMeshBin(const char* filePath) override;
	void CollectSettings(BuildSettings& settings) override;

	/*void getTilePos(const float* pos, int& tx, int& ty);
	void buildTile(const float* pos);
	void removeTile(const float* pos);*/
	void BuildAllTiles();
	void BuildTile(float* minPoint, float* maxPoint);
	void RemoveTile(float* minPoint, float* maxPoint);
	//void removeAllTiles();

private:
	void BuildTileByXY(int x, int y);
	void RemoveTileByXY(int x, int y);

	// Explicitly disabled copy constructor and copy assignment operator.
	TileRecast(const TileRecast&);
	TileRecast& operator=(const TileRecast&);
};



