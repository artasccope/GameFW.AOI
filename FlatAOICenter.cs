using CommonTools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameFW.AOI
{
    public class FlatAOICenter : Iaoi
    {
        private float m_iGridSize;
        private int m_iWidth;
        private int m_iHeight;
        private FlatAOIGrid[,] m_pAoiGrids;
        private Dictionary<int, FlatAOIGrid> m_pEntityGridMap;

        public FlatAOICenter(float gridSize, int width, int height) {
            this.m_iGridSize = gridSize;
            this.m_iWidth = width;
            this.m_iHeight = height;
            this.m_pAoiGrids = new FlatAOIGrid[m_iWidth,m_iHeight];
            m_pEntityGridMap = new Dictionary<int, FlatAOIGrid>();
        }

        public bool AddEntity(int id,ref Vector3 pos)
        {
            if (m_pEntityGridMap.ContainsKey(id))
                return false;

            int x = (int)(pos.x/m_iGridSize);//left
            int z = (int)(pos.z/m_iGridSize);//bottom
            if (m_pAoiGrids[x, z] == null)
                m_pAoiGrids[x, z] = new FlatAOIGrid();

            m_pEntityGridMap.Add(id, m_pAoiGrids[x,z]);
            m_pAoiGrids[x, z].AddEntity(id,ref pos);
            return true;
        }

        private List<RBTree<int, Vector3>> entities = new List<RBTree<int, Vector3>>(1);
        public List<RBTree<int, Vector3>> GetInterestEntities(int id,ref Vector3 pos, float range)
        {
            FlatAOIGrid grid = m_pEntityGridMap[id];
            entities.Clear();

            entities.Add(grid.GetEntities());

            if (grid.x < m_iWidth - 1 && pos.x + range > (grid.x+1) * m_iGridSize && m_pAoiGrids[grid.x + 1, grid.z] != null)
                entities.Add(m_pAoiGrids[grid.x + 1, grid.z].GetEntities());

            if (grid.x > 0 && pos.x - range < grid.x * m_iGridSize && m_pAoiGrids[grid.x - 1, grid.z] != null)
                entities.Add(m_pAoiGrids[grid.x - 1, grid.z].GetEntities());

            if (grid.z < m_iHeight - 1 && pos.z + range > (grid.z + 1) * m_iGridSize && m_pAoiGrids[grid.x, grid.z + 1] != null)
                entities.Add(m_pAoiGrids[grid.x, grid.z + 1].GetEntities());

            if (grid.z > 0 && pos.z - range < grid.z * m_iGridSize && m_pAoiGrids[grid.x, grid.z - 1] != null)
                entities.Add(m_pAoiGrids[grid.x, grid.z - 1].GetEntities());

            if (grid.x < m_iWidth - 1 && grid.z < m_iHeight - 1 && pos.x + range * 0.707106f > (grid.x + 1) * m_iGridSize
                && pos.z + range * 0.707106f > (grid.z + 1) * m_iGridSize && m_pAoiGrids[grid.x + 1, grid.z + 1] != null)
                entities.Add(m_pAoiGrids[grid.x + 1, grid.z + 1].GetEntities());

            if (grid.x > 0 && grid.z > 0 && pos.x - range * 0.707106f < grid.x * m_iGridSize
                && pos.z - range * 0.707106f < grid.z * m_iGridSize && m_pAoiGrids[grid.x - 1, grid.z - 1] != null)
                entities.Add(m_pAoiGrids[grid.x - 1, grid.z - 1].GetEntities());

            if (grid.x > 0 && grid.z < m_iHeight - 1 && pos.x - range * 0.707106f < grid.x * m_iGridSize
                && pos.z + range * 0.707106f > (grid.z + 1) * m_iGridSize && m_pAoiGrids[grid.x - 1, grid.z + 1] != null)
                entities.Add(m_pAoiGrids[grid.x - 1, grid.z + 1].GetEntities());

            if (grid.x < m_iWidth - 1 && grid.z > 0 && pos.x + range * 0.707106f > (grid.x+1) * m_iGridSize
                && pos.z - range * 0.707106f < grid.z * m_iGridSize && m_pAoiGrids[grid.x + 1, grid.z - 1] != null)
                entities.Add(m_pAoiGrids[grid.x + 1, grid.z - 1].GetEntities());

            return entities;
        }

        public bool RemoveEntity(int id)
        {
            if (!m_pEntityGridMap.ContainsKey(id))
                return false;

            m_pEntityGridMap[id].RemoveEntity(id);
            m_pEntityGridMap.Remove(id);
            return true;
        }

        public void UpdatePos(int id,ref Vector3 pos)
        {
            if (!m_pEntityGridMap.ContainsKey(id))
                return;

            FlatAOIGrid oldGrid = m_pEntityGridMap[id];
            int x = (int)(pos.x / m_iGridSize);
            int z = (int)(pos.z / m_iGridSize);
            if (x != oldGrid.x || z != oldGrid.z)
            {
                oldGrid.RemoveEntity(id);
                if (m_pAoiGrids[x, z] == null)
                    m_pAoiGrids[x, z] = new FlatAOIGrid();

                m_pAoiGrids[x, z].AddEntity(id,ref pos);
                m_pEntityGridMap[id] = m_pAoiGrids[x, z];
            }
            else {
                oldGrid.UpdatePos(id,ref pos);
            }
        }

        public bool ContainsKey(int id)
        {
            return m_pEntityGridMap.ContainsKey(id);
        }
    }
}
