using Hexa.NET.ImGui;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Shuriken.Rendering;
using SUFcoTool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Texture = Shuriken.Rendering.Texture;

namespace FcoEditor.ShurikenRenderer
{
    public class ShurikenRenderHelper
    {
        public struct SViewportData
        {
            public int csdRenderTextureHandle;
            public Vector2i framebufferSize;
            public int renderbufferHandle;
            public int framebufferHandle;
        }
        public struct SProjectConfig
        {
            public string WorkFilePath;
            public bool playingAnimations;
            public bool showQuads;
            public double time;
        }
        public List<FcoEditor.Window> windowList = new List<FcoEditor.Window>();
        public Renderer renderer;
        public Vector2 viewportSize;
        public Vector2 screenSize;
        public FCO fcoFile;
        public FTE fteFile;
        public SProjectConfig config;
        private SViewportData viewportData;
        private GameWindow window;
        private int currentDrawPriority;

        public ShurikenRenderHelper(GameWindow window2, Vector2 in_ViewportSize, Vector2 clientSize)
        {
            viewportSize = in_ViewportSize;
            window = window2;
            viewportData = new SViewportData();
            config = new SProjectConfig();
            screenSize = clientSize;
        }

        public void ShowMessageBoxCross(string title, string message, bool isWarning)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, isWarning ? System.Windows.MessageBoxImage.Warning : System.Windows.MessageBoxImage.Information);
            }
        }
        public void LoadFile(string in_Path, string in_PathFte)
        {
            config.WorkFilePath = in_Path;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var pathTable = Path.Combine("tables", "Languages", "English", "Retail", "WorldMap.json");
            fcoFile = FCO.Read(config.WorkFilePath, pathTable, false);
            stopwatch.Stop();
            Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds}s");

            fteFile = FTE.Read(in_PathFte, false);

            string parentPath = Directory.GetParent(config.WorkFilePath).FullName;
            SpriteHelper.textureList = new("");
            List<string> missingTextures = new List<string>();
            foreach (var texture in fteFile.Textures)
            {
                string pathtemp = Path.Combine(parentPath, texture.Name + ".dds");
                if (File.Exists(pathtemp))
                    SpriteHelper.textureList.Textures.Add(new Texture(pathtemp, false));
                else
                {
                    var commonPathTexture = Path.Combine(Program.programDir,"Resources","CommonTextures",texture.Name + ".dds");
                    if (File.Exists(commonPathTexture))
                    {
                        SpriteHelper.textureList.Textures.Add(new Texture(commonPathTexture, false));
                    }
                    else
                    {
                        SpriteHelper.textureList.Textures.Add(new Texture("", false));
                        missingTextures.Add(texture.Name);
                    }
                }
            }
            if (missingTextures.Count > 0)
            {
                string textureNames = "";
                foreach (string textureName in missingTextures)
                    textureNames += "-" + textureName + "\n";
                ShowMessageBoxCross("Warning", $"The file uses textures that could not be found, they will be replaced with squares.\n\nMissing Textures:\n{textureNames}", true);
            }
            ResetWindows();
            SpriteHelper.LoadTextures(fteFile.Characters);
        }
        /// <summary>
        /// Renders contents of a CsdProject to a GL texture for use in ImGui
        /// </summary>
        /// <param name="in_CsdProject"></param>
        /// <param name="in_DeltaTime"></param>
        /// <exception cref="Exception"></exception>
        private void RenderToViewport(float in_DeltaTime)
        {
            GL.ClearColor(Color4.BlueViolet);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderer.Width = (int)viewportSize.X;
            renderer.Height = (int)viewportSize.Y;
            renderer.Start();

            int idx = 0;
            int idx2 = 0;
            foreach(var c in ViewportWindow.test)
            {
                var e = TranslationService.TXTtoHEX(c.ToString(), fcoFile.TranslationTable);
                var spr = SpriteHelper.GetSpriteFromConverseID(e);

                float width = (float)spr.Dimensions.X / (float)renderer.Width;
                float height = (float)spr.Dimensions.Y / (float)renderer.Height;

                var begin = new Vector2(0, 0);
                var end = begin + new Vector2(width, height);

                var pos = new Vector2(0.1f + (0.03f * idx), 0.1f + (0.05f * idx2));
                var color = new System.Numerics.Vector4(1, 1, 1, 1);
                renderer.DrawSprite(
                    new Vector2(begin.X, begin.Y),
                    new Vector2(begin.X, end.Y),
                    new Vector2(end.X, begin.Y),
                    new Vector2(end.X, end.Y), pos, 0, new Vector2(1, 1), 16.0f / 9.0f, spr, spr, 0, color, color, color, color, color, 1);

                idx++;
                if (idx == 12)
                {
                    idx = 0;
                    idx2++;
                }
            }
            foreach (var s in SpriteHelper.Sprites)
            {
                
            }
            renderer.End();
        }
        public int GetViewportImageHandle()
        {
            return viewportData.csdRenderTextureHandle;
        }
        public void SaveCurrentFile(string in_Path)
        {
            fcoFile.Write(in_Path + "test");
        }

        internal void RenderWindows()
        {
            foreach (var item in windowList)
            {
                item.Render(this);
            }
        }
        void ResetWindows()
        {
            foreach (var item in windowList)
            {
                item.OnReset(this);
            }
        }
    }
}


