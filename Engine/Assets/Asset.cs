namespace AGame.Engine.Assets
{
    public abstract class Asset
    {
        public string Name { get; set; }
        public bool IsCore { get; set; }

        public abstract bool InitOpenGL();
    }
}