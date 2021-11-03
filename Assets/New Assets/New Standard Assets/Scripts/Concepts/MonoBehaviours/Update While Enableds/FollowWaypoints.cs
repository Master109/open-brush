using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Extensions;

namespace EternityEngine
{
	[ExecuteInEditMode]
	public class FollowWaypoints : UpdateWhileEnabled
	{
#if UNITY_EDITOR
		public bool autoSetWaypoints = true;
		public bool autoSetPivotOffset = true;
		public bool autoSetLineRenderers = true;
		public Transform waypointsParent;
#endif
		public Transform trs;
		public Rigidbody rigid;
		public FollowType followType;
		public List<Waypoint> waypoints = new List<Waypoint>();
		public float moveSpeed;
		public float rotateSpeed;
		public int currentWaypointIndex;
		public bool isBacktracking;
		public WaypointPath path;
		public Vector3 pivotOffset;
		Transform currentWaypointTrs;
		// List<Rigidbody> collidingRigidbodies = new List<Rigidbody>();
		
		public override void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Bounds bounds = GetBoundsOfChildren();
				if (bounds == BoundsExtensions.INFINITE)
					return;
				if (autoSetPivotOffset)
				{
					Vector3 childrenBoundsCenter = bounds.center;
					pivotOffset = childrenBoundsCenter - trs.position;
					waypoints[currentWaypointIndex].trs.position = childrenBoundsCenter;
				}
				if (autoSetLineRenderers)
					AutoSetLineRenderers ();
				return;
			}
#endif
			for (int i = 0; i < waypoints.Count; i ++)
			{
				Waypoint waypoint = waypoints[i];
				waypoint.trs.SetParent(null);
			}
			currentWaypointTrs = waypoints[currentWaypointIndex].trs;
			base.OnEnable ();
		}

#if UNITY_EDITOR
		void OnValidate ()
		{
			if (trs == null)
				trs = GetComponent<Transform>();
			if (rigid == null)
				rigid = GetComponent<Rigidbody>();
			if (rigid != null)
			{
				rigid.mass = 0;
				MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
				for (int i = 0; i < meshRenderers.Length; i ++)
				{
					MeshRenderer meshRenderer = meshRenderers[i];
					rigid.mass += meshRenderer.bounds.GetVolume();
				}
			}
			if (autoSetWaypoints)
			{
				if (waypointsParent != null)
				{
					Waypoint[] _waypoints = new Waypoint[waypointsParent.childCount];
					for (int i = 0; i < waypointsParent.childCount; i ++)
					{
						Transform child = waypointsParent.GetChild(i);
						_waypoints[i] = new Waypoint(child, Vector3.zero);
					}
					waypoints = new List<Waypoint>(_waypoints);
				}
				else
					waypointsParent = trs;
			}
		}

		void AutoSetLineRenderers ()
		{
			LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>();
			for (int i = 0; i < lineRenderers.Length; i ++)
			{
				LineRenderer lineRenderer = lineRenderers[i];
				LineRendererUtilities.RemoveLineRendererAndGameObjectIfEmpty (lineRenderer, true);
			}
			int previousCurrentWaypointIndex = currentWaypointIndex;
			bool previousIsBacktracking = isBacktracking;
			Waypoint nextWaypoint = waypoints[currentWaypointIndex];
			isBacktracking = !isBacktracking;
			OnReachedWaypoint ();
			int previousWaypointIndex = currentWaypointIndex;
			Waypoint previousWaypoint = waypoints[previousWaypointIndex];
			currentWaypointIndex = previousCurrentWaypointIndex;
			isBacktracking = previousIsBacktracking;
			int passedPreviousCurrentWaypointCount = 0;
			while (true)
			{
				Waypoint waypoint = nextWaypoint;
				if (currentWaypointIndex == previousCurrentWaypointIndex)
					passedPreviousCurrentWaypointCount ++;
				OnReachedWaypoint ();
				nextWaypoint = waypoints[currentWaypointIndex];
				if (followType == FollowType.Loop)
				{
					if (passedPreviousCurrentWaypointCount == 2)
						break;
				}
				else if (followType == FollowType.PingPong)
				{
					if ((passedPreviousCurrentWaypointCount == 2 && (previousCurrentWaypointIndex == 0 || previousCurrentWaypointIndex == waypoints.Count - 1)) || passedPreviousCurrentWaypointCount == 3)
						break;
				}
				else// if (followType == FollowType.Once)
				{
					if (previousWaypointIndex == currentWaypointIndex)
						break;
				}
				MakeLineRenderersForWaypoint (waypoint, previousWaypoint, nextWaypoint);
				previousWaypoint = waypoint;
				previousWaypointIndex = currentWaypointIndex;
			}
			currentWaypointIndex = previousCurrentWaypointIndex;
			isBacktracking = previousIsBacktracking;
		}
