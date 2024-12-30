using PhoenixModel.Helper;

namespace PhoenixWPF.Helper
{
    public interface IPropertyDisplay
    {
        public void Display(IEigenschaftler eigenschaftler);
    }

    public interface IOptions
    {
        public void ChangeZoomLevel(float zoomLevel);
    }
}
