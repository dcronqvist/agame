// using System.Numerics;
// using AGame.Engine.Graphics;

// namespace AGame.Engine.World.Entities
// {
//     public class AnimatorEntity : Entity
//     {
//         protected Animator animator;

//         public AnimatorEntity(Vector2 startPos, Animator animator, bool collidesSolids, float movementTweenFactor) : base(startPos, animator.GetCurrentAnimation(), collidesSolids, movementTweenFactor)
//         {
//             this.animator = animator;
//         }

//         public override void Update(Crater crater)
//         {
//             this.Sprite = this.animator.GetCurrentAnimation();
//             base.Update(crater);
//         }

//         // public override void Render(Crater crater)
//         // {
//         //     base.Render(crater);
//         // }
//     }
// }