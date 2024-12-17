using PhoenixModel.Helper;

namespace PhoenixWPF.Helper
{
    public interface IPropertyDisplay
    {
        public void Display(List<Eigenschaft> eigenschaften);
    }

    public interface IOptions
    {
        public void ChangeZoomLevel(float zoomLevel);
    }
}