//using Hexa.NET.ImGui;
//using FcoEditor.Window;
//using OpenTK.Graphics.OpenGL;
//using OpenTK.Mathematics;
//using OpenTK.Windowing.Desktop;
//using SharpNeedle.Ninja.Csd;
//using SharpNeedle.Ninja.Csd.Motions;
//using Shuriken.Models;
//using Shuriken.Rendering;
//using System.Diagnostics;
//using System.IO;

//namespace FcoEditor.ShurikenRenderer
//{
//    [Flags]
//    public enum AnimationType : uint
//    {
//        None = 0,
//        HideFlag = 1,
//        XPosition = 2,
//        YPosition = 4,
//        Rotation = 8,
//        XScale = 16,
//        YScale = 32,
//        SubImage = 64,
//        Color = 128,
//        GradientTL = 256,
//        GradientBL = 512,
//        GradientTR = 1024,
//        GradientBR = 2048
//    }

//    public class SVisibilityData
//    {
//        public class SCast
//        {
//            public Cast Cast;
//            public bool Active = true;
//            public SCast(Cast in_Scene)
//            {
//                Cast = in_Scene;
//            }
//        }
//        public class SAnimation
//        {
//            public KeyValuePair<string, Motion> Motion;
//            public bool Active = true;
//            public SAnimation(KeyValuePair<string, Motion> in_Scene)
//            {
//                Motion = in_Scene;

//            }
//            public KeyFrameList GetTrack(Cast layer, AnimationType type)
//            {
//                foreach (SharpNeedle.Ninja.Csd.Motions.FamilyMotion animation in Motion.Value.FamilyMotions)
//                {
//                    foreach (SharpNeedle.Ninja.Csd.Motions.CastMotion animtrack in animation.CastMotions)
//                    {
//                        if (layer == animtrack.Cast)
//                        {
//                            if (animtrack.Capacity != 0)
//                            {
//                                foreach (var track in animtrack)
//                                {
//                                    if (track.Property.ToShurikenAnimationType() == type)
//                                        return track;
//                                }
//                            }
//                        }
//                    }
//                }

