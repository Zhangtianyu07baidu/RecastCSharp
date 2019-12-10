#include "stdafx.h"

// Returns a random number [0..1]
static float frand()
{
	//	return ((float)(rand() & 0xffff)/(float)0xffff);
	return (float)rand() / (float)RAND_MAX;
}

inline bool inRange(const float* v1, const float* v2, const float r, const float h)
{
	const float dx = v2[0] - v1[0];
	const float dy = v2[1] - v1[1];
	const float dz = v2[2] - v1[2];
	return (dx*dx + dz * dz) < r*r && fabsf(dy) < h;
}


static int fixupCorridor(dtPolyRef* path, const int npath, const int maxPath,
	const dtPolyRef* visited, const int nvisited)
{
	int furthestPath = -1;
	int furthestVisited = -1;

	// Find furthest common polygon.
	for (int i = npath - 1; i >= 0; --i)
	{
		bool found = false;
		for (int j = nvisited - 1; j >= 0; --j)
		{
			if (path[i] == visited[j])
			{
				furthestPath = i;
				furthestVisited = j;
				found = true;
			}
		}
		if (found)
			break;
	}

	// If no intersection found just return current path. 
	if (furthestPath == -1 || furthestVisited == -1)
		return npath;

	// Concatenate paths.	

	// Adjust beginning of the buffer to include the visited.
	const int req = nvisited - furthestVisited;
	const int orig = rcMin(furthestPath + 1, npath);
	int size = rcMax(0, npath - orig);
	if (req + size > maxPath)
		size = maxPath - req;
	if (size)
		memmove(path + req, path + orig, size * sizeof(dtPolyRef));

	// Store visited
	for (int i = 0; i < req; ++i)
		path[i] = visited[(nvisited - 1) - i];

	return req + size;
}

// This function checks if the path has a small U-turn, that is,
// a polygon further in the path is adjacent to the first polygon
// in the path. If that happens, a shortcut is taken.
// This can happen if the target (T) location is at tile boundary,
// and we're (S) approaching it parallel to the tile edge.
// The choice at the vertex can be arbitrary, 
//  +---+---+
//  |:::|:::|
//  +-S-+-T-+
//  |:::|   | <-- the step can end up in here, resulting U-turn path.
//  +---+---+
static int fixupShortcuts(dtPolyRef* path, int npath, dtNavMeshQuery* navQuery)
{
	if (npath < 3)
		return npath;

	// Get connected polygons
	static const int maxNeis = 16;
	dtPolyRef neis[maxNeis];
	int nneis = 0;

	const dtMeshTile* tile = 0;
	const dtPoly* poly = 0;
	if (dtStatusFailed(navQuery->getAttachedNavMesh()->getTileAndPolyByRef(path[0], &tile, &poly)))
		return npath;

	for (unsigned int k = poly->firstLink; k != DT_NULL_LINK; k = tile->links[k].next)
	{
		const dtLink* link = &tile->links[k];
		if (link->ref != 0)
		{
			if (nneis < maxNeis)
				neis[nneis++] = link->ref;
		}
	}

	// If any of the neighbour polygons is within the next few polygons
	// in the path, short cut to that polygon directly.
	static const int maxLookAhead = 6;
	int cut = 0;
	for (int i = dtMin(maxLookAhead, npath) - 1; i > 1 && cut == 0; i--) {
		for (int j = 0; j < nneis; j++)
		{
			if (path[i] == neis[j]) {
				cut = i;
				break;
			}
		}
	}
	if (cut > 1)
	{
		int offset = cut - 1;
		npath -= offset;
		for (int i = 1; i < npath; i++)
			path[i] = path[i + offset];
	}

	return npath;
}

