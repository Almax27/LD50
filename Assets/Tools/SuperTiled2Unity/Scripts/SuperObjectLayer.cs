using UnityEngine;

namespace SuperTiled2Unity
{
    public class SuperObjectLayer : SuperLayer
    {
        [ReadOnly]
        public Color m_Color;

        [ReadOnly]
        public int m_SortOrder;
    }
}
