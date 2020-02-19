// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RuntimeMeshActor.h"
#include "ProceduralMesh.generated.h"

/**
 * 
 */
UCLASS()
class UE_PCG_API AProceduralMesh : public ARuntimeMeshActor
{
	GENERATED_BODY()

public:
	// Sets default values for this actor's properties
	AProceduralMesh ( );

	// Called every frame
	virtual void Tick ( float DeltaTime ) override;

	virtual void GenerateMeshes_Implementation ( ) override;

	UPROPERTY ( EditAnywhere )
	int32 Width = 10;	
	UPROPERTY ( EditAnywhere )
	int32 Height = 10;

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay ( ) override;

private:
	const int32 numberOfVertices = Width * Height;


	
};