static bool getSteerTarget(dtNavMeshQuery* navQuery, const float* startPos, const float* endPos,
	const float minTargetDist,
	const dtPolyRef* path, const int pathSize,
	float* steerPos, unsigned char& steerPosFlag, dtPolyRef& steerPosRef,
	float* outPoints = 0, int* outPointCount = 0)
{
	// Find steer target.
	static const int MAX_STEER_POINTS = 3;
	float steerPath[MAX_STEER_POINTS * 3];
	unsigned char steerPathFlags[MAX_STEER_POINTS];
	dtPolyRef steerPathPolys[MAX_STEER_POINTS];
	int nsteerPath = 0;
	navQuery->findStraightPath(startPos, endPos, path, pathSize,
		steerPath, steerPathFlags, steerPathPolys, &nsteerPath, MAX_STEER_POINTS);
	if (!nsteerPath)
		return false;

	if (outPoints && outPointCount)
	{
		*outPointCount = nsteerPath;
		for (int i = 0; i < nsteerPath; ++i)
			dtVcopy(&outPoints[i * 3], &steerPath[i * 3]);
	}


	// Find vertex far enough to steer to.
	int ns = 0;
	while (ns < nsteerPath)
	{
		// Stop at Off-Mesh link or when point is further than slop away.
		if ((steerPathFlags[ns] & DT_STRAIGHTPATH_OFFMESH_CONNECTION) ||
			!inRange(&steerPath[ns * 3], startPos, minTargetDist, 1000.0f))
			break;
		ns++;
	}
	// Failed to find good point to steer to.
	if (ns >= nsteerPath)
		return false;

	dtVcopy(steerPos, &steerPath[ns * 3]);
	steerPos[1] = startPos[1];
	steerPosFlag = steerPathFlags[ns];
	steerPosRef = steerPathPolys[ns];

	return true;
}

PathFinder::PathFinder() :
	m_recast(0),
	m_navMesh(0),
	m_navQuery(0),
	m_straightPathOptions(0),
	m_startRef(0),
	m_endRef(0),
	m_npolys(0),
	m_nstraightPath(0),
	m_nsmoothPath(0),
	m_nrandPoints(0),
	m_randPointsInCircle(false),
	m_hitResult(false),
	m_distanceToWall(0),
	m_pathIterPolyCount(0),
	m_steerPointCount(0)
{
	m_filter.setIncludeFlags(POLYFLAGS_ALL ^ POLYFLAGS_DISABLED);
	m_filter.setExcludeFlags(0);

	m_polyPickExt[0] = 2;
	m_polyPickExt[1] = 4;
	m_polyPickExt[2] = 2;

	m_neighbourhoodRadius = 2.5f;
	m_randomRadius = 5.0f;
}


PathFinder::~PathFinder()
{

}

void PathFinder::CollectNavData(RecastBase* recast)
{
	this->m_recast = recast;
	this->m_navMesh = recast->GetNavMesh();
	this->m_navQuery = recast->GetNavMeshQuery();

	if (this->m_navQuery)
	{
		// Change costs.
		this->m_filter.setAreaCost(POLYAREA_GROUND, 1.0f);
		this->m_filter.setAreaCost(POLYAREA_WATER, 10.0f);
		this->m_filter.setAreaCost(POLYAREA_ROAD, 1.0f);
		this->m_filter.setAreaCost(POLYAREA_DOOR, 1.0f);
		this->m_filter.setAreaCost(POLYAREA_GRASS, 2.0f);
		this->m_filter.setAreaCost(POLYAREA_JUMP, 1.5f);
	}

	this->m_neighbourhoodRadius = recast->GetAgentRadius() * 20.0f;
	this->m_randomRadius = recast->GetAgentRadius() * 30.0f;
}


void PathFinder::Reset()
{
	this->m_startRef = 0;
	this->m_endRef = 0;
	this->m_npolys = 0;
	this->m_nstraightPath = 0;
	this->m_nsmoothPath = 0;
	memset(this->m_hitPos, 0, sizeof(this->m_hitPos));
	memset(this->m_hitNormal, 0, sizeof(this->m_hitNormal));
	this->m_distanceToWall = 0;
}

void PathFinder::SetFilterFlag(int flag, bool isIncluded)
{
	if(isIncluded)
	{
		if ((m_filter.getIncludeFlags() & flag) != 0)
		{
			m_filter.setIncludeFlags(m_filter.getIncludeFlags() ^ flag);
		}
	}
	else
	{
		if ((m_filter.getExcludeFlags() & flag) != 0)
		{
			m_filter.setExcludeFlags(m_filter.getExcludeFlags() ^ flag);
		}
	}
}

void PathFinder::SetAreaCost(int area, float cost)
{
	if(this->m_navQuery)
	{
		this->m_filter.setAreaCost(area, cost);
	}
}


