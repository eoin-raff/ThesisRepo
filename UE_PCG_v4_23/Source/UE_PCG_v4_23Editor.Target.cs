// Fill out your copyright notice in the Description page of Project Settings.

using UnrealBuildTool;
using System.Collections.Generic;

public class UE_PCG_v4_23EditorTarget : TargetRules
{
	public UE_PCG_v4_23EditorTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Editor;

		ExtraModuleNames.AddRange( new string[] { "UE_PCG_v4_23" } );
	}
}
