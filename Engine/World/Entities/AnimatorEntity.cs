using System.Numerics;
using AGame.Engine.Graphics;

namespace AGame.Engine.World.Entities
{
    class AnimatorEntity : Entity
    {
        protected Animator animator;

        public AnimatorEntity(Vector2 startPos, Animator animator) : base(startPos, animator.GetCurrentAnimation())
        {
            this.animator = animator;
        }

        public override void Update()
        {
            this.Sprite = this.animator.GetCurrentAnimation();
            base.Update();
        }

        public override void Render()
        {
            base.Render();
        }
    }
}