dtStatus PathFinder::FindSmoothPath(float* startPos, float* endPos)
{
	dtStatus status = DT_FAILURE;

	if (!m_navMesh)
		return status;

	m_navQuery->findNearestPoly(startPos, m_polyPickExt, &m_filter, &m_startRef, 0);

	m_navQuery->findNearestPoly(endPos, m_polyPickExt, &m_filter, &m_endRef, 0);

	if (m_startRef && m_endRef)
	{
		m_navQuery->findPath(m_startRef, m_endRef, startPos, endPos, &m_filter, m_polys, &m_npolys, MAX_POLYS);

		m_nsmoothPath = 0;

		if (m_npolys)
		{
			// Iterate over the path to find smooth path on the detail mesh surface.
			dtPolyRef polys[MAX_POLYS];
			memcpy(polys, m_polys, sizeof(dtPolyRef)*m_npolys);
			int npolys = m_npolys;

			float iterPos[3], targetPos[3];
			m_navQuery->closestPointOnPoly(m_startRef, startPos, iterPos, 0);
			m_navQuery->closestPointOnPoly(polys[npolys - 1], endPos, targetPos, 0);

			static const float STEP_SIZE = 0.5f;
			static const float SLOP = 0.01f;

			m_nsmoothPath = 0;

			dtVcopy(&m_smoothPath[m_nsmoothPath * 3], iterPos);
			m_nsmoothPath++;

			// Move towards target a small advancement at a time until target reached or
			// when ran out of memory to store the path.
			while (npolys && m_nsmoothPath < MAX_SMOOTH)
			{
				// Find location to steer towards.
				float steerPos[3];
				unsigned char steerPosFlag;
				dtPolyRef steerPosRef;

				if (!getSteerTarget(m_navQuery, iterPos, targetPos, SLOP,
					polys, npolys, steerPos, steerPosFlag, steerPosRef))
					break;

				bool endOfPath = (steerPosFlag & DT_STRAIGHTPATH_END) ? true : false;
				bool offMeshConnection = (steerPosFlag & DT_STRAIGHTPATH_OFFMESH_CONNECTION) ? true : false;

				// Find movement delta.
				float delta[3], len;
				dtVsub(delta, steerPos, iterPos);
				len = dtMathSqrtf(dtVdot(delta, delta));
				// If the steer target is end of path or off-mesh link, do not move past the location.
				if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
					len = 1;
				else
					len = STEP_SIZE / len;
				float moveTgt[3];
				dtVmad(moveTgt, iterPos, delta, len);

				// Move
				float result[3];
				dtPolyRef visited[16];
				int nvisited = 0;
				m_navQuery->moveAlongSurface(polys[0], iterPos, moveTgt, &m_filter,
					result, visited, &nvisited, 16);

				npolys = fixupCorridor(polys, npolys, MAX_POLYS, visited, nvisited);
				npolys = fixupShortcuts(polys, npolys, m_navQuery);

				float h = 0;
				m_navQuery->getPolyHeight(polys[0], result, &h);
				result[1] = h;
				dtVcopy(iterPos, result);

				// Handle end of path and off-mesh links when close enough.
				if (endOfPath && inRange(iterPos, steerPos, SLOP, 1.0f))
				{
					// Reached end of path.
					dtVcopy(iterPos, targetPos);
					if (m_nsmoothPath < MAX_SMOOTH)
					{
						dtVcopy(&m_smoothPath[m_nsmoothPath * 3], iterPos);
						m_nsmoothPath++;
					}
					break;
				}
				else if (offMeshConnection && inRange(iterPos, steerPos, SLOP, 1.0f))
				{
					// Reached off-mesh connection.
					float startPos[3], endPos[3];

					// Advance the path up to and over the off-mesh connection.
					dtPolyRef prevRef = 0, polyRef = polys[0];
					int npos = 0;
					while (npos < npolys && polyRef != steerPosRef)
					{
						prevRef = polyRef;
						polyRef = polys[npos];
						npos++;
					}
					for (int i = npos; i < npolys; ++i)
						polys[i - npos] = polys[i];
					npolys -= npos;

					// Handle the connection.
					dtStatus status = m_navMesh->getOffMeshConnectionPolyEndPoints(prevRef, polyRef, startPos, endPos);
					if (dtStatusSucceed(status))
					{
						if (m_nsmoothPath < MAX_SMOOTH)
						{
							dtVcopy(&m_smoothPath[m_nsmoothPath * 3], startPos);
							m_nsmoothPath++;
							// Hack to make the dotted path not visible during off-mesh connection.
							if (m_nsmoothPath & 1)
							{
								dtVcopy(&m_smoothPath[m_nsmoothPath * 3], startPos);
								m_nsmoothPath++;
							}
						}
						// Move position at the other side of the off-mesh link.
						dtVcopy(iterPos, endPos);
						float eh = 0.0f;
						m_navQuery->getPolyHeight(polys[0], iterPos, &eh);
						iterPos[1] = eh;
					}
				}

				// Store results.
				if (m_nsmoothPath < MAX_SMOOTH)
				{
					dtVcopy(&m_smoothPath[m_nsmoothPath * 3], iterPos);
					m_nsmoothPath++;
				}
			}

			status = DT_SUCCESS;
		}
	}
	else
	{
		m_npolys = 0;
		m_nsmoothPath = 0;
	}

	return status;
}

