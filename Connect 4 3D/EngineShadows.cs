using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using System.Drawing;

namespace Connect_4_3D
{
    class ShadowLight
    {
        internal static List<ShadowLight> LightList = new List<ShadowLight>();
        Vector3 Position;
        internal Vector4 LColor;
        float FOV;
        float NearDistance;
        float FarDistance;
        internal static Vector4 AmbientLight;
        internal bool IsSpotLight;
        internal ShadowLight TransformTarget = null;
        internal float TransformTime = 0.4f; // Time in seconds needed to transform.
        internal int LastTickCount = 0;
        internal bool TransForm = false;
        internal float fTransform = -1f; // How far we are tranformed.
        Vector3 TargetPoint = new Vector3(0, 0, 0);
        internal Vector3 _TargetPoint = new Vector3(0, 0, 0);

        internal float _FOV {get; private set;}
        internal float _NearDistance { get; private set; }
        internal float _FarDistance {get; private set;}
        internal Vector4 _LColor;
        internal Vector3 _Position { get; private set; }

        void Transform()
        {
            int TickCount = Environment.TickCount;
            float fDifference = (float)(TickCount - LastTickCount) / 1000 / TransformTime;
            LastTickCount = TickCount;
            float fNew, fOld;

            if (TransformTarget == null) TransForm = false;

            if (!TransForm) // Revert
            {
                fNew = fDifference / fTransform;

                if (fTransform > 0f)
                    fTransform -= fDifference;

                if (fTransform <= 0f) // Fully returned
                {
                    Position = _Position;
                    LColor = _LColor;
                    FOV = _FOV;
                    NearDistance = _NearDistance;
                    FarDistance = _FarDistance;
                    TargetPoint = _TargetPoint;
                    return;
                }

                fOld = 1.0f - fNew;

                Position = Position * fOld + _Position * fNew;
                LColor = LColor * fOld + _LColor * fNew;
                FOV = FOV * fOld + _FOV * fNew;
                NearDistance = NearDistance * fOld + _NearDistance * fNew;
                FarDistance = FarDistance * fOld + _FarDistance * fNew;
                TargetPoint = TargetPoint * fOld + _TargetPoint * fNew;
            }
            else // Transform
            {
                fNew = fDifference / (1.0f - fTransform);
                
                if (fTransform < 1f)
                    fTransform += fDifference;

                if (fTransform >= 1f) // Fully returned
                {
                    Position = TransformTarget._Position;
                    LColor = TransformTarget._LColor;
                    FOV = TransformTarget._FOV;
                    NearDistance = TransformTarget._NearDistance;
                    FarDistance = TransformTarget._FarDistance;
                    TargetPoint = TransformTarget._TargetPoint;
                    return;
                }

                fOld = 1.0f - fNew;

                Position = Position * fOld + TransformTarget._Position * fNew;
                LColor = LColor * fOld + TransformTarget._LColor * fNew;
                FOV = FOV * fOld + TransformTarget._FOV * fNew;
                NearDistance = NearDistance * fOld + TransformTarget._NearDistance * fNew;
                FarDistance = FarDistance * fOld + TransformTarget._FarDistance * fNew;
                TargetPoint = TargetPoint * fOld + TransformTarget._TargetPoint * fNew;
            }
        }

        internal ShadowLight(Vector3 LightPosition, Vector4 LightColor, float LightPower, float LightFOV, float LightNearDistance, float LightFarDistance, bool LightIsSpotLight)
        {
            _Position = LightPosition;
            _LColor = LightColor;
            _LColor.Normalize();
            _LColor *= LightPower;
            _LColor.W = 1f;
            _FOV = LightFOV;
            _NearDistance = LightNearDistance;
            _FarDistance = LightFarDistance;
            IsSpotLight = LightIsSpotLight;
        }

        internal void SetUp()
        {
            Transform();
            Matrix LightViewProjection = Matrix.LookAtLH(Position, TargetPoint, new Vector3(0, 0, 1)) * Matrix.PerspectiveFovLH(FOV, (float)(MainForm._RenderWindowSize.Width / MainForm._RenderWindowSize.Height), NearDistance, FarDistance);
            Engine.effect.SetValue("xLightPos", new Vector4(Position, 1.0f));
            Engine.effect.SetValue("xLightViewProjection", LightViewProjection);
            Engine.effect.SetValue("xLightColor", LColor);
            Engine.effect.SetValue("xMaxDepth", FarDistance);
            Engine.effect.SetValue("xUseSpotLight", IsSpotLight ? 1 : 0);
        }
    }



