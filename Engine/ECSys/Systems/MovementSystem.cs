using AGame.Engine.Configuration;
using AGame.Engine.ECSys.Components;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

// [SystemRunsOn(SystemRunner.Server | SystemRunner.Client)]
// public class MovementSystem : BaseSystem
// {
//     public override void Initialize()
//     {
//         this.RegisterComponentType<PlayerPositionComponent>();
//     }

//     public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
//     {
//         ECS parent = this.ParentECS;

//         foreach (Entity entity in entities)
//         {
//             PlayerPositionComponent ppc = entity.GetComponent<PlayerPositionComponent>();

//             ppc.Velocity += (ppc.TargetVelocity - ppc.Velocity) * deltaTime * 4f;
//             ppc.Position += ppc.Velocity * deltaTime;
//         }
//     }
// }