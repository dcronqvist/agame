using AGame.Engine.Configuration;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Server)]
public class ColorSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<ColorComponent>();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {
        ECS parent = this.ParentECS;

        foreach (Entity entity in entities)
        {
            ColorComponent cc = entity.GetComponent<ColorComponent>();

            cc.Color = ColorF.Lerp(cc.Color, ColorF.White, deltaTime * 0.1f);
        }
    }
}