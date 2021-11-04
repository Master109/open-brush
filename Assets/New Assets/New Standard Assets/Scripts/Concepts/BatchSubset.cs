using TiltBrush;
using UnityEngine;

namespace EternityEngine
{
    public class BatchSubset
    {
        public Stroke m_Stroke;
        public Batch m_ParentBatch;
        public Bounds m_Bounds;
        public int m_StartVertIndex;
        public int m_VertLength;
        public int m_iTriIndex;
        public int m_nTriIndex;
        public bool m_Active;
        public ushort[] m_TriangleBackup;
        public ArtCanvas Canvas
        {
            get
            {
                return m_ParentBatch.ParentPool.Owner.Canvas;
            }
        }
    }
}
