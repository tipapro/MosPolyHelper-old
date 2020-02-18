namespace MosPolyHelper.Features.Common.Interfaces
{
    using AndroidX.Fragment.App;

    public interface IFragmentBase
    {
        public Fragments FragmentType { get; }
        public Fragment Fragment { get; }
    }
}