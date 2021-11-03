using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class Hazard : Spawnable
	{
		public float damage;
		
		public virtual void OnCollisionEnter2D (Collision2D coll)
		{
			IDestructable destructable = coll.collider.GetComponentInParent<IDestructable>();
			if (destructable != null)
				ApplyDamage (destructable, damage);
		}
		
		public virtual void ApplyDamage (IDestructable destructable, float amount)
		{
			destructable.TakeDamage (amount);
		}
	}
}