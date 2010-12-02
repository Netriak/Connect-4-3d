struct SMapVertexToPixel
{
    float4 Position     : POSITION;    
    float3 Position2D    : TEXCOORD0;
};

struct SMapPixelToFrame
{
    float4 Color : COLOR0;
};

struct SSceneVertexToPixel
{
    float4 Position             : POSITION;
    float4 ShadowMapSamplingPos : TEXCOORD0;    
     float4 RealDistance            : TEXCOORD1;
     float2 TexCoords            : TEXCOORD2;
     float3 Normal                : TEXCOORD3;
     float3 Position3D            : TEXCOORD4;
};

struct SBlurVertexToPixel
{
   float4 vPosition    : POSITION;
   float2 vTexCoord    : TEXCOORD0;
};


struct SScenePixelToFrame
{
    float4 Color : COLOR0;
};

struct SSoftSceneVertexToPixel
{
   float4 vPosition      : POSITION;
   float2 vTexCoord      : TEXCOORD0;
   float4 ShadowMapSamplingPos     : TEXCOORD1;
   float3 vPosition2D	 : TEXCOORD2;
   float3 vNormal        : TEXCOORD3;
   float3 vLightVec      : TEXCOORD4;
   float3 vEyeVec        : TEXCOORD5;
};

struct SSoftSceneVertexToPixelInput
{
   float2 vTexCoord      : TEXCOORD0;
   float4 ShadowMapSamplingPos     : TEXCOORD1;
   float3 vPosition2D	 : TEXCOORD2;
   float2 vScreenCoord   : VPOS;
   float3 vNormal        : TEXCOORD3;
   float3 vLightVec      : TEXCOORD4;
   float3 vEyeVec        : TEXCOORD5;
};


//------- Constants --------

float4x4 xCameraViewProjection;
float4x4 xLightViewProjection;
float4x4 xRotate;
float4x4 xTranslateAndScale;

float4 xLightPos;
float xMaxDepth;
float xCameraDepth;
float3 xCameraPosition;
float4 xLightColor;
float4 xAmbientLight;

float xSampleWeights[16];
float2 xSampleOffsets[16];
int xAnisotropy;
int xUseSpotLight;

float2 xInverseViewPort;

//------- Texture Samplers --------


 Texture xColoredTexture;
 Texture xCameraMap;
 Texture xSpotLight;

sampler CameraDepthMapSampler = sampler_state { texture = <xCameraMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = clamp; AddressV = clamp;};

