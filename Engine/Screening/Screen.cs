namespace AGame.Engine.Screening
{
    abstract class Screen
    {
        public string Name { get; set; }

        public Screen(string name)
        {
            this.Name = name;
        }

        public abstract Screen Initialize();
        public abstract void Update();
        public abstract void Render();

        public abstract void OnEnter(string[] args);
        public abstract void OnLeave();
    }
}