using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9; 
using SlimDX;
using System.Drawing;

namespace Connect_4_3D
{
    static partial class Engine
    {
        static Device device;

        static Dictionary<string, Texture> TextureDict = new Dictionary<string, Texture>();

        static Mesh Board;
        static Mesh Ring;
        static Mesh SkyBox;
        static Mesh Table;

        static Matrix RingScaler = Matrix.Scaling(0.23f, 0.23f, 0.23f); // Ring Scaler
        static Matrix BoardScaler = Matrix.Scaling(1.0f, 0.8f, 1.0f); // Scales the board
        static Matrix TableScaler = Matrix.Scaling(5.0f, 5.0f, 5.0f); // Scales the table

        static Matrix ViewMatrix;
        static Matrix CameraMatrix;

        internal static Effect effect;

        static bool IsPrimaryLightRender; 

        static SlimDX.Direct3D9.Font FeedBackFont;
        static Sprite FontSprite;

        

        internal static Texture GetTextureByFileName(string sFile)
        {
            if (sFile == "") return null;
            if (TextureDict.ContainsKey(sFile))
                return TextureDict[sFile];

            string sFileName = sFile.Substring(sFile.LastIndexOf('/') +1);

            Texture NewTexture = Texture.FromStream(device, Resource_Manager.GetResourceStream(sFileName), Usage.None, Pool.Managed);

            TextureDict.Add(sFile, NewTexture); // Never load the same texture twice.

            return NewTexture;
        }

        static void RenderMesh(Mesh MeshToRender)
        {
            RenderMesh(MeshToRender, "");
        }

        static void RenderMesh(Mesh MeshToRender, string ForcedTexture)
        {
            if (MeshToRender == null) return;

            device.Indices = MeshToRender.IndexBuffer;

            device.SetStreamSource(0, MeshToRender.VertexBuffer, 0, MeshToRender.BytesPerVertex);

            device.VertexFormat = MeshToRender.VertexFormat;

            if (ForcedTexture != "")
            {
                SetTexture(GetTextureByFileName(ForcedTexture));
            }

            int i = 0;

            foreach (ExtendedMaterial CurrentMaterial in MeshToRender.GetMaterials())
            {
                if (ForcedTexture == "")
                    SetTexture(GetTextureByFileName(CurrentMaterial.TextureFileName));
                if (Options.Option_Shaders)
                    effect.CommitChanges();
                MeshToRender.DrawSubset(i++);
            }
        }

        static double CamaraAngle1 = Math.PI * 1;
        static double CamaraAngle2 = Math.PI * 0.2;

        // X, Y, Z All go from 1 to 4. Y is upwards, as always.
        static void RenderRing(int x, int y, int z, bool IsDark)
        {
            string sTexture = IsDark ? "dark_wood.jpg" : "light_wood.jpg";

            float XZMult = (2f / 3f);
            float YMult = 0.27f;

            float YBase = -0.05f;
            float XZBase = -1f - XZMult;

            SetWorldTransform(Matrix.Identity, RingScaler * Matrix.Translation(XZMult * x + XZBase, YMult * y + YBase, XZMult * z + XZBase));

            RenderMesh(Ring, sTexture);
        }

        internal static void CheckMouse(int MouseX, int MouseY)
        {
            int width = MainForm._RenderWindowSize.Width;
            int height = MainForm._RenderWindowSize.Height;

            Matrix mViewProj = ViewMatrix * CameraMatrix;

            Vector3 ZNearPlane = Vector3.Unproject(new Vector3(MouseX, MouseY, 0), 0, 0, width, height, -1000, 1000, mViewProj);
            Vector3 ZFarPlane = Vector3.Unproject(new Vector3(MouseX, MouseY, 1), 0, 0, width, height, -1000, 1000, mViewProj);

            Vector3 direction = ZFarPlane - ZNearPlane;
            direction.Normalize();

            Ray ray = new Ray(ZNearPlane, direction);

            BoundingBox BoundTest = new BoundingBox();

            float XZMult = (2f / 3f);
            float XZBase = -1f - XZMult;
            float XZRadius = 0.075f;


            float ClosestDistance = 10.0f;
            float distance;

            int FoundX = 0;
            int FoundZ = 0;

            for (int x = 1; x < 5; x++)
            {
                for (int z = 1; z < 5; z++)
                {
                    BoundTest.Minimum = new Vector3(
                        XZBase + XZMult * x - XZRadius,
                        0.08f,
                        XZBase + XZMult * z - XZRadius);
                    BoundTest.Maximum = new Vector3(
                        XZBase + XZMult * x + XZRadius,
                        1.24f,
                        XZBase + XZMult * z + XZRadius);

                    if (Ray.Intersects(ray, BoundTest, out distance))
                    {
                        if (distance < ClosestDistance)
                        {
                            ClosestDistance = distance;
                            FoundX = x;
                            FoundZ = z;
                        }
                    }
                }
            }
            if (FoundX != 0)
            {
                //System.Windows.Forms.MessageBox.Show(String.Format("X: {0} Z:{1}", FoundX, FoundZ));
                Game.PerformMove(FoundX, FoundZ);
            }
        }

