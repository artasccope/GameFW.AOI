using CommonTools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameFW.AOI
{
    public class FlatAOIGrid
    {
        private RBTree<int, Vector3> m_pEntities = new RBTree<int, Vector3>();
        public int x;
        public int z;

        public void AddEntity(int id,ref Vector3 pos) {
            m_pEntities.Add(id, pos);
        }

        public RBTree<int, Vector3> GetEntities() {
            return m_pEntities;
        }

        public void RemoveEntity(int id) {
            m_pEntities.Remove(id);
        }

        public void UpdatePos(int id,ref Vector3 pos) {
            m_pEntities[id] = pos;
        }
    }
}
