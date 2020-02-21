Shader "Custom/Terrain"
{
	Properties
	{
		TestTexture ( "Texture", 2D ) = "white"{}
		TestScale ( "Scale2", Float ) = 1
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		const static int maxLayerCount = 8;
	    const static float epsilon = 1E-4;

		int layerCount;
		float3 baseColors[maxLayerCount];
		float baseStartHeights[maxLayerCount];
		float baseBlends[maxLayerCount];
		float baseColorStrengths[maxLayerCount];
		float baseTextureScales[maxLayerCount];

		float minHeight;
		float maxHeight;

		sampler2D TestTexture;
		float TestScale;
		UNITY_DECLARE_TEX2DARRAY ( baseTextures );
        struct Input
        {
			float3 worldPos;
			float3 worldNormal;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		
		float inverseLerp ( float a, float b, float value )
		{
			return saturate(( value - a ) / ( b - a ));
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float heightPercent = inverseLerp ( minHeight, maxHeight, IN.worldPos.y );
			for ( int i = 0; i < layerCount; i++ )
			{
				float drawStrength = inverseLerp ( 
					-baseBlends[i] / 2 - epsilon, 
					baseBlends[i] / 2, 
					heightPercent - baseStartHeights[i] );
				o.Albedo = o.Albedo * ( 1 - drawStrength ) + baseColors[i] * drawStrength;
				
			}
			//tri planar mapping
			float3 scaledWorldPos = IN.worldPos / TestScale;
			float3 blendAxes = abs ( IN.worldNormal );
			blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

			float3 xProjecection = tex2D ( TestTexture, scaledWorldPos.yz / TestScale ) * blendAxes.x;
			float3 yProjecection = tex2D ( TestTexture, scaledWorldPos.xz / TestScale ) * blendAxes.y;
			float3 zProjecection = tex2D ( TestTexture, scaledWorldPos.xy / TestScale ) * blendAxes.z;

			//o.Albedo = xProjecection + yProjecection + zProjecection;

        }
        ENDCG
    }
    FallBack "Diffuse"
}
