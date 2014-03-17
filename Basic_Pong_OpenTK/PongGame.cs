using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Pong
{
    public class PongGame
    {
        public enum PaddleName { LEFT, RIGHT }  
      
        internal struct VBO_Data
        {
            public Vector4[] vertexData;
            public int vertexBufferObject;
            public int shaderProgram;
        }
        
        private Ball PongBall;
        private Paddle LeftPaddle;
        private Paddle RightPaddle;
        private GameBoard Board;

        private int[] Score;
        
        public PongGame(Size ScreenDimensions)
        {
            PongBall = new Ball(new Vector2(0.0f, 0.0f), 1.0f);
            LeftPaddle = new Paddle(new Vector2(-19.0f, 0.0f), 3.0f);
            RightPaddle = new Paddle(new Vector2(19.0f, 0.0f), 3.0f);
            Board = new GameBoard(20.0f, 15.0f);

            Score = new int[2] { 0, 0 };
        }

        public void Render()
        {
            PaddleName? GameStatus = PongBall.UpdateBallLocation(LeftPaddle.YLocation, RightPaddle.YLocation);
            
            if (GameStatus != null)
            {
                //Add 1 to the score of the winning side
                if (GameStatus == PaddleName.LEFT)
                    Score[0]++;
                if (GameStatus == PaddleName.RIGHT)
                    Score[1]++;

                Console.WriteLine("Player1: {0} | Player2: {1}", Score[0].ToString(), Score[1].ToString());

                //reset the ball
                PongBall = new Ball(new Vector2(0.0f, 0.0f), 1.0f);
            }
            
            PongBall.Render();
            LeftPaddle.Render();
            RightPaddle.Render();
            Board.Render();
        }

        public void StartLeftPaddleMove(Vector2 Direction, PaddleName P)
        {
            if (P == PaddleName.LEFT)
                LeftPaddle.StartMove(Direction);
            if (P == PaddleName.RIGHT)
                RightPaddle.StartMove(Direction);
        }

        public void StopLeftPaddleMove(PaddleName P)
        {
            if (P == PaddleName.LEFT)
                LeftPaddle.StopMove();
            if (P == PaddleName.RIGHT)
                RightPaddle.StopMove();
        }

        public class VBO_Object
        {
            private int uniform_Location;
            internal VBO_Data data;

            internal void InitializeVertexBuffer()
            {
                GL.GenBuffers(1, out data.vertexBufferObject);
                GL.BindBuffer(BufferTarget.ArrayBuffer, data.vertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(data.vertexData.Length * sizeof(float) * 4), data.vertexData, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }

            internal void CreateShaderProgram(string Vertex_Shader_Source, string Fragment_Shader_Source)
            {
                int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
                int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

                GL.ShaderSource(vertexShaderHandle, Vertex_Shader_Source);
                GL.ShaderSource(fragmentShaderHandle, Fragment_Shader_Source);

                GL.CompileShader(vertexShaderHandle);
                if (GL.GetProgramInfoLog(vertexShaderHandle).Length > 1) Console.WriteLine(GL.GetProgramInfoLog(vertexShaderHandle));

                GL.CompileShader(fragmentShaderHandle);
                if (GL.GetProgramInfoLog(fragmentShaderHandle).Length > 1) Console.WriteLine(GL.GetProgramInfoLog(fragmentShaderHandle));

                data.shaderProgram = GL.CreateProgram();
                GL.AttachShader(data.shaderProgram, vertexShaderHandle);
                GL.AttachShader(data.shaderProgram, fragmentShaderHandle);
                GL.LinkProgram(data.shaderProgram);
            }

            internal void Render()
            {
                GL.UseProgram(data.shaderProgram);

                GL.BindBuffer(BufferTarget.ArrayBuffer, data.vertexBufferObject);
                GL.EnableVertexAttribArray(0);

                GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0);

                GL.DrawArrays(PrimitiveType.Triangles, 0, data.vertexData.Length);

                GL.DisableVertexAttribArray(0);
                GL.UseProgram(0);
            }
        }

        public class Ball : VBO_Object
        {
            private Vector2 location;
            private Vector2 ang_vel = new Vector2(0.1f, -0.1f);
            private int U_Location;
            private float speed = 0.1f;
            
            public Ball(Vector2 Location, float Radius)
            {
                location = Location;

                int sections = 30;
                data.vertexData = new Vector4[3 * sections];
                for (int i = 0; i < 360; i += 360 / sections)
                {
                    int loc = i / (360 / sections);
                    data.vertexData[loc * 3 + 1] = new Vector4((float)(Math.Sin(MathHelper.DegreesToRadians(i)) * Radius), (float)(Math.Cos(MathHelper.DegreesToRadians(i)) * Radius), 0.0f, 1.0f);
                    data.vertexData[loc * 3] = new Vector4((float)(Math.Sin(MathHelper.DegreesToRadians(i + (360 / sections))) * Radius), (float)(Math.Cos(MathHelper.DegreesToRadians(i + (360 / sections))) * Radius), 0.0f, 1.0f);
                    data.vertexData[loc * 3 + 2] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                }

                

                base.InitializeVertexBuffer();
                base.CreateShaderProgram(Pong.Properties.Resources.VertexShader, Pong.Properties.Resources.FragmentShader);

                U_Location = GL.GetUniformLocation(data.shaderProgram, "location");
                if (U_Location == -1) Console.WriteLine("Unable to Locate 'location' in the vertexShader");
            }

            public PaddleName? UpdateBallLocation(float LeftPaddleY, float RightPaddleY)
            {
                Console.WriteLine(speed);
                location = new Vector2(location.X + ang_vel.X, location.Y + ang_vel.Y);

                //Bounce off the top or bottom of the board
                if (location.Y >= 14 || location.Y <= -14)
                    ang_vel.Y = -ang_vel.Y;

                //Check if we're at the Left paddle
                if (location.X <= -17.5)
                {
                    //if so, bounce and increase speed
                    if (location.Y < LeftPaddleY + 3.0f && location.Y > LeftPaddleY - 3.0f)
                    {
                        speed = speed * 1.025f;
                        ang_vel.X = -(ang_vel.X - speed);
                        return null;
                    }
                    else
                        return PaddleName.LEFT; //return non-null so right gets a point
                }

                //Check if we're at the Right paddle
                if (location.X >= 17.5)
                {
                    //if so, bounce and increase speed
                    if (location.Y < RightPaddleY + 3.0f && location.Y > RightPaddleY - 3.0f)
                    {
                        speed = speed * 1.025f;
                        ang_vel.X = -(ang_vel.X + speed);
                        return null;
                    }
                    else
                        return PaddleName.RIGHT; //return non-null so left gets a point
                }
                return null;
            }

            new public void Render()
            {
                
                GL.UseProgram(data.shaderProgram);
                GL.Uniform2(U_Location, location);
                GL.UseProgram(0);

                base.Render();
            }
        }

        public class Paddle : VBO_Object
        {
            private Vector2 location;
            private int U_Location;
            private float velocity = 0;

            public float YLocation { get { return location.Y; } }

            public Paddle(Vector2 Location, float Length)
            {
                location = Location;
                
                data.vertexData = new Vector4[] {new Vector4(0.5f, -(Length), 0.0f, 1.0f),
                                                 new Vector4(0.5f, (Length), 0.0f, 1.0f),
                                                 new Vector4(-0.5f, -(Length), 0.0f, 1.0f),
                
                                                 new Vector4(-0.5f, -(Length), 0.0f, 1.0f),
                                                 new Vector4(0.5f, (Length), 0.0f, 1.0f),
                                                 new Vector4(-0.5f, (Length), 0.0f, 1.0f)};

                base.InitializeVertexBuffer();
                base.CreateShaderProgram(Pong.Properties.Resources.VertexShader, Pong.Properties.Resources.FragmentShader);

                U_Location = GL.GetUniformLocation(data.shaderProgram, "location");
                if (U_Location == -1) Console.WriteLine("Unable to Locate 'location' in the vertexShader");
            }

            new public void Render()
            {
                location = new Vector2(location.X, location.Y + velocity);

                if (location.Y > 11)
                    location.Y = 11;
                if (location.Y < -11)
                    location.Y = -11;
                
                GL.UseProgram(data.shaderProgram);
                GL.Uniform2(U_Location, location);
                GL.UseProgram(0);

                base.Render();
            }

            public void StartMove(Vector2 Direction)
            {
                if (Direction.Y > 0)
                    velocity = 0.4f;
                else
                    velocity = -0.4f;
            }

            public void StopMove()
            {
                velocity = 0;
            }
        }

        public class GameBoard : VBO_Object
        {
            public GameBoard(float width, float height)
            {
                data.vertexData = new Vector4[] {new Vector4(width, -height, 0.0f, 1.0f),
                                                 new Vector4(width, height, 0.0f, 1.0f),
                                                 new Vector4(-width, height, 0.0f, 1.0f),
                                                 new Vector4(-width, -height, 0.0f, 1.0f)};

                base.InitializeVertexBuffer();
                base.CreateShaderProgram(Pong.Properties.Resources.VertexShader, Pong.Properties.Resources.FragmentShader);
            }

            new public void Render()
            {
                GL.UseProgram(data.shaderProgram);

                GL.BindBuffer(BufferTarget.ArrayBuffer, data.vertexBufferObject);
                GL.EnableVertexAttribArray(0);

                GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0);

                GL.DrawArrays(PrimitiveType.LineLoop, 0, data.vertexData.Length);

                GL.DisableVertexAttribArray(0);
                GL.UseProgram(0);
            }
        }

        //public class Ball
        //{
        //    private float x, y;
        //    private float radius;

        //    private int uniform_Location;
        //    private VBO_Data Data;
            
        //    public Ball(float X, float Y, float Radius)
        //    {
        //        x = X; y = Y; radius = Radius;
        //        Console.WriteLine(string.Format("New Ball @ {0},{1} - Radius of {2}", x.ToString(), y.ToString(), radius.ToString()));

        //        //Calculate the edges of the ball
        //        int sections = 90;
        //        Data.vertexData = new Vector4[3 * sections];
        //        for (int i = 0; i < 360; i += 360 / sections)
        //        {
        //            int loc = i / (360 / sections);
        //            Data.vertexData[loc * 3 + 1] = new Vector4((float)(Math.Sin(MathHelper.DegreesToRadians(i)) * radius), (float)(Math.Cos(MathHelper.DegreesToRadians(i)) * radius), 0.0f, 1.0f);
        //            Data.vertexData[loc * 3] = new Vector4((float)(Math.Sin(MathHelper.DegreesToRadians(i + (360 / sections))) * radius), (float)(Math.Cos(MathHelper.DegreesToRadians(i + (360 / sections))) * radius), 0.0f, 1.0f);
        //            Data.vertexData[loc*3 + 2] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        //        }

        //        InitializeVertexBuffer();
        //        CreateShaderProgram();

        //        uniform_Location = GL.GetUniformLocation(Data.shaderProgram, "location");
        //        if (uniform_Location == -1) Console.WriteLine("Unable to Locate 'location' in the vertexShader");
        //    }

        //    private void InitializeVertexBuffer()
        //    {
        //        GL.GenBuffers(1, out Data.vertexBufferObject);
        //        GL.BindBuffer(BufferTarget.ArrayBuffer, Data.vertexBufferObject);
        //        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Data.vertexData.Length * sizeof(float) * 4), Data.vertexData, BufferUsageHint.StaticDraw);
        //        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        //    }

        //    private void CreateShaderProgram()
        //    {
        //        int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
        //        int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

        //        GL.ShaderSource(vertexShaderHandle, Pong.Properties.Resources.VertexShader);
        //        GL.ShaderSource(fragmentShaderHandle, Pong.Properties.Resources.FragmentShader);

        //        GL.CompileShader(vertexShaderHandle);
        //        if (GL.GetProgramInfoLog(vertexShaderHandle).Length > 1) Console.WriteLine(GL.GetProgramInfoLog(vertexShaderHandle));

        //        GL.CompileShader(fragmentShaderHandle);
        //        if (GL.GetProgramInfoLog(fragmentShaderHandle).Length > 1) Console.WriteLine(GL.GetProgramInfoLog(fragmentShaderHandle));

        //        Data.shaderProgram = GL.CreateProgram();
        //        GL.AttachShader(Data.shaderProgram, vertexShaderHandle);
        //        GL.AttachShader(Data.shaderProgram, fragmentShaderHandle);
        //        GL.LinkProgram(Data.shaderProgram);
        //    }

        //    public void Render()
        //    {
        //        GL.UseProgram(Data.shaderProgram);

        //        GL.Uniform3(uniform_Location, x, y, -10.0f);

        //        GL.BindBuffer(BufferTarget.ArrayBuffer, Data.vertexBufferObject);
        //        GL.EnableVertexAttribArray(0);

        //        GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0);

        //        GL.DrawArrays(PrimitiveType.Triangles, 0, Data.vertexData.Length);

        //        GL.DisableVertexAttribArray(0);
        //        GL.UseProgram(0);
        //    }
        //}
    }
}
