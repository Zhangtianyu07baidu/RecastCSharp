#pragma once

class ConvexPolygon
{
	InputGeom* m_geom;
	int m_areaType;
	float m_polyOffset;
	float m_boxHeight;
	float m_boxDescent;

	static const int MAX_PTS = 12;
	float m_pts[MAX_PTS * 3];
	int m_npts;
	int m_hull[MAX_PTS];
	int m_nhull;

public:
	ConvexPolygon(InputGeom* recast);
	~ConvexPolygon();
	
	bool Delete(float* p);
	void AddPoint(float* p);
	bool Make(int areaType);

private:
	void Reset();
};

