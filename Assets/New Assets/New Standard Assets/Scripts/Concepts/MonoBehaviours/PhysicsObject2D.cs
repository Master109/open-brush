using UnityEngine;
using System.Collections.Generic;
using Extensions;

public class PhysicsObject2D : MonoBehaviour
{
	public string layerName;
	public Collider2D collider;
	[HideInInspector]
	public string[] collidingLayers = new string[0];
	public static Dictionary<string, List<PhysicsObject2D>> physicsObjectsLayersDict = new Dictionary<string, List<PhysicsObject2D>>();

	public virtual void Awake ()
	{
		if (physicsObjectsLayersDict.ContainsKey(layerName))
			physicsObjectsLayersDict[layerName].Add(this);
		else
		{
			List<PhysicsObject2D> physicsObjects = new List<PhysicsObject2D>();
			physicsObjects.Add(this);
			physicsObjectsLayersDict.Add(layerName, physicsObjects);
		}
	}

	public virtual void Start ()
	{
		collidingLayers = PhysicsManager2D.layerCollisionsDict[layerName];
		if (collider == null)
			return;
		for (int i = 0; i < physicsObjectsLayersDict.Count; i ++)
		{
			List<PhysicsObject2D> physicsObjects = physicsObjectsLayersDict[physicsObjectsLayersDict.Keys.Get(i)];
			for (int i2 = 0; i2 < physicsObjects.Count; i2 ++)
			{
				PhysicsObject2D physicsObject = physicsObjects[i2];
				if (physicsObject.collider != null)
					Physics2D.IgnoreCollision(collider, physicsObject.collider, !collidingLayers.Contains(physicsObject.layerName));
			}
		}
	}

	public virtual void OnDestroy ()
	{
		physicsObjectsLayersDict[layerName].Remove(this);
	}
}