    static partial class Engine
    {

        static RenderToSurface RtsHelperLight;
        static RenderToSurface RtsHelperShadow;
        static RenderToSurface RtsHelperCamera;
        static Texture ShadowLightDepthMap; // Rendered once for every light
        static Texture ShadowCameraDepthMap; // Rendered once for every light
        static Texture[] ShadowMapTexture = new Texture[2]; // Array, two are needed so one can be used as input and the other for output. 
        static Surface ShadowLightDepthMapSurface;
        static Surface ShadowCameraDepthMapSurface;
        static Surface[] ShadowMapSurface = new Surface[2];

        static int ShadowMapSize = 1024;

        struct UserVertex
        {
            internal Vector3 Position;
            internal Vector2 TexturePosition;
        }

        static VertexBuffer QuadVertexBuffer;
        static IndexBuffer QuadIndexBuffer;

        internal static void SetSpotLight()
        {
            Game.Move LastMove = Game.LastMove;
            if (LastMove == null)
            {
                if (ShadowLight.LightList.Count < 1) return;
                ShadowLight.LightList[0].TransformTarget = null;
                ShadowLight.LightList[0].TransForm = false;
                return;
            }

            // Victory, not undone.
            if ((Game._GameResult == Game.GAMERESULT_LIGHTWINS || Game._GameResult == Game.GAMERESULT_DARKWINS) && !Game.CanRedo())
            {
                if (ShadowLight.LightList[0].TransForm)
                    ShadowLight.LightList[0].fTransform = 0.0f;

                ShadowLight.LightList[0].TransformTarget = new ShadowLight(
                        ShadowLight.LightList[0]._Position,               // Position
                        new Vector4(0.0f, 0.0f, 0.0f, 0.0f),    // Color
                        0.0f,                                   // Power
                        ShadowLight.LightList[0]._FOV,          // FOV
                        ShadowLight.LightList[0]._NearDistance, // Near
                        ShadowLight.LightList[0]._FarDistance,
                        false);
            }
            else
            {
                // Highlight last move.
                int x = LastMove.X;
                int z = LastMove.Z;

                if (ShadowLight.LightList[0].TransForm)
                    ShadowLight.LightList[0].fTransform = 0.0f;

                ShadowLight.LightList[0].TransformTarget = new ShadowLight(
                        new Vector3(-1f + ((float)(x - 1)) * 2 / 3, 7f, -1f + ((float)(z - 1)) * 2 / 3),               // Position
                        new Vector4(1.2f, 1.0f, 0.5f, 1.0f),    // Color
                        2.5f,                                   // Power
                        (float)(Math.PI / 14f),                  // FOV
                        5f,                                     // Near
                        10f,
                        true);

                ShadowLight.LightList[0].TransformTarget._TargetPoint = new Vector3(-1f + ((float)(x - 1)) * 2 / 3, 0, -1f + ((float)(z - 1)) * 2 / 3);
            }
        }

        static void FixScreenAlignedQuad()
        {
            float w = (1f / (float)device.Viewport.Width);
            float h = (1f / (float)device.Viewport.Height);
            UserVertex[] QuadVertices = new UserVertex[] { 
            new UserVertex() { Position = new Vector3(-1f -w, -1f +h, 0f), TexturePosition = new Vector2(0f, 1f)},
            new UserVertex() { Position = new Vector3(1f -w, -1f +h, 0f), TexturePosition = new Vector2(1f, 1f)},
            new UserVertex() { Position = new Vector3(1f -w, 1f +h, 0f), TexturePosition = new Vector2(1f, 0f)},
            new UserVertex() { Position = new Vector3(-1f -w, 1f +h, 0f), TexturePosition = new Vector2(0f, 0f)},
            };
            DataStream VertexStream = QuadVertexBuffer.Lock(0, 0, LockFlags.None);
            VertexStream.Seek(0, System.IO.SeekOrigin.Begin);
            VertexStream.WriteRange(QuadVertices);
            QuadVertexBuffer.Unlock();
        }

