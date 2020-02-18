namespace MosPolyHelper.Features.Common
{
    using AndroidX.Fragment.App;
    using AndroidX.Preference;
    using MosPolyHelper.Features.Common.Interfaces;

    public abstract class FragmentBase : Fragment, IFragmentBase
    {
        public Fragments FragmentType { get; }
        public Fragment Fragment => this;

        public FragmentBase(Fragments fragmentType) : base()
        {
            this.FragmentType = fragmentType;
        }
    }

    public abstract class FragmentPreferenceBase : PreferenceFragmentCompat, IFragmentBase
    {
        public Fragments FragmentType { get; }
        public Fragment Fragment => this;

        public FragmentPreferenceBase(Fragments fragmentType) : base()
        {
            this.FragmentType = fragmentType;
        }
    }

    public enum Fragments
    {
        ScheduleMain,
        ScheduleManager,
        Settings,
        Buildings,
        Other,
        ScheduleLessonInfo
    }
}