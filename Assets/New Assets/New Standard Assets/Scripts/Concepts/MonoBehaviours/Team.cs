using System;
using UnityEngine;

namespace EternityEngine
{
	[Serializable]
	public class Team<T>
	{
		public T representative;
		public T snake;
		public Color color;
		public Material material;
		public Team<T> opponent;
		public Team<T>[] opponents = new Team<T>[0];
	}
}