//                return null;
//            }
//        }
//        public class SScene
//        {
//            public List<SAnimation> Animation = new List<SAnimation>();
//            public List<SCast> Casts = new List<SCast>();
//            public KeyValuePair<string, Scene> Scene;
//            public bool Active = true;
//            public SScene(KeyValuePair<string, Scene> in_Scene)
//            {
//                Scene = in_Scene;
//                foreach (var group in Scene.Value.Families)
//                {
//                    foreach (var cast in group.Casts)
//                    {
//                        Casts.Add(new SCast(cast));
//                    }
//                }
//                foreach (var mot in Scene.Value.Motions)
//                {
//                    Animation.Add(new SAnimation(mot));
//                }
//            }
//            public SCast GetVisibility(Cast cast)
//            {
//                return Casts.FirstOrDefault(node => node.Cast == cast);
//            }
//        }
//        public class SNode
//        {

//            public List<SScene> Scene = new List<SScene>();
//            public List<SNode> Nodes = new List<SNode>();
//            public KeyValuePair<string, SceneNode> Node;
//            public bool Active = true;


//            public SNode(KeyValuePair<string, SceneNode> in_Node)
//            {
//                Node = in_Node;
//                foreach (var scene in Node.Value.Scenes)
//                {
//                    Scene.Add(new SScene(scene));
//                }
//                foreach (var scene in in_Node.Value.Children)
//                {
//                    Nodes.Add(new SNode(scene));
//                }
//            }
//            public SScene GetVisibility(Scene scene)
//            {
//                return Scene.FirstOrDefault(node => node.Scene.Value == scene);
//            }
//        }

//        public List<SNode> Nodes = new List<SNode>();

//        public SVisibilityData(CsdProject in_Proj)
//        {
//            Nodes.Add(new SNode(new KeyValuePair<string, SceneNode>("Root", in_Proj.Project.Root)));

//        }
//        private SNode RecursiveGetNode(SNode in_Node, SceneNode in_SceneNode)
//        {
//            if (in_Node.Node.Value == in_SceneNode)
//                return in_Node;
//            foreach (var node in in_Node.Nodes)
//            {
//                if (node.Node.Value == in_SceneNode)
//                    return node;
//            }
//            return null;
//        }
//        private SScene RecursiveGetScene(SNode in_Node, Scene in_Scene)
//        {
//            foreach (var s in in_Node.Scene)
//            {
//                if (s.Scene.Value == in_Scene)
//                    return s;
//            }
//            foreach (var node in in_Node.Nodes)
//            {
//                return RecursiveGetScene(node, in_Scene);
//            }
//            return null;
//        }
//        public SNode GetVisibility(SceneNode scene)
//        {
//            foreach (var node in Nodes)
//            {
//                return RecursiveGetNode(node, scene);
//            }
//            return null;
//        }
//        public SScene GetScene(Scene scene)
//        {
//            foreach (var node in Nodes)
//            {
//                return RecursiveGetScene(node, scene);
//            }
//            return null;
//        }
//    }
//    public class ShurikenRenderHelper
//    {
//        public struct SViewportData
//        {
//            public int csdRenderTextureHandle;
//            public Vector2i framebufferSize;
//            public int renderbufferHandle;
//            public int framebufferHandle;
//        }
//        public struct SProjectConfig
//        {
//            public string WorkFilePath;
//            public bool playingAnimations;
//            public bool showQuads;
//            public double time;
//        }
//        public Renderer renderer;
//        public Vector2 viewportSize;
//        public Vector2 screenSize;
//        public SVisibilityData visibilityData;
//        public CsdProject WorkProjectCsd;
//        public SProjectConfig config;
//        private SViewportData viewportData;
//        private GameWindow window;
//        private int currentDrawPriority;
//        public ShurikenRenderHelper(GameWindow window2, Vector2 in_ViewportSize, Vector2 clientSize)
//        {
//            viewportSize = in_ViewportSize;
//            renderer = new Renderer((int)viewportSize.X, (int)viewportSize.Y);
//            renderer.SetShader(renderer.shaderDictionary["basic"]);
//            window = window2;
//            viewportData = new SViewportData();
//            config = new SProjectConfig();
//            screenSize = clientSize;
//        }

