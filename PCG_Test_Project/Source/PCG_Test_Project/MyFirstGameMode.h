// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameMode.h"
#include "MyFirstGameMode.generated.h"

/**
 * 
 */
UCLASS()
class PCG_TEST_PROJECT_API AMyFirstGameMode : public AGameMode
{
	GENERATED_BODY ( )

	virtual void BeginPlay ( ) override;

public: 
	UFUNCTION ( BlueprintCallable, Category = MyCategory )
	void Gen ( );
};
