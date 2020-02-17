// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Landscape.h"
#include "TerrainGenerator.generated.h"

UCLASS()
class UE_PCG_V4_23_API ATerrainGenerator : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	ATerrainGenerator();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;
	TArray<ALandscape*> GetLandscapes ( );
	bool SetHeightmapData ( ALandscape*, TArray<uint16> );

public:	
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	UFUNCTION ( BlueprintCallable )
	void Gen ( );
};