#endif
		
		Bounds GetBoundsOfChildren ()
		{
			Collider[] colliders = GetComponentsInChildren<Collider>();
			if (colliders.Length == 0)
				return BoundsExtensions.INFINITE;
			Bounds[] childBoundsArray = new Bounds[colliders.Length];
			for (int i = 0; i < colliders.Length; i ++)
			{
				Collider collider = colliders[i];
				childBoundsArray[i] = collider.GetComponentInChildren<Renderer>().bounds;
			}
			return childBoundsArray.Combine();
		}

		void SetLineRenderersToBoundsSidesAndRotate (Bounds bounds, LineRenderer[] lineRenderers, Vector3 pivotPoint, Quaternion rotation)
		{
			LineSegment3D[] sides = bounds.GetSides();
			for (int i = 0; i < 12; i ++)
			{
				LineSegment3D side = sides[i];
				lineRenderers[i].SetPositions(new Vector3[2] { side.start.Rotate(pivotPoint, rotation), side.end.Rotate(pivotPoint, rotation) });
			}
		}

		LineRenderer[] MakeLineRenderersForWaypoint (Waypoint waypoint, Waypoint previousWaypoint, Waypoint nextWaypoint)
		{
			List<LineRenderer> output = new List<LineRenderer>();
			if (waypoint.trs.eulerAngles == nextWaypoint.trs.eulerAngles)
			{
				LineRenderer[] lineRenderers = new LineRenderer[12];
				for (int i = 0; i < 12; i ++)
					lineRenderers[i] = LineRendererUtilities.AddLineRendererToGameObjectOrMakeNew(gameObject, path);
				HashSet<LineRenderer> extraLineRenderers = new HashSet<LineRenderer>();
				if (!previousWaypoint.Equals(default(Waypoint)) && !nextWaypoint.Equals(previousWaypoint))
				{
					if (previousWaypoint.trs.position.x > waypoint.trs.position.x)
					{
						extraLineRenderers.Add(lineRenderers[1]);
						extraLineRenderers.Add(lineRenderers[6]);
						extraLineRenderers.Add(lineRenderers[10]);
						extraLineRenderers.Add(lineRenderers[11]);
					}
					else if (previousWaypoint.trs.position.x < waypoint.trs.position.x)
					{
						extraLineRenderers.Add(lineRenderers[1]);
						extraLineRenderers.Add(lineRenderers[4]);
						extraLineRenderers.Add(lineRenderers[7]);
						extraLineRenderers.Add(lineRenderers[8]);
					}
					if (previousWaypoint.trs.position.y > waypoint.trs.position.y)
					{
						extraLineRenderers.Add(lineRenderers[1]);
						extraLineRenderers.Add(lineRenderers[5]);
						extraLineRenderers.Add(lineRenderers[8]);
						extraLineRenderers.Add(lineRenderers[9]);
					}
					else if (previousWaypoint.trs.position.y < waypoint.trs.position.y)
					{
						extraLineRenderers.Add(lineRenderers[0]);
						extraLineRenderers.Add(lineRenderers[2]);
						extraLineRenderers.Add(lineRenderers[3]);
						extraLineRenderers.Add(lineRenderers[10]);
					}
					if (previousWaypoint.trs.position.z > waypoint.trs.position.z)
					{
						extraLineRenderers.Add(lineRenderers[3]);
						extraLineRenderers.Add(lineRenderers[4]);
						extraLineRenderers.Add(lineRenderers[5]);
						extraLineRenderers.Add(lineRenderers[6]);
					}
					else if (previousWaypoint.trs.position.z < waypoint.trs.position.z)
					{
						extraLineRenderers.Add(lineRenderers[0]);
						extraLineRenderers.Add(lineRenderers[7]);
						extraLineRenderers.Add(lineRenderers[9]);
						extraLineRenderers.Add(lineRenderers[11]);
					}
				}
				Bounds bounds = new Bounds(waypoint.trs.position, GetBoundsOfChildren().size);
				SetLineRenderersToBoundsSidesAndRotate (bounds, lineRenderers, waypoint.trs.position + waypoint.pivotOffset, waypoint.trs.rotation);
				if (!nextWaypoint.Equals(waypoint))
				{
					Bounds bounds2 = new Bounds(nextWaypoint.trs.position, GetBoundsOfChildren().size);
					HashSet<LineRenderer> changedLineRenderers = new HashSet<LineRenderer>();
					Vector3[] corners = bounds.GetCorners();
					Vector3 corner0 = corners[0];
					Vector3 corner1 = corners[1];
					Vector3 corner2 = corners[2];
					Vector3 corner3 = corners[3];
					Vector3 corner4 = corners[4];
					Vector3 corner5 = corners[5];
					Vector3 corner6 = corners[6];
					Vector3 corner7 = corners[7];
					Vector3[] corners2 = bounds2.GetCorners();
					Vector3 corner2_0 = corners2[0];
					Vector3 corner2_1 = corners2[1];
					Vector3 corner2_2 = corners2[2];
					Vector3 corner2_3 = corners2[3];
					Vector3 corner2_4 = corners2[4];
					Vector3 corner2_5 = corners2[5];
					Vector3 corner2_6 = corners2[6];
					Vector3 corner2_7 = corners2[7];
					if (nextWaypoint.trs.position.x > waypoint.trs.position.x)
					{
						ChangeLineRenderer (lineRenderers[1], corner1, corner2_1, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[6], corner2, corner2_2, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[10], corner3, corner2_3, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[11], corner7, corner2_7, ref changedLineRenderers);
					}
					else if (nextWaypoint.trs.position.x < waypoint.trs.position.x)
					{
						ChangeLineRenderer (lineRenderers[1], corner0, corner2_0, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[4], corner4, corner2_4, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[7], corner5, corner2_5, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[8], corner6, corner2_6, ref changedLineRenderers);
					}
					if (nextWaypoint.trs.position.y > waypoint.trs.position.y)
					{
						ChangeLineRenderer (lineRenderers[1], corner2, corner2_2, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[5], corner3, corner2_3, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[8], corner4, corner2_4, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[9], corner5, corner2_5, ref changedLineRenderers);
					}
					else if (nextWaypoint.trs.position.y < waypoint.trs.position.y)
					{
						ChangeLineRenderer (lineRenderers[0], corner0, corner2_0, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[2], corner1, corner2_1, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[3], corner6, corner2_6, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[10], corner7, corner2_7, ref changedLineRenderers);
					}
					if (nextWaypoint.trs.position.z > waypoint.trs.position.z)
					{
						ChangeLineRenderer (lineRenderers[3], corner3, corner2_3, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[4], corner5, corner2_5, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[5], corner6, corner2_6, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[6], corner7, corner2_7, ref changedLineRenderers);
					}
					else if (nextWaypoint.trs.position.z < waypoint.trs.position.z)
					{
						ChangeLineRenderer (lineRenderers[0], corner0, corner2_0, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[7], corner1, corner2_1, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[9], corner2, corner2_2, ref changedLineRenderers);
						ChangeLineRenderer (lineRenderers[11], corner4, corner2_4, ref changedLineRenderers);
					}
					foreach (LineRenderer changedLineRenderer in changedLineRenderers)
						extraLineRenderers.Remove(changedLineRenderer);
				}
				output.AddRange(lineRenderers);
				// foreach (LineRenderer extraLineRenderer in extraLineRenderers)
				// {
				// 	output.Remove(extraLineRenderer);
				// 	LineRendererUtilities.RemoveLineRendererAndGameObjectIfEmpty (extraLineRenderer);
				// }
			}
			else
			{
			}
			return output.ToArray();
		}

		void ChangeLineRenderer (LineRenderer lineRenderer, Vector3 point, Vector3 otherPoint, ref HashSet<LineRenderer> changedLineRenderers)
		{
			lineRenderer.SetPositions(new Vector3[2] { point, otherPoint });
			changedLineRenderers.Add(lineRenderer);
		}

		public override void DoUpdate ()
		{
			if (GameManager.paused || _SceneManager.isLoading)
				return;
			if (moveSpeed != 0)
			{
				Vector3 newPosition = Vector3.Lerp(trs.position, currentWaypointTrs.position - pivotOffset, moveSpeed * Time.deltaTime * (1f / Vector3.Distance(trs.position, currentWaypointTrs.position - pivotOffset)));
				if (!float.IsNaN(newPosition.x))
					trs.position = newPosition;
			}
			if (rotateSpeed != 0)
				trs.rotation = Quaternion.Slerp(trs.rotation, currentWaypointTrs.rotation, rotateSpeed * Time.deltaTime * (1f / Quaternion.Angle(trs.rotation, currentWaypointTrs.rotation)));
			if ((trs.position == currentWaypointTrs.position - pivotOffset || moveSpeed == 0) && (trs.eulerAngles == currentWaypointTrs.eulerAngles || rotateSpeed == 0))
				OnReachedWaypoint ();
		}
		
		void OnReachedWaypoint ()
		{
			if (isBacktracking)
				currentWaypointIndex --;
			else
				currentWaypointIndex ++;
			switch (followType)
			{
				case FollowType.Once:
					if (currentWaypointIndex == waypoints.Count)
						currentWaypointIndex = waypoints.Count - 1;
					else if (currentWaypointIndex == -1)
						currentWaypointIndex = 0;
					break;
				case FollowType.Loop:
					if (currentWaypointIndex == waypoints.Count)
						currentWaypointIndex = 0;
					else if (currentWaypointIndex == -1)
						currentWaypointIndex = waypoints.Count - 1;
					break;
				case FollowType.PingPong:
					if (currentWaypointIndex == waypoints.Count)
					{
						currentWaypointIndex -= 2;
						isBacktracking = !isBacktracking;
					}
					else if (currentWaypointIndex == -1)
					{
						currentWaypointIndex += 2;
						isBacktracking = !isBacktracking;
					}
					break;
			}
			currentWaypointTrs = waypoints[currentWaypointIndex].trs;
		}

		// void OnCollisionEnter (Collision coll)
		// {
		// 	if (rigid == null)
		// 		return;
		// 	Rigidbody collidingRigid = coll.rigidbody;
		// 	if (collidingRigid != null)
		// 	{
		// 		collidingRigidbodies.Add(collidingRigid);
		// 		OnCollisionStay (coll);
		// 	}
		// }

		// void OnCollisionStay (Collision coll)
		// {
		// 	if (rigid == null)
		// 		return;
		// 	for (int i = 0; i < collidingRigidbodies.Count; i ++)
		// 	{
		// 		Rigidbody collidingRigid = collidingRigidbodies[i];
		// 		RaycastHit hit;
		// 		if (collidingRigid.SweepTest(currentWaypointTrs.position - pivotOffset - trs.position, out hit, moveSpeed * Time.deltaTime))
		// 		{
		// 			enabled = false;
		// 			return;
		// 		}
		// 	}
		// 	enabled = true;
		// }

		// void OnCollisionExit (Collision coll)
		// {
		// 	if (rigid == null)
		// 		return;
		// 	Rigidbody collidingRigid = coll.rigidbody;
		// 	if (collidingRigid != null)
		// 	{
		// 		collidingRigidbodies.Remove(collidingRigid);
		// 		OnCollisionStay (coll);
		// 	}
		// }

		[Serializable]
		public struct Waypoint
		{
			public Transform trs;
			public Vector3 pivotOffset;

			public Waypoint (Transform trs, Vector3 pivotOffset)
			{
				this.trs = trs;
				this.pivotOffset = pivotOffset;
			}
		}

		[Serializable]
		public struct WaypointPath
		{
			public float width;
			public Color color;
			public Material material;
			public string sortingLayerName;
			[Range(-32768, 32767)]
			public int sortingOrder;
		}

		public enum FollowType
		{
			Once,
			Loop,
			PingPong
		}
	}
}