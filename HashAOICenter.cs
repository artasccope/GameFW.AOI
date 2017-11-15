using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CommonTools;

namespace GameFW.AOI
{
    public class HashAOICenter : Iaoi
    {
        private float m_iGridSize;
        private int m_iWidth;
        private int m_iHeight;
        private Dictionary<int, RBTree<int, Vector3>> m_pEntityLists;
        /// <summary>
        /// entity id与AOI单元id的映射
        /// </summary>
        private Dictionary<int, int> m_pIdListMap;
        /// <summary>
        /// aoi格子缓存
        /// </summary>
        private ObjectPool<RBTree<int, Vector3>> m_pEntityListCache;

        public HashAOICenter(float gridSize, int width, int height)
        {
            this.m_iGridSize = gridSize;
            this.m_iWidth = width;
            this.m_iHeight = height;
            m_pEntityLists = new Dictionary<int, RBTree<int, Vector3>>();
            m_pIdListMap = new Dictionary<int, int>();
            m_pEntityListCache = new ObjectPool<RBTree<int, Vector3>>();
        }

        public bool AddEntity(int id,ref Vector3 pos)
        {
            if (m_pIdListMap.ContainsKey(id))
                return false;

            int x = Mathf.CeilToInt(pos.x / m_iGridSize);//left
            int y = Mathf.CeilToInt(pos.z / m_iGridSize);//bottom
            int index = y * m_iWidth + x;
            if (!m_pEntityLists.ContainsKey(index)) {
                m_pEntityLists.Add(index, GetUseableEntityList());
            }

            m_pEntityLists[index].Add(id, pos);
            m_pIdListMap.Add(id, index);
            return true;
        }

        public bool ContainsKey(int id)
        {
            return m_pIdListMap.ContainsKey(id);
        }

        private List<RBTree<int, Vector3>> entities = new List<RBTree<int, Vector3>>(1);
        public List<RBTree<int, Vector3>> GetInterestEntities(int id,ref Vector3 pos, float range)
        {
            if (!m_pIdListMap.ContainsKey(id))
                return null;

            int x = Mathf.CeilToInt(pos.x / m_iGridSize);//left
            int y = Mathf.CeilToInt(pos.z / m_iGridSize);//bottom
            int index = y * m_iWidth + x;
            entities.Clear();
            entities.Add(m_pEntityLists[index]);

            if (x < m_iWidth - 1 && pos.x + range > (x + 1) * m_iGridSize) {
                if(m_pEntityLists.ContainsKey(index + 1))
                    entities.Add(m_pEntityLists[index+1]);
            }

            if (x > 0 && pos.x - range < x * m_iGridSize) {
                if(m_pEntityLists.ContainsKey(index-1))
                    entities.Add(m_pEntityLists[index - 1]);
            }

            if (y < m_iHeight - 1 && pos.z + range > (y + 1) * m_iGridSize) {
                if (m_pEntityLists.ContainsKey(index + m_iWidth))
                    entities.Add(m_pEntityLists[index + m_iWidth]);
            }

            if (y > 0 && pos.z - range < y * m_iGridSize) {
                if(m_pEntityLists.ContainsKey(index - m_iWidth))
                    entities.Add(m_pEntityLists[index - m_iWidth]);
            }

            if (x < m_iWidth - 1 && y < m_iHeight - 1 && pos.x + range * 0.707106f > (x + 1) * m_iGridSize
                && pos.z + range * 0.707106f > (y + 1) * m_iGridSize) {
                if (m_pEntityLists.ContainsKey(index + m_iWidth + 1))
                    entities.Add(m_pEntityLists[index + m_iWidth + 1]);
            }

            if (x > 0 && y > 0 && pos.x - range * 0.707106f < x * m_iGridSize
                && pos.z - range * 0.707106f < y * m_iGridSize) {
                if (m_pEntityLists.ContainsKey(index - m_iWidth - 1))
                    entities.Add(m_pEntityLists[index - m_iWidth - 1]);
            }

            if (x > 0 && y < m_iHeight - 1 && pos.x - range * 0.707106f < x * m_iGridSize
                && pos.z + range * 0.707106f > (y + 1) * m_iGridSize) {
                if (m_pEntityLists.ContainsKey(index + m_iWidth - 1))
                    entities.Add(m_pEntityLists[index + m_iWidth - 1]);
            }

            if (x < m_iWidth - 1 && y > 0 && pos.x + range * 0.707106f > (x + 1) * m_iGridSize
                && pos.z - range * 0.707106f < y * m_iGridSize) {
                if(m_pEntityLists.ContainsKey(index - m_iWidth + 1))
                    entities.Add(m_pEntityLists[index - m_iWidth + 1]);
            }

            return entities;
        }

        public bool RemoveEntity(int id)
        {
            if (!m_pIdListMap.ContainsKey(id))
                return false;

            int index = m_pIdListMap[id];
            if (!m_pEntityLists.ContainsKey(index))
                return false;

            if (m_pEntityLists[index].ContainsKey(id)) {
                m_pIdListMap.Remove(id);
                m_pEntityLists[index].Remove(id);
                return true;
            }
            return false;
        }

        public void UpdatePos(int id, ref Vector3 pos)
        {
            if (!m_pIdListMap.ContainsKey(id))
                AddEntity(id, ref pos);

            int x = Mathf.CeilToInt(pos.x / m_iGridSize);//left
            int y = Mathf.CeilToInt(pos.z / m_iGridSize);//bottom
            int newIndex = y * m_iWidth + x;

            int oldIndex = m_pIdListMap[id];

            if (newIndex != oldIndex)
            {
                if (m_pEntityLists[oldIndex].ContainsKey(id))
                {
                    m_pEntityLists[oldIndex].Remove(id);
                }
                if (m_pEntityLists[oldIndex].Count == 0) {
                    m_pEntityListCache.PutObj(m_pEntityLists[oldIndex]);
                    m_pEntityLists.Remove(oldIndex);
                }

                if (!m_pEntityLists.ContainsKey(newIndex))
                    m_pEntityLists.Add(newIndex, GetUseableEntityList());

                if(!m_pEntityLists[newIndex].ContainsKey(id))
                    m_pEntityLists[newIndex].Add(id, pos);

                m_pIdListMap[id] = newIndex;
            }
            else
            {
                m_pEntityLists[oldIndex][id] = pos;
            }
        }

        private RBTree<int, Vector3> GetUseableEntityList() {
            if (m_pEntityListCache.Count > 0)
               return m_pEntityListCache.GetObj();
            else
               return new RBTree<int, Vector3>();
        }
    }
}