        static void LoadScreenAlignedQuad()
        {
            float w = 1f / (float)device.Viewport.Width;
            float h = 1f / (float)device.Viewport.Height;
            UserVertex[] QuadVertices = new UserVertex[] { 
            new UserVertex() { Position = new Vector3(-1f -w, -1f +h, 0f), TexturePosition = new Vector2(0f, 1f)},
            new UserVertex() { Position = new Vector3(1f -w, -1f +h, 0f), TexturePosition = new Vector2(1f, 1f)},
            new UserVertex() { Position = new Vector3(1f -w, 1f +h, 0f), TexturePosition = new Vector2(1f, 0f)},
            new UserVertex() { Position = new Vector3(-1f -w, 1f +h, 0f), TexturePosition = new Vector2(0f, 0f)},
            };
            QuadVertexBuffer = new VertexBuffer(device, 4 * sizeof(float) * 5, Usage.None, VertexFormat.Position | VertexFormat.Texture1, Pool.Managed);
            QuadVertexBuffer.Lock(0, 0, LockFlags.None).WriteRange(QuadVertices);
            QuadVertexBuffer.Unlock();
            QuadIndexBuffer = new IndexBuffer(device, sizeof(short) * 6, Usage.None, Pool.Managed, true);

            short[] Indices = new short[] {
                3,1,0,
                2,1,3
            };
            QuadIndexBuffer.Lock(0, 0, LockFlags.None).WriteRange(Indices);
            QuadIndexBuffer.Unlock();
        }

        static void DrawScreenAlignedQuad()
        {
            device.VertexFormat = VertexFormat.Position | VertexFormat.Texture1;
            device.Indices = QuadIndexBuffer;
            device.SetStreamSource(0, QuadVertexBuffer, 0, sizeof(float) * 5);
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }

        static void RenderSoftShadowedScene(bool Blend)
        {
            effect.SetValue("xInverseViewPort", new Vector2(1f / (float)(device.Viewport.Width), 1f / (float)(device.Viewport.Height)));
            if (Blend)
                effect.Technique = "SoftShadowedSceneBlend";
            else
                effect.Technique = "SoftShadowedScene";
            Texture test = ShadowMapTexture[1];
            effect.SetTexture("xShadowMap", test);
            int numpasses = effect.Begin(0);
            for (int i = 0; i < numpasses; i++)
            {
                effect.BeginPass(i);
                DrawScene();
                effect.EndPass();
            }
            effect.End();
        }

        static void GenerateCameraDepthMap()
        {
            RtsHelperCamera.BeginScene(ShadowCameraDepthMapSurface, device.Viewport);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.SetValue("xCameraDepth", 15);
            effect.SetValue("xMaxDepth", 15);
            effect.Technique = "ShadowCameraDepthMap";
            int numpasses = effect.Begin(0);
            for (int i = 0; i < numpasses; i++)
            {
                effect.BeginPass(i);
                DrawScene();
                effect.EndPass();
            }
            effect.End();
            RtsHelperCamera.EndScene(Filter.None);
        }

        static void GenerateShadowMap(ShadowLight CurrentLight)
        {
            effect.SetTexture("xCameraMap", ShadowCameraDepthMap);
            CurrentLight.SetUp();
            // Create shadow for the light
            RtsHelperLight.BeginScene(ShadowLightDepthMapSurface, new Viewport(0, 0, ShadowMapSize, ShadowMapSize));
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Technique = "ShadowMap";
            
            int numpasses = effect.Begin(0);
            for (int i = 0; i < numpasses; i++)
            {
                effect.BeginPass(i);
                DrawScene();
                effect.EndPass();
            }
            effect.End();
            RtsHelperLight.EndScene(Filter.None);

            effect.SetTexture("xShadowMap", ShadowLightDepthMap);

            // Create shadow buffer
            RtsHelperShadow.BeginScene(ShadowMapSurface[1], device.Viewport);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.White, 1.0f, 0);
            effect.Technique = "ShadowBuffer";
            numpasses = effect.Begin(0);
            for (int i = 0; i < numpasses; i++)
            {
                effect.BeginPass(i);
                DrawScene();
                effect.EndPass();
            }
            effect.End();
            RtsHelperShadow.EndScene(Filter.None);
            
