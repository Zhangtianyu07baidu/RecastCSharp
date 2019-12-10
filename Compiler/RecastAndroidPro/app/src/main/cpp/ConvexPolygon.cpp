#include "stdafx.h"

// Returns true if 'c' is left of line 'a'-'b'.
inline bool left(const float* a, const float* b, const float* c)
{
	const float u1 = b[0] - a[0];
	const float v1 = b[2] - a[2];
	const float u2 = c[0] - a[0];
	const float v2 = c[2] - a[2];
	return u1 * v2 - v1 * u2 < 0;
}

// Returns true if 'a' is more lower-left than 'b'.
inline bool cmppt(const float* a, const float* b)
{
	if (a[0] < b[0]) return true;
	if (a[0] > b[0]) return false;
	if (a[2] < b[2]) return true;
	if (a[2] > b[2]) return false;
	return false;
}
// Calculates convex hull on xz-plane of points on 'pts',
// stores the indices of the resulting hull in 'out' and
// returns number of points on hull.
static int convexhull(const float* pts, int npts, int* out)
{
	// Find lower-leftmost point.
	int hull = 0;
	for (int i = 1; i < npts; ++i)
		if (cmppt(&pts[i * 3], &pts[hull * 3]))
			hull = i;
	// Gift wrap hull.
	int endpt = 0;
	int i = 0;
	do
	{
		out[i++] = hull;
		endpt = 0;
		for (int j = 1; j < npts; ++j)
			if (hull == endpt || left(&pts[hull * 3], &pts[endpt * 3], &pts[j * 3]))
				endpt = j;
		hull = endpt;
	} while (endpt != out[0]);

	return i;
}

static int pointInPoly(int nvert, const float* verts, const float* p)
{
	int i, j, c = 0;
	for (i = 0, j = nvert - 1; i < nvert; j = i++)
	{
		const float* vi = &verts[i * 3];
		const float* vj = &verts[j * 3];
		if (((vi[2] > p[2]) != (vj[2] > p[2])) &&
			(p[0] < (vj[0] - vi[0]) * (p[2] - vi[2]) / (vj[2] - vi[2]) + vi[0]))
			c = !c;
	}
	return c;
}

ConvexPolygon::ConvexPolygon(InputGeom* geom) :
	m_areaType(POLYAREA_GRASS),
	m_polyOffset(0.0f),
	m_boxHeight(6.0f),
	m_boxDescent(1.0f),
	m_npts(0),
	m_nhull(0)
{
	this->m_geom = geom;
}


ConvexPolygon::~ConvexPolygon()
{
	this->m_geom = NULL;
}

void ConvexPolygon::Reset()
{
	m_npts = 0;
	m_nhull = 0;
}

void ConvexPolygon::AddPoint(float* p)
{
	if (m_npts < MAX_PTS)
	{
		rcVcopy(&m_pts[m_npts * 3], p);
		m_npts++;
		// Update hull.
		if (m_npts > 1)
			m_nhull = convexhull(m_pts, m_npts, m_hull);
		else
			m_nhull = 0;
	}
}

bool ConvexPolygon::Make(int areaType)
{
	this->m_areaType = areaType;
	InputGeom* geom = this->m_geom;
	if (m_npts && geom)
	{
		if (m_nhull > 2)
		{
			// Create shape.
			float verts[MAX_PTS * 3];
			for (int i = 0; i < m_nhull; ++i)
				rcVcopy(&verts[i * 3], &m_pts[m_hull[i] * 3]);

			float minh = FLT_MAX, maxh = 0;
			for (int i = 0; i < m_nhull; ++i)
				minh = rcMin(minh, verts[i * 3 + 1]);
			minh -= m_boxDescent;
			maxh = minh + m_boxHeight;

			if (m_polyOffset > 0.01f)
			{
				float offset[MAX_PTS * 2 * 3];
				int noffset = rcOffsetPoly(verts, m_nhull, m_polyOffset, offset, MAX_PTS * 2);
				if (noffset > 0)
					geom->addConvexVolume(offset, noffset, minh, maxh, (unsigned char)m_areaType);
			}
			else
			{
				geom->addConvexVolume(verts, m_nhull, minh, maxh, (unsigned char)m_areaType);
			}
		}

		m_npts = 0;
		m_nhull = 0;
		return true;
	}

	this->Reset();
	return false;
}

bool ConvexPolygon::Delete(float* p)
{
	InputGeom* geom = this->m_geom;

	if (geom)
	{
		int nearestIndex = -1;
		const ConvexVolume* vols = geom->getConvexVolumes();
		for (int i = 0; i < geom->getConvexVolumeCount(); ++i)
		{
			if (pointInPoly(vols[i].nverts, vols[i].verts, p) &&
				p[1] >= vols[i].hmin && p[1] <= vols[i].hmax)
			{
				nearestIndex = i;
			}
		}
		// If end point close enough, delete it.
		if (nearestIndex != -1)
		{
			geom->deleteConvexVolume(nearestIndex);
			return true;
		}
	}

	return false;
}
