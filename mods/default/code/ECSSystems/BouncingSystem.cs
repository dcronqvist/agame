using System;
using System.Collections.Generic;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.World;
using GameUDPProtocol;
using AGame.Engine.Assets.Scripting;

namespace DefaultMod;

[SystemRunsOn(SystemRunner.Server | SystemRunner.Client), ScriptType(Name = "bouncing_system")]
public class BouncingSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<TransformComponent>();
        this.RegisterComponentType<BouncingComponent>();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {
        ECS parent = this.ParentECS;

        foreach (Entity entity in entities)
        {
            var transform = entity.GetComponent<TransformComponent>();
            var bounce = entity.GetComponent<BouncingComponent>();

            bounce.VerticalVelocity += bounce.GravityFactor * deltaTime;
            transform.HeightAboveGround -= bounce.VerticalVelocity * deltaTime;

            transform.Position += bounce.Velocity * deltaTime;
            bounce.Velocity -= bounce.Velocity * bounce.VelocityFriction * deltaTime;

            if (transform.HeightAboveGround < 0)
            {
                transform.HeightAboveGround = 0;

                if (MathF.Abs(bounce.VerticalVelocity) > bounce.VelocityThreshold)
                {
                    bounce.VerticalVelocity = -bounce.FallOffFactor * bounce.VerticalVelocity;
                }
                else
                {
                    bounce.VerticalVelocity = 0;
                    // Finished, remove component
                    parent.RemoveComponentFromEntity(entity, typeof(BouncingComponent));
                    Logging.Log(LogLevel.Debug, $"Removed BouncingComponent from entity {entity.ID}");
                }
            }
        }
    }
}