        static void SetTexture(Texture ToSet)
        {
            if (Options.Option_Shaders)
            {
                effect.SetTexture("xColoredTexture", ToSet);
            }
            else
            {
                device.SetTexture(0, ToSet);
            }
        }

        static void SetWorldTransform(Matrix Rotation, Matrix ScaleAndTranslate)
        {
            if (Options.Option_Shaders)
            {
                effect.SetValue("xRotate", Rotation);
                effect.SetValue("xTranslateAndScale", ScaleAndTranslate);
            }
            else
            {
                device.SetTransform(TransformState.World, Rotation * ScaleAndTranslate);
            }
        }

        static void DrawScene()
        {
            SetWorldTransform(Matrix.Identity, TableScaler);
            RenderMesh(Table);

            SetWorldTransform(Matrix.Identity, BoardScaler);
            RenderMesh(Board);

            int nPosition;

            for (int X = 1; X < 5; X++)
            {
                for (int Y = 1; Y < 5; Y++)
                {
                    for (int Z = 1; Z < 5; Z++)
                    {
                        nPosition = Game.GetPosition(X, Y, Z);

                        if (nPosition != Game.POSITION_EMPTY)
                        {
                            if (IsPrimaryLightRender && Game.CheckIsInVictoryRow(X, Y, Z))
                            {
                                float fDimFactor = 1.0f - ShadowLight.LightList[0].LColor.Length() / ShadowLight.LightList[0]._LColor.Length();

                                if (nPosition == Game.POSITION_DARK)
                                    Engine.effect.SetValue("xAmbientLight", ShadowLight.AmbientLight + (ShadowLight.LightList[0]._LColor * 2.0f * fDimFactor));
                                else
                                    Engine.effect.SetValue("xAmbientLight", ShadowLight.AmbientLight + (ShadowLight.LightList[0]._LColor * 1.3f * fDimFactor));

                                RenderRing(X, Y, Z, nPosition == Game.POSITION_DARK);
                                Engine.effect.SetValue("xAmbientLight", ShadowLight.AmbientLight);
                            }
                            else
                            {
                                RenderRing(X, Y, Z, nPosition == Game.POSITION_DARK);
                            }
                        }
                    }
                }
            }

        }

        internal static void RenderThread()
        {
            if (MainForm._RenderWindowSize.Width < 1)
            {
                System.Threading.Thread.Sleep(50);
                return;
            }
            if (MainForm.MouseDragging)
            {
                if ((System.Windows.Forms.Control.MouseButtons & System.Windows.Forms.MouseButtons.Right) == System.Windows.Forms.MouseButtons.Right)
                {
                    CamaraAngle1 -= NormalizeAngle((double)(MainForm.MouseDragStartX - System.Windows.Forms.Control.MousePosition.X) / 100.0);
                    CamaraAngle2 -= (double)(MainForm.MouseDragStartY - System.Windows.Forms.Control.MousePosition.Y) / 100.0;

                    if (CamaraAngle2 < 0.15) CamaraAngle2 = 0.15;
                    if (CamaraAngle2 > 0.45 * Math.PI) CamaraAngle2 = 0.45 * Math.PI;

                    MainForm.MouseDragStartX = System.Windows.Forms.Control.MousePosition.X;
                    MainForm.MouseDragStartY = System.Windows.Forms.Control.MousePosition.Y;
                }
                else
                {
                    MainForm.MouseDragging = false;
                }
            }

            if (Options.Option_Shaders)
            {
                if ((System.Windows.Forms.Control.MouseButtons & System.Windows.Forms.MouseButtons.Middle) == System.Windows.Forms.MouseButtons.Middle) // Middle mouse.
                {
                    if (!ShadowLight.LightList[0].TransForm && ShadowLight.LightList[0].TransformTarget != null)
                    {
                        ShadowLight.LightList[0].TransForm = true;
                    }
                }
                else if (ShadowLight.LightList[0].TransForm)
                {
                    ShadowLight.LightList[0].TransForm = false;
                }
            }



            MainForm._MainMenu.MenuItems[0].Enabled = !(Networking.Connected || Networking.Connecting);
            MainForm._MainMenu.MenuItems[2].Enabled = (Networking.Connected || Networking.Connecting);

            Vector3 CameraPosition = new Vector3(
                        (float)(Math.Sin(CamaraAngle1) * Math.Cos(CamaraAngle2)) * 5.0f,
                        (float)(Math.Sin(CamaraAngle2)) * 5.0f,
                        (float)(Math.Cos(CamaraAngle1) * Math.Cos(CamaraAngle2)) * 5.0f);
            CameraMatrix = Matrix.PerspectiveFovLH(45.0f / 180.0f * (float)Math.PI, (float)MainForm._RenderWindowSize.Width / (float)MainForm._RenderWindowSize.Height, 1.0f, 100.0f);

            ViewMatrix = Matrix.LookAtLH(
                    CameraPosition,    // the camera position
                    new Vector3(0.0f, 0f, 0f),    // the look-at position
                    new Vector3(0.0f, 1.0f, 0.0f));

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.LightBlue, 1.0f, 0);
            
