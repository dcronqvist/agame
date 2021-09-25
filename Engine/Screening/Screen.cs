namespace AGame.Engine.Screening
{
    abstract class Screen
    {
        public string Name { get; set; }

        public Screen(string name)
        {
            this.Name = name;
        }

        public abstract void Initialize();
        public abstract void Update();
        public abstract void Render();

        public abstract void OnEnter();
        public abstract void OnLeave();
    }
}