sampler ColoredTextureSamplerA0 = sampler_state { texture = <xColoredTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
sampler ColoredTextureSamplerA2 = sampler_state { texture = <xColoredTexture> ; magfilter = ANISOTROPIC; minfilter = ANISOTROPIC; mipfilter=ANISOTROPIC; AddressU = mirror; AddressV = mirror; MaxAnisotropy = 2;};
sampler ColoredTextureSamplerA4 = sampler_state { texture = <xColoredTexture> ; magfilter = ANISOTROPIC; minfilter = ANISOTROPIC; mipfilter=ANISOTROPIC; AddressU = mirror; AddressV = mirror; MaxAnisotropy = 4;};

sampler SpotLightSampler = sampler_state { texture = <xSpotLight> ; magfilter = LINEAR; minfilter=LINEAR; mipfilter = LINEAR; AddressU = clamp; AddressV = clamp;};

Texture xShadowMap;

sampler ShadowMapSampler = sampler_state { texture = <xShadowMap> ; magfilter = NONE; minfilter = NONE; mipfilter=NONE; AddressU = clamp; AddressV = clamp;};
//------- Vertex Shaders --------

SMapVertexToPixel ShadowMapVertexShader( float4 inPos : POSITION)
{
    SMapVertexToPixel Output = (SMapVertexToPixel)0;
    float4x4 preWorld = mul(xRotate, xTranslateAndScale);
    float4x4 preLightWorldViewProjection = mul (preWorld, xLightViewProjection);
    
    Output.Position = mul(inPos, preLightWorldViewProjection);
    Output.Position2D = Output.Position.xyz;
    
    return Output;    
}

SMapVertexToPixel ShadowCameraVertexShader( float4 inPos : POSITION)
{
    SMapVertexToPixel Output = (SMapVertexToPixel)0;
    float4x4 preWorld = mul(xRotate, xTranslateAndScale);
    float4x4 preCameraWorldViewProjection = mul (preWorld, xCameraViewProjection);
    
    Output.Position = mul(inPos, preCameraWorldViewProjection);
    Output.Position2D = Output.Position.xyz;
    
    return Output;    
}

SBlurVertexToPixel ShadowBlurVertexShader( float4 inPosition : POSITION, float2 inTexCoord : TEXCOORD0 )
{
   // Output struct
   SBlurVertexToPixel OUT = (SBlurVertexToPixel)0;
   // Output the position
   OUT.vPosition = inPosition;
   // Output the texture coordinates
   OUT.vTexCoord = inTexCoord;
   return OUT;
}


SSceneVertexToPixel ShadowedSceneVertexShader( float4 inPos : POSITION, float2 inTexCoords : TEXCOORD0, float3 inNormal : NORMAL)
{
    SSceneVertexToPixel Output = (SSceneVertexToPixel)0;
    float4x4 preWorld = mul(xRotate, xTranslateAndScale);
    float4x4 preCameraWorldViewProjection = mul (preWorld, xCameraViewProjection);
    float4x4 preLightWorldViewProjection = mul (preWorld, xLightViewProjection);
    
    Output.Position = mul(inPos, preCameraWorldViewProjection);
    Output.ShadowMapSamplingPos = mul(inPos, preLightWorldViewProjection);
    Output.RealDistance = Output.ShadowMapSamplingPos.z/xMaxDepth;
    Output.TexCoords = inTexCoords;
    Output.Normal = mul(inNormal, xRotate);
    Output.Position3D = inPos;
    
    return Output;    
}

// Scene vertex shader
SSoftSceneVertexToPixel SoftShadowedSceneVertexShader( float4 inPosition : POSITION, float3 inNormal : NORMAL,
                         float2 inTexCoord : TEXCOORD0 )
{
   SSoftSceneVertexToPixel OUT = (SSoftSceneVertexToPixel)0;
   
   float4x4 preWorld = mul(xRotate, xTranslateAndScale);
   float4x4 preCameraWorldViewProjection = mul (preWorld, xCameraViewProjection);
   float4x4 preLightWorldViewProjection = mul (preWorld, xLightViewProjection);
   
   // Output the transformed position
   OUT.vPosition = mul( inPosition, preCameraWorldViewProjection );
   OUT.vPosition2D = OUT.vPosition.xyz;
   // Output the texture coordinates
   OUT.vTexCoord = inTexCoord;
   OUT.ShadowMapSamplingPos = mul(inPosition, preLightWorldViewProjection);
   // Get the world space vertex position
   float4 vWorldPos = mul( inPosition, preWorld );
   // Output the world space normal
   OUT.vNormal = mul( inNormal, xRotate );
   // Move the light vector into tangent space
   OUT.vLightVec = xLightPos.xyz - vWorldPos.xyz;
   // Move the eye vector into tangent space
   OUT.vEyeVec = xCameraPosition.xyz - vWorldPos.xyz;
   return OUT;
}


//------- Pixel Shaders --------

float4 SoftShadowedScenePixelShader( SSoftSceneVertexToPixelInput IN) : COLOR0
{
   // Normalize the normal, light and eye vectors
   IN.vNormal   = normalize( IN.vNormal );
   IN.vLightVec = normalize( IN.vLightVec );
   IN.vEyeVec   = normalize( IN.vEyeVec );
   // Sample the color and normal maps
   
   float2 ProjectedTexCoords;
    ProjectedTexCoords[0] = IN.ShadowMapSamplingPos.x/IN.ShadowMapSamplingPos.w/2.0f +0.5f;
    ProjectedTexCoords[1] = -IN.ShadowMapSamplingPos.y/IN.ShadowMapSamplingPos.w/2.0f +0.5f;
    
   float4 vColor;
   if (xAnisotropy == 0)
		vColor = tex2D( ColoredTextureSamplerA0, IN.vTexCoord );
   if (xAnisotropy == 1)
		vColor = tex2D( ColoredTextureSamplerA2, IN.vTexCoord );
   else if (xAnisotropy == 2)
		vColor = tex2D( ColoredTextureSamplerA4, IN.vTexCoord );
   // Compute the ambient, diffuse and specular lighting terms
   float4 ambient = xAmbientLight;
   float diffuse  = max( dot( IN.vNormal, IN.vLightVec ), 0 );
 
   // Grab the shadow term
   
   float2 fCoords = (IN.vScreenCoord +0.5) * xInverseViewPort;
   //return tex2Dproj( ShadowMapSampler, IN.vScreenCoord );
   float LightTextureFactor = 1.0;
   if (xUseSpotLight)
   {
		LightTextureFactor = tex2D(SpotLightSampler, ProjectedTexCoords).r;
   }
   
   float fCameraDepth = tex2D(CameraDepthMapSampler, fCoords).x;
   float fRealDepth = IN.vPosition2D.z / xCameraDepth;
   float fShadowTerm = 0.0;
   if (abs(fRealDepth - fCameraDepth) > 0.005) // MSAA Thingy. Due do anti aliasing pixels can be drawn that weren't drawn before. If so, they should use 
   {												// Shadows near their 3d coordinates, not the 2d ones.
		if (abs(fRealDepth - tex2D(CameraDepthMapSampler, float2(fCoords.x - xInverseViewPort.x, fCoords.y)).x) <= 0.005)
		{
			fCoords.x -= xInverseViewPort.x;
			fShadowTerm = tex2D( ShadowMapSampler, fCoords);
		} else if (abs(fRealDepth - tex2D(CameraDepthMapSampler, float2(fCoords.x + xInverseViewPort.x, fCoords.y)).x) <= 0.005)
		{
			fCoords.x += xInverseViewPort.x;
			fShadowTerm = tex2D( ShadowMapSampler, fCoords);
		} else if (abs(fRealDepth - tex2D(CameraDepthMapSampler, float2(fCoords.x, fCoords.y - xInverseViewPort.y)).x) <= 0.005)
		{
			fCoords.y -= xInverseViewPort.y;
			fShadowTerm = tex2D( ShadowMapSampler, fCoords);
		} else if (abs(fRealDepth - tex2D(CameraDepthMapSampler, float2(fCoords.x, fCoords.y + xInverseViewPort.y)).x) <= 0.005)
		{
			fCoords.y += xInverseViewPort.y;
			fShadowTerm = tex2D( ShadowMapSampler, fCoords);
		}
   } else {
		fShadowTerm = tex2D( ShadowMapSampler, fCoords);
   }
   
   // Compute the final color
   //return float4(IN.vNormal.x, IN.vNormal.y, IN.vNormal.z, 1);
   return (ambient * vColor) +
          (diffuse * vColor * xLightColor * fShadowTerm * LightTextureFactor);
}


// Horizontal blur pixel shader
float4 SBlurPixelShader( SBlurVertexToPixel IN ): COLOR0
{
	// Accumulated color
	float4 vAccum = float4( 0.0f, 0.0f, 0.0f, 0.0f );
	// Sample the taps (xSampleOffsets holds the texel offsets
	// and xSampleWeights holds the texel weights)

	//return tex2D( ShadowMapSampler, IN.vTexCoord);
	float fTotalWeight = 0.0;
	float fBaseDistance = tex2D(CameraDepthMapSampler, IN.vTexCoord).x;
	float fDistance;
	for(int i = 0; i < 15; i++ )
	{
		fDistance = tex2D(CameraDepthMapSampler, IN.vTexCoord + xSampleOffsets[i]).x;
		if (abs(fDistance - fBaseDistance) < 0.001f)
		{
			fTotalWeight += xSampleWeights[i];	
			vAccum += tex2D( ShadowMapSampler, IN.vTexCoord + xSampleOffsets[i] ) * xSampleWeights[i];
		}
	}
	return vAccum * (1.0f / fTotalWeight);
}

 float DotProduct(float4 LightPos, float3 Pos3D, float3 Normal)
 {
     float3 LightDir = normalize(LightPos - Pos3D);
     return dot(LightDir, Normal);
 }
 

SMapPixelToFrame ShadowMapPixelShader(SMapVertexToPixel PSIn)
{
    SMapPixelToFrame Output = (SMapPixelToFrame)0;

    Output.Color = PSIn.Position2D.z/xMaxDepth;

    return Output;
}

float4  ShadowBufferPixelShader( SSceneVertexToPixel PSIn ) : COLOR0
{
	float2 ProjectedTexCoords;
	ProjectedTexCoords[0] = PSIn.ShadowMapSamplingPos.x/PSIn.ShadowMapSamplingPos.w/2.0f +0.5f;
    ProjectedTexCoords[1] = -PSIn.ShadowMapSamplingPos.y/PSIn.ShadowMapSamplingPos.w/2.0f +0.5f;
	if ((saturate(ProjectedTexCoords.x) != ProjectedTexCoords.x) || (saturate(ProjectedTexCoords.y) != ProjectedTexCoords.y))
		return 1.0f; // Outside of shadow area
	// Generate the 9 texture co-ordinates for a 3x3 PCF kernel
	float2 vTexCoords[9];
	// Texel size
	float fTexelSize = 1.0f / 1024.0f;

	// Generate the tecture co-ordinates for the specified depth-map size
	// 4 3 5
	// 1 0 2
	// 7 6 8
	vTexCoords[0] = ProjectedTexCoords;
	vTexCoords[1] = ProjectedTexCoords + float2( -fTexelSize, 0.0f);
	vTexCoords[2] = ProjectedTexCoords + float2(  fTexelSize, 0.0f);
	vTexCoords[3] = ProjectedTexCoords + float2( 0.0f, -fTexelSize);
	vTexCoords[6] = ProjectedTexCoords + float2( 0.0f,  fTexelSize);
	vTexCoords[4] = ProjectedTexCoords + float2( -fTexelSize, -fTexelSize);
	vTexCoords[5] = ProjectedTexCoords + float2(  fTexelSize, -fTexelSize);
	vTexCoords[7] = ProjectedTexCoords + float2( -fTexelSize,  fTexelSize);
	vTexCoords[8] = ProjectedTexCoords + float2(  fTexelSize,  fTexelSize);
	// Sample each of them checking whether the pixel under test is shadowed or not
	float fTotal = 0.0;
	float fShadowTerm = 0.0f;
	
	for( int i = 0; i < 9; i++ )
	{
		float A = tex2D( ShadowMapSampler, vTexCoords[i] ).x; // distance between shadowing object (if any, and light)
		float B = (PSIn.RealDistance.x -0.05f); // Distance between pixel and light
		if (PSIn.Position3D.y < 0.1f)
			B = (PSIn.RealDistance.x -0.01f);
		// Texel is shadowed
		fShadowTerm +=  A < B ? 0.0f : 1.0f;
		fTotal += 1.0f;
	}
	// Get the average
	return fShadowTerm / fTotal;
}




//------- Techniques --------

technique ShadowMap
{
    pass Pass0
    {
        colorwriteenable = red;    
        VertexShader = compile vs_2_0 ShadowMapVertexShader();
        PixelShader = compile ps_2_0 ShadowMapPixelShader();
    }
}

technique ShadowCameraDepthMap
{
    pass Pass0
    {
        colorwriteenable = red;    
        VertexShader = compile vs_2_0 ShadowCameraVertexShader();
        PixelShader = compile ps_2_0 ShadowMapPixelShader();
    }
}

technique ShadowBlur
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 ShadowBlurVertexShader();
        PixelShader = compile ps_3_0 SBlurPixelShader();
    }
}


technique ShadowBuffer
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 ShadowedSceneVertexShader();
        PixelShader = compile ps_2_0 ShadowBufferPixelShader();
    }
}

technique SoftShadowedScene
{
    pass Pass0
    {
        ALPHABLENDENABLE = false;
        VertexShader = compile vs_3_0 SoftShadowedSceneVertexShader();
        PixelShader = compile ps_3_0 SoftShadowedScenePixelShader();
    }
}


technique SoftShadowedSceneBlend
{
    pass Pass0
    {
		SRCBLEND = ONE;
        DESTBLEND = ONE;
        ALPHABLENDENABLE = true;
        VertexShader = compile vs_3_0 SoftShadowedSceneVertexShader();
        PixelShader = compile ps_3_0 SoftShadowedScenePixelShader();
    }
}