//        public void ShowMessageBoxCross(string title, string message, bool isWarning)
//        {
//            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//            {
//                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, isWarning ? System.Windows.MessageBoxImage.Warning : System.Windows.MessageBoxImage.Information);
//            }
//        }
//        public void LoadFile(string in_Path)
//        {
//            config.WorkFilePath = in_Path;
//            SonicUnleashedFCOConv.Program.tableArg = "WorldMap_Small";
//            SonicUnleashedFCOConv.Program.currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

//            SonicUnleashedFCOConv.FCO.ReadFCO(config.WorkFilePath);
//            string temp = @"K:\Games\SonicUnleashed\extracted_decompressed\SonicUnleashed\Languages\English\WorldMap\fte_ConverseMain.fte";
//            SonicUnleashedFCOConv.FTE.ReadFTE(temp);

//            string temp2 = Directory.GetParent(temp).FullName;
//            SpriteHelper.textureList = new("");
//            foreach (var t in SonicUnleashedFCOConv.FTE.textures)
//            {
//                string pathtemp = Path.Combine(temp2, t.textureName + ".dds");
//                if (File.Exists(pathtemp))
//                    SpriteHelper.textureList.Textures.Add(new Texture(pathtemp, false));
//                else
//                {
//                    SpriteHelper.textureList.Textures.Add(new Texture("", false));
//                    //missingTextures.Add(texture.Name);
//                }
//            }
//            SpriteHelper.LoadTextures(SonicUnleashedFCOConv.FTE.characters);
//        }
//        /// <summary>
//        /// Renders contents of a CsdProject to a GL texture for use in ImGui
//        /// </summary>
//        /// <param name="in_CsdProject"></param>
//        /// <param name="in_DeltaTime"></param>
//        /// <exception cref="Exception"></exception>
//        public void Render(float in_DeltaTime)
//        {
//            // Get the size of the child (i.e. the whole draw size of the windows).
//            System.Numerics.Vector2 wsize = screenSize;

//            // make sure the buffers are the currect size
//            Vector2i wsizei = new((int)wsize.X, (int)wsize.Y);
//            if (viewportData.framebufferSize != wsizei)
//            {
//                viewportData.framebufferSize = wsizei;

//                // create our frame buffer if needed
//                if (viewportData.framebufferHandle == 0)
//                {
//                    viewportData.framebufferHandle = GL.GenFramebuffer();
//                    // bind our frame buffer
//                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, viewportData.framebufferHandle);
//                    GL.ObjectLabel(ObjectLabelIdentifier.Framebuffer, viewportData.framebufferHandle, 10, "GameWindow");
//                }

//                // bind our frame buffer
//                GL.BindFramebuffer(FramebufferTarget.Framebuffer, viewportData.framebufferHandle);

//                if (viewportData.csdRenderTextureHandle > 0)
//                    GL.DeleteTexture(viewportData.csdRenderTextureHandle);

//                viewportData.csdRenderTextureHandle = GL.GenTexture();
//                GL.BindTexture(TextureTarget.Texture2D, viewportData.csdRenderTextureHandle);
//                GL.ObjectLabel(ObjectLabelIdentifier.Texture, viewportData.csdRenderTextureHandle, 16, "GameWindow:Color");
//                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, wsizei.X, wsizei.Y, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
//                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
//                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
//                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, viewportData.csdRenderTextureHandle, 0);

//                if (viewportData.renderbufferHandle > 0)
//                    GL.DeleteRenderbuffer(viewportData.renderbufferHandle);

//                viewportData.renderbufferHandle = GL.GenRenderbuffer();
//                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, viewportData.renderbufferHandle);
//                GL.ObjectLabel(ObjectLabelIdentifier.Renderbuffer, viewportData.renderbufferHandle, 16, "GameWindow:Depth");
//                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32f, wsizei.X, wsizei.Y);
//                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, viewportData.renderbufferHandle);
//                //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

//                //texDepth = GL.GenTexture();
//                //GL.BindTexture(TextureTarget.Texture2D, texDepth);
//                //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, 800, 600, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
//                //GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, texDepth, 0);

