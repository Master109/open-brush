using System;
using UnityEngine;

namespace EternityEngine
{
	public class ModulationCurveTweaker : SingletonUpdateWhileEnabled<ModulationCurveTweaker>
	{
		public override void DoUpdate ()
		{
			if (LogicModule.instance.useSeparateTweakingDisplayersForModulationCurvesBoolOption.value)
			{
				for (int i = 0; i < ModulationOption.displayingModulationOptionCurves.Count; i ++)
				{
					ModulationOption modulationOption = ModulationOption.displayingModulationOptionCurves[i];
					TweakModulationOption (modulationOption);
				}
			}
			else
			{
				ModulationOption modulationOption = ModulationOption.displayingModulationOptionCurves[0];
				TweakModulationOption (modulationOption);
			}
		}

		void TweakModulationOption (ModulationOption modulationOption)
		{
			
		}
	}
}