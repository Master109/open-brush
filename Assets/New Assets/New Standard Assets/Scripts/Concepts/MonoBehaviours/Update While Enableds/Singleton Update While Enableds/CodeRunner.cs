using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using CSharpCompiler;
using Object = UnityEngine.Object;

namespace EternityEngine
{ 
	public class CodeRunner : SingletonUpdateWhileEnabled<CodeRunner>
	{
		public const string REPLACE_STRING = "🎩";
		public const string RUN_CODE_ONCE_SCRIPT = @"
using UnityEngine;
using EternityEngine;

public class " + REPLACE_STRING + @" : MonoBehaviour
{
	void Start ()
	{
		" + REPLACE_STRING + @"
		Destroy(this);
	}
}
";
		public const string START_RUNNING_CODE_SCRIPT = @"
using UnityEngine;
using EternityEngine;

public class " + REPLACE_STRING + @" : MonoBehaviour, CodeRunner.IUpdatable
{
	public void DoUpdate ()
	{
		" + REPLACE_STRING + @"
		Destroy(this);
	}
}
";
		IUpdatable[] updatables = new IUpdatable[0];
		static DeferredSynchronizeInvoke synchronizedInvoke;
		static ScriptBundleLoader scriptLoader;
		static Dictionary<string, IUpdatable> runningUpdatablesDict = new Dictionary<string, IUpdatable>();

		void Start ()
		{
			if (synchronizedInvoke != null)
				return;
			synchronizedInvoke = new DeferredSynchronizeInvoke();
			scriptLoader = new ScriptBundleLoader(synchronizedInvoke);
			scriptLoader.logWriter = new UnityLogTextWriter();
			scriptLoader.createInstance = (Type type) => {
				if (type.IsAbstract || type.IsSealed)
					return null;
				else if (typeof(Component).IsAssignableFrom(type))
				{
					Component component = gameObject.AddComponent(type);
					IUpdatable updatable = component as IUpdatable;
					if (updatable != null)
					{
						List<IUpdatable> updatablesList = new List<IUpdatable>(updatables);
						updatablesList.Add(updatable);
						updatables = updatablesList.ToArray();
						runningUpdatablesDict.Add(type.Name, updatable);
					}
					return component;
				}
				else
					return Activator.CreateInstance(type);
			};
			scriptLoader.destroyInstance = (object instance) => {
				Component component = instance as Component;
				if (component != null)
				{
					IUpdatable updatable = component as IUpdatable;
					if (updatable != null)
					{
						List<IUpdatable> updatablesList = new List<IUpdatable>(updatables);
						updatablesList.Remove(updatable);
						updatables = updatablesList.ToArray();
					}
					Destroy(component);
				}
			};
		}

		public override void DoUpdate ()
		{
			synchronizedInvoke.ProcessQueue();
			for (int i = 0; i < updatables.Length; i ++)
			{
				IUpdatable updatable = updatables[i];
				updatable.DoUpdate ();
			}
		}

		public static void RunCodeCommandOnce (string commandName, string commandContents)
		{
			string filePath = Application.persistentDataPath + Path.DirectorySeparatorChar + commandName + ".cs";
			if (!File.Exists(filePath))
			{
				FileStream fileStream = File.Create(filePath);
				fileStream.Close();
			}
			string[] fileSections = RUN_CODE_ONCE_SCRIPT.Split(new string[] { REPLACE_STRING }, StringSplitOptions.None);
			string fileContents = fileSections[0] + commandName + fileSections[1] + commandContents + fileSections[2];
			File.WriteAllText(filePath, fileContents);
			scriptLoader.LoadAndWatchScriptsBundle(new string[] { filePath });
		}

		public static void StartRunningCodeCommand (string commandName, string commandContents)
		{
			if (runningUpdatablesDict.ContainsKey(commandName))
				return;
			string filePath = Application.persistentDataPath + Path.DirectorySeparatorChar + commandName + ".cs";
			if (!File.Exists(filePath))
			{
				FileStream fileStream = File.Create(filePath);
				fileStream.Close();
			}
			string[] fileSections = START_RUNNING_CODE_SCRIPT.Split(new string[] { REPLACE_STRING }, StringSplitOptions.None);
			string fileContents = fileSections[0] + commandName + fileSections[1] + commandContents + fileSections[2];
			File.WriteAllText(filePath, fileContents);
			scriptLoader.LoadAndWatchScriptsBundle(new string[] { filePath });
		}

		public static void StopRunningCodeCommand (string commandName)
		{
			IUpdatable updatable = null;
			if (runningUpdatablesDict.TryGetValue(commandName, out updatable))
			{
				runningUpdatablesDict.Remove(commandName);
				Destroy((Object) updatable);
				File.Delete(Application.persistentDataPath + Path.DirectorySeparatorChar + commandName + ".cs");
			}
		}

		public static void RenameRunningCodeCommand (string currentCommandName, string newCommandName)
		{
			IUpdatable updatable = null;
			if (runningUpdatablesDict.TryGetValue(currentCommandName, out updatable))
			{
				runningUpdatablesDict.Remove(currentCommandName);
				runningUpdatablesDict.Add(newCommandName, updatable);
			}
		}

		public interface IUpdatable
		{
			void DoUpdate ();
		}
	}
}