//                // make sure the frame buffer is complete
//                FramebufferErrorCode errorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
//                if (errorCode != FramebufferErrorCode.FramebufferComplete)
//                    throw new Exception();
//            }
//            else
//            {
//                // bind our frame and depth buffer
//                GL.BindFramebuffer(FramebufferTarget.Framebuffer, viewportData.framebufferHandle);
//                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, viewportData.renderbufferHandle);
//            }

//            GL.Viewport(0, 0, wsizei.X, wsizei.Y); // change the viewport to window

//            // actually draw the scene
//            {
//                RenderToViewport(in_DeltaTime);

//            }

//            // unbind our bo so nothing else uses it
//            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

//            GL.Viewport(0, 0, window.ClientSize.X, window.ClientSize.Y); // back to full screen size

//        }
//        private void RenderToViewport(float in_DeltaTime)
//        {
//            GL.ClearColor(Color4.BlueViolet);
//            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

//            renderer.Width = (int)viewportSize.X;
//            renderer.Height = (int)viewportSize.Y;
//            renderer.Start();
//            var uv = new Vector2[]
//        {
//            new Vector2(0.0f, 1.0f), // Bottom-left
//            new Vector2(0.58f, 1.0f), // Bottom-right
//            new Vector2(0.0f, 0.0f), // Top-left
//            new Vector2(0.58f, 0f)  // Top-right
//        };
//            int idx = 0;
//            int idx2 = 0;
//            foreach (var s in SpriteHelper.Sprites)
//            {
//                float width = (float)s.Value.Dimensions.X / (float)renderer.Width;
//                float height = (float)s.Value.Dimensions.Y / (float)renderer.Height;

//                var begin = new Vector2(0, 0);
//                var end = begin + new Vector2(width, height);

//                var pos = new Vector2(0.1f + (0.05f * idx), 0.1f + (0.05f * idx2));
//                var color = new System.Numerics.Vector4(1, 1, 1, 1);
//                renderer.DrawSprite(
//                    new Vector2(begin.X, begin.Y),
//                    new Vector2(begin.X, end.Y),
//                    new Vector2(end.X, begin.Y),
//                    new Vector2(end.X, end.Y), pos, 0, new Vector2(1, 1), 16.0f / 9.0f, s.Value, s.Value, 0, color, color, color, color, color, 1, 0);

//                idx++;
//                if (idx == 12)
//                {
//                    idx = 0;
//                    idx2++;
//                }
//            }
//            renderer.End();
//        }

//        public void RenderNode(SceneNode in_Node, double in_DeltaTime)
//        {
//            SVisibilityData.SNode vis = visibilityData.GetVisibility(in_Node);
//            int idx = 0;
//            foreach (var scene in in_Node.Scenes)
//            {
//                if (!vis.GetVisibility(scene.Value).Active) continue;
//                RenderScenes(scene.Value, vis, ref idx, in_DeltaTime);
//                // = true;
//            }
//        }
//        public void RenderScenes(Scene in_Scene, SVisibilityData.SNode in_Vis, ref int priority, double in_DeltaTime)
//        {
//            int idx = priority;
//            var vis = in_Vis.GetVisibility(in_Scene);
//            foreach (var family in in_Scene.Families)
//            {
//                var transform = new CastTransform();
//                Cast cast = family.Casts[0];

