// Fill out your copyright notice in the Description page of Project Settings.


#include "TerrainGenerator.h"
#include "Landscape.h"
#include "LandscapeEdit.h"
//#include "LandscapeEditor/Public/LandscapeEditorUtils.h"
#include "LandscapeInfo.h"

// Sets default values
ATerrainGenerator::ATerrainGenerator()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void ATerrainGenerator::BeginPlay()
{
	Super::BeginPlay();
	
}

// Called every frame
void ATerrainGenerator::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);


}

void ATerrainGenerator::Gen ( )
{
	UE_LOG ( LogTemp, Warning, TEXT ( "Generating Terrain" ) );

	// a) REQUIRED STEP: Call static function
	//ULandscapeInfo::RecreateLandscapeInfo ( GetWorld ( ), 1 );

	TArray<ALandscape*> landscapes = GetLandscapes ( );
	for ( ALandscape* landscape : landscapes )
	{
		FIntRect rect = landscape->GetBoundingRect ( );
		// LandscapeEditorUtils::SetHeightmapData adds one to each dimension
		// because the boundary edges may be used.
		int32 cols = rect.Width ( ) + 1, rows = rect.Height ( ) + 1;
		int32 numHeights = cols * rows;

		TArray<uint16> Data;
		Data.Init ( 0, numHeights );

		// #octaves to sum, period in x, period in y.
		// The larger the period, the more variation we get
		// in the lowest frequency.
		int32 octaves = 16, px = 4, py = 4;
		float amplitude = 1000.f;
		for ( int i = 0; i < Data.Num ( ); i++ )
		{
			float nx = ( i % cols ) / ( float ) cols; //normalized col
			float ny = ( i / cols ) / ( float ) rows; //normalized row
//			Data[i] = PerlinNoise2D ( nx, ny, amplitude, octaves, px, py );
			Data[i] = FMath::PerlinNoise2D ( FVector2D ( nx, ny ) );
		}
		
		UE_LOG ( LogTemp, Warning, TEXT ( "SET HEIGHTMAP DATA HERE" ) );
		SetHeightmapData ( landscape, Data );
		//ULandscapeInfo::RecreateLandscapeInfo ( GetWorld ( ), 1 );
	}
	
}

TArray<ALandscape*> ATerrainGenerator::GetLandscapes ( )
{
	TArray<ALandscape*> landscapes;
	ULevel *level = GetLevel ( );
	for ( int i = 0; i < level->Actors.Num ( ); i++ )
		if ( ALandscape* land = Cast<ALandscape> ( level->Actors[i] )
			)
			landscapes.Push ( land );
	return landscapes;
}

bool ATerrainGenerator::SetHeightmapData ( ALandscape* Landscape, TArray<uint16> Data)
{
	FIntRect ComponentsRect = Landscape->GetBoundingRect ( ) + Landscape->LandscapeSectionOffset;

	if ( Data.Num ( ) == ( 1 + ComponentsRect.Width ( ) )*( 1 + ComponentsRect.Height ( ) ) )
	{
		FHeightmapAccessor<false> HeightmapAccessor ( Landscape->GetLandscapeInfo ( ) );
		HeightmapAccessor.SetData ( ComponentsRect.Min.X, ComponentsRect.Min.Y, ComponentsRect.Max.X, ComponentsRect.Max.Y, Data.GetData ( ) );
		return true;
	}

	return false;
}

