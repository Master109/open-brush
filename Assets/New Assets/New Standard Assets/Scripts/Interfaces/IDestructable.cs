using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternityEngine
{
	public interface IDestructable
	{
		float Hp { get; set; }
		int MaxHp { get; set; }
		
		void TakeDamage (float amount);
		void Death ();
	}
}