//                UpdateCast(in_Scene, cast, transform, idx, (float)(in_DeltaTime * in_Scene.FrameRate), vis);
//                idx += cast.Children.Count + 1;
//            }
//            priority = idx++;
//        }
//        private void UpdateCast(Scene scene, Cast in_UiElement, CastTransform transform, int priority, float time, SVisibilityData.SScene vis)
//        {
//            bool hideFlag = in_UiElement.Info.HideFlag != 0;
//            var position = new System.Numerics.Vector2(in_UiElement.Info.Translation.X, in_UiElement.Info.Translation.Y);
//            float rotation = in_UiElement.Info.Rotation;
//            var scale = new System.Numerics.Vector2(in_UiElement.Info.Scale.X, in_UiElement.Info.Scale.Y);
//            float sprID = in_UiElement.Info.SpriteIndex;
//            var color = in_UiElement.Info.Color.Invert();
//            var gradientTopLeft = in_UiElement.Info.GradientTopLeft.Invert();
//            var gradientBottomLeft = in_UiElement.Info.GradientBottomLeft.Invert();
//            var gradientTopRight = in_UiElement.Info.GradientTopRight.Invert();
//            var gradientBottomRight = in_UiElement.Info.GradientBottomRight.Invert();

//            foreach (var animation in vis.Animation)
//            {
//                if (!animation.Active)
//                    continue;

//                for (int i = 0; i < 12; i++)
//                {
//                    var type = (AnimationType)(1 << i);
//                    var track = animation.GetTrack(in_UiElement, type);

//                    if (track == null)
//                        continue;

//                    switch (type)
//                    {
//                        case AnimationType.HideFlag:
//                            hideFlag = track.GetSingle(time) != 0;
//                            break;

//                        case AnimationType.XPosition:
//                            position.X = track.GetSingle(time);
//                            break;

//                        case AnimationType.YPosition:
//                            position.Y = track.GetSingle(time);
//                            break;

//                        case AnimationType.Rotation:
//                            rotation = track.GetSingle(time);
//                            break;

//                        case AnimationType.XScale:
//                            scale.X = track.GetSingle(time);
//                            break;

//                        case AnimationType.YScale:
//                            scale.Y = track.GetSingle(time);
//                            break;

//                        case AnimationType.SubImage:
//                            sprID = track.GetSingle(time);
//                            break;

//                        case AnimationType.Color:
//                            color = track.GetColor(time);
//                            break;

//                        case AnimationType.GradientTL:
//                            gradientTopLeft = track.GetColor(time);
//                            break;

//                        case AnimationType.GradientBL:
//                            gradientBottomLeft = track.GetColor(time);
//                            break;

//                        case AnimationType.GradientTR:
//                            gradientTopRight = track.GetColor(time);
//                            break;

//                        case AnimationType.GradientBR:
//                            gradientBottomRight = track.GetColor(time);
//                            break;
//                    }
//                }
//            }

//            if (hideFlag)
//                return;

//            // Inherit position scale
//            // TODO: Is this handled through flags?
//            position.X *= transform.Scale.X;
//            position.Y *= transform.Scale.Y;

//            // Rotate through parent transform
//            float angle = transform.Rotation * MathF.PI / 180.0f; //to radians
//            float rotatedX = position.X * MathF.Cos(angle) * scene.AspectRatio + position.Y * MathF.Sin(angle);
//            float rotatedY = position.Y * MathF.Cos(angle) - position.X * MathF.Sin(angle) * scene.AspectRatio;

//            position.X = rotatedX / scene.AspectRatio;
//            position.Y = rotatedY;

//            position += in_UiElement.Origin;
//            var InheritanceFlags = (ElementInheritanceFlags)in_UiElement.InheritanceFlags.Value;
//            // Inherit position
//            if (InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritXPosition))
//                position.X += transform.Position.X;

//            if (InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritYPosition))
//                position.Y += transform.Position.Y;

//            // Inherit rotation
//            if (InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritRotation))
//                rotation += transform.Rotation;

//            // Inherit scale
//            if (InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleX))
//                scale.X *= transform.Scale.X;

//            if (InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleY))
//                scale.Y *= transform.Scale.Y;

//            // Inherit color
//            if (InheritanceFlags.HasFlag(ElementInheritanceFlags.InheritColor))
//            {
//                var cF = color.ToVec4() * transform.Color.Invert();
//                color = cF.ToSharpNeedleColor();
//            }
//            var Type = (DrawType)in_UiElement.Field04;
//            var Flags = (ElementMaterialFlags)in_UiElement.Field38;

