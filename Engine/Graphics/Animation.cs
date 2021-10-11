using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;

namespace AGame.Engine.Graphics
{
    public class Animation : Sprite
    {
        private RectangleF[] frames;
        private float currentFrameTime;
        private int amountOfFrames;
        private int currentFrame;
        private int framesPerSecond;

        public Animation(Texture2D texture, Vector2 renderScale, Vector2 origin, ColorF colorTint, RectangleF animationFrames, float rotation, int framesPerSecond, int amountOfFrames) : base(texture, renderScale, origin, colorTint, new RectangleF(0, 0, 0, 0), rotation)
        {
            this.framesPerSecond = framesPerSecond;
            this.amountOfFrames = amountOfFrames;
            this.frames = CreateFrames(texture, animationFrames, amountOfFrames);
            this.currentFrame = 0;
            this.SourceRectangle = this.frames[this.currentFrame];
        }

        private float GetFrameTime()
        {
            return 1f / (float)framesPerSecond;
        }

        public void ResetAnimation()
        {
            this.currentFrame = 0;
        }

        private RectangleF[] CreateFrames(Texture2D atlas, RectangleF animationFrames, int amountOfFrames)
        {
            RectangleF[] frames = new RectangleF[amountOfFrames];

            int frameWidth = (int)animationFrames.Width / amountOfFrames;
            int frameHeight = (int)animationFrames.Height;

            for (int i = 0; i < amountOfFrames; i++)
            {
                int x = i * frameWidth;
                RectangleF frame = new RectangleF(animationFrames.X + x, animationFrames.Y, frameWidth, frameHeight);
                frames[i] = frame;
            }

            return frames;
        }

        public override void Update()
        {
            this.currentFrameTime += GameTime.DeltaTime;

            if (currentFrameTime > GetFrameTime())
            {
                currentFrame = (currentFrame + 1) % amountOfFrames;
                currentFrameTime = 0f;
                this.SourceRectangle = this.frames[currentFrame];
            }

            base.Update();
        }

        public override void Render(Vector2 position)
        {
            base.Render(position);
        }
    }
}