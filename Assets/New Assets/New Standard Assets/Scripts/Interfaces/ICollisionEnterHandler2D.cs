using UnityEngine;

namespace EternityEngine
{
	public interface ICollisionEnterHandler2D
	{
        Collider2D Collider { get; }
        
        void OnCollisionEnter2D (Collision2D coll);
	}
}