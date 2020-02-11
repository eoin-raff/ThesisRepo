// Fill out your copyright notice in the Description page of Project Settings.

#include "Engine/Engine.h"
#include "Engine/World.h" 
#include "MyFirstActor.h"
#include "MyFirstGameMode.h"

void AMyFirstGameMode::BeginPlay ( )
{
	Super::BeginPlay ( );
	GEngine->AddOnScreenDebugMessage ( -1, -1, FColor::Red, TEXT ( "Actor Spawning" ) );

	FTransform SpawnLocation;
//	FActorSpawnParameters ActorSpawnParameters;
	GetWorld ( ) -> SpawnActor <AMyFirstActor> ( AMyFirstActor::StaticClass ( ), SpawnLocation);
}

void AMyFirstGameMode::Gen ( )
{
	//Generate Landscape
	UE_LOG ( LogTemp, Warning, TEXT ( "Generating..." ) );
}