//            if (vis.GetVisibility(in_UiElement).Active && in_UiElement.Enabled)
//            {
//                if (Type == DrawType.Sprite)
//                {
//                    int spriteIdx1 = Math.Min(in_UiElement.SpriteIndices.Length - 1, (int)sprID);
//                    int spriteIdx2 = Math.Min(in_UiElement.SpriteIndices.Length - 1, (int)sprID + 1);
//                    Shuriken.Rendering.Sprite spr = sprID >= 0 ? SpriteHelper.TryGetSprite(in_UiElement.SpriteIndices[spriteIdx1]) : null;
//                    Shuriken.Rendering.Sprite nextSpr = sprID >= 0 ? SpriteHelper.TryGetSprite(in_UiElement.SpriteIndices[spriteIdx2]) : null;
//                    if (config.showQuads)
//                    {
//                        spr = null;
//                        nextSpr = null;
//                    }
//                    spr ??= nextSpr;
//                    nextSpr ??= spr;
//                    renderer.DrawSprite(
//                        in_UiElement.TopLeft, in_UiElement.BottomLeft, in_UiElement.TopRight, in_UiElement.BottomRight,
//                        position, rotation * MathF.PI / 180.0f, scale, scene.AspectRatio, spr, nextSpr, sprID % 1, color.ToVec4(),
//                        gradientTopLeft.ToVec4(), gradientBottomLeft.ToVec4(), gradientTopRight.ToVec4(), gradientBottomRight.ToVec4(),
//                        in_UiElement.Priority, Flags);
//                }
//                else if (Type == DrawType.Font)
//                {
//                    float xOffset = 0.0f;
//                    if (string.IsNullOrEmpty(in_UiElement.Text)) in_UiElement.Text = "";
//                    foreach (char character in in_UiElement.Text)
//                    {

//                        var font = WorkProjectCsd.Project.Fonts[in_UiElement.FontName];
//                        if (font == null)
//                            continue;

//                        Shuriken.Rendering.Sprite spr = null;

//                        foreach (var mapping in font)
//                        {
//                            if (mapping.SourceIndex != character)
//                                continue;

//                            spr = SpriteHelper.TryGetSprite(mapping.DestinationIndex);
//                            break;
//                        }

//                        if (spr == null)
//                            continue;

//                        float width = spr.Dimensions.X / renderer.Width;
//                        float height = spr.Dimensions.Y / renderer.Height;

//                        var begin = (Vector2)in_UiElement.TopLeft;
//                        var end = begin + new Vector2(width, height);

//                        renderer.DrawSprite(
//                            new Vector2(begin.X + xOffset, begin.Y),
//                            new Vector2(begin.X + xOffset, end.Y),
//                            new Vector2(end.X + xOffset, begin.Y),
//                            new Vector2(end.X + xOffset, end.Y),
//                             position, rotation * MathF.PI / 180.0f, scale, scene.AspectRatio, spr, spr, 0, color.ToVec4(),
//                        gradientTopLeft.ToVec4(), gradientBottomLeft.ToVec4(), gradientTopRight.ToVec4(), gradientBottomRight.ToVec4(),
//                        in_UiElement.Priority, Flags
//                        );
//                        //in_UiElement.Field4C = kerning (space between letters)
//                        xOffset += width + BitConverter.ToSingle(BitConverter.GetBytes(in_UiElement.Field4C));
//                    }
//                }

//                var childTransform = new CastTransform(position, rotation, scale, color.Invert());

//                foreach (var child in in_UiElement.Children)
//                    UpdateCast(scene, child, childTransform, priority++, time, vis);
//            }

//        }

//        public int GetViewportImageHandle()
//        {
//            return viewportData.csdRenderTextureHandle;
//        }
//        public void SaveCurrentFile(string in_Path)
//        {
//            WorkProjectCsd.Write(in_Path == null ? config.WorkFilePath : in_Path);
//        }
//    }
//}