            device.BeginScene();
            device.SetTransform(TransformState.World, Matrix.Identity);
            device.SetTransform(TransformState.View, ViewMatrix);

            device.SetTransform(TransformState.Projection, CameraMatrix);
            device.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
            device.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
            device.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.Diffuse);

            if (Options.Option_Shaders) // Shader option
            {
                effect.SetValue("xCameraPosition", CameraPosition);
                effect.SetValue("xCameraViewProjection", ViewMatrix * CameraMatrix);
                
                device.EndScene();
                GenerateCameraDepthMap();

                for (int x = 0; x < ShadowLight.LightList.Count(); x++)
                {
                    if (x == 0)
                        effect.SetValue("xAmbientLight", ShadowLight.AmbientLight);
                    else
                        effect.SetValue("xAmbientLight", new Vector4(0f,0f,0f,1f));
                    GenerateShadowMap(ShadowLight.LightList[x]);

                    device.BeginScene();
                    /*using (Sprite spriteobject = new Sprite(device))
                    {
                        spriteobject.Begin(SpriteFlags.None);
                        spriteobject.Draw(ShadowLightDepthMap, new Rectangle(0, 0, ShadowMapSize, ShadowMapSize), new Vector3(0, 0, 0), new Vector3(0, 0, 0), Color.White);
                        spriteobject.End();
                    }//*/

                    IsPrimaryLightRender = x == 0;

                    RenderSoftShadowedScene(x != 0);

                    IsPrimaryLightRender = false;

                    device.EndScene();
                }
                device.BeginScene();
            }
            else
            {
                DrawScene();
            }

            if (Options.Option_Skybox)
            {
                bool UseShader = Options.Option_Shaders;
                Options.Option_Shaders = false;
                device.SetRenderState(RenderState.Lighting, false); // Lighting
                RenderMesh(SkyBox);
                device.SetRenderState(RenderState.Lighting, true); // Lighting
                Options.Option_Shaders = UseShader;
            }

            DisplayFeedBackText();
#if DEBUG
            UpdateFramerate();
#endif
            device.EndScene();
            device.Present();
        }
#if DEBUG
        static int Frames;
        static int LastTickCount;
        static float LastFrameRate;

        static void UpdateFramerate()
        {
            Frames++;
            if (Math.Abs(Environment.TickCount - LastTickCount) > 1000)
            {
                LastFrameRate = (float)Frames * 1000 / Math.Abs(Environment.TickCount - LastTickCount);
                LastTickCount = Environment.TickCount;
                Frames = 0;
            }
            DrawText(10, 30, string.Format("Framerate : {0:0.00} fps", LastFrameRate), new Color4(1.0f, 0.0f, 0.0f), new Color4(0.3f, 0.0f, 0.0f, 0.0f));
            //FeedBackFont.DrawString(null, , 10, 430, Color.Red);
        }
