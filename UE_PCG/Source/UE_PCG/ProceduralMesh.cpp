// Fill out your copyright notice in the Description page of Project Settings.


#include "ProceduralMesh.h"
#include "Providers/RuntimeMeshProviderCollision.h"
#include "Providers/RuntimeMeshProviderMemoryCache.h"
#include "Providers/RuntimeMeshProviderStatic.h"
#include "Materials/Material.h"

#define WIDTH 10
#define HEIGHT 10
// Sets default values
AProceduralMesh::AProceduralMesh ( )
{
	PrimaryActorTick.bCanEverTick = true;
}

void AProceduralMesh::BeginPlay ( )
{
	Super::BeginPlay ( );
}

void AProceduralMesh::Tick ( float DeltaTime )
{
	Super::Tick ( DeltaTime );
}

void AProceduralMesh::GenerateMeshes_Implementation ( )
{
	UE_LOG ( LogTemp, Warning, TEXT ( "Width: %i, Height: %i"), WIDTH, HEIGHT );

	FVector2D QuadSize ( 100, 100 );
	FVector BoxRadius ( 1, 1, 0 );


	URuntimeMeshProviderStatic* StaticProvider = NewObject<URuntimeMeshProviderStatic> ( this );
	GetRuntimeMeshComponent ( )->GetOrCreateRuntimeMesh ( )->Initialize ( StaticProvider );
	StaticProvider->SetupMaterialSlot ( 0, FName ( "Cube Base" ), UMaterial::GetDefaultMaterial ( MD_Surface ) );

	// Setup section
	FRuntimeMeshSectionProperties Properties;
	Properties.bCastsShadow = true;
	Properties.bIsVisible = true;
	Properties.MaterialSlot = 0;
	StaticProvider->CreateSection ( 0, 0, Properties );


	// Generate verts
	FVector BoxVerts[4];
	float topLeftX = -BoxRadius.X/2.0f;
	float topLeftY = BoxRadius.Y/2.0f;

	for ( int y = 0; y < 2; y++ )
	{
		for ( int x = 0; x < 2; x++ )
		{
			BoxVerts[y * 2 + x] = FVector ( topLeftX + x, topLeftY - y, 0 );
		}
	}
	
	//UE_LOG ( LogTemp, Error, TEXT ( "No Loop" ) );
	//////x: 0, y:0
	//BoxVerts[0] = FVector ( -BoxRadius.X/2, BoxRadius.Y/2, BoxRadius.Z );
	////x:0, y:1
	//BoxVerts[1] = FVector ( -BoxRadius.X/2, -BoxRadius.Y/2, BoxRadius.Z );
	////x:1, y:0
	//BoxVerts[2] = FVector ( BoxRadius.X/2, BoxRadius.Y/2, BoxRadius.Z );
	////x:1, y:1
	//BoxVerts[3] = FVector ( BoxRadius.X/2, -BoxRadius.Y/2, BoxRadius.Z );
	//for ( int i = 0; i < 4; i++ )
	//{
	//	UE_LOG ( LogTemp, Warning, TEXT ( "%i: %s" ),i, *BoxVerts[i].ToString ( ) );
	//}

	FVector TangentX, TangentY, TangentZ;

	// Setup the mesh data
	FRuntimeMeshRenderableMeshData MeshData ( false, false, 1, false );

	// helper function to add vertex to the mesh data
	auto AddVertex = [&] ( const FVector& InPosition, const FVector& InTangentX, const FVector& InTangentZ, const FVector2D& InTexCoord )
	{
		MeshData.Positions.Add ( InPosition );
		MeshData.Tangents.Add ( InTangentZ, InTangentX );
		MeshData.Colors.Add ( FColor::White );
		MeshData.TexCoords.Add ( InTexCoord );
	};

	// Now add all the sides of the box

	// Pos Z
	TangentZ = FVector ( 0.0f, 0.0f, 1.0f );
	TangentX = FVector ( 0.0f, -1.0f, 0.0f );
	AddVertex ( BoxVerts[0], TangentX, TangentZ, FVector2D ( 0.0f, 0.0f ) );
	AddVertex ( BoxVerts[1], TangentX, TangentZ, FVector2D ( 1.0f, 1.0f ) );
	AddVertex ( BoxVerts[2], TangentX, TangentZ, FVector2D ( 0.0f, 1.0f ) );
	AddVertex ( BoxVerts[3], TangentX, TangentZ, FVector2D ( 1.0f, 1.0f ) );
	MeshData.Triangles.AddTriangle ( 0, 2, 1 );
	MeshData.Triangles.AddTriangle ( 2, 3, 1 );

	//// Neg X
	//TangentZ = FVector ( -1.0f, 0.0f, 0.0f );
	//TangentX = FVector ( 0.0f, -1.0f, 0.0f );
	//AddVertex ( BoxVerts[4], TangentX, TangentZ, FVector2D ( 0.0f, 0.0f ) );
	//AddVertex ( BoxVerts[0], TangentX, TangentZ, FVector2D ( 0.0f, 1.0f ) );
	//AddVertex ( BoxVerts[3], TangentX, TangentZ, FVector2D ( 1.0f, 1.0f ) );
	//AddVertex ( BoxVerts[7], TangentX, TangentZ, FVector2D ( 1.0f, 0.0f ) );
	//MeshData.Triangles.AddTriangle ( 4, 5, 7 );
	//MeshData.Triangles.AddTriangle ( 5, 6, 7 );

	//// Pos Y
	//TangentZ = FVector ( 0.0f, 1.0f, 0.0f );
	//TangentX = FVector ( -1.0f, 0.0f, 0.0f );
	//AddVertex ( BoxVerts[5], TangentX, TangentZ, FVector2D ( 0.0f, 0.0f ) );
	//AddVertex ( BoxVerts[1], TangentX, TangentZ, FVector2D ( 0.0f, 1.0f ) );
	//AddVertex ( BoxVerts[0], TangentX, TangentZ, FVector2D ( 1.0f, 1.0f ) );
	//AddVertex ( BoxVerts[4], TangentX, TangentZ, FVector2D ( 1.0f, 0.0f ) );
	//MeshData.Triangles.AddTriangle ( 8, 9, 11 );
	//MeshData.Triangles.AddTriangle ( 9, 10, 11 );

	//// Pos X
	//TangentZ = FVector ( 1.0f, 0.0f, 0.0f );
	//TangentX = FVector ( 0.0f, 1.0f, 0.0f );
	//AddVertex ( BoxVerts[6], TangentX, TangentZ, FVector2D ( 0.0f, 0.0f ) );
	//AddVertex ( BoxVerts[2], TangentX, TangentZ, FVector2D ( 0.0f, 1.0f ) );
	//AddVertex ( BoxVerts[1], TangentX, TangentZ, FVector2D ( 1.0f, 1.0f ) );
	//AddVertex ( BoxVerts[5], TangentX, TangentZ, FVector2D ( 1.0f, 0.0f ) );
	//MeshData.Triangles.AddTriangle ( 12, 13, 15 );
	//MeshData.Triangles.AddTriangle ( 13, 14, 15 );

	//// Neg Y
	//TangentZ = FVector ( 0.0f, -1.0f, 0.0f );
	//TangentX = FVector ( 1.0f, 0.0f, 0.0f );
	//AddVertex ( BoxVerts[7], TangentX, TangentZ, FVector2D ( 0.0f, 0.0f ) );
	//AddVertex ( BoxVerts[3], TangentX, TangentZ, FVector2D ( 0.0f, 1.0f ) );
	//AddVertex ( BoxVerts[2], TangentX, TangentZ, FVector2D ( 1.0f, 1.0f ) );
	//AddVertex ( BoxVerts[6], TangentX, TangentZ, FVector2D ( 1.0f, 0.0f ) );
	//MeshData.Triangles.AddTriangle ( 16, 17, 19 );
	//MeshData.Triangles.AddTriangle ( 17, 18, 19 );

	//// Neg Z
	//TangentZ = FVector ( 0.0f, 0.0f, -1.0f );
	//TangentX = FVector ( 0.0f, 1.0f, 0.0f );
	//AddVertex ( BoxVerts[7], TangentX, TangentZ, FVector2D ( 0.0f, 0.0f ) );
	//AddVertex ( BoxVerts[6], TangentX, TangentZ, FVector2D ( 0.0f, 1.0f ) );
	//AddVertex ( BoxVerts[5], TangentX, TangentZ, FVector2D ( 1.0f, 1.0f ) );
	//AddVertex ( BoxVerts[4], TangentX, TangentZ, FVector2D ( 1.0f, 0.0f ) );
	//MeshData.Triangles.AddTriangle ( 20, 21, 23 );
	//MeshData.Triangles.AddTriangle ( 21, 22, 23 );


	// Add the mesh
	StaticProvider->UpdateSection ( 0, 0, MeshData );



	//// Now setup collision
	//FRuntimeMeshCollisionSettings Settings;
	//Settings.bUseAsyncCooking = true;
	//Settings.bUseComplexAsSimple = false;

	//// Add a single box element for the simple collision
	//Settings.Boxes.Emplace ( BoxRadius.X * 2, BoxRadius.Y * 2, BoxRadius.Z * 2 );


	//// Setup Collision 
	//FRuntimeMeshCollisionData CollisionData;
	//FRuntimeMeshCollisionVertexStream& CollisionVertices = CollisionData.Vertices;
	//FRuntimeMeshCollisionTriangleStream& CollisionTriangles = CollisionData.Triangles;

	//// Generate verts
	//CollisionVertices.Add ( FVector ( -BoxRadius.X, BoxRadius.Y, BoxRadius.Z ) );
	//CollisionVertices.Add ( FVector ( BoxRadius.X, BoxRadius.Y, BoxRadius.Z ) );
	//CollisionVertices.Add ( FVector ( BoxRadius.X, -BoxRadius.Y, BoxRadius.Z ) );
	//CollisionVertices.Add ( FVector ( -BoxRadius.X, -BoxRadius.Y, BoxRadius.Z ) );

	//CollisionVertices.Add ( FVector ( -BoxRadius.X, BoxRadius.Y, -BoxRadius.Z ) );
	//CollisionVertices.Add ( FVector ( BoxRadius.X, BoxRadius.Y, -BoxRadius.Z ) );
	//CollisionVertices.Add ( FVector ( BoxRadius.X, -BoxRadius.Y, -BoxRadius.Z ) );
	//CollisionVertices.Add ( FVector ( -BoxRadius.X, -BoxRadius.Y, -BoxRadius.Z ) );

	//// Pos Z
	//CollisionTriangles.Add ( 0, 1, 3 );
	//CollisionTriangles.Add ( 1, 2, 3 );
	//// Neg X
	//CollisionTriangles.Add ( 4, 0, 7 );
	//CollisionTriangles.Add ( 0, 3, 7 );
	//// Pos Y
	//CollisionTriangles.Add ( 5, 1, 4 );
	//CollisionTriangles.Add ( 1, 0, 4 );
	//// Pos X
	//CollisionTriangles.Add ( 6, 2, 5 );
	//CollisionTriangles.Add ( 2, 1, 5 );
	//// Neg Y
	//CollisionTriangles.Add ( 7, 3, 6 );
	//CollisionTriangles.Add ( 3, 2, 6 );
	//// Neg Z
	//CollisionTriangles.Add ( 7, 6, 4 );
	//CollisionTriangles.Add ( 6, 5, 4 );


	//// add the collision mesh for complex collision
	//StaticProvider->SetCollisionMesh ( CollisionData );

}
