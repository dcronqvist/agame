namespace AGame.Engine.Assets
{
    abstract class Asset
    {
        public string Name { get; set; }

        public Asset(string name)
        {
            this.Name = name;
        }
    }
}