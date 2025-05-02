namespace ConverseEditor
{
    public interface ISubWindow
    {
        public void Render(ConverseProject in_Renderer);
        public void Reset(ConverseProject in_Renderer);
    }
}