            // Blur map
            float W = (float)device.Viewport.Width / 2;
            float H = (float)device.Viewport.Height / 2;
            for (int z = 0; z < 2; z++)
            {
                SetGaussianOffsets(z == 0, new Vector2(1f / 1500f));

                RtsHelperShadow.BeginScene(ShadowMapSurface[z], device.Viewport);
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Green, 1.0f, 0);
                effect.Technique = "ShadowBlur";
                effect.SetTexture("xShadowMap", ShadowMapTexture[-z + 1]);
                numpasses = effect.Begin(0);
                for (int i = 0; i < numpasses; i++)
                {
                    effect.BeginPass(i);
                    DrawScreenAlignedQuad();
                    effect.EndPass();
                }
                effect.End();
                RtsHelperShadow.EndScene(Filter.None);
            }//*/
        }
        static void FillResources()
        {
            if (!Options.Option_Shaders) return;
            // Light camera depth map:
            RtsHelperLight = new RenderToSurface(device, ShadowMapSize, ShadowMapSize, Format.R16F, Format.D16);
            ShadowLightDepthMap = new Texture(device, ShadowMapSize, ShadowMapSize, 1, Usage.RenderTarget, Format.R16F, Pool.Default);
            ShadowLightDepthMapSurface = ShadowLightDepthMap.GetSurfaceLevel(0);

            // Light camera depth map:
            RtsHelperCamera = new RenderToSurface(device, device.Viewport.Width, device.Viewport.Height, Format.R16F, Format.D16);
            ShadowCameraDepthMap = new Texture(device, device.Viewport.Width, device.Viewport.Height, 1, Usage.RenderTarget, Format.R16F, Pool.Default);
            ShadowCameraDepthMapSurface = ShadowCameraDepthMap.GetSurfaceLevel(0);

            // Shadow maps
            RtsHelperShadow = new RenderToSurface(device, device.Viewport.Width, device.Viewport.Height, Format.A8R8G8B8, Format.D16);
            ShadowMapTexture[0] = new Texture(device, device.Viewport.Width, device.Viewport.Height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            ShadowMapTexture[1] = new Texture(device, device.Viewport.Width, device.Viewport.Height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            ShadowMapSurface[0] = ShadowMapTexture[0].GetSurfaceLevel(0);
            ShadowMapSurface[1] = ShadowMapTexture[1].GetSurfaceLevel(0);
        }

        static void DisposeResources()
        {
            if (RtsHelperLight == null || RtsHelperLight.Disposed) return;
            RtsHelperLight.Dispose();
            RtsHelperShadow.Dispose();
            RtsHelperCamera.Dispose();
            ShadowCameraDepthMap.Dispose();
            ShadowLightDepthMap.Dispose();
            ShadowMapTexture[0].Dispose();
            ShadowMapTexture[1].Dispose();
        }

        static float GetGaussianDistribution(float x, float y, float rho)
        {
            float g = 1.0f / (float)Math.Sqrt(2.0 * (float)Math.PI * rho * rho);
            return g * (float)Math.Exp(-(x * x + y * y) / (2 * rho * rho));
        }

        static void SetGaussianOffsets(bool bHorizontal, Vector2 vViewportTexelSize)
        {
            float[] fSampleWeights = new float[16];
            Vector2[] vSampleOffsets = new Vector2[16];
            // Get the center texel offset and weight
            fSampleWeights[0] = 1.0f * GetGaussianDistribution(0, 0, 2.0f);
            vSampleOffsets[0] = new Vector2(0.0f, 0.0f);
            // Get the offsets and weights for the remaining taps
            if (bHorizontal)
            {
                for (int i = 1; i < 15; i += 2)
                {
                    vSampleOffsets[i + 0] = new Vector2(i * vViewportTexelSize.X, 0.0f);
                    vSampleOffsets[i + 1] = new Vector2(-i * vViewportTexelSize.X, 0.0f);
                    fSampleWeights[i + 0] = 2.0f * GetGaussianDistribution((float)(i + 0), 0.0f, 3.0f);
                    fSampleWeights[i + 1] = 2.0f * GetGaussianDistribution((float)(i + 1), 0.0f, 3.0f);
                }
            }
            else
            {
                for (int i = 1; i < 15; i += 2)
                {
                    vSampleOffsets[i + 0] = new Vector2(0.0f, i * vViewportTexelSize.Y);
                    vSampleOffsets[i + 1] = new Vector2(0.0f, -i * vViewportTexelSize.Y);
                    fSampleWeights[i + 0] = 2.0f * GetGaussianDistribution(0.0f, (float)(i + 0), 3.0f);
                    fSampleWeights[i + 1] = 2.0f * GetGaussianDistribution(0.0f, (float)(i + 1), 3.0f);
                }
            }
            effect.SetValue("xSampleOffsets", vSampleOffsets);
            effect.SetValue("xSampleWeights", fSampleWeights);
        }
    }
}
