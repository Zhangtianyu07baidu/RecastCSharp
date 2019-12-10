#pragma once
class SoloRecast : public RecastBase
{

protected:
	bool m_keepInterResults;
	float m_totalBuildTimeMs;

	unsigned char* m_triareas;
	rcHeightfield* m_solid;
	rcCompactHeightfield* m_chf;
	rcContourSet* m_cset;
	rcPolyMesh* m_pmesh;
	rcConfig m_cfg;
	rcPolyMeshDetail* m_dmesh;

	void Cleanup();

public:
	SoloRecast(BuildContext* ctx);
	virtual ~SoloRecast();

	bool LoadMeshGeometry(const char* filePath) override;
	bool Build() override;
	bool SaveMeshBin(const char* filePath) override;
	bool LoadMeshBin(const char* filePath) override;

private:
	// Explicitly disabled copy constructor and copy assignment operator.
	SoloRecast(const SoloRecast&);
	SoloRecast& operator=(const SoloRecast&);

};

