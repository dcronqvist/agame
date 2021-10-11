using System.Collections.Generic;

namespace AGame.Engine.Graphics
{
    public class Animator
    {
        private Dictionary<string, Animation> animations;
        private string currentAnimation;

        public Animator(Dictionary<string, Animation> animations, string startAnim)
        {
            this.animations = animations;
            this.currentAnimation = startAnim;
        }

        public Animation GetCurrentAnimation()
        {
            return animations[currentAnimation];
        }

        public void SetAnimation(string animation)
        {
            if (animation != currentAnimation)
            {
                animations[currentAnimation].ResetAnimation();
                this.currentAnimation = animation;
            }
        }
    }
}