#endif

        static void DisplayFeedBackText()
        {
            int TextX = 10;
            int TextY = 10;
            string sText = "Start a game via the menu.";
            Color4 TextColor = new Color4(0.9f, 0.9f, 0.9f);

            if (Game._GameType != Game.GAMETYPE_NOTSTARTED)
            {
                if (Game._GameResult == Game.GAMERESULT_WAITFORCONNECTION)
                {
                    sText = "Waiting for connection";
                }
                else if (Game._GameResult == Game.GAMERESULT_DISCONNECTED)
                {
                    TextColor = new Color4(1.0f, 0.2f, 0.2f);
                    sText = "Connection lost";
                }
                else if (Game._GameResult == Game.GAMERESULT_CONNECTIONFAILED)
                {
                    TextColor = new Color4(1.0f, 0.2f, 0.2f);
                    sText = "Connection failed";
                }
                else if (Game._GameResult == Game.GAMERESULT_ONGOING)
                {
                    TextColor = Game._CurrentTurn ? new Color4(0.6f, 0.3f, 0.3f) : new Color4(0.9f, 0.9f, 0.5f);

                    string sPlayerDes = "Opponent";

                    if (Game._GameType == Game.GAMETYPE_AIONLY)
                    {
                        sPlayerDes = "AI";
                    } else if (Game._GameType == Game.GAMETYPE_HOTSEAT)
                    {
                        sPlayerDes = "Current";
                    }
                    else if (Game._LocalPlayerSide == Game._CurrentTurn)
                    {
                        sPlayerDes = "Your";
                    }
                    else if (Game._GameType == Game.GAMETYPE_SINGLEPLAYER)
                    {
                        sPlayerDes = "Computer";
                    }

                    sText = String.Format("{1} Turn: {0}", Game._CurrentTurn ? "Dark" : "Light", sPlayerDes);
                }
                else if (Game._GameResult == Game.GAMERESULT_DRAW)
                {
                    TextColor = new Color4(0.5f, 1.0f, 0.5f);
                    sText = "Game has ended in a draw";
                }
                else 
                {
                    sText = String.Format("Victory for {0}", Game._GameResult == Game.GAMERESULT_DARKWINS ? "Dark" : "Light");
                    TextColor = new Color4(0.5f, 1.0f, 0.5f);
                }
            }
            DrawText(TextX, TextY, sText, TextColor, new Color4(0.3f, 0.0f,0.0f,0.0f));
            
        }

        static void DrawText(int X, int Y, string sText, Color4 TextColor, Color4 OutlineColor)
        {
            FontSprite.Begin(SpriteFlags.AlphaBlend);

            Rectangle StringRectangle = FeedBackFont.MeasureString(FontSprite, sText, DrawTextFormat.Left);
            StringRectangle.Height -= 1;
            StringRectangle.Width += 2;

            FontSprite.Draw(GetTextureByFileName("white.png"), StringRectangle,null,new Vector3(X - 1,Y + 2, 0), OutlineColor);
            FeedBackFont.DrawString(FontSprite, sText, X, Y, TextColor);

            FontSprite.Flush();
            FontSprite.End();
        }

        static double NormalizeAngle(double fAngle)
        {
            while (fAngle > (Math.PI * 2)) fAngle -= (Math.PI * 2);
            while (fAngle < 0.0) fAngle += (Math.PI * 2);
            return fAngle;
        }


        static void InitEffects()
        {
            effect = Effect.FromStream(device, Resource_Manager.GetResourceStream("shader.fx"), ShaderFlags.None);
        }

        internal static void InitRenderDevice()
        {
            device = new Device(new Direct3D(), 0, DeviceType.Hardware, MainForm._Handle, CreateFlags.HardwareVertexProcessing, GetDeviceParameters());
            
            CheckEngineCapabilites();
            InitEffects();
            SetRenderState();
            InitFont();
            LoadModels();
            InitLighting();
            FillResources();
        }

        static void LoadModels()
        {
            Board = LoadMesh("Board.x");
            Ring = LoadMesh("Ring.x");
            SkyBox = LoadMesh("Skybox.x");
            Table = LoadMesh("Table.x");
            LoadScreenAlignedQuad();
        }

        static Mesh LoadMesh(string sFile)
        {
            Mesh mesh = Mesh.FromStream(device, Resource_Manager.GetResourceStream(sFile), MeshFlags.Managed);
            mesh.ComputeNormals();
            return mesh;
        }

        static void InitFont()
        {
            FontSprite = new Sprite(device);
            FeedBackFont = new SlimDX.Direct3D9.Font(device, new System.Drawing.Font("Verdana", 11f, FontStyle.Regular));
        }

        static void SetRenderState()
        {
            device.SetRenderState(RenderState.Lighting, true); // Lighting
            device.SetRenderState(RenderState.ZEnable, true); // Z-Buffer, draw order
            device.SetRenderState(RenderState.CullMode, Cull.Counterclockwise); // Clockwise only, singlesided polygons.
            device.SetRenderState(RenderState.Ambient, Color.FromArgb(50, 50, 50).ToArgb());
            device.SetRenderState(RenderState.NormalizeNormals, true);
            device.SetRenderState(RenderState.MultisampleAntialias, Options.Option_AntiAliasing);
            if (Options.Option_Anisotropic > 0)
            {
                device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Anisotropic);
                device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Anisotropic);
                device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.Anisotropic);
                device.SetSamplerState(0, SamplerState.MaxAnisotropy, Options.Option_Anisotropic);
            }
            else
            {
                device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);
                device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
                device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.Linear);
            }
            device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Mirror); // Fixes skybox lines.
            device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Mirror);
            effect.SetValue("xAnisotropy", Options.Option_Anisotropic / 2);
        }

        static PresentParameters GetDeviceParameters()
        {
            PresentParameters Para = new PresentParameters()
            {
                BackBufferWidth = MainForm._RenderWindowSize.Width,
                BackBufferHeight = MainForm._RenderWindowSize.Height,
                EnableAutoDepthStencil = true,
                Multisample = MultisampleType.FourSamples, // Anti alliasing
                PresentationInterval = PresentInterval.One, // VSynch
                AutoDepthStencilFormat = Format.D16
            };

            if (!Options.Option_VSynch)
                Para.PresentationInterval = PresentInterval.Immediate;
            if (Options.Option_AntiAliasing == 2)
                Para.Multisample = MultisampleType.TwoSamples;

            return Para;
        }

        static void InitLighting()
        {
            if (Options.Option_Shaders)
            {// Shader lighting
                ShadowLight.LightList.Clear();
                ShadowLight.AmbientLight = new Vector4(0.25f, 0.25f, 0.25f, 1.0f);

                  // Lamp*/
                ShadowLight.LightList.Add(new ShadowLight(
                    new Vector3(1f, 8f, -2f),               // Position
                    new Vector4(1.2f, 1.0f, 0.5f, 1.0f),    // Color
                    1.5f,                                   // Power
                    (float)(Math.PI / 2.5f),                  // FOV
                    6f,                                     // Near
                    10f,
                    true));//*/
                if (Options.Option_Skybox) // Sun
                {
                    ShadowLight.LightList.Add(new ShadowLight(
                        new Vector3(-40f, 20f, 0f),                // Position
                        new Vector4(2.8f, 1.3f, 0.8f, 1f),      // Color
                        1.3f,                                   // Power
                        (float)(Math.PI / 34f),                  // FOV
                        42f,                                     // Near
                        50f,                                  // Far
                        false));
                }

                SetSpotLight();
                Engine.effect.SetTexture("xSpotLight", Engine.GetTextureByFileName("spotlight.jpg"));
            }
            else
            {
                // FFP lighting
                Light light_Sun = new Light();
                Light light_Lamp = new Light();
                Material material = new Material();

                light_Sun.Type = LightType.Directional; // The sun going down
                light_Sun.Diffuse = new Color4(1.0f, 2.6f, 0.4f, 0.4f);
                light_Sun.Direction = new Vector3(1.0f, -0.2f, 0.0f);

                device.SetLight(0, light_Sun);
                device.EnableLight(0, Options.Option_Skybox);

                light_Lamp.Type = LightType.Point; // The lamp, you are using
                light_Lamp.Diffuse = new Color4(1.0f, 2.0f, 2.0f, 2.0f);
                light_Lamp.Position = new Vector3(0.0f, 20.0f, 0.0f);
                light_Lamp.Attenuation0 = 0.0f;
                light_Lamp.Attenuation1 = 0.001f;
                light_Lamp.Attenuation2 = 0.005f; // 
                light_Lamp.Range = 50.0f;

                device.SetLight(1, light_Lamp);
                device.EnableLight(1, true);

                material.Diffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
                material.Ambient = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

                device.Material = material;
            }
            
        }

        internal static void ResetDevice()
        {
            if (device == null) return;
            DisposeResources();
            FontSprite.OnLostDevice();
            FeedBackFont.OnLostDevice();
            effect.OnLostDevice();
            device.Reset(GetDeviceParameters());
            effect.OnResetDevice();
            FeedBackFont.OnResetDevice();
            FontSprite.OnResetDevice();
            FillResources();
            InitLighting();
            SetRenderState();
            FixScreenAlignedQuad();
        }
    }

   
}