dtStatus PathFinder::FindStraightPath(float* startPos, float* endPos)
{
	dtStatus status = DT_FAILURE;

	if (!m_navMesh)
		return status;

	m_navQuery->findNearestPoly(startPos, m_polyPickExt, &m_filter, &m_startRef, 0);

	m_navQuery->findNearestPoly(endPos, m_polyPickExt, &m_filter, &m_endRef, 0);

	if (m_startRef && m_endRef)
	{
		m_navQuery->findPath(m_startRef, m_endRef, startPos, endPos, &m_filter, m_polys, &m_npolys, MAX_POLYS);
		m_nstraightPath = 0;
		if (m_npolys)
		{
			// In case of partial path, make sure the end point is clamped to the last polygon.
			float epos[3];
			dtVcopy(epos, endPos);
			if (m_polys[m_npolys - 1] != m_endRef)
				m_navQuery->closestPointOnPoly(m_polys[m_npolys - 1], endPos, epos, 0);

			m_navQuery->findStraightPath(startPos, epos, m_polys, m_npolys,
				m_straightPath, m_straightPathFlags,
				m_straightPathPolys, &m_nstraightPath, MAX_POLYS, m_straightPathOptions);

			status = DT_SUCCESS;
		}
	}
	else
	{
		m_npolys = 0;
		m_nstraightPath = 0;
	}

	return status;
}

dtStatus PathFinder::FindRandomPoint(float* resultPos)
{
	if (!resultPos)
	{
		return DT_FAILURE | DT_INVALID_PARAM;
	}
	const dtStatus status = m_navQuery->findRandomPoint(&m_filter, frand, &m_startRef, resultPos);
	if (!dtStatusSucceed(status))
	{
		this->m_recast->GetContext()->log(RC_LOG_WARNING, "Can't find random point");
	}

	return status;
}

dtStatus PathFinder::FindRandomPointAroundCircle(float* center, float radius, float* resultPos)
{
	if (!center || !resultPos)
	{
		return DT_FAILURE | DT_INVALID_PARAM;
	}

	dtStatus status = m_navQuery->findNearestPoly(center, m_polyPickExt, &m_filter, &m_startRef, 0);
	if (!dtStatusSucceed(status))
	{
		this->m_recast->GetContext()->log(RC_LOG_WARNING, "Can't find nearest Poly at center point");
		return status;
	}

	status = m_navQuery->findRandomPointAroundCircle(m_startRef, center, radius, &m_filter, frand, &m_endRef, resultPos);
	if (!dtStatusSucceed(status))
	{
		this->m_recast->GetContext()->log(RC_LOG_WARNING, "Can't find random point around center point by radius %d", radius);
	}

	return status;
}

dtStatus PathFinder::Raycast(float* startPos, float* endPos)
{
	if (!startPos || !endPos)
	{
		return DT_FAILURE | DT_INVALID_PARAM;
	}
	dtVcopy(m_spos, startPos);
	dtVcopy(m_epos, endPos);

	dtStatus status = m_navQuery->findNearestPoly(m_spos, m_polyPickExt, &m_filter, &m_startRef, 0);
	if (!dtStatusSucceed(status))
	{
		this->m_recast->GetContext()->log(RC_LOG_WARNING, "Can't find nearest Poly at start point");
		return status;
	}

	float t = 0;
	m_npolys = 0;

	m_navQuery->raycast(m_startRef, m_spos, m_epos, &m_filter, &t, m_hitNormal, m_polys, &m_npolys, MAX_POLYS);
	if (t > 1)
	{
		// No hit
		dtVcopy(m_hitPos, m_epos);
		m_hitResult = false;
	}
	else
	{
		// Hit
		dtVlerp(m_hitPos, m_spos, m_epos, t);
		m_hitResult = true;
	}
	// Adjust height.
	if (m_npolys > 0)
	{
		float h = 0;
		m_navQuery->getPolyHeight(m_polys[m_npolys - 1], m_hitPos, &h);
		m_hitPos[1] = h;
	}

	return status;
}




