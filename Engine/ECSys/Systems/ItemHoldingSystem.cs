// using System.Collections.Generic;
// using AGame.Engine.ECSys.Components;
// using AGame.Engine.Items;
// using AGame.Engine.World;

// namespace AGame.Engine.ECSys.Systems;

// [SystemRunsOn(SystemRunner.Server)]
// public class ItemHoldingSystem : BaseSystem
// {
//     public override void Initialize()
//     {
//         this.RegisterComponentType<ContainerComponent>();
//         this.RegisterComponentType<HotbarComponent>();
//         this.RegisterComponentType<PlayerStateComponent>();
//     }

//     public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
//     {
//         foreach (var entity in entities)
//         {
//             var container = entity.GetComponent<ContainerComponent>();
//             var hotbar = entity.GetComponent<HotbarComponent>();
//             var playerState = entity.GetComponent<PlayerStateComponent>();

//             var slot = container.GetContainer().GetSlot(hotbar.SelectedSlot);

//             // if (slot.Item != null && slot.Item != "")
//             // {
//             //     playerState.HoldingItem = slot.Item;
//             // }
//             // else
//             // {
//             //     playerState.HoldingItem = "";
//             // }
//         }
//     }
// }