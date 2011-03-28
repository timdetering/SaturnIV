
//= compiler definitions

#define world_matrix          world_matrices[0]
#define world_view_projection world_matrices[1]
#define inverse_transpose      world_matrices[2]

//-

//= vertex and pixel structures
struct VertexToPixel
{
    float4 Position     : POSITION;
    float2 TexCoords    : TEXCOORD0;
    float3 Normal       : TEXCOORD1;
    float3 Position3D   : TEXCOORD2;
    float3 Orig_pos     : TEXCOORD3;
 
    
};

struct PixelToFrame
{  
    float4 Color        : COLOR0;
};
//-

//= standard world matrices and light array
 uniform extern float4x4 world_matrices[3];
//-

//= thrust variables
 uniform extern float ticks;
 uniform extern float4 thrust_color[2];
//-

//= Textures and samplers
Texture noise_texture;

sampler3D Noise = sampler_state
{
  texture = <noise_texture>;
   mipfilter = Linear;
  magfilter = Linear;
  minfilter = Linear;
  AddressU  = mirror; 
  AddressV  = mirror; 
};
//-

//++=============================================================================================================================================
// Vertex Shaders 
//++=============================================================================================================================================

VertexToPixel GP_VertexShader( float4 inPos : POSITION0, float3 inNormal: NORMAL0, float2 inTexCoords : TEXCOORD0, float4 Color : COLOR0)
{

    // this is a standard vertex shader I use in lots of effects, so it passes more stuff than this effect needs
    VertexToPixel Output = (VertexToPixel)0;
 
    Output.Position   = mul(inPos, world_view_projection);
 
    Output.Normal     = normalize(mul(inNormal, (float3x3) inverse_transpose));
    
    Output.Position3D = mul(inPos, world_matrix);
 	Output.Orig_pos = inPos;
    Output.TexCoords  = inTexCoords;
    return Output;
};

//++=============================================================================================================================================
// Pixel Shaders 
//++=============================================================================================================================================

PixelToFrame thrust_PixelShader (VertexToPixel PSIn)
{
      PixelToFrame Output = (PixelToFrame)0;

      float cosang;
      float edgeAlpha;
      float4 tc;
      float4 Up = normalize(world_matrix[1]);
    
      float noiseval;

      float heatfactor = thrust_color[0].a;

       float heat = clamp((1-PSIn.Orig_pos.z/2) ,0,1);

       noiseval = tex3D(Noise,float3(PSIn.TexCoords.x+ticks/25,
                                     PSIn.TexCoords.y-ticks*(5+20*heatfactor), ticks));

       noiseval = pow(noiseval,1.5+1.5*heatfactor);
     
       tc.rgb = ( 2.5*noiseval*thrust_color[1].rgb + 10*noiseval*pow(heat*heatfactor,3.5)*thrust_color[0].rgb); 

       edgeAlpha = 1-abs(dot(Up,PSIn.Normal));
     
       tc.a =  edgeAlpha * pow(heat,.85 + 25*(1-heatfactor));
  
      Output.Color = tc;
 
   return Output;
};


//++=============================================================================================================================================
// Techniques 
//++=============================================================================================================================================


technique thrust_technique
{
    pass Pass0
    {
         VertexShader = compile vs_3_0 GP_VertexShader();
         PixelShader = compile ps_3_0 thrust_PixelShader();
    }
}
