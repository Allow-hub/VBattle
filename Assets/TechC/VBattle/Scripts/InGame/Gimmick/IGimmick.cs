namespace TechC.VBattle.InGame.Gimmick
{
    public interface IGimmick
    {
        void OnEnter();
        void OnUpdate(float deltaTime);
        void OnExit();
    }
}