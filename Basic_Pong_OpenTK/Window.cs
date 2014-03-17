using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Drawing;
using System.Timers;



namespace Pong
{
    public class Window : GameWindow
    {
        private Timer FPSUpdate = new Timer(1000);
        private int FrameCount = 0;

        private PongGame Game;
        private Camera GameCamera;
        
        /// <summary>
        /// Constructor for the Window Class - Initialize the basic window attributes
        /// </summary>
        public Window()
        {
            this.Title = string.Format("Basic Pong Game - FPS: {0} @ {1}x{2}", FrameCount.ToString(), this.Width.ToString(), this.Height.ToString());
            this.Location = new Point(10, 10);
            this.ClientSize = new Size(800, 600);
            this.WindowBorder = OpenTK.WindowBorder.Fixed;

            FPSUpdate.Elapsed += new ElapsedEventHandler(UpdateFPSCount);

            Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);
        }

        /// <summary>
        /// Updates the Title Bar with the Current Frames Per Second, counted in 'OnRenderFrame()' and resets it every second
        /// </summary>
        private void UpdateFPSCount(object source, ElapsedEventArgs e)
        {
            this.Title = string.Format("Basic Pong Game - FPS: {0} @ {1}x{2}", FrameCount.ToString(), this.Width.ToString(), this.Height.ToString());
            FrameCount = 0;
        }

        /// <summary>
        /// The Main Loop
        /// </summary>
        public static void Main()
        {
            using (Window win = new Window())
                win.Run();
        }

        /// <summary>
        /// Initialize anything that needs it
        /// Runs once as the window starts
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            FPSUpdate.Enabled = true;

            //Print OpenGL Information to the Console, for referance
            Console.WriteLine("OpenGL Version: " + GL.GetString(StringName.Version));
            Console.WriteLine("GLSL Version: " + GL.GetString(StringName.ShadingLanguageVersion));
            Console.WriteLine(GL.GetString(StringName.Vendor) + " - " + GL.GetString(StringName.Renderer));

            GL.Enable(EnableCap.CullFace);

            //Initialize the Game
            Game = new PongGame(this.ClientSize);
            GameCamera = new Camera(this.Width, this.Height, 0.1f, 100.0f, new Camera.CameraInfo(new Vector3(0.0f, 0.0f, 19.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f)));
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            //create the Viewport
            GL.Viewport(0, 0, this.Width, this.Height);            
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            FrameCount++;

            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Game.Render();

            this.SwapBuffers();
        }

        private void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape: { this.Exit(); break;}
                case Key.KeypadMinus: { GameCamera.Zoom(); break; }
                case Key.KeypadPlus: { GameCamera.Zoom(-1.0f); break; }
                case Key.Up: { Game.StartLeftPaddleMove(new Vector2(0.0f, 1.0f), PongGame.PaddleName.RIGHT); break; }
                case Key.Down: { Game.StartLeftPaddleMove(new Vector2(0.0f, -1.0f), PongGame.PaddleName.RIGHT); break; }
                case Key.Q: { Game.StartLeftPaddleMove(new Vector2(0.0f, 1.0f), PongGame.PaddleName.LEFT); break; }
                case Key.A: { Game.StartLeftPaddleMove(new Vector2(0.0f, -1.0f), PongGame.PaddleName.LEFT); break; }
            }
        }

        private void Keyboard_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up: { Game.StopLeftPaddleMove(PongGame.PaddleName.RIGHT); break; }
                case Key.Down: { Game.StopLeftPaddleMove(PongGame.PaddleName.RIGHT); break; }
                case Key.Q: { Game.StopLeftPaddleMove(PongGame.PaddleName.LEFT); break; }
                case Key.A: { Game.StopLeftPaddleMove(PongGame.PaddleName.LEFT); break; }
            }
        }
    }
}
