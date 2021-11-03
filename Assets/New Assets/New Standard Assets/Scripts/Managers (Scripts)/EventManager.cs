using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Extensions;

namespace EternityEngine
{
	public class EventManager : SingletonUpdateWhileEnabled<EventManager>
	{
		public static List<Event> events = new List<Event>();
		Event _event;

		public override void DoUpdate ()
		{
			for (int i = 0; i < events.Count; i ++)
			{
				_event = events[i];
				if (Time.timeSinceLevelLoad >= _event.time)
				{
					_event.onEvent (_event.args);
					events.RemoveAt(i);
					i --;
				}
			}
		}

		public struct Event
		{
			public Action<object[]> onEvent;
			public object[] args;
			public float time;

			public Event (Action<object[]> onEvent, object[] args, float time)
			{
				this.onEvent = onEvent;
				this.args = args;
				this.time = time;
			}
		}
	}
}