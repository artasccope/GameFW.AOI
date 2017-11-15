using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using CommonTools;

namespace GameFW.AOI
{

    public interface Iaoi
    {
        List<RBTree<int, Vector3>> GetInterestEntities(int id,ref Vector3 pos, float range);
        bool AddEntity(int id,ref Vector3 pos);
        bool RemoveEntity(int id);
        void UpdatePos(int id,ref Vector3 pos);
        bool ContainsKey(int id);
    }
}
